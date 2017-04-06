using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public class ObjectDataParser : DataParser
    {
        /**
         * @private
         */
        protected static bool _getBoolean(Dictionary<string, object> rawData, string key, bool defaultValue)
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
        protected static uint _getNumber(Dictionary<string, object> rawData, string key, uint defaultValue)
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
        protected static int _getNumber(Dictionary<string, object> rawData, string key, int defaultValue)
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
        protected static float _getNumber(Dictionary<string, object> rawData, string key, float defaultValue)
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
        protected static string _getString(Dictionary<string, object> rawData, string key, string defaultValue)
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
        /**
         * @private
         */
        protected static T _getParameter<T>(List<object> rawData, int index, T defaultValue)
        {
            if (rawData.Count > index)
            {
                var value = rawData[index];
                if (defaultValue is uint)
                {
                    return (T)(object)Convert.ToUInt32(value);
                }
                else if (defaultValue is int)
                {
                    return (T)(object)Convert.ToInt32(value);
                }
                else if (defaultValue is float)
                {
                    return (T)(object)Convert.ToSingle(value);
                }

                return (T)value;
            }

            return defaultValue;
        }
        /**
         * @private
         */
        public ObjectDataParser()
        {
        }
        /**
         * @private
         */
        protected ArmatureData _parseArmature(Dictionary<string, object> rawData, float scale)
        {
            var armature = BaseObject.BorrowObject<ArmatureData>();
            armature.name = _getString(rawData, NAME, null);
            armature.frameRate = _getNumber(rawData, FRAME_RATE, _data.frameRate);
            armature.scale = scale;

            if (armature.frameRate == 0)
            {
                armature.frameRate = 24;
            }

            if (rawData.ContainsKey(TYPE) && rawData[TYPE] is string)
            {
                armature.type = _getArmatureType(rawData[TYPE] as string);
            }
            else
            {
                armature.type = (ArmatureType)_getNumber(rawData, TYPE, (int)ArmatureType.Armature);
            }

            _armature = armature;
            _rawBones.Clear();

            if (rawData.ContainsKey(AABB))
            {
                var rawAABB = rawData[AABB] as Dictionary<string, object>;
                armature.aabb.x = _getNumber(rawAABB, X, 0.0f);
                armature.aabb.y = _getNumber(rawAABB, Y, 0.0f);
                armature.aabb.width = _getNumber(rawAABB, WIDTH, 0.0f);
                armature.aabb.height = _getNumber(rawAABB, HEIGHT, 0.0f);
            }

            if (rawData.ContainsKey(BONE))
            {
                var rawBones = rawData[BONE] as List<object>;
                foreach (Dictionary<string, object> rawBone in rawBones)
                {
                    var bone = _parseBone(rawBone);
                    armature.AddBone(bone, _getString(rawBone, PARENT, null));
                    _rawBones.Add(bone);
                }
            }

            if (rawData.ContainsKey(IK))
            {
                var rawIKS = rawData[IK] as List<object>;
                foreach (Dictionary<string, object> ikObject in rawIKS)
                {
                    _parseIK(ikObject);
                }
            }

            if (rawData.ContainsKey(SLOT))
            {
                var rawSlots = rawData[SLOT] as List<object>;
                var zOrder = 0;
                foreach (Dictionary<string, object> rawSlot in rawSlots)
                {
                    armature.AddSlot(_parseSlot(rawSlot, zOrder++));
                }
            }

            if (rawData.ContainsKey(SKIN))
            {
                var rawSkins = rawData[SKIN] as List<object>;
                foreach (Dictionary<string, object> rawSkin in rawSkins)
                {
                    armature.AddSkin(_parseSkin(rawSkin));
                }
            }

            if (rawData.ContainsKey(ANIMATION))
            {
                var rawAnimations = rawData[ANIMATION] as List<object>;
                foreach (Dictionary<string, object> rawAnimation in rawAnimations)
                {
                    armature.AddAnimation(_parseAnimation(rawAnimation));
                }
            }

            if (
                rawData.ContainsKey(ACTIONS) ||
                rawData.ContainsKey(DEFAULT_ACTIONS)
            )
            {
                _parseActionData(rawData, armature.actions, null, null);
            }

            if (_isOldData && _isGlobalTransform) // Support 2.x ~ 3.x data.
            {
                _globalToLocal(armature);
            }

            _armature = null;
            _rawBones.Clear();

            return armature;
        }
        /**
         * @private
         */
        protected BoneData _parseBone(Dictionary<string, object> rawData)
        {
            var bone = BaseObject.BorrowObject<BoneData>();
            bone.inheritTranslation = _getBoolean(rawData, INHERIT_TRANSLATION, true);
            bone.inheritRotation = _getBoolean(rawData, INHERIT_ROTATION, true);
            bone.inheritScale = _getBoolean(rawData, INHERIT_SCALE, true);
            bone.length = _getNumber(rawData, LENGTH, 0.0f) * _armature.scale;
            bone.name = _getString(rawData, NAME, null);

            if (rawData.ContainsKey(TRANSFORM))
            {
                _parseTransform(rawData[TRANSFORM] as Dictionary<string, object>, bone.transform);
            }

            if (_isOldData) // Support 2.x ~ 3.x data.
            {
                bone.inheritScale = false;
            }

            return bone;
        }
        /**
         * @private
         */
        protected void _parseIK(Dictionary<string, object> rawData)
        {
            var bone = _armature.GetBone(_getString(rawData, rawData.ContainsKey(BONE) ? BONE : NAME, null));
            if (bone != null)
            {
                bone.bendPositive = _getBoolean(rawData, BEND_POSITIVE, true);
                bone.chain = _getNumber(rawData, CHAIN, (uint)0);
                bone.weight = _getNumber(rawData, WEIGHT, 1.0f);
                bone.ik = _armature.GetBone(_getString(rawData, TARGET, null));

                if (bone.chain > 0 && bone.parent != null && bone.parent.ik == null)
                {
                    bone.parent.ik = bone.ik;
                    bone.parent.chainIndex = 0;
                    bone.parent.chain = 0;
                    bone.chainIndex = 1;
                }
                else
                {
                    bone.chain = 0;
                    bone.chainIndex = 0;
                }
            }
        }
        /**
         * @private
         */
        protected SlotData _parseSlot(Dictionary<string, object> rawData, int zOrder)
        {
            var slot = BaseObject.BorrowObject<SlotData>();
            slot.displayIndex = _getNumber(rawData, DISPLAY_INDEX, (int)0);
            slot.zOrder = _getNumber(rawData, Z, zOrder); // Support 2.x ~ 3.x data.
            slot.name = _getString(rawData, NAME, null);
            slot.parent = _armature.GetBone(_getString(rawData, PARENT, null));

            if (rawData.ContainsKey(COLOR) || rawData.ContainsKey(COLOR_TRANSFORM))
            {
                slot.color = SlotData.GenerateColor();
                _parseColorTransform((rawData.ContainsKey(COLOR) ? rawData[COLOR] : rawData[COLOR_TRANSFORM]) as Dictionary<string, object>, slot.color);
            }
            else
            {
                slot.color = SlotData.DEFAULT_COLOR;
            }

            if (rawData.ContainsKey(BLEND_MODE) && rawData[BLEND_MODE] is string)
            {
                slot.blendMode = _getBlendMode(rawData[BLEND_MODE] as string);
            }
            else
            {
                slot.blendMode = (BlendMode)_getNumber(rawData, BLEND_MODE, (int)BlendMode.Normal);
            }

            if (rawData.ContainsKey(ACTIONS) || rawData.ContainsKey(DEFAULT_ACTIONS))
            {
                _parseActionData(rawData, slot.actions, null, null);
            }

            if (_isOldData) // Support 2.x ~ 3.x data.
            {
                if (rawData.ContainsKey(COLOR_TRANSFORM))
                {
                    slot.color = SlotData.GenerateColor();
                    _parseColorTransform(rawData[COLOR_TRANSFORM] as Dictionary<string, object>, slot.color);
                }
                else
                {
                    slot.color = SlotData.DEFAULT_COLOR;
                }
            }

            return slot;
        }
        /**
         * @private
         */
        protected SkinData _parseSkin(Dictionary<string, object> rawData)
        {
            var skin = BaseObject.BorrowObject<SkinData>();
            skin.name = _getString(rawData, NAME, DEFAULT_NAME);
            if (string.IsNullOrEmpty(skin.name))
            {
                skin.name = DEFAULT_NAME;
            }

            if (rawData.ContainsKey(SLOT))
            {
                _skin = skin;

                var slots = rawData[SLOT] as List<object>;
                int zOrder = 0;
                foreach (Dictionary<string, object> slot in slots)
                {
                    if (_isOldData) // Support 2.x ~ 3.x data.
                    {
                        _armature.AddSlot(_parseSlot(slot, zOrder++));
                    }

                    skin.AddSlot(_parseSkinSlotData(slot));
                }

                _skin = null;
            }

            return skin;
        }
        /**
         * @private
         */
        protected SkinSlotData _parseSkinSlotData(Dictionary<string, object> rawData)
        {
            var skinSlotData = BaseObject.BorrowObject<SkinSlotData>();
            skinSlotData.slot = _armature.GetSlot(_getString(rawData, NAME, null));

            if (rawData.ContainsKey(DISPLAY))
            {
                _skinSlotData = skinSlotData;

                foreach (Dictionary<string, object> rawDisplay in rawData[DISPLAY] as List<object>)
                {
                    skinSlotData.displays.Add(_parseDisplay(rawDisplay));
                }

                _skinSlotData = null;
            }

            return skinSlotData;
        }
        /**
         * @private
         */
        protected DisplayData _parseDisplay(Dictionary<string, object> rawData)
        {
            var display = BaseObject.BorrowObject<DisplayData>();
            display.inheritAnimation = _getBoolean(rawData, INHERIT_ANIMATION, true);
            display.name = _getString(rawData, NAME, null);
            display.path = _getString(rawData, PATH, display.name);

            if (rawData.ContainsKey(TYPE) && rawData[TYPE] is string)
            {
                display.type = _getDisplayType(rawData[TYPE] as string);
            }
            else
            {
                display.type = (DisplayType)_getNumber(rawData, TYPE, (int)DisplayType.Image);
            }

            display.isRelativePivot = true;
            if (rawData.ContainsKey(PIVOT))
            {
                var pivotObject = rawData[PIVOT] as Dictionary<string, object>;
                display.pivot.x = _getNumber(pivotObject, X, 0.0f);
                display.pivot.y = _getNumber(pivotObject, Y, 0.0f);
            }
            else if (_isOldData) // Support 2.x ~ 3.x data.
            {
                var transformObject = rawData[TRANSFORM] as Dictionary<string, object>;
                display.isRelativePivot = false;
                display.pivot.x = _getNumber(transformObject, PIVOT_X, 0.0f) * _armature.scale;
                display.pivot.y = _getNumber(transformObject, PIVOT_Y, 0.0f) * _armature.scale;
            }
            else
            {
                display.pivot.x = 0.5f;
                display.pivot.y = 0.5f;
            }

            if (rawData.ContainsKey(TRANSFORM))
            {
                _parseTransform(rawData[TRANSFORM] as Dictionary<string, object>, display.transform);
            }

            switch (display.type)
            {
                case DisplayType.Image:
                    break;

                case DisplayType.Armature:
                    break;

                case DisplayType.Mesh:
                    display.share = _getString(rawData, SHARE, null);
                    if (string.IsNullOrEmpty(display.share))
                    {
                        display.inheritAnimation = _getBoolean(rawData, INHERIT_FFD, true);
                        display.mesh = _parseMesh(rawData);
                        _skinSlotData.AddMesh(display.mesh);
                    }
                    break;

                case DisplayType.BoundingBox:
                    display.boundingBox = _parseBoundingBox(rawData);
                    break;

                default:
                    break;
            }

            return display;
        }
        /**
         * @private
         */
        protected MeshData _parseMesh(Dictionary<string, object> rawData)
        {
            var mesh = BaseObject.BorrowObject<MeshData>();

            var rawVertices = rawData[VERTICES] as List<object>;
            var rawUVs = rawData[UVS] as List<object>;
            var rawTriangles = rawData[TRIANGLES] as List<object>;

            var numVertices = (int)(rawVertices.Count / 2); // uint
            var numTriangles = (int)(rawTriangles.Count / 3); // uint

            var inverseBindPose = new List<Matrix>(_armature.sortedBones.Count);
            DragonBones.ResizeList(inverseBindPose, _armature.sortedBones.Count, null);

            mesh.skinned = rawData.ContainsKey(WEIGHTS) && (rawData[WEIGHTS] as List<object>).Count > 0;
            mesh.name = _getString(rawData, NAME, null);

            DragonBones.ResizeList(mesh.uvs, numVertices * 2, 0.0f);
            DragonBones.ResizeList(mesh.vertices, numVertices * 2, 0.0f);
            DragonBones.ResizeList(mesh.vertexIndices, numTriangles * 3, 0);

            if (mesh.skinned)
            {
                DragonBones.ResizeList(mesh.boneIndices, numVertices, null);
                DragonBones.ResizeList(mesh.weights, numVertices, null);
                DragonBones.ResizeList(mesh.boneVertices, numVertices, null);

                if (rawData.ContainsKey(SLOT_POSE))
                {
                    var rawSlotPose = rawData[SLOT_POSE] as List<object>;
                    mesh.slotPose.a = _getParameter(rawSlotPose, 0, 1.0f);
                    mesh.slotPose.b = _getParameter(rawSlotPose, 1, 0.0f);
                    mesh.slotPose.c = _getParameter(rawSlotPose, 2, 0.0f);
                    mesh.slotPose.d = _getParameter(rawSlotPose, 3, 1.0f);
                    mesh.slotPose.tx = _getParameter(rawSlotPose, 4, 0.0f) * _armature.scale;
                    mesh.slotPose.ty = _getParameter(rawSlotPose, 5, 0.0f) * _armature.scale;
                }

                if (rawData.ContainsKey(BONE_POSE))
                {
                    var rawBonePose = rawData[BONE_POSE] as List<object>;
                    for (int i = 0, l = rawBonePose.Count; i < l; i += 7)
                    {
                        var rawBoneIndex = Convert.ToInt32(rawBonePose[i]); // uint
                        var boneMatrix = inverseBindPose[rawBoneIndex] = new Matrix();
                        boneMatrix.a = _getParameter(rawBonePose, i + 1, 1.0f);
                        boneMatrix.b = _getParameter(rawBonePose, i + 2, 0.0f);
                        boneMatrix.c = _getParameter(rawBonePose, i + 3, 0.0f);
                        boneMatrix.d = _getParameter(rawBonePose, i + 4, 1.0f);
                        boneMatrix.tx = _getParameter(rawBonePose, i + 5, 0.0f) * _armature.scale;
                        boneMatrix.ty = _getParameter(rawBonePose, i + 6, 0.0f) * _armature.scale;
                        boneMatrix.Invert();

                    }
                }
            }

            for (int i = 0, iW = 0, l = rawVertices.Count; i < l; i += 2)
            {
                var iN = i + 1;
                var vertexIndex = i / 2;

                var x = mesh.vertices[i] = _getParameter(rawVertices, i, 0.0f) * _armature.scale;
                var y = mesh.vertices[iN] = _getParameter(rawVertices, iN, 0.0f) * _armature.scale;
                mesh.uvs[i] = _getParameter(rawUVs, i, 0.0f);
                mesh.uvs[iN] = _getParameter(rawUVs, iN, 0.0f);

                if (mesh.skinned) // If mesh is skinned, transform point by bone bind pose.
                {
                    var rawWeights = rawData[WEIGHTS] as List<object>;
                    var numBones = _getParameter(rawWeights, iW, 0);
                    var indices = mesh.boneIndices[vertexIndex] = new int[numBones];
                    var weights = mesh.weights[vertexIndex] = new float[numBones];
                    var boneVertices = mesh.boneVertices[vertexIndex] = new float[numBones * 2];

                    mesh.slotPose.TransformPoint(x, y, _helpPoint);
                    x = mesh.vertices[i] = _helpPoint.x;
                    y = mesh.vertices[iN] = _helpPoint.y;

                    for (int iB = 0; iB < numBones; ++iB)
                    {
                        var iI = iW + 1 + iB * 2;
                        var rawBoneIndex = Convert.ToInt32(rawWeights[iI]); // uint
                        var boneData = _rawBones[rawBoneIndex];

                        var boneIndex = mesh.bones.IndexOf(boneData);
                        if (boneIndex < 0)
                        {
                            boneIndex = mesh.bones.Count;
                            DragonBones.ResizeList(mesh.bones, boneIndex + 1, null);
                            DragonBones.ResizeList(mesh.inverseBindPose, boneIndex + 1, null);
                            mesh.bones[boneIndex] = boneData;
                            mesh.inverseBindPose[boneIndex] = inverseBindPose[rawBoneIndex];
                        }

                        mesh.inverseBindPose[boneIndex].TransformPoint(x, y, _helpPoint);

                        indices[iB] = boneIndex;
                        weights[iB] = _getParameter(rawWeights, iI + 1, 0.0f);
                        boneVertices[iB * 2] = _helpPoint.x;
                        boneVertices[iB * 2 + 1] = _helpPoint.y;
                    }

                    iW += numBones * 2 + 1;
                }
            }

            for (int i = 0, l = rawTriangles.Count; i < l; ++i)
            {
                mesh.vertexIndices[i] = _getParameter(rawTriangles, i, 0);
            }

            return mesh;
        }
        /**
         * @private
         */
        protected BoundingBoxData _parseBoundingBox(Dictionary<string, object> rawData)
        {
            var boundingBox = BaseObject.BorrowObject<BoundingBoxData>();

            if (rawData.ContainsKey(SUB_TYPE) && rawData[SUB_TYPE] is string)
            {
                boundingBox.type = _getBoundingBoxType(rawData[SUB_TYPE] as string);
            }
            else
            {
                boundingBox.type = (BoundingBoxType)_getNumber(rawData, SUB_TYPE, (int)BoundingBoxType.Rectangle);
            }

            boundingBox.color = _getNumber(rawData, COLOR, (uint)0x000000);

            switch (boundingBox.type)
            {
                case BoundingBoxType.Rectangle:
                case BoundingBoxType.Ellipse:
                    boundingBox.width = _getNumber(rawData, WIDTH, 0.0f);
                    boundingBox.height = _getNumber(rawData, HEIGHT, 0.0f);
                    break;

                case BoundingBoxType.Polygon:
                    if (rawData.ContainsKey(VERTICES))
                    {
                        var rawVertices = rawData[VERTICES] as List<object>;
                        DragonBones.ResizeList(boundingBox.vertices, rawVertices.Count, 0.0f);
                        for (int i = 0, l = boundingBox.vertices.Count; i < l; i += 2)
                        {
                            var iN = i + 1;
                            var x = _getParameter(rawVertices, i, 0.0f);
                            var y = _getParameter(rawVertices, iN, 0.0f);
                            boundingBox.vertices[i] = x;
                            boundingBox.vertices[iN] = y;

                            // AABB.
                            if (i == 0)
                            {
                                boundingBox.x = x;
                                boundingBox.y = y;
                                boundingBox.width = x;
                                boundingBox.height = y;
                            }
                            else
                            {
                                if (x < boundingBox.x)
                                {
                                    boundingBox.x = x;
                                }
                                else if (x > boundingBox.width)
                                {
                                    boundingBox.width = x;
                                }

                                if (y < boundingBox.y)
                                {
                                    boundingBox.y = y;
                                }
                                else if (y > boundingBox.height)
                                {
                                    boundingBox.height = y;
                                }
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            return boundingBox;
        }
        /**
         * @private
         */
        protected AnimationData _parseAnimation(Dictionary<string, object> rawData)
        {
            var animation = BaseObject.BorrowObject<AnimationData>();
            animation.frameCount = Math.Max(_getNumber(rawData, DURATION, (uint)1), 1);
            animation.playTimes = _getNumber(rawData, PLAY_TIMES, (uint)1);
            animation.duration = (float)animation.frameCount / _armature.frameRate;
            animation.fadeInTime = _getNumber(rawData, FADE_IN_TIME, 0.0f);
            animation.name = _getString(rawData, NAME, DEFAULT_NAME);
            if (string.IsNullOrEmpty(animation.name))
            {
                animation.name = DEFAULT_NAME;
            }

            _animation = animation;

            _parseTimeline(rawData, animation, _parseAnimationFrame);

            if (rawData.ContainsKey(Z_ORDER))
            {
                animation.zOrderTimeline = BaseObject.BorrowObject<ZOrderTimelineData>();
                _parseTimeline(rawData[Z_ORDER] as Dictionary<string, object>, animation.zOrderTimeline, _parseZOrderFrame);
            }

            if (rawData.ContainsKey(BONE))
            {
                var boneTimelines = rawData[BONE] as List<object>;
                foreach (Dictionary<string, object> boneTimelineObject in boneTimelines)
                {
                    animation.AddBoneTimeline(_parseBoneTimeline(boneTimelineObject));
                }
            }

            if (rawData.ContainsKey(SLOT))
            {
                var slotTimelines = rawData[SLOT] as List<object>;
                foreach (Dictionary<string, object> slotTimelineObject in slotTimelines)
                {
                    animation.AddSlotTimeline(_parseSlotTimeline(slotTimelineObject));
                }
            }

            if (rawData.ContainsKey(FFD))
            {
                var ffdTimelines = rawData[FFD] as List<object>;
                foreach (Dictionary<string, object> ffdTimelineObject in ffdTimelines)
                {
                    animation.AddFFDTimeline(_parseFFDTimeline(ffdTimelineObject));
                }
            }

            if (_isOldData) // Support 2.x ~ 3.x data.
            {
                _isAutoTween = _getBoolean(rawData, AUTO_TWEEN, true);
                _animationTweenEasing = _getNumber(rawData, TWEEN_EASING, 0.0f);
                animation.playTimes = _getNumber(rawData, LOOP, (uint)1);

                if (rawData.ContainsKey(TIMELINE))
                {
                    var timelines = rawData[TIMELINE] as List<object>;
                    foreach (Dictionary<string, object> timelineObjects in timelines)
                    {
                        animation.AddBoneTimeline(_parseBoneTimeline(timelineObjects));
                        animation.AddSlotTimeline(_parseSlotTimeline(timelineObjects));
                    }
                }
            }
            else
            {
                _isAutoTween = false;
                _animationTweenEasing = 0.0f;
            }

            foreach (var pair in _armature.bones)
            {
                var bone = pair.Value;
                if (animation.GetBoneTimeline(bone.name) == null) // Add default bone timeline for cache if do not have one.
                {
                    var boneTimeline = BaseObject.BorrowObject<BoneTimelineData>();
                    var boneFrame = BaseObject.BorrowObject<BoneFrameData>();
                    boneTimeline.bone = bone;
                    boneTimeline.frames.Add(boneFrame);
                    animation.AddBoneTimeline(boneTimeline);
                }
            }

            foreach (var pair in _armature.slots)
            {
                var slot = pair.Value;
                if (animation.GetSlotTimeline(slot.name) == null) // Add default slot timeline for cache if do not have one.
                {
                    var slotTimeline = BaseObject.BorrowObject<SlotTimelineData>();
                    var slotFrame = BaseObject.BorrowObject<SlotFrameData>();
                    slotTimeline.slot = slot;
                    slotFrame.displayIndex = slot.displayIndex;
                    //slotFrame.zOrder = -2; // TODO zOrder.

                    if (slot.color == SlotData.DEFAULT_COLOR)
                    {
                        slotFrame.color = SlotFrameData.DEFAULT_COLOR;
                    }
                    else
                    {
                        slotFrame.color = SlotFrameData.GenerateColor();
                        slotFrame.color.CopyFrom(slot.color);
                    }

                    slotTimeline.frames.Add(slotFrame);
                    animation.AddSlotTimeline(slotTimeline);

                    if (_isOldData) // Support 2.x ~ 3.x data.
                    {
                        slotFrame.displayIndex = -1;
                    }
                }
            }

            _animation = null;

            return animation;
        }
        /**
         * @private
         */
        protected BoneTimelineData _parseBoneTimeline(Dictionary<string, object> rawData)
        {
            var timeline = BaseObject.BorrowObject<BoneTimelineData>();
            timeline.bone = _armature.GetBone(_getString(rawData, NAME, null));

            _parseTimeline(rawData, timeline, _parseBoneFrame);

            var originTransform = timeline.originTransform;
            BoneFrameData prevFrame = null;

            foreach (var frame in timeline.frames) // bone transform pose = origin + animation origin + animation.
            {
                if (prevFrame == null)
                {
                    originTransform.CopyFrom(frame.transform);
                    frame.transform.Identity();

                    if (originTransform.scaleX == 0.0f) // Pose scale and origin scale can not be 0. (poseScale = originScale * animationOriginScale * animationScale)
                    {
                        originTransform.scaleX = 0.001f;
                        //frame.transform.scaleX = 0.0f;
                    }

                    if (originTransform.scaleY == 0.0f)
                    {
                        originTransform.scaleY = 0.001f;
                        //frame.transform.scaleY = 0.0f;
                    }
                }
                else if (prevFrame != frame)
                {
                    frame.transform.Minus(originTransform);
                }

                prevFrame = frame;
            }

            if (_isOldData && (rawData.ContainsKey(PIVOT_X) || rawData.ContainsKey(PIVOT_Y))) // Support 2.x ~ 3.x data.
            {
                _timelinePivot.x = _getNumber(rawData, PIVOT_X, 0.0f) * _armature.scale;
                _timelinePivot.y = _getNumber(rawData, PIVOT_Y, 0.0f) * _armature.scale;
            }
            else
            {
                _timelinePivot.Clear();
            }

            return timeline;
        }
        /**
         * @private
         */
        protected SlotTimelineData _parseSlotTimeline(Dictionary<string, object> rawData)
        {
            var timeline = BaseObject.BorrowObject<SlotTimelineData>();
            timeline.slot = _armature.GetSlot(_getString(rawData, NAME, null));

            _parseTimeline(rawData, timeline, _parseSlotFrame);

            return timeline;
        }
        /**
         * @private
         */
        protected FFDTimelineData _parseFFDTimeline(Dictionary<string, object> rawData)
        {
            var timeline = BaseObject.BorrowObject<FFDTimelineData>();
            timeline.skin = _armature.GetSkin(_getString(rawData, SKIN, null));
            timeline.slot = timeline.skin.GetSlot(_getString(rawData, SLOT, null)); // NAME;

            var meshName = _getString(rawData, NAME, null);
            for (int i = 0, l = timeline.slot.displays.Count; i < l; ++i)
            {
                var display = timeline.slot.displays[i];
                if (display.mesh != null && display.name == meshName)
                {
                    timeline.display = display;
                    break;
                }
            }

            _parseTimeline(rawData, timeline, _parseFFDFrame);

            return timeline;
        }
        /**
         * @private
         */
        protected AnimationFrameData _parseAnimationFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.BorrowObject<AnimationFrameData>();

            _parseFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(ACTION) || rawData.ContainsKey(ACTIONS))
            {
                _parseActionData(rawData, frame.actions, null, null);
            }

            if (rawData.ContainsKey(EVENTS) || rawData.ContainsKey(EVENT) || rawData.ContainsKey(SOUND))
            {
                _parseEventData(rawData, frame.events, null, null);
            }

            return frame;
        }
        /**
         * @private
         */
        protected ZOrderFrameData _parseZOrderFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.BorrowObject<ZOrderFrameData>();

            _parseFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(Z_ORDER))
            {
                var rawZOrder = rawData[Z_ORDER] as List<object>;
                if (rawZOrder.Count > 0)
                {
                    var slotCount = _armature.sortedSlots.Count;
                    var unchanged = new int[slotCount - rawZOrder.Count / 2];

                    DragonBones.ResizeList(frame.zOrder, slotCount, -1);
                    for (int i = 0; i < slotCount; ++i)
                    {
                        frame.zOrder[i] = -1;
                    }

                    var originalIndex = 0;
                    var unchangedIndex = 0;
                    for (int i = 0, l = rawZOrder.Count; i < l; i += 2)
                    {
                        var slotIndex = _getParameter(rawZOrder, i, 0);
                        var offset = _getParameter(rawZOrder, i + 1, 0);

                        while (originalIndex != slotIndex)
                        {
                            unchanged[unchangedIndex++] = originalIndex++;
                        }

                        frame.zOrder[originalIndex + offset] = originalIndex++;
                    }

                    while (originalIndex < slotCount)
                    {
                        unchanged[unchangedIndex++] = originalIndex++;
                    }

                    var iC = slotCount;
                    while (iC-- != 0)
                    {
                        if (frame.zOrder[iC] == -1)
                        {
                            frame.zOrder[iC] = unchanged[--unchangedIndex];
                        }
                    }
                }
            }

            return frame;
        }
        /**
         * @private
         */
        protected BoneFrameData _parseBoneFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.BorrowObject<BoneFrameData>();
            frame.tweenRotate = _getNumber(rawData, TWEEN_ROTATE, 0.0f);
            frame.tweenScale = _getBoolean(rawData, TWEEN_SCALE, true);

            _parseTweenFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(TRANSFORM))
            {
                var transformObject = rawData[TRANSFORM] as Dictionary<string, object>;
                _parseTransform(transformObject, frame.transform);

                if (_isOldData) // Support 2.x ~ 3.x data.
                {
                    _helpPoint.x = _timelinePivot.x + _getNumber(transformObject, PIVOT_X, 0.0f) * _armature.scale;
                    _helpPoint.y = _timelinePivot.y + _getNumber(transformObject, PIVOT_Y, 0.0f) * _armature.scale;
                    frame.transform.ToMatrix(_helpMatrix);
                    _helpMatrix.TransformPoint(_helpPoint.x, _helpPoint.y, _helpPoint, true);
                    frame.transform.x += _helpPoint.x;
                    frame.transform.y += _helpPoint.y;
                }
            }

            var bone = (_timeline as BoneTimelineData).bone;
            var actions = new List<ActionData>();
            var events = new List<EventData>();

            if (rawData.ContainsKey(ACTION) || rawData.ContainsKey(ACTIONS))
            {
                var slot = _armature.GetSlot(bone.name);
                _parseActionData(rawData, actions, bone, slot);
            }

            if (rawData.ContainsKey(EVENT) || rawData.ContainsKey(SOUND))
            {
                _parseEventData(rawData, events, bone, null);
            }

            if (actions.Count > 0 || events.Count > 0)
            {
                _mergeFrameToAnimationTimeline(frame.position, actions, events); // Merge actions and events to animation timeline.
            }

            return frame;
        }
        /**
         * @private
         */
        protected SlotFrameData _parseSlotFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.BorrowObject<SlotFrameData>();
            frame.displayIndex = _getNumber(rawData, DISPLAY_INDEX, 0);

            _parseTweenFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(COLOR) || rawData.ContainsKey(COLOR_TRANSFORM)) // Support 2.x ~ 3.x data. (colorTransform key)
            {
                frame.color = SlotFrameData.GenerateColor();
                _parseColorTransform((rawData.ContainsKey(COLOR) ? rawData[COLOR] : rawData[COLOR_TRANSFORM]) as Dictionary<string, object>, frame.color);
            }
            else
            {
                frame.color = SlotFrameData.DEFAULT_COLOR;
            }

            if (_isOldData) // Support 2.x ~ 3.x data.
            {
                if (_getBoolean(rawData, HIDE, false))
                {
                    frame.displayIndex = -1;
                }
            }
            else if (rawData.ContainsKey(ACTION) || rawData.ContainsKey(ACTIONS))
            {
                var slot = (_timeline as SlotTimelineData).slot;
                var actions = new List<ActionData>();
                _parseActionData(rawData, actions, slot.parent, slot);

                _mergeFrameToAnimationTimeline(frame.position, actions, null); // Merge actions and events to animation timeline.
            }

            return frame;
        }
        /**
         * @private
         */
        protected ExtensionFrameData _parseFFDFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var ffdTimeline = _timeline as FFDTimelineData;
            var mesh = ffdTimeline.display.mesh;

            var frame = BaseObject.BorrowObject<ExtensionFrameData>();

            _parseTweenFrame(rawData, frame, frameStart, frameCount);

            var rawVertices = rawData.ContainsKey(VERTICES) ? rawData[VERTICES] as List<object> : null;
            var offset = _getNumber(rawData, OFFSET, 0); // uint
            var x = 0.0f;
            var y = 0.0f;
            for (int i = 0, l = mesh.vertices.Count; i < l; i += 2)
            {
                if (rawVertices == null || i < offset || i - offset >= rawVertices.Count) // Fill 0.
                {
                    x = 0.0f;
                    y = 0.0f;
                }
                else
                {
                    x = _getParameter(rawVertices, i - offset, 0.0f) * _armature.scale;
                    y = _getParameter(rawVertices, i + 1 - offset, 0.0f) * _armature.scale;
                }

                if (mesh.skinned) // If mesh is skinned, transform point by bone bind pose.
                {
                    mesh.slotPose.TransformPoint(x, y, _helpPoint, true);
                    x = _helpPoint.x;
                    y = _helpPoint.y;

                    var boneIndices = mesh.boneIndices[i / 2];
                    foreach (var boneIndex in boneIndices)
                    {
                        mesh.inverseBindPose[boneIndex].TransformPoint(x, y, _helpPoint, true);
                        frame.tweens.Add(_helpPoint.x);
                        frame.tweens.Add(_helpPoint.y);
                    }
                }
                else
                {
                    frame.tweens.Add(x);
                    frame.tweens.Add(y);
                }
            }

            return frame;
        }
        /**
         * @private
         */
        protected void _parseTweenFrame<T>(Dictionary<string, object> rawData, T frame, uint frameStart, uint frameCount) where T : TweenFrameData<T>
        {
            _parseFrame(rawData, frame, frameStart, frameCount);

            if (frame.duration > 0.0f)
            {
                if (rawData.ContainsKey(TWEEN_EASING))
                {
                    frame.tweenEasing = _getNumber(rawData, TWEEN_EASING, DragonBones.NO_TWEEN);
                }
                else if (_isOldData) // Support 2.x ~ 3.x data.
                {
                    frame.tweenEasing = _isAutoTween ? _animationTweenEasing : DragonBones.NO_TWEEN;
                }
                else
                {
                    frame.tweenEasing = DragonBones.NO_TWEEN;
                }

                if (_isOldData && _animation.scale == 1 && (_timeline as TimelineData<T>).scale == 1.0f && frame.duration * _armature.frameRate < 2)
                {
                    frame.tweenEasing = DragonBones.NO_TWEEN;
                }

                if (rawData.ContainsKey(CURVE))
                {
                    var rawCurve = rawData[CURVE] as List<object>;
                    var curve = new float[rawCurve.Count];
                    for (int i = 0, l = rawCurve.Count; i < l; ++i)
                    {
                        curve[i] = Convert.ToSingle(rawCurve[i]);
                    }

                    frame.curve = new float[frameCount * 2 - 1];
                    TweenFrameData<T>.SamplingEasingCurve(curve, frame.curve);
                }
            }
            else
            {
                frame.tweenEasing = DragonBones.NO_TWEEN;
                frame.curve = null;
            }
        }
        /**
         * @private
         */
        protected void _parseFrame<T>(Dictionary<string, object> rawData, T frame, uint frameStart, uint frameCount) where T : FrameData<T>
        {
            frame.position = (float)frameStart / _armature.frameRate;
            frame.duration = (float)frameCount / _armature.frameRate;
        }
        /**
         * @private
         */
        protected void _parseTimeline<T>(Dictionary<string, object> rawData, TimelineData<T> timeline, Func<Dictionary<string, object>, uint, uint, T> frameParser) where T : FrameData<T>
        {
            timeline.scale = _getNumber(rawData, SCALE, 1.0f);
            timeline.offset = _getNumber(rawData, OFFSET, 0.0f);

            _timeline = timeline;

            if (rawData.ContainsKey(FRAME))
            {
                var rawFrames = rawData[FRAME] as List<object>;
                if (rawFrames.Count > 0)
                {
                    if (rawFrames.Count == 1) // Only one frame.
                    {
                        DragonBones.ResizeList(timeline.frames, 1, null);
                        timeline.frames[0] = frameParser(rawFrames[0] as Dictionary<string, object>, 0, _getNumber(rawFrames[0] as Dictionary<string, object>, DURATION, (uint)1));
                    }
                    else
                    {
                        DragonBones.ResizeList(timeline.frames, (int)_animation.frameCount + 1, null);

                        uint frameStart = 0;
                        uint frameCount = 0;
                        T frame = null;
                        T prevFrame = null;

                        for (int i = 0, iW = 0, l = timeline.frames.Count; i < l; ++i) // Fill frame link.
                        {
                            if (frameStart + frameCount <= i && iW < rawFrames.Count)
                            {
                                var frameObject = rawFrames[iW++] as Dictionary<string, object>;
                                frameStart = (uint)i;
                                frameCount = _getNumber(frameObject, DURATION, (uint)1);
                                frame = frameParser(frameObject, frameStart, frameCount);

                                if (prevFrame != null)
                                {
                                    prevFrame.next = frame;
                                    frame.prev = prevFrame;

                                    if (_isOldData) // Support 2.x ~ 3.x data.
                                    {
                                        // TweenFrameData<T> TODO
                                        if (prevFrame is BoneFrameData && _getNumber(frameObject, DISPLAY_INDEX, 0) == -1)
                                        {
                                            (prevFrame as BoneFrameData).tweenEasing = DragonBones.NO_TWEEN;
                                        }
                                    }
                                }

                                prevFrame = frame;
                            }

                            timeline.frames[i] = frame;
                        }

                        frame.duration = _animation.duration - frame.position; // Modify last frame duration.

                        frame = timeline.frames[0];

                        prevFrame.next = frame;

                        frame.prev = prevFrame;

                        if (_isOldData) // Support 2.x ~ 3.x data.
                        {
                            // TweenFrameData<T> TODO
                            if (prevFrame is BoneFrameData && _getNumber(rawFrames[0] as Dictionary<string, object>, DISPLAY_INDEX, 0) == -1)
                            {
                                (prevFrame as BoneFrameData).tweenEasing = DragonBones.NO_TWEEN;
                            }
                        }
                    }
                }
            }

            _timeline = null;
        }
        /**
         * @private
         */
        protected void _parseActionData(Dictionary<string, object> rawData, List<ActionData> actions, BoneData bone, SlotData slot)
        {
            var actionsObject = rawData.ContainsKey(ACTION) ? rawData[ACTION] : (rawData.ContainsKey(ACTIONS) ? rawData[ACTIONS] : rawData[DEFAULT_ACTIONS]);
            if (actionsObject is string) // Support string action.
            {
                var actionData = BaseObject.BorrowObject<ActionData>();
                actionData.type = ActionType.Play;
                actionData.bone = bone;
                actionData.slot = slot;
                actionData.animationConfig = BaseObject.BorrowObject<AnimationConfig>();
                actionData.animationConfig.animationName = actionsObject as string;
                actions.Add(actionData);
            }
            else if (actionsObject is IList) // Support [{gotoAndPlay: "animationName"}, ...] or [["gotoAndPlay", "animationName", ...], ...]
            {
                foreach (var actionObject in actionsObject as List<object>)
                {
                    var isArray = actionObject is IList;
                    var actionData = BaseObject.BorrowObject<ActionData>();
                    var animationName = isArray ? _getParameter(actionObject as List<object>, 1, "") : _getString(actionObject as Dictionary<string, object>, "gotoAndPlay", null);

                    if (isArray)
                    {
                        var actionType = (actionObject as List<object>)[0];
                        if (actionType is string)
                        {
                            actionData.type = _getActionType(actionType as string);
                        }
                        else
                        {
                            actionData.type = _getParameter(actionObject as List<object>, 0, ActionType.Play);
                        }
                    }
                    else
                    {
                        actionData.type = ActionType.Play;
                    }

                    switch (actionData.type)
                    {
                        case ActionType.Play:
                            actionData.animationConfig = BaseObject.BorrowObject<AnimationConfig>();
                            actionData.animationConfig.animationName = animationName;
                            break;

                        default:
                            break;
                    }

                    actionData.bone = bone;
                    actionData.slot = slot;
                    actions.Add(actionData);
                }
            }
        }
        /**
         * @private
         */
        protected void _parseEventData(Dictionary<string, object> rawData, List<EventData> events, BoneData bone, SlotData slot)
        {
            if (rawData.ContainsKey(SOUND))
            {
                var soundEventData = BaseObject.BorrowObject<EventData>();
                soundEventData.type = EventType.Sound;
                soundEventData.name = _getString(rawData, SOUND, null);
                soundEventData.bone = bone;
                soundEventData.slot = slot;
                events.Add(soundEventData);
            }

            if (rawData.ContainsKey(EVENT))
            {
                var eventData = BaseObject.BorrowObject<EventData>();
                eventData.type = EventType.Frame;
                eventData.name = _getString(rawData, EVENT, null);
                eventData.bone = bone;
                eventData.slot = slot;

                events.Add(eventData);
            }

            if (rawData.ContainsKey(EVENTS))
            {
                var rawEvents = rawData[EVENTS] as List<object>;
                for (int i = 0, l = rawEvents.Count; i < l; ++i)
                {
                    var rawEvent = rawEvents[i] as Dictionary<string, object>;
                    var boneName = _getString(rawEvent, BONE, null);
                    var slotName = _getString(rawEvent, SLOT, null);
                    var eventData = BaseObject.BorrowObject<EventData>();

                    eventData.type = EventType.Frame;
                    eventData.name = _getString(rawEvent, NAME, null);
                    eventData.bone = _armature.GetBone(boneName);
                    eventData.slot = _armature.GetSlot(slotName);

                    if (rawEvent.ContainsKey(INTS))
                    {
                        if (eventData.data == null)
                        {
                            eventData.data = BaseObject.BorrowObject<CustomData>();
                        }

                        var rawInts = rawEvent[INTS] as List<object>;
                        for (int j = 0, lJ = rawInts.Count; j < lJ; ++j)
                        {
                            eventData.data.ints.Add(_getParameter(rawInts, j, (int)0));
                        }
                    }

                    if (rawEvent.ContainsKey(FLOATS))
                    {
                        if (eventData.data == null)
                        {
                            eventData.data = BaseObject.BorrowObject<CustomData>();
                        }

                        var rawFloats = rawEvent[FLOATS] as List<object>;
                        for (int j = 0, lJ = rawFloats.Count; j < lJ; ++j)
                        {
                            eventData.data.floats.Add(_getParameter(rawFloats, j, 0.0f));
                        }
                    }

                    if (rawEvent.ContainsKey(STRINGS))
                    {
                        if (eventData.data == null)
                        {
                            eventData.data = BaseObject.BorrowObject<CustomData>();
                        }

                        var rawStrings = rawEvent[STRINGS] as List<object>;
                        for (int j = 0, lJ = rawStrings.Count; j < lJ; ++j)
                        {
                            eventData.data.strings.Add(_getParameter(rawStrings, j, ""));
                        }
                    }

                    events.Add(eventData);
                }
            }
        }
        /**
         * @private
         */
        protected void _parseTransform(Dictionary<string, object> rawData, Transform transform)
        {
            transform.x = _getNumber(rawData, X, 0.0f) * _armature.scale;
            transform.y = _getNumber(rawData, Y, 0.0f) * _armature.scale;
            transform.skewX = _getNumber(rawData, SKEW_X, 0.0f) * DragonBones.ANGLE_TO_RADIAN;
            transform.skewY = _getNumber(rawData, SKEW_Y, 0.0f) * DragonBones.ANGLE_TO_RADIAN;
            transform.scaleX = _getNumber(rawData, SCALE_X, 1.0f);
            transform.scaleY = _getNumber(rawData, SCALE_Y, 1.0f);
        }
        /**
         * @private
         */
        protected void _parseColorTransform(Dictionary<string, object> rawData, ColorTransform color)
        {
            color.alphaMultiplier = _getNumber(rawData, ALPHA_MULTIPLIER, 100) * 0.01f;
            color.redMultiplier = _getNumber(rawData, RED_MULTIPLIER, 100) * 0.01f;
            color.greenMultiplier = _getNumber(rawData, GREEN_MULTIPLIER, 100) * 0.01f;
            color.blueMultiplier = _getNumber(rawData, BLUE_MULTIPLIER, 100) * 0.01f;
            color.alphaOffset = _getNumber(rawData, ALPHA_OFFSET, (int)0);
            color.redOffset = _getNumber(rawData, RED_OFFSET, (int)0);
            color.greenOffset = _getNumber(rawData, GREEN_OFFSET, (int)0);
            color.blueOffset = _getNumber(rawData, BLUE_OFFSET, (int)0);
        }
        /**
         * @inheritDoc
         */
        override public DragonBonesData ParseDragonBonesData(Dictionary<string, object> rawData, float scale = 1.0f)
        {
            if (rawData != null)
            {
                var version = _getString(rawData, VERSION, null);
                var compatibleVersion = _getString(rawData, COMPATIBLE_VERSION, null);
                _isOldData = version == DATA_VERSION_2_3 || version == DATA_VERSION_3_0;
                if (_isOldData)
                {
                    _isGlobalTransform = _getBoolean(rawData, IS_GLOBAL, true);
                }
                else
                {
                    _isGlobalTransform = false;
                }

                if (
                    DATA_VERSIONS.Contains(version) ||
                    DATA_VERSIONS.Contains(compatibleVersion)
                )
                {
                    var data = BaseObject.BorrowObject<DragonBonesData>();
                    data.name = _getString(rawData, NAME, null);
                    data.frameRate = _getNumber(rawData, FRAME_RATE, (uint)24);
                    if (data.frameRate == 0)
                    {
                        data.frameRate = 24;
                    }

                    if (rawData.ContainsKey(ARMATURE))
                    {
                        _data = data;

                        var armatures = rawData[ARMATURE] as List<object>;
                        foreach (Dictionary<string, object> rawArmature in armatures)
                        {
                            data.AddArmature(_parseArmature(rawArmature, scale));
                        }

                        _data = null;
                    }

                    return data;
                }
                else
                {
                    DragonBones.Assert(false, "Nonsupport data version.");
                }
            }
            else
            {
                DragonBones.Assert(false, "No data.");
            }

            return null;
        }
        /**
         * @inheritDoc
         */
        public override void ParseTextureAtlasData(Dictionary<string, object> rawData, TextureAtlasData textureAtlasData, float scale = 0.0f)
        {
            if (rawData != null)
            {
                textureAtlasData.name = _getString(rawData, NAME, null);
                textureAtlasData.imagePath = _getString(rawData, IMAGE_PATH, null);
                textureAtlasData.width = _getNumber(rawData, WIDTH, 0.0f);
                textureAtlasData.height = _getNumber(rawData, HEIGHT, 0.0f);
                // Texture format.

                if (scale > 0.0f) // Use params scale.
                {
                    textureAtlasData.scale = scale;
                }
                else // Use data scale.
                {
                    scale = textureAtlasData.scale = _getNumber(rawData, SCALE, textureAtlasData.scale);
                }

                scale = 1.0f / scale;

                if (rawData.ContainsKey(SUB_TEXTURE))
                {
                    var rawTextures = rawData[SUB_TEXTURE] as List<object>;
                    foreach (Dictionary<string, object> rawTexture in rawTextures)
                    {
                        var textureData = textureAtlasData.GenerateTextureData();
                        textureData.name = _getString(rawTexture, NAME, null);
                        textureData.rotated = _getBoolean(rawTexture, ROTATED, false);
                        textureData.region.x = _getNumber(rawTexture, X, 0.0f) * scale;
                        textureData.region.y = _getNumber(rawTexture, Y, 0.0f) * scale;
                        textureData.region.width = _getNumber(rawTexture, WIDTH, 0.0f) * scale;
                        textureData.region.height = _getNumber(rawTexture, HEIGHT, 0.0f) * scale;

                        var frameWidth = _getNumber(rawTexture, FRAME_WIDTH, -1.0f);
                        var frameHeight = _getNumber(rawTexture, FRAME_HEIGHT, -1.0f);
                        if (frameWidth > 0.0f && frameHeight > 0.0f)
                        {
                            textureData.frame = TextureData.GenerateRectangle();
                            textureData.frame.x = _getNumber(rawTexture, FRAME_X, 0.0f) * scale;
                            textureData.frame.y = _getNumber(rawTexture, FRAME_Y, 0.0f) * scale;
                            textureData.frame.width = frameWidth * scale;
                            textureData.frame.height = frameHeight * scale;
                        }

                        textureAtlasData.AddTexture(textureData);
                    }
                }
            }
            else
            {
                DragonBones.Assert(false, "No data.");
            }
        }
    }
}