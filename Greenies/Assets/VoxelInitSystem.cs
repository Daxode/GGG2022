using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct VoxelInitSystem : ISystem
{
    public void OnCreate(ref SystemState state) {}
    public void OnDestroy(ref SystemState state) {}
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<PlayAreaInfo>(out var playAreaEntity))
            return;
        
        if (SystemAPI.QueryBuilder().WithAll<PlayAreaInit>().Build().CalculateEntityCount() == 1)
        {
            // Get PlayArea
            if (!SystemAPI.HasSingleton<PlayAreaData>()) 
                state.EntityManager.AddComponent<PlayAreaData>(playAreaEntity);

            ref var playArea = ref SystemAPI.GetSingletonRW<PlayAreaData>().ValueRW;
            var playAreaInfo = SystemAPI.GetSingleton<PlayAreaInfo>();
            
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
                    playArea.blockField[index] = (BlockState)((int)BlockState.Stone * (int)math.round(t));
                }
            }
            
            // Set Init as done
            state.EntityManager.SetComponentEnabled<PlayAreaInit>(playAreaEntity, false);
        }
    }
}
