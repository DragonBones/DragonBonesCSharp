using System;
using System.Collections;
using UnityEngine;
using DragonBones;

namespace DragonBones
{
    public class MeshBuffer
    {
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

        public void UpdateColor32(int startOffset, int verticeCount, float r, float g, float b, float a)
        {
            for (int i = startOffset, l = startOffset + verticeCount; i < l; i++)
            {
                this.color32Buffers[i].r = (byte)(r * 255);
                this.color32Buffers[i].g = (byte)(g * 255);
                this.color32Buffers[i].b = (byte)(b * 255);
                this.color32Buffers[i].a = (byte)(a * 255);
            }
        }
    }
}