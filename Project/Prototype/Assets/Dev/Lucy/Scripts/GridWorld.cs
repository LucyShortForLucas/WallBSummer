#nullable enable
using System;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// IMPORTANT: the 2d tile arrays this returns are indexed as [y,x]
public class GridWorld: IDisposable
{
    // ------ Static api --------------

    // ---- enums
    public enum WaterTileType: Byte
    {
        GroundWater = 0, // The water of this tile exists underground, like in soil
        FlowingWater = 1, // The water of this tile flows, like in a river. 
        StillWater = 2, // The water of this tile is still, like in a basin or pond. 
        WaterSource = 3, // The water of this tile is a 'source' and magically fills itself, like the mouth of a river.
        NoWater = 4 // This tile cannot contain water.
    }

    public enum BuildObstructionType : Byte
    {
        None,
        Natural,
        Building
    };

    // ---- Constants 
    private const uint NULLHANDLE = uint.MaxValue;
    public const int CHUNK_SIZE = 16;
    public const int CHUNK_DATA_SIZE = CHUNK_SIZE * CHUNK_SIZE;
    public const float UNITS_PER_TILE = 1f;
    public static readonly GridWorldInfo INFO = NativeMethods.get_gridworld_info();

    // ---- Gridworld Helpers
    public static Vector2Int PositionToTile(Vector3 pos)
    {
        var pos2 = new Vector2(pos.x, pos.z);
        pos2 /= new Vector2(UNITS_PER_TILE, UNITS_PER_TILE);
        Vector2Int posInt = new(Mathf.FloorToInt(pos2.x), Mathf.FloorToInt(pos2.y));
        return posInt;
    }
    public static Vector3 TileToPosition(Vector2Int tile, float y = 0)
    {
        return new Vector3(tile.x * UNITS_PER_TILE, y, tile.y * UNITS_PER_TILE);
    }

    public static Vector2Int TileToChunk(Vector2Int tileCoord)
    {
        static int floorIntToChunk(int value) => Math.DivRem(value, GridWorld.CHUNK_SIZE, out var remainder) is var quotient
        ? (remainder < 0 ? (quotient - 1) * GridWorld.CHUNK_SIZE : quotient * GridWorld.CHUNK_SIZE)
        : 0;

        tileCoord.x = floorIntToChunk(tileCoord.x) / GridWorld.CHUNK_SIZE;
        tileCoord.y = floorIntToChunk(tileCoord.y) / GridWorld.CHUNK_SIZE;
        return tileCoord;
    }

    public static Vector2Int ChunkToTile(Vector2Int chunkCoord)
    {
        return chunkCoord * CHUNK_SIZE;
    }
    public static Vector2Int PositionToChunk(Vector2 pos) => TileToChunk(PositionToTile(pos));

    // ------ Data --------------------
    private uint _handle = NULLHANDLE;

    // ------ Ctors + Dispose + Dtor --
    public GridWorld(uint initOp = 0)
    {
        _handle = NativeMethods.create_gridworld(initOp);
    }

    public static GridWorld TestGridWorld()
    {
        return new GridWorld((uint)NativeMethods.WorldInitType.Test);
    }

    public void Dispose()
    {
        NativeMethods.destroy_gridworld(_handle);
        _handle = NULLHANDLE;
    }

    ~GridWorld()
    {
        if (_handle == NULLHANDLE) return;
        NativeMethods.destroy_gridworld(_handle);
    }

    // ------ Public api --------------

    // ---- Update methods
    public void Update(float deltaTime)
    {
        if (_handle == NULLHANDLE) return;
        NativeMethods.update_gridworld(_handle, deltaTime);
    }

    // ---- Chunk control methods
    private void ManageChunk(RectInt rect, NativeMethods.ChunkStateOp op) {
        if (_handle == NULLHANDLE) return;

        NativeMethods.manage_chunks(_handle, (uint)op,
            rect.x, rect.y, rect.width, rect.height);
    }
    public void LoadChunksAsleep(RectInt rect) => ManageChunk(rect, NativeMethods.ChunkStateOp.LoadChunksAsleep);
    public void WakeChunks(RectInt rect) => ManageChunk(rect, NativeMethods.ChunkStateOp.WakeChunks);
    public void SleepChunks(RectInt rect) => ManageChunk(rect, NativeMethods.ChunkStateOp.SleepChunks);

    // ---- Get/set tile methods

    private unsafe T[,] GetTileData<T>(RectInt rect, NativeMethods.TileDataType tileType) where T: unmanaged
    {
        var result = new T[rect.width, rect.height];
        if (_handle == NULLHANDLE) return result;

        fixed (T* pOut = result)
        {
            NativeMethods.get_tile_data(_handle, (uint)tileType,
                rect.x, rect.y, rect.width, rect.height,
                (IntPtr)pOut);
        }

        return result;
    }
    public int[,] GetFertility(RectInt rect) => GetTileData<int>(rect, NativeMethods.TileDataType.Fertility);
    public int[,] GetFertilityChunk(Vector2Int coord) => GetFertility(new RectInt(coord.x * CHUNK_SIZE, coord.y * CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));
    public int[,] GetWaterContent(RectInt rect) => GetTileData<int>(rect, NativeMethods.TileDataType.WaterContent);
    public int[,] GetWaterContentChunk(Vector2Int coord) => GetWaterContent(new RectInt(coord.x * CHUNK_SIZE, coord.y * CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));
    public WaterTileType[,] GetWaterType(RectInt rect) => GetTileData<WaterTileType>(rect, NativeMethods.TileDataType.WaterType);
    public WaterTileType[,] GetWaterTypeChunk(Vector2Int coord) => GetWaterType(new RectInt(coord.x * CHUNK_SIZE, coord.y * CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));
    public BuildObstructionType[,] GetBuildObstructionType(RectInt rect) => GetTileData<BuildObstructionType>(rect, NativeMethods.TileDataType.BuildObstructionType);
    public BuildObstructionType[,] GetBuildObstructionTypeChunk(Vector2Int coord) => GetBuildObstructionType(new RectInt(coord.x * CHUNK_SIZE, coord.y * CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));

    private unsafe void FillTileData<T>(RectInt rect, NativeMethods.TileDataType tileType, T value) where T : unmanaged
    {
        if (_handle == NULLHANDLE) return;

        NativeMethods.fill_tile_data(_handle, (uint)tileType,
            rect.x, rect.y, rect.width, rect.height,
            (IntPtr)(&value));
    }
    public void FillFertility(RectInt rect, int value) => FillTileData(rect, NativeMethods.TileDataType.Fertility, value);
    public void FillWaterContent(RectInt rect, int value) => FillTileData(rect, NativeMethods.TileDataType.WaterContent, value);
    public void FillWaterType(RectInt rect, WaterTileType value) => FillTileData(rect, NativeMethods.TileDataType.WaterType, value);
    public void FillBuildObstructionType(RectInt rect, BuildObstructionType value) => FillTileData(rect, NativeMethods.TileDataType.BuildObstructionType, value);

    // ---- Build system stuff
    private RectInt _buildAttemptRect = new();
    public RectInt BuildAttemptRect { get => _buildAttemptRect; set => _buildAttemptRect = value; }
    private bool _buildAllowed = true;
    public bool BuildAllowed { get => _buildAllowed; set { _buildAllowed = value; } }

    // ------ Private P/Invoke Native Plugin Interop ------
    private static class NativeMethods
    {
        private const string DllName = "NativePlugin_unityExport";

        public enum WorldInitType : uint
        {
            Default = 0,
            Test = 1
        }

        public enum ChunkStateOp : uint
        {
            LoadChunksAsleep = 0,
            WakeChunks = 1,
            SleepChunks = 2
        }

        public enum TileDataType : uint
        {
            WaterContent = 0,
            WaterType = 1,
            Fertility = 2,
            BuildObstructionType = 3
        }

        // ---- World management
        [DllImport(DllName)]
        public static extern uint create_gridworld(uint worldType);

        [DllImport(DllName)]
        public static extern void destroy_gridworld(uint id);

        [DllImport(DllName)]
        public static extern void update_gridworld(uint id, float deltatime);

        // ---- Chunk state management
        [DllImport(DllName)]
        public static extern void manage_chunks(
            uint worldId, uint funcId,
            int x, int y,
            int width, int height);

        // ---- Get/set tiles
        [DllImport(DllName)]
        public static extern void get_tile_data(
            uint worldId, uint tileDataType,
            int x, int y,
            int width, int height,
            IntPtr pOut);

        [DllImport(DllName)]
        public static extern void fill_tile_data(
            uint worldId, uint tileDataType,
            int x, int y,
            int width, int height,
            IntPtr pIn);

        [DllImport(DllName)]
        public static extern GridWorldInfo get_gridworld_info();

    }
}

