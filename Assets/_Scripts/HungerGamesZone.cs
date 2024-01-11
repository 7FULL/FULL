using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class HungerGamesZone : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Reduction velocity")]
    [Tooltip("How fast does the zone reduce")]
    private float _reductionVelocity = 0.1f;
    
    [SerializeField]
    [InspectorName("Reduction time")]
    [Tooltip("Time to complete a reduction")]
    private float _reductionTime = 10f;
    
    [SerializeField]
    [InspectorName("Reduction amount")]
    [Tooltip("How many times does the zone reduce before its scale is 0")]
    private int _reductionAmount = 5;
    
    [SerializeField]
    [InspectorName("Time between reductions")]
    private float _timeBetweenReductions = 10f;
    
    [SerializeField]
    [InspectorName("Damage per second")]
    private int _damagePerSecond = 10;
    
    private List<Player> _players = new List<Player>();
    
    private bool _isReducing = false;

    private float _timeSinceLastDamage = 0f;
    private int _reductions = 0;
    
    private Vector3 _initialScale;

    private void Start()
    {
        _initialScale = transform.localScale;

        _reductionAmount++;
    }
    
    public void StartZone()
    {
        StartCoroutine(ReduceZone());
    }
    
    private IEnumerator ReduceZone()
    {
        //We reduce the zone once and then wait for the time between reductions
        while (_reductions < _reductionAmount)
        {
            _isReducing = true;
            
            float reductionAmount = _initialScale.x / _reductionAmount;
            
            //We reduce the zone until it reaches the desired scale using DOTween
            transform.DOScaleX(transform.localScale.x - reductionAmount, _reductionTime / _reductionVelocity).SetEase(Ease.Linear);
            transform.DOScaleZ(transform.localScale.z - reductionAmount, _reductionTime / _reductionVelocity).SetEase(Ease.Linear);
            
            _reductions++;
            _isReducing = false;
            yield return new WaitForSeconds(_timeBetweenReductions + _reductionTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponentInParent<Player>();
            _players.Remove(player);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponentInParent<Player>();
            _players.Add(player);
        }
    }

    private void FixedUpdate()
    {
        //Every 50 iterations of the fixed update is 1 second
        _timeSinceLastDamage += 1;
        if (_players.Count > 0 && _timeSinceLastDamage >= 50)
        {
            foreach (Player player in _players)
            {
                player.TakeDamage(Mathf.RoundToInt(_damagePerSecond));
            }
            
            _timeSinceLastDamage = 0f;
        }
    }
}
