using System.Collections.Generic;

namespace DragonBones
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
        public readonly Rectangle aabb = new Rectangle();

        /**
         * @private
         */
        public readonly List<ActionData> actions = new List<ActionData>();

        /**
         * @language zh_CN
         * 所有的骨骼数据。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, BoneData> bones = new Dictionary<string, BoneData>();

        /**
         * @language zh_CN
         * 所有的插槽数据。
         * @see dragonBones.SlotData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();

        /**
         * @language zh_CN
         * 所有的皮肤数据。
         * @see dragonBones.SkinData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, SkinData> skins = new Dictionary<string, SkinData>();

        /**
         * @language zh_CN
         * 所有的动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();

        /**
         * @private
         */
        public uint cacheFrameRate;

        /**
         * @private
         */
        public float scale;

        private bool _boneDirty;
        private bool _slotDirty;
        private SkinData _defaultSkin;
        private AnimationData _defaultAnimation;
        private readonly List<string> _animationNames = new List<string>();
        private readonly List<BoneData> _sortedBones = new List<BoneData>();
        private readonly List<SlotData> _sortedSlots = new List<SlotData>();
        private readonly Dictionary<string, List<BoneData>> _bonesChildren = new Dictionary<string, List<BoneData>>();

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
                pair.Value.ReturnToPool();
            }

            foreach (var pair in slots)
            {
                pair.Value.ReturnToPool();
            }

            foreach (var pair in skins)
            {
                pair.Value.ReturnToPool();
            }

            foreach (var pair in animations)
            {
                pair.Value.ReturnToPool();
            }

            foreach (var action in actions)
            {
                action.ReturnToPool();
            }

            frameRate = 0;
            type = ArmatureType.Armature;
            name = null;
            parent = null;
            aabb.Clear();
            actions.Clear();
            bones.Clear();
            slots.Clear();
            skins.Clear();
            animations.Clear();

            cacheFrameRate = 0;
            scale = 1.0f;

            _boneDirty = false;
            _slotDirty = false;
            _defaultSkin = null;
            _defaultAnimation = null;
            _animationNames.Clear();
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

                if (bone.parent != null && !_sortedBones.Contains(bone.parent))
                {
                    continue;
                }

                if (bone.ik != null && !_sortedBones.Contains(bone.ik))
                {
                    continue;
                }

                if (bone.ik != null && bone.chain > 0 && bone.chainIndex == bone.chain)
                {
                    _sortedBones.Insert(_sortedBones.IndexOf(bone.parent) + 1, bone);
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
        public void CacheFrames(uint value)
        {
            if (cacheFrameRate == value)
            {
                return;
            }

            cacheFrameRate = value;

            var frameScale = (float)cacheFrameRate / frameRate;
            foreach (var pair in animations)
            {
                pair.Value.CacheFrames(frameScale);
            }
        }

        /**
         * @private
         */
        public void AddBone(BoneData value, string parentName)
        {
            if (value != null && value.name != null && !bones.ContainsKey(value.name))
            {
                if (parentName != null)
                {
                    var parent = GetBone(parentName);
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
                DragonBones.Warn("");
            }
        }

        /**
         * @private
         */
        public void AddSlot(SlotData value)
        {
            if (value != null && value.name != null && !slots.ContainsKey(value.name))
            {
                slots[value.name] = value;
                _sortedSlots.Add(value);
                _slotDirty = true;
            }
            else
            {
                DragonBones.Warn("");
            }
        }

        /**
         * @private
         */
        public void AddSkin(SkinData value)
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
                DragonBones.Warn("");
            }
        }

        /**
         * @private
         */
        public void AddAnimation(AnimationData value)
        {
            if (value != null && value.name != null && !animations.ContainsKey(value.name))
            {
                animations[value.name] = value;
                _animationNames.Add(value.name);
                if (_defaultAnimation == null)
                {
                    _defaultAnimation = value;
                }
            }
            else
            {
                DragonBones.Warn("");
            }
        }

        /**
         * @language zh_CN
         * 获取指定名称的骨骼数据。
         * @param name 骨骼数据名称。
         * @see dragonBones.BoneData
         * @version DragonBones 3.0
         */
        public BoneData GetBone(string name)
        {
            return bones.ContainsKey(name) ? bones[name] : null;
        }

        /**
         * @language zh_CN
         * 获取指定名称的插槽数据。
         * @param name 插槽数据名称。
         * @see dragonBones.SlotData
         * @version DragonBones 3.0
         */
        public SlotData GetSlot(string name)
        {
            return slots.ContainsKey(name) ? slots[name] : null;
        }

        /**
         * @language zh_CN
         * 获取指定名称的皮肤数据。
         * @param name 皮肤数据名称。
         * @see dragonBones.SkinData
         * @version DragonBones 3.0
         */
        public SkinData GetSkin(string name)
        {
            return DragonBones.IsAvailableString(name) ? (skins.ContainsKey(name) ? skins[name] : null) : _defaultSkin;
        }

        /**
         * @language zh_CN
         * 获取指定名称的动画数据。
         * @param name 动画数据名称。
         * @see dragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public AnimationData GetAnimation(string name)
        {
            return DragonBones.IsAvailableString(name) ? (animations.ContainsKey(name) ? animations[name] : null) : _defaultAnimation;
        }

        /**
         * @private
         */
        public List<BoneData> sortedBones
        {
            get
            {
                if (_boneDirty)
                {
                    _boneDirty = false;
                    _sortBones();
                }

                return _sortedBones;
            }
        }

        /**
         * @private
         */
        public List<SlotData> sortedSlots
        {
            get
            {
                if (_slotDirty)
                {
                    _slotDirty = false;
                    _sortSlots();
                }

                return _sortedSlots;
            }
        }

        /**
         * @language zh_CN
         * 所有的动画数据名称。
         * @see #armatures
         * @version DragonBones 3.0
         */
        public List<string> animationNames
        {
            get { return _animationNames; }
        }

        /**
         * @language zh_CN
         * 获取默认的皮肤数据。
         * @see dragonBones.SkinData
         * @version DragonBones 4.5
         */
        public SkinData defaultSkin
        {
            get { return _defaultSkin; }
        }

        /**
         * @language zh_CN
         * 获取默认的动画数据。
         * @see dragonBones.AnimationData
         * @version DragonBones 4.5
         */
        public AnimationData defaultAnimation
        {
            get { return _defaultAnimation; }
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
        public readonly Transform transform = new Transform();

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
            weight = 0.0f;
            length = 0.0f;
            name = null;
            parent = null;
            ik = null;
            transform.Identity();
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
        public static readonly ColorTransform DEFAULT_COLOR = new ColorTransform();

        /**
         * @private
         */
        public static ColorTransform GenerateColor()
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
        public readonly List<ActionData> actions = new List<ActionData>();

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
                action.ReturnToPool();
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
        public readonly Dictionary<string, SlotDisplayDataSet> slots = new Dictionary<string, SlotDisplayDataSet>();

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
                pair.Value.ReturnToPool();
            }

            name = null;
            slots.Clear();
        }

        /**
         * @private
         */
        public void AddSlot(SlotDisplayDataSet value)
        {
            if (value != null && value.slot != null && !slots.ContainsKey(value.slot.name))
            {
                slots[value.slot.name] = value;
            }
            else
            {
                DragonBones.Warn("");
            }
        }

        /**
         * @private
         */
        public SlotDisplayDataSet GetSlot(string name)
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
        public readonly List<DisplayData> displays = new List<DisplayData>();

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
                display.ReturnToPool();
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
        public readonly Point pivot = new Point();
        public readonly Transform transform = new Transform();

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
                mesh.ReturnToPool();
                mesh = null;
            }

            pivot.Clear();
            transform.Identity();
        }
    }

    /**
     * @private
     */
    public class MeshData : BaseObject
    {
        public bool skinned;
        public readonly Matrix slotPose = new Matrix();

        public readonly List<float> uvs = new List<float>(); // vertices * 2
        public readonly List<float> vertices = new List<float>(); // vertices * 2
        public readonly List<int> vertexIndices = new List<int>(); // triangles * 3

        public readonly List<int[]> boneIndices = new List<int[]>(); // vertices bones
        public readonly List<float[]> weights = new List<float[]>(); // vertices bones
        public readonly List<float[]> boneVertices = new List<float[]>(); // vertices bones * 2

        public readonly List<BoneData> bones = new List<BoneData>(); // bones
        public readonly List<Matrix> inverseBindPose = new List<Matrix>(); // bones

        public MeshData()
        {
        }

        /**
        * @inheritDoc
        */
        protected override void _onClear()
        {
            skinned = false;
            slotPose.Identity();
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