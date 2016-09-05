using System;
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
                _parseTransform(rawData[TRANSFORM], bone.transform);
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
                _parseColorTransform(rawData.ContainsKey(COLOR) ? rawData[COLOR] : rawData[COLOR_TRANSFORM], slot.color);
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
                    _parseColorTransform(rawData[COLOR_TRANSFORM], slot.color);
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
                    DragonBones.assert("Nonsupport data version.");
                }
            }
            else
            {
                DragonBones.assert("No data.");
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
                DragonBones.assert("No data.");
            }
        }
    }
}