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
			_mesh.MarkDynamic();
			if(_unityArmature.armature!=null){
				UpdateMesh();
			}
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

		void CollectMesh(Armature armature,List<List<CombineInstance>> combines,List<Material> mats){
			List<Slot> slots = (armature.eventDispatcher as UnityArmatureComponent).sortedSlots;
			foreach(Slot slot in slots){
				UnitySlot us = slot as UnitySlot;
				if(slot.childArmature!=null){
					CollectMesh(slot.childArmature,combines,mats);
				}

				var currentTextureData = us.currentTextureAtlasData;

				if(currentTextureData!=null && currentTextureData.texture){
					if(mats.Count==0 || mats[mats.Count-1] != currentTextureData.texture )
					{
						mats.Add(currentTextureData.texture);
					}
					if(combines.Count<mats.Count) {
						combines.Add(new List<CombineInstance>());
					}

					if(us!=null && us.mesh!=null){
						GameObject go = us.renderDisplay;
						if(go && go.activeSelf){
							CombineInstance com = new CombineInstance();
							com.mesh = us.mesh;
							com.transform = transform.worldToLocalMatrix * go.transform.localToWorldMatrix;
							combines[mats.Count-1].Add(com);
						}
					}
				}
			}
		}

		void UpdateMesh(){
			#if UNITY_EDITOR
			Init();
			#endif

			if(_mesh==null) return;
			_mesh.Clear();

			if(!_unityArmature.isUGUI){
				meshRenderer.sortingLayerName = _unityArmature.sortingLayerName;
				meshRenderer.sortingOrder = _unityArmature.sortingOrder;
			}

			List<Material> mats = new List<Material>();
			List<List<CombineInstance>> combines =  new List<List<CombineInstance>>();
			CollectMesh(_unityArmature.armature,combines,mats);
			int len = mats.Count;
			if(len>1){
				CombineInstance[] newCombines = new CombineInstance[len];
				for(int i=0;i<len;++i){
					Mesh mesh = new Mesh();
					mesh.CombineMeshes(combines[i].ToArray(),true,true);

					CombineInstance combine = new CombineInstance();
					combine.mesh = mesh;
					newCombines[i] = combine;
				}
				_mesh.CombineMeshes(newCombines,false,false);
			}else if(len==1){
				_mesh.CombineMeshes(combines[0].ToArray());
			}
			else
			{
				meshFilter.sharedMesh = _mesh;
				return;
			}
			_mesh.RecalculateBounds();
			meshFilter.sharedMesh = _mesh;
			meshRenderer.sharedMaterials = mats.ToArray();
		}

		void LateUpdate () {
			if(_unityArmature ==null || _unityArmature.armature==null) return;
			#if UNITY_EDITOR
			UpdateMesh();
			#else
			if(_unityArmature.animation.isPlaying){
				UpdateMesh();
			}
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
					ucm.unityArmature.armature.parent==null && GUILayout.Button("Remove Combine Mesh",GUILayout.Height(20)))
				{
					ucm.Remove();
					GUIUtility.ExitGUI();
				}
			}
		}
	}
}
#endif