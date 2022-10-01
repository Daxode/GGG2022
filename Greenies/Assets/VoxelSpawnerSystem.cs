using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[UpdateAfter(typeof(VoxelInitSystem))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct VoxelSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state) {}
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        using var cmd = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (playArea, playInfo) in Query<PlayAreaData, PlayAreaInfo>().WithChangeFilter<PlayAreaDirty>())
        {
            cmd.DestroyEntitiesForEntityQuery(QueryBuilder().WithAll<BlockTag>().Build());

            var sum = 0;
            foreach (var blockState in playArea.blockField)
                sum += (int)blockState;
            
            var entities = CollectionHelper.CreateNativeArray<Entity>(sum, state.WorldUpdateAllocator);
            cmd.Instantiate(playInfo.voxelPrefab, entities);
            cmd.AddComponent<BlockTag>(entities);
            var creationEntityCounter = 0;
            var offset = new float3(-playInfo.gridDimensionSize, 0, -playInfo.gridDimensionSize) * .5f;
            for (int y = 0; y < playInfo.gridDimensionSize; y++)
            {
                for (int x = 0; x < playInfo.gridDimensionSize; x++)
                {
                    var index = playArea.blockField[x + y * playInfo.gridDimensionSize];
                    if (index != BlockState.Clear)
                    {
                        var i = creationEntityCounter++;
                        cmd.SetComponent(entities[i], new Translation{Value = offset + new float3(x, 1, y)});
                        //cmd.SetComponent();
                    }
                }
            }
        }
        cmd.Playback(state.EntityManager);
    }
}

struct BlockTag : IComponentData {}

[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct VoxelDestroySystem : ISystem
{
    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var queryToClean = QueryBuilder().WithAll<PlayAreaData>().WithNone<PlayAreaInfo>().Build();
        if (queryToClean.TryGetSingleton(out PlayAreaData playAreaData))
            playAreaData.blockField.Dispose();
        state.EntityManager.RemoveComponent<PlayAreaData>(queryToClean);
    }
}