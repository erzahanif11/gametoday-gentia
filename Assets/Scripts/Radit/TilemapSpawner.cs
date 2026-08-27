using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Reads a <see cref="Tilemap"/> painted in the Scene View and spawns the
/// corresponding platform prefab for every tile found.
///
/// Pressure platform IDs are assigned via the <see cref="pressureIdMap"/>
/// Inspector list — map each cell position to a unique ID.
/// No secondary Tilemap or extra tile assets needed.
/// </summary>
public class TilemapSpawner : MonoBehaviour
{
    [Header("Tilemap Source")]
    [Tooltip("The Tilemap you painted in the Scene View. It will be hidden at runtime after spawning.")]
    [SerializeField] private Tilemap sourceTilemap;

    [Header("Lever Tilemap (Separate Layer)")]
    [Tooltip("Optional. A separate Tilemap layer for lever tiles.")]
    [SerializeField] private Tilemap leverTilemap;

    [Header("Chain Reaction")]
    [Tooltip("Optional. Defines which pressure platforms trigger which targets.")]
    [SerializeField] private ChainReactionData chainData;

    [Header("Lever Rules")]
    [Tooltip("Optional. Defines which levers control which platforms.")]
    [SerializeField] private LeverData leverData;

    [Header("Tile → Platform Mapping")]
    [Tooltip("Map each Tile asset to its PlatformType and spawn prefab. Only one Pressure tile needed.")]
    [SerializeField] private TilePlatformEntry[] tileMappings;

    [Header("Pressure ID Assignment")]
    [Tooltip("Assign a unique pressure ID to each cell position that has a Pressure tile.")]
    [SerializeField] private CellIdEntry[] pressureIdMap;

    [Header("Lever ID Assignment")]
    [Tooltip("Assign a unique lever ID to each cell position that has a Lever tile on the lever tilemap.")]
    [SerializeField] private LeverCellIdEntry[] leverIdMap;

    [Header("Options")]
    [Tooltip("If true, the source Tilemap's renderer is disabled after spawning so only the prefabs are visible.")]
    [SerializeField] private bool hideTilemapAfterSpawn = true;

    // Keeps spawned objects so we can clear / respawn at runtime.
    private Transform gridParent;

    // All spawned pressure platforms, used for deferred initialization.
    private List<PressurePlatform> spawnedPressurePlatforms = new List<PressurePlatform>();

    // All spawned levers, used for deferred rule application.
    private List<Lever> spawnedLevers = new List<Lever>();

    private void Start()
    {
        SpawnFromTilemap();
    }

    // ───────── Core Spawning ─────────

    /// <summary>
    /// Destroys any previously spawned grid and rebuilds it by reading the Tilemap.
    /// </summary>
    [ContextMenu("Spawn From Tilemap")]
    public void SpawnFromTilemap()
    {
        ClearGrid();
        spawnedPressurePlatforms.Clear();
        spawnedLevers.Clear();

        if (sourceTilemap == null)
        {
            Debug.LogWarning("TilemapSpawner: No source Tilemap assigned.", this);
            return;
        }

        if (tileMappings == null || tileMappings.Length == 0)
        {
            Debug.LogWarning("TilemapSpawner: No tile mappings configured.", this);
            return;
        }

        // Parent object keeps the hierarchy tidy
        gridParent = new GameObject("SpawnedPlatforms").transform;
        gridParent.SetParent(transform);
        gridParent.localPosition = Vector3.zero;

        // Ensure a PressurePlatformManager exists in the scene
        EnsurePressurePlatformManager();

        // Build lookups
        Dictionary<TileBase, TilePlatformEntry> tileLookup = BuildTileLookup();
        Dictionary<Vector2Int, int> idLookup = BuildIdLookup();

        // ── Phase 1: Iterate tilemap and spawn prefabs ──
        BoundsInt bounds = sourceTilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = sourceTilemap.GetTile(cellPos);

                if (tile == null) continue;

                if (!tileLookup.TryGetValue(tile, out TilePlatformEntry entry))
                {
                    Debug.LogWarning(
                        $"TilemapSpawner: Tile '{tile.name}' at ({x},{y}) has no mapping. Skipping.", this);
                    continue;
                }

                if (entry.prefab == null)
                {
                    Debug.LogWarning(
                        $"TilemapSpawner: Tile '{tile.name}' mapping has no prefab assigned. Skipping.", this);
                    continue;
                }

                // Convert cell position to world position (center of the cell)
                Vector3 worldPos = sourceTilemap.CellToWorld(cellPos)
                                 + sourceTilemap.cellSize * 0.5f;

                GameObject instance = Instantiate(entry.prefab, gridParent);
                instance.transform.position = worldPos;
                instance.name = $"{entry.type}_{x}_{y}";

                // Initialize pressure platforms — read ID from the pressureIdMap
                if (entry.type == PlatformType.Pressure)
                {
                    Vector2Int key = new Vector2Int(x, y);

                    if (!idLookup.TryGetValue(key, out int pressureId) || pressureId <= 0)
                    {
                        Debug.LogWarning(
                            $"TilemapSpawner: Pressure tile at ({x},{y}) has no ID in pressureIdMap. " +
                            $"Add a CellIdEntry for this position in the Inspector.", this);
                        continue;
                    }

                    PressurePlatform pp = instance.GetComponent<PressurePlatform>();
                    if (pp != null)
                    {
                        pp.Initialize(pressureId);
                        instance.name = $"Pressure_{pressureId}";
                        spawnedPressurePlatforms.Add(pp);
                    }
                }
            }
        }

        // ── Phase 2: Spawn levers from the lever tilemap ──
        if (leverTilemap != null)
        {
            SpawnLeversFromTilemap(tileLookup);
        }

        // ── Phase 3: Apply chain rules (now all platforms are registered) ──
        if (chainData != null)
        {
            ApplyChainRules();
        }

        // ── Phase 4: Apply lever rules ──
        if (leverData != null)
        {
            ApplyLeverRules();
        }

        // ── Phase 5: Apply initial visibility state AFTER rules are set ──
        for (int i = 0; i < spawnedPressurePlatforms.Count; i++)
        {
            spawnedPressurePlatforms[i].InitializeState();
        }

        // ── Phase 6: Hide the source tilemaps ──
        if (hideTilemapAfterSpawn)
        {
            TilemapRenderer renderer = sourceTilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
                renderer.enabled = false;

            if (leverTilemap != null)
            {
                TilemapRenderer leverRenderer = leverTilemap.GetComponent<TilemapRenderer>();
                if (leverRenderer != null)
                    leverRenderer.enabled = false;
            }
        }

        int leverCount = spawnedLevers.Count;
        Debug.Log($"TilemapSpawner: Spawned {gridParent.childCount} objects ({spawnedPressurePlatforms.Count} platforms, {leverCount} levers).", this);
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

    // ───────── Lookups ─────────

    /// <summary>
    /// Builds a Dictionary for O(1) layout tile lookups.
    /// </summary>
    private Dictionary<TileBase, TilePlatformEntry> BuildTileLookup()
    {
        var lookup = new Dictionary<TileBase, TilePlatformEntry>();

        for (int i = 0; i < tileMappings.Length; i++)
        {
            TilePlatformEntry entry = tileMappings[i];
            if (entry.tile == null) continue;

            if (lookup.ContainsKey(entry.tile))
            {
                Debug.LogWarning(
                    $"TilemapSpawner: Duplicate tile mapping for '{entry.tile.name}'. " +
                    $"Using the first entry.", this);
                continue;
            }

            lookup[entry.tile] = entry;
        }

        return lookup;
    }

    /// <summary>
    /// Builds a Dictionary mapping cell positions to pressure IDs for O(1) lookups.
    /// </summary>
    private Dictionary<Vector2Int, int> BuildIdLookup()
    {
        var lookup = new Dictionary<Vector2Int, int>();

        if (pressureIdMap == null) return lookup;

        for (int i = 0; i < pressureIdMap.Length; i++)
        {
            CellIdEntry entry = pressureIdMap[i];

            if (lookup.ContainsKey(entry.cellPosition))
            {
                Debug.LogWarning(
                    $"TilemapSpawner: Duplicate pressure ID at cell ({entry.cellPosition.x},{entry.cellPosition.y}). " +
                    $"Using the first entry.", this);
                continue;
            }

            lookup[entry.cellPosition] = entry.id;
        }

        return lookup;
    }

    // ───────── Chain Reaction Integration ─────────

    /// <summary>
    /// Creates a PressurePlatformManager if one doesn't already exist.
    /// </summary>
    private void EnsurePressurePlatformManager()
    {
        if (PressurePlatformManager.Instance != null) return;

        GameObject managerGO = new GameObject("PressurePlatformManager");
        managerGO.transform.SetParent(gridParent);
        managerGO.AddComponent<PressurePlatformManager>();
    }

    /// <summary>
    /// Applies chain reaction rules from <see cref="chainData"/> to all
    /// spawned <see cref="PressurePlatform"/> components.
    /// Platforms that are sources but never targets are set to start revealed.
    /// </summary>
    private void ApplyChainRules()
    {
        if (chainData == null || chainData.rules == null) return;

        var manager = PressurePlatformManager.Instance;
        if (manager == null) return;

        // Collect all target IDs so we know which platforms are "entry points"
        HashSet<int> allTargetIds = new HashSet<int>();
        for (int i = 0; i < chainData.rules.Length; i++)
        {
            if (chainData.rules[i].targetIds == null) continue;
            for (int j = 0; j < chainData.rules[i].targetIds.Length; j++)
            {
                allTargetIds.Add(chainData.rules[i].targetIds[j]);
            }
        }

        // Apply rules to each platform
        for (int i = 0; i < chainData.rules.Length; i++)
        {
            ChainRule rule = chainData.rules[i];
            PressurePlatform source = manager.GetById(rule.sourceId);

            if (source == null)
            {
                Debug.LogWarning(
                    $"TilemapSpawner: ChainRule source ID {rule.sourceId} not found.", this);
                continue;
            }

            source.targetIds = rule.targetIds;
            source.triggerDelay = rule.delay;
            source.persistent = rule.persistent;

            // If this source is never a target of another rule, it's an entry point
            if (!allTargetIds.Contains(rule.sourceId))
            {
                source.startsRevealed = true;
            }
        }
    }

    // ───────── Lever Integration ─────────

    /// <summary>
    /// Spawns lever prefabs from the lever tilemap layer.
    /// Uses the same tile lookup as the main tilemap — entries with
    /// <see cref="PlatformType.Lever"/> are matched.
    /// </summary>
    private void SpawnLeversFromTilemap(Dictionary<TileBase, TilePlatformEntry> tileLookup)
    {
        if (leverTilemap == null) return;

        Dictionary<Vector2Int, int> leverIdLookup = BuildLeverIdLookup();

        BoundsInt bounds = leverTilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = leverTilemap.GetTile(cellPos);

                if (tile == null) continue;

                if (!tileLookup.TryGetValue(tile, out TilePlatformEntry entry))
                {
                    Debug.LogWarning(
                        $"TilemapSpawner: Lever tile '{tile.name}' at ({x},{y}) has no mapping. Skipping.", this);
                    continue;
                }

                if (entry.type != PlatformType.Lever)
                {
                    Debug.LogWarning(
                        $"TilemapSpawner: Tile '{tile.name}' on lever tilemap at ({x},{y}) is not mapped as Lever type. Skipping.", this);
                    continue;
                }

                if (entry.prefab == null)
                {
                    Debug.LogWarning(
                        $"TilemapSpawner: Lever tile '{tile.name}' mapping has no prefab assigned. Skipping.", this);
                    continue;
                }

                // Convert cell position to world position (center of the cell)
                Vector3 worldPos = leverTilemap.CellToWorld(cellPos)
                                 + leverTilemap.cellSize * 0.5f;

                GameObject instance = Instantiate(entry.prefab, gridParent);
                instance.transform.position = worldPos;

                // Read lever ID from the leverIdMap
                Vector2Int key = new Vector2Int(x, y);

                if (!leverIdLookup.TryGetValue(key, out int lid) || lid <= 0)
                {
                    Debug.LogWarning(
                        $"TilemapSpawner: Lever tile at ({x},{y}) has no ID in leverIdMap. " +
                        $"Add a LeverCellIdEntry for this position in the Inspector.", this);
                    continue;
                }

                Lever lever = instance.GetComponent<Lever>();
                if (lever != null)
                {
                    lever.Initialize(lid);
                    instance.name = $"Lever_{lid}";
                    spawnedLevers.Add(lever);
                }
                else
                {
                    instance.name = $"Lever_{x}_{y}";
                    Debug.LogWarning(
                        $"TilemapSpawner: Lever prefab at ({x},{y}) is missing a Lever component.", this);
                }
            }
        }
    }

    /// <summary>
    /// Builds a Dictionary mapping cell positions to lever IDs for O(1) lookups.
    /// </summary>
    private Dictionary<Vector2Int, int> BuildLeverIdLookup()
    {
        var lookup = new Dictionary<Vector2Int, int>();

        if (leverIdMap == null) return lookup;

        for (int i = 0; i < leverIdMap.Length; i++)
        {
            LeverCellIdEntry entry = leverIdMap[i];

            if (lookup.ContainsKey(entry.cellPosition))
            {
                Debug.LogWarning(
                    $"TilemapSpawner: Duplicate lever ID at cell ({entry.cellPosition.x},{entry.cellPosition.y}). " +
                    $"Using the first entry.", this);
                continue;
            }

            lookup[entry.cellPosition] = entry.id;
        }

        return lookup;
    }

    /// <summary>
    /// Applies lever rules from <see cref="leverData"/> to all spawned levers.
    /// </summary>
    private void ApplyLeverRules()
    {
        if (leverData == null || leverData.rules == null) return;

        // Build a lever lookup by ID for quick access
        var leverLookup = new Dictionary<int, Lever>();
        for (int i = 0; i < spawnedLevers.Count; i++)
        {
            if (!leverLookup.ContainsKey(spawnedLevers[i].leverId))
                leverLookup[spawnedLevers[i].leverId] = spawnedLevers[i];
        }

        for (int i = 0; i < leverData.rules.Length; i++)
        {
            LeverRule rule = leverData.rules[i];

            if (!leverLookup.TryGetValue(rule.leverId, out Lever lever))
            {
                Debug.LogWarning(
                    $"TilemapSpawner: LeverRule lever ID {rule.leverId} not found.", this);
                continue;
            }

            lever.targetIds = rule.targetIds;
        }
    }

    // ───────── Debug / Testing ─────────

    [Header("Debug")]
    [Tooltip("Platform ID to activate when using the 'Test Activate ID' context menu.")]
    [SerializeField] private int testActivateId = 1;

    [Tooltip("Delay between each activation step in the full chain test.")]
    [SerializeField] private float testChainDelay = 0.5f;

    /// <summary>
    /// Runs the entire chain from entry point to the very end, activating each platform in sequence.
    /// </summary>
    [ContextMenu("Test Full Chain")]
    private void TestFullChain()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Chain: Only works in Play mode.");
            return;
        }

        if (chainData == null || chainData.rules == null)
        {
            Debug.LogWarning("Test Chain: No ChainReactionData assigned.");
            return;
        }

        StartCoroutine(RunFullChainTest());
    }

    private IEnumerator RunFullChainTest()
    {
        var manager = PressurePlatformManager.Instance;
        if (manager == null) yield break;

        // Find entry points (sources that are never targets)
        HashSet<int> allTargetIds = new HashSet<int>();
        for (int i = 0; i < chainData.rules.Length; i++)
        {
            if (chainData.rules[i].targetIds == null) continue;
            for (int j = 0; j < chainData.rules[i].targetIds.Length; j++)
                allTargetIds.Add(chainData.rules[i].targetIds[j]);
        }

        // Build a queue: start from entry points, follow chain
        Queue<int> activationQueue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        for (int i = 0; i < chainData.rules.Length; i++)
        {
            if (!allTargetIds.Contains(chainData.rules[i].sourceId))
                activationQueue.Enqueue(chainData.rules[i].sourceId);
        }

        int step = 0;
        while (activationQueue.Count > 0)
        {
            int currentId = activationQueue.Dequeue();
            if (visited.Contains(currentId)) continue;
            visited.Add(currentId);

            PressurePlatform platform = manager.GetById(currentId);
            if (platform == null)
            {
                Debug.LogWarning($"Test Chain: Platform ID {currentId} not found, skipping.");
                continue;
            }

            // Reveal if hidden
            if (platform.CurrentState == PressurePlatform.State.Hidden)
                platform.Reveal(animate: true);

            yield return new WaitForSeconds(testChainDelay);

            // Activate
            step++;
            Debug.Log($"Test Chain [{step}]: Activating Platform {currentId}");
            platform.Activate();

            // Enqueue targets
            ChainRule? rule = chainData.GetRuleForSource(currentId);
            if (rule.HasValue && rule.Value.targetIds != null)
            {
                for (int i = 0; i < rule.Value.targetIds.Length; i++)
                    activationQueue.Enqueue(rule.Value.targetIds[i]);
            }

            yield return new WaitForSeconds(testChainDelay);
        }

        Debug.Log($"Test Chain: Complete! {step} platforms activated.");
    }

    /// <summary>
    /// Activates the platform with the ID specified in <see cref="testActivateId"/>.
    /// </summary>
    [ContextMenu("Test Activate ID")]
    private void TestActivateById()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Activate: Only works in Play mode.");
            return;
        }

        var manager = PressurePlatformManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("Test Activate: No PressurePlatformManager found.");
            return;
        }

        PressurePlatform target = manager.GetById(testActivateId);
        if (target == null)
        {
            Debug.LogWarning($"Test Activate: Platform ID {testActivateId} not found.");
            return;
        }

        // If hidden, reveal first so Activate() can proceed
        if (target.CurrentState == PressurePlatform.State.Hidden)
        {
            Debug.Log($"Test Activate: Revealing hidden Platform {testActivateId} first.");
            target.Reveal(animate: true);
        }

        // Small delay to let reveal finish, then activate
        if (target.CurrentState == PressurePlatform.State.Revealed)
        {
            Debug.Log($"Test Activate: Activating Platform {testActivateId}");
            target.Activate();
        }
        else
        {
            Debug.LogWarning($"Test Activate: Platform {testActivateId} is already activated.");
        }
    }

    /// <summary>
    /// Deactivates the platform with the ID specified in <see cref="testActivateId"/>.
    /// This will revert its tint and hide all chain targets (recursively).
    /// </summary>
    [ContextMenu("Test Deactivate ID")]
    private void TestDeactivateById()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Deactivate: Only works in Play mode.");
            return;
        }

        var manager = PressurePlatformManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("Test Deactivate: No PressurePlatformManager found.");
            return;
        }

        PressurePlatform target = manager.GetById(testActivateId);
        if (target == null)
        {
            Debug.LogWarning($"Test Deactivate: Platform ID {testActivateId} not found.");
            return;
        }

        if (target.CurrentState == PressurePlatform.State.Activated)
        {
            Debug.Log($"Test Deactivate: Deactivating Platform {testActivateId}");
            target.Deactivate();
        }
        else
        {
            Debug.LogWarning(
                $"Test Deactivate: Platform {testActivateId} is not activated (state: {target.CurrentState}).");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TilemapSpawner))]
    public class TilemapSpawnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TilemapSpawner tilemapSpawner = (TilemapSpawner)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Test Full Chain"))
            {
                tilemapSpawner.TestFullChain();
            }
        }
    }
#endif
}

