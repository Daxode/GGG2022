using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct CreateMaterialLookupSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state) => state.RequireForUpdate<VoxelColors>();
    public void OnDestroy(ref SystemState state) {}
    public void OnUpdate(ref SystemState state) {}
    
    public void OnStartRunning(ref SystemState state)
    {
        state.EntityManager.AddComponent<VoxelMaterialLookup>(state.SystemHandle);
        var voxelMaterialLookup = new VoxelMaterialLookup {
            Value = new NativeHashMap<BlockStateHack, URPMaterialPropertyBaseColor>(3, Allocator.Persistent)
        };
        var voxelColors = GetSingleton<VoxelColors>();
        voxelMaterialLookup.Value[BlockState.Dirt] = voxelColors.colorDirt;
        voxelMaterialLookup.Value[BlockState.MachineHydrogenGenerator] = voxelColors.colorStone;
        voxelMaterialLookup.Value[BlockState.MachineH20Generator] = voxelColors.colorDryStone;
        voxelMaterialLookup.Value[BlockState.Stone] = voxelColors.colorSand;
        voxelMaterialLookup.Value[BlockState.Grass] = voxelColors.colorGrass;
        SetComponent(state.SystemHandle, voxelMaterialLookup);
    }

    public void OnStopRunning(ref SystemState state)
    {
        var voxelMaterialLookup = GetComponent<VoxelMaterialLookup>(state.SystemHandle).Value;
        if (voxelMaterialLookup.IsCreated)
            voxelMaterialLookup.Dispose();
        state.EntityManager.RemoveComponent<VoxelMaterialLookup>(state.SystemHandle);
    }
}

public struct VoxelMaterialLookup : IComponentData
{
    public NativeHashMap<BlockStateHack, URPMaterialPropertyBaseColor> Value;
}


public class VoxelColorsAuthor : MonoBehaviour
{
    [SerializeField] Color colorSand;
    [SerializeField] Color colorDryStone;
    [SerializeField] Color colorStone;
    [SerializeField] Color colorDirt;
    [SerializeField] Color colorGrass;
    
    class Baker : Baker<VoxelColorsAuthor>
    {
        public override void Bake(VoxelColorsAuthor author)
        {
            AddComponent(new VoxelColors
            {
                colorStone = new URPMaterialPropertyBaseColor{Value = author.colorStone.AsFloat4()},
                colorDirt = new URPMaterialPropertyBaseColor{Value = author.colorDirt.AsFloat4()},
                colorGrass = new URPMaterialPropertyBaseColor{Value = author.colorGrass.AsFloat4()},
                colorSand = new URPMaterialPropertyBaseColor{Value = author.colorSand.AsFloat4()},
                colorDryStone = new URPMaterialPropertyBaseColor{Value = author.colorDryStone.AsFloat4()},
            });
        }
    }
}

public static class ColorHelpers
{
    public static float4 AsFloat4(ref this Color color) => UnsafeUtility.As<Color, float4>(ref color);
}

public struct VoxelColors : IComponentData
{
    public URPMaterialPropertyBaseColor colorSand;
    public URPMaterialPropertyBaseColor colorDryStone;
    public URPMaterialPropertyBaseColor colorStone;
    public URPMaterialPropertyBaseColor colorDirt;
    public URPMaterialPropertyBaseColor colorGrass;
}