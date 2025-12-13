using UnityEngine;

public class PetVisualScaler : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;

    public void SetScale(float scale)
    {
        if (visualRoot == null)
        {
            Debug.LogError("PetVisualScaler: VisualRoot not assigned!");
            return;
        }

        visualRoot.localScale = Vector3.one * scale;
    }
}
