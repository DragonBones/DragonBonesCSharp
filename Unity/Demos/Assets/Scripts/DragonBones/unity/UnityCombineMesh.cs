using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
	[DisallowMultipleComponent]
	[ExecuteInEditMode,RequireComponent(typeof(UnityArmatureComponent))]
	public class UnityCombineMesh : MonoBehaviour {

		private UnityArmatureComponent _unityArmature;
		public UnityArmatureComponent unityArmature{
			get{ return _unityArmature; }
		}

		private Mesh _mesh;
		private MeshRenderer _meshRenderer;
		private MeshRenderer meshRenderer{
			get{
				if(_unityArmature.isUGUI){
					return null;
				}
				if(_meshRenderer==null) {
					_meshRenderer = gameObject.GetComponent<MeshRenderer>();
					if(_meshRenderer==null) {
						_meshRenderer = gameObject.AddComponent<MeshRenderer>();
						#if UNITY_EDITOR
						UnityEditorInternal.ComponentUtility.MoveComponentDown (this);
						#endif
					}
				}
				return _meshRenderer;
			}
		}
		private MeshFilter _meshFilter;
		private MeshFilter meshFilter{
			get{
				if(_unityArmature.isUGUI){
					return null;
				}
				if(_meshFilter==null) {
					_meshFilter = gameObject.GetComponent<MeshFilter>();
					if(_meshFilter==null) {
						_meshFilter = gameObject.AddComponent<MeshFilter>();
						#if UNITY_EDITOR
						UnityEditorInternal.ComponentUtility.MoveComponentDown (this);
						#endif
					}
				}
				return _meshFilter;
			}
		}


		void Start () {
			_unityArmature = GetComponent<UnityArmatureComponent>();
			if(_unityArmature.isUGUI){
				Destroy(gameObject);
				return;
			}
			_mesh = new Mesh();
			Init();
		}

		void Init(){
			if(_unityArmature.armature!=null){
				DisplayEnable(_unityArmature.armature,false);
			}
		}

		void DisplayEnable(Armature armature, bool flag){
			foreach(Slot slot in armature.GetSlots()){
				if(slot.childArmature!=null){
					DisplayEnable(slot.childArmature,flag);
				}else  if(slot.rawDisplay!=null){
					MeshRenderer mr = (slot.rawDisplay as GameObject).GetComponent<MeshRenderer>();
					if(mr) mr.enabled=flag;
				}
			}
		}

		void CollectMesh(Armature armature,List<CombineInstance> combines,Dictionary<Material,bool> matKV){
			List<Slot> slots = new List<Slot>(armature.GetSlots());
			slots.Sort(delegate(Slot x, Slot y) {
				return x._zOrder-y._zOrder;
			});
			foreach(Slot slot in slots){
				UnitySlot us = slot as UnitySlot;
				var currentTextureData = us.currentTextureAtlasData;

				#if UNITY_EDITOR
				if(currentTextureData!=null && currentTextureData.texture){
					matKV[currentTextureData.texture] = true;
				}
				#endif

				if(us!=null && us.mesh!=null){
					GameObject go = us.renderDisplay;
					if(go && go.activeSelf){
						CombineInstance com = new CombineInstance();
						com.mesh = us.mesh;
						com.transform = transform.worldToLocalMatrix * go.transform.localToWorldMatrix;
						combines.Add(com);
					}
				}

				if(slot.childArmature!=null){
					CollectMesh(slot.childArmature,combines,matKV);
				}
			}
		}

		void Update () {
			if(_unityArmature ==null || _unityArmature.armature==null) return;

			#if UNITY_EDITOR
			Init();
			#endif

			Dictionary<Material,bool> matKV = new Dictionary<Material,bool>();
			List<CombineInstance> combines = new List<CombineInstance>();
			CollectMesh(_unityArmature.armature,combines,matKV);

			_mesh.Clear();
			_mesh.CombineMeshes(combines.ToArray());
			_mesh.RecalculateBounds();
			meshFilter.sharedMesh = _mesh;

			#if UNITY_EDITOR
			List<Material> mats = new List<Material>();
			foreach(Material mat in matKV.Keys){
				mats.Add(mat);
			}
			meshRenderer.sharedMaterials = mats.ToArray();
			#endif
		}

		#if UNITY_EDITOR
		internal void Remove(){
			if(_meshFilter) {
				DestroyImmediate(_meshFilter.sharedMesh);
				DestroyImmediate(_meshFilter);
			}
			if(_meshRenderer) {
				_meshRenderer.sharedMaterials=new Material[0];
				DestroyImmediate(_meshRenderer);
			}
			if(_unityArmature.armature!=null)
				DisplayEnable(_unityArmature.armature,true);
			DestroyImmediate(this);
		}
		#endif
	}
}


#if UNITY_EDITOR
namespace DragonBones
{
	[CustomEditor(typeof(UnityCombineMesh))]
	class UnityCombineMeshEditor:Editor
	{
		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();
			if(!Application.isPlaying){
				UnityCombineMesh ucm = target as UnityCombineMesh;
				if (ucm.unityArmature!=null && ucm.unityArmature.armature!=null &&
					ucm.unityArmature.armature.parent==null && GUILayout.Button("Remove",GUILayout.Height(20)))
				{
					ucm.Remove();
				}
			}
		}
	}
}
#endif