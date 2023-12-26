using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Items/Vehicles/Vehicle", order = 3)]
public class VehicleData: ItemData
{
    public float speed;
    public float acceleration;
    public float handling;
    public float braking;
    public float weight;
}