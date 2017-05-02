using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
    /**
     * @inheritDoc
     */
	[ExecuteInEditMode,DisallowMultipleComponent]
    public class UnityArmatureComponent : UnityEventDispatcher<EventObject>, IArmatureProxy
    {
        private bool _disposeProxy = true;
        /**
         * @private
         */
        internal Armature _armature = null;
        /**
         * @private
         */
        public void _onClear()
        {
            if (_armature != null)
            {
                _armature = null;
                if (_disposeProxy)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(gameObject);
#else
                    Object.Destroy(gameObject);
#endif
                }
            }
        }
        /**
         * @inheritDoc
         */
        public void Dispose(bool disposeProxy = true)
        {
            _disposeProxy = disposeProxy;

            if (_armature != null)
            {
                _armature.Dispose();
            }
        }
        /**
         * @language zh_CN
         * 获取骨架。
         * @readOnly
         * @see DragonBones.Armature
         * @version DragonBones 4.5
         */
        public Armature armature
        {
            get { return _armature; }
        }
        /**
         * @language zh_CN
         * 获取动画控制器。
         * @readOnly
         * @see DragonBones.Animation
         * @version DragonBones 4.5
         */
        new public Animation animation
        {
            get { return _armature != null ? _armature.animation : null; }
        }
        
        /**
         * @private
         */
        public TextAsset dragonBonesJSON = null;
        /**
         * @private
         */
        public List<string> textureAtlasJSON = null;
        /**
         * @private
         */
        public string armatureName = null;
        /**
         * @private
         */
        public string animationName = null;

        [SerializeField]
        protected string _sortingLayerName = "Default";
        public string sortingLayerName
        {
            get { return _sortingLayerName; }
            set
            {
                if (_sortingLayerName == value)
                {
                    //return;
                }
                _sortingLayerName = value;
				if(!isUGUI){
					foreach (var render in GetComponentsInChildren<Renderer>(true))
					{
						render.sortingLayerName = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying){
							EditorUtility.SetDirty(render);
						}
						#endif
					}
				}
            }
        }

        [SerializeField]
        protected int _sortingOrder = 0;
        public int sortingOrder
        {
            get { return _sortingOrder; }
            set
            {
                if (_sortingOrder == value)
                {
                    //return;
                }
                _sortingOrder = value;
				if(!isUGUI){
                	foreach (var render in GetComponentsInChildren<Renderer>(true))
	                {
	                    render.sortingOrder = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying){
							EditorUtility.SetDirty(render);
						}
						#endif
	                }
				}
            }
        }

        [SerializeField]
        protected float _zSpace = 0.0f;
        public float zSpace
        {
            get { return _zSpace; }
            set
            {
                if (value < 0.0f || float.IsNaN(value))
                {
                    value = 0.0f;
                }

                if (_zSpace == value)
                {
                    return;
                }

                _zSpace = value;

                foreach (var slot in _armature.GetSlots())
                {
                    var display = slot.display as GameObject;
                    if (display != null)
                    {
                        display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (_zSpace + 0.001f));
                    }
                }
            }
        }
	
		public bool isUGUI = false;
		public bool zorderIsDirty = false;
		public bool flipX = false;
		public bool flipY = false;
		public bool addNormal = false;
		public GameObject slotsRoot;
		public GameObject bonesRoot;
		public List<UnityBone> unityBones = null;

		private List<Slot> _sortedSlots = null;
		public List<Slot> sortedSlots{
			get{
				if(_sortedSlots==null){
					_sortedSlots = new List<Slot>(_armature.GetSlots());
				}
				return _sortedSlots;
			}
		}

        /**
         * @private
         */
        void Awake()
        {
			if(slotsRoot==null){
				GameObject go = new GameObject("Slots");
				go.transform.SetParent(transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				slotsRoot = go;
				go.hideFlags = HideFlags.NotEditable;
			}
			zorderIsDirty = true;
            var dragonBonesData = LoadData(dragonBonesJSON, textureAtlasJSON);

			if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
            {
				UnityFactory.factory.BuildArmatureComponent(armatureName, dragonBonesData.name, null, null, gameObject);
            }
            if (_armature != null)
            {
                sortingLayerName = sortingLayerName;
                sortingOrder = sortingOrder;
				_armature.flipX = flipX;
				_armature.flipY = flipY;
				if(zSpace>0){
					foreach (var slot in _armature.GetSlots())
					{
						var display = slot.display as GameObject;
						if (display != null)
						{
							display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (_zSpace + 0.001f));
						}
					}
				}
                if (!string.IsNullOrEmpty(animationName))
                {
                    _armature.animation.Play(animationName);
                }
				CollectBones();
            }
        }

		void LateUpdate(){
			if(_armature==null) return;

			flipX = _armature.flipX ;
			flipY = _armature.flipY ;

			#if UNITY_EDITOR
			if(!Application.isPlaying && zSpace>0f){
				foreach (var slot in _armature.GetSlots())
				{
					var display = slot.display as GameObject;
					if (display != null)
					{
						display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (_zSpace + 0.001f));
					}
				}
			}
			#endif
			if(zorderIsDirty){
				_sortedSlots = new List<Slot>(_armature.GetSlots());
				_sortedSlots.Sort(delegate(Slot x, Slot y) {
					return x._zOrder-y._zOrder;
				});
				for (int i=0 ; i<_sortedSlots.Count ;++i )
				{
					Slot slot = _sortedSlots[i];
					var display = slot.display as GameObject;
					if (display != null)
					{
						display.transform.SetSiblingIndex(i);
					}
				}
				zorderIsDirty = false;
			}
			if(unityBones!=null){
				int len = unityBones.Count;
				for(int i=0;i<len;++i){
					UnityBone bone = unityBones[i];
					if(bone) bone._Update();
				}
			}
		}

        /**
         * @private
         */
        void OnDestroy()
        {
            if (_armature != null)
            {
                var armature = _armature;
                _armature = null;
                armature.Dispose();
            }
			unityBones = null;
			_sortedSlots = null;
            _disposeProxy = true;
            _armature = null;
        }
        /**
         * @private
         */
        public DragonBonesData LoadData(TextAsset dragonBonesJSON, List<string> textureAtlasJSON)
        {
            DragonBonesData dragonBonesData = null;

            if (dragonBonesJSON != null)
            {
                dragonBonesData = UnityFactory.factory.LoadDragonBonesData(dragonBonesJSON);

                if (dragonBonesData != null && textureAtlasJSON != null)
                {
                    foreach (var eachJSON in textureAtlasJSON)
                    {
						UnityFactory.factory.LoadTextureAtlasData(eachJSON,null,0,isUGUI);
                    }
                }
            }

            return dragonBonesData;
        }


		public void CollectBones(){
			if(unityBones!=null )
			{
				foreach(UnityBone unityBone in unityBones){
					foreach(Bone bone in armature.GetBones()){
						if(unityBone.name.Equals(bone.name)){
							unityBone._bone = bone;
							unityBone._proxy=this;
						}
					}
				}
			}
		}
		public void ShowBones(){
			RemoveBones();
			if(bonesRoot==null){
				GameObject go = new GameObject("Bones");
				go.transform.SetParent(transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				bonesRoot = go;
				go.hideFlags = HideFlags.NotEditable;
			}
			if(armature!=null)
			{
				unityBones = new List<UnityBone>();
				foreach(Bone bone in armature.GetBones()){
					GameObject go = new GameObject(bone.name);
					UnityBone ub = go.AddComponent<UnityBone> ();
					ub._bone = bone;
					ub._proxy = this;
					unityBones.Add(ub);

					go.transform.SetParent(bonesRoot.transform);
				}
				foreach(UnityBone bone in unityBones){
					bone.GetParentGameObject();
				}
				foreach (UnityArmatureComponent child in slotsRoot.GetComponentsInChildren<UnityArmatureComponent>(true))
				{
					child.ShowBones();
				}
			}
		}
		public void RemoveBones(){
			foreach (UnityArmatureComponent child in slotsRoot.GetComponentsInChildren<UnityArmatureComponent>(true))
			{
				child.RemoveBones();
			}
			if(unityBones!=null){
				unityBones = null;
			}
			if(bonesRoot){
				DestroyImmediate(bonesRoot);
			}
		}
    }
}