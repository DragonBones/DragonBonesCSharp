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
using UnityEngine;

namespace DragonBones
{
    [DisallowMultipleComponent]
    public class UnityBone : MonoBehaviour
    {
        private static Vector3 _helpVector3 = new Vector3();
        internal UnityArmatureComponent _proxy;
        internal Bone _bone;
        public Bone bone
        {
            get { return _bone; }
        }

        [SerializeField]
        private GameObject _parent = null;

        /**
         * 获取父骨骼
         * 
         */
        public GameObject GetParentGameObject()
        {
            if (_parent)
            {
                return _parent;
            }

            if (_bone != null && _bone.parent != null)
            {
                for (int i = 0; i < transform.parent.childCount; ++i)
                {
                    UnityEngine.Transform child = transform.parent.GetChild(i);
                    if (child.name.Equals(_bone.parent.name))
                    {
                        _parent = child.gameObject;
                        break;
                    }
                }

                if (_proxy.boneHierarchy && _parent)
                {
                    transform.SetParent(_parent.transform);
                }
            }

            return _parent;
        }

        internal void _Update()
        {
            if (_bone != null && _proxy != null && _proxy.armature != null)
            {
                GameObject parent = null;
                if (_proxy.boneHierarchy)
                {
                    parent = GetParentGameObject();
                    if (parent)
                    {
                        transform.SetParent(_proxy.bonesRoot.transform);
                    }

                }
                else if (transform.parent != _proxy.bonesRoot)
                {
                    transform.SetParent(_proxy.bonesRoot.transform);
                }

                _bone.UpdateGlobalTransform();

                Armature armature = _proxy.armature;

                var flipX = armature.flipX;
                var flipY = armature.flipY;

                // localPosition
                _helpVector3.x = _bone.globalTransformMatrix.tx;
                _helpVector3.y = _bone.globalTransformMatrix.ty;
                _helpVector3.z = 0f;
                transform.localPosition = _helpVector3;

                // localEulerAngles
                _helpVector3.x = flipY ? 180.0f : 0.0f;
                _helpVector3.y = flipX ? 180.0f : 0.0f;
                _helpVector3.z = _bone.global.rotation * Transform.RAD_DEG;

                if (flipX || flipY)
                {
                    if (flipX && flipY)
                    {
                        _helpVector3.z += 180.0f;
                    }
                    else
                    {
                        if (flipX)
                        {
                            _helpVector3.z = 180.0f - _helpVector3.z;
                        }
                        else
                        {
                            _helpVector3.z = -_helpVector3.z;
                        }
                    }
                }

                transform.localEulerAngles = _helpVector3;

                // localScale
                _helpVector3.x = _bone.global.scaleX;
                _helpVector3.y = _bone.global.scaleY;
                _helpVector3.z = 1.0f;

                transform.localScale = _helpVector3;

                if (_proxy.boneHierarchy && parent)
                {
                    transform.SetParent(parent.transform);
                }
            }
        }
    }
}