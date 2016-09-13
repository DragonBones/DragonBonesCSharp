using System.Collections.Generic;

namespace dragonBones
{
    /**
     * @language zh_CN
     * 骨架数据。
     * @see dragonBones.Armature
     * @version DragonBones 3.0
     */
    public class ArmatureData : BaseObject
    {
        private static int _onSortSlots(SlotData a, SlotData b)
        {
            return a.zOrder > b.zOrder ? 1 : -1;
        }

        /**
         * @language zh_CN
         * 动画帧率。
         * @version DragonBones 3.0
         */
        public uint frameRate;

        /**
         * @private
         */
        public uint cacheFrameRate;

        /**
         * @language zh_CN
         * 骨架类型。
         * @see dragonBones.ArmatureType
         * @version DragonBones 3.0
         */
        public ArmatureType type;

        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @private
         */
        public DragonBonesData parent;

        /**
         * @private
         */
        public Rectangle aabb = new Rectangle();

        /**
         * @language zh_CN
         * 所有的骨骼数据。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         */
        public Dictionary<string, BoneData> bones = new Dictionary<string, BoneData>();

        /**
         * @language zh_CN
         * 所有的插槽数据。
         * @see dragonBones.SlotData
         * @version DragonBones 3.0
         */
        public Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();

        /**
         * @language zh_CN
         * 所有的皮肤数据。
         * @see dragonBones.SkinData
         * @version DragonBones 3.0
         */
        public Dictionary<string, SkinData> skins = new Dictionary<string, SkinData>();

        /**
         * @language zh_CN
         * 所有的动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();

        /**
         * @private
         */
        public List<ActionData> actions = new List<ActionData>();

        private bool _boneDirty;
        private bool _slotDirty;
        private SkinData _defaultSkin;
        private AnimationData _defaultAnimation;
        private List<BoneData> _sortedBones = new List<BoneData>();
        private List<SlotData> _sortedSlots = new List<SlotData>();
        private Dictionary<string, List<BoneData>> _bonesChildren = new Dictionary<string, List<BoneData>>();

        /**
         * @private
         */
        public ArmatureData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            foreach (var pair in bones)
            {
                pair.Value.returnToPool();
            }

            foreach (var pair in slots)
            {
                pair.Value.returnToPool();
            }

            foreach (var pair in skins)
            {
                pair.Value.returnToPool();
            }

            foreach (var pair in animations)
            {
                pair.Value.returnToPool();
            }

            foreach (var action in actions)
            {
                action.returnToPool();
            }

            frameRate = 0;
            cacheFrameRate = 0;
            type = ArmatureType.Armature;
            name = null;
            parent = null;
            aabb.clear();
            actions.Clear();

            _boneDirty = false;
            _slotDirty = false;
            _defaultSkin = null;
            _defaultAnimation = null;
            _sortedBones.Clear();
            _sortedSlots.Clear();
            _bonesChildren.Clear();
        }

        private void _sortBones()
        {
            var total = _sortedBones.Count;
            if (total < 1)
            {
                return;
            }

            var sortHelper = _sortedBones.ToArray();
            int index = 0;
            int count = 0;

            _sortedBones.Clear();

            while (count < total)
            {
                var bone = sortHelper[index++];
                if (index >= total)
                {
                    index = 0;
                }

                if (_sortedBones.Contains(bone))
                {
                    continue;
                }

                if (bone.parent != null && _sortedBones.Contains(bone.parent))
                {
                    continue;
                }

                if (bone.ik != null && _sortedBones.Contains(bone.ik))
                {
                    continue;
                }

                if (bone.ik != null && bone.chain > 0 && bone.chainIndex == bone.chain)
                {
                    //_sortedBones.splice(_sortedBones.indexOf(bone.parent) + 1, 0, bone); // TODO
                }
                else
                {
                    _sortedBones.Add(bone);
                }

                count++;
            }
        }

        private void _sortSlots()
        {
            _sortedSlots.Sort(ArmatureData._onSortSlots);
        }

        /**
         * @private
         */
        public void cacheFrames(uint value)
        {
            if (cacheFrameRate == value)
            {
                return;
            }

            cacheFrameRate = value;

            var frameScale = (float)cacheFrameRate / frameRate;
            foreach (var pair in animations)
            {
                pair.Value.cacheFrames(frameScale);
            }
        }

        /**
         * @private
         */
        public void addBone(BoneData value, string parentName)
        {
            if (value != null && value.name != null && !bones.ContainsKey(value.name))
            {
                if (parentName != null)
                {
                    var parent = getBone(parentName);
                    if (parent != null)
                    {
                        value.parent = parent;
                    }
                    else
                    {
                        (_bonesChildren.ContainsKey(parentName) ?
                            _bonesChildren[parentName] :
                            (_bonesChildren[parentName] = new List<BoneData>())).Add(value);
                    }
                }

                if (_bonesChildren.ContainsKey(value.name))
                {
                    foreach (var child in _bonesChildren[value.name])
                    {
                        child.parent = value;
                    }

                    _bonesChildren.Remove(value.name);
                }

                bones[value.name] = value;
                _sortedBones.Add(value);
                _boneDirty = true;
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @private
         */
        public void addSlot(SlotData value)
        {
            if (value != null && value.name != null && !slots.ContainsKey(value.name))
            {
                slots[value.name] = value;
                _sortedSlots.Add(value);
                _slotDirty = true;
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @private
         */
        public void addSkin(SkinData value)
        {
            if (value != null && value.name != null && !skins.ContainsKey(value.name))
            {
                skins[value.name] = value;
                if (_defaultSkin == null)
                {
                    _defaultSkin = value;
                }
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @private
         */
        public void addAnimation(AnimationData value)
        {
            if (value != null && value.name != null && !animations.ContainsKey(value.name))
            {
                animations[value.name] = value;
                if (_defaultAnimation == null)
                {
                    _defaultAnimation = value;
                }
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @language zh_CN
         * 获取指定名称的骨骼数据。
         * @param name 骨骼数据名称。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         */
        public BoneData getBone(string name)
        {
            return bones[name];
        }

        /**
         * @language zh_CN
         * 获取指定名称的插槽数据。
         * @param name 插槽数据名称。
         * @see dragonBones.SlotData
         * @version DragonBones 3.0
         */
        public SlotData getSlot(string name)
        {
            return slots[name];
        }

        /**
         * @language zh_CN
         * 获取指定名称的皮肤数据。
         * @param name 皮肤数据名称。
         * @see dragonBones.SkinData
         * @version DragonBones 3.0
         */
        public SkinData getSkin(string name)
        {
            return name != null ? skins[name] : _defaultSkin;
        }

        /**
         * @language zh_CN
         * 获取指定名称的动画数据。
         * @param name 动画数据名称。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public AnimationData getAnimation(string name)
        {
            return name != null ? animations[name] : _defaultAnimation;
        }

        /**
         * @private
         */
        public List<BoneData> getSortedBones()
        {
            if (_boneDirty)
            {
                _boneDirty = false;
                _sortBones();
            }

            return _sortedBones;
        }

        /**
         * @private
         */
        public List<SlotData> getSortedSlots()
        {
            if (_slotDirty)
            {
                _slotDirty = false;
                _sortSlots();
            }

            return _sortedSlots;
        }

        /**
         * @language zh_CN
         * 获取默认的皮肤数据。
         * @see dragonBones.SkinData
         * @version DragonBones 4.5
         */
        public SkinData getDefaultSkin()
        {
            return _defaultSkin;
        }

        /**
         * @language zh_CN
         * 获取默认的动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 4.5
         */
        public AnimationData getDefaultAnimation()
        {
            return _defaultAnimation;
        }
    }

    /**
     * @language zh_CN
     * 骨骼数据。
     * @see dragonBones.Bone
     * @version DragonBones 3.0
     */
    public class BoneData : BaseObject
    {
        /**
         * @private
         */
        public bool inheritTranslation;

        /**
         * @private
         */
        public bool inheritRotation;

        /**
         * @private
         */
        public bool inheritScale;

        /**
         * @private
         */
        public bool bendPositive;

        /**
         * @private
         */
        public uint chain;

        /**
         * @private
         */
        public int chainIndex;

        /**
         * @private
         */
        public float weight;

        /**
         * @private
         */
        public float length;

        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @language zh_CN
         * 所属的父骨骼数据。
         * @version DragonBones 3.0
         */
        public BoneData parent;

        /**
         * @private
         */
        public BoneData ik;

        /**
         * @private
         */
        public Transform transform = new Transform();

        /**
         * @private
         */
        public BoneData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            inheritTranslation = false;
            inheritRotation = false;
            inheritScale = false;
            bendPositive = false;
            chain = 0;
            chainIndex = 0;
            weight = 0;
            length = 0.0f;
            name = null;
            parent = null;
            ik = null;
            transform.identity();
        }
    }

    /**
     * @language zh_CN
     * 插槽数据。
     * @see dragonBones.Slot
     * @version DragonBones 3.0
     */
    public class SlotData : BaseObject
    {
        /**
         * @private
         */
        public static ColorTransform DEFAULT_COLOR = new ColorTransform();

        /**
         * @private
         */
        public static ColorTransform generateColor()
        {
            return new ColorTransform();
        }

        /**
         * @private
         */
        public int displayIndex;

        /**
         * @private
         */
        public int zOrder;

        /**
         * @private
         */
        public BlendMode blendMode;

        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @language zh_CN
         * 所属的父骨骼数据。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         */
        public BoneData parent;

        /**
         * @private
         */
        public ColorTransform color;

        /**
         * @private
         */
        public List<ActionData> actions = new List<ActionData>();

        /**
         * @private
         */
        public SlotData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            foreach (var action in actions)
            {
                action.returnToPool();
            }

            displayIndex = 0;
            zOrder = 0;
            blendMode = BlendMode.Normal;
            name = null;
            parent = null;
            color = null;
            actions.Clear();
        }
    }

    /**
     * @language zh_CN
     * 皮肤数据。
     * @version DragonBones 3.0
     */
    public class SkinData : BaseObject
    {
        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;

        /**
         * @private
         */
        public Dictionary<string, SlotDisplayDataSet> slots = new Dictionary<string, SlotDisplayDataSet>();

        /**
         * @private
         */
        public SkinData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {

            foreach (var pair in slots)
            {
                pair.Value.returnToPool();
            }

            name = null;
            slots.Clear();
        }

        /**
         * @private
         */
        public void addSlot(SlotDisplayDataSet value)
        {
            if (value != null && value.slot != null && !slots.ContainsKey(value.slot.name))
            {
                slots[value.slot.name] = value;
            }
            else
            {
                DragonBones.warn("");
            }
        }

        /**
         * @private
         */
        public SlotDisplayDataSet getSlot(string name)
        {
            return slots[name];
        }
    }

    /**
     * @private
     */
    public class SlotDisplayDataSet : BaseObject
    {
        public SlotData slot;
        public List<DisplayData> displays = new List<DisplayData>();

        public SlotDisplayDataSet()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            foreach (var display in displays)
            {
                display.returnToPool();
            }

            slot = null;
            displays.Clear();
        }
    }

    /**
     * @private
     */
    public class DisplayData : BaseObject
    {
        public bool isRelativePivot;
        public DisplayType type;
        public string name;
        public TextureData texture;
        public ArmatureData armature;
        public MeshData mesh;
        public Point pivot = new Point();
        public Transform transform = new Transform();

        public DisplayData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            isRelativePivot = false;
            type = DisplayType.Image;
            name = null;
            texture = null;
            armature = null;

            if (mesh != null)
            {
                mesh.returnToPool();
                mesh = null;
            }

            pivot.clear();
            transform.identity();
        }
    }

    /**
     * @private
     */
    public class MeshData : BaseObject
    {
        public bool skinned;
        public Matrix slotPose = new Matrix();

        public List<float> uvs = new List<float>(); // vertices * 2
        public List<float> vertices = new List<float>(); // vertices * 2
        public List<int> vertexIndices = new List<int>(); // triangles * 3

        public List<List<int>> boneIndices = new List<List<int>>(); // vertices bones
        public List<List<float>> weights = new List<List<float>>(); // vertices bones
        public List<List<float>> boneVertices = new List<List<float>>(); // vertices bones * 2

        public List<BoneData> bones = new List<BoneData>(); // bones
        public List<Matrix> inverseBindPose = new List<Matrix>(); // bones

        public MeshData()
        {
        }

        /**
        * @inheritDoc
        */
        protected override void _onClear()
        {
            skinned = false;
            slotPose.identity();
            uvs.Clear();
            vertices.Clear();
            vertexIndices.Clear();
            boneIndices.Clear();
            weights.Clear();
            boneVertices.Clear();
            bones.Clear();
            inverseBindPose.Clear();
        }
    }
}