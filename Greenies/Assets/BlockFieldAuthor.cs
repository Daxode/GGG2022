using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BlockFieldAuthor : MonoBehaviour
{
    public GameObject voxelPrefab;
    public int gridDimensionSize;
    class Baker : Baker<BlockFieldAuthor>
    {
        public override void Bake(BlockFieldAuthor authoring)
        {
            AddComponent(new BlockFieldInfo
            {
                voxelPrefab = GetEntity(authoring.voxelPrefab),
                gridDimensionSize = authoring.gridDimensionSize
            });
            AddComponent<BlockFieldInit>();
        }
    }
}
struct BlockFieldInit : IComponentData, IEnableableComponent {}

public struct BlockField : ICleanupComponentData
{
    public NativeArray<BlockState> blockField;
}

struct BlockFieldInfo : IComponentData
{
    public int gridDimensionSize;
    public Entity voxelPrefab;
}

public enum BlockState : byte
{
    Clear,
    Stone,
    Dirt,
    Grass,
}