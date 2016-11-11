using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    /**
     * @private
     */
    internal class ClockHandler : MonoBehaviour
    {
        void Update()
        {
            UnityFactory.clock.AdvanceTime(Time.deltaTime);
        }
    }

    public class UnityFactory : BaseFactory
    {
        private static IEventDispatcher<EventObject> _eventManager = null;
        private static GameObject _gameObject = null;

        /**
         * @language zh_CN
         * 一个正在运行的全局 WorldClock 实例.
         * @version DragonBones 3.0
         */
        public static readonly WorldClock clock = new WorldClock();

        /**
         * @language zh_CN
         * 一个可以直接使用的全局工厂实例.
         * @version DragonBones 4.7
         */
        public static readonly UnityFactory factory = new UnityFactory();

        public string defaultShaderName = "Sprites/Default";
        
        private string _textureAtlasPath = null;
        private GameObject _armatureGameObject = null;
        private readonly Dictionary<string, DragonBonesData> _pathDragonBonesDataMap = new Dictionary<string, DragonBonesData>();
        private readonly Dictionary<string, TextureAtlasData> _pathTextureAtlasDataMap = new Dictionary<string, TextureAtlasData>();

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
                (textureAtlasData as UnityTextureAtlasData).texture = textureAtlas as Material;
            }
            else
            {
                textureAtlasData = BaseObject.BorrowObject<UnityTextureAtlasData>();
            }

            return textureAtlasData;
        }

        /**
         * @private
         */
        override protected Armature _generateArmature(BuildArmaturePackage dataPackage)
        {
            if (Application.isPlaying)
            {
                if (_gameObject == null)
                {
                    _gameObject = new GameObject("DragonBones Object", typeof(ClockHandler));
                    _gameObject.isStatic = true;
                    _gameObject.hideFlags = HideFlags.HideInHierarchy;
                }

                if (_eventManager == null)
                {
                    _eventManager = _gameObject.AddComponent<UnityArmatureComponent>();
                }
            }

            var armature = BaseObject.BorrowObject<Armature>();
            var armatureDisplayContainer = _armatureGameObject == null ? new GameObject() : _armatureGameObject;
            var armatureComponent = armatureDisplayContainer.GetComponent<UnityArmatureComponent>();
            armatureDisplayContainer.name = dataPackage.armature.name;

            if (armatureComponent == null)
            {
                armatureComponent = armatureDisplayContainer.AddComponent<UnityArmatureComponent>();
            }

            armatureComponent._armature = armature;

            armature._armatureData = dataPackage.armature;
            armature._skinData = dataPackage.skin;
            armature._animation = BaseObject.BorrowObject<Animation>();
            armature._display = armatureDisplayContainer;
            armature._eventDispatcher = armatureComponent;
            armature._eventManager = _eventManager;
            armature._animation._armature = armature;
            armature.animation.animations = dataPackage.armature.animations;

            _armatureGameObject = null;

            return armature;
        }

        /**
         * @private
         */
        override protected Slot _generateSlot(BuildArmaturePackage dataPackage, SlotDisplayDataSet slotDisplayDataSet, Armature armature)
        {
            var slot = BaseObject.BorrowObject<UnitySlot>();
            var slotData = slotDisplayDataSet.slot;
            var displayList = new List<object>();
            var armatureDisplay = armature.display as GameObject;
            var transform = armatureDisplay.transform.Find(slotData.name);
            var gameObject = transform == null ? null : transform.gameObject;

            if (gameObject == null)
            {
                gameObject = new GameObject(slotData.name);
                gameObject.AddComponent<MeshRenderer>();
                gameObject.AddComponent<MeshFilter>();
            }

            slot.name = slotData.name;
            slot._rawDisplay = gameObject;
            slot._meshDisplay = slot._rawDisplay;
            
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
                        var childDisplayName = slotData.name + " (" + displayData.name + ")";
                        var childTransform = armatureDisplay.transform.Find(childDisplayName);
                        var childArmature = childTransform == null ?
                            this.BuildArmature(displayData.name, dataPackage.dataName) :
                            BuildArmatureComponent(displayData.name, dataPackage.dataName, null, childTransform.gameObject).armature;

                        if (childArmature != null)
                        {
                            if (childArmature._clock != null)
                            {
                                childArmature._clock.Remove(childArmature);
                            }

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
                                    childArmature.animation.Play();
                                }
                            }

                            displayData.armature = childArmature.armatureData; // 

                            // Hide
                            var childArmatureDisplay = childArmature.display as GameObject;
                            childArmatureDisplay.name = childDisplayName;
                            childArmatureDisplay.gameObject.hideFlags = HideFlags.HideInHierarchy;
                            childArmatureDisplay.SetActive(false);
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

        public DragonBonesData LoadDragonBonesData(string path, string name = null)
        {
            var index = path.LastIndexOf("Resources");
            if (index > 0)
            {
                path = path.Substring(index + 10);
            }

            index = path.LastIndexOf(".");
            if (index > 0)
            {
                path = path.Substring(0, index);
            }

            if (_pathDragonBonesDataMap.ContainsKey(path))
            {
                return _pathDragonBonesDataMap[path];
            }

            var dragonBonesData = LoadDragonBonesData(Resources.Load<TextAsset>(path), name);
            if (dragonBonesData != null)
            {
                _pathDragonBonesDataMap[path] = dragonBonesData;
            }

            return dragonBonesData;
        }

        public DragonBonesData LoadDragonBonesData(TextAsset dragonBonesJSON, string name = null)
        {
            if (dragonBonesJSON == null)
            {
                return null;
            }

            if (DragonBones.IsAvailableString(name))
            {
                var existedData = this.GetDragonBonesData(name);
                if (existedData != null)
                {
                    return existedData;
                }
            }

            return this.ParseDragonBonesData((Dictionary<string, object>)MiniJSON.Json.Deserialize(dragonBonesJSON.text), name, 0.01f); // Unity default Scale Factor.
        }
        
        public UnityTextureAtlasData LoadTextureAtlasData(string path, string name = null, float scale = 0.0f)
        {
            var index = path.LastIndexOf("Resources");
            if (index > 0)
            {
                path = path.Substring(index + 10);
            }

            index = path.LastIndexOf(".");
            if (index > 0)
            {
                path = path.Substring(0, index);
            }

            if (_pathTextureAtlasDataMap.ContainsKey(path))
            {
                return _pathTextureAtlasDataMap[path] as UnityTextureAtlasData;
            }

            _textureAtlasPath = path;

            var textureAtlasData = LoadTextureAtlasData(Resources.Load<TextAsset>(path), name, scale);
            if (textureAtlasData != null)
            {
                _pathTextureAtlasDataMap[path] = textureAtlasData;
            }

            return textureAtlasData;
        }

        public UnityTextureAtlasData LoadTextureAtlasData(TextAsset textureAtlasJSON, string name = null, float scale = 0.0f)
        {
            if (textureAtlasJSON == null)
            {
                return null;
            }

            var textureAtlasData = this.ParseTextureAtlasData((Dictionary<string, object>)MiniJSON.Json.Deserialize(textureAtlasJSON.text), null, name, scale) as UnityTextureAtlasData;
            var path = _textureAtlasPath;

            var index = path.LastIndexOf("Resources");
            if (index > 0)
            {
                path = path.Substring(index + 10);
            }

            index = path.LastIndexOf("/");
            if (index > 0)
            {
                textureAtlasData.imagePath = path.Substring(0, index + 1) + textureAtlasData.imagePath;
            }

            index = textureAtlasData.imagePath.LastIndexOf(".");
            if (index > 0)
            {
                textureAtlasData.imagePath = textureAtlasData.imagePath.Substring(0, index);
            }
            
            var textureAtlas = Resources.Load<Texture2D>(textureAtlasData.imagePath);
            var shader = Shader.Find(defaultShaderName);
            var material = new Material(shader);
            material.mainTexture = textureAtlas;

            textureAtlasData.texture = material;

            return textureAtlasData;
        }

        /**
         * @inheritDoc
         */
        public override void RemoveDragonBonesData(string name, bool disposeData = true)
        {
            var dragonBonesData = this.GetDragonBonesData(name);
            if (_pathDragonBonesDataMap.ContainsValue(dragonBonesData))
            {
                foreach (var pair in _pathDragonBonesDataMap)
                {
                    if (pair.Value == dragonBonesData)
                    {
                        _pathDragonBonesDataMap.Remove(pair.Key);
                        break;
                    }
                }
            }

            base.RemoveDragonBonesData(name, disposeData);
        }

        /**
         * @inheritDoc
         */
        public override void RemoveTextureAtlasData(string name, bool disposeData = true)
        {
            var textureAtlasDataList = this.GetTextureAtlasData(name);
            if (textureAtlasDataList != null)
            {
                foreach (var textureAtlasData in textureAtlasDataList)
                {
                    if (_pathTextureAtlasDataMap.ContainsValue(textureAtlasData))
                    {
                        foreach (var pair in _pathTextureAtlasDataMap)
                        {
                            if (pair.Value == textureAtlasData)
                            {
                                _pathTextureAtlasDataMap.Remove(pair.Key);
                                break;
                            }
                        }
                    }
                }
            }

            base.RemoveTextureAtlasData(name, disposeData);
        }

        /**
         * @inheritDoc
         */
        public override void Clear(bool disposeData = true)
        {
            base.Clear(disposeData);

            _pathDragonBonesDataMap.Clear();
            _pathTextureAtlasDataMap.Clear();
        }

        /**
         * @private
         */
        public void RefreshAllTextureAtlas()
        {
            foreach (var textureAtlasDatas in this._textureAtlasDataMap.Values)
            {
                foreach (UnityTextureAtlasData textureAtlasData in textureAtlasDatas)
                {
                    if (textureAtlasData.texture == null)
                    {
                        var textureAtlas = Resources.Load<Texture2D>(textureAtlasData.imagePath);
                        var shader = Shader.Find(defaultShaderName);
                        var material = new Material(shader);
                        material.mainTexture = textureAtlas;

                        textureAtlasData.texture = material;
                    }
                }
            }
        }

        /**
         * @language zh_CN
         * 创建一个指定名称的骨架，并使用骨架的显示容器来更新骨架动画。
         * @param armatureName 骨架数据名称。
         * @param dragonBonesName 龙骨数据名称，如果未设置，将检索所有的龙骨数据，如果多个数据中包含同名的骨架数据，可能无法创建出准确的骨架。
         * @param skinName 皮肤名称，如果未设置，则使用默认皮肤。
         * @returns 骨架的显示容器。
         * @see dragonBones.UnityArmatureComponent
         * @version DragonBones 4.5
         */
        public UnityArmatureComponent BuildArmatureComponent(string armatureName, string dragonBonesName = null, string skinName = null, GameObject gameObject = null)
        {
            _armatureGameObject = gameObject;
            var armature = this.BuildArmature(armatureName, dragonBonesName, skinName);
            if (armature != null)
            {
                var armatureDisplay = armature.display as GameObject;
                var armatureComponent = armatureDisplay.GetComponent<UnityArmatureComponent>();
                clock.Add(armature);

                return armatureComponent;
            }

            return null;
        }

        /**
         * @language zh_CN
         * 获取带有指定贴图的显示对象。
         * @param textureName 指定的贴图名称。
         * @param textureAtlasName 指定的龙骨数据名称，如果未设置，将检索所有的龙骨数据。
         * @version DragonBones 3.0
         */
        public GameObject GetTextureDisplay(string textureName, string textureAtlasName = null)
        {
            /*var textureData = this._getTextureData(textureAtlasName, textureName) as UnityTextureData;
            if (textureData != null)
            {
                if (textureData.texture == null)
                {
                    var textureAtlasTexture = (textureData.parent as UnityTextureAtlasData).texture;

                    var rect = new Rect(
                        textureData.region.x,
                        textureAtlasTexture.height - textureData.region.y - textureData.region.height,
                        textureData.region.width,
                        textureData.region.height
                    );

                    textureData.texture = Sprite.Create(textureAtlasTexture, rect, new Vector2(), 1.0f);
                }

                var gameObject = new GameObject();
                gameObject.AddComponent<SpriteRenderer>().sprite = textureData.texture;
                return gameObject;
            }*/

            return null;
        }

        /**
         * @language zh_CN
         * 获取全局声音事件管理器。
         * @version DragonBones 4.5
         */
        public IEventDispatcher<EventObject> soundEventManater
        {
            get { return _eventManager; }
        }
    }
}