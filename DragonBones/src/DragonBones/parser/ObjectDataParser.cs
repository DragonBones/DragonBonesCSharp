using System;
using System.Collections.Generic;
using System.Text;

namespace DragonBones
{
    public class ObjectDataParser : DataParser
    {
        protected static bool _GetBoolean(Dictionary<string, object> rawData, string key, bool defaultValue)
        {
            if (rawData.ContainsKey(key))
            {
                var value = rawData[key];
                if (value is bool)
                {
                    return (bool)value;
                }
                else if (value is string)
                {
                    switch (value as string)
                    {
                        case "0":
                        case "NaN":
                        case "":
                        case "false":
                        case "null":
                        case "undefined":
                            return false;

                        default:
                            return true;
                    }
                }
                else
                {
                    return Convert.ToBoolean(value);
                }
            }

            return defaultValue;
        }

        /**
         * @private
         */
        protected static uint _GetNumber(Dictionary<string, object> rawData, string key, uint defaultValue)
        {
            if (rawData.ContainsKey(key))
            {
                var value = rawData[key];

                if (value == null)
                {
                    return defaultValue;
                }

                if (value is uint)
                {
                    return (uint)value;
                }

                return Convert.ToUInt32(value);

            }

            return defaultValue;
        }

        /**
         * @private
         */
        protected static int _GetNumber(Dictionary<string, object> rawData, string key, int defaultValue)
        {
            if (rawData.ContainsKey(key))
            {
                var value = rawData[key];

                if (value == null)
                {
                    return defaultValue;
                }

                if (value is int)
                {
                    return (int)value;
                }

                return Convert.ToInt32(value);
            }

            return defaultValue;
        }
        /**
         * @private
         */
        protected static float _GetNumber(Dictionary<string, object> rawData, string key, float defaultValue)
        {
            if (rawData.ContainsKey(key))
            {
                var value = rawData[key];

                if (value == null)
                {
                    return defaultValue;
                }

                if (value is float)
                {
                    return (float)value;
                }

                return Convert.ToSingle(value);
            }

            return defaultValue;
        }

        /**
         * @private
         */
        protected static string _GetString(Dictionary<string, object> rawData, string key, string defaultValue)
        {
            if (rawData.ContainsKey(key))
            {
                var value = rawData[key];
                if (value is string)
                {
                    return (string)value;
                }

                return Convert.ToString(value);
            }

            return defaultValue;
        }

        protected uint _rawTextureAtlasIndex = 0;
        protected readonly List<BoneData> _rawBones = new List<BoneData>();
        protected DragonBonesData _data = null; //
        protected ArmatureData _armature = null; //
        protected BoneData _bone = null; //
        protected SlotData _slot = null; //
        protected SkinData _skin = null; //
        protected MeshDisplayData _mesh = null; //
        protected AnimationData _animation = null; //
        protected TimelineData _timeline = null; //
        protected List<object> _rawTextureAtlases = null;

        private int _defalultColorOffset = -1;
        private int _prevClockwise = 0;
        private float _prevRotation = 0.0f;
        private readonly Matrix _helpMatrixA = new Matrix();
        private readonly Matrix _helpMatrixB = new Matrix();
        private readonly Transform _helpTransform = new Transform();
        private readonly ColorTransform _helpColorTransform = new ColorTransform();
        private readonly Point _helpPoint = new Point();
        private readonly List<float> _helpArray = new List<float>();
        private readonly List<int> _intArray = new List<int>();
        private readonly List<float> _floatArray = new List<float>();
        private readonly List<int> _frameIntArray = new List<int>();
        private readonly List<float> _frameFloatArray = new List<float>();
        private readonly List<int> _frameArray = new List<int>();
        private readonly List<int> _timelineArray = new List<int>();
        private readonly List<ActionFrame> _actionFrames = new List<ActionFrame>();
        private readonly Dictionary<string, List<float>> _weightSlotPose = new Dictionary<string, List<float>>();
        private readonly Dictionary<string, List<float>> _weightBonePoses = new Dictionary<string, List<float>>();
        private readonly Dictionary<string, List<uint>> _weightBoneIndices = new Dictionary<string, List<uint>>();
        private readonly Dictionary<string, List<BoneData>> _cacheBones = new Dictionary<string, List<BoneData>>();
        private readonly Dictionary<string, MeshDisplayData> _meshs = new Dictionary<string, MeshDisplayData>();
        private readonly Dictionary<string, List<MeshDisplayData>> _shareMeshs = new Dictionary<string, List<MeshDisplayData>>();
        private readonly Dictionary<string, List<ActionData>> _slotChildActions = new Dictionary<string, List<ActionData>>();

        public ObjectDataParser()
        {

        }

        /**
         * @private
         */
        private void _GetCurvePoint(float x1, float y1,
                                    float x2, float y2,
                                    float x3, float y3,
                                    float x4, float y4,
                                    float t, Point result)
        {
            var l_t = 1.0f - t;
            var powA = l_t * l_t;
            var powB = t * t;
            var kA = l_t * powA;
            var kB = 3.0f * t * powA;
            var kC = 3.0f * l_t * powB;
            var kD = t * powB;

            result.x = kA* x1 + kB* x2 + kC* x3 + kD* x4;
            result.y = kA* y1 + kB* y2 + kC* y3 + kD* y4;
        }

        /**
         * @private
         */
        private void _SamplingEasingCurve(float[] curve, float[] samples)
        {
            var curveCount = curve.Length;
            var stepIndex = -2;
            for (int i = 0, l = samples.Length; i < l; ++i)
            {
                var t = (i + 1) / (l + 1);
                while ((stepIndex + 6 < curveCount ? curve[stepIndex + 6] : 1) < t)
                {
                    // stepIndex + 3 * 2
                    stepIndex += 6;
                }

                var isInCurve = stepIndex >= 0 && stepIndex + 6 < curveCount;
                var x1 = isInCurve ? curve[stepIndex] : 0.0f;
                var y1 = isInCurve ? curve[stepIndex + 1] : 0.0f;
                var x2 = curve[stepIndex + 2];
                var y2 = curve[stepIndex + 3];
                var x3 = curve[stepIndex + 4];
                var y3 = curve[stepIndex + 5];
                var x4 = isInCurve ? curve[stepIndex + 6] : 1.0f;
                var y4 = isInCurve ? curve[stepIndex + 7] : 1.0f;

                var lower = 0.0f;
                var higher = 1.0f;
                while (higher - lower > 0.0001f)
                {
                    var percentage = (higher + lower) * 0.5f;
                    this._GetCurvePoint(x1, y1, x2, y2, x3, y3, x4, y4, percentage, this._helpPoint);
                    if (t - this._helpPoint.x > 0.0)
                    {
                        lower = percentage;
                    }
                    else
                    {
                        higher = percentage;
                    }
                }

                samples[i] = this._helpPoint.y;
            }
        }
        /**
         * @private
         */
        private int _SortActionFrame(ActionFrame a, ActionFrame b)
        {
            return a.frameStart > b.frameStart? 1 : -1;
        }
        /**
         * @private
         */
        private void _ParseActionDataInFrame(object rawData, int frameStart, BoneData bone = null, SlotData slot = null)
        {
            Dictionary<string, object> rawDic = rawData as Dictionary<string, object>;
            if (rawDic == null)
            {
                return;
            }

            if (rawDic.ContainsKey(ObjectDataParser.EVENT))
            {
                this._MergeActionFrame(rawDic[ObjectDataParser.EVENT], frameStart, ActionType.Frame, bone, slot);
            }

            if (rawDic.ContainsKey(ObjectDataParser.SOUND)) 
            {
                this._MergeActionFrame(rawDic[ObjectDataParser.SOUND], frameStart, ActionType.Sound, bone, slot);
            }

            if (rawDic.ContainsKey(ObjectDataParser.ACTION))
            {
                this._MergeActionFrame(rawDic[ObjectDataParser.ACTION], frameStart, ActionType.Play, bone, slot);
            }

            if (rawDic.ContainsKey(ObjectDataParser.EVENTS)) 
            {
                this._MergeActionFrame(rawDic[ObjectDataParser.EVENTS], frameStart, ActionType.Frame, bone, slot);
            }

            if (rawDic.ContainsKey(ObjectDataParser.ACTIONS))
            {
                this._MergeActionFrame(rawDic[ObjectDataParser.ACTIONS], frameStart, ActionType.Play, bone, slot);
            }
        }
        /**
         * @private
         */
        private void _MergeActionFrame(object rawData, int frameStart, ActionType type, BoneData bone = null, SlotData slot = null)
        {
            var actionOffset = this._armature.actions.Count;
            var actionCount = this._ParseActionData(rawData, this._armature.actions, type, bone, slot);
            ActionFrame frame = null;

            if (this._actionFrames.Count == 0)
            { 
                // First frame.
                frame = new ActionFrame();
                frame.frameStart = 0;
                this._actionFrames.Add(frame);
                frame = null;
            }

            foreach (var eachFrame in this._actionFrames)
            {
                // Get same frame.
                if (eachFrame.frameStart == frameStart)
                {
                    frame = eachFrame;
                    break;
                }
            }

            if (frame == null)
            { 
                // Create and cache frame.
                frame = new ActionFrame();
                frame.frameStart = frameStart;
                this._actionFrames.Add(frame);
            }

            for (var i = 0; i < actionCount; ++i)
            { 
                // Cache action offsets.
                frame.actions.Add(actionOffset + i);
            }
        }

        private int _ParseCacheActionFrame(ActionFrame frame)
        {
            var frameOffset = this._frameArray.Count;
            var actionCount = frame.actions.Count;
            DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1 + 1 + actionCount, 0);
            this._frameArray[frameOffset + (int)BinaryOffset.FramePosition] = frame.frameStart;
            this._frameArray[frameOffset + (int)BinaryOffset.FramePosition + 1] = actionCount; // Action count.

            for (var i = 0; i < actionCount; ++i)
            { 
                // Action offsets.
                this._frameArray[frameOffset + (int)BinaryOffset.FramePosition + 2 + i] = frame.actions[i];
            }

            return frameOffset;
        }

        private ArmatureData _ParseArmature(Dictionary<string, object> rawData, float scale)
        {

            var armature = BaseObject.BorrowObject<ArmatureData>();
            armature.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, "");
            armature.frameRate = ObjectDataParser._GetNumber(rawData, ObjectDataParser.FRAME_RATE, this._data.frameRate);
            armature.scale = scale;

            if (rawData.ContainsKey(ObjectDataParser.TYPE) && rawData[ObjectDataParser.TYPE] is string)
            {
                armature.type = ObjectDataParser._GetArmatureType((string)rawData[ObjectDataParser.TYPE]);
            }
            else
            {
                armature.type = (ArmatureType)ObjectDataParser._GetNumber(rawData, ObjectDataParser.TYPE.ToString(), (int)ArmatureType.Armature);
            }

            if (armature.frameRate == 0)
            { 
                // Data error.
                armature.frameRate = 24;
            }

            this._armature = armature;

            if (rawData.ContainsKey(ObjectDataParser.AABB)) 
            {
                var rawAABB = rawData[AABB] as Dictionary<string, object>;
                armature.aabb.x = ObjectDataParser._GetNumber(rawAABB, ObjectDataParser.X, 0.0f);
                armature.aabb.y = ObjectDataParser._GetNumber(rawAABB, ObjectDataParser.Y, 0.0f);
                armature.aabb.width = ObjectDataParser._GetNumber(rawAABB, ObjectDataParser.WIDTH, 0.0f);
                armature.aabb.height = ObjectDataParser._GetNumber(rawAABB, ObjectDataParser.HEIGHT, 0.0f);
            }

            //CANVAS功能为完全实现，这里先注释
            /*if (rawDic != null && rawDic.ContainsKey(ObjectDataParser.CANVAS)) 
            {
                var rawCanvas = rawDic[ObjectDataParser.CANVAS];
                var canvas = BaseObject.BorrowObject<CanvasData>();

                if (rawDic.ContainsKey(ObjectDataParser.COLOR)) 
                {
                    ObjectDataParser._GetNumber(rawCanvas, ObjectDataParser.COLOR, 0);
                    canvas.hasBackground = true;
                }
                else 
{
                    canvas.hasBackground = false;
                }

                canvas.color = ObjectDataParser._GetNumber(rawCanvas, ObjectDataParser.COLOR, 0);
                canvas.x = ObjectDataParser._GetNumber(rawCanvas, ObjectDataParser.X, 0);
                canvas.y = ObjectDataParser._GetNumber(rawCanvas, ObjectDataParser.Y, 0);
                canvas.width = ObjectDataParser._GetNumber(rawCanvas, ObjectDataParser.WIDTH, 0);
                canvas.height = ObjectDataParser._GetNumber(rawCanvas, ObjectDataParser.HEIGHT, 0);

                armature.canvas = canvas;
            }*/

            if (rawData.ContainsKey(ObjectDataParser.BONE))
            {
                var rawBones = rawData[ObjectDataParser.BONE] as List<object>;
                foreach (Dictionary<string, object> rawBone in rawBones)
                {
                    var parentName = ObjectDataParser._GetString(rawBone, ObjectDataParser.PARENT, "");
                    var bone = this._ParseBone(rawBone);

                    if (parentName.Length > 0)
                    { 
                        // Get bone parent.
                        var parent = armature.GetBone(parentName);
                        if (parent != null)
                        {
                            bone.parent = parent;
                        }
                        else
                        {
                            // Cache.
                            if (!this._cacheBones.ContainsKey(parentName))
                            {
                                this._cacheBones[parentName] = new List<BoneData>();
                            }

                            this._cacheBones[parentName].Add(bone);
                        }
                    }

                    if (this._cacheBones.ContainsKey(bone.name)) 
                    {
                        foreach (var child in this._cacheBones[bone.name])
                        {
                            child.parent = bone;
                        }

                        this._cacheBones[bone.name].Clear();
                    }

                    armature.AddBone(bone);

                    this._rawBones.Add(bone); // Raw bone sort.
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.IK))
            {
                var rawIKS = rawData[ObjectDataParser.IK] as List<object>;
                foreach (Dictionary<string, object> rawIK in rawIKS)
                {
                    this._ParseIKConstraint(rawIK);
                }
            }

            armature.SortBones();

            if (rawData.ContainsKey(ObjectDataParser.SLOT)) 
            {
                var rawSlots = rawData[ObjectDataParser.SLOT] as List<object>;
                foreach (Dictionary<string, object> rawSlot in rawSlots) 
                {
                    armature.AddSlot(this._ParseSlot(rawSlot));
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.SKIN))
            {
                var rawSkins = rawData[ObjectDataParser.SKIN] as List<object>;
                foreach (Dictionary<string, object> rawSkin in rawSkins)
                {
                    armature.AddSkin(this._ParseSkin(rawSkin));
                }
            }

            foreach (var meshName in this._shareMeshs.Keys) 
            {
                var meshs = this._shareMeshs[meshName];
                foreach (var meshDisplay in meshs)
                {
                    var shareMesh = this._meshs[meshName];
                    meshDisplay.offset = shareMesh.offset;
                    meshDisplay.weight = shareMesh.weight;
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.ANIMATION))
            {
                var rawAnimations = rawData[ObjectDataParser.ANIMATION] as List<object>;
                foreach (Dictionary<string, object> rawAnimation in rawAnimations)
                {
                    var animation = this._ParseAnimation(rawAnimation);
                    armature.AddAnimation(animation);
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.DEFAULT_ACTIONS))
            {
                this._ParseActionData(rawData[ObjectDataParser.DEFAULT_ACTIONS], armature.defaultActions, ActionType.Play, null, null);
            }

            if (rawData.ContainsKey(ObjectDataParser.ACTIONS))
            {
                this._ParseActionData(rawData[ObjectDataParser.ACTIONS], armature.actions, ActionType.Play, null, null);
            }

            // for (const action of armature.defaultActions) { // Set default animation from default action.
            for (var i = 0; i < armature.defaultActions.Count; ++i)
            {
                var action = armature.defaultActions[i];
                if (action.type == ActionType.Play)
                {
                    var animation = armature.GetAnimation(action.name);
                    if (animation != null)
                    {
                        armature.defaultAnimation = animation;
                    }
                    break;
                }
            }

            // Clear helper.
            this._armature = null;
            this._rawBones.Clear();
            this._meshs.Clear();
            this._shareMeshs.Clear();
            this._cacheBones.Clear();
            this._slotChildActions.Clear();
            this._weightSlotPose.Clear();
            this._weightBonePoses.Clear();
            this._weightBoneIndices.Clear();

            return armature;
        }
        /**
         * @private
         */
        protected BoneData _ParseBone(Dictionary<string, object> rawData)
        {
            var bone = BaseObject.BorrowObject<BoneData>();
            bone.inheritTranslation = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.INHERIT_TRANSLATION, true);
            bone.inheritRotation = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.INHERIT_ROTATION, true);
            bone.inheritScale = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.INHERIT_SCALE, true);
            bone.inheritReflection = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.INHERIT_REFLECTION, true);
            bone.length = ObjectDataParser._GetNumber(rawData, ObjectDataParser.LENGTH, 0) * this._armature.scale;
            bone.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, "");
            
            if (rawData.ContainsKey(ObjectDataParser.TRANSFORM)) 
            {
                this._ParseTransform(rawData[ObjectDataParser.TRANSFORM] as Dictionary<string, object>, bone.transform, this._armature.scale);

            }

            return bone;
        }
        /**
         * @private
         */
        protected void _ParseIKConstraint(Dictionary<string, object> rawData)
        {
            var bone = _armature.GetBone(_GetString(rawData, rawData.ContainsKey(BONE) ? BONE : NAME, null));
            if (bone == null)
            {
                return;
            }

            var target = this._armature.GetBone(ObjectDataParser._GetString(rawData, ObjectDataParser.TARGET, ""));
            if (target == null)
            {
                return;
            }

            var constraint = BaseObject.BorrowObject<IKConstraintData>();
            constraint.bendPositive = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.BEND_POSITIVE, true);
            constraint.scaleEnabled = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.SCALE, false);
            constraint.weight = ObjectDataParser._GetNumber(rawData, ObjectDataParser.WEIGHT, 1.0f);
            constraint.bone = bone;
            constraint.target = target;

            var chain = ObjectDataParser._GetNumber(rawData, ObjectDataParser.CHAIN, 0);
            if (chain > 0)
            {
                constraint.root = bone.parent;
            }

            bone.constraints.Add(constraint);
        }

        private SlotData _ParseSlot(Dictionary<string, object> rawData)
        {
            var slot = BaseObject.BorrowObject<SlotData>();
            slot.displayIndex = ObjectDataParser._GetNumber(rawData, ObjectDataParser.DISPLAY_INDEX, 0);
            slot.zOrder = this._armature.sortedSlots.Count;
            slot.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, "");
            slot.parent = this._armature.GetBone(ObjectDataParser._GetString(rawData, ObjectDataParser.PARENT, "")); //

            if (rawData.ContainsKey(ObjectDataParser.BLEND_MODE) && rawData[ObjectDataParser.BLEND_MODE] is string)
            {
                slot.blendMode = ObjectDataParser._GetBlendMode((string)rawData[ObjectDataParser.BLEND_MODE]);
            }
            else
            {
                slot.blendMode = (BlendMode)ObjectDataParser._GetNumber(rawData, ObjectDataParser.BLEND_MODE, (int)BlendMode.Normal);
            }

            if (rawData.ContainsKey(ObjectDataParser.COLOR)) 
            {
                slot.color = SlotData.CreateColor();
                this._ParseColorTransform(rawData[ObjectDataParser.COLOR] as Dictionary<string, object>, slot.color);
            }
            else
            {
                slot.color = SlotData.DEFAULT_COLOR;
            }

            if (rawData.ContainsKey(ObjectDataParser.ACTIONS))
            {
                var actions = this._slotChildActions[slot.name] = new List<ActionData>();
                this._ParseActionData(rawData[ObjectDataParser.ACTIONS] as Dictionary<string, object>, actions, ActionType.Play, null, null);
            }

            return slot;
        }

        protected SkinData _ParseSkin(Dictionary<string, object> rawData)
        {
            var skin = BaseObject.BorrowObject<SkinData>();
            skin.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, ObjectDataParser.DEFAULT_NAME);
            if (skin.name.Length == 0)
            {
                skin.name = ObjectDataParser.DEFAULT_NAME;
            }

            if (rawData.ContainsKey(ObjectDataParser.SLOT))
            {
                this._skin = skin;
                var rawSlots = rawData[ObjectDataParser.SLOT] as List<object>;

                foreach (Dictionary<string, object> rawSlot in rawSlots)
                {
                    var slotName = ObjectDataParser._GetString(rawSlot, ObjectDataParser.NAME, "");
                    var slot = this._armature.GetSlot(slotName);
                    if (slot != null)
                    {
                        this._slot = slot;

                        if (rawSlot.ContainsKey(ObjectDataParser.DISPLAY)) 
                        {
                            var rawDisplays = rawSlot[ObjectDataParser.DISPLAY] as List<object>;
                            foreach (Dictionary<string, object> rawDisplay in rawDisplays)
                            {
                                skin.AddDisplay(slotName, this._ParseDisplay(rawDisplay));
                            }
                        }

                        this._slot = null; //
                    }
                }

                this._skin = null;
            }

            return skin;
        }

        /**
         * @private
         */
        protected DisplayData _ParseDisplay(Dictionary<string, object> rawData)
        {
            DisplayData display = null;
            var name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, "");
            var path = ObjectDataParser._GetString(rawData, ObjectDataParser.PATH, "");
            var type = DisplayType.Image;

            if (rawData.ContainsKey(ObjectDataParser.TYPE) && rawData[ObjectDataParser.TYPE] is string)
            {
                type = ObjectDataParser._GetDisplayType((string)rawData[ObjectDataParser.TYPE]);
            }
            else
            {
                type = (DisplayType)ObjectDataParser._GetNumber(rawData, ObjectDataParser.TYPE, (int)type);
            }

            switch (type)
            {
                case DisplayType.Image:
                    var imageDisplay = BaseObject.BorrowObject<ImageDisplayData>();
                    display = imageDisplay;
                    imageDisplay.name = name;
                    imageDisplay.path = path.Length > 0 ? path : name;
                    this._ParsePivot(rawData, imageDisplay);
                    break;
                case DisplayType.Armature:
                    var armatureDisplay = BaseObject.BorrowObject<ArmatureDisplayData>();
                    display = armatureDisplay;
                    armatureDisplay.name = name;
                    armatureDisplay.path = path.Length > 0 ? path : name;
                    armatureDisplay.inheritAnimation = true;

                    if (rawData.ContainsKey(ObjectDataParser.ACTIONS)) 
                    {
                        this._ParseActionData(rawData[ObjectDataParser.ACTIONS], armatureDisplay.actions, ActionType.Play, null, null);
                    }
                    else if (this._slotChildActions.ContainsKey(this._slot.name))
                    {
                        var displays = this._skin.GetDisplays(this._slot.name);
                        if (displays == null ? this._slot.displayIndex == 0 : this._slot.displayIndex == displays.Count)
                        {
                            foreach (var action in this._slotChildActions[this._slot.name])
                            {
                                armatureDisplay.actions.Add(action);
                            }

                            this._slotChildActions[this._slot.name].Clear();
                        }
                    }
                    break;

                case DisplayType.Mesh:
                    var meshDisplay = BaseObject.BorrowObject<MeshDisplayData>();
                    display = meshDisplay;
                    meshDisplay.name = name;
                    meshDisplay.path = path.Length > 0 ? path : name;
                    meshDisplay.inheritAnimation = ObjectDataParser._GetBoolean(rawData, ObjectDataParser.INHERIT_FFD, true);
                    this._ParsePivot(rawData, meshDisplay);

                    var shareName = ObjectDataParser._GetString(rawData, ObjectDataParser.SHARE, "");
                    if (shareName.Length > 0)
                    {
                        if (!this._shareMeshs.ContainsKey(shareName)) 
                        {
                            this._shareMeshs[shareName] = new List<MeshDisplayData>();
                        }

                        this._shareMeshs[shareName].Add(meshDisplay);
                    }
                    else
                    {
                        this._ParseMesh(rawData, meshDisplay);
                        this._meshs[meshDisplay.name] = meshDisplay;
                    }
                    break;

                case DisplayType.BoundingBox:
                    var boundingBox = this._ParseBoundingBox(rawData);
                    if (boundingBox != null)
                    {
                        var boundingBoxDisplay = BaseObject.BorrowObject<BoundingBoxDisplayData>();
                        display = boundingBoxDisplay;
                        boundingBoxDisplay.name = name;
                        boundingBoxDisplay.path = path.Length > 0 ? path : name;
                        boundingBoxDisplay.boundingBox = boundingBox;
                    }
                    break;
            }

            if (display != null)
            {
                display.parent = this._armature;
                if (rawData.ContainsKey(ObjectDataParser.TRANSFORM))
                {
                    this._ParseTransform(rawData[ObjectDataParser.TRANSFORM], display.transform, this._armature.scale);
                }
            }

            return display;
        }

        /**
        * @private
        */
        protected void _ParsePivot(Dictionary<string, object> rawData, ImageDisplayData display)
        {
            if (rawData.ContainsKey(ObjectDataParser.PIVOT))
            {
                var rawPivot = rawData[ObjectDataParser.PIVOT] as Dictionary<string, object>;
                display.pivot.x = ObjectDataParser._GetNumber(rawPivot, ObjectDataParser.X, 0.0f);
                display.pivot.y = ObjectDataParser._GetNumber(rawPivot, ObjectDataParser.Y, 0.0f);
            }
            else
            {
                display.pivot.x = 0.5f;
                display.pivot.y = 0.5f;
            }
        }
        /**
         * @private
         */
        protected void _ParseMesh(Dictionary<string, object> rawData, MeshDisplayData mesh)
        {
            var rawVertices = rawData[ObjectDataParser.VERTICES] as List<object>;//float
            var rawUVs = rawData[ObjectDataParser.UVS] as List<object>;//float
            var rawTriangles = rawData[ObjectDataParser.TRIANGLES] as List<object>;//uint
            var vertexCount = (rawVertices.Count / 2); // uint
            var triangleCount = (rawTriangles.Count / 3); // uint
            var vertexOffset = this._floatArray.Count;
            var uvOffset = vertexOffset + vertexCount * 2;

            mesh.offset = this._intArray.Count;
            DragonBones.ResizeList(this._intArray, this._intArray.Count + 1 + 1 + 1 + 1 + triangleCount * 3, 0);
            this._intArray[mesh.offset + (int)BinaryOffset.MeshVertexCount] = vertexCount;
            this._intArray[mesh.offset + (int)BinaryOffset.MeshTriangleCount] = triangleCount;
            this._intArray[mesh.offset + (int)BinaryOffset.MeshFloatOffset] = vertexOffset;

            for (int i = 0, l = triangleCount * 3; i < l; ++i)
            {
                this._intArray[mesh.offset + (int)BinaryOffset.MeshVertexIndices + i] = (int)rawTriangles[i];
            }

            DragonBones.ResizeList(this._floatArray, this._floatArray.Count + vertexCount * 2 + vertexCount * 2, 0.0f);
            for (int i = 0, l = vertexCount * 2; i < l; ++i)
            {
                this._floatArray[vertexOffset + i] = (float)rawVertices[i];
                this._floatArray[uvOffset + i] = (float)rawUVs[i];
            }

            if (rawData.ContainsKey(ObjectDataParser.WEIGHTS))
            {
                var rawWeights = rawData[ObjectDataParser.WEIGHTS] as List<float>;
                var rawSlotPose = rawData[ObjectDataParser.SLOT_POSE] as List<float>;
                var rawBonePoses = rawData[ObjectDataParser.BONE_POSE] as List<float>;
                var weightBoneIndices = new List<uint>();
                var weightBoneCount = rawBonePoses.Count / 7; // uint
                var floatOffset = this._floatArray.Count;
                var weight = BaseObject.BorrowObject<WeightData>();

                weight.count = (rawWeights.Count - vertexCount) / 2;
                weight.offset = this._intArray.Count;

                DragonBones.ResizeList(weight.bones, weightBoneCount, null);
                DragonBones.ResizeList(weightBoneIndices, weightBoneCount, uint.MinValue);
                DragonBones.ResizeList(this._intArray, this._intArray.Count + 1 + 1 + weightBoneCount + vertexCount + weight.count, 0);
                this._intArray[weight.offset + (int)BinaryOffset.WeigthFloatOffset] = floatOffset;

                for (var i = 0; i < weightBoneCount; ++i)
                {
                    var rawBoneIndex = (int)rawBonePoses[i * 7]; // uint
                    var bone = this._rawBones[(int)rawBoneIndex];
                    weight.bones[i] = bone;
                    weightBoneIndices[i] = (uint)rawBoneIndex;

                    this._intArray[weight.offset + (int)BinaryOffset.WeigthBoneIndices + i] = this._armature.sortedBones.IndexOf(bone);
                }
                
                DragonBones.ResizeList(this._floatArray, this._floatArray.Count + weight.count * 3, 0.0f);
                this._helpMatrixA.CopyFromArray(rawSlotPose, 0);

                
                for (int i = 0, iW = 0, iB = weight.offset + (int)BinaryOffset.WeigthBoneIndices + weightBoneCount, iV = floatOffset; i < vertexCount; ++i)
                {
                    var iD = i * 2;
                    var vertexBoneCount = this._intArray[iB++] = (int)rawWeights[iW++]; // uint

                    var x = this._floatArray[vertexOffset + iD];
                    var y = this._floatArray[vertexOffset + iD + 1];
                    this._helpMatrixA.TransformPoint(x, y, this._helpPoint);
                    x = this._helpPoint.x;
                    y = this._helpPoint.y;

                    for (var j = 0; j < vertexBoneCount; ++j)
                    {
                        var rawBoneIndex = (int)rawWeights[iW++]; // uint
                        var bone = this._rawBones[(int)rawBoneIndex];
                        this._helpMatrixB.CopyFromArray(rawBonePoses, weightBoneIndices.IndexOf((uint)rawBoneIndex) * 7 + 1);
                        this._helpMatrixB.Invert();
                        this._helpMatrixB.TransformPoint(x, y, this._helpPoint);
                        this._intArray[iB++] = weight.bones.IndexOf(bone);
                        this._floatArray[iV++] = rawWeights[iW++];
                        this._floatArray[iV++] = this._helpPoint.x;
                        this._floatArray[iV++] = this._helpPoint.y;
                    }
                }

                mesh.weight = weight;

                //
                this._weightSlotPose[mesh.name] = rawSlotPose;
                this._weightBonePoses[mesh.name] = rawBonePoses;
                this._weightBoneIndices[mesh.name] = weightBoneIndices;
            }
        }
        /**
         * @private
         */
        protected BoundingBoxData _parseBoundingBox(Dictionary<string, object> rawData)
        {
            BoundingBoxData boundingBox = null;
            var type = BoundingBoxType.Rectangle;

            if (rawData.ContainsKey(ObjectDataParser.SUB_TYPE) && rawData[ObjectDataParser.SUB_TYPE] is string) 
            {
                type = ObjectDataParser._GetBoundingBoxType((string)rawData[ObjectDataParser.SUB_TYPE]);
            }
            else
            {
                type = (BoundingBoxType)ObjectDataParser._GetNumber(rawData, ObjectDataParser.SUB_TYPE, (uint)type);
            }

            switch (type)
            {
                case BoundingBoxType.Rectangle:
                    boundingBox = BaseObject.BorrowObject<RectangleBoundingBoxData>();
                    break;

                case BoundingBoxType.Ellipse:
                    boundingBox = BaseObject.BorrowObject<EllipseBoundingBoxData>();
                    break;

                case BoundingBoxType.Polygon:
                    boundingBox = this._ParsePolygonBoundingBox(rawData);
                    break;
            }

            if (boundingBox != null)
            {
                boundingBox.color = ObjectDataParser._GetNumber(rawData, ObjectDataParser.COLOR, (uint)0x000000);
                if (boundingBox.type == BoundingBoxType.Rectangle || boundingBox.type == BoundingBoxType.Ellipse)
                {
                    boundingBox.width = ObjectDataParser._GetNumber(rawData, ObjectDataParser.WIDTH, 0.0f);
                    boundingBox.height = ObjectDataParser._GetNumber(rawData, ObjectDataParser.HEIGHT, 0.0f);
                }
            }

            return boundingBox;
        }
        /**
         * @private
         */
        protected PolygonBoundingBoxData _ParsePolygonBoundingBox(Dictionary<string, object> rawData)
        {
            var polygonBoundingBox = BaseObject.BorrowObject<PolygonBoundingBoxData>();

            if (rawData.ContainsKey(ObjectDataParser.VERTICES))
            {
                var rawVertices = rawData[ObjectDataParser.VERTICES] as List<float>;

                DragonBones.ResizeList(polygonBoundingBox.vertices, rawVertices.Count, 0.0f);

                for (int i = 0, l = rawVertices.Count; i < l; i += 2)
                {
                    var x = rawVertices[i];
                    var y = rawVertices[i + 1];

                    polygonBoundingBox.vertices[i] = x;
                    polygonBoundingBox.vertices[i + 1] = y;

                    if (i == 0)
                    {
                        polygonBoundingBox.x = x;
                        polygonBoundingBox.y = y;
                        polygonBoundingBox.width = x;
                        polygonBoundingBox.height = y;
                    }
                    else
                    {
                        if (x < polygonBoundingBox.x)
                        {
                            polygonBoundingBox.x = x;
                        }
                        else if (x > polygonBoundingBox.width)
                        {
                            polygonBoundingBox.width = x;
                        }

                        if (y < polygonBoundingBox.y)
                        {
                            polygonBoundingBox.y = y;
                        }
                        else if (y > polygonBoundingBox.height)
                        {
                            polygonBoundingBox.height = y;
                        }
                    }
                }
            }
            else
            {
                DragonBones.Assert(false, "Data error.\n Please reexport DragonBones Data to fixed the bug.");
            }

            return polygonBoundingBox;

        }
        /**
         * @private
         */
        protected AnimationData _ParseAnimation(Dictionary<string, object> rawData)
        {
            var animation = BaseObject.BorrowObject<AnimationData>();

            animation.frameCount = (uint)Math.Max(ObjectDataParser._GetNumber(rawData, ObjectDataParser.DURATION, 1), 1);
            animation.playTimes = (uint)ObjectDataParser._GetNumber(rawData, ObjectDataParser.PLAY_TIMES, 1);
            animation.duration = animation.frameCount / this._armature.frameRate;
            animation.fadeInTime = ObjectDataParser._GetNumber(rawData, ObjectDataParser.FADE_IN_TIME, 0.0f);
            animation.scale = ObjectDataParser._GetNumber(rawData, ObjectDataParser.SCALE, 1.0f);
            animation.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, ObjectDataParser.DEFAULT_NAME);

            // TDOO Check std::string length
            if (animation.name.Length < 1)
            {
                animation.name = ObjectDataParser.DEFAULT_NAME;
            }

            animation.frameIntOffset = (uint)this._frameIntArray.Count;
            animation.frameFloatOffset = (uint)this._frameFloatArray.Count;
            animation.frameOffset = (uint)this._frameArray.Count;

            this._animation = animation;

            if (rawData.ContainsKey(ObjectDataParser.FRAME)) 
            {
                var rawFrames = rawData[ObjectDataParser.FRAME] as List<object>;
                var keyFrameCount = rawFrames.Count;
                if (keyFrameCount > 0)
                {
                    for (int i = 0, frameStart = 0; i < keyFrameCount; ++i)
                    {
                        var rawFrame = rawFrames[i] as Dictionary<string, object>;
                        this._ParseActionDataInFrame(rawFrame, frameStart, null, null);
                        frameStart += ObjectDataParser._GetNumber(rawFrame, ObjectDataParser.DURATION, 1);
                    }
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.Z_ORDER)) 
            {
                this._animation.zOrderTimeline = this._ParseTimeline(
                    rawData[ObjectDataParser.Z_ORDER], ObjectDataParser.FRAME, TimelineType.ZOrder,
                    false, false, 0,
                    this._ParseZOrderFrame
                );
            }

            if (rawData.ContainsKey(ObjectDataParser.BONE))
            {
                var rawTimelines = rawData[ObjectDataParser.BONE] as List<object>;
                foreach (Dictionary<string, object> rawTimeline in rawTimelines)
                {
                    this._ParseBoneTimeline(rawTimeline);
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.SLOT))
            {
                var rawTimelines = rawData[ObjectDataParser.SLOT] as List<object>;
                foreach (Dictionary<string, object> rawTimeline in rawTimelines) 
                {
                    this._ParseSlotTimeline(rawTimeline);
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.FFD))
            {
                var rawTimelines = rawData[ObjectDataParser.FFD] as List<object>;
                foreach (Dictionary<string, object> rawTimeline in rawTimelines)
                {
                    var slotName = ObjectDataParser._GetString(rawTimeline, ObjectDataParser.SLOT, "");
                    var displayName = ObjectDataParser._GetString(rawTimeline, ObjectDataParser.NAME, "");
                    var slot = this._armature.GetSlot(slotName);
                    if (slot == null)
                    {
                        continue;
                    }

                    this._slot = slot;
                    this._mesh = this._meshs[displayName];

                    var timelineFFD = this._ParseTimeline(
                        rawTimeline, ObjectDataParser.FRAME, TimelineType.SlotFFD,
                        false, true, 0,
                        this._ParseSlotFFDFrame
                    );

                    if (timelineFFD != null)
                    {
                        this._animation.AddSlotTimeline(slot, timelineFFD);
                    }

                    this._slot = null; //
                    this._mesh = null; //
                }
            }

            if (this._actionFrames.Count > 0)
            {
                this._actionFrames.Sort();

                var timeline = this._animation.actionTimeline = BaseObject.BorrowObject<TimelineData>();
                var keyFrameCount = this._actionFrames.Count;
                timeline.type = TimelineType.Action;
                timeline.offset = (uint)this._timelineArray.Count;

                DragonBones.ResizeList(this._timelineArray, this._timelineArray.Count + 1 + 1 + 1 + 1 + 1 + keyFrameCount, 0);
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineScale] = 100;
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineOffset] = 0;
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineKeyFrameCount] = keyFrameCount;
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameValueCount] = 0;
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameValueOffset] = 0;

                this._timeline = timeline;
                if (keyFrameCount == 1)
                {
                    timeline.frameIndicesOffset = -1;
                    this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameOffset + 0] = this._ParseCacheActionFrame(this._actionFrames[0]) - (int)this._animation.frameOffset;
                }
                else
                {
                    var totalFrameCount = this._animation.frameCount + 1; // One more frame than animation.
                    var frameIndices = this._data.frameIndices;

                    timeline.frameIndicesOffset = frameIndices.Count;
                    DragonBones.ResizeList(frameIndices, frameIndices.Count + (int)totalFrameCount, uint.MinValue);

                    for (
                        int i = 0, iK = 0, frameStart = 0, frameCount = 0;
                        i < totalFrameCount;
                        ++i
                        )
                    {
                        if (frameStart + frameCount <= i && iK < keyFrameCount)
                        {
                            var frame = this._actionFrames[iK];
                            frameStart = frame.frameStart;
                            if (iK == keyFrameCount - 1)
                            {
                                frameCount = (int)this._animation.frameCount - frameStart;
                            }
                            else
                            {
                                frameCount = this._actionFrames[iK + 1].frameStart - frameStart;
                            }

                            this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameOffset + iK] = this._ParseCacheActionFrame(frame) - (int)this._animation.frameOffset;
                            iK++;
                        }

                        frameIndices[timeline.frameIndicesOffset + i] = (uint)iK - 1;
                    }
                }

                this._timeline = null; //
                this._actionFrames.Clear();
            }

            this._animation = null; //

            return animation;
        }
        /**
         * @private
         */
        protected TimelineData _ParseTimeline(
                                                Dictionary<string, object> rawData, string framesKey, TimelineType type,
                                                bool addIntOffset, bool addFloatOffset, uint frameValueCount,
                                                Func<Dictionary<string, object>, int, uint, int> frameParser)
        {
            if (!rawData.ContainsKey(framesKey))
            {
                return null;
            }

            var rawFrames = rawData[framesKey] as List<object>;
            var keyFrameCount = rawFrames.Count;
            if (keyFrameCount == 0)
            {
                return null;
            }

            var frameIntArrayLength = this._frameIntArray.Count;
            var frameFloatArrayLength = this._frameFloatArray.Count;
            var timeline = BaseObject.BorrowObject<TimelineData>();
            timeline.type = type;
            timeline.offset = (uint)this._timelineArray.Count;
            
            DragonBones.ResizeList(this._timelineArray, this._timelineArray.Count + 1 + 1 + 1 + 1 + 1 + keyFrameCount, 0);
            this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineScale] = (int)Math.Round(ObjectDataParser._GetNumber(rawData, ObjectDataParser.SCALE, 1.0f) * 100);
            this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineOffset] = (int)Math.Round(ObjectDataParser._GetNumber(rawData, ObjectDataParser.OFFSET, 0.0f) * 100);
            this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineKeyFrameCount] = keyFrameCount;
            this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameValueCount] = (int)frameValueCount;

            if (addIntOffset)
            {
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameValueOffset] = frameIntArrayLength - (int)this._animation.frameIntOffset;
            }
            else if (addFloatOffset)
            {
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameValueOffset] = frameFloatArrayLength - (int)this._animation.frameFloatOffset;
            }
            else
            {
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameValueOffset] = 0;
            }

            this._timeline = timeline;

            if (keyFrameCount == 1)
            { 
                // Only one frame.
                timeline.frameIndicesOffset = -1;
                int frameParserResult = frameParser(rawFrames[0] as Dictionary<string, object>, 0, 0);
                this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameOffset + 0] = frameParserResult - (int)this._animation.frameOffset;
            }
            else
            {
                var frameIndices = this._data.frameIndices;
                var totalFrameCount = this._animation.frameCount + 1; // One more frame than animation.

                timeline.frameIndicesOffset = frameIndices.Count;
                DragonBones.ResizeList(frameIndices, frameIndices.Count + (int)totalFrameCount, uint.MinValue);

                for (
                    int i = 0, iK = 0, frameStart = 0, frameCount = 0;
                    i < totalFrameCount;
                    ++i
                    )
                {
                    if (frameStart + frameCount <= i && iK < keyFrameCount)
                    {
                        var rawFrame = rawFrames[iK] as Dictionary<string, object>;
                        frameStart = i;
                        frameCount = ObjectDataParser._GetNumber(rawFrame, ObjectDataParser.DURATION, 1);
                        if (iK == keyFrameCount - 1)
                        {
                            frameCount = (int)this._animation.frameCount - frameStart;
                        }

                        int frameParserResult = frameParser(rawFrame, frameStart, (uint)frameCount);
                        this._timelineArray[(int)timeline.offset + (int)BinaryOffset.TimelineFrameOffset + iK] = frameParserResult - (int)this._animation.frameOffset;
                        iK++;
                    }

                    frameIndices[timeline.frameIndicesOffset + i] = (uint)iK - 1;
                }
            }

            this._timeline = null; //

            return timeline;
        }

        /**
         * @private
         */
        protected void _ParseBoneTimeline(Dictionary<string, object> rawData)
        {
            var bone = this._armature.GetBone(ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, ""));
            if (bone == null)
            {
                return;
            }

            this._bone = bone;
            this._slot = this._armature.GetSlot(this._bone.name);

            if (rawData.ContainsKey(ObjectDataParser.TRANSLATE_FRAME)) 
            {
                var timeline = this._ParseTimeline(
                    rawData, ObjectDataParser.TRANSLATE_FRAME, TimelineType.BoneTranslate,
                    false, true, 2,
                    this._ParseBoneTranslateFrame
                );

                if (timeline != null)
                {
                    this._animation.AddBoneTimeline(bone, timeline);
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.ROTATE_FRAME))
            {
                var timeline = this._ParseTimeline(
                    rawData, ObjectDataParser.ROTATE_FRAME, TimelineType.BoneRotate,
                    false, true, 2,
                    this._ParseBoneRotateFrame
                );

                if (timeline != null)
                {
                    this._animation.AddBoneTimeline(bone, timeline);
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.SCALE_FRAME)) 
            {
                var timeline = this._ParseTimeline(
                    rawData, ObjectDataParser.SCALE_FRAME, TimelineType.BoneScale,
                    false, true, 2,
                    this._ParseBoneScaleFrame
                );

                if (timeline != null)
                {
                    this._animation.AddBoneTimeline(bone, timeline);
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.FRAME))
            {
                var timeline = this._ParseTimeline(
                    rawData, ObjectDataParser.FRAME, TimelineType.BoneAll,
                    false, true, 6,
                    this._ParseBoneAllFrame
                );

                if (timeline != null)
                {
                    this._animation.AddBoneTimeline(bone, timeline);
                }
            }

            this._bone = null; //
            this._slot = null; //
        }
        /**
         * @private
         */
        protected void _ParseSlotTimeline(Dictionary<string, object> rawData)
        {
            var slot = this._armature.GetSlot(ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, ""));
            if (slot == null)
            {
                return;
            }

            this._slot = slot;

            TimelineData displayTimeline = null;
            if (rawData.ContainsKey(ObjectDataParser.DISPLAY_FRAME))
            {
                displayTimeline = this._ParseTimeline(
                    rawData, ObjectDataParser.DISPLAY_FRAME, TimelineType.SlotDisplay,
                    false, false, 0,
                    this._ParseSlotDisplayIndexFrame
                );
            }
            else
            {
                displayTimeline = this._ParseTimeline(
                    rawData, ObjectDataParser.FRAME, TimelineType.SlotDisplay,
                    false, false, 0,
                    this._ParseSlotDisplayIndexFrame
                );
            }

            if (displayTimeline != null)
            {
                this._animation.AddSlotTimeline(slot, displayTimeline);
            }

            TimelineData colorTimeline = null;
            if (rawData.ContainsKey(ObjectDataParser.COLOR_FRAME))
            {
                colorTimeline = this._ParseTimeline(
                    rawData, ObjectDataParser.COLOR_FRAME, TimelineType.SlotColor,
                    true, false, 1,
                    this._ParseSlotColorFrame
                );
            }
            else
            {
                colorTimeline = this._ParseTimeline(
                    rawData, ObjectDataParser.FRAME, TimelineType.SlotColor,
                    true, false, 1,
                    this._ParseSlotColorFrame
                );
            }

            if (colorTimeline != null)
            {
                this._animation.AddSlotTimeline(slot, colorTimeline);
            }

            this._slot = null; //
        }

        /**
         * @private
         */
        protected int _ParseFrame(Dictionary<string, object> rawData, int frameStart, uint frameCount)
        {
            //rawData没用到
            var frameOffset = this._frameArray.Count;
            DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1, 0);
            this._frameArray[(int)frameOffset + (int)BinaryOffset.FramePosition] = frameStart;

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseTweenFrame(Dictionary<string, object> rawData, int frameStart, uint frameCount)
        {
            var frameOffset = this._ParseFrame(rawData, frameStart, frameCount);
            if (frameCount > 0)
            {
                if (rawData.ContainsKey(ObjectDataParser.CURVE))
                {
                    var sampleCount = (int)frameCount + 1;
                    DragonBones.ResizeList(this._helpArray, sampleCount, 0.0f);
                    var rawCurve = rawData[ObjectDataParser.CURVE] as List<object>;
                    var curve = new float[rawCurve.Count];
                    for (int i = 0, l = rawCurve.Count; i < l; ++i)
                    {
                        curve[i] = Convert.ToSingle(rawCurve[i]);
                    }
                    this._SamplingEasingCurve(curve, this._helpArray.ToArray());

                    DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1 + 1 + this._helpArray.Count, 0);
                    this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.Curve;
                    this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = sampleCount;
                    for (var i = 0; i < sampleCount; ++i)
                    {
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameCurveSamples + i] = (int)Math.Round(this._helpArray[i] * 10000.0f);
                    }
                }
                else
                {
                    var noTween = -2.0f;
                    var tweenEasing = noTween;
                    if (rawData.ContainsKey(ObjectDataParser.TWEEN_EASING)) 
                    {
                        tweenEasing = ObjectDataParser._GetNumber(rawData, ObjectDataParser.TWEEN_EASING, noTween);
                    }

                    if (tweenEasing == noTween)
                    {
                        DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.None;
                    }
                    else if (tweenEasing == 0.0)
                    {
                        DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.Line;
                    }
                    else if (tweenEasing < 0.0)
                    {
                        DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1 + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.QuadIn;
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = (int)Math.Round(-tweenEasing * 100.0);
                    }
                    else if (tweenEasing <= 1.0)
                    {
                        DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1 + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.QuadOut;
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = (int)Math.Round(tweenEasing * 100.0);
                    }
                    else
                    {
                        DragonBones.ResizeList(this._frameArray, this._frameArray.Count + 1 + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.QuadInOut;
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = (int)Math.Round(tweenEasing * 100.0 - 100.0);
                    }
                }
            }
        }
    }

    class ActionFrame
    {
        public int frameStart = 0;
        public readonly List<int> actions = new List<int>();
    }
}
