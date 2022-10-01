using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

partial struct BlockUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        m_Random.InitState(123);
    }

    public void OnDestroy(ref SystemState state) { }

    Random m_Random;
    public void OnUpdate(ref SystemState state)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ref var data = ref GetSingletonRW<PlayAreaData>().ValueRW;
            var index = m_Random.NextInt(data.blockField.Length);
            data.blockField[index] = data.blockField[index] == BlockState.Clear ? BlockState.Dirt : BlockState.Clear;
            foreach (var _ in Query<EnabledRefRW<PlayAreaDirty>>()) {}
        }
    }
}

[UpdateAfter(typeof(VoxelSpawnerSystem))]
partial struct ChangeColorSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (color, translation) in Query<RefRW<URPMaterialPropertyBaseColor>, RefRO<Translation>>())
        {
            var hueColor = Color.HSVToRGB((float)math.frac(math.csum(translation.ValueRO.Value) * 0.01f+SystemAPI.Time.ElapsedTime), 1, 1);
            color.ValueRW.Value = UnsafeUtility.As<Color, float4>(ref hueColor);
        }
    }
}