﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[BurstCompile]
partial struct VoxelSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VoxelMaterialLookup>();
        //state.RequireForUpdate(QueryBuilder().WithAll<BlockField, BlockFieldInfo>().Build());
    }
    
    public void OnDestroy(ref SystemState state) { }

    bool m_RunWithChangeFilter;
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // if (!HasSingleton<VoxelMaterialLookup>() || QueryBuilder().WithAll<BlockField, BlockFieldInfo>().Build().IsEmpty)
        // {
        Debug.Log("Not there?");
        //     return;
        // }
        
        using var cmd = new EntityCommandBuffer(Allocator.Temp);
        if (m_RunWithChangeFilter)
        {
            foreach (var (playArea, playInfo) in Query<BlockField, BlockFieldInfo>().WithChangeFilter<BlockField>())
            {
                ReplaceVoxels(ref state, cmd, playArea, playInfo);
            }
        }
        else
        {
            foreach (var (playArea, playInfo) in Query<BlockField, BlockFieldInfo>())
            {
                ReplaceVoxels(ref state, cmd, playArea, playInfo);
            }
            m_RunWithChangeFilter = true;
        }
        cmd.Playback(state.EntityManager);
    }

    void ReplaceVoxels(ref SystemState state, EntityCommandBuffer cmd, BlockField playArea, BlockFieldInfo playInfo)
    {
        cmd.DestroyEntitiesForEntityQuery(QueryBuilder().WithAll<VoxelTag>().Build());

        var sum = 0;
        foreach (var blockState in playArea.blockField)
            sum += (int)blockState;

        var entities = CollectionHelper.CreateNativeArray<Entity>(sum, state.WorldUpdateAllocator);
        cmd.Instantiate(playInfo.voxelPrefab, entities);
        cmd.AddComponent<VoxelTag>(entities);
        var creationEntityCounter = 0;
        var offset = new float3(-playInfo.gridDimensionSize, 0, -playInfo.gridDimensionSize) * .5f;
        for (int y = 0; y < playInfo.gridDimensionSize; y++)
        {
            for (int x = 0; x < playInfo.gridDimensionSize; x++)
            {
                var blockState = playArea.blockField[x + y * playInfo.gridDimensionSize];
                if (blockState != BlockState.Clear)
                {
                    var entity = entities[creationEntityCounter++];
                    cmd.SetComponent(entity, new Translation { Value = offset + new float3(x, 1, y) });
                    cmd.SetComponent(entity, GetSingleton<VoxelMaterialLookup>().Value[blockState]);
                }
            }
        }
    }
}

struct VoxelTag : IComponentData {}