using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

[UpdateAfter(typeof(VoxelInitSystem))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct VoxelSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    
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
            for (int y = 0; y < playInfo.gridDimensionSize; y++)
            {
                for (int x = 0; x < playInfo.gridDimensionSize; x++)
                {
                    var index = playArea.blockField[x + y * playInfo.gridDimensionSize];
                    if (index != BlockState.Clear)
                        cmd.SetComponent(entities[creationEntityCounter++], new Translation{Value = new float3(x, 1, y)});
                }
            }
        }
        cmd.Playback(state.EntityManager);
    }
}

struct BlockTag : IComponentData {}

[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct VoxelInitSystem : ISystem
{
    public void OnCreate(ref SystemState state) {}
    public void OnDestroy(ref SystemState state) {}
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!TryGetSingletonEntity<PlayAreaInfo>(out var playAreaEntity))
            return;
        
        if (QueryBuilder().WithAll<PlayAreaInit>().Build().CalculateEntityCount() == 1)
        {
            // Get PlayArea
            if (!HasSingleton<PlayAreaData>()) 
                state.EntityManager.AddComponent<PlayAreaData>(playAreaEntity);

            ref var playArea = ref GetSingletonRW<PlayAreaData>().ValueRW;
            var playAreaInfo = GetSingleton<PlayAreaInfo>();
            
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

partial struct VoxelUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        m_Random.InitState(123);
    }

    public void OnDestroy(ref SystemState state) { }

    Random m_Random;
    public void OnUpdate(ref SystemState state)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            ref var data = ref GetSingletonRW<PlayAreaData>().ValueRW;
            var index = m_Random.NextInt(data.blockField.Length);
            data.blockField[index] = data.blockField[index] == BlockState.Clear ? BlockState.Dirt : BlockState.Clear;
            foreach (var _ in Query<EnabledRefRW<PlayAreaDirty>>()) {}
        }
    }
}