using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
        if (Mouse.current.leftButton.isPressed)
        {
            // Get plane mouse hit
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            new Plane(Vector3.up, new Vector3(0,.5f,0)).Raycast(ray, out float enter);
            float3 hitPoint = ray.GetPoint(enter);
            
            
            ref var data = ref GetSingletonRW<BlockField>().ValueRW;
            var info = GetSingleton<BlockFieldInfo>();
            var offset = new float3(-info.gridDimensionSize, 0, -info.gridDimensionSize) * .5f;
            
            for (int y = 0; y < info.gridDimensionSize; y++)
            for (int x = 0; x < info.gridDimensionSize; x++)
            {
                var index = x + y * info.gridDimensionSize;
                if (math.distance(offset+new float3(x, .5f, y), hitPoint) < 4)
                    data.blockField[index] = BlockState.Grass;
            }
        }
    }
}