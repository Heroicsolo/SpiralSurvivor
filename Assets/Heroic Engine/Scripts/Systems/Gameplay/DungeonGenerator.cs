using HeroicEngine.Systems.DI;
using HeroicEngine.Utils.Math;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public class DungeonRoom
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        public readonly List<DungeonRoom> ConnectedRooms;

        public DungeonRoom(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ConnectedRooms = new List<DungeonRoom>();
        }
        public DungeonRoom(List<DungeonRoom> connectedRooms)
        {
            ConnectedRooms = connectedRooms;
        }
    }

    public class DungeonGenerator : SystemBase, IDungeonGenerator
    {
        [SerializeField] private List<GameObject> floorTilePrefabs = new();
        [SerializeField] private List<GameObject> wallTilePrefabs = new();
        [SerializeField] private bool enableWalls = true;
        [SerializeField] private bool generateAtStart = true;

        [Header("Dungeon Settings")]
        [SerializeField] private int dungeonWidth = 50;
        [SerializeField] private int dungeonLength = 50;
        [SerializeField] private int minRoomSize = 6;
        [SerializeField] private int maxIterations = 5;
        [SerializeField] private int roomMargin = 2; // Space to leave between rooms for corridors
        [SerializeField] private int corridorWidth = 2;

        private readonly List<DungeonRoom> _rooms = new();
        private readonly List<GameObject> _floorTiles = new();
        private readonly List<GameObject> _wallTiles = new();
        private readonly HashSet<Vector2Int> _floorPositions = new();
        private NavMeshSurface _navMeshSurface;

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            GenerateDungeon(Vector3.zero);
        }

        /// <summary>
        /// This method generates dungeon with default settings set in DungeonGenerator inspector
        /// </summary>
        /// <param name="pos">Start position</param>
        public List<DungeonRoom> GenerateDungeon(Vector3 pos)
        {
            return GenerateDungeon(pos, dungeonWidth, dungeonLength, enableWalls, minRoomSize, corridorWidth, maxIterations);
        }

        /// <summary>
        /// This method generates dungeon with default settings set in DungeonGenerator inspector,
        /// but with custom width and length
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <param name="width">Width of dungeon</param>
        /// <param name="length">Length of dungeon</param>
        public List<DungeonRoom> GenerateDungeon(Vector3 pos, int width, int length)
        {
            return GenerateDungeon(pos, width, length, enableWalls, minRoomSize, corridorWidth, maxIterations);
        }

        /// <summary>
        /// This method generates dungeon with custom settings
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <param name="width">Width of dungeon</param>
        /// <param name="length">Length of dungeon</param>
        /// <param name="enableWalls">Enable walls?</param>
        /// <param name="minRoomSize">Min room size</param>
        /// <param name="roomMargin">Margin between rooms</param>
        /// <param name="maxIterations">Max number of rooms splitting iterations</param>
        public List<DungeonRoom> GenerateDungeon(Vector3 pos, int width, int length, bool enableWalls = true, int minRoomSize = 5, int roomMargin = 2, int corridorWidth = 2, int maxIterations = 5, bool generateNavMesh = true)
        {
            dungeonWidth = width;
            dungeonLength = length;
            this.minRoomSize = minRoomSize;
            this.maxIterations = maxIterations;
            this.enableWalls = enableWalls;
            this.roomMargin = roomMargin;
            this.corridorWidth = corridorWidth;

            ClearDungeon();

            var rootRoom = new DungeonRoom((int)pos.x, (int)pos.z, dungeonWidth, dungeonLength);
            SplitRoom(rootRoom, 0);

            foreach (var room in _rooms)
            {
                CreateFloor(room);
            }

            for (var i = 0; i < _rooms.Count - 1; i++)
            {
                CreateCorridor(_rooms[i], _rooms[i + 1]);
            }

            if (enableWalls)
            {
                CreateWalls();
            }

            if (generateNavMesh)
            {
                GenerateNavMesh();
            }

            return _rooms;
        }

        /// <summary>
        /// This method clears current dungeon
        /// </summary>
        public void ClearDungeon()
        {
            _rooms.Clear();
            foreach (var floor in _floorTiles.ToArray())
            {
                Destroy(floor);
            }
            foreach (var wall in _wallTiles.ToArray())
            {
                Destroy(wall);
            }
            _floorPositions.Clear();
            _floorTiles.Clear();
            _wallTiles.Clear();
        }

        /// <summary>
        /// This method generates NavMesh for current dungeon
        /// </summary>
        public void GenerateNavMesh()
        {
            if (!gameObject.TryGetComponent(out NavMeshSurface _))
            {
                _navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            }

            _navMeshSurface.collectObjects = CollectObjects.Children;
            _navMeshSurface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            _navMeshSurface.BuildNavMesh();
        }

        private void SplitRoom(DungeonRoom room, int iteration)
        {
            if (iteration >= maxIterations || room.Width <= minRoomSize + roomMargin * 2 && room.Height <= minRoomSize + roomMargin * 2)
            {
                // Reduce room size to leave a margin
                var roomX = room.X + roomMargin;
                var roomY = room.Y + roomMargin;
                var roomWidth = room.Width - roomMargin * 2;
                var roomHeight = room.Height - roomMargin * 2;

                if (roomWidth > 0 && roomHeight > 0)
                {
                    _rooms.Add(new DungeonRoom(roomX, roomY, roomWidth, roomHeight));
                }

                return;
            }

            var splitHorizontal = Random.Range(0, 2) == 0;

            if (room.Width > room.Height && room.Width / 2 >= minRoomSize + roomMargin * 2) splitHorizontal = false;
            else if (room.Height > room.Width && room.Height / 2 >= minRoomSize + roomMargin * 2) splitHorizontal = true;

            if (splitHorizontal)
            {
                var split = Random.Range(minRoomSize + roomMargin, room.Height - (minRoomSize + roomMargin));
                var topRoom = new DungeonRoom(room.X, room.Y, room.Width, split);
                var bottomRoom = new DungeonRoom(room.X, room.Y + split, room.Width, room.Height - split);
                SplitRoom(topRoom, iteration + 1);
                SplitRoom(bottomRoom, iteration + 1);
            }
            else
            {
                var split = Random.Range(minRoomSize + roomMargin, room.Width - (minRoomSize + roomMargin));
                var leftRoom = new DungeonRoom(room.X, room.Y, split, room.Height);
                var rightRoom = new DungeonRoom(room.X + split, room.Y, room.Width - split, room.Height);
                SplitRoom(leftRoom, iteration + 1);
                SplitRoom(rightRoom, iteration + 1);
            }
        }

        private void CreateCorridor(DungeonRoom roomA, DungeonRoom roomB)
        {
            roomA.ConnectedRooms.Add(roomB);
            roomB.ConnectedRooms.Add(roomA);

            var centerA = new Vector2Int(roomA.X + roomA.Width / 2, roomA.Y + roomA.Height / 2);
            var centerB = new Vector2Int(roomB.X + roomB.Width / 2, roomB.Y + roomB.Height / 2);

            if (Random.Range(0, 2) == 0)
            {
                CreateFloor(new DungeonRoom(centerA.x, centerA.y, Mathf.Abs(centerB.x - centerA.x) + 1, corridorWidth));
                CreateFloor(new DungeonRoom(centerB.x, Mathf.Min(centerA.y, centerB.y), corridorWidth, Mathf.Abs(centerB.y - centerA.y) + 1));
            }
            else
            {
                CreateFloor(new DungeonRoom(Mathf.Min(centerA.x, centerB.x), centerA.y, Mathf.Abs(centerB.x - centerA.x) + 1, corridorWidth));
                CreateFloor(new DungeonRoom(centerB.x, centerB.y, corridorWidth, Mathf.Abs(centerB.y - centerA.y) + 1));
            }
        }

        private void CreateFloor(DungeonRoom room)
        {
            for (var x = room.X; x < room.X + room.Width; x++)
            {
                for (var y = room.Y; y < room.Y + room.Height; y++)
                {
                    var tilePos = new Vector2Int(x, y);
                    if (!_floorPositions.Contains(tilePos))
                    {
                        _floorTiles.Add(Instantiate(floorTilePrefabs.GetRandomElement(), new Vector3(x, 0, y), Quaternion.identity, transform));
                        _floorPositions.Add(tilePos);
                    }
                }
            }
        }

        private void CreateWalls()
        {
            foreach (var position in _floorPositions.ToArray())
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dz = -1; dz <= 1; dz++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dz) == 0) continue;

                        var neighborPosition = position + new Vector2Int(dx, dz);

                        if (!_floorPositions.Contains(neighborPosition))
                        {
                            _wallTiles.Add(Instantiate(wallTilePrefabs.GetRandomElement(), new Vector3(neighborPosition.x, 0, neighborPosition.y), Quaternion.identity, transform));
                            _floorPositions.Add(neighborPosition);
                        }
                    }
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            if (generateAtStart)
            {
                GenerateDungeon(Vector3.zero, dungeonWidth, dungeonLength, true, minRoomSize, roomMargin, maxIterations);
            }
        }
    }
}
