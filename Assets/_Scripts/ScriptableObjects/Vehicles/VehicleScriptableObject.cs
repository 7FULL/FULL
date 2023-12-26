using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Items/Vehicles/Vehicle", order = 3)]
public class VehicleScriptableObject: ItemScriptableObject
{
    public float speed;
    public float acceleration;
    public float handling;
    public float braking;
    public float weight;

    public override Item ToModel()
    {
        //TODO: This is wrong we should make this class abstract
        return new Vehicle(this);
    }
}