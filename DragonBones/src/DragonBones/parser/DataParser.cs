using System.Collections.Generic;

namespace DragonBones
{
    /**
     * @private
     */
    public abstract class DataParser
    {
        protected const string DATA_VERSION_2_3 = "2.3";
        protected const string DATA_VERSION_3_0 = "3.0";
        protected const string DATA_VERSION_4_0 = "4.0";
        protected const string DATA_VERSION_4_5 = "4.5";
        protected const string DATA_VERSION_5_0 = "5.0";
        protected const string DATA_VERSION_5_5 = "5.5";
        protected const string DATA_VERSION = DATA_VERSION_5_5;
        protected static readonly List<string> DATA_VERSIONS = new List<string>()
        {
            DATA_VERSION_5_5,
            DATA_VERSION_5_0,
            DATA_VERSION_4_5,
            DATA_VERSION_4_0,
            DATA_VERSION_3_0,
            DATA_VERSION_2_3
        };

        protected static readonly string TEXTURE_ATLAS = "textureAtlas";
        protected static readonly string SUB_TEXTURE = "SubTexture";
        protected static readonly string FORMAT = "format";
        protected static readonly string IMAGE_PATH = "imagePath";
        protected static readonly string WIDTH = "width";
        protected static readonly string HEIGHT = "height";
        protected static readonly string ROTATED = "rotated";
        protected static readonly string FRAME_X = "frameX";
        protected static readonly string FRAME_Y = "frameY";
        protected static readonly string FRAME_WIDTH = "frameWidth";
        protected static readonly string FRAME_HEIGHT = "frameHeight";

        protected static readonly string DRADON_BONES = "dragonBones";
        protected static readonly string USER_DATA = "userData";
        protected static readonly string ARMATURE = "armature";
        protected static readonly string BONE = "bone";
        protected static readonly string IK = "ik";
        protected static readonly string SLOT = "slot";
        protected static readonly string SKIN = "skin";
        protected static readonly string DISPLAY = "display";
        protected static readonly string ANIMATION = "animation";
        protected static readonly string Z_ORDER = "zOrder";
        protected static readonly string FFD = "ffd";
        protected static readonly string FRAME = "frame";
        protected static readonly string TRANSLATE_FRAME = "translateFrame";
        protected static readonly string ROTATE_FRAME = "rotateFrame";
        protected static readonly string SCALE_FRAME = "scaleFrame";
        protected static readonly string DISPLAY_FRAME = "displayFrame";
        protected static readonly string COLOR_FRAME = "colorFrame";
        protected static readonly string DEFAULT_ACTIONS = "defaultActions";
        protected static readonly string ACTIONS = "actions";
        protected static readonly string EVENTS = "events";
        protected static readonly string INTS = "ints";
        protected static readonly string FLOATS = "floats";
        protected static readonly string STRINGS = "strings";
        protected static readonly string CANVAS = "canvas";

        protected const string TRANSFORM = "transform";
        protected const string PIVOT = "pivot";
        protected const string AABB = "aabb";
        protected const string COLOR = "color";

        protected static readonly string VERSION = "version";
        protected static readonly string COMPATIBLE_VERSION = "compatibleVersion";
        protected static readonly string FRAME_RATE = "frameRate";
        protected static readonly string TYPE = "type";
        protected static readonly string SUB_TYPE = "subType";
        protected static readonly string NAME = "name";
        protected static readonly string PARENT = "parent";
        protected static readonly string TARGET = "target";
        protected static readonly string SHARE = "share";
        protected static readonly string PATH = "path";
        protected static readonly string LENGTH = "length";
        protected static readonly string DISPLAY_INDEX = "displayIndex";
        protected static readonly string BLEND_MODE = "blendMode";
        protected static readonly string INHERIT_TRANSLATION = "inheritTranslation";
        protected static readonly string INHERIT_ROTATION = "inheritRotation";
        protected static readonly string INHERIT_SCALE = "inheritScale";
        protected static readonly string INHERIT_REFLECTION = "inheritReflection";
        protected static readonly string INHERIT_ANIMATION = "inheritAnimation";
        protected static readonly string INHERIT_FFD = "inheritFFD";
        protected static readonly string BEND_POSITIVE = "bendPositive";
        protected static readonly string CHAIN = "chain";
        protected static readonly string WEIGHT = "weight";

        protected static readonly string FADE_IN_TIME = "fadeInTime";
        protected static readonly string PLAY_TIMES = "playTimes";
        protected static readonly string SCALE = "scale";
        protected static readonly string OFFSET = "offset";
        protected static readonly string POSITION = "position";
        protected static readonly string DURATION = "duration";
        protected static readonly string TWEEN_TYPE = "tweenType";
        protected static readonly string TWEEN_EASING = "tweenEasing";
        protected static readonly string TWEEN_ROTATE = "tweenRotate";
        protected static readonly string TWEEN_SCALE = "tweenScale";
        protected static readonly string CLOCK_WISE = "clockwise";
        protected static readonly string CURVE = "curve";
        protected static readonly string SOUND = "sound";
        protected static readonly string EVENT = "event";
        protected static readonly string ACTION = "action";

        protected static readonly string X = "x";
        protected static readonly string Y = "y";
        protected static readonly string SKEW_X = "skX";
        protected static readonly string SKEW_Y = "skY";
        protected static readonly string SCALE_X = "scX";
        protected static readonly string SCALE_Y = "scY";
        protected static readonly string VALUE = "value";
        protected static readonly string ROTATE = "rotate";
        protected static readonly string SKEW = "skew";

        protected static readonly string ALPHA_OFFSET = "aO";
        protected static readonly string RED_OFFSET = "rO";
        protected static readonly string GREEN_OFFSET = "gO";
        protected static readonly string BLUE_OFFSET = "bO";
        protected static readonly string ALPHA_MULTIPLIER = "aM";
        protected static readonly string RED_MULTIPLIER = "rM";
        protected static readonly string GREEN_MULTIPLIER = "gM";
        protected static readonly string BLUE_MULTIPLIER = "bM";

        protected static readonly string UVS = "uvs";
        protected static readonly string VERTICES = "vertices";
        protected static readonly string TRIANGLES = "triangles";
        protected static readonly string WEIGHTS = "weights";
        protected static readonly string SLOT_POSE = "slotPose";
        protected static readonly string BONE_POSE = "bonePose";

        protected static readonly string GOTO_AND_PLAY = "gotoAndPlay";

        protected static readonly string DEFAULT_NAME = "default";

        protected static ArmatureType _GetArmatureType(string value)
        {
            switch (value.ToLower())
            {
                case "stage":
                    return ArmatureType.Stage;

                case "armature":
                    return ArmatureType.Armature;

                case "movieclip":
                    return ArmatureType.MovieClip;

                default:
                    return ArmatureType.None;
            }
        }

        protected static DisplayType _GetDisplayType(string value)
        {
            switch (value.ToLower())
            {
                case "image":
                    return DisplayType.Image;

                case "mesh":
                    return DisplayType.Mesh;

                case "armature":
                    return DisplayType.Armature;

                case "boundingbox":
                    return DisplayType.BoundingBox;

                default:
                    return DisplayType.None;
            }
        }

        protected static BoundingBoxType _GetBoundingBoxType(string value)
        {
            switch (value.ToLower())
            {
                case "rectangle":
                    return BoundingBoxType.Rectangle;

                case "ellipse":
                    return BoundingBoxType.Ellipse;

                case "polygon":
                    return BoundingBoxType.Polygon;

                default:
                    return BoundingBoxType.Rectangle;
            }
        }

        protected static ActionType _GetActionType(string value)
        {
            switch (value.ToLower())
            {
                case "play":
                    return ActionType.Play;

                case "frame":
                    return ActionType.Frame;

                case "sound":
                    return ActionType.Sound;

                default:
                    return ActionType.Play;
            }
        }

        protected static BlendMode _GetBlendMode(string value)
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

        public DataParser()
        {

        }

        /**
         * @private
         */
        public abstract DragonBonesData ParseDragonBonesData(object rawData, float scale);

        /**
         * @private
         */
        public abstract void ParseTextureAtlasData(object rawData, TextureAtlasData textureAtlasData, float scale);
    }
}