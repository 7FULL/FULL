using System;
using Photon.Pun;
using UnityEngine;

public abstract class Entity: MonoBehaviour
{
    private int _health = 1000;
    private int _maxHealth = 1000;
    private int _armor = 100;
    private int _maxArmor = 100;
    
    [SerializeField]
    [InspectorName("Collider")]
    private Collider _collider;
    
    private PhotonView pv;

    public PhotonView PV => pv;
    
    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public int Armor => _armor;
    public int MaxArmor => _maxArmor;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void TakeDamageRPC(int damage)
    {
        if (_armor > 0)
        {
            _armor -= damage;
            if (_armor < 0)
            {
                _health += _armor;
                _armor = 0;
            }
        }
        else
        {
            _health -= damage;
        }
        
        if (_health <= 0)
        {
            Die();
        }
    }
    
    public virtual bool TakeDamage(int damage, PhotonView playerToKill = null)
    {
        //If the entity is going to die for the damage, before communicating it to the other players, we will disable the collider
        if (_health <= damage)
        {
            _collider.enabled = false;
        }
        
        pv.RPC("TakeDamageRPC", RpcTarget.All, damage);
        
        return _health <= 0;
    }
    
    public void Heal(int amount)
    {
        _health += amount;
        if (_health > _maxHealth)
        {
            _health = _maxHealth;
        }
    }
    
    public void RestoreArmor(int amount)
    {
        _armor += amount;
        if (_armor > _maxArmor)
        {
            _armor = _maxArmor;
        }
    }
    
    public void RestoreHealthAndArmor(int healthAmount, int armorAmount)
    {
        Heal(healthAmount);
        RestoreArmor(armorAmount);
    }
    
    //TODO: This doesnt seem very secure
    public void RestoreAll()
    {
        RestoreHealthAndArmor(_maxHealth, _maxArmor);
    }
    
    public abstract void Die(bool restore = true);
    
    //Initialize
    [PunRPC]
    private void Initialize(int health, int armor)
    {
        _health = health;
        _maxHealth = health;
        _armor = armor;
        _maxArmor = armor;
    }
    
    public void InitializeRPC(int health, int armor)
    {
        pv.RPC("Initialize", RpcTarget.All, health, armor);
    }
}