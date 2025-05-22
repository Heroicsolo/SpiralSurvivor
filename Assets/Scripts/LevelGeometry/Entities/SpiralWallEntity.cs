using System;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.LevelGeometry.Entities
{
    [Serializable]
    public sealed class SpiralWallEntity
    {
        public Vector3 Position { get; set; }
        public float StartRadius { get; set; }
        public float Spacing { get; set; }
        public float WallThickness { get; set; }
        public float WallHeight { get; set; }
        public int NumTurns { get; set; }
        public Vector3Int GridResolution { get; set; }
        public Vector3 VolumeMin { get; set; }
        public Vector3 VolumeMax { get; set; }
    }
}