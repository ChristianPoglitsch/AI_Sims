using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private Transform currentTarget;
    public Transform player;
    private NavMeshAgent navMeshAgent;
    public bool isFollowingPlayer = true;
    public List<Transform> target = new List<Transform>();
    private float RunAwayTimer = 0f;
    private float RunAwayInterval = 20f;

    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        animator.SetBool("isWalking", true);
        animator.SetBool("isStanding", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && isFollowingPlayer == true)
        {
            navMeshAgent.SetDestination(player.position);
        }
        else
        {
            navMeshAgent.SetDestination(currentTarget.position);
        }

        if (isFollowingPlayer == false)
        {
            RunAwayTimer = RunAwayTimer + Time.deltaTime;
            if (RunAwayTimer > RunAwayInterval)
            {
                RunAwayTimer = 0;
                StartChasingTarget();
            }
        }
    }

    public void StartChasingTarget()
    {
        isFollowingPlayer = true;
        navMeshAgent.speed = 3.5f;
    }

    public void chooseTarget()
    {
        int newTargetIndex = Random.Range(0, target.Count);
        Transform NewTarget = target[newTargetIndex];
        if (NewTarget == currentTarget)
        {
            chooseTarget();
        }
        else { currentTarget = NewTarget; }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPCTarget") && isFollowingPlayer == true)
        {
            //chooseTarget();
            navMeshAgent.speed = 0;
            animator.SetBool("isStanding", true);
            animator.SetBool("isWalking", false);
        }
    }
}
