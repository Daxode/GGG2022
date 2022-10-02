using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Serialization;
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
        voxelMaterialLookup.Value[BlockState.MachineHydrogenGenerator] = voxelColors.colorMachineHydrogenGenerator;
        voxelMaterialLookup.Value[BlockState.MachineH20Generator] = voxelColors.colorMachineH20Generator;
        voxelMaterialLookup.Value[BlockState.Stone] = voxelColors.colorStone;
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
    [FormerlySerializedAs("colorSand")]
    [SerializeField] Color colorStone;
    [FormerlySerializedAs("colorDryStone")]
    [SerializeField] Color colorMachineH20Generator;
    [FormerlySerializedAs("colorStone")]
    [SerializeField] Color colorMachineHydrogenGenerator;
    [SerializeField] Color colorDirt;
    [SerializeField] Color colorGrass;
    
    class Baker : Baker<VoxelColorsAuthor>
    {
        public override void Bake(VoxelColorsAuthor author)
        {
            AddComponent(new VoxelColors
            {
                colorMachineHydrogenGenerator = new URPMaterialPropertyBaseColor{Value = author.colorMachineHydrogenGenerator.AsFloat4()},
                colorDirt = new URPMaterialPropertyBaseColor{Value = author.colorDirt.AsFloat4()},
                colorGrass = new URPMaterialPropertyBaseColor{Value = author.colorGrass.AsFloat4()},
                colorStone = new URPMaterialPropertyBaseColor{Value = author.colorStone.AsFloat4()},
                colorMachineH20Generator = new URPMaterialPropertyBaseColor{Value = author.colorMachineH20Generator.AsFloat4()},
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
    public URPMaterialPropertyBaseColor colorStone;
    public URPMaterialPropertyBaseColor colorMachineH20Generator;
    public URPMaterialPropertyBaseColor colorMachineHydrogenGenerator;
    public URPMaterialPropertyBaseColor colorDirt;
    public URPMaterialPropertyBaseColor colorGrass;
}