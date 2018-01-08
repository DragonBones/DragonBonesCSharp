using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DragonBones;

public class ReplaceSkin : BaseDemo
{
    private UnityArmatureComponent _bodyArmatureComp = null;
    //
    private int _replaceSuitIndex = 0;    
    private Dictionary<string, List<string>> _suitConfigs = new Dictionary<string, List<string>>();
    private readonly List<string> _replaceSuitParts = new List<string>();

    void Awake()
    {
        // Clear
        this._bodyArmatureComp = null;
        this._replaceSuitIndex = 0;

        this._suitConfigs.Clear();
        this._replaceSuitParts.Clear();

        // Init suit1
        var suit1 = new List<string>();
        suit1.Add("2010600a");
        suit1.Add("2010600a_1");
        suit1.Add("20208003");
        suit1.Add("20208003_1");
        suit1.Add("20208003_2");
        suit1.Add("20208003_3");
        suit1.Add("20405006");
        suit1.Add("20509005");
        suit1.Add("20703016");
        suit1.Add("20703016_1");
        suit1.Add("2080100c");
        suit1.Add("2080100e");
        suit1.Add("2080100e_1");
        suit1.Add("20803005");
        suit1.Add("2080500b");
        suit1.Add("2080500b_1");
        this._suitConfigs.Add("suit1", suit1);

        // Init suit2
        var suit2 = new List<string>();
        suit2.Add("20106010");
        suit2.Add("20106010_1");
        suit2.Add("20208006");
        suit2.Add("20208006_1");
        suit2.Add("20208006_2");
        suit2.Add("20208006_3");
        suit2.Add("2040600b");
        suit2.Add("2040600b_1");
        suit2.Add("20509007");
        suit2.Add("20703020");
        suit2.Add("20703020_1");
        suit2.Add("2080b003");
        suit2.Add("20801015");
        this._suitConfigs.Add("suit2", suit2);
    }

    protected override void OnStart()
    {
        // Load body
        this._LoadData("you_xin/body/body_ske", "you_xin/body/body_tex");
        // Load suits
        var dragonBonesJSONPath = "";
        var textureAtlasJSONPath = "";
        foreach (var suitName in this._suitConfigs.Keys)
        {
            var parts = this._suitConfigs[suitName];
            foreach (var partArmatureName in parts)
            {
                //you_xin/suit1/2010600a/2010600a_ske
                dragonBonesJSONPath = "you_xin/" + suitName + "/" + partArmatureName + "/" + partArmatureName + "_ske";
                //you_xin/suit1/2010600a/2010600a_tex
                textureAtlasJSONPath = "you_xin/" + suitName + "/" + partArmatureName + "/" + partArmatureName + "_tex";
                //
                this._LoadData(dragonBonesJSONPath, textureAtlasJSONPath);
            }
        }

        // Build body
        this._bodyArmatureComp = UnityFactory.factory.BuildArmatureComponent("body");
        //
        this._bodyArmatureComp.CloseCombineMeshs();
        //
        this._bodyArmatureComp.transform.localScale = new Vector3(0.5f, 0.5f, 1.0f);
        this._bodyArmatureComp.transform.localPosition = new Vector3(0.0f, -4.0f, 0.0f);

        // Add loop complete event listener
        this._bodyArmatureComp.AddDBEventListener(EventObject.LOOP_COMPLETE, this._OnFrameEventHandler);

        // Play idle animation
        this._bodyArmatureComp.animation.Play("idle", 0);

        // Init the first suit
        var initSuitIndex = 0;
        var suitNames = this._suitConfigs.Keys.ToArray();
        var initSuitParts = this._suitConfigs[suitNames[initSuitIndex]];
        foreach (var part in initSuitParts)
        {
            var skin = UnityFactory.factory.GetArmatureData(part);

            UnityFactory.factory.ReplaceSkin(this._bodyArmatureComp.armature, skin.defaultSkin);
        }
        // Replace the sencond suit
        this._replaceSuitIndex = ++initSuitIndex;
        this._replaceSuitParts.AddRange(this._suitConfigs[suitNames[initSuitIndex]]);
    }

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.RandomReplaceSkin();
        }
    }

    //
    private void _LoadData(string dragonBonesJSONPath, string textureAtlasJSONPath)
    {
        UnityFactory.factory.LoadDragonBonesData(dragonBonesJSONPath);
        UnityFactory.factory.LoadTextureAtlasData(textureAtlasJSONPath);
    }
    //
    private void _OnFrameEventHandler(string type, EventObject eventObject)
    {
        if (type == EventObject.LOOP_COMPLETE)
        {
            // Random animation index
            var randomAniIndex = Random.Range(0, this._bodyArmatureComp.animation.animationNames.Count);
            //
            var animationName = this._bodyArmatureComp.animation.animationNames[randomAniIndex];
            // Play animation
            this._bodyArmatureComp.animation.FadeIn(animationName, 0.3f, 0);
        }
    }
    //
    void RandomReplaceSkin()
    {
        // This suit has been replaced, Next suit
        if (this._replaceSuitParts.Count == 0)
        {
            this._replaceSuitIndex++;
            var suitNames = this._suitConfigs.Keys.ToArray();
            if (this._replaceSuitIndex >= suitNames.Length)
            {
                this._replaceSuitIndex = 0;
            }
            // Refill the unset parits
            this._replaceSuitParts.AddRange(this._suitConfigs[suitNames[this._replaceSuitIndex]]);
        }
        // Random one part in this suit
        var randomPartIndex = Random.Range(0, this._replaceSuitParts.Count);
        //
        var partArmatureName = this._replaceSuitParts[randomPartIndex];
        //
        ArmatureData partArmatureData = UnityFactory.factory.GetArmatureData(partArmatureName);
        // Replace skin
        UnityFactory.factory.ReplaceSkin(this._bodyArmatureComp.armature, partArmatureData.defaultSkin);
        // Remove has been replaced
        this._replaceSuitParts.RemoveAt(randomPartIndex);
    }
}
