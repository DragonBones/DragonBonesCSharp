using System.Collections.Generic;
using System.Collections;

namespace DragonBones
{
    /**
     * @private
     */
    public class ActionData : BaseObject
    {
        public ActionType type;
        public BoneData bone;
        public SlotData slot;
        public readonly ArrayList data = new ArrayList();

        public ActionData()
        {
        }

        protected override void _onClear()
        {
            type = ActionType.Play;
            bone = null;
            slot = null;
            data.Clear();
        }
    }

    /**
     * @private
     */
    public class EventData : BaseObject
    {

        public EventType type;
        public string name;
        //public any data; // TODO
        public BoneData bone;
        public SlotData slot;

        public EventData()
        {
        }

        protected override void _onClear()
        {
            type = EventType.Frame;
            name = null;
            //data = null;
            bone = null;
            slot = null;
        }
    }

    /**
     * @private
     */
    public abstract class FrameData<T> : BaseObject where T : FrameData<T>
    {
        public float position;
        public float duration;
        public T prev;
        public T next;

        public FrameData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            position = 0;
            duration = 0;
            prev = null;
            next = null;
        }
    }

    /**
     * @private
     */
    public abstract class TweenFrameData<T> : FrameData<T> where T : TweenFrameData<T>
    {
        private static void _getCurvePoint(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float t, Point result)
        {
            var l_t = 1 - t;
            var powA = l_t * l_t;
            var powB = t * t;
            var kA = l_t * powA;
            var kB = 3.0f * t * powA;
            var kC = 3.0f * l_t * powB;
            var kD = t * powB;

            result.x = kA * x1 + kB * x2 + kC * x3 + kD * x4;
            result.y = kA * y1 + kB * y2 + kC * y3 + kD * y4;
        }

        public static void SamplingEasingCurve(float[] curve, float[] samples)
        {
            var curveCount = curve.Length;
            var result = new Point();
            
            var stepIndex = -2;
            for (int i = 0, l = samples.Length; i < l; ++i) {
                var t = (float)(i + 1) / (l + 1);
                while ((stepIndex + 6 < curveCount ? curve[stepIndex + 6] : 1) < t) // stepIndex + 3 * 2
                { 
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
                while (higher - lower > 0.01f)
                {
                    var percentage = (higher + lower) / 2.0f;
                    _getCurvePoint(x1, y1, x2, y2, x3, y3, x4, y4, percentage, result);
                    if (t - result.x > 0.00f)
                    {
                        lower = percentage;
                    }
                    else
                    {
                        higher = percentage;
                    }
                }

                samples[i] = result.y;
            }
        }

        public float tweenEasing;
        public float[] curve;

        public TweenFrameData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            tweenEasing = 0.0f;
            curve = null;
        }
    }

    /**
     * @private
     */
    public class AnimationFrameData : FrameData<AnimationFrameData>
    {
        public readonly List<ActionData> actions = new List<ActionData>();
        public readonly List<EventData> events = new List<EventData>();

        public AnimationFrameData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            foreach (var action in actions)
            {
                action.ReturnToPool();
            }

            foreach (var evt in events)
            {
                evt.ReturnToPool();
            }

            actions.Clear();
            events.Clear();
        }
    }

    /**
     * @private
     */
    public class ZOrderFrameData : FrameData<ZOrderFrameData>
    {
        public List<int> zOrder = new List<int>();

        public ZOrderFrameData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            zOrder.Clear();
        }
    }

    /**
     * @private
     */
    public class BoneFrameData : TweenFrameData<BoneFrameData>
    {
        public bool tweenScale;
        public float tweenRotate;
        public readonly Transform transform = new Transform();

        public BoneFrameData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            tweenScale = false;
            tweenRotate = 0.0f;
            transform.Identity();
        }
    }

    /**
     * @private
     */
    public class SlotFrameData : TweenFrameData<SlotFrameData>
    {
        public static readonly ColorTransform DEFAULT_COLOR = new ColorTransform();
        public static ColorTransform GenerateColor()
        {
            return new ColorTransform();
        }

        public int displayIndex;
        public ColorTransform color;

        public SlotFrameData()
        {
        }

        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            displayIndex = 0;
            color = null;
        }
    }

    /**
     * @private
     */
    public class ExtensionFrameData : TweenFrameData<ExtensionFrameData>
    {
        public ExtensionType type;
        public readonly List<float> tweens = new List<float>();
        public readonly List<float> keys = new List<float>();

        public ExtensionFrameData()
        {
        }
        /**
         * @inheritDoc
         */
        protected override void _onClear()
        {
            base._onClear();

            type = ExtensionType.FFD;
            tweens.Clear();
            keys.Clear();
        }
    }
}
