using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class ReplaceSlotDisplay : BaseDemo
{
    private static readonly string[] WEAPON_RIGHT_LIST = { "weapon_1004_r", "weapon_1004b_r", "weapon_1004c_r", "weapon_1004d_r", "weapon_1004e_r" };

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
        //
        this._armatureComp.CloseCombineMeshs();

        this._armatureComp.animation.Play("idle");

        this._armatureComp.transform.localPosition = new Vector3(0.0f, -2.0f, 0.0f);

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
        if (Input.GetMouseButtonDown(0))
        {
            var leftSide = 0.0f + Screen.width / 2.0f - Screen.width / 6.0f;
            var rightSide = Screen.width / 2.0f + Screen.width / 6.0f;
            var isMiddle = Input.mousePosition.x < rightSide && Input.mousePosition.x > leftSide;
            var isTouchRight = Input.mousePosition.x > rightSide;
            //
            if (isMiddle)
            {
                this._ReplaceDisplay(0);
            }
            else
            {
                this._ReplaceDisplay(isTouchRight ? 1 : -1);
            }
        }
    }

    private void _ReplaceDisplay(int type)
    {
        switch (type)
        {
            case 1:
                {
                    // Switch slot display index
                    this._leftWeaponIndex++;
                    this._leftWeaponIndex %= this._leftWeaponSlot.displayList.Count;
                    this._leftWeaponSlot.displayIndex = this._leftWeaponIndex;
                }
                break;
            case -1:
                {
                    // Replace slot display
                    this._rightWeaponIndex++;
                    this._rightWeaponIndex %= WEAPON_RIGHT_LIST.Length;
                    var weaponDisplayName = WEAPON_RIGHT_LIST[this._rightWeaponIndex];
                    //
                    UnityFactory.factory.ReplaceSlotDisplay("weapon_1004_show", "weapon", "weapon_r", weaponDisplayName, this._rightWeaponSlot);
                }
                break;
            default:
                {
                    var logoSlot = this._armatureComp.armature.GetSlot("logo") as UnitySlot;
                    //
                    if (logoSlot.renderDisplay.GetComponent<TextMesh>() != null)
                    {
                        logoSlot.display = this._sourceLogoDisplay;
                    }
                    else
                    {
                        logoSlot.display = this._GetTextLogo();
                    }
                }
                break;
        }

    }

    private GameObject _GetTextLogo()
    {
        if (this._logoReplaceTxt == null)
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
