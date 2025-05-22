using System.Collections.Generic;
using Heroicsolo.SpiralSurvivor.LevelGeometry.Entities;
using Heroicsolo.SpiralSurvivor.Utils;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.LevelGeometry
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class SpiralWallGenerator : MonoBehaviour
    {
        private const float TWO_PI = Mathf.PI * 2f;

        [Header("Spiral Settings")]
        [SerializeField] private float _startRadius = 1f;
        [SerializeField] private float _spacing = 0.2f; // Distance between turns
        [SerializeField] private float _wallThickness = 0.3f; // Width of the wall
        [SerializeField] private float _wallHeight = 2f; // Height of the wall
        [SerializeField] private int _numTurns = 5;
        [SerializeField] private int _segmentsPerTurn = 50;
        [SerializeField] private int _startOffset = -10;

        private readonly Vector3Int _gridResolution = new(512, 32, 512);
        private readonly Vector3 _volumeMin = new(-100, 0, -100);
        private readonly Vector3 _volumeMax = new(100, 5, 100);

        private SpiralWallEntity _wallEntity;

        private void Start()
        {
            GenerateSpiralWall();
            
            _wallEntity = new SpiralWallEntity
            {
                GridResolution = _gridResolution, VolumeMin = _volumeMin,
                VolumeMax = _volumeMax, StartRadius = _startRadius,
                Spacing = _spacing, WallThickness = _wallThickness,
                WallHeight = _wallHeight, NumTurns = _numTurns,
                Position = transform.position
            };

            SpiralSDF3D.SetWallParams(_wallEntity);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            for (var i = _startOffset; i < _segmentsPerTurn * _numTurns; i++)
            {
                var a0 = i / (float)_segmentsPerTurn;
                var a1 = (i + 1) / (float)_segmentsPerTurn;
                var theta0 = a0 * TWO_PI;
                var theta1 = a1 * TWO_PI;

                var r0 = _startRadius + _spacing * theta0;
                var r1 = _startRadius + _spacing * theta1;

                var p0 = new Vector3(Mathf.Cos(theta0), 0, Mathf.Sin(theta0)) * r0;
                var p1 = new Vector3(Mathf.Cos(theta1), 0, Mathf.Sin(theta1)) * r1;

                // Draw line representing spiral path center
                Gizmos.DrawLine(p0, p1);

                // Draw wall width indicator
                var dir0 = Quaternion.Euler(0, 90, 0) * (p1 - p0).normalized * (_wallThickness * 0.5f);
                Gizmos.DrawLine(p0 - dir0, p0 + dir0);
            }
        }

        private void GenerateSpiralWall()
        {
            var totalSegments = Mathf.RoundToInt(_numTurns * _segmentsPerTurn);
            var angleStep = TWO_PI * _numTurns / totalSegments;

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (var i = _startOffset; i < totalSegments; i++)
            {
                var a0 = i * angleStep;
                var a1 = (i + 1) * angleStep;

                var r0 = _startRadius + _spacing * a0;
                var r1 = _startRadius + _spacing * a1;

                var center0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)) * r0;
                var center1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * r1;

                var dir0 = new Vector3(-Mathf.Sin(a0), 0, Mathf.Cos(a0)).normalized;
                var dir1 = new Vector3(-Mathf.Sin(a1), 0, Mathf.Cos(a1)).normalized;

                var right0 = Vector3.Cross(Vector3.up, dir0);
                var right1 = Vector3.Cross(Vector3.up, dir1);

                var halfH = _wallHeight / 2f;
                var halfT = _wallThickness / 2f;

                // 8 vertices per segment (quad ring)
                var ring = new Vector3[8];
                ring[0] = center0 + right0 * -halfT + Vector3.up * -halfH; // bottom inner
                ring[1] = center0 + right0 * +halfT + Vector3.up * -halfH; // bottom outer
                ring[2] = center0 + right0 * +halfT + Vector3.up * +halfH; // top outer
                ring[3] = center0 + right0 * -halfT + Vector3.up * +halfH; // top inner

                ring[4] = center1 + right1 * -halfT + Vector3.up * -halfH;
                ring[5] = center1 + right1 * +halfT + Vector3.up * -halfH;
                ring[6] = center1 + right1 * +halfT + Vector3.up * +halfH;
                ring[7] = center1 + right1 * -halfT + Vector3.up * +halfH;

                var baseIndex = vertices.Count;
                vertices.AddRange(ring);

                // Inner side
                AddQuad(0, 4, 7, 3);
                // Outer side
                AddQuad(5, 1, 2, 6);
                // Top face
                AddQuad(3, 7, 6, 2);
                // Bottom face
                AddQuad(0, 1, 5, 4);
                continue;

                void AddQuad(int a, int b, int c, int d)
                {
                    triangles.Add(baseIndex + a);
                    triangles.Add(baseIndex + b);
                    triangles.Add(baseIndex + c);
                    triangles.Add(baseIndex + c);
                    triangles.Add(baseIndex + d);
                    triangles.Add(baseIndex + a);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
