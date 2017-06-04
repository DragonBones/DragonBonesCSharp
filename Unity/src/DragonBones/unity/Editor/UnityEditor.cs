using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

namespace DragonBones
{
    public class UnityEditor
	{
        [MenuItem("GameObject/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObjectMenuItem()
        {
            _createEmptyObject(GetSelectionParentTransform());
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object", true)]
        private static bool _createArmatureObjectFromJSONValidateMenuItem()
        {
            return _getDragonBonesJSONPaths().Count > 0;
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObjectFromJSONMenuItem()
        {
            var parentTransform = GetSelectionParentTransform();
            foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths())
            {
                var armatureComponent = _createEmptyObject(parentTransform);
                var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

                ChangeDragonBonesData(armatureComponent, dragonBonesJSON);
            }
        }

		[MenuItem("GameObject/DragonBones/Armature Object(UGUI)", false, 11)]
		private static void _createUGUIArmatureObjectMenuItem()
		{
			var armatureComponent = _createEmptyObject(GetSelectionParentTransform());
			armatureComponent.isUGUI=true;
			if(armatureComponent.GetComponentInParent<Canvas>()==null){
				var canvas = GameObject.Find("/Canvas");
				if(canvas){
					armatureComponent.transform.SetParent(canvas.transform);
				}
			}
			armatureComponent.transform.localScale = Vector2.one*100f;
			armatureComponent.transform.localPosition = Vector3.zero;
		}

		[MenuItem("Assets/Create/DragonBones/Armature Object(UGUI)", true)]
		private static bool _createUGUIArmatureObjectFromJSONValidateMenuItem()
		{
			return _getDragonBonesJSONPaths().Count > 0;
		}

		[MenuItem("Assets/Create/DragonBones/Armature Object(UGUI)", false, 11)]
		private static void _createNUGUIArmatureObjectFromJSOIMenuItem()
		{
			var parentTransform = GetSelectionParentTransform();
			foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths())
			{
				var armatureComponent = _createEmptyObject(parentTransform);
				armatureComponent.isUGUI=true;
				if(armatureComponent.GetComponentInParent<Canvas>()==null){
					var canvas = GameObject.Find("/Canvas");
					if(canvas){
						armatureComponent.transform.SetParent(canvas.transform);
					}
				}
				armatureComponent.transform.localScale = Vector2.one*100f;
				armatureComponent.transform.localPosition = Vector3.zero;
				var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

				ChangeDragonBonesData(armatureComponent, dragonBonesJSON);
			}
		}


		[MenuItem("Assets/Create/DragonBones/Create Unity Data", true)]
		private static bool _createUnityDataValidateMenuItem()
		{
			return _getDragonBonesJSONPaths(true).Count > 0;
		}

		[MenuItem("Assets/Create/DragonBones/Create Unity Data", false, 32)]
		private static void _createUnityDataMenuItem()
		{
			foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths(true))
			{
				var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;
				var textureAtlasJSONs = new List<string>();
				GetTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBonesJSON.GetInstanceID()));
				UnityDragonBonesData.TextureAtlas[] textureAtlas = new UnityDragonBonesData.TextureAtlas[textureAtlasJSONs.Count];
				for(int i=0;i<textureAtlasJSONs.Count;++i){
					string path = textureAtlasJSONs[i];
					//load textureAtlas data
					UnityDragonBonesData.TextureAtlas ta = new UnityDragonBonesData.TextureAtlas();
					ta.textureAtlasJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
					//load texture
					path = path.Substring(0,path.LastIndexOf(".json"));
					ta.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path+".png");
					//load material
					ta.material = AssetDatabase.LoadAssetAtPath<Material>(path+"_Mat.mat");
					ta.uiMaterial = AssetDatabase.LoadAssetAtPath<Material>(path+"_UI_Mat.mat");
					textureAtlas[i] = ta;
				}
				CreateUnityDragonBonesData(dragonBonesJSON,textureAtlas);
			}
		}




		public static bool ChangeDragonBonesData(UnityArmatureComponent _armatureComponent, TextAsset dragonBoneJSON)
		{
			if (dragonBoneJSON != null)
			{
				var textureAtlasJSONs = new List<string>();
				UnityEditor.GetTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBoneJSON.GetInstanceID()));
				UnityDragonBonesData.TextureAtlas[] textureAtlas = new UnityDragonBonesData.TextureAtlas[textureAtlasJSONs.Count];
				for(int i=0;i<textureAtlasJSONs.Count;++i){
					string path = textureAtlasJSONs[i];
					//load textureAtlas data
					UnityDragonBonesData.TextureAtlas ta = new UnityDragonBonesData.TextureAtlas();
					ta.textureAtlasJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
					//load texture
					path = path.Substring(0,path.LastIndexOf(".json"));
					ta.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path+".png");
					//load material
					ta.material = AssetDatabase.LoadAssetAtPath<Material>(path+"_Mat.mat");
					ta.uiMaterial = AssetDatabase.LoadAssetAtPath<Material>(path+"_UI_Mat.mat");
					textureAtlas[i] = ta;
				}
				UnityDragonBonesData data = UnityEditor.CreateUnityDragonBonesData(dragonBoneJSON,textureAtlas);
				_armatureComponent.unityData = data;

				var dragonBonesData = UnityFactory.factory.LoadData(data,_armatureComponent.isUGUI);
				if (dragonBonesData != null)
				{
					Undo.RecordObject(_armatureComponent, "Set DragonBones");

					_armatureComponent.unityData = data;

					var armatureName = dragonBonesData.armatureNames[0];
					ChangeArmatureData(_armatureComponent, armatureName, dragonBonesData.name);

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
			else if (_armatureComponent.unityData != null)
			{
				Undo.RecordObject(_armatureComponent, "Set DragonBones");

				_armatureComponent.unityData = null;

				if (_armatureComponent.armature != null)
				{
					_armatureComponent.Dispose(false);
				}

				EditorUtility.SetDirty(_armatureComponent);

				return true;
			}

			return false;
		}

		public static void ChangeArmatureData(UnityArmatureComponent _armatureComponent, string armatureName, string dragonBonesName)
		{
			Slot slot = null;
			if (_armatureComponent.armature != null)
			{
				slot = _armatureComponent.armature.parent;
				_armatureComponent.Dispose(false);
			}
			_armatureComponent.armatureName = armatureName;

			_armatureComponent = UnityFactory.factory.BuildArmatureComponent(_armatureComponent.armatureName, dragonBonesName, null, _armatureComponent.unityData.dataName, _armatureComponent.gameObject,_armatureComponent.isUGUI);
			if (slot != null)
			{
				slot.childArmature = _armatureComponent.armature;
			}

			_armatureComponent.sortingLayerName = _armatureComponent.sortingLayerName;
			_armatureComponent.sortingOrder = _armatureComponent.sortingOrder;
		}


		public static UnityEngine.Transform GetSelectionParentTransform()
		{
			var parent = Selection.activeObject as GameObject;
			return parent != null ? parent.transform : null;
		}



		public static void GetTextureAtlasConfigs(List<string> textureAtlasFiles, string filePath, string rawName = null, string suffix = "tex")
		{
			var folder = Directory.GetParent(filePath).ToString();
			var name = rawName != null ? rawName : filePath.Substring(0, filePath.LastIndexOf(".")).Substring(filePath.LastIndexOf("/") + 1);
			if(name.LastIndexOf("_ske")==name.Length-4){
				name = name.Substring(0,name.LastIndexOf("_ske"));
			}
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

			GetTextureAtlasConfigs(textureAtlasFiles, filePath, "", suffix);
			if (textureAtlasFiles.Count > 0)
			{
				return;
			}

			index = name.LastIndexOf("_");
			if (index >= 0)
			{
				name = name.Substring(0, index);

				GetTextureAtlasConfigs(textureAtlasFiles, filePath, name, suffix);
				if (textureAtlasFiles.Count > 0)
				{
					return;
				}

				GetTextureAtlasConfigs(textureAtlasFiles, filePath, name, "");
				if (textureAtlasFiles.Count > 0)
				{
					return;
				}
			}

			if (suffix != "texture")
			{
				GetTextureAtlasConfigs(textureAtlasFiles, filePath, null, "texture");
			}
		}

		public static UnityDragonBonesData CreateUnityDragonBonesData(TextAsset dragonBonesJSON,UnityDragonBonesData.TextureAtlas[] textureAtlas){
			if(dragonBonesJSON){
				bool isDirty = false;
				string path = AssetDatabase.GetAssetPath(dragonBonesJSON);
				path = path.Substring(0,path.Length-5);
				int index = path.LastIndexOf("_ske");
				if(index>0){
					path = path.Substring(0,index);
				}
				string dataPath = path+"_Data.asset";
				UnityDragonBonesData data = AssetDatabase.LoadAssetAtPath<UnityDragonBonesData>(dataPath);
				if(data==null){
					data = UnityDragonBonesData.CreateInstance<UnityDragonBonesData>();
					AssetDatabase.CreateAsset(data,dataPath);
					isDirty = true;
				}
				string name = path.Substring(path.LastIndexOf("/")+1);
				if(string.IsNullOrEmpty(data.dataName) || !data.dataName.Equals(name)){
					data.dataName = name;
					isDirty = true;
				}
				if(data.dragonBonesJSON!=dragonBonesJSON){
					data.dragonBonesJSON = dragonBonesJSON;
					isDirty = true;
				}

				if(textureAtlas!=null && textureAtlas.Length>0 && textureAtlas[0]!=null && textureAtlas[0].texture!=null){
					if(data.textureAtlas == null || data.textureAtlas.Length!=textureAtlas.Length){
						isDirty = true;
					}else{
						for(int i=0;i<textureAtlas.Length;++i){
							if(textureAtlas[i].material!=data.textureAtlas[i].material || 
								textureAtlas[i].uiMaterial!=data.textureAtlas[i].uiMaterial || 
								textureAtlas[i].texture!=data.textureAtlas[i].texture || 
								textureAtlas[i].textureAtlasJSON!=data.textureAtlas[i].textureAtlasJSON
							){
								isDirty = true;
								break;
							}
						}
					}
					data.textureAtlas = textureAtlas;
				}
				if(isDirty){
					AssetDatabase.Refresh();
					EditorUtility.SetDirty(data);
				}
				AssetDatabase.SaveAssets();
				return data;
			}
			return null;
		}


		private static List<string> _getDragonBonesJSONPaths( bool isCreateUnityData = false)
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
				else if(!isCreateUnityData && assetPath.EndsWith("_Data.asset"))
				{
					UnityDragonBonesData data = AssetDatabase.LoadAssetAtPath<UnityDragonBonesData>(assetPath);
					if(data.dragonBonesJSON!=null){
						dragonBonesJSONPaths.Add(AssetDatabase.GetAssetPath(data.dragonBonesJSON));
					}
				}
			}

			return dragonBonesJSONPaths;
		}


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
    }
}