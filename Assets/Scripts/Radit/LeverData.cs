using UnityEngine;

/// <summary>
/// ScriptableObject that defines lever-to-platform rules for a level.
/// Each rule maps a lever ID to one or more target platform IDs
/// that should be revealed/hidden when the lever is toggled.
/// </summary>
[CreateAssetMenu(fileName = "NewLeverData", menuName = "Grid/Lever Data")]
public class LeverData : ScriptableObject
{
    [Tooltip("List of lever rules for this level.")]
    public LeverRule[] rules;

    /// <summary>
    /// Finds the rule for a given lever ID.
    /// Returns null if no rule is defined for that lever.
    /// </summary>
    public LeverRule? GetRuleForLever(int leverId)
    {
        if (rules == null) return null;

        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].leverId == leverId)
                return rules[i];
        }
        return null;
    }
}

/// <summary>
/// A single lever rule: when the lever with leverId is toggled ON,
/// reveal all targetIds. When toggled OFF, hide them.
/// </summary>
[System.Serializable]
public struct LeverRule
{
    [Tooltip("ID of the lever that triggers this rule.")]
    public int leverId;

    [Tooltip("IDs of pressure platforms to reveal when ON (and hide when OFF).")]
    public int[] targetIds;

    [Tooltip("IDs of pressure platforms to reveal when OFF (and hide when ON).")]
    public int[] offTargetIds;
}
