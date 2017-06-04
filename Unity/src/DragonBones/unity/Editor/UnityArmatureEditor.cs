using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

namespace DragonBones
{
	[CustomEditor(typeof(UnityArmatureComponent))]
	public class UnityArmatureEditor : Editor {
		
		private int _armatureIndex = -1;
		private int _animationIndex = -1;
		private int _sortingLayerIndex = -1;
		private long _nowTime = 0;
		private List<string> _armatureNames = null;
		private List<string> _animationNames = null;
		private List<string> _sortingLayerNames = null;
		private UnityArmatureComponent _armatureComponent = null;

		void OnEnable()
		{
			_armatureComponent = target as UnityArmatureComponent;

			// 
			_nowTime = System.DateTime.Now.Ticks;
			_sortingLayerNames = _getSortingLayerNames();
			_sortingLayerIndex = _sortingLayerNames.IndexOf(_armatureComponent.sortingLayerName);

			// Update armature.
			if (
				!EditorApplication.isPlayingOrWillChangePlaymode &&
				_armatureComponent.armature == null &&
				_armatureComponent.unityData != null &&
				!string.IsNullOrEmpty(_armatureComponent.armatureName)
			)
			{
				//clear cache
				UnityFactory.factory.Clear(true);
				// Load data.
				var dragonBonesData = UnityFactory.factory.LoadData(_armatureComponent.unityData);

				// Refresh texture atlas.
				UnityFactory.factory.RefreshAllTextureAtlas(_armatureComponent);

				// Refresh armature.
				UnityEditor.ChangeArmatureData(_armatureComponent, _armatureComponent.armatureName, dragonBonesData.name);

				// Refresh texture.
				_armatureComponent.armature.InvalidUpdate(null, true);

				// Play animation.
				if (!string.IsNullOrEmpty(_armatureComponent.animationName))
				{
					_armatureComponent.animation.Play(_armatureComponent.animationName);
				}
				_armatureComponent.CollectBones();
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
			_updateParameters();
		}

		public override void OnInspectorGUI()
		{
			if(_armatureIndex==-1){
				_updateParameters();
			}
			// DragonBones Data
			EditorGUILayout.BeginHorizontal();

			_armatureComponent.unityData = EditorGUILayout.ObjectField("DragonBones Data", _armatureComponent.unityData, typeof(UnityDragonBonesData), false) as UnityDragonBonesData;

			var created = false;
			if (_armatureComponent.unityData != null)
			{
				if (_armatureComponent.armature == null)
				{
					if (GUILayout.Button("Create"))
					{
						created = true;
					}
				}
				else
				{
					if (GUILayout.Button("Reload"))
					{
						if(EditorUtility.DisplayDialog("DragonBones Alert", "Are you sure you want to reload data", "Yes", "No")){
							created = true;
						}
					}
				}
			}

			if (created)
			{	
				//clear cache
				UnityFactory.factory.Clear(true);
				_armatureNames = null;
				_animationNames = null;
				_armatureIndex = -1;
				_animationIndex = -1;
				_armatureComponent.animationName = null;
				if (UnityEditor.ChangeDragonBonesData(_armatureComponent, _armatureComponent.unityData.dragonBonesJSON))
				{
					_armatureComponent.CollectBones();
					_updateParameters();
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

						var armatureName = _armatureNames[_armatureIndex];
						UnityEditor.ChangeArmatureData(_armatureComponent, armatureName, dragonBonesData.name);
						_updateParameters();
						if(_armatureComponent.bonesRoot!=null && _armatureComponent.unityBones!=null){
							_armatureComponent.ShowBones();
						}

						_armatureComponent.gameObject.name = armatureName;
						_armatureComponent.zorderIsDirty = true;

						EditorUtility.SetDirty(_armatureComponent);
						if (!Application.isPlaying && !_isPrefab()){
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}
				}

				// Animation
				if (_animationNames != null && _animationNames.Count > 0)
				{
					EditorGUILayout.BeginHorizontal();
					List<string> anims=new List<string>(_animationNames);
					anims.Insert(0,"<None>");
					var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex+1, anims.ToArray())-1;
					if (animationIndex != _animationIndex)
					{
						_animationIndex = animationIndex;
						if(animationIndex>=0){
							_armatureComponent.animationName = _animationNames[animationIndex];
							_armatureComponent.animation.Play(_armatureComponent.animationName);
							_updateParameters();
						}else{
							_armatureComponent.animationName = null;
							_armatureComponent.animation.Stop();
						}
						EditorUtility.SetDirty(_armatureComponent);
						if (!Application.isPlaying && !_isPrefab()){
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
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

				if(!_armatureComponent.isUGUI){
					bool haveSpriteGorup = false;
					#if UNITY_5_6_OR_NEWER
					haveSpriteGorup = _armatureComponent.GetComponent<UnityEngine.Rendering.SortingGroup>()!=null;
					#endif
					if(!haveSpriteGorup)
					{
						//sort mode
						serializedObject.Update();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("sortingMode"), true);
						serializedObject.ApplyModifiedProperties();

						// Sorting Layer
						_sortingLayerIndex = EditorGUILayout.Popup("Sorting Layer", _sortingLayerIndex, _sortingLayerNames.ToArray());
						if (_sortingLayerNames[_sortingLayerIndex] != _armatureComponent.sortingLayerName)
						{
							Undo.RecordObject(_armatureComponent, "Sorting Layer");
							_armatureComponent.sortingLayerName = _sortingLayerNames[_sortingLayerIndex];
							EditorUtility.SetDirty(_armatureComponent);
							if (!Application.isPlaying && !_isPrefab()){
								EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
							}
						}
						if(_armatureComponent.sortingMode ==  SortingMode.SortByZ){
							// Sorting Order
							var sortingOrder = EditorGUILayout.IntField("Order in Layer", _armatureComponent.sortingOrder);
							if (sortingOrder != _armatureComponent.sortingOrder)
							{
								Undo.RecordObject(_armatureComponent, "Edit Sorting Order");
								_armatureComponent.sortingOrder = sortingOrder;
								EditorUtility.SetDirty(_armatureComponent);
								if (!Application.isPlaying && !_isPrefab()){
									EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
								}
							}
						}
					}
					// ZSpace
					EditorGUILayout.BeginHorizontal();
					_armatureComponent.zSpace = EditorGUILayout.Slider("Z Space", _armatureComponent.zSpace, 0.0f, 0.2f);
					EditorGUILayout.EndHorizontal();
				}

				// TimeScale
				serializedObject.Update();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_timeScale"), true);
				serializedObject.ApplyModifiedProperties();
				_armatureComponent.animation.timeScale = serializedObject.FindProperty("_timeScale").floatValue;

				// Flip
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Flip");
				bool flipX = _armatureComponent.flipX;
				bool flipY = _armatureComponent.flipY;
				_armatureComponent.flipX = GUILayout.Toggle(_armatureComponent.flipX, "X",GUILayout.Width(30));
				_armatureComponent.flipY = GUILayout.Toggle(_armatureComponent.flipY, "Y",GUILayout.Width(30));
				_armatureComponent.armature.flipX = _armatureComponent.flipX;
				_armatureComponent.armature.flipY = _armatureComponent.flipY;
				if(_armatureComponent.flipX!=flipX || _armatureComponent.flipY!=flipY){
					EditorUtility.SetDirty(_armatureComponent);
					if (!Application.isPlaying && !_isPrefab()){
						EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
					}
				}
				EditorGUILayout.EndHorizontal();

				//normals
				EditorGUILayout.BeginHorizontal();
				_armatureComponent.addNormal = EditorGUILayout.Toggle("Normals", _armatureComponent.addNormal);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}

			if(_armatureComponent.armature!=null && _armatureComponent.armature.parent==null)
			{
				if(_armatureComponent.unityBones!=null && _armatureComponent.bonesRoot!=null)
				{
					_armatureComponent.boneHierarchy = EditorGUILayout.Toggle("Bone Hierarchy", _armatureComponent.boneHierarchy);
				}
				EditorGUILayout.BeginHorizontal();
				if(!Application.isPlaying){
					if(_armatureComponent.unityBones!=null && _armatureComponent.bonesRoot!=null){
						if(GUILayout.Button("Remove Bones",GUILayout.Height(20))){
							if(EditorUtility.DisplayDialog("DragonBones Alert", "Are you sure you want to remove bones", "Yes", "No")){
								_armatureComponent.RemoveBones();
							}
						}
					}else{
						if(GUILayout.Button("Show Bones",GUILayout.Height(20))){
							_armatureComponent.ShowBones();
						}
					}
				}
				if(!Application.isPlaying && !_armatureComponent.isUGUI){
					UnityCombineMesh ucm = _armatureComponent.gameObject.GetComponent<UnityCombineMesh>();
					if(!ucm) {
						if(GUILayout.Button("Add Mesh Combine",GUILayout.Height(20))){
							ucm = _armatureComponent.gameObject.AddComponent<UnityCombineMesh>();
						}
					}
				}
				EditorGUILayout.EndHorizontal();
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
			if (_armatureComponent.armature != null)
			{
				if(_armatureComponent.armature.armatureData.parent!=null)
				{
					_armatureNames = _armatureComponent.armature.armatureData.parent.armatureNames;
					_animationNames = _armatureComponent.armature.armatureData.animationNames;
					_armatureIndex = _armatureNames.IndexOf(_armatureComponent.armature.name);
					if(!string.IsNullOrEmpty(_armatureComponent.animationName)){
						_animationIndex = _animationNames.IndexOf(_armatureComponent.animationName);
					}
				}
				else
				{
					_armatureNames = null;
					_animationNames = null;
					_armatureIndex = -1;
					_animationIndex = -1;
				}
			}
			else
			{
				_armatureNames = null;
				_animationNames = null;
				_armatureIndex = -1;
				_animationIndex = -1;
			}
		}

		private bool _isPrefab(){
			return PrefabUtility.GetPrefabParent(_armatureComponent.gameObject) == null 
				&& PrefabUtility.GetPrefabObject(_armatureComponent.gameObject) != null;
		}

		private List<string> _getSortingLayerNames()
		{
			var internalEditorUtilityType = typeof(InternalEditorUtility);
			var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

			return new List<string>(sortingLayersProperty.GetValue(null, new object[0]) as string[]);
		}
	}
}