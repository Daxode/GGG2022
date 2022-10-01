using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
partial struct BlockFieldInitSystem : ISystem
{
    public void OnCreate(ref SystemState state) {}
    public void OnDestroy(ref SystemState state) {}
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!TryGetSingletonEntity<BlockFieldInfo>(out var playAreaEntity))
            return;
        
        if (!QueryBuilder().WithAll<BlockFieldInit>().Build().IsEmpty)
        {
            // Get PlayArea
            if (!HasSingleton<BlockField>()) 
                state.EntityManager.AddComponent<BlockField>(playAreaEntity);

            ref var playArea = ref GetSingletonRW<BlockField>().ValueRW;
            var playAreaInfo = GetSingleton<BlockFieldInfo>();
            
            // Dispose and Create
            if (playArea.blockField.IsCreated)
                playArea.blockField.Dispose();
            playArea.blockField = new NativeArray<BlockState>(playAreaInfo.gridDimensionSize * playAreaInfo.gridDimensionSize, Allocator.Persistent);
            
            // Set landscape
            for (int y = 0; y < playAreaInfo.gridDimensionSize; y++)
            {
                for (int x = 0; x < playAreaInfo.gridDimensionSize; x++)
                {
                    var index = x + y * playAreaInfo.gridDimensionSize;
                    var t = 0.5+noise.cnoise(.1f * new float3(x, y, 0))*0.5;
                    playArea.blockField[index] = (BlockState)((int)BlockState.DryStone * (int)math.round(t));
                }
            }
            
            // Set Init as done
            state.EntityManager.SetComponentEnabled<BlockFieldInit>(playAreaEntity, false);
        }
    }
}

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
partial struct BlockFieldDestroySystem : ISystem
{
    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var queryToClean = QueryBuilder().WithAll<BlockField>().WithNone<BlockFieldInfo>().Build();
        if (queryToClean.TryGetSingleton(out BlockField playAreaData))
            playAreaData.blockField.Dispose();
        state.EntityManager.RemoveComponent<BlockField>(queryToClean);
    }
}