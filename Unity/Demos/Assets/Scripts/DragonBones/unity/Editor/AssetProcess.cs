using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace DragonBones
{
	public class AssetProcess : AssetPostprocessor {
		[System.Serializable]
		struct SubTextureClass{
			public string name;
			public float x,y,width,height,frameX,frameY,frameWidth,frameHeight;
		}
		[System.Serializable]
		class TextureDataClass{
			public string name=null;
			public string imagePath=null;
			public int width=0,height=0;
			public List<SubTextureClass> SubTexture=null;
		}

		public static void OnPostprocessAllAssets(string[]imported,string[] deletedAssets,string[] movedAssets,string[]movedFromAssetPaths)  
		{  
			if (imported.Length == 0)
				return;

			var atlasPaths = new List<string>();
			var imagePaths = new List<string>();
			var skeletonPaths = new List<string>();

			foreach (string str in imported) {
				string extension = Path.GetExtension(str).ToLower();
				switch (extension) {
				case ".png":
					imagePaths.Add(str);
					break;
				case ".json":
					if (str.EndsWith("_tex.json", System.StringComparison.Ordinal))
						atlasPaths.Add(str);
					else if (IsValidDragonBonesData((TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset))))
						skeletonPaths.Add(str);
					else
						atlasPaths.Add(str);
					break;
				}
			} 
			if(skeletonPaths.Count==0) return;
			ProcessTexture(imagePaths);
			ProcessTextureAtlasData(atlasPaths);
		}

		static bool IsValidDragonBonesData (TextAsset asset) {
			if (asset.name.Contains("_ske")) return true;

			object obj = null;
			obj = MiniJSON.Json.Deserialize(asset.text);

			if (obj == null) {
				return false;
			}

			var root = obj as Dictionary<string, object>;
			if (root == null) {
				return false;
			}

			return root.ContainsKey("armature");
		}

		static void ProcessTextureAtlasData(List<string> atlasPaths){
			foreach(string path in atlasPaths){
				TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				if(ta){
					TextureDataClass tdc = JsonUtility.FromJson<TextureDataClass>(ta.text);
					if(tdc!=null && (tdc.width==0 || tdc.height==0)){
						//add width and height
						string imgPath = path.Substring(0,path.IndexOf(".json"))+".png";
						Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
						if(texture){
							tdc.width = texture.width;
							tdc.height = texture.height;
							//save
							string json = JsonUtility.ToJson(tdc);
							File.WriteAllText(path,json);
							EditorUtility.SetDirty(ta);
						}
					}
				}
			}
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}

		static void ProcessTexture(List<string> imagePaths){
			foreach(string texturePath in imagePaths){
				TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
				texImporter.textureType = TextureImporterType.Advanced;
				texImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				texImporter.mipmapEnabled = false;
				texImporter.alphaIsTransparency = false;
				texImporter.spriteImportMode = SpriteImportMode.None;
				texImporter.anisoLevel = 0;
				texImporter.wrapMode = TextureWrapMode.Clamp;
				texImporter.maxTextureSize = 2048;

				EditorUtility.SetDirty(texImporter);
				AssetDatabase.ImportAsset(texturePath);
				AssetDatabase.SaveAssets();
			}
		}
	}
}