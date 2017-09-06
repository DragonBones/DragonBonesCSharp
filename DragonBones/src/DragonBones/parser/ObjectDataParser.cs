using System;
using System.Collections;
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

        protected int _rawTextureAtlasIndex = 0;
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
            this._frameArray.ResizeList(this._frameArray.Count + 1 + 1 + actionCount, 0);
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
                    this._ParseTransform(rawData[ObjectDataParser.TRANSFORM] as Dictionary<string, object>, display.transform, this._armature.scale);
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
        protected virtual void _ParseMesh(Dictionary<string, object> rawData, MeshDisplayData mesh)
        {
            var rawVertices = rawData[ObjectDataParser.VERTICES] as List<object>;//float
            var rawUVs = rawData[ObjectDataParser.UVS] as List<object>;//float
            var rawTriangles = rawData[ObjectDataParser.TRIANGLES] as List<object>;//uint
            var vertexCount = (rawVertices.Count / 2); // uint
            var triangleCount = (rawTriangles.Count / 3); // uint
            var vertexOffset = this._floatArray.Count;
            var uvOffset = vertexOffset + vertexCount * 2;

            mesh.offset = this._intArray.Count;
            this._intArray.ResizeList(this._intArray.Count + 1 + 1 + 1 + 1 + triangleCount * 3, 0);
            this._intArray[mesh.offset + (int)BinaryOffset.MeshVertexCount] = vertexCount;
            this._intArray[mesh.offset + (int)BinaryOffset.MeshTriangleCount] = triangleCount;
            this._intArray[mesh.offset + (int)BinaryOffset.MeshFloatOffset] = vertexOffset;

            for (int i = 0, l = triangleCount * 3; i < l; ++i)
            {
                this._intArray[mesh.offset + (int)BinaryOffset.MeshVertexIndices + i] = (int)rawTriangles[i];
            }

            this._floatArray.ResizeList(this._floatArray.Count + vertexCount * 2 + vertexCount * 2, 0.0f);
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

                weight.bones.ResizeList(weightBoneCount, null);
                weightBoneIndices.ResizeList(weightBoneCount, uint.MinValue);
                this._intArray.ResizeList(this._intArray.Count + 1 + 1 + weightBoneCount + vertexCount + weight.count, 0);
                this._intArray[weight.offset + (int)BinaryOffset.WeigthFloatOffset] = floatOffset;

                for (var i = 0; i < weightBoneCount; ++i)
                {
                    var rawBoneIndex = (int)rawBonePoses[i * 7]; // uint
                    var bone = this._rawBones[(int)rawBoneIndex];
                    weight.bones[i] = bone;
                    weightBoneIndices[i] = (uint)rawBoneIndex;

                    this._intArray[weight.offset + (int)BinaryOffset.WeigthBoneIndices + i] = this._armature.sortedBones.IndexOf(bone);
                }

                this._floatArray.ResizeList(this._floatArray.Count + weight.count * 3, 0.0f);
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
        protected BoundingBoxData _ParseBoundingBox(Dictionary<string, object> rawData)
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

                polygonBoundingBox.vertices.ResizeList(rawVertices.Count, 0.0f);

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
                Helper.Assert(false, "Data error.\n Please reexport DragonBones Data to fixed the bug.");
            }

            return polygonBoundingBox;

        }
        /**
         * @private
         */
        protected virtual AnimationData _ParseAnimation(Dictionary<string, object> rawData)
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
                    rawData[ObjectDataParser.Z_ORDER] as Dictionary<string, object>, ObjectDataParser.FRAME, TimelineType.ZOrder,
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

                this._timelineArray.ResizeList(this._timelineArray.Count + 1 + 1 + 1 + 1 + 1 + keyFrameCount, 0);
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
                    frameIndices.ResizeList(frameIndices.Count + (int)totalFrameCount, uint.MinValue);

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
                                                Func<Dictionary<string, object>, int, int, int> frameParser)
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

            this._timelineArray.ResizeList(this._timelineArray.Count + 1 + 1 + 1 + 1 + 1 + keyFrameCount, 0);
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
                frameIndices.ResizeList(frameIndices.Count + (int)totalFrameCount, uint.MinValue);

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

                        int frameParserResult = frameParser(rawFrame, frameStart, frameCount);
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
        protected int _ParseFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            //rawData没用到
            var frameOffset = this._frameArray.Count;
            this._frameArray.ResizeList(this._frameArray.Count + 1, 0);
            this._frameArray[(int)frameOffset + (int)BinaryOffset.FramePosition] = frameStart;

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseTweenFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseFrame(rawData, frameStart, frameCount);
            if (frameCount > 0)
            {
                if (rawData.ContainsKey(ObjectDataParser.CURVE))
                {
                    var sampleCount = frameCount + 1;
                    this._helpArray.ResizeList(sampleCount, 0.0f);
                    var rawCurve = rawData[ObjectDataParser.CURVE] as List<object>;
                    var curve = new float[rawCurve.Count];
                    for (int i = 0, l = rawCurve.Count; i < l; ++i)
                    {
                        curve[i] = Convert.ToSingle(rawCurve[i]);
                    }
                    this._SamplingEasingCurve(curve, this._helpArray.ToArray());

                    this._frameArray.ResizeList(this._frameArray.Count + 1 + 1 + this._helpArray.Count, 0);
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
                        this._frameArray.ResizeList(this._frameArray.Count + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.None;
                    }
                    else if (tweenEasing == 0.0f)
                    {
                        this._frameArray.ResizeList(this._frameArray.Count + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.Line;
                    }
                    else if (tweenEasing < 0.0f)
                    {
                        this._frameArray.ResizeList(this._frameArray.Count + 1 + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.QuadIn;
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = (int)Math.Round(-tweenEasing * 100.0f);
                    }
                    else if (tweenEasing <= 1.0f)
                    {
                        this._frameArray.ResizeList(this._frameArray.Count + 1 + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.QuadOut;
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = (int)Math.Round(tweenEasing * 100.0f);
                    }
                    else
                    {
                        this._frameArray.ResizeList(this._frameArray.Count + 1 + 1, 0);
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.QuadInOut;
                        this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenEasingOrCurveSampleCount] = (int)Math.Round(tweenEasing * 100.0f - 100.0f);
                    }
                }
            }
            else
            {
                this._frameArray.ResizeList(this._frameArray.Count + 1);
                this._frameArray[frameOffset + (int)BinaryOffset.FrameTweenType] = (int)TweenType.None;
            }

            return frameOffset;
        }
        /**
         * @private
         */
        private int _ParseZOrderFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseFrame(rawData, frameStart, frameCount);

            if (rawData.ContainsKey(ObjectDataParser.Z_ORDER))
            {
                var rawZOrder = rawData[ObjectDataParser.Z_ORDER] as List<int>;

                if (rawZOrder.Count > 0)
                {
                    int slotCount = this._armature.sortedSlots.Count;
                    List<int> unchanged = new List<int>(slotCount - rawZOrder.Count / 2);
                    List<int> zOrders = new List<int>(slotCount);

                    for (var i = 0; i < unchanged.Count; ++i)
                    {
                        unchanged[i] = 0;
                    }

                    for (var i = 0; i < slotCount; ++i)
                    {
                        zOrders[i] = -1;
                    }

                    var originalIndex = 0;
                    var unchangedIndex = 0;

                    for (int i = 0, l = rawZOrder.Count; i < l; i += 2)
                    {
                        var slotIndex = rawZOrder[i];
                        var zOrderOffset = rawZOrder[i + 1];

                        while (originalIndex != slotIndex)
                        {
                            unchanged[unchangedIndex++] = originalIndex++;
                        }

                        zOrders[originalIndex + zOrderOffset] = originalIndex++;
                    }

                    while (originalIndex < slotCount)
                    {
                        unchanged[unchangedIndex++] = originalIndex++;
                    }
                    
                    this._frameArray.ResizeList(this._frameArray.Count + 1);
                    this._frameArray[frameOffset + 1] = slotCount;

                    var index = slotCount;
                    while (index-- > 0)
                    {
                        var value = 0;
                        if (zOrders[index] == -1)
                        {
                            value = unchanged[--unchangedIndex];
                            this._frameArray[frameOffset + 2 + index] = value > 0 ? value : 0;
                        }
                        else
                        {
                            value = zOrders[index];
                            this._frameArray[frameOffset + 2 + index] = value > 0 ? value : 0;
                        }
                    }

                    return frameOffset;
                }
            }
            
            this._frameArray.ResizeList(this._frameArray.Count + 1);
            this._frameArray[frameOffset + 1] = 0;

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseBoneAllFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseTweenFrame(rawData, frameStart, frameCount);

            this._helpTransform.Identity();
            if (rawData.ContainsKey(ObjectDataParser.TRANSFORM)) 
            {
                this._ParseTransform(rawData[ObjectDataParser.TRANSFORM] as Dictionary<string, object>, this._helpTransform, 1.0f);
            }

            // Modify rotation.
            var rotation = this._helpTransform.rotation;
            if (frameStart != 0)
            {
                if (this._prevClockwise == 0)
                {
                    rotation = this._prevRotation + Transform.NormalizeRadian(rotation - this._prevRotation);
                }
                else
                {
                    if (this._prevClockwise > 0 ? rotation >= this._prevRotation : rotation <= this._prevRotation)
                    {
                        this._prevClockwise = this._prevClockwise > 0 ? this._prevClockwise - 1 : this._prevClockwise + 1;
                    }

                    rotation = this._prevRotation + rotation - this._prevRotation + Transform.PI_D * this._prevClockwise;
                }
            }

            this._prevClockwise = ObjectDataParser._GetNumber(rawData, ObjectDataParser.TWEEN_ROTATE, 0);
            this._prevRotation = rotation;

            var frameFloatOffset = this._frameFloatArray.Count;
            this._frameFloatArray.ResizeList(this._frameFloatArray.Count + 6);
            this._frameFloatArray[frameFloatOffset++] = this._helpTransform.x;
            this._frameFloatArray[frameFloatOffset++] = this._helpTransform.y;
            this._frameFloatArray[frameFloatOffset++] = rotation;
            this._frameFloatArray[frameFloatOffset++] = this._helpTransform.skew;
            this._frameFloatArray[frameFloatOffset++] = this._helpTransform.scaleX;
            this._frameFloatArray[frameFloatOffset++] = this._helpTransform.scaleY;

            this._ParseActionDataInFrame(rawData, frameStart, this._bone, this._slot);

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseBoneTranslateFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseTweenFrame(rawData, frameStart, frameCount);

            var frameFloatOffset = this._frameFloatArray.Count;
            this._frameFloatArray.ResizeList(this._frameFloatArray.Count + 2);
            this._frameFloatArray[frameFloatOffset++] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.X, 0.0f);
            this._frameFloatArray[frameFloatOffset++] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.Y, 0.0f);

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseBoneRotateFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseTweenFrame(rawData, frameStart, frameCount);

            var rotation = ObjectDataParser._GetNumber(rawData, ObjectDataParser.ROTATE, 0.0f) * Transform.DEG_RAD;
            if (frameStart != 0)
            {
                if (this._prevClockwise == 0)
                {
                    rotation = this._prevRotation + Transform.NormalizeRadian(rotation - this._prevRotation);
                }
                else
                {
                    if (this._prevClockwise > 0 ? rotation >= this._prevRotation : rotation <= this._prevRotation)
                    {
                        this._prevClockwise = this._prevClockwise > 0 ? this._prevClockwise - 1 : this._prevClockwise + 1;
                    }

                    rotation = this._prevRotation + rotation - this._prevRotation + Transform.PI_D * this._prevClockwise;
                }
            }

            this._prevClockwise = ObjectDataParser._GetNumber(rawData, ObjectDataParser.CLOCK_WISE, 0);
            this._prevRotation = rotation;

            var frameFloatOffset = this._frameFloatArray.Count;
            this._frameFloatArray.ResizeList(this._frameFloatArray.Count + 2);
            this._frameFloatArray[frameFloatOffset++] = rotation;
            this._frameFloatArray[frameFloatOffset++] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.SKEW, 0.0f) * Transform.DEG_RAD;

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseBoneScaleFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseTweenFrame(rawData, frameStart, frameCount);

            var frameFloatOffset = this._frameFloatArray.Count;
            this._frameFloatArray.ResizeList(this._frameFloatArray.Count + 2);
            this._frameFloatArray[frameFloatOffset++] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.X, 1.0f);
            this._frameFloatArray[frameFloatOffset++] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.Y, 1.0f);

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseSlotDisplayIndexFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseFrame(rawData, frameStart, frameCount);
            
            this._frameArray.ResizeList(this._frameArray.Count + 1);

            if (rawData.ContainsKey(ObjectDataParser.VALUE))
            {
                this._frameArray[frameOffset + 1] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.VALUE, 0);
            }
            else
            {
                this._frameArray[frameOffset + 1] = ObjectDataParser._GetNumber(rawData, ObjectDataParser.DISPLAY_INDEX, 0);
            }

            this._ParseActionDataInFrame(rawData, frameStart, this._slot.parent, this._slot);

            return frameOffset;
        }
        /**
         * @private
         */
        protected int _ParseSlotColorFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameOffset = this._ParseTweenFrame(rawData, frameStart, frameCount);
            var colorOffset = -1;
            if (rawData.ContainsKey(ObjectDataParser.VALUE) || rawData.ContainsKey(ObjectDataParser.COLOR))
            {
                var rawColor = rawData.ContainsKey(ObjectDataParser.VALUE) ? rawData[ObjectDataParser.VALUE] as Dictionary<string, object> : rawData[ObjectDataParser.COLOR] as Dictionary<string, object>;

                foreach (var k in rawColor)
                {
                    this._ParseColorTransform(rawColor, this._helpColorTransform);
                    colorOffset = this._intArray.Count;
                    this._intArray.ResizeList(this._intArray.Count + 8);
                    this._intArray[colorOffset++] = (int)Math.Round(this._helpColorTransform.alphaMultiplier * 100);
                    this._intArray[colorOffset++] = (int)Math.Round(this._helpColorTransform.redMultiplier * 100);
                    this._intArray[colorOffset++] = (int)Math.Round(this._helpColorTransform.greenMultiplier * 100);
                    this._intArray[colorOffset++] = (int)Math.Round(this._helpColorTransform.blueMultiplier * 100);
                    this._intArray[colorOffset++] = (int)Math.Round((float)this._helpColorTransform.alphaOffset);
                    this._intArray[colorOffset++] = (int)Math.Round((float)this._helpColorTransform.redOffset);
                    this._intArray[colorOffset++] = (int)Math.Round((float)this._helpColorTransform.greenOffset);
                    this._intArray[colorOffset++] = (int)Math.Round((float)this._helpColorTransform.blueOffset);
                    colorOffset -= 8;
                    break;
                }
            }

            if (colorOffset < 0)
            {
                if (this._defalultColorOffset < 0)
                {
                    this._defalultColorOffset = colorOffset = this._intArray.Count;
                    this._intArray.ResizeList(this._intArray.Count + 8);
                    this._intArray[colorOffset++] = 100;
                    this._intArray[colorOffset++] = 100;
                    this._intArray[colorOffset++] = 100;
                    this._intArray[colorOffset++] = 100;
                    this._intArray[colorOffset++] = 0;
                    this._intArray[colorOffset++] = 0;
                    this._intArray[colorOffset++] = 0;
                    this._intArray[colorOffset++] = 0;
                }

                colorOffset = this._defalultColorOffset;
            }

            var frameIntOffset = this._frameIntArray.Count;
            this._frameIntArray.ResizeList(this._frameIntArray.Count + 1);
            this._frameIntArray[frameIntOffset] = colorOffset;

            return frameOffset;
        }

        /**
         * @private
         */
        protected int _ParseSlotFFDFrame(Dictionary<string, object> rawData, int frameStart, int frameCount)
        {
            var frameFloatOffset = this._frameFloatArray.Count;
            var frameOffset = this._ParseTweenFrame(rawData, frameStart, frameCount);
            var rawVertices = rawData.ContainsKey(ObjectDataParser.VERTICES) ? rawData[ObjectDataParser.VERTICES] as List<float> : null;
            var offset = ObjectDataParser._GetNumber(rawData, ObjectDataParser.OFFSET, 0); // uint
            var vertexCount = this._intArray[this._mesh.offset + (int)BinaryOffset.MeshVertexCount];

            var x = 0.0f;
            var y = 0.0f;
            var iB = 0;
            var iV = 0;

            if (this._mesh.weight != null)
            {
                var rawSlotPose = this._weightSlotPose[this._mesh.name];
                this._helpMatrixA.CopyFromArray(rawSlotPose, 0);
                this._frameFloatArray.ResizeList(this._frameFloatArray.Count + this._mesh.weight.count * 2);
                iB = this._mesh.weight.offset + (int)BinaryOffset.WeigthBoneIndices + this._mesh.weight.bones.Count;
            }
            else
            {
                this._frameFloatArray.ResizeList(this._frameFloatArray.Count + vertexCount * 2);
            }

            for ( var i = 0; i < vertexCount * 2; i += 2 )
            {
                if (rawVertices == null)
                { // Fill 0.
                    x = 0.0f;
                    y = 0.0f;
                }
                else
                {
                    if (i < offset || i - offset >= rawVertices.Count)
                    {
                        x = 0.0f;
                    }
                    else
                    {
                        x = rawVertices[i - offset];
                    }

                    if (i + 1 < offset || i + 1 - offset >= rawVertices.Count)
                    {
                        y = 0.0f;
                    }
                    else
                    {
                        y = rawVertices[i + 1 - offset];
                    }
                }

                if (this._mesh.weight != null)
                {
                    // If mesh is skinned, transform point by bone bind pose.
                    var rawBonePoses = this._weightBonePoses[this._mesh.name];
                    var weightBoneIndices = this._weightBoneIndices[this._mesh.name];
                    var vertexBoneCount = this._intArray[iB++];

                    this._helpMatrixA.TransformPoint(x, y, this._helpPoint, true);
                    x = this._helpPoint.x;
                    y = this._helpPoint.y;

                    for (var j = 0; j < vertexBoneCount; ++j)
                    {
                        var boneIndex = this._intArray[iB++];
                        var bone = this._mesh.weight.bones[boneIndex];
                        var rawBoneIndex = (uint)this._rawBones.IndexOf(bone);

                        this._helpMatrixB.CopyFromArray(rawBonePoses, weightBoneIndices.IndexOf(rawBoneIndex) * 7 + 1);
                        this._helpMatrixB.Invert();
                        this._helpMatrixB.TransformPoint(x, y, this._helpPoint, true);

                        this._frameFloatArray[frameFloatOffset + iV++] = this._helpPoint.x;
                        this._frameFloatArray[frameFloatOffset + iV++] = this._helpPoint.y;
                    }
                }
                else
                {
                    this._frameFloatArray[frameFloatOffset + i] = x;
                    this._frameFloatArray[frameFloatOffset + i + 1] = y;
                }
            }

            if (frameStart == 0)
            {
                var frameIntOffset = this._frameIntArray.Count;
                this._frameIntArray.ResizeList(this._frameIntArray.Count + 1 + 1 + 1 + 1 + 1);
                this._frameIntArray[frameIntOffset + (int)BinaryOffset.FFDTimelineMeshOffset] = this._mesh.offset;
                this._frameIntArray[frameIntOffset + (int)BinaryOffset.FFDTimelineFFDCount] = this._frameFloatArray.Count - frameFloatOffset;
                this._frameIntArray[frameIntOffset + (int)BinaryOffset.FFDTimelineValueCount] = this._frameFloatArray.Count - frameFloatOffset;
                this._frameIntArray[frameIntOffset + (int)BinaryOffset.FFDTimelineValueOffset] = 0;
                this._frameIntArray[frameIntOffset + (int)BinaryOffset.FFDTimelineFloatOffset] = frameFloatOffset;
                this._timelineArray[(int)this._timeline.offset + (int)BinaryOffset.TimelineFrameValueCount] = frameIntOffset - (int)this._animation.frameIntOffset;
            }

            return frameOffset;
        }

        /**
         * @private
         */
        protected int _ParseActionData(object rawData, List<ActionData> actions, ActionType type, BoneData bone = null, SlotData slot = null)
        {
            var actionCount = 0;
            if (rawData is string)
            {
                var action = BaseObject.BorrowObject<ActionData>();
                action.type = type;
                action.name = (string)rawData;
                action.bone = bone;
                action.slot = slot;
                actions.Add(action);
                actionCount++;
            }
            else if (rawData is IList)
            {
                var actionsObject = rawData as List<object>;
                foreach (Dictionary<string, object> rawAction in actionsObject)
                {
                    var action = BaseObject.BorrowObject<ActionData>();
                    if (rawAction.ContainsKey(ObjectDataParser.GOTO_AND_PLAY))
                    {
                        action.type = ActionType.Play;
                        action.name = ObjectDataParser._GetString(rawAction, ObjectDataParser.GOTO_AND_PLAY, "");
                    }
                    else
                    {
                        if (rawAction.ContainsKey(ObjectDataParser.TYPE) && rawAction[ObjectDataParser.TYPE] is string)
                        {
                            action.type = (ActionType)ObjectDataParser._GetActionType((string)rawAction[ObjectDataParser.TYPE]);
                        }
                        else {
                            action.type = (ActionType)ObjectDataParser._GetNumber(rawAction, ObjectDataParser.TYPE, (uint)type);
                        }

                        action.name = ObjectDataParser._GetString(rawAction, ObjectDataParser.NAME, "");
                    }

                    if (rawAction.ContainsKey(ObjectDataParser.BONE))
                    {
                        var boneName = ObjectDataParser._GetString(rawAction, ObjectDataParser.BONE, "");
                        action.bone = this._armature.GetBone(boneName);
                    }
                    else
                    {
                        action.bone = bone;
                    }

                    if (rawAction.ContainsKey(ObjectDataParser.SLOT))
                    {
                        var slotName = ObjectDataParser._GetString(rawAction, ObjectDataParser.SLOT, "");
                        action.slot = this._armature.GetSlot(slotName);
                    }
                    else
                    {
                        action.slot = slot;
                    }

                    if (rawAction.ContainsKey(ObjectDataParser.INTS))
                    {
                        if (action.data == null)
                        {
                            action.data = BaseObject.BorrowObject<UserData>();
                        }

                        var rawInts = rawAction[ObjectDataParser.INTS] as List<int>;
                        foreach (var rawValue in rawInts)
                        {
                            action.data.ints.Add(rawValue);
                        }
                    }

                    if (rawAction.ContainsKey(ObjectDataParser.FLOATS)) 
                    {
                        if (action.data == null)
                        {
                            action.data = BaseObject.BorrowObject<UserData>();
                        }

                        var rawFloats = rawAction[ObjectDataParser.FLOATS] as List<float>;
                        foreach (var rawValue in rawFloats)
                        {
                            action.data.floats.Add(rawValue);
                        }
                    }

                    if (rawAction.ContainsKey(ObjectDataParser.STRINGS))
                    {
                        if (action.data == null)
                        {
                            action.data = BaseObject.BorrowObject<UserData>();
                        }

                        var rawStrings = rawAction[ObjectDataParser.STRINGS] as List<string>;
                        foreach (var rawValue in rawStrings)
                        {
                            action.data.strings.Add(rawValue);
                        }
                    }

                    actions.Add(action);
                    actionCount++;
                }
            }

            return actionCount;
        }

        /**
         * @private
         */
        protected void _ParseTransform(Dictionary<string, object> rawData, Transform transform, float scale)
        {
            transform.x = ObjectDataParser._GetNumber(rawData, ObjectDataParser.X, 0.0f) * scale;
            transform.y = ObjectDataParser._GetNumber(rawData, ObjectDataParser.Y, 0.0f) * scale;

            if (rawData.ContainsKey(ObjectDataParser.ROTATE) || rawData.ContainsKey(ObjectDataParser.SKEW))
            {
                transform.rotation = Transform.NormalizeRadian(ObjectDataParser._GetNumber(rawData, ObjectDataParser.ROTATE, 0.0f) * Transform.DEG_RAD);
                transform.skew = Transform.NormalizeRadian(ObjectDataParser._GetNumber(rawData, ObjectDataParser.SKEW, 0.0f) * Transform.DEG_RAD);
            }
            else if (rawData.ContainsKey(ObjectDataParser.SKEW_X) || rawData.ContainsKey(ObjectDataParser.SKEW_Y))
            {
                transform.rotation = Transform.NormalizeRadian(ObjectDataParser._GetNumber(rawData, ObjectDataParser.SKEW_Y, 0.0f) * Transform.DEG_RAD);
                transform.skew = Transform.NormalizeRadian(ObjectDataParser._GetNumber(rawData, ObjectDataParser.SKEW_X, 0.0f) * Transform.DEG_RAD) - transform.rotation;
            }

            transform.scaleX = ObjectDataParser._GetNumber(rawData, ObjectDataParser.SCALE_X, 1.0f);
            transform.scaleY = ObjectDataParser._GetNumber(rawData, ObjectDataParser.SCALE_Y, 1.0f);
        }

        /**
         * @private
         */
        protected void _ParseColorTransform(Dictionary<string, object> rawData, ColorTransform color)
        {
            color.alphaMultiplier = ObjectDataParser._GetNumber(rawData, ObjectDataParser.ALPHA_MULTIPLIER, 100) * 0.01f;
            color.redMultiplier = ObjectDataParser._GetNumber(rawData, ObjectDataParser.RED_MULTIPLIER, 100) * 0.01f;
            color.greenMultiplier = ObjectDataParser._GetNumber(rawData, ObjectDataParser.GREEN_MULTIPLIER, 100) * 0.01f;
            color.blueMultiplier = ObjectDataParser._GetNumber(rawData, ObjectDataParser.BLUE_MULTIPLIER, 100) * 0.01f;
            color.alphaOffset = ObjectDataParser._GetNumber(rawData, ObjectDataParser.ALPHA_OFFSET, 0);
            color.redOffset = ObjectDataParser._GetNumber(rawData, ObjectDataParser.RED_OFFSET, 0);
            color.greenOffset = ObjectDataParser._GetNumber(rawData, ObjectDataParser.GREEN_OFFSET, 0);
            color.blueOffset = ObjectDataParser._GetNumber(rawData, ObjectDataParser.BLUE_OFFSET, 0);
        }

        /**
         * @private
         */
        protected virtual void _ParseArray(Dictionary<string, object> rawData)
        {

        }

        /**
         * @private
         */
        protected void _ModifyArray()
        {
            // Align.
            
            if ((this._intArray.Count % Helper.INT16_SIZE) != 0)
            {
                this._intArray.Add(0);
            }

            if ((this._frameIntArray.Count % Helper.INT16_SIZE) != 0)
            {
                this._frameIntArray.Add(0);
            }

            if ((this._frameArray.Count % Helper.INT16_SIZE) != 0)
            {
                this._frameArray.Add(0);
            }

            if ((this._timelineArray.Count % Helper.UINT16_SIZE) != 0)
            {
                this._timelineArray.Add(0);
            }

            var l1 = this._intArray.Count * Helper.INT16_SIZE;
            var l2 = this._floatArray.Count * Helper.FLOAT_SIZE;
            var l3 = this._frameIntArray.Count * Helper.INT16_SIZE;
            var l4 = this._frameFloatArray.Count * Helper.FLOAT_SIZE;
            var l5 = this._frameArray.Count * Helper.INT16_SIZE;
            var l6 = this._timelineArray.Count * Helper.UINT16_SIZE;
            
            //TODO
            byte[] buffer = new byte[l1 + l2 + l3 + l4 + l5 + l6];

            var intArray = new short[l1 / Helper.FLOAT_SIZE];
            var floatArray = new float[l2 / Helper.FLOAT_SIZE];
            short[] frameIntArray = new short[l3 / Helper.INT16_SIZE];
            float[] frameFloatArray = new float[l4 / Helper.FLOAT_SIZE];
            short[] frameArray = new short[l5 / Helper.INT16_SIZE];
            ushort[] timelineArray = new ushort[l6 / Helper.UINT16_SIZE];

            for (var i = 0; i < this._intArray.Count; ++i)
            {
                intArray[i] = (short)this._intArray[i];
            }

            for (var i = 0; i < this._floatArray.Count; ++i)
            {
                floatArray[i] = this._floatArray[i];
            }

            for (var i = 0; i < this._frameIntArray.Count; ++i)
            {
                frameIntArray[i] = (short)this._frameIntArray[i];
            }

            for (var i = 0; i < this._frameFloatArray.Count; ++i)
            {
                frameFloatArray[i] = this._frameFloatArray[i];
            }

            for (var i = 0; i < this._frameArray.Count; ++i)
            {
                frameArray[i] = (short)this._frameArray[i];
            }

            for (var i = 0; i < this._timelineArray.Count; ++i)
            {
                timelineArray[i] = (ushort)this._timelineArray[i];
            }

            this._data.intArray = intArray;
            this._data.floatArray = floatArray;
            this._data.frameIntArray = frameIntArray;
            this._data.frameFloatArray = frameFloatArray;
            this._data.frameArray = frameArray;
            this._data.timelineArray = timelineArray;

            this._defalultColorOffset = -1;
            this._intArray.Clear();
            this._floatArray.Clear();
            this._frameIntArray.Clear();
            this._frameFloatArray.Clear();
            this._frameArray.Clear();
            this._timelineArray.Clear();
        }
        /**
         * @inheritDoc
         */
        public override DragonBonesData ParseDragonBonesData(object rawObj, float scale = 1.0f)
        {
            var rawData = rawObj as Dictionary<string, object>;
            Helper.Assert(rawData != null, "Data error.");

            var version = ObjectDataParser._GetString(rawData, ObjectDataParser.VERSION, "");
            var compatibleVersion = ObjectDataParser._GetString(rawData, ObjectDataParser.COMPATIBLE_VERSION, "");

            if (ObjectDataParser.DATA_VERSIONS.IndexOf(version) >= 0 ||
                ObjectDataParser.DATA_VERSIONS.IndexOf(compatibleVersion) >= 0)
            {
                var data = BaseObject.BorrowObject<DragonBonesData>();
                data.version = version;
                data.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, "");
                data.frameRate = ObjectDataParser._GetNumber(rawData, ObjectDataParser.FRAME_RATE, (uint)24);

                if (data.frameRate == 0)
                {
                    // Data error.
                    data.frameRate = 24;
                }

                if (rawData.ContainsKey(ObjectDataParser.ARMATURE))
                {
                    this._data = data;

                    this._ParseArray(rawData);

                    var rawArmatures = rawData[ObjectDataParser.ARMATURE] as List<object>;
                    foreach (Dictionary<string, object> rawArmature in rawArmatures)
                    {
                        data.AddArmature(this._ParseArmature(rawArmature, scale));
                    }

                    if (this._data.intArray == null)
                    {
                        this._ModifyArray();
                    }

                    this._data = null;
                }

                this._rawTextureAtlasIndex = 0;
                if (rawData.ContainsKey(ObjectDataParser.TEXTURE_ATLAS))
                {
                    this._rawTextureAtlases = rawData[ObjectDataParser.TEXTURE_ATLAS] as List<object>;
                }
                else
                {
                    this._rawTextureAtlases = null;
                }

                return data;
            }
            else
            {
                Helper.Assert(
                    false,
                    "Nonsupport data version: " + version + "\n" +
                    "Please convert DragonBones data to support version.\n" +
                    "Read more: https://github.com/DragonBones/Tools/"
                );
            }

            return null;
        }

        /**
         * @inheritDoc
         */
        public override bool ParseTextureAtlasData(object rawObj, TextureAtlasData textureAtlasData, float scale = 0.0f)
        {
            var rawData = rawObj as Dictionary<string, object>;
            if (rawData == null)
            {
                if (this._rawTextureAtlases == null)
                {
                    return false;
                }

                var rawTextureAtlas = this._rawTextureAtlases[this._rawTextureAtlasIndex++];
                this.ParseTextureAtlasData(rawTextureAtlas, textureAtlasData, scale);
                if (this._rawTextureAtlasIndex >= this._rawTextureAtlases.Count)
                {
                    this._rawTextureAtlasIndex = 0;
                    this._rawTextureAtlases = null;
                }

                return true;
            }

            // Texture format.
            textureAtlasData.width = ObjectDataParser._GetNumber(rawData, ObjectDataParser.WIDTH, 0);
            textureAtlasData.height = ObjectDataParser._GetNumber(rawData, ObjectDataParser.HEIGHT, (uint)0);
            textureAtlasData.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, "");
            textureAtlasData.imagePath = ObjectDataParser._GetString(rawData, ObjectDataParser.IMAGE_PATH, "");

            if (scale > 0.0f)
            { 
                // Use params scale.
                textureAtlasData.scale = scale;
            }
            else
            { 
                // Use data scale.
                scale = textureAtlasData.scale = ObjectDataParser._GetNumber(rawData, ObjectDataParser.SCALE, textureAtlasData.scale);
            }

            scale = 1.0f / scale; //

            if (rawData.ContainsKey(ObjectDataParser.SUB_TEXTURE))
            {
                var rawTextures = rawData[ObjectDataParser.SUB_TEXTURE] as List<object>;

                for (int i = 0, l = rawTextures.Count; i < l; ++i)
                {
                    var rawTexture = rawTextures[i] as Dictionary<string, object>;
                    var textureData = textureAtlasData.CreateTexture();
                    textureData.rotated = ObjectDataParser._GetBoolean(rawTexture, ObjectDataParser.ROTATED, false);
                    textureData.name = ObjectDataParser._GetString(rawTexture, ObjectDataParser.NAME, "");
                    textureData.region.x = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.X, 0.0f) * scale;
                    textureData.region.y = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.Y, 0.0f) * scale;
                    textureData.region.width = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.WIDTH, 0.0f) * scale;
                    textureData.region.height = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.HEIGHT, 0.0f) * scale;

                    var frameWidth = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.FRAME_WIDTH, -1.0f);
                    var frameHeight = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.FRAME_HEIGHT, -1.0f);
                    if (frameWidth > 0.0 && frameHeight > 0.0)
                    {
                        textureData.frame = TextureData.CreateRectangle();
                        textureData.frame.x = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.FRAME_X, 0.0f) * scale;
                        textureData.frame.y = ObjectDataParser._GetNumber(rawTexture, ObjectDataParser.FRAME_Y, 0.0f) * scale;
                        textureData.frame.width = frameWidth * scale;
                        textureData.frame.height = frameHeight * scale;
                    }

                    textureAtlasData.AddTexture(textureData);
                }
            }

            return true;
        }
    }

    class ActionFrame
    {
        public int frameStart = 0;
        public readonly List<int> actions = new List<int>();
    }
}
