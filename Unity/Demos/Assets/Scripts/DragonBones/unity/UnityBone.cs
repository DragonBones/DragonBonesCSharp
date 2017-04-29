using UnityEngine;
using System.Collections;

namespace DragonBones
{
	[DisallowMultipleComponent]
	public class UnityBone :MonoBehaviour  {
		
		private static Vector3 _helpVector3 = new Vector3();
		internal UnityArmatureComponent _proxy;
		internal Bone _bone;

		[SerializeField]
		private GameObject _parent = null;

		/**
		 * 获取父骨骼
		 * 
		 */ 
		public GameObject GetParentGameObject(){
			if(_parent) return _parent;
			if(_bone!=null && _bone.parent!=null){
				for(int i=0;i<transform.parent.childCount;++i){
					UnityEngine.Transform child = transform.parent.GetChild(i);
					if(child.name.Equals(_bone.parent.name)){
						_parent = child.gameObject;
					}
				}
			}
			return _parent;
		}

		internal void _Update(){
			if(_bone!=null && _proxy!=null && _proxy.armature!=null)
			{
				Armature armature = _proxy.armature;

				var flipX = armature.flipX;
				var flipY = armature.flipY;
				var scaleX = flipX ? -_bone.global.scaleX : _bone.global.scaleX;
				var scaleY = flipY ? -_bone.global.scaleY : _bone.global.scaleY;

				_helpVector3.x = _bone.globalTransformMatrix.tx;
				_helpVector3.y = -_bone.globalTransformMatrix.ty;

				if (flipX)
				{
					_helpVector3.x = -_helpVector3.x;
				}
				if (flipY)
				{
					_helpVector3.y = -_helpVector3.y;
				}
				_helpVector3.z = 0f;
				transform.localPosition = _helpVector3;

				if (scaleY >= 0.0f )
				{
					_helpVector3.x = 0.0f;
				}
				else
				{
					_helpVector3.x = 180.0f;
				}

				if (scaleX >= 0.0f)
				{
					_helpVector3.y = 0.0f;
				}
				else
				{
					_helpVector3.y = 180.0f;
				}

				transform.localEulerAngles = _helpVector3;

				_helpVector3.x = scaleX >= 0.0f ? scaleX : -scaleX;
				_helpVector3.y = scaleY >= 0.0f ? scaleY : -scaleY;
				_helpVector3.z = 1f;

				transform.localScale = _helpVector3;
			}
		}
	}
}