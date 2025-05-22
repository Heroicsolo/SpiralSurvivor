using Heroicsolo.SpiralSurvivor.LevelGeometry.Entities;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.Utils
{
    public static class SpiralSDF3D
    {
        private const float EPSILON = 0.01f;
        private const float TWO_PI = 2f * Mathf.PI;

        private static SpiralWallEntity _wallEntity;
        private static Texture3D _sdfTex;

        public static void SetWallParams(SpiralWallEntity wallEntity)
        {
            _wallEntity = wallEntity;
            _sdfTex = GenerateSDFTexture();
        }

        // Get distance from a 3D point to the spiral wall
        public static float GetDistance(Vector3 pos)
        {
            var xz = new Vector2(pos.x - _wallEntity.Position.x, pos.z - _wallEntity.Position.z);
            var angle = Mathf.Atan2(xz.y, xz.x);
            if (angle < 0f)
            {
                angle += TWO_PI;
            }
            var unwrappedAngle = angle + TWO_PI * Mathf.Floor((xz.magnitude - _wallEntity.StartRadius) / (_wallEntity.Spacing * TWO_PI));
            var spiralRadius = _wallEntity.StartRadius + _wallEntity.Spacing * unwrappedAngle;

            var distXZ = Mathf.Abs(xz.magnitude - spiralRadius) - _wallEntity.WallThickness / 2f;
            var distY = Mathf.Abs(pos.y) - _wallEntity.WallHeight / 2f;

            var outside = Mathf.Max(distXZ, distY);
            var inside = Mathf.Min(Mathf.Max(distXZ, distY), 0.0f);

            return outside + inside;
        }

        public static Vector3 EstimateNormal(Vector3 worldPos)
        {
            var dx = SampleSDF(worldPos + Vector3.right * EPSILON) - SampleSDF(worldPos - Vector3.right * EPSILON);
            var dy = SampleSDF(worldPos + Vector3.up * EPSILON) - SampleSDF(worldPos - Vector3.up * EPSILON);
            var dz = SampleSDF(worldPos + Vector3.forward * EPSILON) - SampleSDF(worldPos - Vector3.forward * EPSILON);
            var normal = new Vector3(dx, dy, dz).normalized;
            return normal;
        }
        
        private static Texture3D GenerateSDFTexture()
        {
            var width = _wallEntity.GridResolution.x;
            var height = _wallEntity.GridResolution.y;
            var depth = _wallEntity.GridResolution.z;

            var sdfTexture = new Texture3D(width, height, depth, TextureFormat.RFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear
            };

            var colors = new Color[width * height * depth];

            for (var z = 0; z < depth; z++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var normalized = new Vector3(
                            x / (float)(width - 1),
                            y / (float)(height - 1),
                            z / (float)(depth - 1)
                        );

                        var worldPos = new Vector3(
                            Mathf.Lerp(_wallEntity.VolumeMin.x, _wallEntity.VolumeMax.x, normalized.x),
                            Mathf.Lerp(_wallEntity.VolumeMin.y, _wallEntity.VolumeMax.y, normalized.y),
                            Mathf.Lerp(_wallEntity.VolumeMin.z, _wallEntity.VolumeMax.z, normalized.z)
                        );
                        
                        var distance = GetDistance(worldPos); // Your SDF here

                        // Convert to grayscale float format (distance)
                        colors[x + y * width + z * width * height] = new Color(distance, 0, 0, 1);
                    }
                }
            }

            sdfTexture.SetPixels(colors);
            sdfTexture.Apply();

            return sdfTexture;
        }

        private static float SampleSDF(Vector3 worldPos)
        {
            var localNorm = new Vector3(
                Mathf.InverseLerp(_wallEntity.VolumeMin.x, _wallEntity.VolumeMax.x, worldPos.x),
                Mathf.InverseLerp(_wallEntity.VolumeMin.y, _wallEntity.VolumeMax.y, worldPos.y),
                Mathf.InverseLerp(_wallEntity.VolumeMin.z, _wallEntity.VolumeMax.z, worldPos.z)
            );

            // Clamp between 0 and 1
            localNorm = Vector3.Min(Vector3.one, Vector3.Max(Vector3.zero, localNorm));

            // Sample with trilinear interpolation manually
            return SampleTexture3DTrilinear(localNorm);
        }

        private static float SampleTexture3DTrilinear(Vector3 normalizedPos)
        {
            var width = _sdfTex.width;
            var height = _sdfTex.height;
            var depth = _sdfTex.depth;

            // Convert normalizedPos [0,1] to voxel space
            var fx = normalizedPos.x * (width - 1);
            var fy = normalizedPos.y * (height - 1);
            var fz = normalizedPos.z * (depth - 1);

            var x0 = Mathf.FloorToInt(fx);
            var y0 = Mathf.FloorToInt(fy);
            var z0 = Mathf.FloorToInt(fz);
            var x1 = Mathf.Min(x0 + 1, width - 1);
            var y1 = Mathf.Min(y0 + 1, height - 1);
            var z1 = Mathf.Min(z0 + 1, depth - 1);

            var dx = fx - x0;
            var dy = fy - y0;
            var dz = fz - z0;

            // Get 8 surrounding samples
            var c000 = _sdfTex.GetPixel(x0, y0, z0).r;
            var c100 = _sdfTex.GetPixel(x1, y0, z0).r;
            var c010 = _sdfTex.GetPixel(x0, y1, z0).r;
            var c110 = _sdfTex.GetPixel(x1, y1, z0).r;
            var c001 = _sdfTex.GetPixel(x0, y0, z1).r;
            var c101 = _sdfTex.GetPixel(x1, y0, z1).r;
            var c011 = _sdfTex.GetPixel(x0, y1, z1).r;
            var c111 = _sdfTex.GetPixel(x1, y1, z1).r;

            // Interpolate
            var c00 = Mathf.Lerp(c000, c100, dx);
            var c01 = Mathf.Lerp(c001, c101, dx);
            var c10 = Mathf.Lerp(c010, c110, dx);
            var c11 = Mathf.Lerp(c011, c111, dx);

            var c0 = Mathf.Lerp(c00, c10, dy);
            var c1 = Mathf.Lerp(c01, c11, dy);

            var c = Mathf.Lerp(c0, c1, dz);

            return c;
        }
    }
}
