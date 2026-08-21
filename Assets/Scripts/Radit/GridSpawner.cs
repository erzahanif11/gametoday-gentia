using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads a <see cref="GridLayoutSO"/> and spawns the corresponding platform prefab
/// for every non-empty cell.
/// </summary>
public class GridSpawner : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private GridLayoutSO gridLayout;

    [Header("Chain Reaction")]
    [Tooltip("Optional. Defines which pressure platforms trigger which targets.")]
    [SerializeField] private ChainReactionData chainData;

    [Header("Prefabs")]
    [Tooltip("Assign one prefab per PlatformType. Index must match the enum value.")]
    [SerializeField] private PlatformPrefabEntry[] platformPrefabs;

    [Header("Spacing")]
    [SerializeField] private float cellSize = 1f;

    // Keeps spawned objects so we can clear / respawn at runtime.
    private Transform gridParent;

    // All spawned pressure platforms, used for deferred initialization.
    private List<PressurePlatform> spawnedPressurePlatforms = new List<PressurePlatform>();

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
        spawnedPressurePlatforms.Clear();

        if (gridLayout == null)
        {
            Debug.LogWarning("GridSpawner: No GridLayoutSO assigned.", this);
            return;
        }

        // Parent object keeps the hierarchy tidy
        gridParent = new GameObject("Grid").transform;
        gridParent.SetParent(transform);
        gridParent.localPosition = Vector3.zero;

        // Ensure a PressurePlatformManager exists in the scene
        EnsurePressurePlatformManager();

        // ── Phase 1: Spawn all cells and register pressure platforms ──
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

                // Initialize pressure platforms with custom ID from grid
                if (type == PlatformType.Pressure)
                {
                    PressurePlatform pp = instance.GetComponent<PressurePlatform>();
                    if (pp != null)
                    {
                        int customId = gridLayout.GetCellId(x, y);
                        pp.Initialize(customId); // sets ID + registers with manager
                        instance.name = $"Pressure_{customId}";
                        spawnedPressurePlatforms.Add(pp);
                    }
                }
            }
        }

        // ── Phase 2: Apply chain rules (now all platforms are registered) ──
        if (chainData != null)
        {
            ApplyChainRules();
        }

        // ── Phase 3: Apply initial visibility state AFTER rules are set ──
        for (int i = 0; i < spawnedPressurePlatforms.Count; i++)
        {
            spawnedPressurePlatforms[i].InitializeState();
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
                    $"GridSpawner: ChainRule source ID {rule.sourceId} not found.", this);
                continue;
            }

            source.targetIds = rule.targetIds;
            source.triggerDelay = rule.delay;

            // If this source is never a target of another rule, it's an entry point
            if (!allTargetIds.Contains(rule.sourceId))
            {
                source.startsRevealed = true;
            }
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
