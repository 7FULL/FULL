using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Items/Vehicles/Vehicle", order = 3)]
public class Vehicle: Item
{
    private float _speed;
    private float _acceleration;
    private float _handling;
    private float _braking;
    private float _weight;
    
    public override void Use()
    {
        throw new System.NotImplementedException();
    }
}