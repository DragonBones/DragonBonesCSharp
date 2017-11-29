using UnityEngine;
using System.Collections;
using DragonBones;

[RequireComponent(typeof(UnityArmatureComponent))]
public class ReplaceSlotDisplay : MonoBehaviour
{
    private int _displayIndex = -1;
    private string[] _replaceDisplays = {
        // Replace normal display.
        "display0002", "display0003", "display0004", "display0005", "display0006", "display0007", "display0008", "display0009", "display0010",
        // Replace mesh display.
        "meshA", "meshB", "meshC"
    };

    [SerializeField]
    private UnityDragonBonesData replaceData;

    private UnityArmatureComponent _armatureComponent = null;

    // Use this for initialization
    void Start()
    {
        UnityFactory.factory.LoadData(replaceData);

        _armatureComponent = this.GetComponent<UnityArmatureComponent>();
        //_armatureComponent.timeScale = 0.1f;
        _armatureComponent.animation.timeScale = 0.1f;
        _armatureComponent.animation.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _replaceDisplay();
        }
    }

    private void _replaceDisplay()
    {
        _displayIndex = (_displayIndex + 1) % _replaceDisplays.Length;

        var replaceDisplayName = _replaceDisplays[_displayIndex];
        if (replaceDisplayName.IndexOf("mesh") >= 0) // Replace mesh display.
        {
            switch (replaceDisplayName)
            {
                case "meshA":
                    // Normal to mesh.
                    UnityFactory.factory.ReplaceSlotDisplay(
                        "replace",
                        "MyMesh",
                        "meshA",
                        "weapon_1004_1",
                        _armatureComponent.armature.GetSlot("weapon")
                    );
                    // Replace mesh texture. 
                    UnityFactory.factory.ReplaceSlotDisplay(
                        "replace",
                        "MyDisplay",
                        "ball",
                        "display0002",
                        _armatureComponent.armature.GetSlot("mesh")
                    );
                    break;

                case "meshB":
                    // Normal to mesh.
                    UnityFactory.factory.ReplaceSlotDisplay(
                        "replace",
                        "MyMesh",
                        "meshB",
                        "weapon_1004_1",
                        _armatureComponent.armature.GetSlot("weapon")
                    );
                    // Replace mesh texture. 
                    UnityFactory.factory.ReplaceSlotDisplay(
                        "replace",
                        "MyDisplay",
                        "ball",
                        "display0003",
                        _armatureComponent.armature.GetSlot("mesh")
                    );
                    break;

                case "meshC":
                    // Back to normal.
                    UnityFactory.factory.ReplaceSlotDisplay(
                        "replace",
                        "MyMesh",
                        "mesh",
                        "weapon_1004_1",
                        _armatureComponent.armature.GetSlot("weapon")
                    );
                    // Replace mesh texture. 
                    UnityFactory.factory.ReplaceSlotDisplay(
                        "replace",
                        "MyDisplay",
                        "ball",
                        "display0005",
                        _armatureComponent.armature.GetSlot("mesh")
                    );
                    break;
            }
        }
        else // Replace normal display.
        {
            UnityFactory.factory.ReplaceSlotDisplay(
                "replace",
                "MyDisplay",
                "ball",
                replaceDisplayName,
                _armatureComponent.armature.GetSlot("ball")
            );
        }
    }
}
