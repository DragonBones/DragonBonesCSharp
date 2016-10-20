using UnityEngine;
using DragonBones;

namespace performanceTest
{
    public class PerformanceTest : MonoBehaviour
    {
        void Start()
        {
            UnityFactory.factory.LoadDragonBonesData("BirdAndCentaur/skeleton");
            UnityFactory.factory.LoadTextureAtlasData("BirdAndCentaur/texture");

            int lY = 20;
            int lX = 40;

            for (var y = 0; y < lY; ++y)
            {
                for (var x = 0; x < lX; ++x)
                {
                    var armatureComponent = UnityFactory.factory.BuildArmatureComponent("centaur/charactor");
                    armatureComponent.armature.cacheFrameRate = 30; // Cache animation.
                    armatureComponent.animation.Play("run");
                    armatureComponent.transform.localPosition = new Vector3((x - lX * 0.5f) * 1.0f, (y - lY * 0.5f) * 1.0f, x + lX * y * 0.01f);
                }
            }
        }
    }
}
