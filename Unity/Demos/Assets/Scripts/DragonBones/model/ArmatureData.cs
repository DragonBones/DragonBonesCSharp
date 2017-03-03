using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 骨架数据。
     * @see DragonBones.Armature
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
        public ArmatureType type;
        /**
         * @private
         */
        public uint cacheFrameRate;
        /**
         * @private
         */
        public float scale;
        /**
         * @language zh_CN
         * 数据名称。
         * @version DragonBones 3.0
         */
        public string name;
        /**
         * @private
         */
        public readonly Rectangle aabb = new Rectangle();
        /**
         * @language zh_CN
         * 所有骨骼数据。
         * @see DragonBones.BoneData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, BoneData> bones = new Dictionary<string, BoneData>();
        /**
         * @language zh_CN
         * 所有插槽数据。
         * @see DragonBones.SlotData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();
        /**
         * @language zh_CN
         * 所有皮肤数据。
         * @see DragonBones.SkinData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, SkinData> skins = new Dictionary<string, SkinData>();
        /**
         * @language zh_CN
         * 所有动画数据。
         * @see DragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public readonly Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();
        /**
         * @private
         */
        public readonly List<ActionData> actions = new List<ActionData>();
        /**
         * @language zh_CN
         * 所属的龙骨数据。
         * @see DragonBones.DragonBonesData
         * @version DragonBones 4.5
         */
        public DragonBonesData parent;
        /**
         * @private
         */
        public CustomData userData;

        private bool _boneDirty;
        private bool _slotDirty;
        private readonly List<string> _animationNames = new List<string>();
        private readonly List<BoneData> _sortedBones = new List<BoneData>();
        private readonly List<SlotData> _sortedSlots = new List<SlotData>();
        private readonly Dictionary<string, List<BoneData>> _bonesChildren = new Dictionary<string, List<BoneData>>();
        private SkinData _defaultSkin;
        private AnimationData _defaultAnimation;
        /**
         * @private
         */
        public ArmatureData()
        {
        }
        /**
         * @private
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

            if (userData != null)
            {
                userData.ReturnToPool();
            }

            frameRate = 0;
            type = ArmatureType.None;
            cacheFrameRate = 0;
            scale = 1.0f;
            name = null;
            aabb.Clear();
            bones.Clear();
            slots.Clear();
            skins.Clear();
            animations.Clear();
            actions.Clear();
            parent = null;

            _boneDirty = false;
            _slotDirty = false;
            _animationNames.Clear();
            _sortedBones.Clear();
            _sortedSlots.Clear();
            _bonesChildren.Clear();
            _defaultSkin = null;
            _defaultAnimation = null;
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
        public void CacheFrames(uint frameRate)
        {
            if (cacheFrameRate > 0)
            {
                return;
            }

            cacheFrameRate = frameRate;
            
            foreach (var animation in animations.Values)
            {
                animation.CacheFrames(cacheFrameRate);
            }
        }
        /**
         * @private
         */
        public int SetCacheFrame(Matrix globalTransformMatrix, Transform transform)
        {
            var dataArray = parent.cachedFrames;
            var arrayOffset = dataArray.Count;

            DragonBones.ResizeList(dataArray, arrayOffset + 10, 0.0f);

            dataArray[arrayOffset] = globalTransformMatrix.a;
            dataArray[arrayOffset + 1] = globalTransformMatrix.b;
            dataArray[arrayOffset + 2] = globalTransformMatrix.c;
            dataArray[arrayOffset + 3] = globalTransformMatrix.d;
            dataArray[arrayOffset + 4] = globalTransformMatrix.tx;
            dataArray[arrayOffset + 5] = globalTransformMatrix.ty;
            dataArray[arrayOffset + 6] = transform.skewX;
            dataArray[arrayOffset + 7] = transform.skewY;
            dataArray[arrayOffset + 8] = transform.scaleX;
            dataArray[arrayOffset + 9] = transform.scaleY;

            return arrayOffset;
        }
        /**
         * @private
         */
        public void GetCacheFrame(Matrix globalTransformMatrix, Transform transform, int arrayOffset)
        {
            var dataArray = parent.cachedFrames;
            globalTransformMatrix.a = dataArray[arrayOffset];
            globalTransformMatrix.b = dataArray[arrayOffset + 1];
            globalTransformMatrix.c = dataArray[arrayOffset + 2];
            globalTransformMatrix.d = dataArray[arrayOffset + 3];
            globalTransformMatrix.tx = dataArray[arrayOffset + 4];
            globalTransformMatrix.ty = dataArray[arrayOffset + 5];
            transform.skewX = dataArray[arrayOffset + 6];
            transform.skewY = dataArray[arrayOffset + 7];
            transform.scaleX = dataArray[arrayOffset + 8];
            transform.scaleY = dataArray[arrayOffset + 9];
            transform.x = globalTransformMatrix.tx;
            transform.y = globalTransformMatrix.ty;
        }
        /**
         * @private
         */
        public void AddBone(BoneData value, string parentName)
        {
            if (value != null && !string.IsNullOrEmpty(value.name) && !bones.ContainsKey(value.name))
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
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public void AddSlot(SlotData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name) && !slots.ContainsKey(value.name))
            {
                slots[value.name] = value;
                _sortedSlots.Add(value);

                _slotDirty = true;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public void AddSkin(SkinData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name) && !skins.ContainsKey(value.name))
            {
                skins[value.name] = value;

                if (_defaultSkin == null)
                {
                    _defaultSkin = value;
                }
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @private
         */
        public void AddAnimation(AnimationData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name) && !animations.ContainsKey(value.name))
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
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }
        /**
         * @language zh_CN
         * 获取指定名称的骨骼数据。
         * @param name 骨骼数据名称。
         * @see DragonBones.BoneData
         * @version DragonBones 3.0
         */
        public BoneData GetBone(string name)
        {
            return (!string.IsNullOrEmpty(name) && bones.ContainsKey(name)) ? bones[name] : null;
        }
        /**
         * @language zh_CN
         * 获取指定名称的插槽数据。
         * @param name 插槽数据名称。
         * @see DragonBones.SlotData
         * @version DragonBones 3.0
         */
        public SlotData GetSlot(string name)
        {
            return (!string.IsNullOrEmpty(name) && slots.ContainsKey(name)) ? slots[name] : null;
        }
        /**
         * @language zh_CN
         * 获取指定名称的皮肤数据。
         * @param name 皮肤数据名称。
         * @see DragonBones.SkinData
         * @version DragonBones 3.0
         */
        public SkinData GetSkin(string name)
        {
            return !string.IsNullOrEmpty(name) ? (skins.ContainsKey(name) ? skins[name] : null) : _defaultSkin;
        }
        /**
         * @language zh_CN
         * 获取指定名称的动画数据。
         * @param name 动画数据名称。
         * @see DragonBones.AnimationData
         * @version DragonBones 3.0
         */
        public AnimationData GetAnimation(string name)
        {
            return !string.IsNullOrEmpty(name) ? (animations.ContainsKey(name) ? animations[name] : null) : _defaultAnimation;
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
         * 所有动画数据名称。
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
         * @see DragonBones.SkinData
         * @version DragonBones 4.5
         */
        public SkinData defaultSkin
        {
            get { return _defaultSkin; }
        }
        /**
         * @language zh_CN
         * 获取默认的动画数据。
         * @see DragonBones.AnimationData
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
     * @see DragonBones.Bone
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
         * @private
         */
        public readonly Transform transform = new Transform();
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
        public CustomData userData;
        /**
         * @private
         */
        public BoneData()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            if (userData != null)
            {
                userData.ReturnToPool();
            }

            inheritTranslation = false;
            inheritRotation = false;
            inheritScale = false;
            bendPositive = false;
            chain = 0;
            chainIndex = 0;
            weight = 0.0f;
            length = 0.0f;
            name = null;
            transform.Identity();
            parent = null;
            ik = null;
            userData = null;
        }
    }
    /**
     * @language zh_CN
     * 插槽数据。
     * @see DragonBones.Slot
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
         * @private
         */
        public readonly List<ActionData> actions = new List<ActionData>();
        /**
         * @language zh_CN
         * 所属的父骨骼数据。
         * @see DragonBones.BoneData
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
        public CustomData userData;
        /**
         * @private
         */
        public SlotData()
        {
        }
        /**
         * @private
         */
        protected override void _onClear()
        {
            for (int i = 0, l = actions.Count; i < l; ++i)
            {
                actions[i].ReturnToPool();
            }

            if (userData != null)
            {
                userData.ReturnToPool();
            }

            displayIndex = 0;
            zOrder = 0;
            blendMode = BlendMode.Normal;
            name = null;
            actions.Clear();
            parent = null;
            color = null;
            userData = null;
        }
    }
    /**
     * @private
     */
    public class SkinData : BaseObject
    {
        public string name;
        public readonly Dictionary<string, SkinSlotData> slots = new Dictionary<string, SkinSlotData>();

        public SkinData()
        {
        }

        protected override void _onClear()
        {
            foreach (var pair in slots)
            {
                pair.Value.ReturnToPool();
            }

            name = null;
            slots.Clear();
        }
        
        public void AddSlot(SkinSlotData value)
        {
            if (value != null && value.slot != null && !slots.ContainsKey(value.slot.name))
            {
                slots[value.slot.name] = value;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }

        public SkinSlotData GetSlot(string name)
        {
            return slots[name];
        }
    }
    /**
     * @private
     */
    public class SkinSlotData : BaseObject
    {
        public readonly List<DisplayData> displays = new List<DisplayData>();
        public readonly Dictionary<string, MeshData> meshs = new Dictionary<string, MeshData>();
        public SlotData slot;

        public SkinSlotData()
        {
        }

        protected override void _onClear()
        {
            for (int i = 0, l = displays.Count; i < l; ++i)
            {
                displays[i].ReturnToPool();
            }

            foreach (var mesh in meshs.Values)
            {
                mesh.ReturnToPool();
            }

            displays.Clear();
            meshs.Clear();
            slot = null;
        }

        public DisplayData GetDisplay(string name)
        {
            for (int i = 0, l = displays.Count; i < l; ++i)
            {
                var display = displays[i];
                if (display.name == name)
                {
                    return display;
                }
            }

            return null;
        }

        public void AddMesh(MeshData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name) && !meshs.ContainsKey(value.name))
            {
                meshs[value.name] = value;
            }
            else
            {
                DragonBones.Assert(false, DragonBones.ARGUMENT_ERROR);
            }
        }

        public MeshData GetMesh(string name)
        {
            return meshs[name];
        }
    }
    /**
     * @private
     */
    public class DisplayData : BaseObject
    {
        public bool isRelativePivot;
        public DisplayType type;
        public bool inheritAnimation;
        public string name;
        public string path;
        public string share;
        public readonly Point pivot = new Point();
        public readonly Transform transform = new Transform();
        public TextureData texture;
        public ArmatureData armature;
        public MeshData mesh;
        public BoundingBoxData boundingBox;

        public DisplayData()
        {
        }

        protected override void _onClear()
        {
            if (boundingBox != null)
            {
                boundingBox.ReturnToPool();
            }

            isRelativePivot = false;
            type = DisplayType.Image;
            name = null;
            path = null;
            share = null;
            pivot.Clear();
            transform.Identity();
            texture = null;
            armature = null;
            mesh = null;
            boundingBox = null;
        }
    }
    /**
     * @private
     */
    public class MeshData : BaseObject
    {
        public bool skinned;
        public string name;
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

        protected override void _onClear()
        {
            skinned = false;
            name = null;
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
    /**
     * Cohen–Sutherland algorithm https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
     * ----------------------
     * | 0101 | 0100 | 0110 |
     * ----------------------
     * | 0001 | 0000 | 0010 |
     * ----------------------
     * | 1001 | 1000 | 1010 |
     * ----------------------
     */
    enum OutCode
    {
        InSide = 0, // 0000
        Left = 1,   // 0001
        Right = 2,  // 0010
        Top = 4,    // 0100
        Bottom = 8  // 1000
    }
    /**
     * @language zh_CN
     * 包围盒数据。
     * @version DragonBones 5.0
     */
    public class BoundingBoxData : BaseObject
    {
        /**
         * Compute the bit code for a point (x, y) using the clip rectangle
         */
        private static OutCode _computeOutCode(float x, float y, float xMin, float yMin, float xMax, float yMax)
        {
            var code = OutCode.InSide;  // initialised as being inside of [[clip window]]

            if (x < xMin)               // to the left of clip window
            {
                code |= OutCode.Left;
            }
            else if (x > xMax)          // to the right of clip window
            {
                code |= OutCode.Right;
            }

            if (y < yMin)               // below the clip window
            {
                code |= OutCode.Top;
            }
            else if (y > yMax)          // above the clip window
            {
                code |= OutCode.Bottom;
            }

            return code;
        }
        /**
         * @private
         */
        public static int SegmentIntersectsRectangle(
            float xA, float yA, float xB, float yB,
            float xMin, float yMin, float xMax, float yMax,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        )
        {
            var inSideA = xA > xMin && xA < xMax && yA > yMin && yA < yMax;
            var inSideB = xB > xMin && xB < xMax && yB > yMin && yB < yMax;

            if (inSideA && inSideB)
            {
                return -1;
            }

            int intersectionCount = 0;
            var outcode0 = _computeOutCode(xA, yA, xMin, yMin, xMax, yMax);
            var outcode1 = _computeOutCode(xB, yB, xMin, yMin, xMax, yMax);

            while (true)
            {
                if ((outcode0 | outcode1) == 0)
                {   // Bitwise OR is 0. Trivially accept and get out of loop
                    intersectionCount = 2;
                    break;
                }
                else if ((outcode0 & outcode1) != 0)
                { // Bitwise AND is not 0. Trivially reject and get out of loop
                    break;
                }

                // failed both tests, so calculate the line segment to clip
                // from an outside point to an intersection with clip edge
                float x = 0.0f;
                float y = 0.0f;
                float normalRadian = 0.0f;

                // At least one endpoint is outside the clip rectangle; pick it.
                var outcodeOut = outcode0 != 0 ? outcode0 : outcode1;

                // Now find the intersection point;
                if ((outcodeOut & OutCode.Top) != 0)
                {             // point is above the clip rectangle
                    x = xA + (xB - xA) * (yMin - yA) / (yB - yA);
                    y = yMin;

                    if (normalRadians != null)
                    {
                        normalRadian = -DragonBones.PI * 0.5f;
                    }
                }
                else if ((outcodeOut & OutCode.Bottom) != 0)
                {     // point is below the clip rectangle
                    x = xA + (xB - xA) * (yMax - yA) / (yB - yA);
                    y = yMax;

                    if (normalRadians != null)
                    {
                        normalRadian = DragonBones.PI * 0.5f;
                    }
                }
                else if ((outcodeOut & OutCode.Right) != 0)
                {      // point is to the right of clip rectangle
                    y = yA + (yB - yA) * (xMax - xA) / (xB - xA);
                    x = xMax;

                    if (normalRadians != null)
                    {
                        normalRadian = 0.0f;
                    }
                }
                else if ((outcodeOut & OutCode.Left) != 0)
                {       // point is to the left of clip rectangle
                    y = yA + (yB - yA) * (xMin - xA) / (xB - xA);
                    x = xMin;

                    if (normalRadians != null)
                    {
                        normalRadian = DragonBones.PI;
                    }
                }

                // Now we move outside point to intersection point to clip
                // and get ready for next pass.
                if (outcodeOut == outcode0)
                {
                    xA = x;
                    yA = y;
                    outcode0 = _computeOutCode(xA, yA, xMin, yMin, xMax, yMax);

                    if (normalRadians != null)
                    {
                        normalRadians.x = normalRadian;
                    }
                }
                else
                {
                    xB = x;
                    yB = y;
                    outcode1 = _computeOutCode(xB, yB, xMin, yMin, xMax, yMax);

                    if (normalRadians != null)
                    {
                        normalRadians.y = normalRadian;
                    }
                }
            }

            if (intersectionCount > 0)
            {
                if (inSideA)
                {
                    intersectionCount = 2; // 10

                    if (intersectionPointA != null)
                    {
                        intersectionPointA.x = xB;
                        intersectionPointA.y = yB;
                    }

                    if (intersectionPointB != null)
                    {
                        intersectionPointB.x = xB;
                        intersectionPointB.y = xB;
                    }

                    if (normalRadians != null)
                    {
                        normalRadians.x = normalRadians.y + DragonBones.PI;
                    }
                }
                else if (inSideB)
                {
                    intersectionCount = 1; // 01

                    if (intersectionPointA != null)
                    {
                        intersectionPointA.x = xA;
                        intersectionPointA.y = yA;
                    }

                    if (intersectionPointB != null)
                    {
                        intersectionPointB.x = xA;
                        intersectionPointB.y = yA;
                    }

                    if (normalRadians != null)
                    {
                        normalRadians.y = normalRadians.x + DragonBones.PI;
                    }
                }
                else
                {
                    intersectionCount = 3; // 11
                    if (intersectionPointA != null)
                    {
                        intersectionPointA.x = xA;
                        intersectionPointA.y = yA;
                    }

                    if (intersectionPointB != null)
                    {
                        intersectionPointB.x = xB;
                        intersectionPointB.y = yB;
                    }
                }
            }

            return intersectionCount;
        }
        /**
         * @private
         */
        public static int SegmentIntersectsEllipse(
            float xA, float yA, float xB, float yB,
            float xC, float yC, float widthH, float heightH,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        )
        {
            var d = widthH / heightH;
            var dd = d * d;

            yA *= d;
            yB *= d;

            var dX = xB - xA;
            var dY = yB - yA;
            var lAB = (float)Math.Sqrt(dX * dX + dY * dY);
            var xD = dX / lAB;
            var yD = dY / lAB;
            var a = (xC - xA) * xD + (yC - yA) * yD;
            var aa = a * a;
            var ee = xA * xA + yA * yA;
            var rr = widthH * widthH;
            var dR = rr - ee + aa;
            int intersectionCount = 0;

            if (dR >= 0)
            {
                var dT = (float)Math.Sqrt(dR);
                var sA = a - dT;
                var sB = a + dT;
                int inSideA = sA < 0.0f ? -1 : (sA <= lAB ? 0 : 1);
                int inSideB = sB < 0.0f ? -1 : (sB <= lAB ? 0 : 1);
                int sideAB = inSideA * inSideB;

                if (sideAB < 0)
                {
                    return -1;
                }
                else if (sideAB == 0)
                {
                    if (inSideA == -1)
                    {
                        intersectionCount = 2; // 10
                        xB = xA + sB * xD;
                        yB = (yA + sB * yD) / d;

                        if (intersectionPointA != null)
                        {
                            intersectionPointA.x = xB;
                            intersectionPointA.y = yB;
                        }

                        if (intersectionPointB != null)
                        {
                            intersectionPointB.x = xB;
                            intersectionPointB.y = yB;
                        }

                        if (normalRadians != null)
                        {
                            normalRadians.x = (float)Math.Atan2(yB / rr * dd, xB / rr);
                            normalRadians.y = normalRadians.x + DragonBones.PI;
                        }
                    }
                    else if (inSideB == 1)
                    {
                        intersectionCount = 1; // 01
                        xA = xA + sA * xD;
                        yA = (yA + sA * yD) / d;

                        if (intersectionPointA != null)
                        {
                            intersectionPointA.x = xA;
                            intersectionPointA.y = yA;
                        }

                        if (intersectionPointB != null)
                        {
                            intersectionPointB.x = xA;
                            intersectionPointB.y = yA;
                        }

                        if (normalRadians != null)
                        {
                            normalRadians.x = (float)Math.Atan2(yA / rr * dd, xA / rr);
                            normalRadians.y = normalRadians.x + DragonBones.PI;
                        }
                    }
                    else
                    {
                        intersectionCount = 3; // 11

                        if (intersectionPointA != null)
                        {
                            intersectionPointA.x = xA + sA * xD;
                            intersectionPointA.y = (yA + sA * yD) / d;

                            if (normalRadians != null)
                            {
                                normalRadians.x = (float)Math.Atan2(intersectionPointA.y / rr * dd, intersectionPointA.x / rr);
                            }
                        }

                        if (intersectionPointB != null)
                        {
                            intersectionPointB.x = xA + sB * xD;
                            intersectionPointB.y = (yA + sB * yD) / d;

                            if (normalRadians != null)
                            {
                                normalRadians.y = (float)Math.Atan2(intersectionPointB.y / rr * dd, intersectionPointB.x / rr);
                            }
                        }
                    }
                }
            }

            return intersectionCount;
        }
        /**
         * @private
         */
        public static int SegmentIntersectsPolygon(
            float xA, float yA, float xB, float yB,
            List<float> vertices,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        )
        {
            if (xA == xB)
            {
                xA = xB + 0.01f;
            }

            if (yA == yB)
            {
                yA = yB + 0.01f;
            }

            var l = vertices.Count;
            var dXAB = xA - xB;
            var dYAB = yA - yB;
            var llAB = xA * yB - yA * xB;
            int intersectionCount = 0;
            var xC = vertices[l - 2];
            var yC = vertices[l - 1];
            var dMin = 0.0f;
            var dMax = 0.0f;
            var xMin = 0.0f;
            var yMin = 0.0f;
            var xMax = 0.0f;
            var yMax = 0.0f;

            for (int i = 0; i < l; i += 2)
            {
                var xD = vertices[i];
                var yD = vertices[i + 1];

                if (xC == xD)
                {
                    xC = xD + 0.01f;
                }

                if (yC == yD)
                {
                    yC = yD + 0.01f;
                }

                var dXCD = xC - xD;
                var dYCD = yC - yD;
                var llCD = xC * yD - yC * xD;
                var ll = dXAB * dYCD - dYAB * dXCD;
                var x = (llAB * dXCD - dXAB * llCD) / ll;

                if (((x >= xC && x <= xD) || (x >= xD && x <= xC)) && (dXAB == 0 || (x >= xA && x <= xB) || (x >= xB && x <= xA)))
                {
                    var y = (llAB * dYCD - dYAB * llCD) / ll;
                    if (((y >= yC && y <= yD) || (y >= yD && y <= yC)) && (dYAB == 0 || (y >= yA && y <= yB) || (y >= yB && y <= yA)))
                    {
                        if (intersectionPointB != null)
                        {
                            var d = x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intersectionCount == 0)
                            {
                                dMin = d;
                                dMax = d;
                                xMin = x;
                                yMin = y;
                                xMax = x;
                                yMax = y;

                                if (normalRadians != null)
                                {
                                    normalRadians.x = (float)Math.Atan2(yD - yC, xD - xC) - DragonBones.PI * 0.5f;
                                    normalRadians.y = normalRadians.x;
                                }
                            }
                            else
                            {
                                if (d < dMin)
                                {
                                    dMin = d;
                                    xMin = x;
                                    yMin = y;

                                    if (normalRadians != null)
                                    {
                                        normalRadians.x = (float)Math.Atan2(yD - yC, xD - xC) - DragonBones.PI * 0.5f;
                                    }
                                }

                                if (d > dMax)
                                {
                                    dMax = d;
                                    xMax = x;
                                    yMax = y;

                                    if (normalRadians != null)
                                    {
                                        normalRadians.y = (float)Math.Atan2(yD - yC, xD - xC) - DragonBones.PI * 0.5f;
                                    }
                                }
                            }

                            intersectionCount++;
                        }
                        else
                        {
                            xMin = x;
                            yMin = y;
                            xMax = x;
                            yMax = y;
                            intersectionCount++;

                            if (normalRadians != null)
                            {
                                normalRadians.x = (float)Math.Atan2(yD - yC, xD - xC) - DragonBones.PI * 0.5f;
                                normalRadians.y = normalRadians.x;
                            }
                            break;
                        }
                    }
                }

                xC = xD;
                yC = yD;
            }

            if (intersectionCount == 1)
            {
                if (intersectionPointA != null)
                {
                    intersectionPointA.x = xMin;
                    intersectionPointA.y = yMin;
                }

                if (intersectionPointB != null)
                {
                    intersectionPointB.x = xMin;
                    intersectionPointB.y = yMin;
                }

                if (normalRadians != null)
                {
                    normalRadians.y = normalRadians.x + DragonBones.PI;
                }
            }
            else if (intersectionCount > 1)
            {
                intersectionCount++;

                if (intersectionPointA != null)
                {
                    intersectionPointA.x = xMin;
                    intersectionPointA.y = yMin;
                }

                if (intersectionPointB != null)
                {
                    intersectionPointB.x = xMax;
                    intersectionPointB.y = yMax;
                }
            }

            return intersectionCount;
        }
        /**
         * @language zh_CN
         * 包围盒类型。
         * @see DragonBones.BoundingBoxType
         * @version DragonBones 5.0
         */
        public BoundingBoxType type;
        /**
         * @language zh_CN
         * 包围盒颜色。
         * @version DragonBones 5.0
         */
        public uint color;

        public float x;
        public float y;
        public float width;
        public float height;
        /**
         * @language zh_CN
         * 自定义多边形顶点。
         * @version DragonBones 5.0
         */
        public readonly List<float> vertices = new List<float>();
        /**
         * @private
         */
        public BoundingBoxData()
        {
        }
        /**
         * @private
         */
        override protected void _onClear()
        {
            type = BoundingBoxType.None;
            color = 0x000000;
            x = 0.0f;
            y = 0.0f;
            width = 0.0f;
            height = 0.0f;
            vertices.Clear();
        }
        /**
         * @language zh_CN
         * 是否包含点。
         * @version DragonBones 5.0
         */
        public bool ContainsPoint(float pX, float pY)
        {
            var isInSide = false;

            if (type == BoundingBoxType.Polygon)
            {
                if (pX >= x && pX <= width && pY >= y && pY <= height)
                {
                    for (int i = 0, l = vertices.Count, prevIndex = l - 2; i < l; i += 2)
                    {
                        var yA = vertices[prevIndex + 1];
                        var yB = vertices[i + 1];
                        if ((yB < pY && yA >= pY) || (yA < pY && yB >= pY))
                        {
                            var xA = vertices[prevIndex];
                            var xB = vertices[i];
                            if ((pY - yB) * (xA - xB) / (yA - yB) + xB < pX)
                            {
                                isInSide = !isInSide;
                            }
                        }

                        prevIndex = i;
                    }
                }
            }
            else
            {
                var widthH = width * 0.5f;
                if (pX >= -widthH && pX <= widthH)
                {
                    var heightH = height * 0.5f;
                    if (pY >= -heightH && pY <= heightH)
                    {
                        if (type == BoundingBoxType.Ellipse)
                        {
                            pY *= widthH / heightH;
                            isInSide = (float)Math.Sqrt(pX * pX + pY * pY) <= widthH;
                        }
                        else
                        {
                            isInSide = true;
                        }
                    }
                }
            }

            return isInSide;
        }
        /**
         * @language zh_CN
         * 是否与线段相交。
         * @version DragonBones 5.0
         */
        public int IntersectsSegment(
            float xA, float yA, float xB, float yB,
            Point intersectionPointA = null,
            Point intersectionPointB = null,
            Point normalRadians = null
        )
        {
            int intersectionCount = 0;
            switch (type)
            {
                case BoundingBoxType.Rectangle:
                    var widthH = width * 0.5f;
                    var heightH = height * 0.5f;
                    intersectionCount = SegmentIntersectsRectangle(
                        xA, yA, xB, yB,
                        -widthH, -heightH, widthH, heightH,
                        intersectionPointA, intersectionPointB, normalRadians
                    );
                    break;

                case BoundingBoxType.Ellipse:
                    intersectionCount = SegmentIntersectsEllipse(
                        xA, yA, xB, yB,
                        0.0f, 0.0f, width * 0.5f, height * 0.5f,
                        intersectionPointA, intersectionPointB, normalRadians
                    );
                    break;

                case BoundingBoxType.Polygon:
                    if (SegmentIntersectsRectangle(xA, yA, xB, yB, x, y, width, height, null, null) != 0)
                    {
                        intersectionCount = SegmentIntersectsPolygon(
                            xA, yA, xB, yB,
                            vertices,
                            intersectionPointA, intersectionPointB, normalRadians
                        );
                    }
                    break;

                default:
                    break;
            }

            return intersectionCount;
        }
    }
}