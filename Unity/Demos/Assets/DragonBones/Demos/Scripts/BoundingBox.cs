using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class BoundingBox : BaseDemo
{
    public const float LINE_WIDTH = 4.0f;

    private readonly DragonBones.Point _intersectionPointA = new DragonBones.Point();
    private readonly DragonBones.Point _intersectionPointB = new DragonBones.Point();
    private readonly DragonBones.Point _normalRadians = new DragonBones.Point();
    private Vector3 _helpPointA = new Vector3();
    private Vector3 _helpPointB = new Vector3();

    private UnityArmatureComponent _armatureComp = null;
    private UnityArmatureComponent _boundingBoxComp = null;

    private UnityArmatureComponent _targetA;
    private UnityArmatureComponent _targetB;
    private GameObject _lineSlot;
    private GameObject _pointSlotA;
    private GameObject _pointSlotB;

    // Use this for initialization
    protected override void OnStart()
    {
        // Load Data
        UnityFactory.factory.LoadDragonBonesData("mecha_2903/mecha_2903_ske");
        UnityFactory.factory.LoadTextureAtlasData("mecha_2903/mecha_2903_tex");
        UnityFactory.factory.LoadDragonBonesData("bounding_box_tester/bounding_box_tester_ske");
        UnityFactory.factory.LoadTextureAtlasData("bounding_box_tester/bounding_box_tester_tex");
        // Build Armature
        this._armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_2903d");

        this._boundingBoxComp = UnityFactory.factory.BuildArmatureComponent("tester");
        //
        this._targetA = this._boundingBoxComp.armature.GetSlot("target_a").childArmature.proxy as UnityArmatureComponent;
        this._targetB = this._boundingBoxComp.armature.GetSlot("target_b").childArmature.proxy as UnityArmatureComponent;
        this._pointSlotA = this._boundingBoxComp.armature.GetSlot("point_a").display as GameObject;
        this._pointSlotB = this._boundingBoxComp.armature.GetSlot("point_b").display as GameObject;
        this._lineSlot = this._boundingBoxComp.armature.GetSlot("line").display as GameObject;
        // Open debug draw
        this._armatureComp.debugDraw = true;
        // Disable inheritAnimation
        this._targetA.armature.inheritAnimation = false;
        this._targetB.armature.inheritAnimation = false;
		//
        this._armatureComp.sortingOrder = 0;
        this._boundingBoxComp.sortingOrder = 1;

        this._pointSlotA.transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);
        this._pointSlotB.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);

        this._armatureComp.animation.Play("walk");
        this._boundingBoxComp.animation.Play("0");

        this._targetA.animation.Play("0");
        this._targetB.animation.Play("0");

        // Drag
        EnableDrag(this._targetA.gameObject);
        EnableDrag(this._targetB.gameObject);
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        //
        BoundingBoxCheck();
    }

    void BoundingBoxCheck()
    {
        // Transforms targetA position from world space to the armatureComp local space.
        this._helpPointA = this._armatureComp.transform.InverseTransformPoint(this._targetA.transform.position);
        this._helpPointB = this._armatureComp.transform.InverseTransformPoint(this._targetB.transform.position);
        // Check targetA position is inside a custom bounding box in a slot
        var containsTargetA = this._armatureComp.armature.ContainsPoint(this._helpPointA.x, this._helpPointA.y);
        var containsTargetB = this._armatureComp.armature.ContainsPoint(this._helpPointB.x, this._helpPointB.y);
        // Check whether a specific segment intersects a custom bounding box for a slot in the armature.
        var intersectsSlots = this._armatureComp.armature.IntersectsSegment(this._helpPointA.x, this._helpPointA.y,
                                                                            this._helpPointB.x, this._helpPointB.y,
                                                                            this._intersectionPointA, _intersectionPointB, _normalRadians);

        // if it hits, play 1, otherwise, play 0
        var animationName = containsTargetA != null ? "1" : "0";
        if (this._targetA.animation.lastAnimationName != animationName)
        {
            this._targetA.animation.FadeIn(animationName, 0.2f).resetToPose = false; ;
        }
        // if it hits, play 1, otherwise, play 0
        animationName = containsTargetB != null ? "1" : "0";
        if (this._targetB.animation.lastAnimationName != animationName)
        {
            this._targetB.animation.FadeIn(animationName, 0.2f).resetToPose = false; ;
        }
        // if it hits, play 1, otherwise, play 0
        animationName = intersectsSlots != null ? "1" : "0";
        if (this._boundingBoxComp.animation.lastAnimationName != animationName)
        {
            this._boundingBoxComp.animation.FadeIn(animationName, 0.2f).resetToPose = false;
        }

        var targetPointA = this._targetA.transform.localPosition;
        var targetPointB = this._targetB.transform.localPosition;
        // Set the line transfrom
        var dis = targetPointB - targetPointA;
        var localScaleX = dis.magnitude / LINE_WIDTH;
        var linePoint = targetPointA + dis.normalized * LINE_WIDTH * localScaleX / 2.0f;
        this._lineSlot.transform.localPosition = new Vector3(linePoint.x, linePoint.y, this._lineSlot.transform.localPosition.z);
        this._lineSlot.transform.localScale = new Vector3(localScaleX, 1.0f, 1.0f);
        this._lineSlot.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Atan2(dis.y, dis.x) * Mathf.Rad2Deg);

        if (intersectsSlots != null)
        {
            // Transforms intersection position from the armatureComp local space to world space.
            this._helpPointA = this._armatureComp.transform.TransformPoint(this._intersectionPointA.x, this._intersectionPointA.y, 0.0f);
            this._helpPointB = this._armatureComp.transform.TransformPoint(this._intersectionPointB.x, this._intersectionPointB.y, 0.0f);

            // Transform intersection position from world space to the boundingBoxComp local space.
            this._helpPointA = this._boundingBoxComp.transform.InverseTransformPoint(this._helpPointA);
            this._helpPointB = this._boundingBoxComp.transform.InverseTransformPoint(this._helpPointB);

            this._helpPointA.z = this._pointSlotA.transform.localPosition.z;
            this._helpPointB.z = this._pointSlotB.transform.localPosition.z;

            this._pointSlotA.SetActive(true);
            this._pointSlotB.SetActive(true);

            this._pointSlotA.transform.localPosition = this._helpPointA;
            this._pointSlotB.transform.localPosition = this._helpPointB;

            this._pointSlotA.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this._normalRadians.x * Mathf.Rad2Deg);
            this._pointSlotB.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this._normalRadians.y * Mathf.Rad2Deg);
        }
        else
        {
            this._pointSlotA.SetActive(false);
            this._pointSlotB.SetActive(false);
        }
    }
}
