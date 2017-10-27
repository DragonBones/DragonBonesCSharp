using UnityEngine;
using UnityEngine.UI;

namespace DragonBones
{
    [DisallowMultipleComponent]
	[ExecuteInEditMode,RequireComponent(typeof(CanvasRenderer), typeof(RectTransform))]
	public class UnityUGUIDisplay : MaskableGraphic
    {
		[HideInInspector]
		public Mesh sharedMesh;

		private Texture _texture;
		public override Texture mainTexture
        {
			get { return _texture == null ? material.mainTexture : _texture; }
		}

		/// <summary>
		/// Texture to be used.
		/// </summary>
		public Texture texture
		{
			get	{ return _texture; }
			set
			{
                if (_texture == value)
                {
                    return;
                }
				
                _texture = value;
				SetMaterialDirty();
			}
		}

		protected override void OnPopulateMesh (VertexHelper vh)
		{
			vh.Clear();
		}

		public override void Rebuild (CanvasUpdate update)
        {
			base.Rebuild(update);
            if (canvasRenderer.cull)
            {
                return;
            }

			if (update == CanvasUpdate.PreRender)
            {
				canvasRenderer.SetMesh(sharedMesh);
			}
		}

		void Update()
        {
			canvasRenderer.SetMesh(sharedMesh);
		}
	}
}