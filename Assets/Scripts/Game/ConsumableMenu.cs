using UnityEngine;

public class ConsumableMenu : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    private bool open = false;

    public void ToggleMenu()
    {
        open = !open;
        menuPanel.SetActive(open);
    }
}
