using System;
using System.Collections.Generic;

namespace DragonBones
{
    public class BinaryDataParser : ObjectDataParser
    {
        private byte[] _binary;
        private int _binaryOffset;
        private short[] _intArrayBuffer;
        private float[] _floatArrayBuffer;
        private short[] _frameIntArrayBuffer;
        private float[] _frameFloatArrayBuffer;
        private short[] _frameArrayBuffer;
        private ushort[] _timelineArrayBuffer;
        /**
         * @private
         */
        private bool _InRange(int a, int min, int max)
        {
            return min <= a && a <= max;
        }
        /**
         * @private
         */
        private string _DecodeUTF8(ushort[] data)
        {
            var EOF_byte = -1;
            var EOF_code_point = -1;
            var FATAL_POINT = 0xFFFD;

            var pos = 0;
            var result = "";
            int? code_point;
            var utf8_code_point = 0;
            var utf8_bytes_needed = 0;
            var utf8_bytes_seen = 0;
            var utf8_lower_boundary = 0;

            while (data.Length > pos)
            {
                var _byte = data[pos++];

                if (_byte == EOF_byte)
                {
                    if (utf8_bytes_needed != 0)
                    {
                        code_point = FATAL_POINT;
                    }
                    else
                    {
                        code_point = EOF_code_point;
                    }
                }
                else
                {
                    if (utf8_bytes_needed == 0)
                    {
                        if (this._InRange(_byte, 0x00, 0x7F))
                        {
                            code_point = _byte;
                        }
                        else
                        {
                            if (this._InRange(_byte, 0xC2, 0xDF))
                            {
                                utf8_bytes_needed = 1;
                                utf8_lower_boundary = 0x80;
                                utf8_code_point = _byte - 0xC0;
                            }
                            else if (this._InRange(_byte, 0xE0, 0xEF))
                            {
                                utf8_bytes_needed = 2;
                                utf8_lower_boundary = 0x800;
                                utf8_code_point = _byte - 0xE0;
                            }
                            else if (this._InRange(_byte, 0xF0, 0xF4))
                            {
                                utf8_bytes_needed = 3;
                                utf8_lower_boundary = 0x10000;
                                utf8_code_point = _byte - 0xF0;
                            }
                            else
                            {

                            }

                            utf8_code_point = utf8_code_point * (int)Math.Pow(64, utf8_bytes_needed);
                            code_point = null;
                        }
                    }
                    else if (!this._InRange(_byte, 0x80, 0xBF))
                    {
                        utf8_code_point = 0;
                        utf8_bytes_needed = 0;
                        utf8_bytes_seen = 0;
                        utf8_lower_boundary = 0;
                        pos--;
                        code_point = _byte;
                    }
                    else
                    {
                        utf8_bytes_seen += 1;
                        utf8_code_point = utf8_code_point + (_byte - 0x80) * (int)Math.Pow(64, utf8_bytes_needed - utf8_bytes_seen);

                        if (utf8_bytes_seen != utf8_bytes_needed)
                        {
                            code_point = null;
                        }
                        else
                        {
                            var cp = utf8_code_point;
                            var lower_boundary = utf8_lower_boundary;
                            utf8_code_point = 0;
                            utf8_bytes_needed = 0;
                            utf8_bytes_seen = 0;
                            utf8_lower_boundary = 0;
                            if (this._InRange(cp, lower_boundary, 0x10FFFF) && !this._InRange(cp, 0xD800, 0xDFFF))
                            {
                                code_point = cp;
                            }
                            else
                            {
                                code_point = _byte;
                            }
                        }
                    }
                }

                //Decode string
                if (code_point != null && code_point != EOF_code_point)
                {
                    if (code_point <= 0xFFFF)
                    {
                        
                        if (code_point > 0) result += Convert.ToChar(code_point);
                    }
                    else
                    {
                        code_point -= 0x10000;
                        result += Convert.ToChar(0xD800 + ((code_point >> 10) & 0x3ff));
                        result += Convert.ToChar(0xDC00 + (code_point & 0x3ff));
                    }
                }
            }

            return result;
        }
        /**
         * @private
         */
        private TimelineData _ParseBinaryTimeline(TimelineType type, uint offset, TimelineData timelineData = null)
        {
            var timeline = timelineData != null ? timelineData : BaseObject.BorrowObject<TimelineData>();
            timeline.type = type;
            timeline.offset = offset;

            this._timeline = timeline;

            var keyFrameCount = this._timelineArrayBuffer[timeline.offset + (int)BinaryOffset.TimelineKeyFrameCount];

            if (keyFrameCount == 1)
            {
                timeline.frameIndicesOffset = -1;
            }
            else
            {
                // One more frame than animation.
                var totalFrameCount = this._animation.frameCount + 1;
                var frameIndices = this._data.frameIndices;

                timeline.frameIndicesOffset = frameIndices.Count;
                frameIndices.ResizeList(frameIndices.Count + (int)totalFrameCount);

                for (int i = 0, iK = 0, frameStart = 0, frameCount = 0; i < totalFrameCount; ++i)
                {
                    if (frameStart + frameCount <= i && iK < keyFrameCount)
                    {
                        frameStart = this._frameArrayBuffer[this._animation.frameOffset + this._timelineArrayBuffer[timeline.offset + (int)BinaryOffset.TimelineFrameOffset + iK]];
                        if (iK == keyFrameCount - 1)
                        {
                            frameCount = (int)this._animation.frameCount - frameStart;
                        }
                        else
                        {
                            frameCount = this._frameArrayBuffer[this._animation.frameOffset + this._timelineArrayBuffer[timeline.offset + (int)BinaryOffset.TimelineFrameOffset + iK + 1]] - frameStart;
                        }

                        iK++;
                    }

                    frameIndices[timeline.frameIndicesOffset + i] = (uint)(iK - 1);
                }
            }

            this._timeline = null; //

            return timeline;
        }

        /**
         * @private
         */
        protected override void _ParseMesh(Dictionary<string, object> rawData, MeshDisplayData mesh)
        {
            mesh.offset = (int)rawData[ObjectDataParser.OFFSET];

            var weightOffset = this._intArrayBuffer[mesh.offset + (int)BinaryOffset.MeshWeightOffset];

            if (weightOffset >= 0)
            {
                var weight = BaseObject.BorrowObject<WeightData>();

                var vertexCount = this._intArrayBuffer[mesh.offset + (int)BinaryOffset.MeshVertexCount];
                var boneCount = this._intArrayBuffer[weightOffset + (int)BinaryOffset.WeigthBoneCount];
                weight.offset = weightOffset;
                
                weight.bones.ResizeList(boneCount);

                for (var i = 0; i < boneCount; ++i)
                {
                    var boneIndex = this._intArrayBuffer[weightOffset + (int)BinaryOffset.WeigthBoneIndices + i];
                    weight.bones[i] = this._rawBones[boneIndex];
                }

                var boneIndicesOffset = weightOffset + (short)BinaryOffset.WeigthBoneIndices + boneCount;
                for (int i = 0, l = vertexCount; i < l; ++i)
                {
                    var vertexBoneCount = this._intArrayBuffer[boneIndicesOffset++];
                    weight.count += vertexBoneCount;
                    boneIndicesOffset += vertexBoneCount;
                }

                mesh.weight = weight;
            }
        }
        /**
         * @private
         */
        protected override AnimationData _ParseAnimation(Dictionary<string, object> rawData)
        {
            var animation = BaseObject.BorrowObject<AnimationData>();
            animation.frameCount = (uint)Math.Max(ObjectDataParser._GetNumber(rawData, ObjectDataParser.DURATION, 1), 1);
            animation.playTimes = (uint)ObjectDataParser._GetNumber(rawData, ObjectDataParser.PLAY_TIMES, 1);
            animation.duration = animation.frameCount / this._armature.frameRate;
            animation.fadeInTime = ObjectDataParser._GetNumber(rawData, ObjectDataParser.FADE_IN_TIME, 0.0f);
            animation.scale = ObjectDataParser._GetNumber(rawData, ObjectDataParser.SCALE, 1.0f);
            animation.name = ObjectDataParser._GetString(rawData, ObjectDataParser.NAME, ObjectDataParser.DEFAULT_NAME);
            if (animation.name.Length == 0)
            {
                animation.name = ObjectDataParser.DEFAULT_NAME;
            }

            // Offsets.
            var offsets = rawData[ObjectDataParser.OFFSET] as List<uint>;
            animation.frameIntOffset = offsets[0];
            animation.frameFloatOffset = offsets[1];
            animation.frameOffset = offsets[2];

            this._animation = animation;

            if (rawData.ContainsKey(ObjectDataParser.ACTION))
            {
                animation.actionTimeline = this._ParseBinaryTimeline(TimelineType.Action, (uint)rawData[ObjectDataParser.ACTION]);
            }

            if (rawData.ContainsKey(ObjectDataParser.Z_ORDER))
            {
                animation.zOrderTimeline = this._ParseBinaryTimeline(TimelineType.ZOrder, (uint)rawData[ObjectDataParser.Z_ORDER]);
            }

            if (rawData.ContainsKey(ObjectDataParser.BONE))
            {
                var rawTimeliness = rawData[ObjectDataParser.BONE] as Dictionary<string, object>;
                foreach (var k in rawTimeliness.Keys)
                {
                    var rawTimelines = rawTimeliness[k] as List<int>;

                    var bone = this._armature.GetBone(k);
                    if (bone == null)
                    {
                        continue;
                    }

                    for (int i = 0, l = rawTimelines.Count; i < l; i += 2)
                    {
                        var timelineType = rawTimelines[i];
                        var timelineOffset = rawTimelines[i + 1];
                        var timeline = this._ParseBinaryTimeline((TimelineType)timelineType, (uint)timelineOffset);
                        this._animation.AddBoneTimeline(bone, timeline);
                    }
                }
            }

            if (rawData.ContainsKey(ObjectDataParser.SLOT))
            {
                var rawTimeliness = rawData[ObjectDataParser.SLOT] as Dictionary<string, object>;
                foreach (var k in rawTimeliness.Keys)
                {
                    var rawTimelines = rawTimeliness[k] as List<int>;

                    var slot = this._armature.GetSlot(k);
                    if (slot == null)
                    {
                        continue;
                    }

                    for (int i = 0, l = rawTimelines.Count; i < l; i += 2)
                    {
                        var timelineType = rawTimelines[i];
                        var timelineOffset = rawTimelines[i + 1];
                        var timeline = this._ParseBinaryTimeline((TimelineType)timelineType, (uint)timelineOffset);
                        this._animation.AddSlotTimeline(slot, timeline);
                    }
                }
            }

            this._animation = null;

            return animation;
        }
        /**
         * @private
         */
        protected override void _ParseArray(Dictionary<string, object> rawData)
        {
            //TODO
            var offsets = rawData[ObjectDataParser.OFFSET] as List<int>;
            var intArray = new short[0];
            var floatArray = new float[0];
            var frameIntArray = new short[0];
            var frameFloatArray = new float[0];
            var frameArray = new short[0];
            var timelineArray = new ushort[0];

            this._data.intArray = this._intArrayBuffer = intArray;
            this._data.floatArray = this._floatArrayBuffer = floatArray;
            this._data.frameIntArray = this._frameIntArrayBuffer = frameIntArray;
            this._data.frameFloatArray = this._frameFloatArrayBuffer = frameFloatArray;
            this._data.frameArray = this._frameArrayBuffer = frameArray;
            this._data.timelineArray = this._timelineArrayBuffer = timelineArray;
        }
        /**
         * @inheritDoc
         */
        public override DragonBonesData ParseDragonBonesData(object rawObj, float scale = 1)
        {
            Helper.Assert(rawObj != null  && rawObj is byte[], "Data error.");

            //TODO
            var tag = new ushort[0];
            if (tag[0] != Convert.ToByte("D") ||
                tag[1] != Convert.ToByte("B") ||
                tag[2] != Convert.ToByte("D") ||
                tag[3] != Convert.ToByte("T")   )
            {
                Helper.Assert(false, "Nonsupport data.");
                return null;
            }

            return base.ParseDragonBonesData(null, scale);
        }

        private string _GetUTF16Key(string value)
        {
            for (int i = 0, l = value.Length; i<l; ++i)
            {
                if (Convert.ToByte(value[i]) > 255)
                {
                    return Uri.EscapeUriString(value);
                }
            }

            return value;
        }
    }
}
