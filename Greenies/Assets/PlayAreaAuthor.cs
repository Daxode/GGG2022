using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class PlayAreaAuthor : MonoBehaviour
{
    public GameObject voxelPrefab;
    public int gridDimensionSize;
    class Baker : Baker<PlayAreaAuthor>
    {
        public override void Bake(PlayAreaAuthor authoring)
        {
            AddComponent(new PlayAreaInfo
            {
                voxelPrefab = GetEntity(authoring.voxelPrefab),
                gridDimensionSize = authoring.gridDimensionSize
            });
            AddComponent<PlayAreaDirty>();
            AddComponent<PlayAreaInit>();
        }
    }
}
struct PlayAreaDirty : IComponentData, IEnableableComponent {}
struct PlayAreaInit : IComponentData, IEnableableComponent {}

public struct PlayAreaData : ICleanupComponentData
{
    public NativeArray<BlockState> blockField;
}

struct PlayAreaInfo : IComponentData
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