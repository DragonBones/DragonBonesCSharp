using System;
using System.Collections;
using UnityEngine;
using DragonBones;

namespace DragonBones
{
    [Serializable]
    public class MeshBuffer : IDisposable
    {
        public string name;
        public Mesh sharedMesh;
        public int vertexCount;
        public Vector3[] rawVertextBuffers;
        public Vector2[] uvBuffers;
        public Vector3[] vertexBuffers;
        public Color32[] color32Buffers;
        public int[] triangleBuffers;

        public bool meshDirty;
        public bool enabled;

        public static Mesh GenerateMesh()
        {
            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            mesh.MarkDynamic();

            return mesh;
        }

        public void Dispose()
        {
            if (this.sharedMesh != null)
            {
                UnityFactoryHelper.DestroyUnityObject(this.sharedMesh);
            }

            this.name = string.Empty;
            this.sharedMesh = null;
            this.vertexCount = 0;
            this.rawVertextBuffers = null;
            this.uvBuffers = null;
            this.vertexBuffers = null;
            this.color32Buffers = null;
            this.meshDirty = false;
            this.enabled = false;
        }

        public void Clear()
        {
            if (this.sharedMesh != null)
            {
                this.sharedMesh.Clear();
                this.sharedMesh.uv = null;
                this.sharedMesh.vertices = null;
                this.sharedMesh.normals = null;
                this.sharedMesh.triangles = null;
                this.sharedMesh.colors32 = null;
            }
        }

        public void Copy(MeshBuffer source, int sourceOffset)
        {
            if (this.uvBuffers == null || this.vertexBuffers == null || this.color32Buffers == null)
            {
                return;
            }

            //
            int index = 0;
            int i = 0;
            int len = 0;
            for (i = 0, len = this.uvBuffers.Length; i < len; i++)
            {
                index = i + sourceOffset;
                if (index >= source.uvBuffers.Length)
                {
                    continue;
                }

                this.uvBuffers[i] = source.uvBuffers[index];
            }

            //
            for (i = 0, len = this.vertexBuffers.Length; i < len; i++)
            {
                index = i + sourceOffset;
                if (index >= source.vertexBuffers.Length)
                {
                    continue;
                }

                this.vertexBuffers[i] = source.vertexBuffers[index];
            }

            // //
            // for(i = 0, len = this.triangleBuffers.Length; i < len; i++)
            // {
            //     index = i + sourceOffset;
            //     if(index >= source.triangleBuffers.Length)
            //     {
            //         continue;
            //     }

            //     this.triangleBuffers[i] = source.triangleBuffers[index];
            // }

            //
            for (i = 0, len = this.color32Buffers.Length; i < len; i++)
            {
                index = i + sourceOffset;
                if (index >= source.color32Buffers.Length)
                {
                    continue;
                }

                this.color32Buffers[i] = source.color32Buffers[index];
            }
        }

        public void CombineMeshes(CombineInstance[] combines)
        {
            if (this.sharedMesh == null)
            {
                this.sharedMesh = GenerateMesh();
            }

            this.sharedMesh.CombineMeshes(combines);

            //
            this.uvBuffers = this.sharedMesh.uv;
            this.vertexBuffers = this.sharedMesh.vertices;
            this.color32Buffers = this.sharedMesh.colors32;
            this.triangleBuffers = this.sharedMesh.triangles;

            this.vertexCount = this.vertexBuffers.Length;
            //
            if (this.color32Buffers == null || this.color32Buffers.Length != this.vertexCount)
            {
                this.color32Buffers = new Color32[vertexCount];
            }
        }

        public void InitMesh()
        {
            this.sharedMesh.vertices = this.vertexBuffers;// Must set vertices before uvs.
            this.sharedMesh.uv = this.uvBuffers;
            this.sharedMesh.triangles = this.triangleBuffers;

            if (this.vertexBuffers != null)
            {
                this.vertexCount = this.vertexBuffers.Length;
            }
            else
            {
                this.vertexCount = 0;
            }

            if (this.color32Buffers == null || this.color32Buffers.Length != this.vertexCount)
            {
                this.color32Buffers = new Color32[this.vertexCount];
            }

            this.enabled = true;
        }

        public void UpdateVertices()
        {
            this.sharedMesh.vertices = this.vertexBuffers;
            this.sharedMesh.RecalculateBounds();
        }

        public void UpdateColors()
        {
            this.sharedMesh.colors32 = this.color32Buffers;
        }
    }
}