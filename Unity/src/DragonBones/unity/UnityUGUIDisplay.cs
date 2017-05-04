using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace DragonBones
{
	[DisallowMultipleComponent]
	[ExecuteInEditMode,RequireComponent(typeof(CanvasRenderer), typeof(RectTransform))]
	public class UnityUGUIDisplay : MaskableGraphic {
		[HideInInspector]
		public Mesh sharedMesh;

		private Texture _Texture;
		public override Texture mainTexture {
			get { 
				return _Texture == null ? material.mainTexture : _Texture;
			}
		}

		/// <summary>
		/// Texture to be used.
		/// </summary>
		public Texture texture
		{
			get
			{
				return _Texture;
			}
			set
			{
				if (_Texture == value)
					return;
				_Texture = value;
				SetMaterialDirty();
			}
		}

		protected override void OnPopulateMesh (VertexHelper vh)
		{
			vh.Clear();
		}

		public override void Rebuild (CanvasUpdate update) {
			base.Rebuild(update);
			if (canvasRenderer.cull) return;
			if (update == CanvasUpdate.PreRender){
				canvasRenderer.SetMesh(sharedMesh);
			}
		}

		void Update(){
			canvasRenderer.SetMesh(sharedMesh);
		}
	}
}