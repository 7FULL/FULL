using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class MonsterHorrorMaze : Entity
{
    [SerializeField]
    [InspectorName("Animator")]
    private Animator animator;
    
    [SerializeField]
    [InspectorName("")]
    
    private List<Player> players;
    
    private NavMeshAgent agent;
    
    [SerializeField]
    [InspectorName("Random points")]
    private Transform[] randomPoints;
    
    [Title("Monster data")]
    
    [SerializeField]
    [InspectorName("Speed")]
    private float speed;
    
    [SerializeField]
    [InspectorName("Damage")]
    private int damage;
    
    [SerializeField]
    [InspectorName("Attack cooldown")]
    private float attackCooldown;
    
    private float attackTime;
    
    [SerializeField]
    [InspectorName("Attack range")]
    private float attackRange;
    
    [SerializeField]
    [InspectorName("Forward detection range")]
    private float forwardDetectionRange;
    
    private Transform target;
    
    private bool followingPlayer = false;
    
    private int index = 0;
    
    private bool waiting = false;
    
    [SerializeField]
    [InspectorName("Scream audio")]
    private AudioClip screamAudio;
    
    [SerializeField]
    [InspectorName("Time to unfollow")]
    private float timeToUnfollow;
    
    private float timeToUnfollowAux;
    
    [SerializeField]
    [InspectorName("Distance to detect player")]
    private float distanceToDetectPlayer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        
        if (randomPoints.Length > 0)
        {
            agent.SetDestination(randomPoints[index].position);
        }
        
        attackTime = Time.time;
        
        timeToUnfollowAux = timeToUnfollow;
    }
    
    public void AddPlayer(Player player)
    {
        if (players == null)
        {
            players = new List<Player>();
        }
        
        players.Add(player);
    }

    public override void Die(bool restore = true)
    {
        animator.SetTrigger("Die");
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }
        
        if (followingPlayer)
        {
            timeToUnfollowAux -= Time.deltaTime;
            
            if (timeToUnfollowAux <= 0)
            {
                followingPlayer = false;
                timeToUnfollowAux = timeToUnfollow;
            }
        }
        
        //Go to random points and if we detect a player, follow it and attack it
        if (players != null && players.Count > 0)
        {
            // If we are following a player
            if (followingPlayer)
            {
                if (Vector3.Distance(transform.position, target.position) <= attackRange)
                {
                    Attack();
                }
                else
                {
                    agent.SetDestination(target.position);
                }
            }
            else
            {
                
                bool aux = false;  
                // We check if we are close to a player
                // If the player is forward to us and in range, we follow it
                foreach (Player player in players)
                {
                    //Check for nulls
                    if (player == null)
                    {
                        continue;
                    }
                    if (Vector3.Distance(transform.position, player.CharacterCamera.Camera.transform.position) <= forwardDetectionRange)
                    {
                        Vector3 direction = player.CharacterCamera.Camera.transform.position - transform.position;
                        float angle = Vector3.Angle(direction, transform.forward);
                        
                        if (angle < 45)
                        {
                            //We send a raycast to check if there is something between the player and the monster
                            RaycastHit hit;
                            
                            if (Physics.Raycast(transform.position, direction, out hit, forwardDetectionRange))
                            {
                                if (hit.collider.CompareTag("Player"))
                                {
                                    target = player.CharacterCamera.Camera.transform;
                                    followingPlayer = true;
                            
                                    agent.isStopped = true;
                            
                                    animator.SetTrigger("Alert");
                            
                                    AudioManager.Instance.PlaySound(screamAudio, transform.position);
                            
                                    Invoke("StartFollowingPlayer", 3);
                            
                                    aux = true;
                            
                                    break;
                                }
                            }
                        }
                    }
                }
                
                // If we didnt detect a player in front of us, we check if we are close to a player
                if (!aux)
                {
                    foreach (Player player in players)
                    {
                        if (Vector3.Distance(transform.position, player.CharacterCamera.Camera.transform.position) <= distanceToDetectPlayer)
                        {
                            //We send a raycast to check if there is something between the player and the monster
                            RaycastHit hit;

                            if (Physics.Raycast(transform.position,
                                    player.CharacterCamera.Camera.transform.position - transform.position, out hit,
                                    distanceToDetectPlayer))
                            {
                                if (hit.collider.CompareTag("Player"))
                                {
                                    target = player.CharacterCamera.Camera.transform;
                                    followingPlayer = true;

                                    agent.isStopped = true;

                                    animator.SetTrigger("Alert");

                                    AudioManager.Instance.PlaySound(screamAudio, transform.position);

                                    Invoke("StartFollowingPlayer", 3);

                                    aux = true;

                                    break;
                                }
                            }
                        }
                    }
                }

                if (!aux)
                {
                    if (!waiting)
                    {
                        if (randomPoints.Length > 0)
                        {
                            if (Vector3.Distance(transform.position, randomPoints[index].position) <= 2.5f)
                            {
                                index++;
                    
                                if (index >= randomPoints.Length)
                                {
                                    index = 0;
                                }
                            }
                
                            // We wait a bit before moving to the next point
                            waiting = true;
                            Invoke("WaitForNextPoint", 5);
                        }
                    }
                }
            }
        }
        
        //Animation
        //We check if we are moving but if we are following we increase the speed
        if (agent.velocity.magnitude > 0)
        {
            animator.SetBool("Moving", true);
            
            if (followingPlayer)
            {
                agent.speed = speed * 2.5f;
            }
            else
            {
                agent.speed = speed;
            }
        }
        else
        {
            animator.SetBool("Moving", false);
        }
    }
    
    private void StartFollowingPlayer()
    {
        agent.isStopped = false;
        
        agent.speed = speed * 1.5f;
    }
    
    private void WaitForNextPoint()
    {
        waiting = false;

        if (!followingPlayer)
        {
            agent.SetDestination(randomPoints[index].position);
        }
    }
    
    private void Attack()
    {
        if (Time.time > attackTime)
        {
            attackTime = Time.time + attackCooldown;
            
            foreach (Player player in players)
            {
                if (Vector3.Distance(transform.position, player.CharacterCamera.Camera.transform.position) <= attackRange)
                {
                    player.TakeDamage(damage, player.PV);
                    animator.SetTrigger("Attack");
                }
            }
        }
    }

    public void UpdatePlayers()
    {
        // We update all the players just in case we killed one
        players = new List<Player>(FindObjectsOfType<Player>());
    }
}
