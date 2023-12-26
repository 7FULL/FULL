using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Items/Vehicles/Vehicle", order = 3)]
public class Vehicle: Item
{
    private float _speed;
    private float _acceleration;
    private float _handling;
    private float _braking;
    private float _weight;
    
    //Constructor based on VehicleScriptableObject
    public Vehicle(VehicleScriptableObject vehicleScriptableObject)
    {
        _speed = vehicleScriptableObject.speed;
        _acceleration = vehicleScriptableObject.acceleration;
        _handling = vehicleScriptableObject.handling;
        _braking = vehicleScriptableObject.braking;
        _weight = vehicleScriptableObject.weight;
    }
    
    public Vehicle(){}
}