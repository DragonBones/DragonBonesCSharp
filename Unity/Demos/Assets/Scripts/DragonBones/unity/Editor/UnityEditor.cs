using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.SceneManagement;

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
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Armature Object");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private int _sortingLayerIndex = -1;
        private int _armatureIndex = -1;
        private int _animationIndex = -1;
        private long _nowTime = 0;
        private string[] _sortingLayerNames;
        private TextAsset _dragonBoneJSON = null;
        private List<string> _armatureNames = null;
        private List<string> _animationNames = null;
        private UnityArmatureComponent _armatureComponent = null;

        void OnEnable()
        {
            _armatureComponent = this.target as UnityArmatureComponent;
            _dragonBoneJSON = _armatureComponent == null ? null : _armatureComponent.dragonBonesJSON;

            // 
            _nowTime = System.DateTime.Now.Ticks;
            _sortingLayerNames = _getSortingLayerNames();
            _sortingLayerIndex = _getSortingLayerIndex(_armatureComponent.sortingLayerName);

            // Create armature.
            if (
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                _armatureComponent.armature == null &&
                _armatureComponent.dragonBonesJSON != null &&
                DragonBones.IsAvailableString(_armatureComponent.armatureName)
            )
            {
                var dragonBonesData = _armatureComponent.LoadData(_armatureComponent.dragonBonesJSON, _armatureComponent.textureAtlasJSON);
                UnityFactory.factory.RefreshAllTextureAtlas();

                _changeArmature(_armatureComponent.armatureName, dragonBonesData.name);
                _armatureComponent.armature.InvalidUpdate(null, true);
                if (DragonBones.IsAvailableString(_armatureComponent.animationName))
                {
                    _armatureComponent.animation.Play(_armatureComponent.animationName);
                }
            }

            // Update hideFlags.
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

            // 
            _update();
        }

        public override void OnInspectorGUI()
        {
            // DragonBones Data
            GUILayout.BeginHorizontal();

            _dragonBoneJSON = EditorGUILayout.ObjectField("DragonBones Data", _dragonBoneJSON, typeof(TextAsset), false) as TextAsset;

            bool created = false;
            if (_dragonBoneJSON != null)
            {
                if (_armatureComponent.armature == null)
                {
                    if (GUILayout.Button("Create"))
                        created = true;
                }
                else if (_armatureComponent.dragonBonesJSON != _dragonBoneJSON)
                {
                    if (GUILayout.Button("Change"))
                        created = true;
                }
            }

            if (created)
            {
                var textureAtlasJSON = new List<string>();
                _getTextureAtlasConfigs(textureAtlasJSON, AssetDatabase.GetAssetPath(_dragonBoneJSON.GetInstanceID()));

                var dragonBonesData = _armatureComponent.LoadData(_dragonBoneJSON, textureAtlasJSON);
                if (dragonBonesData != null)
                {
                    Undo.RecordObject(_armatureComponent, "Set Armature DragonBones");
                    _armatureComponent.dragonBonesJSON = _dragonBoneJSON;
                    _armatureComponent.textureAtlasJSON = textureAtlasJSON;

                    _clearGameObjectChildren(_armatureComponent.gameObject);
                    _changeArmature(dragonBonesData.armatureNames[0], dragonBonesData.name);
                    _update();
                    EditorUtility.SetDirty(_armatureComponent);
                }
                else
                {
                    _dragonBoneJSON = _armatureComponent.dragonBonesJSON;

                    EditorUtility.DisplayDialog("Error", "Could not load dragonBones data.", "OK", null);
                }
            }
            else if (_dragonBoneJSON == null && _armatureComponent.dragonBonesJSON != null)
            {
                Undo.RecordObject(_armatureComponent, "Set Armature DragonBones");
                _armatureComponent.dragonBonesJSON = null;

                if (_armatureComponent.armature != null)
                {
                    _armatureComponent.Dispose(false);
                }

                _clearGameObjectChildren(_armatureComponent.gameObject);
                EditorUtility.SetDirty(_armatureComponent);
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_armatureComponent.armature != null)
            {
                var dragonBonesData = _armatureComponent.armature.armatureData.parent;

                // Armature
                if (UnityFactory.factory.GetAllDragonBonesData().ContainsValue(dragonBonesData) && _armatureNames != null)
                {
                    var armatureIndex = EditorGUILayout.Popup("Armature", _armatureIndex, _armatureNames.ToArray());
                    if (_armatureIndex != armatureIndex)
                    {
                        _armatureIndex = armatureIndex;
                        _clearGameObjectChildren(_armatureComponent.gameObject);
                        _changeArmature(_armatureNames[_armatureIndex], dragonBonesData.name);

                        _update();

                        EditorUtility.SetDirty(_armatureComponent);
                    }
                }

                // Animation
                if (_animationNames != null && _animationNames.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex, _animationNames.ToArray());
                    if (animationIndex != _animationIndex)
                    {
                        _animationIndex = animationIndex;
                        _armatureComponent.animationName = _animationNames[animationIndex];
                        _armatureComponent.animation.Play(_armatureComponent.animationName);

                        _update();

                        EditorUtility.SetDirty(_armatureComponent);
                    }

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
                }

                EditorGUILayout.Space();

                // Sorting Layer
                _sortingLayerIndex = EditorGUILayout.Popup("Sorting Layer", _sortingLayerIndex, _sortingLayerNames);
                if (_sortingLayerNames[_sortingLayerIndex] != _armatureComponent.sortingLayerName)
                {
                    Undo.RecordObject(_armatureComponent, "Sorting Layer");
                    _armatureComponent.sortingLayerName = _sortingLayerNames[_sortingLayerIndex];
                    EditorUtility.SetDirty(_armatureComponent);
                }

                // Sorting Order
                var sortingOrder = EditorGUILayout.IntField("Order in Layer", _armatureComponent.sortingOrder);
                if (sortingOrder != _armatureComponent.sortingOrder)
                {
                    Undo.RecordObject(_armatureComponent, "Edit Sorting Order");
                    _armatureComponent.sortingOrder = sortingOrder;
                    EditorUtility.SetDirty(_armatureComponent);
                }

                // ZSpace
                GUILayout.BeginHorizontal();
                _armatureComponent.zSpace = EditorGUILayout.Slider("Z Space", _armatureComponent.zSpace, 0.0f, 0.2f);
                GUILayout.EndHorizontal();

                // TimeScale
                GUILayout.BeginHorizontal();
                _armatureComponent.animation.timeScale = EditorGUILayout.Slider("Time Scale", _armatureComponent.animation.timeScale, 0.0f, 2.0f);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode && Selection.activeObject == _armatureComponent.gameObject)
            {
                EditorUtility.SetDirty(_armatureComponent);
                HandleUtility.Repaint();
            }
        }

        void OnSceneGUI()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && _armatureComponent.armature != null)
            {
                var dt = System.DateTime.Now.Ticks - _nowTime;
                if (dt >= 1.0f / _armatureComponent.armature.armatureData.frameRate * 1000000.0f)
                {
                    _armatureComponent.armature.AdvanceTime(dt * 0.0000001f);
                    _nowTime = System.DateTime.Now.Ticks;
                }
            }
        }

        private void _update()
        {
            _dragonBoneJSON = _armatureComponent.dragonBonesJSON;

            if (_armatureComponent.armature != null)
            {
                _armatureNames = _armatureComponent.armature.armatureData.parent.armatureNames;
                _animationNames = _armatureComponent.armature.armatureData.animationNames;
                _armatureIndex = _armatureNames.IndexOf(_armatureComponent.armature.name);
                _animationIndex = _animationNames.IndexOf(_armatureComponent.armature.animation.lastAnimationName);
            }
            else
            {
                _armatureNames = null;
                _animationNames = null;
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

        private void _getTextureAtlasConfigs(List<string> textureAtlasFiles, string filePath, string rawName = null, string suffix = "tex")
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

            _getTextureAtlasConfigs(textureAtlasFiles, filePath, null, "texture");
        }

        private void _clearGameObjectChildren(GameObject gameObject)
        {
            var children = new List<GameObject>();
            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                children.Add(child);
            }
            for (int i = 0; i < childCount; ++i)
            {
                var child = children[i];
#if UNITY_EDITOR
                Object.DestroyImmediate(child);
#else
                Object.Destroy(child);
#endif
            }
        }
    }
}