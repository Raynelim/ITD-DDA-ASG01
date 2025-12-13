using UnityEngine;

public class DevSettings : MonoBehaviour
{
    public static DevSettings Instance;

    [Header("Developer Overrides")]
    public bool devModeEnabled = true;

    [Header("Starting Batteries (Dev Mode Only)")]
    public int smallBattery = 999;
    public int mediumBattery = 999;
    public int largeBattery = 999;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
