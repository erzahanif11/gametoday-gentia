using UnityEngine;

[CreateAssetMenu(fileName = "LevelSpiritInfo", menuName = "Scriptable Objects/LevelSpiritInfo")]
public class LevelSpiritInfo : ScriptableObject
{
    public int spiritCount = 0;
    public Vector2 minSpawnPosition;
    public Vector2 maxSpawnPosition;
}
