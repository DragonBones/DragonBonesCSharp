using System.Collections.Generic;
using UnityEngine;

namespace dragonBones
{
    public class UnityFactory : BaseFactory
    {
        /**
         * @language zh_CN
         * 一个可以直接使用的全局工厂实例.
         * @version DragonBones 4.7
         */
        public static UnityFactory factory = new UnityFactory();

        /**
         * @private
         */
        internal static WorldClock _clock = new WorldClock();

        private static void _clockHandler(float time)
        {
            _clock.advanceTime(-1.0f);
        }

        /**
         * @language zh_CN
         * 创建一个工厂。 (通常只需要一个全局工厂实例)
         * @param dataParser 龙骨数据解析器，如果不设置，则使用默认解析器。
         * @version DragonBones 3.0
         */
        public UnityFactory(DataParser dataParser = null) : base(dataParser)
        {
        }

        /**
         * @private
         */
        override protected TextureAtlasData _generateTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
        {
            if (textureAtlasData != null)
            {
                ((UnityTextureAtlasData)textureAtlasData).texture = (Texture2D)textureAtlas;
            }
            else
            {
                textureAtlasData = BaseObject.borrowObject<UnityTextureAtlasData>();
            }

            return textureAtlasData;
        }

        /**
         * @private
         */
        override protected Armature _generateArmature(BuildArmaturePackage dataPackage)
        {
            var armature = BaseObject.borrowObject<Armature>();
            var armatureDisplayContainer = new UnityArmatureDisplay();

            armature._armatureData = dataPackage.armature;
            armature._skinData = dataPackage.skin;
            armature._animation = BaseObject.borrowObject<Animation>();
            armature._display = armatureDisplayContainer;
            //armature._eventManager = _eventManager;

            armatureDisplayContainer._armature = armature;
            armature._animation._armature = armature;

            armature.animation.animations = dataPackage.armature.animations;

            return armature;
        }

        /**
         * @private
         */
        override protected Slot _generateSlot(BuildArmaturePackage dataPackage, SlotDisplayDataSet slotDisplayDataSet)
        {
            var slot = BaseObject.borrowObject<UnitySlot>();
            var slotData = slotDisplayDataSet.slot;
            var displayList = new List<object>();

            slot.name = slotData.name;
            slot._rawDisplay = new GameObject();
            slot._meshDisplay = new GameObject();

            foreach (var displayData in slotDisplayDataSet.displays)
            {
                switch (displayData.type)
                {
                    case DisplayType.Image:
                        if (displayData.texture == null)
                        {
                            displayData.texture = this._getTextureData(dataPackage.dataName, displayData.name);
                        }

                        displayList.Add(slot._rawDisplay);
                        break;

                    case DisplayType.Mesh:
                        if (displayData.texture == null)
                        {
                            displayData.texture = this._getTextureData(dataPackage.dataName, displayData.name);
                        }

                        displayList.Add(slot._meshDisplay);
                        break;

                    case DisplayType.Armature:
                        var childArmature = this.buildArmature(displayData.name, dataPackage.dataName);
                        if (childArmature != null)
                        {
                            if (!slot.inheritAnimation)
                            {
                                var actions = slotData.actions.Count > 0 ? slotData.actions : childArmature.armatureData.actions;
                                if (actions.Count > 0)
                                {
                                    foreach (var actionData in actions)
                                    {
                                        childArmature._bufferAction(actionData);
                                    }
                                }
                                else
                                {
                                    childArmature.animation.play();
                                }
                            }

                            displayData.armature = childArmature.armatureData; // 
                        }

                        displayList.Add(childArmature);
                        break;

                    default:
                        displayList.Add(null);
                        break;
                }
            }

            slot._setDisplayList(displayList);

            return slot;
        }

        /**
         * @language zh_CN
         * 创建一个指定名称的骨架，并使用骨架的显示容器来更新骨架动画。
         * @param armatureName 骨架数据名称。
         * @param dragonBonesName 龙骨数据名称，如果未设置，将检索所有的龙骨数据，如果多个数据中包含同名的骨架数据，可能无法创建出准确的骨架。
         * @param skinName 皮肤名称，如果未设置，则使用默认皮肤。
         * @returns 骨架的显示容器。
         * @see dragonBones.IArmatureDisplayContainer
         * @version DragonBones 4.5
         */
        public UnityArmatureDisplay buildArmatureDisplay(string armatureName, string dragonBonesName = null, string skinName = null)
        {
            var armature = this.buildArmature(armatureName, dragonBonesName, skinName);
            var armatureDisplay = armature != null ? (UnityArmatureDisplay)armature._display : null;
            if (armatureDisplay != null)
            {
                armatureDisplay.advanceTimeBySelf(true);
            }

            return armatureDisplay;
        }

        /**
         * @language zh_CN
         * 获取带有指定贴图的显示对象。
         * @param textureName 指定的贴图名称。
         * @param dragonBonesName 指定的龙骨数据名称，如果未设置，将检索所有的龙骨数据。
         * @version DragonBones 3.0
         */
        public GameObject getTextureDisplay(string textureName, string dragonBonesName = null)
        {
            var textureData = (UnityTextureData)this._getTextureData(dragonBonesName, textureName);
            if (textureData != null)
            {
                if (textureData.texture == null)
                {
                    var textureAtlasTexture = ((UnityTextureAtlasData)textureData.parent).texture;
                    /*textureData.texture = new egret.Texture();
                    textureData.texture._bitmapData = textureAtlasTexture._bitmapData;

                    textureData.texture.$initData(
                        textureData.region.x, textureData.region.y,
                        textureData.region.width, textureData.region.height,
                        0, 0,
                        textureData.region.width, textureData.region.height,
                        textureAtlasTexture.textureWidth, textureAtlasTexture.textureHeight
                    );*/
                }

                //return new egret.Bitmap(textureData.texture);
            }

            return null;
        }

        /**
         * @language zh_CN
         * 获取全局声音事件管理器。
         * @version DragonBones 4.5
         */
        /*public soundEventManater {
            return EgretFactory._eventManager;
        }*/
    }
}