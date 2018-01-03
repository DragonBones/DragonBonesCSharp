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
        public bool dirty = false;

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
            this.dirty = true;
        }

        private void OnDestroy()
        {
            if (this._unityArmature != null)
            {
                this.RestoreArmature(this._unityArmature._armature);

                if (this._unityArmature._armature != null)
                {
                    this._unityArmature._armature.InvalidUpdate();
                }
            }

            if (this.meshBuffers != null)
            {
                for (var i = 0; i < this.meshBuffers.Length; i++)
                {
                    var meshBuffer = this.meshBuffers[i];
                    meshBuffer.Dispose();
                }
            }

            this.meshBuffers = null;
            this.dirty = false;

            this._unityArmature = null;
            this._subSlotCount = 0;
            this._sumMeshIndex = -1;
            this._verticeIndex = -1;
            this._verticeOffset = -1;

            this._isCanCombineMesh = false;
        }

        private void RestoreArmature(Armature armature)
        {
            if (armature == null)
            {
                return;
            }
            //
            foreach (UnityNewSlot slot in armature.GetSlots())
            {
                if (slot.childArmature != null)
                {
                    RestoreArmature(armature);
                }
                else
                {
                    slot.CancelCombineMesh();
                }
            }
        }

        private void LateUpdate()
        {
            if (this.dirty)
            {
                this.BeginCombineMesh();
                this.dirty = false;
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
                    meshBuffer.UpdateVertices();
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

            //
            if (this.meshBuffers != null)
            {
                for (var i = 0; i < this.meshBuffers.Length; i++)
                {
                    var meshBuffer = this.meshBuffers[i];
                    meshBuffer.Dispose();
                }

                this.meshBuffers = null;
            }

            List<CombineSlotInfo> combineSlots = new List<CombineSlotInfo>();
            //
            this.CollectMesh(this._unityArmature.armature, combineSlots);

            //
            //先合并
            List<MeshBuffer> buffers = new List<MeshBuffer>();
            for (var i = 0; i < combineSlots.Count; i++)
            {
                var combineSlot = combineSlots[i];

                //
                var proxySlot = combineSlot.proxySlot;
                MeshBuffer meshBuffer = new MeshBuffer();
                meshBuffer.name = proxySlot._meshBuffer.name;
                meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                meshBuffer.sharedMesh.Clear();

                meshBuffer.CombineMeshes(combineSlot.combines.ToArray());
                // meshBuffer.meshDirty = true;
                meshBuffer.UpdateVertices();
                //
                proxySlot._meshFilter.sharedMesh = meshBuffer.sharedMesh;

                buffers.Add(meshBuffer);

                //

            }

            this.meshBuffers = buffers.ToArray();

            //
            for (var i = 0; i < combineSlots.Count; i++)
            {
                var combineSlot = combineSlots[i];

                //
                var proxySlot = combineSlot.proxySlot;
                this._verticeOffset = 0;
                for (int j = 0; j < combineSlot.slots.Count; j++)
                {
                    var slot = combineSlot.slots[j];

                    slot._isCombineMesh = true;
                    slot._sumMeshIndex = i;
                    slot._verticeIndex = j;
                    slot._verticeOffset = this._verticeOffset;
                    slot._combineMesh = this;
                    slot._meshBuffer.enabled = false;
                    if (slot._renderDisplay != null)
                    {
                        slot._renderDisplay.SetActive(false);
                    }

                    this._verticeOffset += slot._meshBuffer.vertexBuffers.Length;
                    this._subSlotCount++;

                    // slot.Test();
                    // slot._meshDirty = true;
                    // slot._transformDirty = true;
                }

                //被合并的显示
                if (proxySlot._renderDisplay != null)
                {
                    proxySlot._renderDisplay.SetActive(true);
                }
            }

            UnityEngine.Debug.Log("buffers:" + buffers.Count + " slots:" + combineSlots.Count);
            UnityEngine.Debug.Log("合并结束:" + this._subSlotCount);
        }

        public void CollectMesh(Armature armature, List<CombineSlotInfo> combineSlots)
        {
            var slots = new List<Slot>(armature.GetSlots());
            if (slots.Count == 0)
            {
                return;
            }

            this._verticeIndex = 0;
            this._verticeOffset = 0;
            //
            var isBreakCombineMesh = false;
            var isSameMaterial = false;
            var isChildAramture = false;
            UnityNewSlot slotMeshProxy = null;

            GameObject slotDisplay = null;
            MeshRenderer slotMeshRenderer = null;
            Material slotMat = null;
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i] as UnityNewSlot;

                slot.CancelCombineMesh();

                isChildAramture = slot.childArmature != null;
                slotDisplay = slot.renderDisplay;
                slotMeshRenderer = slot._meshRenderer;
                slotMat = slot._meshRenderer.sharedMaterial;

                if (slotMeshProxy != null && slotMeshProxy._meshRenderer.sharedMaterial != null)
                {
                    if (slotMat == null)
                    {
                        isSameMaterial = true;
                    }
                    else
                    {
                        isSameMaterial = slotMeshProxy._meshRenderer.sharedMaterial.name == slotMat.name;
                    }
                }
                else
                {
                    isSameMaterial = slotMeshProxy == null;
                }

                //先检查这个slot会不会打断网格合并
                isBreakCombineMesh = isChildAramture ||
                                    slotMeshRenderer == null ||
                                    slot._isIgnoreCombineMesh ||
                                    !isSameMaterial ||
                                    slotDisplay == null ||
                                    !slotDisplay.activeSelf;

                //如果会打断，那么先合并一次
                if (isBreakCombineMesh)
                {
                    // UnityEngine.Debug.Log("打断合并:" + slot.name);
                    if (combineSlots.Count > 0)
                    {
                        if (combineSlots[combineSlots.Count - 1].combines.Count == 1)
                        {
                            combineSlots.RemoveAt(combineSlots.Count - 1);
                        }
                    }

                    slotMeshProxy = null;
                }
                //
                if (slotMeshProxy == null && !isBreakCombineMesh)
                {
                    CombineSlotInfo combineSlot = new CombineSlotInfo();
                    combineSlot.proxySlot = slot;
                    combineSlot.combines = new List<CombineInstance>();
                    combineSlot.slots = new List<UnityNewSlot>();
                    combineSlots.Add(combineSlot);

                    slotMeshProxy = slot;
                    // UnityEngine.Debug.Log("新的代理:" + slot.name);
                }

                //如果不会合并，检查一下是否是子骨架
                if (isChildAramture)
                {
                    //如果是子骨架，递归，子骨架必然打断
                    this.CollectMesh(slot.childArmature, combineSlots);
                }
                else
                {
                    if (slotDisplay != null && slotDisplay.activeSelf && !slot._isIgnoreCombineMesh)
                    {
                        var parentTransfrom = (slot._armature.proxy as UnityArmatureComponent).transform;
                        CombineInstance com = new CombineInstance();
                        com.mesh = slot._meshBuffer.sharedMesh;
                        com.transform = slotDisplay.transform.localToWorldMatrix;

                        combineSlots[combineSlots.Count - 1].combines.Add(com);
                        combineSlots[combineSlots.Count - 1].slots.Add(slot);
                    }
                    //如果是最后一个合并一下
                    if (i != slots.Count - 1)
                    {
                        continue;
                    }
                    //
                    if (combineSlots.Count > 0)
                    {
                        if (combineSlots[combineSlots.Count - 1].combines.Count == 1)
                        {
                            combineSlots.RemoveAt(combineSlots.Count - 1);
                        }
                    }
                    slotMeshProxy = null;
                }
            }
        }
    }

    public struct CombineSlotInfo
    {
        public UnityNewSlot proxySlot;
        public List<CombineInstance> combines;
        public List<UnityNewSlot> slots;
    }
}