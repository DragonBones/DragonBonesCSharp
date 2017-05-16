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
			foreach(string skeletonPath in skeletonPaths){

				List<string> imgPaths = new List<string>();
				List<string> atlPaths = new List<string>();
				foreach(string atlasPath in atlasPaths){
					if(atlasPath.IndexOf(skeletonPath.Substring(0,skeletonPath.LastIndexOf("/")))==0){
						atlPaths.Add(atlasPath);
						imgPaths.Add(atlasPath.Substring(0,atlasPath.LastIndexOf(".json"))+".png");
					}
				}
				ProcessTextureAtlasData(atlPaths);
				ProcessTexture(imgPaths);
			}
		}

		static bool IsValidDragonBonesData (TextAsset asset) {
			if (asset.name.Contains("_ske")) return true;

			if (asset.text.IndexOf("\"armature\":") > 0)
			{
				return true;
			}
			return false;
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
				if(texImporter!=null){
					texImporter.textureType = TextureImporterType.Advanced;
					#if UNITY_5_5_OR_NEWER
					texImporter.textureCompression = TextureImporterCompression.Uncompressed;
					#else
					texImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
					#endif
					texImporter.mipmapEnabled = false;
					texImporter.alphaIsTransparency = true;
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
}