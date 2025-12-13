using UnityEngine;

public class EvolutionModelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject stage1;
    [SerializeField] private GameObject stage2;
    [SerializeField] private GameObject stage3;

    public void SetStage(int stage)
    {
        stage1.SetActive(stage == 1);
        stage2.SetActive(stage == 2);
        stage3.SetActive(stage == 3);

        Debug.Log($"EvolutionModelSwitcher → Stage {stage}");
    }

    public Animator GetActiveAnimator()
    {
        if (stage1.activeSelf) return stage1.GetComponent<Animator>();
        if (stage2.activeSelf) return stage2.GetComponent<Animator>();
        if (stage3.activeSelf) return stage3.GetComponent<Animator>();
        return null;
    }
}
