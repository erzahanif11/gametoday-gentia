using UnityEngine;

/// <summary>
/// Reads a <see cref="GridLayoutSO"/> and spawns the corresponding platform prefab
/// for every non-empty cell.
/// </summary>
public class GridSpawner : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private GridLayoutSO gridLayout;

    [Header("Prefabs")]
    [Tooltip("Assign one prefab per PlatformType. Index must match the enum value.")]
    [SerializeField] private PlatformPrefabEntry[] platformPrefabs;

    [Header("Spacing")]
    [SerializeField] private float cellSize = 1f;

    // Keeps spawned objects so we can clear / respawn at runtime.
    private Transform gridParent;

    private void Start()
    {
        SpawnGrid();
    }

    /// <summary>
    /// Destroys any previously spawned grid and rebuilds it from the layout.
    /// </summary>
    [ContextMenu("Spawn Grid")]
    public void SpawnGrid()
    {
        ClearGrid();

        if (gridLayout == null)
        {
            Debug.LogWarning("GridSpawner: No GridLayoutSO assigned.", this);
            return;
        }

        // Parent object keeps the hierarchy tidy
        gridParent = new GameObject("Grid").transform;
        gridParent.SetParent(transform);
        gridParent.localPosition = Vector3.zero;

        for (int y = 0; y < gridLayout.height; y++)
        {
            for (int x = 0; x < gridLayout.width; x++)
            {
                PlatformType type = gridLayout.GetCell(x, y);
                GameObject prefab = GetPrefab(type);

                if (prefab == null) continue; // skip Empty or unmapped types

                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0f);
                GameObject instance = Instantiate(prefab, gridParent);
                instance.transform.localPosition = pos;
                instance.name = $"{type}_{x}_{y}";
            }
        }
    }

    /// <summary>
    /// Removes all spawned platform objects.
    /// </summary>
    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        if (gridParent != null)
        {
            if (Application.isPlaying)
                Destroy(gridParent.gameObject);
            else
                DestroyImmediate(gridParent.gameObject);

            gridParent = null;
        }
    }

    private GameObject GetPrefab(PlatformType type)
    {
        if (platformPrefabs == null) return null;

        for (int i = 0; i < platformPrefabs.Length; i++)
        {
            if (platformPrefabs[i].type == type)
                return platformPrefabs[i].prefab;
        }

        return null;
    }
}

/// <summary>
/// Maps a <see cref="PlatformType"/> to its prefab.
/// </summary>
[System.Serializable]
public struct PlatformPrefabEntry
{
    public PlatformType type;
    public GameObject prefab;
}
