using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

[BurstCompile]
partial struct BlockUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        m_Random.InitState(123);
        state.RequireForUpdate<BlockField>();
    }

    public void OnDestroy(ref SystemState state) { }

    Random m_Random;
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log($"VoxelMaterialLookup:{HasSingleton<VoxelMaterialLookup>()}, S:{!QueryBuilder().WithAll<BlockField, BlockFieldInfo>().Build().IsEmpty}");
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ref var data = ref GetSingletonRW<BlockField>().ValueRW;
            var index = m_Random.NextInt(data.blockField.Length);
            data.blockField[index] = data.blockField[index] == BlockState.Clear ? BlockState.Dirt : BlockState.Clear;
            Debug.Log("Mouse pressed");
        }
    }
}