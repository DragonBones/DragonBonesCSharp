using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System.IO;

namespace DragonBones
{
	[InitializeOnLoad]
	public class DragonBonesIcons : Editor {

		static Texture2D textureBone,textureArmature,textureImg,textureMesh,textureIk;
		static string editorPath="";
		static string editorGUIPath=  "";
		static bool isInited = false;
		static DragonBonesIcons(){
			Initialize();
		}
		static void Initialize(){
			if(isInited) return;

			DirectoryInfo rootDir = new DirectoryInfo(Application.dataPath);
			FileInfo[] files = rootDir.GetFiles("DragonBonesIcons.cs", SearchOption.AllDirectories); 
			editorPath = Path.GetDirectoryName(files[0].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
			editorGUIPath = editorPath + "/GUI"; 

			textureBone = AssetDatabase.LoadAssetAtPath<Texture2D>(editorGUIPath+"/icon-bone.png");
			textureIk = AssetDatabase.LoadAssetAtPath<Texture2D>(editorGUIPath+"/icon-ik.png");
			textureArmature = AssetDatabase.LoadAssetAtPath<Texture2D>(editorGUIPath+"/icon-skeleton.png");
			textureImg = AssetDatabase.LoadAssetAtPath<Texture2D>(editorGUIPath+"/icon-image.png");
			textureMesh = AssetDatabase.LoadAssetAtPath<Texture2D>(editorGUIPath+"/icon-mesh.png");

			EditorApplication.hierarchyWindowItemOnGUI -= HierarchyIconsOnGUI;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyIconsOnGUI;
			isInited = true;
		}

		static void HierarchyIconsOnGUI (int instanceId, Rect selectionRect) {
			GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceId); 
			if(!go) return;

			Rect rect = new Rect(selectionRect.x-25f, selectionRect.y+2, 15f, 15f);

			if(go.GetComponent<UnityArmatureComponent>())
			{
				rect.x =  selectionRect.x+ selectionRect.width - 15f;
				GUI.Label(rect,textureArmature);
				return;
			}

			UnityUGUIDisplay ugui = go.GetComponent<UnityUGUIDisplay>();
			if(ugui && ugui.sharedMesh)
			{
				if(ugui.sharedMesh.vertexCount==4){
					GUI.Label(rect,textureImg);
				}else{
					GUI.Label(rect,textureMesh);
				}
				return;
			}

			MeshFilter mf = go.GetComponent<MeshFilter>();
			if(mf && mf.sharedMesh && 
				mf.transform.parent!=null && 
				mf.transform.parent.parent!=null &&
				mf.transform.parent.parent.GetComponent<UnityArmatureComponent>()!=null)
			{
				if(mf.sharedMesh.vertexCount==4){
					GUI.Label(rect,textureImg);
				}else{
					GUI.Label(rect,textureMesh);
				}
				return;
			}

			UnityBone ubone = go.GetComponent<UnityBone>();
			if(ubone){
				Texture2D icon = textureBone;
				Bone bone = ubone.bone;
				if(bone!=null && bone.ik!=null){
					icon = textureIk;
				}
				GUI.Label(rect,icon);
				return;
			}
		}
	}

}