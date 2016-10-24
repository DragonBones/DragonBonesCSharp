using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace DragonBones
{
    /**
     * @private
     */
    [CustomEditor(typeof(UnityArmatureComponent))]
    public class UnityArmatureEditor : Editor
    {
        [MenuItem("GameObject/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObject()
        {
            _createObject<UnityArmatureComponent>("New Armature Object");
        }

        private static void _createObject<T>(string name) where T : MonoBehaviour
        {
            var parent = Selection.activeObject as GameObject;
            var parentTransform = parent != null ? parent.transform : null;

            var gameObject = new GameObject(name, typeof(T));
            gameObject.transform.SetParent(parentTransform, false);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = gameObject;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private int _selectedOption = -1;
        private int _armatureIndex = -1;
        private int _animationIndex = -1;
        private string[] _sortingLayerNames;
        private TextAsset _dragonBoneJSON = null;
        private List<string> _armatureNames = null;
        private List<string> _animationNames = null;
        private UnityArmatureComponent _armatureComponent = null;
        private long _nowTime;

        void Awake()
        {
            _nowTime = System.DateTime.Now.Ticks;
            _armatureComponent = this.target as UnityArmatureComponent;
            _dragonBoneJSON = _armatureComponent.draggonBonesJSON;
            
            if (
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                _armatureComponent.draggonBonesJSON != null &&
                _armatureComponent.armature == null
            )
            {
                if (DragonBones.IsAvailableString(_armatureComponent.armatureName))
                {
                    var dragonBonesData = _armatureComponent.LoadData();
                    _changeArmature(_armatureComponent.armatureName, dragonBonesData.name);

                    if (DragonBones.IsAvailableString(_armatureComponent.animationName))
                    {
                        _armatureComponent.animation.Play(_armatureComponent.animationName);
                        _armatureComponent.animation.Stop();
                    }
                }
            }

            _update();
            _sortingLayerNames = _getSortingLayerNames();
            _selectedOption = _getSortingLayerIndex(_armatureComponent.sortingLayerName);

            //
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                _armatureComponent.armature != null &&
                _armatureComponent.armature.parent != null
            )
            {
                _armatureComponent.gameObject.hideFlags = HideFlags.NotEditable;
            }
            else
            {
                _armatureComponent.gameObject.hideFlags = HideFlags.None;
            }
        }

        public override void OnInspectorGUI()
        {
            // DragonBones Data
            GUILayout.BeginHorizontal();

            var dragonBonesJSON = EditorGUILayout.ObjectField("DragonBones Data", _dragonBoneJSON, typeof(TextAsset), false) as TextAsset;
            if (_dragonBoneJSON != dragonBonesJSON)
            {
                _dragonBoneJSON = dragonBonesJSON;
                if (_dragonBoneJSON == null)
                {
                }
                else
                {
                }
            }

            if (_armatureComponent.draggonBonesJSON != _dragonBoneJSON && GUILayout.Button(_armatureComponent.armature == null ? "Create" : "Change"))
            {
                DragonBonesData dragonBonesData = null;
                _armatureComponent.draggonBonesJSON = _dragonBoneJSON;

                if (_armatureComponent.draggonBonesJSON != null)
                {
                    //try
                    //{
                        _armatureComponent.textureAtlasJSON = new List<string>();
                        _getTextureAtlasConfigs(
                            _armatureComponent.textureAtlasJSON,
                            AssetDatabase.GetAssetPath(_armatureComponent.draggonBonesJSON.GetInstanceID())
                            );

                        dragonBonesData = _armatureComponent.LoadData();
                    /*}
                    catch
                    {
                    }*/
                }

                if (dragonBonesData == null)
                {
                    _dragonBoneJSON = null;
                    _armatureComponent.draggonBonesJSON = null;
                    _armatureComponent.textureAtlasJSON = null;
                    EditorUtility.DisplayDialog("Error", "Could not load DragonBones Data.", "OK", null);
                }
                else
                {
                    _armatureComponent.ClearChildren();
                    _changeArmature(dragonBonesData.armatureNames[0], dragonBonesData.name);
                    _update();
                }
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_armatureComponent.armature != null)
            {
                var dragonBonesData = _armatureComponent.armature.armatureData.parent;

                if (
                    _armatureNames != null &&
                    UnityFactory.factory.GetAllDragonBonesData().ContainsValue(dragonBonesData)
                )
                {
                    // Armature
                    var armatureIndex = EditorGUILayout.Popup("Armature", _armatureIndex, _armatureNames.ToArray());
                    if (_armatureIndex != armatureIndex)
                    {
                        _armatureIndex = armatureIndex;
                        _armatureComponent.ClearChildren();
                        _changeArmature(_armatureNames[_armatureIndex], dragonBonesData.name);
                        _update();
                    }
                }

                if (_animationNames != null)
                {
                    // Animation
                    GUILayout.BeginHorizontal();
                    var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex, _animationNames.ToArray());

                    if (_animationIndex >= 0)
                    {
                        if (_armatureComponent.animation.isPlaying)
                        {
                            if (GUILayout.Button("Stop"))
                            {
                                _armatureComponent.animation.Stop();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Play"))
                            {
                                _armatureComponent.animation.Play();
                            }
                        }
                    }

                    GUILayout.EndHorizontal();

                    if (animationIndex != _animationIndex)
                    {
                        _animationIndex = animationIndex;
                        _armatureComponent.animationName = _animationNames[animationIndex];
                        _armatureComponent.animation.Play(_armatureComponent.animationName);
                    }
                }

                EditorGUILayout.Space();

                // ZSpace
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Z Space", GUILayout.Width(120.0f));
                _armatureComponent.zSpace = GUILayout.HorizontalSlider(_armatureComponent.zSpace, 0.0f, 0.2f);
                GUILayout.EndHorizontal();

                // TimeScale
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time Scale", GUILayout.Width(120.0f));
                _armatureComponent.animation.timeScale = GUILayout.HorizontalSlider(_armatureComponent.animation.timeScale, 0.0f, 2.0f);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                _selectedOption = EditorGUILayout.Popup("Sorting Layer", _selectedOption, _sortingLayerNames);
                if (_sortingLayerNames[_selectedOption] != _armatureComponent.sortingLayerName)
                {
                    Undo.RecordObject(_armatureComponent, "Sorting Layer");
                    _armatureComponent.sortingLayerName = _sortingLayerNames[_selectedOption];
                    EditorUtility.SetDirty(_armatureComponent);
                }

                int newSortingLayerOrder = EditorGUILayout.IntField("Order in Layer", _armatureComponent.sortingOrder);
                if (newSortingLayerOrder != _armatureComponent.sortingOrder)
                {
                    Undo.RecordObject(_armatureComponent, "Edit Sorting Order");
                    _armatureComponent.sortingOrder = newSortingLayerOrder;
                    EditorUtility.SetDirty(_armatureComponent);
                }
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode && Selection.activeObject == _armatureComponent.gameObject)
            {
                EditorUtility.SetDirty(_armatureComponent);
                HandleUtility.Repaint();
            }
        }

        void OnSceneGUI()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && _armatureComponent.armature != null && _armatureComponent.animation.isPlaying)
            {
                long dt = System.DateTime.Now.Ticks-_nowTime;
                if(dt>=1f/_armatureComponent.armature.armatureData.frameRate*1000000f){
                    _armatureComponent.armature.AdvanceTime(dt*0.0000001f);
                    _nowTime = System.DateTime.Now.Ticks;
                }
            }
        }

        private void _update()
        {
            if (_armatureComponent.armature != null)
            {
                _armatureNames = _armatureComponent.armature.armatureData.parent.armatureNames;
                _animationNames = _armatureComponent.armature.armatureData.animationNames;
                _armatureIndex = _armatureNames.IndexOf(_armatureComponent.armature.name);
                _animationIndex = _animationNames.IndexOf(_armatureComponent.armature.animation.lastAnimationName);
            }
        }

        private void _getTextureAtlasConfigs(List<string> textureAtlasFiles, string filePath, string rawName = null, string suffix = "texture")
        {
            var folder = Directory.GetParent(filePath).ToString();
            var name = rawName != null ? rawName : filePath.Substring(0, filePath.LastIndexOf(".")).Substring(filePath.LastIndexOf("/") + 1);
            int index = 0;
            var textureAtlasName = "";
            var textureAtlasConfigFile = "";

            textureAtlasName = DragonBones.IsAvailableString(name) ? name + (DragonBones.IsAvailableString(suffix) ? "_" + suffix : suffix) : suffix;
            textureAtlasConfigFile = folder + "/" + textureAtlasName + ".json";

            if (File.Exists(textureAtlasConfigFile))
            {
                textureAtlasFiles.Add(textureAtlasConfigFile);
                return;
            }

            if (textureAtlasFiles.Count > 0 || rawName != null)
            {
                return;
            }

            while (true)
            {
                textureAtlasName = (DragonBones.IsAvailableString(name) ? name + (DragonBones.IsAvailableString(suffix) ? "_" + suffix : suffix) : suffix) + "_" + (index++);
                textureAtlasConfigFile = folder + "/" + textureAtlasName + ".json";
                if (File.Exists(textureAtlasConfigFile))
                {
                    textureAtlasFiles.Add(textureAtlasConfigFile);
                }
                else if (index > 1)
                {
                    break;
                }
            }

            _getTextureAtlasConfigs(textureAtlasFiles, filePath, "", suffix);
            if (textureAtlasFiles.Count > 0)
            {
                return;
            }

            index = name.LastIndexOf("_");
            if (index >= 0)
            {
                name = name.Substring(0, index);

                _getTextureAtlasConfigs(textureAtlasFiles, filePath, name, suffix);
                if (textureAtlasFiles.Count > 0)
                {
                    return;
                }

                _getTextureAtlasConfigs(textureAtlasFiles, filePath, name, "");
                if (textureAtlasFiles.Count > 0)
                {
                    return;
                }
            }
        }

        private void _changeArmature(string armatureName, string dragonBonesName)
        {
            Slot slot = null;
            if (_armatureComponent.armature != null)
            {
                slot = _armatureComponent.armature.parent;
                _armatureComponent.Dispose(false);
            }

            _armatureComponent.armatureName = armatureName;
            _armatureComponent = UnityFactory.factory.BuildArmatureComponent(_armatureComponent.armatureName, dragonBonesName, null, _armatureComponent.gameObject);

            if (slot != null)
            {
                slot.childArmature = _armatureComponent.armature;
                UnityFactory.clock.Remove(_armatureComponent.armature);
            }

            _armatureComponent.sortingLayerName = _armatureComponent.sortingLayerName;
            _armatureComponent.sortingOrder = _armatureComponent.sortingOrder;
        }

        private string[] _getSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

            return sortingLayersProperty.GetValue(null, new object[0]) as string[];
        }

        private int _getSortingLayerIndex(string layerName)
        {
            for (int i = 0; i < _sortingLayerNames.Length; ++i)
            {
                if (_sortingLayerNames[i] == layerName)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}