using System.Collections;
using UnityEngine;
using DragonBones;

namespace performanceTest
{
    public class PerformanceTest : MonoBehaviour
    {
        public UnityDragonBonesData dragonBoneData;

        void Start()
        {
            UnityFactory.factory.LoadData(dragonBoneData);

            StartCoroutine(BuildArmatureComponent());
        }

        IEnumerator BuildArmatureComponent()
        {
            int lY = 10;
            int lX = 10;
            for (var y = 0; y < lY; ++y)
            {
                for (var x = 0; x < lX; ++x)
                {
                    var position = new Vector3((x - lX * 0.5f) * 1.0f, (y - lY * 0.5f) * 1.0f, x + lX * y * 0.01f);

                    var armatureComponent = UnityFactory.factory.BuildArmatureComponent("DragonBoy");
                    armatureComponent.armature.cacheFrameRate = 24; // Cache animation.
                    armatureComponent.animation.Play("walk");
                    armatureComponent.transform.localPosition = position;

                    yield return new WaitForSecondsRealtime(1.0f);
                }
            }
        }
    }
}
