using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class ReplaceSlot : BaseDemo
{
    private static readonly string[] WEAPON_RIGHT_LIST = { "weapon_1004_r", "weapon_1004b_r", "weapon_1004c_r", "weapon_1004d_r", "weapon_1004e_r", "weapon_1004s_r" };

    private GameObject _logoReplaceTxt;

    private UnityArmatureComponent _armatureComp = null;
    private Slot _leftWeaponSlot = null;
    private Slot _rightWeaponSlot = null;

    private GameObject _sourceLogoDisplay = null;

    private int _leftWeaponIndex = -1;
    private int _rightWeaponIndex = -1;

    protected override void OnStart()
    {
        // Load Mecha Data
        UnityFactory.factory.LoadDragonBonesData("mecha_1004d_show/mecha_1004d_show_ske");
        UnityFactory.factory.LoadTextureAtlasData("mecha_1004d_show/mecha_1004d_show_tex");

        // Load Right Weapon Data
        UnityFactory.factory.LoadDragonBonesData("weapon_1004_show/weapon_1004_show_ske");
        UnityFactory.factory.LoadTextureAtlasData("weapon_1004_show/weapon_1004_show_tex");

        // Build Mecha Armature
        this._armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1004d");

        this._armatureComp.transform.localPosition = new Vector3(0.0f, -2.0f, 0.0f);

        this._armatureComp.animation.Play("idle");

        //
        this._leftWeaponSlot = this._armatureComp.armature.GetSlot("weapon_hand_l");
        this._rightWeaponSlot = this._armatureComp.armature.GetSlot("weapon_hand_r");

        this._sourceLogoDisplay = this._armatureComp.armature.GetSlot("logo").display as GameObject;

		// Set left weapon default value
		this._leftWeaponIndex = 0;
        // Set right weapon default value
        this._rightWeaponIndex = 0;
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        // Left weapon change
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            // Switch slot display index
            this._leftWeaponIndex++;
            if (this._leftWeaponIndex >= this._leftWeaponSlot.displayList.Count)
            {
                this._leftWeaponIndex = 0;
            }

			//
            this._leftWeaponSlot.displayIndex = this._leftWeaponIndex;
        }

        // Right weapon change
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            // Replace slot display
            this._rightWeaponIndex++;
            if (this._rightWeaponIndex >= WEAPON_RIGHT_LIST.Length)
            {
                this._rightWeaponIndex = 0;
            }

            var weaponDisplayName = WEAPON_RIGHT_LIST[this._rightWeaponIndex];
			//
            UnityFactory.factory.ReplaceSlotDisplay("weapon_1004_show", "weapon", "weapon_r", weaponDisplayName, this._rightWeaponSlot);
        }

        // Logon change
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var logoSlot = this._armatureComp.armature.GetSlot("logo") as UnitySlot;
            //
            if(logoSlot.renderDisplay.GetComponent<TextMesh>() != null)
            {
                logoSlot.display = this._sourceLogoDisplay;
            }
            else
            {
                logoSlot.display = this.GetTextLogo();
            }
        }
    }

    private GameObject GetTextLogo()
    {
        if(this._logoReplaceTxt == null)
        {           
            // Create 3d Text
            this._logoReplaceTxt = new GameObject("txt_logo");
            var textMesh = this._logoReplaceTxt.AddComponent<TextMesh>();
            textMesh.characterSize = 0.2f;
            textMesh.fontSize = 20;
            textMesh.text = "Core Element";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.richText = false;
        }

        return this._logoReplaceTxt;
    }
}
