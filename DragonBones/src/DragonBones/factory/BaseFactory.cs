using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public class BuildArmaturePackage
    {
        public string dataName = null;
        public string textureAtlasName = null;
        public DragonBonesData data = null;
        public ArmatureData armature = null;
        public SkinData skin = null;
    }
    /**
     * @language zh_CN
     * 创建骨架的基础工厂。 (通常只需要一个全局工厂实例)
     * @see DragonBones.DragonBonesData
     * @see DragonBones.TextureAtlasData
     * @see DragonBones.ArmatureData
     * @see DragonBones.Armature
     * @version DragonBones 3.0
     */
    public abstract class BaseFactory
    {
        protected static readonly ObjectDataParser _defaultDataParser = new ObjectDataParser();
        /**
         * @language zh_CN
         * 是否开启共享搜索。
         * 如果开启，创建一个骨架时，可以从多个龙骨数据中寻找骨架数据，或贴图集数据中寻找贴图数据。 (通常在有共享导出的数据时开启)
         * @see DragonBones.DragonBonesData#autoSearch
         * @see DragonBones.TextureAtlasData#autoSearch
         * @version DragonBones 4.5
         */
        public bool autoSearch = false;
        /**
         * @private
         */
        protected DataParser _dataParser = null;
        /**
         * @private
         */
        protected readonly Dictionary<string, DragonBonesData> _dragonBonesDataMap = new Dictionary<string, DragonBonesData>();
        /**
         * @private
         */
        protected readonly Dictionary<string, List<TextureAtlasData>> _textureAtlasDataMap = new Dictionary<string, List<TextureAtlasData>>();
        /** 
         * @private 
         */
        public BaseFactory(DataParser dataParser = null)
        {
            _dataParser = dataParser;

            if (_dataParser == null)
            {
                _dataParser = _defaultDataParser;
            }
        }
        /** 
         * @private 
         */
        protected TextureData _getTextureData(string textureAtlasName, string textureName)
        {
            if (_textureAtlasDataMap.ContainsKey(textureAtlasName))
            {
                var textureAtlasDataList = _textureAtlasDataMap[textureAtlasName];
                for (int i = 0, l = textureAtlasDataList.Count; i < l; ++i)
                {
                    var textureAtlasData = textureAtlasDataList[i];
                    var textureData = textureAtlasData.GetTexture(textureName);
                    if (textureData != null)
                    {
                        return textureData;
                    }
                }
            }

            if (autoSearch) // Will be search all data, if the autoSearch is true.
            {
                foreach (var textureAtlasDataList in _textureAtlasDataMap.Values)
                {
                    for (int i = 0, l = textureAtlasDataList.Count; i < l; ++i)
                    {
                        var textureAtlasData = textureAtlasDataList[i];
                        if (textureAtlasData.autoSearch)
                        {
                            var textureData = textureAtlasData.GetTexture(textureName);
                            if (textureData != null)
                            {
                                return textureData;
                            }
                        }
                    }
                }
            }

            return null;
        }
        /**
         * @private
         */
        protected bool _fillBuildArmaturePackage(BuildArmaturePackage dataPackage, string dragonBonesName, string armatureName, string skinName, string textureAtlasName)
        {
            DragonBonesData dragonBonesData = null;
            ArmatureData armatureData = null;

            var isAvailableName = !string.IsNullOrEmpty(dragonBonesName);
            if (isAvailableName)
            {
                if (_dragonBonesDataMap.ContainsKey(dragonBonesName))
                {
                    dragonBonesData = _dragonBonesDataMap[dragonBonesName];
                    armatureData = dragonBonesData.GetArmature(armatureName);
                }
            }

            if (armatureData == null && (!isAvailableName || autoSearch)) // Will be search all data, if do not give a data name or the autoSearch is true.
            {
                foreach (var pair in _dragonBonesDataMap)
                {
                    dragonBonesData = pair.Value;
                    if (!isAvailableName || dragonBonesData.autoSearch)
                    {
                        armatureData = dragonBonesData.GetArmature(armatureName);
                        if (armatureData != null)
                        {
                            dragonBonesName = dragonBonesData.name;
                            break;
                        }
                    }
                }
            }

            if (armatureData != null)
            {
                dataPackage.dataName = dragonBonesName;
                dataPackage.textureAtlasName = textureAtlasName;
                dataPackage.data = dragonBonesData;
                dataPackage.armature = armatureData;
                dataPackage.skin = armatureData.GetSkin(skinName);
                if (dataPackage.skin == null)
                {
                    dataPackage.skin = armatureData.defaultSkin;
                }

                return true;
            }

            return false;
        }
        /**
         * @private
         */
        protected void _buildBones(BuildArmaturePackage dataPackage, Armature armature)
        {
            var bones = dataPackage.armature.sortedBones;
            for (int i = 0, l = bones.Count; i < l; ++i)
            {
                var boneData = bones[i];
                var bone = BaseObject.BorrowObject<Bone>();
                bone._init(boneData);

                if (boneData.parent != null)
                {
                    armature.AddBone(bone, boneData.parent.name);
                }
                else
                {
                    armature.AddBone(bone);
                }

                if (boneData.ik != null)
                {
                    bone.ikBendPositive = boneData.bendPositive;
                    bone.ikWeight = boneData.weight;
                    bone._setIK(armature.GetBone(boneData.ik.name), boneData.chain, boneData.chainIndex);
                }
            }
        }
        /**
         * @private
         */
        protected void _buildSlots(BuildArmaturePackage dataPackage, Armature armature)
        {
            var currentSkin = dataPackage.skin;
            var defaultSkin = dataPackage.armature.defaultSkin;
            var skinSlotDatas = new Dictionary<string, SkinSlotData>();

            foreach (var skinSlotData in defaultSkin.slots.Values)
            {
                skinSlotDatas[skinSlotData.slot.name] = skinSlotData;
            }

            if (currentSkin != defaultSkin)
            {
                foreach (var skinSlotData in currentSkin.slots.Values)
                {
                    skinSlotDatas[skinSlotData.slot.name] = skinSlotData;
                }
            }

            var slots = dataPackage.armature.sortedSlots;
            for (int i = 0, l = slots.Count; i < l; ++i)
            {
                var slotData = slots[i];
                if (!skinSlotDatas.ContainsKey(slotData.name))
                {
                    continue;
                }

                var skinSlotData = skinSlotDatas[slotData.name];
                var slot = _generateSlot(dataPackage, skinSlotData, armature);
                if (slot != null)
                {
                    armature.AddSlot(slot, slotData.parent.name);
                    slot._setDisplayIndex(slotData.displayIndex);
                }
            }
        }
        /**
         * @private
         */
        protected void _replaceSlotDisplay(BuildArmaturePackage dataPackage, DisplayData displayData, Slot slot, int displayIndex)
        {
            if (displayIndex < 0)
            {
                displayIndex = slot.displayIndex;
            }

            if (displayIndex >= 0)
            {
                var displayList = slot.displayList; // Copy.
                if (displayList.Count <= displayIndex)
                {
                    DragonBones.ResizeList(displayList, displayIndex + 1, null);
                }

                if (slot._replacedDisplayDatas.Count <= displayIndex)
                {
                    DragonBones.ResizeList(slot._replacedDisplayDatas, displayIndex + 1, null);
                }

                slot._replacedDisplayDatas[displayIndex] = displayData;

                if (displayData.type == DisplayType.Armature)
                {
                    var childArmature = BuildArmature(displayData.path, dataPackage.dataName);
                    displayList[displayIndex] = childArmature;
                }
                else
                {
                    if (displayData.texture == null)
                    {
                        displayData.texture = _getTextureData(dataPackage.dataName, displayData.path);
                    }

                    var displayDatas = slot.skinSlotData.displays;
                    if (
                        displayData.mesh != null ||
                        (displayIndex < displayDatas.Count && displayDatas[displayIndex].mesh != null)
                    )
                    {
                        displayList[displayIndex] = slot.meshDisplay;
                    }
                    else
                    {
                        displayList[displayIndex] = slot.rawDisplay;
                    }
                }

                slot.displayList = displayList;
            }
        }
        /** 
         * @private 
         */
        protected abstract TextureAtlasData _generateTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas);
        /** 
         * @private 
         */
        protected abstract Armature _generateArmature(BuildArmaturePackage dataPackage);
        /** 
         * @private 
         */
        protected abstract Slot _generateSlot(BuildArmaturePackage dataPackage, SkinSlotData skinSlotData, Armature armature);
        /**
         * @language zh_CN
         * 解析并添加龙骨数据。
         * @param rawData 需要解析的原始数据。 (JSON)
         * @param name 为数据提供一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @param scale 为所有骨架设置一个缩放值。
         * @returns DragonBonesData
         * @see #GetDragonBonesData()
         * @see #AddDragonBonesData()
         * @see #RemoveDragonBonesData()
         * @see DragonBones.DragonBonesData
         * @version DragonBones 4.5
         */
        public DragonBonesData ParseDragonBonesData(Dictionary<string, object> rawData, string name = null, float scale = 1.0f)
        {
            var dragonBonesData = _dataParser.ParseDragonBonesData(rawData, scale);
            AddDragonBonesData(dragonBonesData, name);

            return dragonBonesData;
        }
        /**
         * @language zh_CN
         * 解析并添加贴图集数据。
         * @param rawData 需要解析的原始数据。 (JSON)
         * @param textureAtlas 贴图。 (根据渲染引擎的不同而不同，通常是贴图或材质)
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @param scale 为贴图集设置一个缩放值。
         * @returns 贴图集数据
         * @see #GetTextureAtlasData()
         * @see #AddTextureAtlasData()
         * @see #RemoveTextureAtlasData()
         * @see DragonBones.TextureAtlasData
         * @version DragonBones 4.5
         */
        public TextureAtlasData ParseTextureAtlasData(Dictionary<string, object> rawData, object textureAtlas, string name = null, float scale = 0.0f)
        {
            var textureAtlasData = _generateTextureAtlasData(null, null);
            _dataParser.ParseTextureAtlasData(rawData, textureAtlasData, scale);

            _generateTextureAtlasData(textureAtlasData, textureAtlas);
            AddTextureAtlasData(textureAtlasData, name);

            return textureAtlasData;
        }
        /**
         * @language zh_CN
         * 获取指定名称的龙骨数据。
         * @param name 数据名称。
         * @returns DragonBonesData
         * @see #ParseDragonBonesData()
         * @see #AddDragonBonesData()
         * @see #RemoveDragonBonesData()
         * @see DragonBones.DragonBonesData
         * @version DragonBones 3.0
         */
        public DragonBonesData GetDragonBonesData(string name)
        {
            return _dragonBonesDataMap.ContainsKey(name) ? _dragonBonesDataMap[name] : null;
        }
        /**
         * @language zh_CN
         * 添加龙骨数据。
         * @param data 龙骨数据。
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @see #ParseDragonBonesData()
         * @see #GetDragonBonesData()
         * @see #RemoveDragonBonesData()
         * @see DragonBones.DragonBonesData
         * @version DragonBones 3.0
         */
        public void AddDragonBonesData(DragonBonesData data, string name = null)
        {
            if (data != null)
            {
                name = !string.IsNullOrEmpty(name) ? name : data.name;
                if (!string.IsNullOrEmpty(name))
                {
                    if (!_dragonBonesDataMap.ContainsKey(name))
                    {
                        _dragonBonesDataMap[name] = data;
                    }
                    else
                    {
                        DragonBones.Assert(false, "Same name data. " + name);
                    }
                }
                else
                {
                    DragonBones.Assert(false, "Unnamed data.");
                }
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @language zh_CN
         * 移除龙骨数据。
         * @param name 数据名称。
         * @param disposeData 是否释放数据。
         * @see #ParseDragonBonesData()
         * @see #GetDragonBonesData()
         * @see #AddDragonBonesData()
         * @see DragonBones.DragonBonesData
         * @version DragonBones 3.0
         */
        virtual public void RemoveDragonBonesData(string name, bool disposeData = true)
        {
            if (_dragonBonesDataMap.ContainsKey(name))
            {
                if (disposeData)
                {
                    _dragonBonesDataMap[name].ReturnToPool();
                }

                _dragonBonesDataMap.Remove(name);
            }
        }
        /**
         * @language zh_CN
         * 获取指定名称的贴图集数据列表。
         * @param textureAtlasName 数据名称。
         * @returns 贴图集数据列表。
         * @see #ParseTextureAtlasData()
         * @see #AddTextureAtlasData()
         * @see #RemoveTextureAtlasData()
         * @see DragonBones.textures.TextureAtlasData
         * @version DragonBones 3.0
         */
        public List<TextureAtlasData> GetTextureAtlasData(string textureAtlasName)
        {
            return _textureAtlasDataMap.ContainsKey(textureAtlasName) ? _textureAtlasDataMap[textureAtlasName] : null;
        }
        /**
         * @language zh_CN
         * 添加贴图集数据。
         * @param data 贴图集数据。
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @see #ParseTextureAtlasData()
         * @see #GetTextureAtlasData()
         * @see #RemoveTextureAtlasData()
         * @see DragonBones.textures.TextureAtlasData
         * @version DragonBones 3.0
         */
        public void AddTextureAtlasData(TextureAtlasData data, string name = null)
        {
            if (data != null)
            {
                name = !string.IsNullOrEmpty(name) ? name : data.name;
                if (!string.IsNullOrEmpty(name))
                {
                    var textureAtlasList = _textureAtlasDataMap.ContainsKey(name) ? _textureAtlasDataMap[name] : (_textureAtlasDataMap[name] = new List<TextureAtlasData>());
                    if (!textureAtlasList.Contains(data))
                    {
                        textureAtlasList.Add(data);
                    }
                }
                else
                {
                    DragonBones.Assert(false, "Unnamed data.");
                }
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @language zh_CN
         * 移除贴图集数据。
         * @param name 数据名称。
         * @param disposeData 是否释放数据。
         * @see #ParseTextureAtlasData()
         * @see #GetTextureAtlasData()
         * @see #AddTextureAtlasData()
         * @see DragonBones.textures.TextureAtlasData
         * @version DragonBones 3.0
         */
        virtual public void RemoveTextureAtlasData(string name, bool disposeData = true)
        {
            if (_textureAtlasDataMap.ContainsKey(name))
            {
                if (disposeData)
                {
                    foreach (var textureAtlasData in _textureAtlasDataMap[name])
                    {
                        textureAtlasData.ReturnToPool();
                    }
                }

                _textureAtlasDataMap.Remove(name);
            }
        }
        /**
         * @language zh_CN
         * 清除所有的数据。
         * @param disposeData 是否释放数据。
         * @version DragonBones 4.5
         */
        virtual public void Clear(bool disposeData = true)
        {
            if (disposeData)
            {
                foreach (var dragonBonesData in _dragonBonesDataMap.Values)
                {
                    dragonBonesData.ReturnToPool();
                }

                foreach (var textureAtlasDataList in _textureAtlasDataMap.Values)
                {
                    foreach (var textureAtlasData in textureAtlasDataList)
                    {
                        textureAtlasData.ReturnToPool();
                    }
                }
            }

            _dragonBonesDataMap.Clear();
            _textureAtlasDataMap.Clear();
        }
        /**
         * @language zh_CN
         * 创建一个指定名称的骨架。
         * @param armatureName 骨架数据名称。
         * @param dragonBonesName 龙骨数据名称，如果未设置，将检索所有的龙骨数据，当多个龙骨数据中包含同名的骨架数据时，可能无法创建出准确的骨架。
         * @param skinName 皮肤名称，如果未设置，则使用默认皮肤。
         * @returns 骨架
         * @see DragonBones.Armature
         * @version DragonBones 3.0
         */
        public Armature BuildArmature(string armatureName, string dragonBonesName = null, string skinName = null, string textureAtlasName = null)
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, skinName, textureAtlasName))
            {
                var armature = _generateArmature(dataPackage);
                _buildBones(dataPackage, armature);
                _buildSlots(dataPackage, armature);

                armature.InvalidUpdate(null, true);
                armature.AdvanceTime(0.0f); // Update armature pose.

                return armature;
            }

            DragonBones.Assert(false, "No armature data. " + armatureName + " " + dragonBonesName != null ? dragonBonesName : "");

            return null;
        }
        /**
         * @language zh_CN
         * 将指定骨架的动画替换成其他骨架的动画。 (通常这些骨架应该具有相同的骨架结构)
         * @param toArmature 指定的骨架。
         * @param fromArmatreName 其他骨架的名称。
         * @param fromSkinName 其他骨架的皮肤名称，如果未设置，则使用默认皮肤。
         * @param fromDragonBonesDataName 其他骨架属于的龙骨数据名称，如果未设置，则检索所有的龙骨数据。
         * @param ifRemoveOriginalAnimationList 是否移除原有的动画。 [true: 移除, false: 不移除]
         * @returns 是否替换成功。 [true: 成功, false: 不成功]
         * @see DragonBones.Armature
         * @version DragonBones 4.5
         */
        public bool CopyAnimationsToArmature(
            Armature toArmature, string fromArmatreName, string fromSkinName = null,
            string fromDragonBonesDataName = null, bool ifRemoveOriginalAnimationList = true
        )
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dataPackage, fromDragonBonesDataName, fromArmatreName, fromSkinName, null))
            {
                var fromArmatureData = dataPackage.armature;
                if (ifRemoveOriginalAnimationList)
                {
                    toArmature.animation.animations = fromArmatureData.animations;
                }
                else
                {
                    var animations = new Dictionary<string, AnimationData>();
                    foreach (var pair in toArmature.animation.animations)
                    {
                        animations[pair.Key] = toArmature.animation.animations[pair.Key];
                    }

                    foreach (var pair in fromArmatureData.animations)
                    {
                        animations[pair.Key] = fromArmatureData.animations[pair.Key];
                    }

                    toArmature.animation.animations = animations;
                }

                if (dataPackage.skin != null)
                {
                    var slots = toArmature.GetSlots();
                    for (int i = 0, l = slots.Count; i < l; ++i)
                    {
                        var toSlot = slots[i];
                        var toSlotDisplayList = toSlot.displayList;
                        for (int iA = 0, lA = toSlotDisplayList.Count; iA < lA; ++iA)
                        {
                            var toDisplayObject = toSlotDisplayList[iA];
                            if (toDisplayObject is Armature)
                            {
                                var displays = dataPackage.skin.GetSlot(toSlot.name).displays;
                                if (iA < displays.Count)
                                {
                                    var fromDisplayData = displays[iA];
                                    if (fromDisplayData.type == DisplayType.Armature)
                                    {
                                        CopyAnimationsToArmature((Armature)toDisplayObject, fromDisplayData.path, fromSkinName, fromDragonBonesDataName, ifRemoveOriginalAnimationList);
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }
        /**
         * @language zh_CN
         * 用指定资源替换插槽的显示对象。
         * @param dragonBonesName 指定的龙骨数据名称。
         * @param armatureName 指定的骨架名称。
         * @param slotName 指定的插槽名称。
         * @param displayName 指定的显示对象名称。
         * @param slot 指定的插槽实例。
         * @param displayIndex 要替换的显示对象的索引，如果未设置，则替换当前正在显示的显示对象。
         * @version DragonBones 4.5
         */
        public void ReplaceSlotDisplay(string dragonBonesName, string armatureName, string slotName, string displayName, Slot slot, int displayIndex = -1)
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, null, null))
            {
                var slotDisplayDataSet = dataPackage.skin.GetSlot(slotName);
                if (slotDisplayDataSet != null)
                {
                    foreach (var displayData in slotDisplayDataSet.displays)
                    {
                        if (displayData.name == displayName)
                        {
                            _replaceSlotDisplay(dataPackage, displayData, slot, displayIndex);
                            break;
                        }
                    }
                }
            }
        }
        /**
         * @language zh_CN
         * 用指定资源列表替换插槽的显示对象列表。
         * @param dragonBonesName 指定的 DragonBonesData 名称。
         * @param armatureName 指定的骨架名称。
         * @param slotName 指定的插槽名称。
         * @param slot 指定的插槽实例。
         * @version DragonBones 4.5
         */
        public void ReplaceSlotDisplayList(string dragonBonesName, string armatureName, string slotName, Slot slot)
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, null, null))
            {
                var skinSlotData = dataPackage.skin.GetSlot(slotName);
                if (skinSlotData != null)
                {
                    for (int i = 0, l = skinSlotData.displays.Count; i < l; ++i)
                    {
                        var displayData = skinSlotData.displays[i];
                        _replaceSlotDisplay(dataPackage, displayData, slot, i);
                    }
                }
            }
        }
        /**
         * @private
         */
        public Dictionary<string, DragonBonesData> GetAllDragonBonesData()
        {
            return _dragonBonesDataMap;
        }
        /**
         * @private
         */
        public Dictionary<string, List<TextureAtlasData>> GetAllTextureAtlasData()
        {
            return _textureAtlasDataMap;
        }
    }
}