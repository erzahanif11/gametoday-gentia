using UnityEngine;

/// <summary>
/// ScriptableObject that defines chain-reaction rules for a level.
/// Each rule maps a source platform ID to one or more target platform IDs.
/// </summary>
[CreateAssetMenu(fileName = "NewChainReaction", menuName = "Grid/Chain Reaction Data")]
public class ChainReactionData : ScriptableObject
{
    [Tooltip("List of chain reaction rules for this level.")]
    public ChainRule[] rules;

    /// <summary>
    /// Finds the rule for a given source platform ID.
    /// Returns null if no rule is defined for that source.
    /// </summary>
    public ChainRule? GetRuleForSource(int sourceId)
    {
        if (rules == null) return null;

        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].sourceId == sourceId)
                return rules[i];
        }
        return null;
    }
}

/// <summary>
/// A single chain-reaction rule: when sourceId is activated,
/// reveal all targetIds after a delay.
/// </summary>
[System.Serializable]
public struct ChainRule
{
    [Tooltip("ID of the platform that triggers this rule when stepped on.")]
    public int sourceId;

    [Tooltip("IDs of platforms to reveal when this rule fires.")]
    public int[] targetIds;

    [Tooltip("Delay (seconds) before targets are revealed.")]
    public float delay;

    [Tooltip("If true, the SOURCE platform stays visible permanently once activated. " +
             "It will not disappear when its parent platform is deactivated.")]
    public bool persistent;
}
