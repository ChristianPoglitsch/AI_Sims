using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AiSims
{
    public class EnemyMovement : MonoBehaviour
    {
        private Transform currentTarget;
        public Transform player;
        private NavMeshAgent navMeshAgent;
        public bool isFollowingPlayer = true;
        public List<Transform> targets = new List<Transform>();
        private float RunAwayTimer = 0f;
        private float RunAwayInterval = 20f;
        private float navSpeed = 0f;

        public string endAnimation;
        public string target;

        private Animator animator;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Start()
        {
            animator = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>(); // Use Stop() and Resume()

            navSpeed = navMeshAgent.speed;

            animator.SetBool("isWalking", true);
            animator.SetBool(endAnimation, false);

            chooseTarget();
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
            //isFollowingPlayer = true;
            navMeshAgent.speed = navSpeed;
        }

        public void chooseTarget()
        {
            int newTargetIndex = Random.Range(0, targets.Count);
            Transform NewTarget = targets[newTargetIndex];
            if (NewTarget == currentTarget)
            {
                chooseTarget();
                StartChasingTarget();
            }
            else
            {
                currentTarget = NewTarget;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(target) && isFollowingPlayer == true)
            {
                navMeshAgent.speed = 0;
                animator.SetBool(endAnimation, true);
                animator.SetBool("isWalking", false);
            }
            else
            {
                chooseTarget();
            }
        }
    }
}