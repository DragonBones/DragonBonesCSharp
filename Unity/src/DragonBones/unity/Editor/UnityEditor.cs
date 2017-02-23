using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

namespace DragonBones
{
    /**
     * @private
     */
    [CustomEditor(typeof(UnityArmatureComponent))]
    public class UnityArmatureEditor : Editor
    {
        [MenuItem("GameObject/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObjectMenuItem()
        {
            _createEmptyObject(_getSelectionParentTransform());
        }

        #region 右键JSON创建对应的Prefab
        [MenuItem("Assets/Create/DragonBones/Armature Object", true)]
        private static bool _createArmatureObjectFromJSONValidateMenuItem()
        {
            return _getDragonBonesJSONPaths().Count > 0;
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObjectFromJSONMenuItem()
        {
            var parentTransform = _getSelectionParentTransform();
            foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths())
            {
                var armatureComponent = _createEmptyObject(parentTransform);
                var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

                _changeDragonBonesData(armatureComponent, dragonBonesJSON);
            }
        }

        private static List<string> _getDragonBonesJSONPaths()
        {
            var dragonBonesJSONPaths = new List<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".json"))
                {
                    var jsonCode = File.ReadAllText(assetPath);
                    if (jsonCode.IndexOf("\"armature\":") > 0)
                    {
                        dragonBonesJSONPaths.Add(assetPath);
                    }
                }
            }

            return dragonBonesJSONPaths;
        }
        #endregion

        private static UnityArmatureComponent _createEmptyObject(UnityEngine.Transform parentTransform)
        {
            var gameObject = new GameObject("New Armature Object", typeof(UnityArmatureComponent));
            var armatureComponent = gameObject.GetComponent<UnityArmatureComponent>();
            gameObject.transform.SetParent(parentTransform, false);

            //
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = gameObject;
            EditorGUIUtility.PingObject(Selection.activeObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Armature Object");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return armatureComponent;
        }

        private static void _getTextureAtlasConfigs(List<string> textureAtlasFiles, string filePath, string rawName = null, string suffix = "tex")
        {
            var folder = Directory.GetParent(filePath).ToString();
            var name = rawName != null ? rawName : filePath.Substring(0, filePath.LastIndexOf(".")).Substring(filePath.LastIndexOf("/") + 1);
            int index = 0;
            var textureAtlasName = "";
            var textureAtlasConfigFile = "";

            textureAtlasName = !string.IsNullOrEmpty(name) ? name + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : suffix) : suffix;
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
                textureAtlasName = (!string.IsNullOrEmpty(name) ? name + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : suffix) : suffix) + "_" + (index++);
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

            if (suffix != "texture")
            {
                _getTextureAtlasConfigs(textureAtlasFiles, filePath, null, "texture");
            }
        }

        private static bool _changeDragonBonesData(UnityArmatureComponent _armatureComponent, TextAsset dragonBoneJSON)
        {
            if (dragonBoneJSON != null)
            {
                var textureAtlasJSONs = new List<string>();
                _getTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBoneJSON.GetInstanceID()));

                var dragonBonesData = _armatureComponent.LoadData(dragonBoneJSON, textureAtlasJSONs);
                if (dragonBonesData != null)
                {
                    Undo.RecordObject(_armatureComponent, "Set DragonBones");

                    _armatureComponent.dragonBonesJSON = dragonBoneJSON;
                    _armatureComponent.textureAtlasJSON = textureAtlasJSONs;

                    var armatureName = dragonBonesData.armatureNames[0];
                    _changeArmatureData(_armatureComponent, armatureName, dragonBonesData.name);

                    _armatureComponent.gameObject.name = armatureName;

                    EditorUtility.SetDirty(_armatureComponent);

                    return true;
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not load dragonBones data.", "OK", null);

                    return false;
                }
            }
            else if (_armatureComponent.dragonBonesJSON != null)
            {
                Undo.RecordObject(_armatureComponent, "Set DragonBones");

                _armatureComponent.dragonBonesJSON = null;

                if (_armatureComponent.armature != null)
                {
                    _armatureComponent.Dispose(false);
                }

                EditorUtility.SetDirty(_armatureComponent);

                return true;
            }

            return false;
        }

        private static void _changeArmatureData(UnityArmatureComponent _armatureComponent, string armatureName, string dragonBonesName)
        {
            Slot slot = null;
            if (_armatureComponent.armature != null)
            {
                slot = _armatureComponent.armature.parent;
                _armatureComponent.Dispose(false);
            }

            _armatureComponent.armatureName = armatureName;
            _armatureComponent = UnityFactory.factory.BuildArmatureComponent(_armatureComponent.armatureName, dragonBonesName, null, null, _armatureComponent.gameObject);

            if (slot != null)
            {
                slot.childArmature = _armatureComponent.armature;
            }

            _armatureComponent.sortingLayerName = _armatureComponent.sortingLayerName;
            _armatureComponent.sortingOrder = _armatureComponent.sortingOrder;
        }

        private static void _clearGameObjectChildren(GameObject gameObject)
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

        private static List<string> _getSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

            return new List<string>(sortingLayersProperty.GetValue(null, new object[0]) as string[]);
        }

        private static UnityEngine.Transform _getSelectionParentTransform()
        {
            var parent = Selection.activeObject as GameObject;
            return parent != null ? parent.transform : null;
        }

        private int _armatureIndex = -1;
        private int _animationIndex = -1;
        private int _sortingLayerIndex = -1;
        private long _nowTime = 0;
        private TextAsset _dragonBoneJSON = null;
        private List<string> _armatureNames = null;
        private List<string> _animationNames = null;
        private List<string> _sortingLayerNames = null;
        private UnityArmatureComponent _armatureComponent = null;

        void OnEnable()
        {
            _armatureComponent = target as UnityArmatureComponent;
            _dragonBoneJSON = _armatureComponent == null ? null : _armatureComponent.dragonBonesJSON; //

            // 
            _nowTime = System.DateTime.Now.Ticks;
            _sortingLayerNames = _getSortingLayerNames();
            _sortingLayerIndex = _sortingLayerNames.IndexOf(_armatureComponent.sortingLayerName);

            // Update armature.
            if (
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                _armatureComponent.armature == null &&
                _armatureComponent.dragonBonesJSON != null &&
                !string.IsNullOrEmpty(_armatureComponent.armatureName)
            )
            {
                // Load data.
                var dragonBonesData = _armatureComponent.LoadData(_armatureComponent.dragonBonesJSON, _armatureComponent.textureAtlasJSON);

                // Refresh texture atlas.
                UnityFactory.factory.RefreshAllTextureAtlas();

                // Refresh armature.
                _changeArmatureData(_armatureComponent, _armatureComponent.armatureName, dragonBonesData.name);

                // Refresh texture.
                _armatureComponent.armature.InvalidUpdate(null, true);

                // Play animation.
                if (!string.IsNullOrEmpty(_armatureComponent.animationName))
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
            _updateParameters();
        }

        public override void OnInspectorGUI()
        {
            // DragonBones Data
            EditorGUILayout.BeginHorizontal();

            _dragonBoneJSON = EditorGUILayout.ObjectField("DragonBones Data", _dragonBoneJSON, typeof(TextAsset), false) as TextAsset;

            var created = false;
            if (_dragonBoneJSON != null)
            {
                if (_armatureComponent.armature == null)
                {
                    if (GUILayout.Button("Create"))
                    {
                        created = true;
                    }
                }
                else if (_armatureComponent.dragonBonesJSON != _dragonBoneJSON)
                {
                    if (GUILayout.Button("Change"))
                    {
                        created = true;
                    }
                }
            }
            else if (_armatureComponent.dragonBonesJSON != null)
            {
                created = true;
            }

            if (created)
            {
                var currentDragonBoneJSON = _armatureComponent.dragonBonesJSON;

                if (_changeDragonBonesData(_armatureComponent, _dragonBoneJSON))
                {
                    _updateParameters();
                }
                else
                {
                    _dragonBoneJSON = currentDragonBoneJSON;
                }
            }

            EditorGUILayout.EndHorizontal();
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
                        //_clearGameObjectChildren(_armatureComponent.gameObject);

                        var armatureName = _armatureNames[_armatureIndex];
                        _changeArmatureData(_armatureComponent, armatureName, dragonBonesData.name);
                        _updateParameters();

                        _armatureComponent.gameObject.name = armatureName;

                        EditorUtility.SetDirty(_armatureComponent);
                    }
                }

                // Animation
                if (_animationNames != null && _animationNames.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex, _animationNames.ToArray());
                    if (animationIndex != _animationIndex)
                    {
                        _animationIndex = animationIndex;
                        _armatureComponent.animationName = _animationNames[animationIndex];
                        _armatureComponent.animation.Play(_armatureComponent.animationName);

                        _updateParameters();

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

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                // Sorting Layer
                _sortingLayerIndex = EditorGUILayout.Popup("Sorting Layer", _sortingLayerIndex, _sortingLayerNames.ToArray());
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
                EditorGUILayout.BeginHorizontal();
                _armatureComponent.zSpace = EditorGUILayout.Slider("Z Space", _armatureComponent.zSpace, 0.0f, 0.2f);
                EditorGUILayout.EndHorizontal();

                // TimeScale
                EditorGUILayout.BeginHorizontal();
                _armatureComponent.animation.timeScale = EditorGUILayout.Slider("Time Scale", _armatureComponent.animation.timeScale, 0.0f, 2.0f);
                EditorGUILayout.EndHorizontal();

                // Flip
                /*EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Flip");
                _armatureComponent.flipX = GUILayout.Toggle(_armatureComponent.flipX, "X");
                _armatureComponent.flipY = GUILayout.Toggle(_armatureComponent.flipY, "Y");
                EditorGUILayout.EndHorizontal();*/

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

        private void _updateParameters()
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
                _armatureIndex = -1;
                _animationIndex = -1;
            }
        }
    }
}