using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    public GameObject indicator;

    public void toggleIndicator(bool isActive)
    {
        if (indicator != null)
        {
            indicator.SetActive(isActive);
        }
    }
}
