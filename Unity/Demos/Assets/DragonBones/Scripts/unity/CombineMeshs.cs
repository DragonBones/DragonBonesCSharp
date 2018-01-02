using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

namespace DragonBones
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityArmatureComponent))]
    public class CombineMeshs : MonoBehaviour
    {
        public MeshBuffer[] meshBuffers;
        public bool _dirty = false;
        private UnityArmatureComponent _unityArmature;
        private int _subSlotCount;
        private int _sumMeshIndex;
        private int _verticeIndex;
        private int _verticeOffset;

        private bool _isCanCombineMesh = false;

        private void Start()
        {
            this._unityArmature = GetComponent<UnityArmatureComponent>();
            this._isCanCombineMesh = true;
            // this.BeginCombineMesh();
            this._dirty = true;
        }

        private void OnDestroy()
        {
            if (this.meshBuffers != null)
            {
                for (var i = 0; i < this.meshBuffers.Length; i++)
                {
                    var meshBuffer = this.meshBuffers[i];
                    meshBuffer.Dispose();
                }
            }

            this.meshBuffers = null;
            this._dirty = false;

            this._unityArmature = null;
            this._subSlotCount = 0;
            this._sumMeshIndex = -1;
            this._verticeIndex = -1;
            this._verticeOffset = -1;

            this._isCanCombineMesh = false;
        }

        private void RestoreArmature(Armature armatre)
        {
            var slots = armatre.GetSlots();

            foreach (UnityNewSlot slot in slots)
            {
                if (slot.childArmature != null)
                {
                    RestoreArmature(slot.childArmature);
                }
                else
                {
                    slot.CancelCombineMesh();
                }
            }
        }

        private void LateUpdate()
        {
            //
            // if (Input.GetMouseButtonDown(0))
            // {
            //     this._isCanCombineMesh = false;
            //     this.RestoreArmature(this._unityArmature.armature);
            // }
            if (this._dirty)
            {
                this.BeginCombineMesh();
                this._dirty = false;
            }

            if (this.meshBuffers == null)
            {
                return;
            }

            for (var i = 0; i < this.meshBuffers.Length; i++)
            {
                var meshBuffer = this.meshBuffers[i];
                if (meshBuffer.meshDirty)
                {
                    meshBuffer.UpdateMesh();
                    meshBuffer.meshDirty = false;
                }
            }
        }

        public void BeginCombineMesh()
        {
            if (!this._isCanCombineMesh || _unityArmature.isUGUI)
            {
                return;
            }
            //
            UnityEngine.Debug.Log("开始合并网格:" + this._unityArmature.armature.GetSlots().Count);

            this._sumMeshIndex = 0;
            this._verticeIndex = 0;
            this._verticeOffset = 0;
            this._subSlotCount = 0;

            //这里先回复数据
            if (this.meshBuffers != null)
            {
                for (var i = 0; i < this.meshBuffers.Length; i++)
                {
                    var meshBuffer = this.meshBuffers[i];
                    meshBuffer.Dispose();
                }

                this.meshBuffers = null;
            }

            List<MeshBuffer> buffers = new List<MeshBuffer>();
            //
            this.CombineSingleArmatureMesh(this._unityArmature.armature, buffers);

            this.meshBuffers = buffers.ToArray();

            // this._unityArmature.armature.InvalidUpdate(null, true);

            UnityEngine.Debug.Log("合并结束:" + this._subSlotCount);
        }

        public void CombineSingleArmatureMesh(Armature armature, List<MeshBuffer> buffers)
        {
            var slots = new List<Slot>(armature.GetSlots());
            if (slots.Count == 0)
            {
                return;
            }

            this._verticeIndex = 0;
            this._verticeOffset = 0;
            List<CombineInstance> readyCombines = new List<CombineInstance>();
            //
            var isBreakCombineMesh = false;
            var isSameMaterial = false;
            var isChildAramture = false;
            UnityNewSlot slotMeshProxy = null;

            GameObject slotDisplay = null;
            MeshRenderer slotMeshRenderer = null;
            Mesh slotMesh = null;
            Material slotMat = null;
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i] as UnityNewSlot;

                slot.CancelCombineMesh();

                isChildAramture = slot.childArmature != null;
                slotDisplay = slot.renderDisplay;
                slotMeshRenderer = slot._renderer;
                slotMesh = slot.mesh;
                slotMat = slot._renderer.sharedMaterial;

                if (slotMeshProxy != null &&
                    slotMeshProxy.meshRenderer.sharedMaterial != null)
                {
                    if (slotMat == null)
                    {
                        isSameMaterial = true;
                    }
                    else
                    {
                        isSameMaterial = slotMeshProxy.meshRenderer.sharedMaterial.name == slotMat.name;
                    }
                }
                else
                {
                    isSameMaterial = false;
                }

                //先检查这个slot会不会打断网格合并
                isBreakCombineMesh = isChildAramture ||
                                    slotMeshRenderer == null ||
                                    slot._isIgnoreCombineMesh ||
                                    !isSameMaterial;

                //如果会打断，那么先合并一次
                if (isBreakCombineMesh)
                {
                    // UnityEngine.Debug.Log("打断合并:" + slot.name);
                    this.CombineMesh(slotMeshProxy, readyCombines, buffers);
                    slotMeshProxy = null;
                }

                if (slotMeshProxy == null && !isChildAramture &&
                    !slot._isIgnoreCombineMesh &&
                    slotDisplay != null && slotDisplay.activeSelf &&
                    slotMeshRenderer != null && slotMesh != null)
                {
                    slotMeshProxy = slot;
                    // UnityEngine.Debug.Log("新的代理:" + slot.name);
                }

                //如果不会合并，检查一下是否是子骨架
                if (isChildAramture)
                {
                    //如果是子骨架，递归，子骨架必然打断
                    this.CombineSingleArmatureMesh(slot.childArmature, buffers);
                }
                else
                {
                    this.PushReadyCombines(slot, readyCombines, slot == slotMeshProxy);
                    //如果是最后一个合并一下
                    if (i == slots.Count - 1)
                    {
                        this.CombineMesh(slotMeshProxy, readyCombines, buffers);
                        slotMeshProxy = null;
                    }
                }
            }
        }

        private void PushReadyCombines(UnityNewSlot slot, List<CombineInstance> readyCombines, bool isSlotProxy)
        {
            GameObject go = slot.renderDisplay;
            if (go == null || !go.activeSelf || slot._isIgnoreCombineMesh)
            {
                slot._isCombineMesh = false;
                slot._sumMeshIndex = -1;
                slot._verticeIndex = -1;
                slot._verticeOffset = -1;
                slot._combineMesh = null;
                // UnityEngine.Debug.Log("不能合并:" + slot.name);
                return;
            }

            var parentTransfrom = (slot._armature.proxy as UnityArmatureComponent).transform;
            CombineInstance com = new CombineInstance();
            com.mesh = slot._meshBuffer.sharedMesh;
            com.transform = go.transform.localToWorldMatrix;

            //
            slot._isCombineMesh = true;
            slot._sumMeshIndex = this._sumMeshIndex;
            slot._verticeIndex = this._verticeIndex;
            slot._verticeOffset = this._verticeOffset;
            slot._combineMesh = this;

            DisableSlot(slot, !isSlotProxy);

            this._verticeIndex++;
            this._verticeOffset += com.mesh.vertices.Length;
            this._subSlotCount++;
            
            slot._meshDirty = true;
            slot._transformDirty = true;

            // UnityEngine.Debug.Log("待合并:" + slot.name);
            readyCombines.Add(com);
        }

        private void DisableSlot(UnityNewSlot slot, bool b)
        {
            GameObject go = slot.renderDisplay;
            // UnityEngine.Debug.LogWarning(slot.name + "恢复网格:" + go.activeSelf + " b:" + b);
            if (b)
            {
                slot._meshBuffer.enabled = false;
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
            else
            {
                slot._meshBuffer.enabled = true;
                if (go.activeSelf)
                {
                    go.SetActive(true);
                }
                else
                {
                    go.SetActive(false);
                }
            }
        }

        private void CombineMesh(UnityNewSlot slotMeshProxy, List<CombineInstance> readyCombines, List<MeshBuffer> buffers)
        {
            if (slotMeshProxy == null)
            {
                return;
            }

            //合并上一次的
            if (readyCombines.Count > 1)
            {
                MeshBuffer meshBuffer = new MeshBuffer();
                meshBuffer.name = slotMeshProxy.name;
                meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                meshBuffer.sharedMesh.Clear();

                meshBuffer.CombineMeshes(readyCombines.ToArray());

                slotMeshProxy._meshFilter.sharedMesh = meshBuffer.sharedMesh;
                meshBuffer.meshDirty = true;

                //
                DisableSlot(slotMeshProxy, false);

                // UnityEngine.Debug.Log("合并:" + slotMeshProxy.name);

                buffers.Add(meshBuffer);
                //重新赋值
                this._sumMeshIndex++;
            }
            else if (readyCombines.Count == 1)//单个的没必要合并
            {
                slotMeshProxy.CancelCombineMesh();

                // UnityEngine.Debug.Log("放弃合并:" + slotMeshProxy.name);
            }

            //清理数据
            readyCombines.Clear();
            this._verticeIndex = 0;
            this._verticeOffset = 0;
            slotMeshProxy = null;
        }
    }
}