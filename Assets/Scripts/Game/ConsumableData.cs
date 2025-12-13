using UnityEngine;

[CreateAssetMenu(menuName = "AR Pet/Consumable")]
public class ConsumableData : ScriptableObject
{
    public string consumableName;

    [Header("UI Material")]
    public Material uiMaterial;

    [Header("World Prefab")]
    public GameObject worldPrefab;

    [Header("Battery Cost")]
    public string batteryType; // "small", "medium", or "large"
    public int batteryCost = 1; // How many batteries required

    [Header("Pet Effects")]
    public int xpBoost = 0;
    public int happinessBoost = 0;
}
