using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public class BuildArmaturePackage
    {
        public string dataName = "";
        public string textureAtlasName = "";
        public DragonBonesData data;
        public ArmatureData armature;
        public SkinData skin;
    }
    /**
     * 创建骨架的基础工厂。 (通常只需要一个全局工厂实例)
     * @see dragonBones.DragonBonesData
     * @see dragonBones.TextureAtlasData
     * @see dragonBones.ArmatureData
     * @see dragonBones.Armature
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public abstract class BaseFactory
    {
        /**
         * 是否开启共享搜索。
         * 如果开启，创建一个骨架时，可以从多个龙骨数据中寻找骨架数据，或贴图集数据中寻找贴图数据。 (通常在有共享导出的数据时开启)
         * @see dragonBones.DragonBonesData#autoSearch
         * @see dragonBones.TextureAtlasData#autoSearch
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public bool autoSearch = false;
        /**
         * @private
         */
        protected static ObjectDataParser _objectParser = null;
        /**
         * @private
         */
        protected static BinaryDataParser _binaryParser = null;
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
        protected DragonBones _dragonBones = null;
        /**
         * @private
         */
        protected DataParser _dataParser = null;
        /**
         * 创建一个工厂。 (通常只需要一个全局工厂实例)
         * @param dataParser 龙骨数据解析器，如果不设置，则使用默认解析器。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public BaseFactory(DataParser dataParser = null)
        {
            if (BaseFactory._objectParser == null)
            {
                BaseFactory._objectParser = new ObjectDataParser();
            }

            if (BaseFactory._binaryParser == null)
            {
                BaseFactory._binaryParser = new BinaryDataParser();
            }

            this._dataParser = dataParser != null ? dataParser : BaseFactory._objectParser;
        }
        /** 
         * @private 
         */
        protected bool _IsSupportMesh()
        {
            return true;
        }
        /** 
         * @private 
         */
        protected TextureData _GetTextureData(string textureAtlasName, string textureName)
        {
            if (this._textureAtlasDataMap.ContainsKey(textureAtlasName))
            {
                foreach (var textureAtlasData in this._textureAtlasDataMap[textureAtlasName])
                {
                    var textureData = textureAtlasData.GetTexture(textureName);
                    if (textureData != null)
                    {
                        return textureData;
                    }
                }
            }

            if (this.autoSearch)
            {
                // Will be search all data, if the autoSearch is true.
                foreach (var values in this._textureAtlasDataMap.Values)
                {
                    foreach (var textureAtlasData in values)
                    {
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
        protected bool _FillBuildArmaturePackage(BuildArmaturePackage dataPackage,
                                                string dragonBonesName,
                                                string armatureName,
                                                string skinName,
                                                string textureAtlasName)
        {
            DragonBonesData dragonBonesData = null;
            ArmatureData armatureData = null;

            if (dragonBonesName.Length > 0)
            {
                if (this._dragonBonesDataMap.ContainsKey(dragonBonesName))
                {
                    dragonBonesData = this._dragonBonesDataMap[dragonBonesName];
                    armatureData = dragonBonesData.GetArmature(armatureName);
                }
            }

            if (armatureData == null && (dragonBonesName.Length == 0 || this.autoSearch))
            {
                // Will be search all data, if do not give a data name or the autoSearch is true.
                foreach (var key in this._dragonBonesDataMap.Keys)
                {
                    dragonBonesData = this._dragonBonesDataMap[key];
                    if (dragonBonesName.Length == 0 || dragonBonesData.autoSearch)
                    {
                        armatureData = dragonBonesData.GetArmature(armatureName);
                        if (armatureData != null)
                        {
                            dragonBonesName = key;
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
                dataPackage.skin = null;

                if (skinName.Length > 0)
                {
                    dataPackage.skin = armatureData.GetSkin(skinName);
                    if (dataPackage.skin == null && this.autoSearch)
                    {
                        foreach (var k in this._dragonBonesDataMap.Keys)
                        {
                            var skinDragonBonesData = this._dragonBonesDataMap[k];
                            var skinArmatureData = skinDragonBonesData.GetArmature(skinName);
                            if (skinArmatureData != null)
                            {
                                dataPackage.skin = skinArmatureData.defaultSkin;
                                break;
                            }
                        }
                    }
                }

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
        protected void _BuildBones(BuildArmaturePackage dataPackage, Armature armature)
        {
            var bones = dataPackage.armature.sortedBones;
            for (var i = 0; i < bones.Count; ++i)
            {
                var boneData = bones[i];
                var bone = BaseObject.BorrowObject<Bone>();
                bone.Init(boneData);

                if (boneData.parent != null)
                {
                    armature.AddBone(bone, boneData.parent.name);
                }
                else
                {
                    armature.AddBone(bone);
                }

                var constraints = boneData.constraints;

                for (var j = 0; j < constraints.Count; ++j)
                {
                    var constraintData = constraints[j];
                    var target = armature.GetBone(constraintData.target.name);
                    if (target == null)
                    {
                        continue;
                    }

                    // TODO more constraint type.
                    var ikConstraintData = constraintData as IKConstraintData;
                    var constraint = BaseObject.BorrowObject<IKConstraint>();
                    var root = ikConstraintData.root != null ? armature.GetBone(ikConstraintData.root.name) : null;
                    constraint.target = target;
                    constraint.bone = bone;
                    constraint.root = root;
                    constraint.bendPositive = ikConstraintData.bendPositive;
                    constraint.scaleEnabled = ikConstraintData.scaleEnabled;
                    constraint.weight = ikConstraintData.weight;

                    if (root != null)
                    {
                        root.AddConstraint(constraint);
                    }
                    else
                    {
                        bone.AddConstraint(constraint);
                    }
                }
            }
        }
        /**
         * @private
         */
        protected void _BuildSlots(BuildArmaturePackage dataPackage, Armature armature)
        {
            var currentSkin = dataPackage.skin;
            var defaultSkin = dataPackage.armature.defaultSkin;
            if (currentSkin == null || defaultSkin == null)
            {
                return;
            }

            Dictionary<string, List<DisplayData>> skinSlots = new Dictionary<string, List<DisplayData>>();
            foreach (var key in defaultSkin.displays.Keys)
            {
                var displays = defaultSkin.displays[key];
                skinSlots[key] = displays;
            }

            if (currentSkin != defaultSkin)
            {
                foreach (var k in currentSkin.displays.Keys)
                {
                    var displays = currentSkin.displays[k];
                    skinSlots[k] = displays;
                }
            }

            foreach (var slotData in dataPackage.armature.sortedSlots)
            {
                if (!skinSlots.ContainsKey(slotData.name))
                {
                    continue;
                }

                var displays = skinSlots[slotData.name];
                var slot = this._BuildSlot(dataPackage, slotData, displays, armature);
                var displayList = new List<object>();
                foreach (var displayData in displays)
                {
                    if (displayData != null)
                    {
                        displayList.Add(this._GetSlotDisplay(dataPackage, displayData, null, slot));
                    }
                    else
                    {
                        displayList.Add(null);
                    }
                }

                armature.AddSlot(slot, slotData.parent.name);
                slot._SetDisplayList(displayList);
                slot._SetDisplayIndex(slotData.displayIndex, true);
            }
        }
        /**
         * @private
         */
        protected object _GetSlotDisplay(BuildArmaturePackage dataPackage, DisplayData displayData, DisplayData rawDisplayData, Slot slot)
        {
            var dataName = dataPackage != null ? dataPackage.dataName : displayData.parent.parent.name;
            object display = null;
            switch (displayData.type)
            {
                case DisplayType.Image:
                    var imageDisplayData = displayData as ImageDisplayData;
                    if (imageDisplayData.texture == null)
                    {
                        imageDisplayData.texture = this._GetTextureData(dataName, displayData.path);
                    }
                    else if (dataPackage != null && dataPackage.textureAtlasName.Length > 0)
                    {
                        imageDisplayData.texture = this._GetTextureData(dataPackage.textureAtlasName, displayData.path);
                    }

                    if (rawDisplayData != null && rawDisplayData.type == DisplayType.Mesh && this._IsSupportMesh())
                    {
                        display = slot.meshDisplay;
                    }
                    else
                    {
                        display = slot.rawDisplay;
                    }
                    break;
                case DisplayType.Mesh:
                    var meshDisplayData = displayData as MeshDisplayData;
                    if (meshDisplayData.texture == null)
                    {
                        meshDisplayData.texture = this._GetTextureData(dataName, meshDisplayData.path);
                    }
                    else if (dataPackage != null && dataPackage.textureAtlasName.Length > 0)
                    {
                        meshDisplayData.texture = this._GetTextureData(dataPackage.textureAtlasName, meshDisplayData.path);
                    }

                    if (this._IsSupportMesh())
                    {
                        display = slot.meshDisplay;
                    }
                    else
                    {
                        display = slot.rawDisplay;
                    }
                    break;
                case DisplayType.Armature:
                    var armatureDisplayData = displayData as ArmatureDisplayData;
                    var childArmature = this.BuildArmature(armatureDisplayData.path, dataName, null, dataPackage != null ? dataPackage.textureAtlasName : null);
                    if (childArmature != null)
                    {
                        childArmature.inheritAnimation = armatureDisplayData.inheritAnimation;
                        if (!childArmature.inheritAnimation)
                        {
                            var actions = armatureDisplayData.actions.Count > 0 ? armatureDisplayData.actions : childArmature.armatureData.defaultActions;
                            if (actions.Count > 0)
                            {
                                foreach (var action in actions)
                                {
                                    childArmature._BufferAction(action, true);
                                }
                            }
                            else
                            {
                                childArmature.animation.Play();
                            }
                        }

                        armatureDisplayData.armature = childArmature.armatureData; // 
                    }

                    display = childArmature;
                    break;
            }

            return display;
        }
        /**
         * @private
         */
        protected void _ReplaceSlotDisplay(BuildArmaturePackage dataPackage, DisplayData displayData, Slot slot, int displayIndex)
        {
            if (displayIndex < 0)
            {
                displayIndex = slot.displayIndex;
            }

            if (displayIndex < 0)
            {
                displayIndex = 0;
            }

            var displayList = slot.displayList; // Copy.
            if (displayList.Count <= displayIndex)
            {
                displayList.ResizeList(displayIndex + 1);
            }

            if (slot._displayDatas.Count <= displayIndex)
            {
                slot._displayDatas.ResizeList(displayIndex + 1);
            }

            slot._displayDatas[displayIndex] = displayData;
            if (displayData != null)
            {
                displayList[displayIndex] = this._GetSlotDisplay(dataPackage,
                                                                displayData,
                                                                displayIndex < slot._rawDisplayDatas.Count ? slot._rawDisplayDatas[displayIndex] : null, slot);
            }
            else
            {
                displayList[displayIndex] = null;
            }

            slot.displayList = displayList;
        }
        /** 
         * @private 
         */
        protected abstract TextureAtlasData _BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas);
        /** 
         * @private 
         */
        protected abstract Armature _BuildArmature(BuildArmaturePackage dataPackage);
        /** 
         * @private 
         */
        protected abstract Slot _BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, List<DisplayData> displays, Armature armature);
        /**
         * 解析并添加龙骨数据。
         * @param rawData 需要解析的原始数据。
         * @param name 为数据提供一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @returns DragonBonesData
         * @see #getDragonBonesData()
         * @see #addDragonBonesData()
         * @see #removeDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public DragonBonesData ParseDragonBonesData(object rawData, string name = null, float scale = 1.0f)
        {
            DragonBonesData dragonBonesData = null;
            if (rawData is byte[])
            {
                dragonBonesData = BaseFactory._binaryParser.ParseDragonBonesData(rawData, scale);
            }
            else
            {
                dragonBonesData = this._dataParser.ParseDragonBonesData(rawData, scale);
            }

            while (true)
            {
                var textureAtlasData = this._BuildTextureAtlasData(null, null);
                if (this._dataParser.ParseTextureAtlasData(null, textureAtlasData, scale))
                {
                    this.AddTextureAtlasData(textureAtlasData, name);
                }
                else
                {
                    textureAtlasData.ReturnToPool();
                    break;
                }
            }

            if (dragonBonesData != null)
            {
                this.AddDragonBonesData(dragonBonesData, name);
            }

            return dragonBonesData;
        }
        /**
         * 解析并添加贴图集数据。
         * @param rawData 需要解析的原始数据。 (JSON)
         * @param textureAtlas 贴图。
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @param scale 为贴图集设置一个缩放值。
         * @returns 贴图集数据
         * @see #getTextureAtlasData()
         * @see #addTextureAtlasData()
         * @see #removeTextureAtlasData()
         * @see dragonBones.TextureAtlasData
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public TextureAtlasData ParseTextureAtlasData(Dictionary<string, object>  rawData, object textureAtlas, string name = null, float scale = 0.0f)
        {
            var textureAtlasData = this._BuildTextureAtlasData(null, null);
            this._dataParser.ParseTextureAtlasData(rawData, textureAtlasData, scale);
            this._BuildTextureAtlasData(textureAtlasData, textureAtlas);
            this.AddTextureAtlasData(textureAtlasData, name);

            return textureAtlasData;
        }
        /**
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public void UpdateTextureAtlasData(string name, List<object> textureAtlases)
        {
            var textureAtlasDatas = this.GetTextureAtlasData(name);
            if (textureAtlasDatas != null)
            {
                for (int i = 0, l = textureAtlasDatas.Count; i < l; ++i)
                {
                    if (i < textureAtlases.Count)
                    {
                        this._BuildTextureAtlasData(textureAtlasDatas[i], textureAtlases[i]);
                    }
                }
            }
        }
        /**
         * 获取指定名称的龙骨数据。
         * @param name 数据名称。
         * @returns DragonBonesData
         * @see #parseDragonBonesData()
         * @see #addDragonBonesData()
         * @see #removeDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public DragonBonesData GetDragonBonesData(string name)
        {
            return this._dragonBonesDataMap.ContainsKey(name) ? this._dragonBonesDataMap[name] : null;
        }
        /**
         * 添加龙骨数据。
         * @param data 龙骨数据。
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @see #parseDragonBonesData()
         * @see #getDragonBonesData()
         * @see #removeDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void AddDragonBonesData(DragonBonesData data, string name = null)
        {
            name = !string.IsNullOrEmpty(name) ? name : data.name;
            if (this._dragonBonesDataMap.ContainsKey(name))
            {
                if (this._dragonBonesDataMap[name] == data)
                {
                    return;
                }

                Helper.Assert(false, "Can not add same name data: " + name);
                return;
            }

            this._dragonBonesDataMap[name] = data;
        }
        /**
         * 移除龙骨数据。
         * @param name 数据名称。
         * @param disposeData 是否释放数据。
         * @see #parseDragonBonesData()
         * @see #getDragonBonesData()
         * @see #addDragonBonesData()
         * @see dragonBones.DragonBonesData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public virtual void RemoveDragonBonesData(string name, bool disposeData = true)
        {
            if (this._dragonBonesDataMap.ContainsKey(name))
            {
                if (disposeData)
                {
                    this._dragonBones.BufferObject(this._dragonBonesDataMap[name]);
                }

                this._dragonBonesDataMap.Remove(name);
            }
        }
        /**
         * 获取指定名称的贴图集数据列表。
         * @param name 数据名称。
         * @returns 贴图集数据列表。
         * @see #parseTextureAtlasData()
         * @see #addTextureAtlasData()
         * @see #removeTextureAtlasData()
         * @see dragonBones.TextureAtlasData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public List<TextureAtlasData> GetTextureAtlasData(string name)
        {
            return this._textureAtlasDataMap.ContainsKey(name) ? this._textureAtlasDataMap[name] : null;
        }
        /**
         * 添加贴图集数据。
         * @param data 贴图集数据。
         * @param name 为数据指定一个名称，以便可以通过这个名称获取数据，如果未设置，则使用数据中的名称。
         * @see #parseTextureAtlasData()
         * @see #getTextureAtlasData()
         * @see #removeTextureAtlasData()
         * @see dragonBones.TextureAtlasData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public void AddTextureAtlasData(TextureAtlasData data, string name = null)
        {
            name = !string.IsNullOrEmpty(name) ? name : data.name;
            var textureAtlasList = (this._textureAtlasDataMap.ContainsKey(name)) ?
                                    this._textureAtlasDataMap[name] :
                                    (this._textureAtlasDataMap[name] = new List<TextureAtlasData>());
            if (!textureAtlasList.Contains(data))
            {
                textureAtlasList.Add(data);
            }
        }
        /**
         * 移除贴图集数据。
         * @param name 数据名称。
         * @param disposeData 是否释放数据。
         * @see #parseTextureAtlasData()
         * @see #getTextureAtlasData()
         * @see #addTextureAtlasData()
         * @see dragonBones.TextureAtlasData
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public virtual void RemoveTextureAtlasData(string name, bool disposeData = true)
        {
            if (this._textureAtlasDataMap.ContainsKey(name))
            {
                var textureAtlasDataList = this._textureAtlasDataMap[name];
                if (disposeData) {
                    foreach (var textureAtlasData in textureAtlasDataList)
                    {
                        this._dragonBones.BufferObject(textureAtlasData);
                    }
                }

                this._textureAtlasDataMap.Remove(name);
            }
        }
        /**
         * 获取骨架数据。
         * @param name 骨架数据名称。
         * @param dragonBonesName 龙骨数据名称。
         * @see dragonBones.ArmatureData
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public virtual ArmatureData GetArmatureData(string name, string dragonBonesName = "")
        {
            var dataPackage = new BuildArmaturePackage();
            if (!this._FillBuildArmaturePackage(dataPackage, dragonBonesName, name, "", ""))
            {
                return null;
            }

            return dataPackage.armature;
        }
        /**
         * 清除所有的数据。
         * @param disposeData 是否释放数据。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public virtual void Clear(bool disposeData = true)
        {
            if (disposeData)
            {
                foreach (var dragonBoneData in this._dragonBonesDataMap.Values)
                {
                    this._dragonBones.BufferObject(dragonBoneData);
                }
                
                foreach (var textureAtlasDatas in this._textureAtlasDataMap.Values)
                {
                    foreach (var textureAtlasData in textureAtlasDatas)
                    {
                        this._dragonBones.BufferObject(textureAtlasData);
                    }
                }
            }

            _dragonBonesDataMap.Clear();
            _textureAtlasDataMap.Clear();
        }
        /**
         * 创建一个骨架。
         * @param armatureName 骨架数据名称。
         * @param dragonBonesName 龙骨数据名称，如果未设置，将检索所有的龙骨数据，当多个龙骨数据中包含同名的骨架数据时，可能无法创建出准确的骨架。
         * @param skinName 皮肤名称，如果未设置，则使用默认皮肤。
         * @param textureAtlasName 贴图集数据名称，如果未设置，则使用龙骨数据名称。
         * @returns 骨架
         * @see dragonBones.ArmatureData
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Armature BuildArmature(string armatureName, string dragonBonesName = null, string skinName = null, string textureAtlasName = null)
        {
            var dataPackage = new BuildArmaturePackage();
            if (!this._FillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, skinName, textureAtlasName))
            {
                Helper.Assert(false, "No armature data: " + armatureName + ", " + (dragonBonesName != null ? dragonBonesName : ""));
                return null;
            }

            var armature = this._BuildArmature(dataPackage);
            this._BuildBones(dataPackage, armature);
            this._BuildSlots(dataPackage, armature);
            // armature.invalidUpdate(null, true); TODO
            armature.InvalidUpdate("", true);
            // Update armature pose.
            armature.AdvanceTime(0.0f); 

            return armature;
        }
        /**
         * 用指定资源替换指定插槽的显示对象。(用 "dragonBonesName/armatureName/slotName/displayName" 的资源替换 "slot" 的显示对象)
         * @param dragonBonesName 指定的龙骨数据名称。
         * @param armatureName 指定的骨架名称。
         * @param slotName 指定的插槽名称。
         * @param displayName 指定的显示对象名称。
         * @param slot 指定的插槽实例。
         * @param displayIndex 要替换的显示对象的索引，如果未设置，则替换当前正在显示的显示对象。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public void ReplaceSlotDisplay(string dragonBonesName,
                                        string armatureName,
                                        string slotName,
                                        string displayName,
                                        Slot slot, int displayIndex = -1)
        {
            BuildArmaturePackage dataPackage = new BuildArmaturePackage();
            if (!this._FillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, "", "") || dataPackage.skin == null)
            {
                return;
            }

            var displays = dataPackage.skin.GetDisplays(slotName);
            if (displays == null)
            {
                return;
            }

            foreach (var display in displays)
            {
                if (display != null && display.name == displayName)
                {
                    this._ReplaceSlotDisplay(dataPackage, display, slot, displayIndex);
                    break;
                }
            }
        }
        /**
         * 用指定资源列表替换插槽的显示对象列表。
         * @param dragonBonesName 指定的 DragonBonesData 名称。
         * @param armatureName 指定的骨架名称。
         * @param slotName 指定的插槽名称。
         * @param slot 指定的插槽实例。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public void ReplaceSlotDisplayList(string dragonBonesName, string armatureName, string slotName, Slot slot)
        {
            var dataPackage = new BuildArmaturePackage();
            if (!this._FillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, "", "") || dataPackage.skin == null)
            {
                return;
            }

            var displays = dataPackage.skin.GetDisplays(slotName);
            if (displays == null)
            {
                return;
            }

            var displayIndex = 0;
            foreach (var displayData in displays)
            {
                this._ReplaceSlotDisplay(dataPackage, displayData, slot, displayIndex++);
            }
        }
        /**
         * 更换骨架皮肤。
         * @param armature 骨架。
         * @param skin 皮肤数据。
         * @param exclude 不需要更新的插槽。
         * @see dragonBones.Armature
         * @see dragonBones.SkinData
         * @version DragonBones 5.1
         * @language zh_CN
         */
        public void ChangeSkin(Armature armature, SkinData skin, List<string> exclude = null)
        {
            foreach (var slot in armature.GetSlots())
            {
                if (!(skin.displays.ContainsKey(slot.name)) || (exclude != null && exclude.Contains(slot.name)))
                {
                    continue;
                }

                var displays = skin.displays[slot.name];
                var displayList = slot.displayList; // Copy.
                displayList.ResizeList(displays.Count); // Modify displayList length.
                for (int i = 0, l = displays.Count; i < l; ++i)
                {
                    var displayData = displays[i];
                    if (displayData != null)
                    {
                        displayList[i] = this._GetSlotDisplay(null, displayData, null, slot);
                    }
                    else
                    {
                        displayList[i] = null;
                    }
                }

                slot._rawDisplayDatas = displays;
                slot._displayDatas.ResizeList(displays.Count);
                for (int i = 0, l = slot._displayDatas.Count; i < l; ++i)
                {
                    slot._displayDatas[i] = displays[i];
                }

                slot.displayList = displayList;
            }
        }
        /**
         * 将骨架的动画替换成其他骨架的动画。 (通常这些骨架应该具有相同的骨架结构)
         * @param toArmature 指定的骨架。
         * @param fromArmatreName 其他骨架的名称。
         * @param fromSkinName 其他骨架的皮肤名称，如果未设置，则使用默认皮肤。
         * @param fromDragonBonesDataName 其他骨架属于的龙骨数据名称，如果未设置，则检索所有的龙骨数据。
         * @param replaceOriginalAnimation 是否替换原有的同名动画。
         * @returns 是否替换成功。
         * @see dragonBones.Armature
         * @see dragonBones.ArmatureData
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public bool CopyAnimationsToArmature(Armature toArmature,
                                                string fromArmatreName,
                                                string fromSkinName = null,
                                                string fromDragonBonesDataName = null,
                                                bool replaceOriginalAnimation = true)
        {
            var dataPackage = new BuildArmaturePackage();

            if (this._FillBuildArmaturePackage(dataPackage, fromDragonBonesDataName, fromArmatreName, fromSkinName, ""))
            {
                var fromArmatureData = dataPackage.armature;
                if (replaceOriginalAnimation)
                {
                    toArmature.animation.animations = fromArmatureData.animations;
                }
                else
                {
                    Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();
                    foreach (var animationName in toArmature.animation.animations.Keys)
                    {
                        animations[animationName] = toArmature.animation.animations[animationName];
                    }

                    foreach (var animationName in fromArmatureData.animations.Keys)
                    {
                        animations[animationName] = fromArmatureData.animations[animationName];
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
                        for (int j = 0, lJ = toSlotDisplayList.Count; j < lJ; ++j)
                        {
                            var toDisplayObject = toSlotDisplayList[j];
                            if (toDisplayObject is Armature)
                            {
                                var displays = dataPackage.skin.GetDisplays(toSlot.name);
                                if (displays != null && j < displays.Count)
                                {
                                    var fromDisplayData = displays[j];
                                    if (fromDisplayData != null && fromDisplayData.type == DisplayType.Armature)
                                    {
                                        this.CopyAnimationsToArmature(toDisplayObject as Armature, fromDisplayData.path, fromSkinName, fromDragonBonesDataName, replaceOriginalAnimation);
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
         * @private 
         */
        public Dictionary<string, DragonBonesData> GetAllDragonBonesData()
        {
            return this._dragonBonesDataMap;
        }
        /** 
         * @private 
         */
        public Dictionary<string, List<TextureAtlasData>> GetAllTextureAtlasData()
        {
                return this._textureAtlasDataMap;
        }
    }
}
