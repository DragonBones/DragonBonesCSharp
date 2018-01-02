using System.Collections;
using UnityEngine;
using DragonBones;

public class Performance : MonoBehaviour
{
    public UnityDragonBonesData dragonBoneData;

	public UnityEngine.UI.Text text;
    void Start()
    {
        UnityFactory.factory.LoadData(dragonBoneData);

        StartCoroutine(BuildArmatureComponent());
    }

    IEnumerator BuildArmatureComponent()
    {
        int lY = 20;
        int lX = 20;
		int index = 0;
        for (var y = 0; y < lY; ++y)
        {
            for (var x = 0; x < lX; ++x)
            {
                var position = new Vector3((x * 10.0f / lX - 5.0f) * 1.0f, (y * 10.0f / lY - 5.0f) * 1.0f, x + lX * y * 0.01f);

                var gameObject = new GameObject("mecha_1406");
                var armatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1406", "", "", "", gameObject);
				// armatureComponent.gameObject.AddComponent<CombineMesh>();
                armatureComponent.armature.cacheFrameRate = 24; // Cache animation.
                armatureComponent.animation.Play("walk");
                armatureComponent.transform.localPosition = position;
				armatureComponent.transform.localScale = Vector3.one * 0.5f;

                // armatureComponent.combineMesh = true;
				
                yield return new WaitForSecondsRealtime(0.1f);
				text.text = "Count:" + (++index);
            }
        }
    }
}
