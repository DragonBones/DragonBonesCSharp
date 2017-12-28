using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

namespace DragonBones
{
    [RequireComponent(typeof(UnityArmatureComponent))]
    public class CombineMeshs : MonoBehaviour
    {
        private UnityArmatureComponent _unityArmature;

        public MeshBuffer[] meshBuffers;
        public Vector3[] viewVertex;
        public bool _dirty = false;
        private int _sumMeshIndex;
        private int _verticeIndex;

        private bool _isCanCombineMesh = false;

        private void Start()
        {
            this._unityArmature = GetComponent<UnityArmatureComponent>();
            this._isCanCombineMesh = true;
            this._dirty = true;
        }

        private void LateUpdate()
        {
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
                    meshBuffer.sharedMesh.vertices = meshBuffer.vertexBuffers;
                    meshBuffer.sharedMesh.colors32 = meshBuffer.color32Buffers;
                    meshBuffer.meshDirty = false;
                }
            }
        }

        private void BeginCombineMesh()
        {
            if (!this._isCanCombineMesh || _unityArmature.isUGUI)
            {
                return;
            }
            //
            UnityEngine.Debug.Log("开始合并网格");

            this._sumMeshIndex = 0;
            this._verticeIndex = 0;

            // int order = 0;
            // foreach (UnityNewSlot slot in this._unityArmature.armature.GetSlots())
            // {
            //     if (slot.childArmature != null)
            //     {
            //         foreach (UnityNewSlot sslot in slot.childArmature.GetSlots())
            //         {
            //             UnityEngine.Debug.Log("name:" + sslot.name + " order:" + order++);

            //             if (sslot.meshRenderer != null && sslot.meshRenderer.sharedMaterial != null)
            //             {
            //                 UnityEngine.Debug.Log("matName:" + sslot.meshRenderer.sharedMaterial.name);
            //             }
            //         }
            //     }
            //     else
            //     {
            //         UnityEngine.Debug.Log("name:" + slot.name + " order:" + order++);

            //         if (slot.meshRenderer != null && slot.meshRenderer.sharedMaterial != null)
            //         {
            //             UnityEngine.Debug.Log("matName:" + slot.meshRenderer.sharedMaterial.name);
            //         }
            //     }
            // }

            List<MeshBuffer> buffers = new List<MeshBuffer>();
            //
            this.CombineSingleArmatureMesh(this._unityArmature.armature, buffers);

            this.meshBuffers = buffers.ToArray();
        }

        public void CombineSingleArmatureMesh(Armature armature, List<MeshBuffer> buffers)
        {
            var slots = new List<Slot>(armature.GetSlots());
            if (slots.Count == 0)
            {
                return;
            }

            this._verticeIndex = 0;
            List<CombineInstance> readyCombines = new List<CombineInstance>();
            //
            var parentTransfrom = (armature.proxy as UnityArmatureComponent).transform;
            var isBreakCombineMesh = false;
            var isSameMaterial = false;
            var isChildAramture = false;
            UnityNewSlot slotMeshProxy = null;

            GameObject slotDisplay = null;
            MeshRenderer slotMeshRenderer = null;
            Mesh slotMesh = null;
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i] as UnityNewSlot;

                isChildAramture = slot.childArmature != null;
                slotDisplay = slot.renderDisplay;
                slotMeshRenderer = slot.meshRenderer;
                slotMesh = slot.mesh;

                if (slotMeshProxy != null && slotMeshRenderer != null &&
                    slotDisplay != null && slotDisplay.activeSelf &&
                    slotMeshProxy.meshRenderer.sharedMaterial != null && slotMeshRenderer.sharedMaterial != null)
                {
                    isSameMaterial = slotMeshProxy.meshRenderer.sharedMaterial.name == slotMeshRenderer.sharedMaterial.name;
                    // UnityEngine.Debug.Log("mat1:" + mat1 + " mat2:" + mat2 + " " + (mat1 == mat2) );
                }
                else
                {
                    isSameMaterial = false;
                }

                //先检查这个slot会不会打断网格合并
                isBreakCombineMesh = isChildAramture ||
                                    slotMeshRenderer == null ||
                                    slotMesh == null ||
                                    !isSameMaterial;

                //如果会打断，那么先合并一次
                if (isBreakCombineMesh)
                {
                    this.CombineMesh(slotMeshProxy, readyCombines, buffers);
                    slotMeshProxy = null;
                }

                if (slotMeshProxy == null && !isChildAramture && slotMeshRenderer != null && slotMesh != null)
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
                    this.PushReadyCombines(slot, parentTransfrom, readyCombines);
                    //如果是最后一个合并一下
                    if (i == slots.Count - 1)
                    {
                        this.CombineMesh(slotMeshProxy, readyCombines, buffers);
                        slotMeshProxy = null;
                    }
                }
            }
        }

        private void PushReadyCombines(UnityNewSlot slot, UnityEngine.Transform parentTransfrom, List<CombineInstance> readyCombines)
        {
            GameObject go = slot.renderDisplay;
            if (go == null || !go.activeSelf)
            {
                return;
            }

            CombineInstance com = new CombineInstance();
            com.mesh = slot.mesh;
            com.transform = parentTransfrom.worldToLocalMatrix * go.transform.localToWorldMatrix;

            //
            slot._isCombineMesh = true;
            slot._sumMeshIndex = this._sumMeshIndex;
            slot._verticeOffset = this._verticeIndex;
            slot._combineMesh = this;

            slot.meshRenderer.enabled = false;
            go.hideFlags = HideFlags.HideAndDontSave;
            go.SetActive(false);

            this._verticeIndex += com.mesh.vertices.Length;

            // UnityEngine.Debug.Log("待合并:" + slot.name);
            readyCombines.Add(com);
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
                meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                meshBuffer.sharedMesh.Clear();

                meshBuffer.CombineMeshes(readyCombines.ToArray());
                meshBuffer.meshDirty = true;

                //
                slotMeshProxy._meshFilter.sharedMesh = meshBuffer.sharedMesh;
                slotMeshProxy.renderDisplay.SetActive(true);
                slotMeshProxy.meshRenderer.enabled = true;
                slotMeshProxy.renderDisplay.hideFlags = HideFlags.None;

                // UnityEngine.Debug.Log("合并:" + slotMeshProxy.name);

                buffers.Add(meshBuffer);
                //重新赋值
                this._sumMeshIndex++;
            }
            else if(readyCombines.Count == 1)//单个的没必要合并
            {
                slotMeshProxy._isCombineMesh = false;
                slotMeshProxy._sumMeshIndex = -1;
                slotMeshProxy._verticeOffset = -1;
                slotMeshProxy._combineMesh = null;

                slotMeshProxy.renderDisplay.SetActive(true);
                slotMeshProxy.meshRenderer.enabled = true;
                slotMeshProxy.renderDisplay.hideFlags = HideFlags.None;
            }

            //清理数据
            readyCombines.Clear();
            this._verticeIndex = 0;
            slotMeshProxy = null;
        }
    }
}