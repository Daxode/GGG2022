using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Entities.SystemAPI;

partial struct CommandingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.AddComponent<Singleton>(state.SystemHandle);
        SetComponent(state.SystemHandle, Singleton.CreateInstance());
    }

    public void OnDestroy(ref SystemState state) => GetComponent<Singleton>(state.SystemHandle).Dispose();

    public void OnUpdate(ref SystemState state)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            // Get plane mouse hit
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            new Plane(Vector3.up, new Vector3(0, .5f, 0)).Raycast(ray, out var enter);
            float3 hitPoint = ray.GetPoint(enter);
            
            ref var data = ref GetSingletonRW<BlockField>().ValueRW;
            var info = GetSingleton<BlockFieldInfo>();

            var index = VoxelSpawnSystem.GetIndex(ref info, hitPoint);
            data.blockField[index] = BlockState.Grass;
        }
    }
    
    struct Singleton : IComponentData, IDisposable
    {
        NativeList<CommandQueue> m_Queues;

        Singleton(NativeList<CommandQueue> queues)
        {
            m_Queues = queues;
        }

        public static Singleton CreateInstance()
        {
            return new(new NativeList<CommandQueue>(2,Allocator.Persistent));
        }

        public void Dispose()
        {
            m_Queues.Dispose();
        }
    }
    
}
public struct CommandQueue
{
    UnsafeList<Command> m_Queue;
    FixedList64Bytes<int> m_Greenies;
}
public struct Command
{
    CommandFlag m_Flag;
    int m_TargetBlockIndex; // index in BlockField
}
[Flags]
public enum CommandFlag : byte
{
    None,
    DoAction = 1,
    DoPickup = 2,
}
