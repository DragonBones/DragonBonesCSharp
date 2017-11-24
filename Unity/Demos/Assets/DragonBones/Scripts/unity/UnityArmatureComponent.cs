using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
    /// <summary>
    /// The slots sorting mode
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    /// 
    /// <summary>
    /// 插槽排序模式
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>zh_CN</language>
    public enum SortingMode
    {
        /// <summary>
        /// Sort by Z values
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 按照插槽显示对象的z值排序
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        SortByZ,
        /// <summary>
        /// Renderer's order within a sorting layer.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 在同一层sorting layer中插槽按照sortingOrder排序
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        SortByOrder,
        /// <summary>
        /// Adding a SortingGroup component to a GameObject will ensure that all Renderers within the GameObject's descendants will be sorted and rendered together.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 使用Sorting Group组件，可以在同一个Sorting Layer内将一组对象和其他对象分开来渲染。它会确保同一组下渲染的所有子对象一起被排序，这对管理复杂的场景非常有用。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        SortByGroup,
    }
    
    ///<inheritDoc/>
    [ExecuteInEditMode, DisallowMultipleComponent]
    public class UnityArmatureComponent : DragonBoneEventDispatcher, IArmatureProxy
    {
        ///<private/>
        private bool _disposeProxy = true;        
        ///<private/>
        internal Armature _armature = null;
        ///<private/>
        public UnityDragonBonesData unityData = null;
        ///<private/>
        public string armatureName = null;
        /// <summary>
        /// Is it the UGUI model?
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 是否是UGUI模式
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public bool isUGUI = false;
        ///<private/>
        public bool addNormal = false;
        public GameObject bonesRoot;
        public List<UnityBone> unityBones = null;
        /// <summary>
        /// Display bones hierarchy
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 显示骨骼层级
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public bool boneHierarchy = false;

        internal readonly ColorTransform _colorTransform = new ColorTransform();

        ///<private/>
        public string animationName = null;
        [Tooltip("0 : Loop")]
        [Range(0, 100)]
        [SerializeField]
        protected int _playTimes = 0;
        [Range(-2f, 2f)]
        [SerializeField]
        protected float _timeScale = 1.0f;

        [SerializeField]
        protected SortingMode _sortingMode = SortingMode.SortByZ;
        [SerializeField]
        protected string _sortingLayerName = "Default";
        [SerializeField]
        protected int _sortingOrder = 0;
        [SerializeField]
        protected float _zSpace = 0.0f;
        
        [SerializeField]
        protected bool _flipX = false;
        [SerializeField]
        protected bool _flipY = false;

        private List<Slot> _sortedSlots = null;
        ///<private/>
        public void DBClear()
        {
            bonesRoot = null;
            if (_armature != null)
            {
                _armature = null;
                if (_disposeProxy)
                {
                    UnityFactoryHelper.DestroyUnityObject(gameObject);
                }
            }

            _disposeProxy = true;
            _armature = null;
            unityData = null;
            armatureName = null;
            animationName = null;
            isUGUI = false;
            addNormal = false;
            unityBones = null;
            boneHierarchy = false;

            _colorTransform.Identity();
            _sortingMode = SortingMode.SortByZ;
            _sortingLayerName = "Default";
            _sortingOrder = 0;
            _playTimes = 0;
            _timeScale = 1.0f;
            _zSpace = 0.0f;
            //zorderIsDirty = false;
            _flipX = false;
            _flipY = false;
            _sortedSlots = null;
        }
        ///
        public void DBInit(Armature armature)
        {
            this._armature = armature;
        }

        public void DBUpdate()
        {

        }

        ///<inheritDoc/>
        public void Dispose(bool disposeProxy = true)
        {
            _disposeProxy = disposeProxy;

            if (_armature != null)
            {
                _armature.Dispose();
            }
        }
        /// <summary>
        /// Get the Armature.
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// 获取骨架。
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public Armature armature
        {
            get { return _armature; }
        }

        /// <summary>
        /// Get the animation player
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// 获取动画播放器。
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public new Animation animation
        {
            get { return _armature != null ? _armature.animation : null; }
        }

        /// <summary>
        /// The slots sorting mode
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 插槽排序模式
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public SortingMode sortingMode
        {
            get { return _sortingMode; }
            set
            {
                if (_sortingMode == value)
                {
                    return;
                }

                _sortingMode = value;

                if (!isUGUI && _armature != null)
                {
                    foreach (UnitySlot slot in _armature.GetSlots())
                    {
                        if (slot.childArmature == null)
                        {
                            if (slot.meshRenderer != null)
                            {
                                if (sortingMode == SortingMode.SortByOrder)
                                {
                                    slot.meshRenderer.sortingOrder = slot._zOrder;
                                }
                                else
                                {
                                    slot.meshRenderer.sortingOrder = sortingOrder;
                                }
                            }
                        }
                        else
                        {
                            (slot.childArmature.proxy as UnityArmatureComponent).sortingMode = _sortingMode;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Name of the Renderer's sorting layer.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// sorting layer名称。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
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
				if(!isUGUI)
                {
				#if UNITY_5_6_OR_NEWER
					if(_sortingGroup)
                    {
						_sortingGroup.sortingLayerName = value;
					#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(_sortingGroup);
						}
					#endif
						return;
					}
				#endif

					foreach (var render in GetComponentsInChildren<Renderer>(true))
					{
						render.sortingLayerName = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(render);
						}
						#endif
					}
				}
            }
        }

        /// <summary>
        /// Renderer's order within a sorting layer.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 插槽按照sortingOrder在同一层sorting layer中排序
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
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
				if(!isUGUI)
                {
					#if UNITY_5_6_OR_NEWER
					if(_sortingGroup)
                    {
						_sortingGroup.sortingOrder = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(_sortingGroup);
						}
						#endif
						return;
					}
					#endif
					foreach (var render in GetComponentsInChildren<Renderer>(true))
					{
						render.sortingOrder = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(render);
						}
						#endif
					}
				}
            }
        }
        /// <summary>
        /// The Z axis spacing of slot display objects
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        /// 
        /// <summary>
        /// 插槽显示对象的z轴间隔
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
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
                        display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (zSpace + 0.001f));
						if(!isUGUI)
                        {
							UnitySlot us = slot as UnitySlot;
							if(us.meshRenderer != null)
                            {
								if(sortingMode == SortingMode.SortByOrder)
                                {
									us.meshRenderer.sortingOrder = slot._zOrder;
								}
                                else
                                {
									us.meshRenderer.sortingOrder = sortingOrder;
								}
							}
						}
					}
                }
            }
        }
        /// <summary>
        /// Playing repeat times. [-1: Use default value of the animation data, 0: No end loop playing, [1~N]: Repeat N times]
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 循环播放次数。 [-1: 使用动画数据默认值, 0: 无限循环播放, [1~N]: 循环播放 N 次]
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public int playTimes
        {
            set
            {
                _playTimes = value;
            }

            get
            {
                return _playTimes;
            }
        }
        /// <summary>
        /// The play speed of current animations.   [0: Stop, (0~1): Slow, 1: Normal, (1~N): Fast]
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 当前动画的播放速度。  [0: 停止播放, (0~1): 慢速播放, 1: 正常播放, (1~N): 快速播放]
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
		public float timeScale
        {
			set
            { 
				_timeScale = value;
				if(_armature != null)
                {
					_armature.animation.timeScale = _timeScale;
				}
			}
			get
            {
				if(_armature != null)
                {
					_timeScale = _armature.animation.timeScale;
				}

				return _timeScale;
			}
		}
	
		
		public List<Slot> sortedSlots
        {
			get
            {
				if(_sortedSlots == null)
                {
					_sortedSlots = new List<Slot>(_armature.GetSlots());
				}

				return _sortedSlots;
			}
		}

		#if UNITY_5_6_OR_NEWER
		internal UnityEngine.Rendering.SortingGroup _sortingGroup;
		public UnityEngine.Rendering.SortingGroup sortingGroup
        {
			get { return _sortingGroup; }
		}
#endif
        /// <summary>
        /// Whether to flip the armature horizontally.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 是否将骨架水平翻转。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public bool flipX
        {
            get { return _flipX; }
            set
            {
                if (_flipX == value)
                {
                    return;
                }

                _flipX = value;
                armature.flipX = _flipX;
            }
        }
        /// <summary>
        /// Whether to flip the armature vertically.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        ///是否将骨架垂直翻转。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public bool flipY
        {
            get { return _flipY; }
            set
            {
                if (_flipY == value)
                {
                    return;
                }

                _flipY = value;
                armature.flipY = _flipY;
            }
        }

        /// <summary>
        /// The armature color.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 骨架的颜色。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public ColorTransform color
        {
            get { return this._colorTransform; }
            set
            {
                this._colorTransform.CopyFrom(value);

                foreach (var slot in this._armature.GetSlots())
                {
                    slot._colorDirty = true;
                }
            }
        }

#if UNITY_EDITOR
        private bool _isPrefab()
        {
			return PrefabUtility.GetPrefabParent(gameObject) == null 
				&& PrefabUtility.GetPrefabObject(gameObject) != null;
		}
		#endif

		///<private/>
        void Awake()
		{
            #if UNITY_EDITOR
            if (_isPrefab())
            {
                return;
            }
            #endif

            #if UNITY_5_6_OR_NEWER
            if (!isUGUI)
            {
				_sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
				if(_sortingGroup)
                {
					sortingMode = SortingMode.SortByOrder;
					_sortingLayerName = _sortingGroup.sortingLayerName;
					_sortingOrder = _sortingGroup.sortingOrder;
				}
			}
			#endif

			//zorderIsDirty = true;
            if (unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null)
            {
				var dragonBonesData = UnityFactory.factory.LoadData(unityData, isUGUI);
				if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
				{
					UnityFactory.factory.BuildArmatureComponent(armatureName, unityData.dataName, null, null, gameObject , isUGUI);
				}
			}
            
            if (_armature != null)
            {
                sortingLayerName = sortingLayerName;
                sortingOrder = sortingOrder;
				_armature.flipX = flipX;
				_armature.flipY = flipY;
				_armature.animation.timeScale = _timeScale;

                if (zSpace > 0 || sortingMode == SortingMode.SortByOrder)
                {
                    foreach (var slot in _armature.GetSlots())
                    {
                        var display = slot.display as GameObject;
                        if (display != null)
                        {
                            display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (zSpace + 0.001f));
                            if (!isUGUI && sortingMode == SortingMode.SortByOrder)
                            {
                                UnitySlot us = slot as UnitySlot;
                                if (us.meshRenderer != null)
                                {
                                    us.meshRenderer.sortingOrder = slot._zOrder;
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(animationName))
                {
                    _armature.animation.Play(animationName, _playTimes);
                }

				CollectBones();
            }
        }

		void LateUpdate()
        {
            if (_armature == null)
            {
                return;
            }

			_flipX = _armature.flipX ;
			_flipY = _armature.flipY ;

			#if UNITY_EDITOR
			if(!Application.isPlaying)
            {
				#if UNITY_5_6_OR_NEWER
				if(!isUGUI)
                {
					_sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
					if(_sortingGroup)
                    {
						sortingMode = SortingMode.SortByOrder;
						_sortingLayerName = _sortingGroup.sortingLayerName;
						_sortingOrder = _sortingGroup.sortingOrder;
					}
				}
				#endif
				foreach (var slot in _armature.GetSlots())
				{
					var display = slot.display as GameObject;
					if (display != null)
					{
                        display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (zSpace + 0.001f));
						if(!isUGUI)
                        {
							UnitySlot us = slot as UnitySlot;
							if(us.meshRenderer!=null)
                            {
								us.meshRenderer.sortingLayerName = _sortingLayerName;
								if(sortingMode == SortingMode.SortByOrder)
                                {
									us.meshRenderer.sortingOrder = slot._zOrder;
								}
                                else
                                {
									us.meshRenderer.sortingOrder = _sortingOrder;
								}
							}
						}
					}
				}
			}
#endif

            //child armatrue
            if (armature.parent != null && armature.parent.armature != null)
            {
                UnityArmatureComponent parentArmature = armature.parent.armature.proxy as UnityArmatureComponent;
                if (parentArmature != null)
                {
                    sortingMode = parentArmature.sortingMode;
                    _sortingOrder = parentArmature.sortingOrder;
                    _sortingLayerName = parentArmature.sortingLayerName;
#if UNITY_5_6_OR_NEWER
                    if (parentArmature.sortingGroup)
                    {
                        if (_sortingGroup == null)
                        {
                            _sortingGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                        }

                        _sortingGroup.sortingLayerName = parentArmature.sortingGroup.sortingLayerName;
                        _sortingGroup.sortingOrder = armature.parent._zOrder;
                        _sortingOrder = _sortingGroup.sortingOrder;
                        _sortingLayerName = _sortingGroup.sortingLayerName;
                    }
                    else if (_sortingGroup)
                    {
                        DestroyImmediate(_sortingGroup);
                    }
#endif
                }
            } 

            if (unityBones!=null)
            {
				int len = unityBones.Count;
				for(int i=0;i<len;++i)
                {
					UnityBone bone = unityBones[i];
                    if (bone != null)
                    {
                        bone._Update();
                    }
				}
			}
		}

        ///<private/>
        void OnDestroy()
        {
            if (_armature != null)
            {
                var armature = _armature;
                _armature = null;

                armature.Dispose();

#if UNITY_EDITOR
                UnityFactory.factory._dragonBones.AdvanceTime(0.0f);
#endif
            }

            unityBones = null;
			_sortedSlots = null;
            _disposeProxy = true;
            _armature = null;
        }



        public void CollectBones()
        {
			if(unityBones != null )
			{
				foreach(UnityBone unityBone in unityBones)
                {
					foreach(Bone bone in armature.GetBones())
                    {
						if(unityBone.name.Equals(bone.name))
                        {
							unityBone._bone = bone;
							unityBone._proxy=this;
						}
					}
				}
			}
		}
		public void ShowBones()
        {
			RemoveBones();
			if(bonesRoot == null)
            {
				GameObject go = new GameObject("Bones");
				go.transform.SetParent(transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				bonesRoot = go;
				go.hideFlags = HideFlags.NotEditable;
			}

			if(armature != null)
			{
				unityBones = new List<UnityBone>();
				foreach(Bone bone in armature.GetBones())
                {
					GameObject go = new GameObject(bone.name);
					UnityBone ub = go.AddComponent<UnityBone> ();
					ub._bone = bone;
					ub._proxy = this;
					unityBones.Add(ub);

					go.transform.SetParent(bonesRoot.transform);
				}

				foreach(UnityBone bone in unityBones)
                {
					bone.GetParentGameObject();
				}
                
                foreach (UnityArmatureComponent child in GetComponentsInChildren<UnityArmatureComponent>(true))
                {
                    if (child != this)
                    {
                        child.ShowBones();
                    }
				}
			}
		}
		public void RemoveBones()
        {
            foreach (UnityArmatureComponent child in GetComponentsInChildren<UnityArmatureComponent>(true))
            {
                if (child != this)
                {
                    child.RemoveBones();
                }
			}

			if(unityBones!=null)
            {
				unityBones = null;
			}

			if(bonesRoot)
            {
				DestroyImmediate(bonesRoot);
			}
		}
    }
}