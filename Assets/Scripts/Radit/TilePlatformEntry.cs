using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Maps a <see cref="TileBase"/> asset (painted in the Tilemap) to its
/// corresponding <see cref="PlatformType"/> and spawn prefab.
/// </summary>
[System.Serializable]
public struct TilePlatformEntry
{
    [Tooltip("The Tile asset painted in the Tilemap.")]
    public TileBase tile;

    [Tooltip("Platform type this tile represents.")]
    public PlatformType type;

    [Tooltip("Prefab to spawn at this tile's position.")]
    public GameObject prefab;
}

/// <summary>
/// Maps a cell position on the Tilemap to a pressure platform ID.
/// Used to assign unique IDs to Pressure tiles painted at specific positions.
/// </summary>
[System.Serializable]
public struct CellIdEntry
{
    [Tooltip("Cell position (x, y) on the Tilemap where a Pressure tile is painted.")]
    public Vector2Int cellPosition;

    [Tooltip("The unique pressure platform ID for this cell.")]
    public int id;
}

/// <summary>
/// Maps a cell position on the lever Tilemap to a lever ID.
/// Used to assign unique IDs to Lever tiles painted at specific positions.
/// </summary>
[System.Serializable]
public struct LeverCellIdEntry
{
    [Tooltip("Cell position (x, y) on the lever Tilemap where a Lever tile is painted.")]
    public Vector2Int cellPosition;

    [Tooltip("The unique lever ID for this cell.")]
    public int id;
}
