using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton registry for all <see cref="PressurePlatform"/> instances.
/// Platforms register themselves on Awake and can be looked up by ID.
/// </summary>
public class PressurePlatformManager : MonoBehaviour
{
    public static PressurePlatformManager Instance { get; private set; }

    private readonly Dictionary<int, PressurePlatform> registry = new Dictionary<int, PressurePlatform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Register a platform so it can be found by ID.</summary>
    public void Register(PressurePlatform platform)
    {
        if (platform == null) return;

        if (registry.ContainsKey(platform.platformId))
        {
            Debug.LogWarning(
                $"PressurePlatformManager: Duplicate ID {platform.platformId} — " +
                $"overwriting with {platform.name}.", platform);
        }

        registry[platform.platformId] = platform;
    }

    /// <summary>Remove a platform from the registry.</summary>
    public void Unregister(PressurePlatform platform)
    {
        if (platform == null) return;
        if (registry.ContainsKey(platform.platformId) && registry[platform.platformId] == platform)
        {
            registry.Remove(platform.platformId);
        }
    }

    /// <summary>Returns the platform with the given ID, or null.</summary>
    public PressurePlatform GetById(int id)
    {
        registry.TryGetValue(id, out PressurePlatform p);
        return p;
    }

    /// <summary>Returns an array of platforms matching the given IDs (nulls excluded).</summary>
    public List<PressurePlatform> GetByIds(int[] ids)
    {
        var result = new List<PressurePlatform>();
        if (ids == null) return result;

        for (int i = 0; i < ids.Length; i++)
        {
            PressurePlatform p = GetById(ids[i]);
            if (p != null) result.Add(p);
        }
        return result;
    }

    /// <summary>Clears the entire registry (e.g. on level reload).</summary>
    public void ClearAll()
    {
        registry.Clear();
    }

    /// <summary>Returns the platform at a specific world position, or null.</summary>
    public PressurePlatform GetByPosition(Vector2 position)
    {
        foreach (var platform in registry.Values)
        {
            if (Vector2.Distance(platform.transform.position, position) < 0.1f)
            {
                return platform;
            }
        }
        return null;
    }
}
