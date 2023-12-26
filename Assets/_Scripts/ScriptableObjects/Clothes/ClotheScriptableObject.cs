using UnityEngine;

[CreateAssetMenu(fileName = "Clothe", menuName = "Items/Clothes/Clothe", order = 1)]
public class ClotheScriptableObject: ItemScriptableObject
{
    public override Item ToModel()
    {
        //TODO: This is wrong we should make this class abstract
        return new Item();
    }
}