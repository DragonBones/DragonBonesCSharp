using UnityEngine;
using DragonBones;

public class ReplaceDisplayTextureTest : MonoBehaviour 
{
	public string dragonBonesName,armatureName,slotName,displayName;
	[Space]
	public Texture2D replaceTex;
	public string replaceSlot;

	// Use this for initialization
	void Start ()
    {
		UnityArmatureComponent unityArmature = GetComponent<UnityArmatureComponent>();

        UnityFactory.factory.autoSearch = true;
		UnityFactory.factory.ReplaceSlotDisplay(
			dragonBonesName,armatureName,slotName,displayName,unityArmature.armature.GetSlot(replaceSlot),
			replaceTex,new Material(Shader.Find("Sprites/Default"))
		);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
