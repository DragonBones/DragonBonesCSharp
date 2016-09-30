using System;
using System.Collections.Generic;

namespace dragonBones
{
    /**
     * @private
     */
    public abstract class DataParser
    {
        protected const string DATA_VERSION_2_3 = "2.3";
        protected const string DATA_VERSION_3_0 = "3.0";
        protected const string DATA_VERSION_4_0 = "4.0";
        protected const string DATA_VERSION = "4.5";

        protected const string TEXTURE_ATLAS = "TextureAtlas";
        protected const string SUB_TEXTURE = "SubTexture";
        protected const string FORMAT = "format";
        protected const string IMAGE_PATH = "imagePath";
        protected const string WIDTH = "width";
        protected const string HEIGHT = "height";
        protected const string ROTATED = "rotated";
        protected const string FRAME_X = "frameX";
        protected const string FRAME_Y = "frameY";
        protected const string FRAME_WIDTH = "frameWidth";
        protected const string FRAME_HEIGHT = "frameHeight";

        protected const string DRADON_BONES = "dragonBones";
        protected const string ARMATURE = "armature";
        protected const string BONE = "bone";
        protected const string IK = "ik";
        protected const string SLOT = "slot";
        protected const string SKIN = "skin";
        protected const string DISPLAY = "display";
        protected const string ANIMATION = "animation";
        protected const string FFD = "ffd";
        protected const string FRAME = "frame";

        protected const string PIVOT = "pivot";
        protected const string TRANSFORM = "transform";
        protected const string AABB = "aabb";
        protected const string COLOR = "color";
        protected const string FILTER = "filter";

        protected const string VERSION = "version";
        protected const string IS_GLOBAL = "isGlobal";
        protected const string FRAME_RATE = "frameRate";
        protected const string TYPE = "type";
        protected const string NAME = "name";
        protected const string PARENT = "parent";
        protected const string LENGTH = "length";
        protected const string DATA = "data";
        protected const string DISPLAY_INDEX = "displayIndex";
        protected const string Z_ORDER = "z";
        protected const string BLEND_MODE = "blendMode";
        protected const string INHERIT_TRANSLATION = "inheritTranslation";
        protected const string INHERIT_ROTATION = "inheritRotation";
        protected const string INHERIT_SCALE = "inheritScale";
        protected const string TARGET = "target";
        protected const string BEND_POSITIVE = "bendPositive";
        protected const string CHAIN = "chain";
        protected const string WEIGHT = "weight";

        protected const string FADE_IN_TIME = "fadeInTime";
        protected const string PLAY_TIMES = "playTimes";
        protected const string SCALE = "scale";
        protected const string OFFSET = "offset";
        protected const string POSITION = "position";
        protected const string DURATION = "duration";
        protected const string TWEEN_EASING = "tweenEasing";
        protected const string TWEEN_ROTATE = "tweenRotate";
        protected const string TWEEN_SCALE = "tweenScale";
        protected const string CURVE = "curve";
        protected const string EVENT = "event";
        protected const string SOUND = "sound";
        protected const string ACTION = "action";
        protected const string ACTIONS = "actions";
        protected const string DEFAULT_ACTIONS = "defaultActions";

        protected const string X = "x";
        protected const string Y = "y";
        protected const string SKEW_X = "skX";
        protected const string SKEW_Y = "skY";
        protected const string SCALE_X = "scX";
        protected const string SCALE_Y = "scY";

        protected const string ALPHA_OFFSET = "aO";
        protected const string RED_OFFSET = "rO";
        protected const string GREEN_OFFSET = "gO";
        protected const string BLUE_OFFSET = "bO";
        protected const string ALPHA_MULTIPLIER = "aM";
        protected const string RED_MULTIPLIER = "rM";
        protected const string GREEN_MULTIPLIER = "gM";
        protected const string BLUE_MULTIPLIER = "bM";

        protected const string UVS = "uvs";
        protected const string VERTICES = "vertices";
        protected const string TRIANGLES = "triangles";
        protected const string WEIGHTS = "weights";
        protected const string SLOT_POSE = "slotPose";
        protected const string BONE_POSE = "bonePose";

        protected const string TWEEN = "tween";
        protected const string KEY = "key";

        protected const string COLOR_TRANSFORM = "colorTransform";
        protected const string TIMELINE = "timeline";
        protected const string PIVOT_X = "pX";
        protected const string PIVOT_Y = "pY";
        protected const string LOOP = "loop";
        protected const string AUTO_TWEEN = "autoTween";
        protected const string HIDE = "hide";

        protected const string RECTANGLE = "rectangle";
        protected const string ELLIPSE = "ellipse";

        protected static ArmatureType _getArmatureType(string value)
        {
            switch (value.ToLower())
            {
                case "stage":
                    return ArmatureType.Armature;

                case "armature":
                    return ArmatureType.Armature;

                case "movieclip":
                    return ArmatureType.MovieClip;

                default:
                    return ArmatureType.Armature;
            }
        }

        protected static DisplayType _getDisplayType(string value)
        {
            switch (value.ToLower())
            {
                case "image":
                    return DisplayType.Image;

                case "armature":
                    return DisplayType.Armature;

                case "mesh":
                    return DisplayType.Mesh;

                default:
                    return DisplayType.Image;
            }
        }

        protected static BlendMode _getBlendMode(string value)
        {
            switch (value.ToLower())
            {
                case "normal":
                    return BlendMode.Normal;

                case "add":
                    return BlendMode.Add;

                case "alpha":
                    return BlendMode.Alpha;

                case "darken":
                    return BlendMode.Darken;

                case "difference":
                    return BlendMode.Difference;

                case "erase":
                    return BlendMode.Erase;

                case "hardlight":
                    return BlendMode.HardLight;

                case "invert":
                    return BlendMode.Invert;

                case "layer":
                    return BlendMode.Layer;

                case "lighten":
                    return BlendMode.Lighten;

                case "multiply":
                    return BlendMode.Multiply;

                case "overlay":
                    return BlendMode.Overlay;

                case "screen":
                    return BlendMode.Screen;

                case "subtract":
                    return BlendMode.Subtract;

                default:
                    return BlendMode.Normal;
            }
        }

        protected static ActionType _getActionType(string value)
        {
            switch (value.ToLower())
            {
                case "play":
                    return ActionType.Play;

                case "stop":
                    return ActionType.Stop;

                case "gotoandplay":
                    return ActionType.GotoAndPlay;

                case "gotoandstop":
                    return ActionType.GotoAndStop;

                case "fadein":
                    return ActionType.FadeIn;

                case "fadeout":
                    return ActionType.FadeOut;

                default:
                    return ActionType.FadeIn;
            }
        }

        protected DragonBonesData _data = null;
        protected ArmatureData _armature = null;
        protected SkinData _skin = null;
        protected SlotDisplayDataSet _slotDisplayDataSet = null;
        protected MeshData _mesh = null;
        protected AnimationData _animation = null;
        protected object _timeline = null;

        protected bool _isOldData = false; // For 2.x ~ 3.x
        protected bool _isGlobalTransform = false; // For 2.x ~ 3.x
        protected bool _isAutoTween = false; // For 2.x ~ 3.x
        protected float _animationTweenEasing = 0.0f; // For 2.x ~ 3.x
        protected readonly Point _timelinePivot = new Point(); // For 2.x ~ 3.x
        
        protected readonly Point _helpPoint = new Point();
        protected readonly Transform _helpTransformA = new Transform();
        protected readonly Transform _helpTransformB = new Transform();
        protected readonly Matrix _helpMatrix = new Matrix();
        protected readonly List<BoneData> _rawBones = new List<BoneData>(); // For skinned mesh

        public DataParser() { }

        /**
         * @private
         */
        public abstract DragonBonesData parseDragonBonesData(Dictionary<string, object> rawData, float scale = 1.0f);

        /**
         * @private
         */
        public abstract void parseTextureAtlasData(Dictionary<string, object> rawData, TextureAtlasData textureAtlasData, float scale = 0.0f);

        private void _getTimelineFrameMatrix(AnimationData animation, BoneTimelineData timeline, float position, Transform transform) // Support 2.x ~ 3.x data.
        {
            var frameIndex = (int)Math.Floor(position * animation.frameCount / animation.duration);
            if (timeline.frames.Count == 1 || frameIndex >= timeline.frames.Count)
            {
                transform.copyFrom(timeline.frames[0].transform);
            }
            else
            {
                var frame = timeline.frames[frameIndex];
                float tweenProgress = 0.0f;

                if (frame.tweenEasing != DragonBones.NO_TWEEN)
                {
                    tweenProgress = (position - frame.position) / frame.duration;
                    if (frame.tweenEasing != 0.0f)
                    {
                        tweenProgress = TweenTimelineState<BoneFrameData, BoneTimelineData>._getEasingValue(tweenProgress, frame.tweenEasing);
                    }
                }
                else if (frame.curve != null)
                {
                    tweenProgress = (position - frame.position) / frame.duration;
                    tweenProgress = TweenTimelineState<BoneFrameData, BoneTimelineData>._getCurveEasingValue(tweenProgress, frame.curve);
                }

                var nextFrame = frame.next;

                transform.x = nextFrame.transform.x - frame.transform.x;
                transform.y = nextFrame.transform.y - frame.transform.y;
                transform.skewX = Transform.normalizeRadian(nextFrame.transform.skewX - frame.transform.skewX);
                transform.skewY = Transform.normalizeRadian(nextFrame.transform.skewY - frame.transform.skewY);
                transform.scaleX = nextFrame.transform.scaleX - frame.transform.scaleX;
                transform.scaleY = nextFrame.transform.scaleY - frame.transform.scaleY;

                transform.x = frame.transform.x + transform.x * tweenProgress;
                transform.y = frame.transform.y + transform.y * tweenProgress;
                transform.skewX = frame.transform.skewX + transform.skewX * tweenProgress;
                transform.skewY = frame.transform.skewY + transform.skewY * tweenProgress;
                transform.scaleX = frame.transform.scaleX + transform.scaleX * tweenProgress;
                transform.scaleY = frame.transform.scaleY + transform.scaleY * tweenProgress;
            }

            transform.add(timeline.originTransform);
        }

        protected void _globalToLocal(ArmatureData armature) // Support 2.x ~ 3.x data.
        {
            var keyFrames = new List<BoneFrameData>();
            var bones = armature.getSortedBones().ToArray();
            Array.Reverse(bones);

            foreach (var bone in bones)
            {
                if (bone.parent != null)
                {
                    bone.parent.transform.toMatrix(_helpMatrix);
                    _helpMatrix.invert();
                    _helpMatrix.transformPoint(bone.transform.x, bone.transform.y, _helpPoint);
                    bone.transform.x = _helpPoint.x;
                    bone.transform.y = _helpPoint.y;
                    bone.transform.rotation -= bone.parent.transform.rotation;
                }

                foreach (var pair in armature.animations)
                {
                    var animation = pair.Value;
                    var timeline = animation.getBoneTimeline(bone.name);

                    if (timeline == null)
                    {
                        continue;
                    }

                    var parentTimeline = bone.parent != null ? animation.getBoneTimeline(bone.parent.name) : null;
                    _helpTransformB.copyFrom(timeline.originTransform);
                    keyFrames.Clear();

                    var isFirstFrame = true;
                    foreach (var frame in timeline.frames)
                    {
                        if (keyFrames.Contains(frame))
                        {
                            continue;
                        }

                        keyFrames.Add(frame);

                        if (parentTimeline != null)
                        {
                            _getTimelineFrameMatrix(animation, parentTimeline, frame.position, _helpTransformA);
                            frame.transform.add(_helpTransformB);
                            _helpTransformA.toMatrix(_helpMatrix);
                            _helpMatrix.invert();
                            _helpMatrix.transformPoint(frame.transform.x, frame.transform.y, _helpPoint);
                            frame.transform.x = _helpPoint.x;
                            frame.transform.y = _helpPoint.y;
                            frame.transform.rotation -= _helpTransformA.rotation;
                        }
                        else
                        {
                            frame.transform.add(_helpTransformB);
                        }

                        frame.transform.minus(bone.transform);

                        if (isFirstFrame)
                        {
                            isFirstFrame = false;
                            timeline.originTransform.copyFrom(frame.transform);
                            frame.transform.identity();
                        }
                        else
                        {
                            frame.transform.minus(timeline.originTransform);
                        }
                    }
                }
            }
        }

        protected void _mergeFrameToAnimationTimeline(float framePostion, List<ActionData> actions, List<EventData> events)
        {
            var frameStart = (int)Math.Floor(framePostion * _armature.frameRate); // uint()
            var frames = _animation.frames;

            if (frames.Count == 0)
            {
                var startFrame = BaseObject.borrowObject<AnimationFrameData>(); // Add start frame.
                startFrame.position = 0.0f;

                if (_animation.frameCount > 1)
                {
                    // frames.Count = _animation.frameCount + 1; // One more count for zero duration frame. // TODO

                    var endFrame = BaseObject.borrowObject<AnimationFrameData>(); // Add end frame to keep animation timeline has two different frames atleast.
                    endFrame.position = _animation.frameCount / _armature.frameRate;

                    frames[0] = startFrame;
                    frames[(int)_animation.frameCount] = endFrame;
                }
            }

            AnimationFrameData insertedFrame = null;
            var replacedFrame = frames[frameStart];
            if (replacedFrame != null && (frameStart == 0 || frames[frameStart - 1] == replacedFrame.prev)) // Key frame.
            {
                insertedFrame = replacedFrame;
            }
            else
            {
                insertedFrame = BaseObject.borrowObject<AnimationFrameData>(); // Create frame.
                insertedFrame.position = frameStart / _armature.frameRate;
                frames[frameStart] = insertedFrame;

                for (int i = frameStart + 1, l = frames.Count; i < l; ++i) // Clear replaced frame.
                {
                    var frame = frames[i];
                    if (replacedFrame != null && frame == replacedFrame)
                    {
                        frames[i] = null;
                    }
                }
            }

            if (actions.Count > 0) // Merge actions.
            {
                foreach (var action in actions)
                {
                    insertedFrame.actions.Add(action);
                }
            }

            if (events.Count > 0) // Merge events.
            {
                foreach (var evt in events)
                {
                    insertedFrame.events.Add(evt);
                }
            }

            // Modify frame link and duration.
            AnimationFrameData prevFrame = null;
            AnimationFrameData nextFrame = null;
            for (int i = 0, l = frames.Count; i < l; ++i)
            {
                var currentFrame = frames[i];
                if (currentFrame != null && nextFrame != currentFrame)
                {
                    nextFrame = currentFrame;

                    if (prevFrame != null)
                    {
                        nextFrame.prev = prevFrame;
                        prevFrame.next = nextFrame;
                        prevFrame.duration = nextFrame.position - prevFrame.position;
                    }

                    prevFrame = nextFrame;
                }
                else
                {
                    frames[i] = prevFrame;
                }
            }

            nextFrame.duration = _animation.duration - nextFrame.position;

            nextFrame = frames[0];
            prevFrame.next = nextFrame;
            nextFrame.prev = prevFrame;
        }
    }
}