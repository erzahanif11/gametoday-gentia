using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Scriptable Objects/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public LevelData GetLevel(int levelId)
    {
        return levels.Find(l => l.levelId == levelId);
    }

    public int GetLevelCount()
    {
        return levels.Count;
    }
}
