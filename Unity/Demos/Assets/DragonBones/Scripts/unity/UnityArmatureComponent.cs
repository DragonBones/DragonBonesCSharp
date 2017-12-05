/**
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2017 DragonBones team and other contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
﻿using System.Collections.Generic;
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
        //SortByGroup,
    }

    ///<inheritDoc/>
    [ExecuteInEditMode, DisallowMultipleComponent]
    public class UnityArmatureComponent : DragonBoneEventDispatcher, IArmatureProxy
    {
        public const int ORDER_SPACE = 10;
        /// <private/>
        private bool _disposeProxy = true;
        /// <private/>
        internal Armature _armature = null;
        /// <private/>
        public UnityDragonBonesData unityData = null;
        /// <private/>
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

        /// <private/>
        public string animationName = null;
        [Tooltip("0 : Loop")]
        [Range(0, 100)]
        [SerializeField]
        protected int _playTimes = 0;
        [Range(-2f, 2f)]
        [SerializeField]
        protected float _timeScale = 1.0f;

        [SerializeField]
        internal SortingMode _sortingMode = SortingMode.SortByZ;
        [SerializeField]
        internal string _sortingLayerName = "Default";
        [SerializeField]
        internal int _sortingOrder = 0;
        [SerializeField]
        internal float _zSpace = 0.0f;

        [SerializeField]
        protected bool _flipX = false;
        [SerializeField]
        protected bool _flipY = false;

        private bool _hasSortingGroup = false;
        /// <private/>
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
            _flipX = false;
            _flipY = false;

            _hasSortingGroup = false;
        }
        ///
        public void DBInit(Armature armature)
        {
            this._armature = armature;
        }

        public void DBUpdate()
        {

        }

        /// <inheritDoc/>
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

                #if UNITY_5_6_OR_NEWER
                var isWarning = false;
                #else
                var isWarning = value == SortingMode.SortByOrder;
                #endif

                if(isWarning)
                {
                    LogHelper.LogWarning("SortingMode.SortByOrder is userd by Unity 5.6 or highter only.");
                    return;
                }

                _sortingMode = value;

                //
                #if UNITY_5_6_OR_NEWER
                if(_sortingMode == SortingMode.SortByOrder)
                {
                    _sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
                    if(_sortingGroup == null)
                    {
                        _sortingGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                    }
                }
                else
                {
                    _sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();

                    if(_sortingGroup != null)
                    {
                        DestroyImmediate(_sortingGroup);
                    }
                }
                #endif

                _UpdateSlotsSorting();
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

                _UpdateSlotsSorting();
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

                _UpdateSlotsSorting();
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

                _UpdateSlotsSorting();
            }
        }        
        /// <summary>
        /// - The armature color.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// - 骨架的颜色。
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
        

#if UNITY_5_6_OR_NEWER
        internal UnityEngine.Rendering.SortingGroup _sortingGroup;
        public UnityEngine.Rendering.SortingGroup sortingGroup
        {
            get { return _sortingGroup; }
        }

        private void _UpdateSortingGroup()
        {
            //发现骨架有SortingGroup，那么子骨架也都加上，反之删除
            _sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (_sortingGroup != null)
            {
                _sortingMode = SortingMode.SortByOrder;
                _sortingLayerName = _sortingGroup.sortingLayerName;
                _sortingOrder = _sortingGroup.sortingOrder;

                foreach (UnitySlot slot in _armature.GetSlots())
                {
                    if (slot.childArmature != null)
                    {
                        var childArmatureProxy = slot.childArmature.proxy as UnityArmatureComponent;
                        childArmatureProxy._sortingGroup = childArmatureProxy.GetComponent<UnityEngine.Rendering.SortingGroup>();
                        if (childArmatureProxy._sortingGroup == null)
                        {
                            childArmatureProxy._sortingGroup = childArmatureProxy.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                        }

                        childArmatureProxy._sortingGroup.sortingLayerName = _sortingLayerName;
                        childArmatureProxy._sortingGroup.sortingOrder = _sortingOrder;
                    }
                }
            }
            else
            {
                _sortingMode = SortingMode.SortByZ;
                foreach (UnitySlot slot in _armature.GetSlots())
                {
                    if (slot.childArmature != null)
                    {
                        var childArmatureProxy = slot.childArmature.proxy as UnityArmatureComponent;
                        childArmatureProxy._sortingGroup = childArmatureProxy.GetComponent<UnityEngine.Rendering.SortingGroup>();
                        if (childArmatureProxy._sortingGroup != null)
                        {
                            DestroyImmediate(childArmatureProxy._sortingGroup);
                        }
                    }
                }
            }

            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif

            _UpdateSlotsSorting();
        }
#endif
        private void _UpdateSlotsSorting()
        {
            if (_armature == null)
            {
                return;
            }

            if (!isUGUI)
            {
#if UNITY_5_6_OR_NEWER
                if (_sortingGroup)
                {
                    _sortingMode = SortingMode.SortByOrder;
                    _sortingGroup.sortingLayerName = _sortingLayerName;
                    _sortingGroup.sortingOrder = _sortingOrder;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(_sortingGroup);
                    }
#endif
                }
#endif
            }

            //
            foreach (UnitySlot slot in _armature.GetSlots())
            {
                var display = slot.display as GameObject;
                if (display == null)
                {
                    continue;
                }

                slot._SetZorder(new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (_zSpace + 0.001f)));

                if(slot.childArmature != null)
                {
                    (slot.childArmature.proxy as UnityArmatureComponent)._UpdateSlotsSorting();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && slot.meshRenderer != null)
                {
                    EditorUtility.SetDirty(slot.meshRenderer);
                }
#endif
            }
        }

#if UNITY_EDITOR
        private bool _IsPrefab()
        {
            return PrefabUtility.GetPrefabParent(gameObject) == null
                && PrefabUtility.GetPrefabObject(gameObject) != null;
        }
#endif

        /// <private/>
        void Awake()
        {
#if UNITY_EDITOR
            if (_IsPrefab())
            {
                return;
            }
#endif
            if (unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null)
            {
                var dragonBonesData = UnityFactory.factory.LoadData(unityData, isUGUI);
                if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
                {
                    UnityFactory.factory.BuildArmatureComponent(armatureName, unityData.dataName, null, null, gameObject, isUGUI);
                }
            }

            if (_armature != null)
            {
#if UNITY_5_6_OR_NEWER
                if (!isUGUI)
                {
                    _sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
                }
#endif
                _UpdateSlotsSorting();

                _armature.flipX = _flipX;
                _armature.flipY = _flipY;

                _armature.animation.timeScale = _timeScale;

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

            _flipX = _armature.flipX;
            _flipY = _armature.flipY;

            #if UNITY_5_6_OR_NEWER
            var hasSortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>() != null;
            if (hasSortingGroup != _hasSortingGroup)
            {
                _hasSortingGroup = hasSortingGroup;

                _UpdateSortingGroup();
            }
            #endif

            if (unityBones != null)
            {
                int len = unityBones.Count;
                for (int i = 0; i < len; ++i)
                {
                    UnityBone bone = unityBones[i];
                    if (bone != null)
                    {
                        bone._Update();
                    }
                }
            }
        }

        /// <private/>
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
            _disposeProxy = true;
            _armature = null;
        }



        public void CollectBones()
        {
            if (unityBones != null)
            {
                foreach (UnityBone unityBone in unityBones)
                {
                    foreach (Bone bone in armature.GetBones())
                    {
                        if (unityBone.name.Equals(bone.name))
                        {
                            unityBone._bone = bone;
                            unityBone._proxy = this;
                        }
                    }
                }
            }
        }
        public void ShowBones()
        {
            RemoveBones();
            if (bonesRoot == null)
            {
                GameObject go = new GameObject("Bones");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                bonesRoot = go;
                go.hideFlags = HideFlags.NotEditable;
            }

            if (armature != null)
            {
                unityBones = new List<UnityBone>();
                foreach (Bone bone in armature.GetBones())
                {
                    GameObject go = new GameObject(bone.name);
                    UnityBone ub = go.AddComponent<UnityBone>();
                    ub._bone = bone;
                    ub._proxy = this;
                    unityBones.Add(ub);

                    go.transform.SetParent(bonesRoot.transform);
                }

                foreach (UnityBone bone in unityBones)
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

            if (unityBones != null)
            {
                unityBones = null;
            }

            if (bonesRoot)
            {
                DestroyImmediate(bonesRoot);
            }
        }
    }
}