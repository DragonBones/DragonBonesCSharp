using System;
using System.Collections;
using System.Collections.Generic;

namespace dragonBones
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
                    switch ((string)value)
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
                    return (bool)value;
                }
            }

            return defaultValue;
        }

        /**
         * @private
         */
        protected static T _getNumber<T>(Dictionary<string, object> rawData, string key, T defaultValue)
        {
            if (rawData.ContainsKey(key))
            {
                var value = rawData[key];
                if (value == null)
                {
                    return defaultValue;
                }

                return (T)value;
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
                return (string)rawData[key];
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
                return (T)rawData[index];
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
        protected ArmatureData _parseArmature(Dictionary<string, object> rawData)
        {
            var armature = BaseObject.borrowObject<ArmatureData>();
            armature.name = _getString(rawData, NAME, null);
            armature.frameRate = _getNumber(rawData, FRAME_RATE, this._data.frameRate);
            if (armature.frameRate == 0)
            {
                armature.frameRate = 24;
            }

            if (rawData.ContainsKey(TYPE) && rawData[TYPE] is string)
            {
                armature.type = _getArmatureType((string)rawData[TYPE]);
            }
            else
            {
                armature.type = _getNumber(rawData, TYPE, ArmatureType.Armature);
            }

            this._armature = armature;
            this._rawBones.Clear();

            if (rawData.ContainsKey(AABB))
            {
                var aabbObject = (Dictionary<string, object>)rawData[AABB];
                armature.aabb.x = _getNumber(aabbObject, X, 0.0f);
                armature.aabb.y = _getNumber(aabbObject, Y, 0.0f);
                armature.aabb.width = _getNumber(aabbObject, WIDTH, 0.0f);
                armature.aabb.height = _getNumber(aabbObject, HEIGHT, 0.0f);
            }

            if (rawData.ContainsKey(BONE))
            {
                var bones = (List<Dictionary<string, object>>)rawData[BONE];
                foreach (var boneObject in bones)
                {
                    var bone = _parseBone(boneObject);
                    armature.addBone(bone, _getString(boneObject, PARENT, null));
                    this._rawBones.Add(bone);
                }
            }

            if (rawData.ContainsKey(IK))
            {
                var iks = (List<Dictionary<string, object>>)rawData[IK];
                foreach (var ikObject in iks)
                {
                    _parseIK(ikObject);
                }
            }

            if (rawData.ContainsKey(SLOT))
            {
                var slots = (List<Dictionary<string, object>>)rawData[SLOT];
                var zOrder = 0;
                foreach (var slotObject in slots)
                {
                    armature.addSlot(_parseSlot(slotObject, zOrder++));
                }
            }

            if (rawData.ContainsKey(SKIN))
            {
                var skins = (List<Dictionary<string, object>>)rawData[SKIN];
                foreach (var skin in skins)
                {
                    armature.addSkin(_parseSkin(skin));
                }
            }

            if (rawData.ContainsKey(ANIMATION))
            {
                var animations = (List<Dictionary<string, object>>)rawData[ANIMATION];
                foreach (var animation in animations)
                {
                    armature.addAnimation(_parseAnimation(animation));
                }
            }

            if (
                rawData.ContainsKey(ACTIONS) ||
                rawData.ContainsKey(DEFAULT_ACTIONS)
            )
            {
                _parseActionData(rawData, armature.actions, null, null);
            }

            if (this._isOldData && this._isGlobalTransform) // Support 2.x ~ 3.x data.
            {
                this._globalToLocal(armature);
            }

            this._armature = null;
            this._rawBones.Clear();

            return armature;
        }

        /**
         * @private
         */
        protected BoneData _parseBone(Dictionary<string, object> rawData)
        {
            var bone = BaseObject.borrowObject<BoneData>();
            bone.name = _getString(rawData, NAME, null);
            bone.inheritTranslation = _getBoolean(rawData, INHERIT_TRANSLATION, true);
            bone.inheritRotation = _getBoolean(rawData, INHERIT_ROTATION, true);
            bone.inheritScale = _getBoolean(rawData, INHERIT_SCALE, true);
            bone.length = _getNumber(rawData, LENGTH, 0.0f) * this._armatureScale;

            if (rawData.ContainsKey(TRANSFORM))
            {
                _parseTransform((Dictionary<string, object>)rawData[TRANSFORM], bone.transform);
            }

            if (this._isOldData) // Support 2.x ~ 3.x data.
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
            var bone = this._armature.getBone(_getString(rawData, rawData.ContainsKey(BONE) ? BONE : NAME, null));
            if (bone != null)
            {
                bone.ik = this._armature.getBone(_getString(rawData, TARGET, null));
                bone.bendPositive = _getBoolean(rawData, BEND_POSITIVE, true);
                bone.chain = _getNumber(rawData, CHAIN, (uint)0);
                bone.weight = _getNumber(rawData, WEIGHT, 1.0f);

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
            var slot = BaseObject.borrowObject<SlotData>();
            slot.name = _getString(rawData, NAME, null);
            slot.parent = this._armature.getBone(_getString(rawData, PARENT, null));
            slot.displayIndex = _getNumber(rawData, DISPLAY_INDEX, (int)0);
            slot.zOrder = _getNumber(rawData, Z_ORDER, zOrder); // TODO zOrder.

            if (
                rawData.ContainsKey(COLOR) ||
                rawData.ContainsKey(COLOR_TRANSFORM)
            )
            {
                slot.color = SlotData.generateColor();
                _parseColorTransform((Dictionary<string, object>)(rawData.ContainsKey(COLOR) ? rawData[COLOR] : rawData[COLOR_TRANSFORM]), slot.color);
            }
            else
            {
                slot.color = SlotData.DEFAULT_COLOR;
            }

            if (rawData.ContainsKey(BLEND_MODE) && rawData[BLEND_MODE] is string)
            {
                slot.blendMode = _getBlendMode((string)rawData[BLEND_MODE]);
            }
            else
            {
                slot.blendMode = _getNumber(rawData, BLEND_MODE, BlendMode.Normal);
            }

            if (
                rawData.ContainsKey(ACTIONS) ||
                rawData.ContainsKey(DEFAULT_ACTIONS)
            )
            {
                _parseActionData(rawData, slot.actions, null, null);
            }

            if (this._isOldData) // Support 2.x ~ 3.x data.
            {
                if (rawData.ContainsKey(COLOR_TRANSFORM))
                {
                    slot.color = SlotData.generateColor();
                    _parseColorTransform((Dictionary<string, object>)rawData[COLOR_TRANSFORM], slot.color);
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
            var skin = BaseObject.borrowObject<SkinData>();
            skin.name = _getString(rawData, NAME, "__default");
            if (skin.name == "")
            {
                skin.name = "__default";
            }

            if (rawData.ContainsKey(SLOT))
            {
                this._skin = skin;

                var slots = (List<Dictionary<string, object>>)rawData[SLOT];
                int zOrder = 0;
                foreach (var slot in slots)
                {
                    if (this._isOldData) // Support 2.x ~ 3.x data.
                    {
                        this._armature.addSlot(_parseSlot(slot, zOrder++));
                    }

                    skin.addSlot(this._parseSlotDisplaySet(slot));
                }

                this._skin = null;
            }

            return skin;
        }

        /**
         * @private
         */
        protected SlotDisplayDataSet _parseSlotDisplaySet(Dictionary<string, object> rawData)
        {
            var slotDisplayDataSet = BaseObject.borrowObject<SlotDisplayDataSet>();
            slotDisplayDataSet.slot = this._armature.getSlot(_getString(rawData, NAME, null));

            if (rawData.ContainsKey(DISPLAY))
            {
                var displayObjectSet = (List<Dictionary<string, object>>)rawData[DISPLAY];
                var displayDataSet = slotDisplayDataSet.displays;

                this._slotDisplayDataSet = slotDisplayDataSet;

                foreach (var displayObject in displayObjectSet)
                {
                    displayDataSet.Add(_parseDisplay(displayObject));
                }

                this._slotDisplayDataSet = null;
            }

            return slotDisplayDataSet;
        }

        /**
         * @private
         */
        protected DisplayData _parseDisplay(Dictionary<string, object> rawData)
        {
            var display = BaseObject.borrowObject<DisplayData>();
            display.name = _getString(rawData, NAME, null);

            if (rawData.ContainsKey(TYPE) && rawData[TYPE] is string)
            {
                display.type = _getDisplayType((string)rawData[TYPE]);
            }
            else
            {
                display.type = _getNumber(rawData, TYPE, DisplayType.Image);
            }

            display.isRelativePivot = true;

            if (rawData.ContainsKey(PIVOT))
            {
                var pivotObject = (Dictionary<string, object>)rawData[PIVOT];
                display.pivot.x = _getNumber(pivotObject, X, 0.0f);
                display.pivot.y = _getNumber(pivotObject, Y, 0.0f);
            }
            else if (this._isOldData) // Support 2.x ~ 3.x data.
            {
                var transformObject = (Dictionary<string, object>)rawData[TRANSFORM];
                display.isRelativePivot = false;
                display.pivot.x = _getNumber(transformObject, PIVOT_X, 0.0f) * this._armatureScale;
                display.pivot.y = _getNumber(transformObject, PIVOT_Y, 0.0f) * this._armatureScale;
            }
            else
            {
                display.pivot.x = 0.5f;
                display.pivot.y = 0.5f;
            }

            if (rawData.ContainsKey(TRANSFORM))
            {
                _parseTransform((Dictionary<string, object>)rawData[TRANSFORM], display.transform);
            }

            switch (display.type)
            {
                case DisplayType.Image:
                    break;

                case DisplayType.Armature:
                    break;

                case DisplayType.Mesh:
                    display.mesh = _parseMesh(rawData);
                    break;
            }

            return display;
        }

        /**
         * @private
         */
        protected MeshData _parseMesh(Dictionary<string, object> rawData)
        {
            var mesh = BaseObject.borrowObject<MeshData>();

            var rawVertices = (List<float>)rawData[VERTICES];
            var rawUVs = (List<float>)rawData[UVS];
            var rawTriangles = (List<int>)rawData[TRIANGLES];

            var numVertices = (int)(rawVertices.Count / 2); // uint
            var numTriangles = (int)(rawTriangles.Count / 3); // uint

            var inverseBindPose = new List<Matrix>(this._armature.getSortedBones().Count);

            mesh.skinned = (rawData.ContainsKey(WEIGHTS)) && ((List<float>)rawData[WEIGHTS]).Count > 0;
            mesh.uvs.Capacity = numVertices * 2;
            mesh.vertices.Capacity = numVertices * 2;
            mesh.vertexIndices.Capacity = numTriangles * 3;

            if (mesh.skinned)
            {
                mesh.boneIndices.Capacity = numVertices;
                mesh.weights.Capacity = numVertices;
                mesh.boneVertices.Capacity = numVertices;

                if (rawData.ContainsKey(SLOT_POSE))
                {
                    var rawSlotPose = (List<float>)rawData[SLOT_POSE];
                    mesh.slotPose.a = rawSlotPose[0];
                    mesh.slotPose.b = rawSlotPose[1];
                    mesh.slotPose.c = rawSlotPose[2];
                    mesh.slotPose.d = rawSlotPose[3];
                    mesh.slotPose.tx = rawSlotPose[4];
                    mesh.slotPose.ty = rawSlotPose[5];
                }

                if (rawData.ContainsKey(BONE_POSE))
                {
                    var rawBonePose = (List<float>)rawData[BONE_POSE];
                    for (int i = 0, l = rawBonePose.Count; i < l; i += 7)
                    {
                        var rawBoneIndex = (int)rawBonePose[i]; // uint
                        var boneMatrix = inverseBindPose[rawBoneIndex] = new Matrix();
                        boneMatrix.a = rawBonePose[i + 1];
                        boneMatrix.b = rawBonePose[i + 2];
                        boneMatrix.c = rawBonePose[i + 3];
                        boneMatrix.d = rawBonePose[i + 4];
                        boneMatrix.tx = rawBonePose[i + 5];
                        boneMatrix.ty = rawBonePose[i + 6];
                        boneMatrix.invert();
                    }
                }
            }

            for (int i = 0, iW = 0, l = rawVertices.Count; i < l; i += 2)
            {
                var iN = i + 1;
                var vertexIndex = i / 2;

                var x = mesh.vertices[i] = rawVertices[i] * this._armatureScale;
                var y = mesh.vertices[iN] = rawVertices[iN] * this._armatureScale;
                mesh.uvs[i] = rawUVs[i];
                mesh.uvs[iN] = rawUVs[iN];

                if (mesh.skinned) // If mesh is skinned, transform point by bone bind pose.
                {
                    var rawWeights = (List<float>)rawData[WEIGHTS];
                    var numBones = (int)rawWeights[iW]; // uint
                    var indices = mesh.boneIndices[vertexIndex] = new List<int>(numBones);
                    var weights = mesh.weights[vertexIndex] = new List<float>(numBones);
                    var boneVertices = mesh.boneVertices[vertexIndex] = new List<float>(numBones * 2);

                    mesh.slotPose.transformPoint(x, y, this._helpPoint);
                    x = mesh.vertices[i] = this._helpPoint.x;
                    y = mesh.vertices[iN] = this._helpPoint.y;

                    for (int iB = 0; iB < numBones; ++iB)
                    {
                        var iI = iW + 1 + iB * 2;
                        var rawBoneIndex = (int)rawWeights[iI]; // uint
                        var boneData = this._rawBones[rawBoneIndex];

                        var boneIndex = mesh.bones.IndexOf(boneData);
                        if (boneIndex < 0)
                        {
                            boneIndex = mesh.bones.Count;
                            mesh.bones[boneIndex] = boneData;
                            mesh.inverseBindPose[boneIndex] = inverseBindPose[rawBoneIndex];
                        }

                        mesh.inverseBindPose[boneIndex].transformPoint(x, y, this._helpPoint);

                        indices[iB] = boneIndex;
                        weights[iB] = rawWeights[iI + 1];
                        boneVertices[iB * 2] = this._helpPoint.x;
                        boneVertices[iB * 2 + 1] = this._helpPoint.y;
                    }

                    iW += numBones * 2 + 1;
                }
            }

            for (int i = 0, l = rawTriangles.Count; i < l; ++i)
            {
                mesh.vertexIndices[i] = rawTriangles[i];
            }

            return mesh;
        }

        /**
         * @private
         */
        protected AnimationData _parseAnimation(Dictionary<string, object> rawData)
        {
            var animation = BaseObject.borrowObject<AnimationData>();
            animation.name = _getString(rawData, NAME, "__default");
            if (animation.name == "")
            {
                animation.name = "__default";
            }

            animation.frameCount = Math.Max(_getNumber(rawData, DURATION, (uint)1), 1);
            animation.position = _getNumber(rawData, POSITION, 0.0f) / this._armature.frameRate;
            animation.duration = (float)animation.frameCount / this._armature.frameRate;
            animation.playTimes = _getNumber(rawData, PLAY_TIMES, (uint)1);
            animation.fadeInTime = _getNumber(rawData, FADE_IN_TIME, 0.0f);

            this._animation = animation;

            var animationName = _getString(rawData, ANIMATION, null);
            if (animationName == null)
            {
                animation.animation = this._armature.getAnimation(animationName);
                if (animation.animation != null)
                {
                    // TODO animation clip.
                }

                return animation;
            }

            _parseTimeline(rawData, animation, _parseAnimationFrame);

            if (rawData.ContainsKey(BONE))
            {
                var boneTimelines = (List<Dictionary<string, object>>)rawData[BONE];
                foreach (var boneTimelineObject in boneTimelines)
                {
                    animation.addBoneTimeline(_parseBoneTimeline(boneTimelineObject));
                }
            }

            if (rawData.ContainsKey(SLOT))
            {
                var slotTimelines = (List<Dictionary<string, object>>)rawData[SLOT];
                foreach (var slotTimelineObject in slotTimelines)
                {
                    animation.addSlotTimeline(_parseSlotTimeline(slotTimelineObject));
                }
            }

            if (rawData.ContainsKey(FFD))
            {
                var ffdTimelines = (List<Dictionary<string, object>>)rawData[FFD];
                foreach (var ffdTimelineObject in ffdTimelines)
                {
                    animation.addFFDTimeline(_parseFFDTimeline(ffdTimelineObject));
                }
            }

            if (this._isOldData) // Support 2.x ~ 3.x data.
            {
                this._isAutoTween = _getBoolean(rawData, AUTO_TWEEN, true);
                this._animationTweenEasing = _getNumber(rawData, TWEEN_EASING, 0.0f);
                animation.playTimes = _getNumber(rawData, LOOP, (uint)1);

                if (rawData.ContainsKey(TIMELINE))
                {
                    var timelines = (List<Dictionary<string, object>>)rawData[TIMELINE];
                    foreach (var boneTimelineObject in timelines)
                    {
                        animation.addBoneTimeline(_parseBoneTimeline(boneTimelineObject));
                    }

                    foreach (var slotTimelineObject in timelines)
                    {
                        animation.addSlotTimeline(_parseSlotTimeline(slotTimelineObject));
                    }
                }
            }
            else
            {
                this._isAutoTween = false;
                this._animationTweenEasing = 0.0f;
            }

            foreach (var pair in this._armature.bones)
            {
                var bone = pair.Value;
                if (animation.getBoneTimeline(bone.name) == null) // Add default bone timeline for cache if do not have one.
                {
                    var boneTimeline = BaseObject.borrowObject<BoneTimelineData>();
                    var boneFrame = BaseObject.borrowObject<BoneFrameData>();
                    boneTimeline.bone = bone;
                    boneTimeline.frames[0] = boneFrame;
                    animation.addBoneTimeline(boneTimeline);
                }
            }

            foreach (var pair in this._armature.slots)
            {
                var slot = pair.Value;
                if (animation.getSlotTimeline(slot.name) == null) // Add default slot timeline for cache if do not have one.
                {
                    var slotTimeline = BaseObject.borrowObject<SlotTimelineData>();
                    var slotFrame = BaseObject.borrowObject<SlotFrameData>();
                    slotTimeline.slot = slot;
                    slotFrame.displayIndex = slot.displayIndex;
                    //slotFrame.zOrder = -2; // TODO zOrder.

                    if (slot.color == SlotData.DEFAULT_COLOR)
                    {
                        slotFrame.color = SlotFrameData.DEFAULT_COLOR;
                    }
                    else
                    {
                        slotFrame.color = SlotFrameData.generateColor();
                        slotFrame.color.copyFrom(slot.color);
                    }

                    slotTimeline.frames[0] = slotFrame;
                    animation.addSlotTimeline(slotTimeline);

                    if (this._isOldData) // Support 2.x ~ 3.x data.
                    {
                        slotFrame.displayIndex = -1;
                    }
                }
            }

            this._animation = null;

            return animation;
        }

        /**
         * @private
         */
        protected BoneTimelineData _parseBoneTimeline(Dictionary<string, object> rawData)
        {
            var timeline = BaseObject.borrowObject<BoneTimelineData>();
            timeline.bone = this._armature.getBone(_getString(rawData, NAME, null));

            _parseTimeline(rawData, timeline, _parseBoneFrame);

            var originTransform = timeline.originTransform;
            BoneFrameData prevFrame = null;

            foreach (var frame in timeline.frames) // bone transform pose = origin + animation origin + animation.
            {
                if (prevFrame == null)
                {
                    originTransform.copyFrom(frame.transform);
                    frame.transform.identity();

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
                    frame.transform.minus(originTransform);
                }

                prevFrame = frame;
            }

            if (timeline.scale != 1.0f || timeline.offset != 0.0f)
            {
                this._animation.hasAsynchronyTimeline = true;
            }

            if (this._isOldData && (rawData.ContainsKey(PIVOT_X) || rawData.ContainsKey(PIVOT_Y))) // Support 2.x ~ 3.x data.
            {
                this._timelinePivot.x = _getNumber(rawData, PIVOT_X, 0.0f);
                this._timelinePivot.y = _getNumber(rawData, PIVOT_Y, 0.0f);
            }
            else
            {
                this._timelinePivot.clear();
            }

            return timeline;
        }

        /**
         * @private
         */
        protected SlotTimelineData _parseSlotTimeline(Dictionary<string, object> rawData)
        {
            var timeline = BaseObject.borrowObject<SlotTimelineData>();
            timeline.slot = this._armature.getSlot(_getString(rawData, NAME, null));

            _parseTimeline(rawData, timeline, _parseSlotFrame);

            if (timeline.scale != 1.0f || timeline.offset != 0.0f)
            {
                this._animation.hasAsynchronyTimeline = true;
            }

            return timeline;
        }

        /**
         * @private
         */
        protected FFDTimelineData _parseFFDTimeline(Dictionary<string, object> rawData)
        {
            var timeline = BaseObject.borrowObject<FFDTimelineData>();
            timeline.skin = this._armature.getSkin(_getString(rawData, SKIN, null));
            timeline.slot = timeline.skin.getSlot(_getString(rawData, SLOT, null)); // NAME;

            var meshName = _getString(rawData, NAME, null);
            for (int i = 0, l = timeline.slot.displays.Count; i < l; ++i)
            {
                var displayData = timeline.slot.displays[i];
                if (displayData.mesh != null && displayData.name == meshName)
                {
                    timeline.displayIndex = i; // rawData[DISPLAY_INDEX];
                    this._mesh = displayData.mesh; // Find the ffd's mesh.
                    break;
                }
            }

            _parseTimeline(rawData, timeline, _parseFFDFrame);

            this._mesh = null;

            return timeline;
        }

        /**
         * @private
         */
        protected AnimationFrameData _parseAnimationFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.borrowObject<AnimationFrameData>();

            _parseFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(ACTION) || rawData.ContainsKey(ACTIONS))
            {
                _parseActionData(rawData, frame.actions, null, null);
            }

            if (rawData.ContainsKey(EVENT) || rawData.ContainsKey(SOUND))
            {
                this._parseEventData(rawData, frame.events, null, null);
            }

            return frame;
        }

        /**
         * @private
         */
        protected BoneFrameData _parseBoneFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.borrowObject<BoneFrameData>();
            frame.tweenRotate = _getNumber(rawData, TWEEN_ROTATE, 0.0f);
            frame.tweenScale = _getBoolean(rawData, TWEEN_SCALE, true);

            _parseTweenFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(TRANSFORM))
            {
                var transformObject = (Dictionary<string, object>)rawData[TRANSFORM];
                _parseTransform(transformObject, frame.transform);

                if (this._isOldData) // Support 2.x ~ 3.x data.
                {
                    this._helpPoint.x = this._timelinePivot.x + _getNumber(transformObject, PIVOT_X, 0.0f);
                    this._helpPoint.y = this._timelinePivot.y + _getNumber(transformObject, PIVOT_Y, 0.0f);
                    frame.transform.toMatrix(this._helpMatrix);
                    this._helpMatrix.transformPoint(this._helpPoint.x, this._helpPoint.y, this._helpPoint, true);
                    frame.transform.x += this._helpPoint.x;
                    frame.transform.y += this._helpPoint.y;
                }
            }

            var bone = ((BoneTimelineData)this._timeline).bone;
            var actions = new List<ActionData>();
            var events = new List<EventData>();

            if (rawData.ContainsKey(ACTION) || rawData.ContainsKey(ACTIONS))
            {
                var slot = this._armature.getSlot(bone.name);
                _parseActionData(rawData, actions, bone, slot);
            }

            if (rawData.ContainsKey(EVENT) || rawData.ContainsKey(SOUND))
            {
                _parseEventData(rawData, events, bone, null);
            }

            if (actions.Count > 0 || events.Count > 0)
            {
                this._mergeFrameToAnimationTimeline(frame.position, actions, events); // Merge actions and events to animation timeline.
            }

            return frame;
        }

        /**
         * @private
         */
        protected SlotFrameData _parseSlotFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.borrowObject<SlotFrameData>();
            frame.displayIndex = _getNumber(rawData, DISPLAY_INDEX, 0);
            //frame.zOrder = _getNumber(rawData, Z_ORDER, -2); // TODO zorder

            _parseTweenFrame(rawData, frame, frameStart, frameCount);

            if (rawData.ContainsKey(COLOR) || rawData.ContainsKey(COLOR_TRANSFORM)) // Support 2.x ~ 3.x data. (colorTransform key)
            {
                frame.color = SlotFrameData.generateColor();
                _parseColorTransform((Dictionary<string, object>)(rawData.ContainsKey(COLOR) ? rawData[COLOR] : rawData[COLOR_TRANSFORM]), frame.color);
            }
            else
            {
                frame.color = SlotFrameData.DEFAULT_COLOR;
            }

            if (this._isOldData) // Support 2.x ~ 3.x data.
            {
                if (_getBoolean(rawData, HIDE, false))
                {
                    frame.displayIndex = -1;
                }
            }
            else if (rawData.ContainsKey(ACTION) || rawData.ContainsKey(ACTIONS))
            {
                var slot = ((SlotTimelineData)this._timeline).slot;
                var actions = new List<ActionData>();
                _parseActionData(rawData, actions, slot.parent, slot);

                this._mergeFrameToAnimationTimeline(frame.position, actions, null); // Merge actions and events to animation timeline.
            }

            return frame;
        }

        /**
         * @private
         */
        protected ExtensionFrameData _parseFFDFrame(Dictionary<string, object> rawData, uint frameStart, uint frameCount)
        {
            var frame = BaseObject.borrowObject<ExtensionFrameData>();
            frame.type = _getNumber(rawData, TYPE, ExtensionType.FFD);

            _parseTweenFrame(rawData, frame, frameStart, frameCount);

            var rawVertices = (List<float>)rawData[VERTICES];
            var offset = _getNumber(rawData, OFFSET, (int)0); // uint
            var x = 0.0f;
            var y = 0.0f;
            for (int i = 0, l = this._mesh.vertices.Count; i < l; i += 2)
            {
                if (rawVertices == null || i < offset || i - offset >= rawVertices.Count) // Fill 0.
                {
                    x = 0.0f;
                    y = 0.0f;
                }
                else
                {
                    x = rawVertices[i - offset] * this._armatureScale;
                    y = rawVertices[i + 1 - offset] * this._armatureScale;
                }

                if (this._mesh.skinned) // If mesh is skinned, transform point by bone bind pose.
                {
                    this._mesh.slotPose.transformPoint(x, y, this._helpPoint, true);
                    x = this._helpPoint.x;
                    y = this._helpPoint.y;

                    var boneIndices = this._mesh.boneIndices[i / 2];
                    foreach (var boneIndex in boneIndices)
                    {
                        this._mesh.inverseBindPose[boneIndex].transformPoint(x, y, this._helpPoint, true);
                        frame.tweens.Add(this._helpPoint.x);
                        frame.tweens.Add(this._helpPoint.y);
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
                else if (this._isOldData) // Support 2.x ~ 3.x data.
                {
                    frame.tweenEasing = this._isAutoTween ? this._animationTweenEasing : DragonBones.NO_TWEEN;
                }
                else
                {
                    frame.tweenEasing = DragonBones.NO_TWEEN;
                }

                if (this._isOldData && this._animation.scale == 1 && ((TimelineData<T>)this._timeline).scale == 1.0f && frame.duration * this._armature.frameRate < 2)
                {
                    frame.tweenEasing = DragonBones.NO_TWEEN;
                }

                if (rawData.ContainsKey(CURVE))
                {
                    frame.curve = TweenFrameData<T>.samplingCurve((List<float>)rawData[CURVE], frameCount);
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
            frame.position = (float)frameStart / this._armature.frameRate;
            frame.duration = (float)frameCount / this._armature.frameRate;
        }

        /**
         * @private
         */
        protected void _parseTimeline<T>(Dictionary<string, object> rawData, TimelineData<T> timeline, Func<Dictionary<string, object>, uint, uint, T> frameParser) where T : FrameData<T>
        {
            var a = new List<ArmatureData>();
            timeline.scale = _getNumber(rawData, SCALE, 1.0f);
            timeline.offset = _getNumber(rawData, OFFSET, 0.0f);

            this._timeline = timeline;

            if (rawData.ContainsKey(FRAME))
            {
                var rawFrames = (List<Dictionary<string, object>>)rawData[FRAME];
                if (rawFrames.Count > 0)
                {
                    if (rawFrames.Count == 1) // Only one frame.
                    {
                        timeline.frames.Capacity = 1;
                        timeline.frames[0] = frameParser(rawFrames[0], 0, _getNumber(rawFrames[0], DURATION, (uint)1));
                    }
                    else
                    {
                        timeline.frames.Capacity = (int)this._animation.frameCount + 1;

                        uint frameStart = 0;
                        uint frameCount = 0;
                        T frame = null;
                        T prevFrame = null;

                        for (int i = 0, iW = 0, l = timeline.frames.Count; i < l; ++i) // Fill frame link.
                        {
                            if (frameStart + frameCount <= i && iW < rawFrames.Count)
                            {
                                var frameObject = rawFrames[iW++];
                                frameStart = (uint)i;
                                frameCount = _getNumber(frameObject, DURATION, (uint)1);
                                frame = frameParser(frameObject, frameStart, frameCount);

                                if (prevFrame != null)
                                {
                                    prevFrame.next = frame;
                                    frame.prev = prevFrame;

                                    if (this._isOldData) // Support 2.x ~ 3.x data.
                                    {
                                        /*if (prevFrame is TweenFrameData<T> && (int)frameObject[DISPLAY_INDEX] == -1)
                                        {
                                            //(< TweenFrameData < T >>< any > prevFrame).tweenEasing = DragonBones.NO_TWEEN;
                                        }*/
                                    }
                                }

                                prevFrame = frame;
                            }

                            timeline.frames[i] = frame;
                        }

                        frame.duration = this._animation.duration - frame.position; // Modify last frame duration.

                        frame = timeline.frames[0];

                        prevFrame.next = frame;

                        frame.prev = prevFrame;

                        if (this._isOldData) // Support 2.x ~ 3.x data.
                        {
                            /*if (prevFrame is TweenFrameData<T> && (int)rawFrames[0][DISPLAY_INDEX] == -1) {
                                //(< TweenFrameData < T >>< any > prevFrame).tweenEasing = DragonBones.NO_TWEEN;
                            }*/
                        }
                    }
                }
            }

            this._timeline = null;
        }

        /**
         * @private
         */
        protected void _parseActionData(Dictionary<string, object> rawData, List<ActionData> actions, BoneData bone, SlotData slot)
        {
            var actionsObject = rawData.ContainsKey(ACTION) ? rawData[ACTION] : (rawData.ContainsKey(ACTIONS) ? rawData[ACTIONS] : rawData[DEFAULT_ACTIONS]);
            if (actionsObject is string) // Support string action.
            {
                var actionData = BaseObject.borrowObject<ActionData>();
                actionData.type = ActionType.FadeIn;
                actionData.bone = bone;
                actionData.slot = slot;
                actionData.data[0] = actionsObject;
                actionData.data[1] = -1;
                actionData.data[2] = -1;
                actions.Add(actionData);
            }
            else if (actionsObject is IList) // Support [{gotoAndPlay: "animationName"}, ...] or [["gotoAndPlay", "animationName", ...], ...]
            {
                foreach (var actionObject in (List<object>)actionsObject)
                {
                    var isArray = actionObject is IList;
                    var actionData = BaseObject.borrowObject<ActionData>();
                    var animationName = isArray ? _getParameter((List<object>)actionObject, 1, "") : _getString((Dictionary<string, object>)actionObject, "gotoAndPlay", "");

                    if (isArray)
                    {
                        var actionType = ((List<object>)actionObject)[0];
                        if (actionType is string)
                        {
                            actionData.type = _getActionType((string)actionType);
                        }
                        else
                        {
                            actionData.type = _getParameter((List<object>)actionObject, 0, ActionType.FadeIn);
                        }
                    }
                    else
                    {
                        actionData.type = ActionType.GotoAndPlay;
                    }

                    switch (actionData.type)
                    {
                        case ActionType.Play:
                            actionData.data[0] = animationName;
                            actionData.data[1] = isArray ? _getParameter((List<object>)actionObject, 2, -1) : -1; // playTimes
                            break;

                        case ActionType.Stop:
                            actionData.data[0] = animationName;
                            break;

                        case ActionType.GotoAndPlay:
                            actionData.data[0] = animationName;
                            actionData.data[1] = isArray ? _getParameter((List<object>)actionObject, 2, 0.0f) : 0; // time
                            actionData.data[2] = isArray ? _getParameter((List<object>)actionObject, 3, -1) : -1; // playTimes
                            break;

                        case ActionType.GotoAndStop:
                            actionData.data[0] = animationName;
                            actionData.data[1] = isArray ? _getParameter((List<object>)actionObject, 2, 0.0f) : 0; // time
                            break;

                        case ActionType.FadeIn:
                            actionData.data[0] = animationName;
                            actionData.data[1] = isArray ? _getParameter((List<object>)actionObject, 2, -1.0f) : -1; // fadeInTime
                            actionData.data[2] = isArray ? _getParameter((List<object>)actionObject, 3, -1) : -1; // playTimes
                            break;

                        case ActionType.FadeOut:
                            actionData.data[0] = animationName;
                            actionData.data[1] = isArray ? _getParameter((List<object>)actionObject, 2, 0.0f) : 0; // fadeOutTime
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
                var soundEventData = BaseObject.borrowObject<EventData>();
                soundEventData.type = EventType.Sound;
                soundEventData.name = (string)rawData[SOUND];
                soundEventData.bone = bone;
                soundEventData.slot = slot;
                events.Add(soundEventData);
            }

            if (rawData.ContainsKey(EVENT))
            {
                var eventData = BaseObject.borrowObject<EventData>();
                eventData.type = EventType.Frame;
                eventData.name = (string)rawData[ObjectDataParser.EVENT];
                eventData.bone = bone;
                eventData.slot = slot;

                if (rawData.ContainsKey(DATA)) // TODO 
                {
                    //eventData.data = rawData[DATA];
                }

                events.Add(eventData);
            }
        }

        /**
         * @private
         */
        protected void _parseTransform(Dictionary<string, object> rawData, Transform transform)
        {
            transform.x = _getNumber(rawData, X, 0.0f) * this._armatureScale;
            transform.y = _getNumber(rawData, Y, 0.0f) * this._armatureScale;
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
            color.alphaMultiplier = _getNumber(rawData, ALPHA_MULTIPLIER, 100.0f) * 0.01f;
            color.redMultiplier = _getNumber(rawData, RED_MULTIPLIER, 100.0f) * 0.01f;
            color.greenMultiplier = _getNumber(rawData, GREEN_MULTIPLIER, 100.0f) * 0.01f;
            color.blueMultiplier = _getNumber(rawData, BLUE_MULTIPLIER, 100.0f) * 0.01f;
            color.alphaOffset = _getNumber(rawData, ALPHA_OFFSET, (int)0);
            color.redOffset = _getNumber(rawData, RED_OFFSET, (int)0);
            color.greenOffset = _getNumber(rawData, GREEN_OFFSET, (int)0);
            color.blueOffset = _getNumber(rawData, BLUE_OFFSET, (int)0);
        }

        /**
         * @inheritDoc
         */
        override public DragonBonesData parseDragonBonesData(Dictionary<string, object> rawData, float scale = 1.0f)
        {
            if (rawData != null)
            {
                var version = _getString(rawData, VERSION, null);
                this._isOldData = version == DATA_VERSION_2_3 || version == DATA_VERSION_3_0;
                if (this._isOldData)
                {
                    this._isGlobalTransform = _getBoolean(rawData, IS_GLOBAL, true);
                }
                else
                {
                    this._isGlobalTransform = false;
                }

                this._armatureScale = scale;

                if (
                    version == DATA_VERSION ||
                    version == DATA_VERSION_4_0 ||
                    this._isOldData
                )
                {
                    var data = BaseObject.borrowObject<DragonBonesData>();
                    data.name = _getString(rawData, NAME, null);
                    data.frameRate = _getNumber(rawData, FRAME_RATE, (uint)24);
                    if (data.frameRate == 0)
                    {
                        data.frameRate = 24;
                    }

                    if (rawData.ContainsKey(ARMATURE))
                    {
                        this._data = data;

                        var armatures = (List<Dictionary<string, object>>)rawData[ARMATURE];
                        foreach (var armatureObject in armatures)
                        {
                            data.addArmature(_parseArmature(armatureObject));
                        }

                        this._data = null;
                    }

                    return data;
                }
                else
                {
                    DragonBones.warn("Nonsupport data version.");
                }
            }
            else
            {
                DragonBones.warn("No data.");
            }

            return null;
        }

        /**
         * @inheritDoc
         */
        public override void parseTextureAtlasData(Dictionary<string, object> rawData, TextureAtlasData textureAtlasData, float scale = 0.0f)
        {
            if (rawData != null)
            {
                textureAtlasData.name = _getString(rawData, NAME, null);
                textureAtlasData.imagePath = _getString(rawData, IMAGE_PATH, null);
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
                    var textures = (List<Dictionary<string, object>>)rawData[SUB_TEXTURE];
                    foreach (var textureObject in textures)
                    {
                        var textureData = textureAtlasData.generateTextureData();
                        textureData.name = _getString(textureObject, NAME, null);
                        textureData.rotated = _getBoolean(textureObject, ROTATED, false);
                        textureData.region.x = _getNumber(textureObject, X, 0.0f) * scale;
                        textureData.region.y = _getNumber(textureObject, Y, 0.0f) * scale;
                        textureData.region.width = _getNumber(textureObject, WIDTH, 0.0f) * scale;
                        textureData.region.height = _getNumber(textureObject, HEIGHT, 0.0f) * scale;

                        var frameWidth = _getNumber(textureObject, FRAME_WIDTH, -1.0f);
                        var frameHeight = _getNumber(textureObject, FRAME_HEIGHT, -1.0f);
                        if (frameWidth > 0.0f && frameHeight > 0.0f)
                        {
                            textureData.frame = TextureData.generateRectangle();
                            textureData.frame.x = _getNumber(textureObject, FRAME_X, 0.0f) * scale;
                            textureData.frame.y = _getNumber(textureObject, FRAME_Y, 0.0f) * scale;
                            textureData.frame.width = frameWidth * scale;
                            textureData.frame.height = frameHeight * scale;
                        }

                        textureAtlasData.addTexture(textureData);
                    }
                }
            }
            else
            {
                DragonBones.warn("No data.");
            }
        }
    }
}