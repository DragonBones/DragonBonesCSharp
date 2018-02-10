using System.Collections.Generic;
namespace DragonBones
{
    public class DeformVertices : BaseObject
    {
        public bool verticesDirty;
        public readonly List<float> vertices = new List<float>();
        public readonly List<Bone> bones = new List<Bone>();
        public VerticesData verticesData;

        protected override void _OnClear()
        {
            this.verticesDirty = false;
            this.vertices.Clear();
            this.bones.Clear();
            this.verticesData = null;
        }

        public void init(VerticesData verticesDataValue, Armature armature)
        {
            this.verticesData = verticesDataValue;

            if (this.verticesData != null)
            {
                var vertexCount = 0;
                if (this.verticesData.weight != null)
                {
                    vertexCount = this.verticesData.weight.count * 2;
                }
                else
                {
                    vertexCount = (int)this.verticesData.data.intArray[this.verticesData.offset + (int)BinaryOffset.MeshVertexCount] * 2;
                }

                this.verticesDirty = true;
                this.vertices.ResizeList(vertexCount);
                this.bones.Clear();
                //
                for (int i = 0, l = this.vertices.Count; i < l; ++i)
                {
                    this.vertices[i] = 0.0f;
                }

                if (this.verticesData.weight != null)
                {
                    for (int i = 0, l = this.verticesData.weight.bones.Count; i < l; ++i)
                    {
                        var bone = armature.GetBone(this.verticesData.weight.bones[i].name);
                        this.bones.Add(bone);
                    }
                }
            }
            else
            {
                this.verticesDirty = false;
                this.vertices.Clear();
                this.bones.Clear();
                this.verticesData = null;
            }
        }

        public bool isBonesUpdate()
        {
            foreach (var bone in this.bones)
            {
                if (bone != null && bone._childrenTransformDirty)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
