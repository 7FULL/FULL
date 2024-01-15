using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Util", order = 0)]
public class UtilData: ItemData
{
    [Title("Util data")]
    
    [InspectorName("Restore health")]
    public bool restoreHealth;
    
    [InspectorName("Restore shield")]
    public bool restoreShield;
    
    [InspectorName("How much health to restore")]
    [ShowIf("restoreHealth")]
    public int healthToRestore;
    
    [InspectorName("How much shield to restore")]
    [ShowIf("restoreShield")]
    public int shieldToRestore;
}