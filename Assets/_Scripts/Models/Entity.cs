using UnityEngine;

public abstract class Entity: MonoBehaviour
{
    private int _health;
    private int _maxHealth;
    private int _armor;
    private int _maxArmor;
    
    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public int Armor => _armor;
    public int MaxArmor => _maxArmor;
    
    public void TakeDamage(int damage)
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
        
        Debug.Log($"Health: {_health}, Armor: {_armor}");
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
    
    public abstract void Die();
    
    //Initialize
    public void Initialize(int health, int armor)
    {
        _health = health;
        _maxHealth = health;
        _armor = armor;
        _maxArmor = armor;
    }
}