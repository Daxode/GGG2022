using Unity.Entities;
using Unity.Scenes;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Splines;
using Unity.Collections;
using Hash128 = Unity.Entities.Hash128;

[TemporaryBakingType]
class RailBaking : IComponentData
{
    public Entity railPrefab;
    public SplineContainer splineContainer;
    //public Hash128 sceneGUID;
}


[RequireComponent(typeof(SplineContainer))]
public class RailBaker : MonoBehaviour
{
    [AssetReferenceUILabelRestriction("railing")]
    public AssetReference railPrefab;

    class Baker : Baker<RailBaker>
    {
        public override void Bake(RailBaker authoring)
        {
            if(authoring.railPrefab == null)
                return;

            if (authoring.railPrefab.IsValid())
                authoring.railPrefab.ReleaseAsset();
            
            var railPrefabLoadHandle = authoring.railPrefab.LoadAssetAsync<GameObject>();
            if (railPrefabLoadHandle.IsValid())
            {
                var railPrefab = DependsOn(railPrefabLoadHandle.WaitForCompletion());
                AddComponentObject(new RailBaking
                {
                    railPrefab = GetEntity(railPrefab),
                    splineContainer = GetComponent<SplineContainer>(),
                    //sceneGUID = AssetDatabase.GUIDFromAssetPath(authoring.gameObject.scene.path)
                });
            }
        }
    }
}


[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
partial struct RailBakingSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var translations = SystemAPI.GetComponentLookup<Translation>();
        foreach (var railBaking in SystemAPI.Query<RailBaking>())
        {
            for (float t = 0; t <= 1f; t+=0.01f)
            {
                railBaking.splineContainer.Spline.Evaluate(t, out var pos, out var tan, out var up);
                var e = state.EntityManager.CreateEntity(typeof(Translation));
                state.EntityManager.AddSharedComponent(e, new SceneSection{Section = (int)(t*1000)});
                translations[e] = new Translation{Value = pos};
            }
        }
    }
}

