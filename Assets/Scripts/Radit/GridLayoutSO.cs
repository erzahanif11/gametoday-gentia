using UnityEngine;

/// <summary>
/// ScriptableObject that stores a 2D grid of platform types.
/// The grid is stored as a flat array and accessed via (x, y) helpers.
/// </summary>
[CreateAssetMenu(fileName = "NewGridLayout", menuName = "Grid/Grid Layout")]
public class GridLayoutSO : ScriptableObject
{
    [Min(1)] public int width = 5;
    [Min(1)] public int height = 5;

    [Tooltip("Flat array backing the 2D grid (row-major). Resized automatically via the context menu or editor script.")]
    public PlatformType[] cells;

    // ───────── Helpers ─────────

    /// <summary>Converts (x, y) to a flat index. Returns -1 if out of bounds.</summary>
    public int Index(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return -1;
        return y * width + x;
    }

    /// <summary>Gets the platform type at (x, y). Returns Empty if out of bounds.</summary>
    public PlatformType GetCell(int x, int y)
    {
        int i = Index(x, y);
        if (i < 0 || cells == null || i >= cells.Length) return PlatformType.Empty;
        return cells[i];
    }

    /// <summary>Sets the platform type at (x, y). No-op if out of bounds.</summary>
    public void SetCell(int x, int y, PlatformType type)
    {
        int i = Index(x, y);
        if (i < 0 || cells == null || i >= cells.Length) return;
        cells[i] = type;
    }

    /// <summary>Returns true when (x, y) is inside the grid.</summary>
    public bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    // ───────── Resize ─────────

    /// <summary>
    /// Resizes the cells array to match width × height.
    /// Existing data that still fits is preserved; new cells default to Empty.
    /// </summary>
    [ContextMenu("Resize Grid")]
    public void ResizeGrid()
    {
        var old = cells;
        int oldWidth = (old != null && old.Length > 0) ? (cells.Length > 0 ? width : 0) : 0;

        cells = new PlatformType[width * height];

        if (old == null) return;

        // Copy overlapping region
        int copyW = Mathf.Min(oldWidth, width);
        int copyH = Mathf.Min(old.Length / Mathf.Max(oldWidth, 1), height);
        for (int y = 0; y < copyH; y++)
        {
            for (int x = 0; x < copyW; x++)
            {
                int srcIdx = y * oldWidth + x;
                int dstIdx = y * width + x;
                if (srcIdx < old.Length)
                    cells[dstIdx] = old[srcIdx];
            }
        }
    }

    private void OnValidate()
    {
        if (cells == null || cells.Length != width * height)
            ResizeGrid();
    }
}
