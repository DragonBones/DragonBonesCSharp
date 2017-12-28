using System;
using System.Collections;
using UnityEngine;
using DragonBones;

namespace DragonBones
{
    [Serializable]
    public class MeshBuffer
    {
        public string name;
        public Mesh sharedMesh;
        public Vector2[] uvBuffers;
        public Vector3[] vertexBuffers;
        public Color32[] color32Buffers;
        public int[] triangleBuffers;

        public bool meshDirty;

        public static Mesh GenerateMesh()
        {
            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            mesh.MarkDynamic();

            return mesh;
        }

        public void Dispose()
        {
            if(this.sharedMesh != null)
            {
                UnityFactoryHelper.DestroyUnityObject(this.sharedMesh);
            }

            this.name = string.Empty;
            this.sharedMesh = null;
            this.meshDirty = false;
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
        }

        public void UpdateMesh()
        {
            this.sharedMesh.vertices = this.vertexBuffers;// Must set vertices before uvs.
            this.sharedMesh.uv = this.uvBuffers;
            this.sharedMesh.triangles = this.triangleBuffers;
        }
    }
}