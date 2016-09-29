using System.Collections.Generic;

namespace dragonBones
{
    /**
     * @private
     */
    public class BuildArmaturePackage
    {
        public string dataName = null;
        public DragonBonesData data = null;
        public ArmatureData armature = null;
        public SkinData skin = null;
    }

    /**
     * @language zh_CN
     * 创建骨架的基础工厂。 (通常只需要一个全局工厂实例)
     * @see dragonBones.DragonBonesData
     * @see dragonBones.TextureAtlasData
     * @see dragonBones.ArmatureData
     * @see dragonBones.Armature
     * @version DragonBones 3.0
     */
    public abstract class BaseFactory
    {
        protected static ObjectDataParser _defaultDataParser = new ObjectDataParser();

        /**
         * @language zh_CN
         * 是否开启共享搜索。 [true: 开启, false: 不开启]
         * 如果开启，创建一个骨架时，可以从多个龙骨数据中寻找骨架数据，或贴图集数据中寻找贴图数据。 (通常在有共享导出的数据时开启)
         * @see dragonBones.DragonBonesData#autoSearch
         * @see dragonBones.TextureAtlasData#autoSearch
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
        protected Dictionary<string, DragonBonesData> _dragonBonesDataMap = new Dictionary<string, DragonBonesData>();

        /**
         * @private
         */
        protected Dictionary<string, List<TextureAtlasData>> _textureAtlasDataMap = new Dictionary<string, List<TextureAtlasData>>();

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
        protected TextureData _getTextureData(string dragonBonesName, string textureName)
        {
            if (_textureAtlasDataMap.ContainsKey(dragonBonesName))
            {
                foreach (var textureAtlasData in _textureAtlasDataMap[dragonBonesName])
                {
                    var textureData = textureAtlasData.getTexture(textureName);
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
                    foreach (var textureAtlasData in textureAtlasDataList)
                    {
                        if (textureAtlasData.autoSearch)
                        {
                            var textureData = textureAtlasData.getTexture(textureName);
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
        protected bool _fillBuildArmaturePackage(string dragonBonesName, string armatureName, string skinName, BuildArmaturePackage dataPackage)
        {
            var isAvailableName = DragonBones.isAvailableString(dragonBonesName);
            if (isAvailableName)
            {
                if (_dragonBonesDataMap.ContainsKey(dragonBonesName))
                {
                    var dragonBonesData = _dragonBonesDataMap[dragonBonesName];
                    var armatureData = dragonBonesData.getArmature(armatureName);
                    if (armatureData != null)
                    {
                        dataPackage.dataName = dragonBonesName;
                        dataPackage.data = dragonBonesData;
                        dataPackage.armature = armatureData;
                        dataPackage.skin = armatureData.getSkin(skinName);
                        if (dataPackage.skin == null)
                        {
                            dataPackage.skin = armatureData.getDefaultSkin();
                        }

                        return true;
                    }
                }
            }

            if (!isAvailableName || this.autoSearch) // Will be search all data, if do not give a data name or the autoSearch is true.
            {
                foreach (var pair in _dragonBonesDataMap)
                {
                    if (!isAvailableName || pair.Value.autoSearch)
                    {
                        var armatureData = pair.Value.getArmature(armatureName);
                        if (armatureData != null)
                        {
                            dataPackage.dataName = pair.Key;
                            dataPackage.data = pair.Value;
                            dataPackage.armature = armatureData;
                            dataPackage.skin = armatureData.getSkin(skinName);
                            if (dataPackage.skin == null)
                            {
                                dataPackage.skin = armatureData.getDefaultSkin();
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /**
         * @private
         */
        protected void _buildBones(BuildArmaturePackage dataPackage, Armature armature)
        {
            foreach (var boneData in dataPackage.armature.getSortedBones())
            {
                var bone = BaseObject.borrowObject<Bone>();
                bone.name = boneData.name;
                bone.inheritTranslation = boneData.inheritTranslation;
                bone.inheritRotation = boneData.inheritRotation;
                bone.inheritScale = boneData.inheritScale;
                bone.length = boneData.length;
                bone.origin.copyFrom(boneData.transform);

                if (boneData.parent != null)
                {
                    armature.addBone(bone, boneData.parent.name);
                }
                else
                {
                    armature.addBone(bone);
                }

                if (boneData.ik != null)
                {
                    bone.ikBendPositive = boneData.bendPositive;
                    bone.ikWeight = boneData.weight;
                    bone._setIK(armature.getBone(boneData.ik.name), boneData.chain, boneData.chainIndex);
                }
            }
        }

        /**
         * @private
         */
        protected void _buildSlots(BuildArmaturePackage dataPackage, Armature armature)
        {
            var currentSkin = dataPackage.skin;
            var defaultSkin = dataPackage.armature.getDefaultSkin();
            var slotDisplayDataSetMap = new Dictionary<string, SlotDisplayDataSet>();

            foreach (var slotDisplayDataSet in defaultSkin.slots.Values)
            {
                slotDisplayDataSetMap[slotDisplayDataSet.slot.name] = slotDisplayDataSet;
            }

            if (currentSkin != defaultSkin)
            {
                foreach (var slotDisplayDataSet in currentSkin.slots.Values)
                {
                    slotDisplayDataSetMap[slotDisplayDataSet.slot.name] = slotDisplayDataSet;
                }
            }

            foreach (var slotData in dataPackage.armature.getSortedSlots())
            {
                var slotDisplayDataSet = slotDisplayDataSetMap[slotData.name];
                if (slotDisplayDataSet == null)
                {
                    continue;
                }

                var slot = _generateSlot(dataPackage, slotDisplayDataSet);
                if (slot != null)
                {
                    slot._displayDataSet = slotDisplayDataSet;
                    slot._setDisplayIndex(slotData.displayIndex);
                    slot._setBlendMode(slotData.blendMode);
                    slot._setColor(slotData.color);

                    armature.addSlot(slot, slotData.parent.name);
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
                    DragonBones.resizeList(displayList, displayIndex + 1, null);
                }

                if (slot._replacedDisplayDataSet.Count <= displayIndex)
                {
                    DragonBones.resizeList(slot._replacedDisplayDataSet, displayIndex + 1, null);
                }

                slot._replacedDisplayDataSet[displayIndex] = displayData;

                if (displayData.type == DisplayType.Armature)
                {
                    var childArmature = buildArmature(displayData.name, dataPackage.dataName);
                    displayList[displayIndex] = childArmature;
                }
                else
                {
                    if (displayData.texture == null)
                    {
                        displayData.texture = _getTextureData(dataPackage.dataName, displayData.name);
                    }

                    if (
                        displayData.mesh != null ||
                        (displayIndex < slot._displayDataSet.displays.Count && slot._displayDataSet.displays[displayIndex].mesh != null)
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
                slot.invalidUpdate();
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
        protected abstract Slot _generateSlot(BuildArmaturePackage dataPackage, SlotDisplayDataSet slotDisplayDataSet);

        /**
         * @language zh_CN
         * 解析并添加龙骨数据。
         * @param rawData 需要解析的原始数据。 (JSON)
         * @param dragonBonesName 为数据提供一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @returns DragonBonesData
         * @see #getDragonBonesData()
         * @see #addDragonBonesData()
         * @see #removeDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 4.5
         */
        public DragonBonesData parseDragonBonesData(Dictionary<string, object> rawData, string dragonBonesName = null)
        {
            var dragonBonesData = _dataParser.parseDragonBonesData(rawData, 1.0f);
            addDragonBonesData(dragonBonesData, dragonBonesName);

            return dragonBonesData;
        }

        /**
         * @language zh_CN
         * 解析并添加贴图集数据。
         * @param rawData 需要解析的原始数据。 (JSON)
         * @param textureAtlas 贴图集数据。 (JSON)
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @param scale 为贴图集设置一个缩放值。
         * @returns 贴图集数据
         * @see #getTextureAtlasData()
         * @see #addTextureAtlasData()
         * @see #removeTextureAtlasData()
         * @see dragonBones.TextureAtlasData
         * @version DragonBones 4.5
         */
        public TextureAtlasData parseTextureAtlasData(Dictionary<string, object> rawData, object textureAtlas, string name = null, float scale = 0.0f)
        {
            var textureAtlasData = _generateTextureAtlasData(null, null);
            _dataParser.parseTextureAtlasData(rawData, textureAtlasData, scale);

            _generateTextureAtlasData(textureAtlasData, textureAtlas);
            addTextureAtlasData(textureAtlasData, name);

            return textureAtlasData;
        }

        /**
         * @language zh_CN
         * 获取指定名称的龙骨数据。
         * @param name 数据名称。
         * @returns DragonBonesData
         * @see #parseDragonBonesData()
         * @see #addDragonBonesData()
         * @see #removeDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 3.0
         */
        public DragonBonesData getDragonBonesData(string name)
        {
            return _dragonBonesDataMap[name];
        }

        /**
         * @language zh_CN
         * 添加龙骨数据。
         * @param data 龙骨数据。
         * @param dragonBonesName 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @see #parseDragonBonesData()
         * @see #getDragonBonesData()
         * @see #removeDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 3.0
         */
        public void addDragonBonesData(DragonBonesData data, string dragonBonesName = null)
        {
            if (data != null)
            {
                dragonBonesName = DragonBones.isAvailableString(dragonBonesName) ? dragonBonesName : data.name;
                if (DragonBones.isAvailableString(dragonBonesName))
                {
                    if (!_dragonBonesDataMap.ContainsKey(dragonBonesName))
                    {
                        _dragonBonesDataMap[dragonBonesName] = data;
                    }
                    else
                    {
                        DragonBones.warn("Same name data. " + dragonBonesName);
                    }
                }
                else
                {
                    DragonBones.warn("Unnamed data.");
                }
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @language zh_CN
         * 移除龙骨数据。
         * @param dragonBonesName 数据名称。
         * @param disposeData 是否释放数据。 [false: 不释放, true: 释放]
         * @see #parseDragonBonesData()
         * @see #getDragonBonesData()
         * @see #addDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 3.0
         */
        public void removeDragonBonesData(string dragonBonesName, bool disposeData = true)
        {
            if (_dragonBonesDataMap.ContainsKey(dragonBonesName))
            {
                if (disposeData)
                {
                    _dragonBonesDataMap[dragonBonesName].returnToPool();
                }

                _dragonBonesDataMap.Remove(dragonBonesName);
            }
        }

        /**
         * @language zh_CN
         * 获取指定名称的贴图集数据列表。
         * @param dragonBonesName 数据名称。
         * @returns 贴图集数据列表。
         * @see #parseTextureAtlasData()
         * @see #addTextureAtlasData()
         * @see #removeTextureAtlasData()
         * @see dragonBones.textures.TextureAtlasData
         * @version DragonBones 3.0
         */
        public List<TextureAtlasData> getTextureAtlasData(string dragonBonesName)
        {
            return _textureAtlasDataMap[dragonBonesName];
        }

        /**
         * @language zh_CN
         * 添加贴图集数据。
         * @param data 贴图集数据。
         * @param dragonBonesName 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @see #parseTextureAtlasData()
         * @see #getTextureAtlasData()
         * @see #removeTextureAtlasData()
         * @see dragonBones.textures.TextureAtlasData
         * @version DragonBones 3.0
         */
        public void addTextureAtlasData(TextureAtlasData data, string dragonBonesName = null)
        {
            if (data != null)
            {
                dragonBonesName = DragonBones.isAvailableString(dragonBonesName) ? dragonBonesName : data.name;
                if (DragonBones.isAvailableString(dragonBonesName))
                {
                    var textureAtlasList = _textureAtlasDataMap.ContainsKey(dragonBonesName) ? _textureAtlasDataMap[dragonBonesName] : (_textureAtlasDataMap[dragonBonesName] = new List<TextureAtlasData>());
                    if (!textureAtlasList.Contains(data))
                    {
                        textureAtlasList.Add(data);
                    }
                }
                else
                {
                    DragonBones.warn("Unnamed data.");
                }
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @language zh_CN
         * 移除贴图集数据。
         * @param dragonBonesName 数据名称。
         * @param disposeData 是否释放数据。 [false: 不释放, true: 释放]
         * @see #parseTextureAtlasData()
         * @see #getTextureAtlasData()
         * @see #addTextureAtlasData()
         * @see dragonBones.textures.TextureAtlasData
         * @version DragonBones 3.0
         */
        public void removeTextureAtlasData(string dragonBonesName, bool disposeData = true)
        {
            if (_textureAtlasDataMap.ContainsKey(dragonBonesName))
            {
                if (disposeData)
                {
                    foreach (var textureAtlasData in _textureAtlasDataMap[dragonBonesName])
                    {
                        textureAtlasData.returnToPool();
                    }
                }

                _textureAtlasDataMap.Remove(dragonBonesName);
            }
        }

        /**
         * @language zh_CN
         * 清除所有的数据。
         * @param disposeData 是否释放数据。 [false: 不释放, true: 释放]
         * @version DragonBones 4.5
         */
        public void clear(bool disposeData = true)
        {
            if (disposeData)
            {
                foreach (var dragonBonesData in _dragonBonesDataMap.Values)
                {
                    dragonBonesData.returnToPool();
                }

                foreach (var textureAtlasDataList in _textureAtlasDataMap.Values)
                {
                    foreach (var textureAtlasData in textureAtlasDataList)
                    {
                        textureAtlasData.returnToPool();
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
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         */
        public Armature buildArmature(string armatureName, string dragonBonesName = null, string skinName = null)
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dragonBonesName, armatureName, skinName, dataPackage))
            {
                var armature = _generateArmature(dataPackage);
                _buildBones(dataPackage, armature);
                _buildSlots(dataPackage, armature);

                armature.advanceTime(0.0f); // Update armature pose.

                return armature;
            }

            DragonBones.warn("No armature data. " + armatureName + " " + dragonBonesName != null ? dragonBonesName : "");

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
         * @see dragonBones.Armature
         * @version DragonBones 4.5
         */
        public bool copyAnimationsToArmature(
            Armature toArmature, string fromArmatreName, string fromSkinName = null,
            string fromDragonBonesDataName = null, bool ifRemoveOriginalAnimationList = true
        )
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(fromDragonBonesDataName, fromArmatreName, fromSkinName, dataPackage))
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
                    foreach (var toSlot in toArmature.getSlots())
                    {
                        var toSlotDisplayList = toSlot.displayList;
                        for (int i = 0, l = toSlotDisplayList.Count; i < l; ++i)
                        {
                            var toDisplayObject = toSlotDisplayList[i];
                            if (toDisplayObject is Armature)
                            {
                                var displays = dataPackage.skin.getSlot(toSlot.name).displays;
                                if (i < displays.Count)
                                {
                                    var fromDisplayData = displays[i];
                                    if (fromDisplayData.type == DisplayType.Armature)
                                    {
                                        copyAnimationsToArmature((Armature)toDisplayObject, fromDisplayData.name, fromSkinName, fromDragonBonesDataName, ifRemoveOriginalAnimationList);
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
         * 将指定插槽的显示对象替换为指定资源创造出的显示对象。
         * @param dragonBonesName 指定的龙骨数据名称。
         * @param armatureName 指定的骨架名称。
         * @param slotName 指定的插槽名称。
         * @param displayName 指定的显示对象名称。
         * @param slot 指定的插槽实例。
         * @param displayIndex 要替换的显示对象的索引，如果未设置，则替换当前正在显示的显示对象。
         * @version DragonBones 4.5
         */
        public void replaceSlotDisplay(string dragonBonesName, string armatureName, string slotName, string displayName, Slot slot, int displayIndex = -1)
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dragonBonesName, armatureName, null, dataPackage))
            {
                var slotDisplayDataSet = dataPackage.skin.getSlot(slotName);
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
         * 将指定插槽的显示对象列表替换为指定资源创造出的显示对象列表。
         * @param dragonBonesName 指定的 DragonBonesData 名称。
         * @param armatureName 指定的骨架名称。
         * @param slotName 指定的插槽名称。
         * @param slot 指定的插槽实例。
         * @version DragonBones 4.5
         */
        public void replaceSlotDisplayList(string dragonBonesName, string armatureName, string slotName, Slot slot)
        {
            var dataPackage = new BuildArmaturePackage();
            if (_fillBuildArmaturePackage(dragonBonesName, armatureName, null, dataPackage))
            {
                var slotDisplayDataSet = dataPackage.skin.getSlot(slotName);
                if (slotDisplayDataSet != null)
                {
                    int displayIndex = 0;
                    foreach (var displayData in slotDisplayDataSet.displays)
                    {
                        _replaceSlotDisplay(dataPackage, displayData, slot, displayIndex++);
                    }
                }
            }
        }
    }
}