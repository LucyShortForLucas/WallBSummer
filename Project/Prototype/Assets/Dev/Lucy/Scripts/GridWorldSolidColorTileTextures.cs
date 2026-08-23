#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class GridWorldSolidColorTileTextures
{
    // ------ Helper class -----------
    private class Chunk { public Color32[] colors = new Color32[GridWorld.CHUNK_DATA_SIZE]; public int texId = -1; public bool dirty = false; };

    // ------ Events -----------------
    public event Action<Vector2Int>? ChunkTextureFreed;

    // ------ public API --------------

    // ---- Color and Texture methods

    public void SetChunkColor(Vector2Int chunkCoord, Color32[] colors)
    {
        if (!_chunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            chunk = new();
            _chunks.Add(chunkCoord, chunk);
        }

        chunk.colors = colors;

        MarkChunkDirty(chunk);
    }
    public void MakeChunkTexture(Vector2Int chunkCoord)
    {
        if (!_chunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            chunk = new();
            _chunks.Add(chunkCoord, chunk);
        }

        if (chunk.texId < 0)
            chunk.texId = NextTexId();

        MarkChunkDirty(chunk);
    }

    public void FreeChunkTexture(Vector2Int chunkCoord)
    {
        _chunks.TryGetValue(chunkCoord, out Chunk chunk);

        if (chunk == null || chunk.texId < 0)
            return;

        _freeTexIds.Push(chunk.texId);
        chunk.texId = -1;

        ChunkTextureFreed?.Invoke(chunkCoord);
    }

    public void UpdateDirtyTextures()
    {
        foreach (var chunk in _dirtyChunks)
        {
            _textures.SetPixels32(chunk.colors, chunk.texId, 0);
            chunk.dirty = false;
        }
        _textures.Apply();
    }

    // ---- Properties and getters

    public Texture2DArray TextureArray => _textures;

    public int GetTextureId(Vector2Int chunkCoord)
    {
        _chunks.TryGetValue(chunkCoord, out Chunk chunk);
        return chunk != null ? chunk.texId : -1;
    }

    // ------ Data --------------------

    // ---- Chunk data
    private Dictionary<Vector2Int, Chunk> _chunks = new();
    private Texture2DArray _textures = new Texture2DArray(GridWorld.CHUNK_SIZE, GridWorld.CHUNK_SIZE, 1024, TextureFormat.RGBA32, false, false) 
    { 
        filterMode = FilterMode.Point,
        wrapMode = TextureWrapMode.Clamp
    };

    // ---- Helper data
    private List<Chunk> _dirtyChunks = new();
    private Stack<int> _freeTexIds = new(new []{ 1});

    // ------ private methods ---------

    // ---- Private helper methods
    private int NextTexId()
    {
        int id = _freeTexIds.Pop();

        if (_freeTexIds.Count == 0)
            _freeTexIds.Push(id + 1);

        return id;
    }

    private void MarkChunkDirty(Chunk chunk)
    {
        if (chunk.texId < 0 || chunk.dirty)
            return;

        _dirtyChunks.Add(chunk);
        chunk.dirty = true;
    }
}
