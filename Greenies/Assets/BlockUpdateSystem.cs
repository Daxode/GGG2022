﻿using Unity.Burst;
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
    }

    public void OnDestroy(ref SystemState state) { }

    Random m_Random;
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ref var data = ref GetSingletonRW<BlockField>().ValueRW;
            var index = m_Random.NextInt(data.blockField.Length);
            data.blockField[index] = data.blockField[index] == BlockState.Clear ? BlockState.Dirt : BlockState.Clear;
        }
    }
}