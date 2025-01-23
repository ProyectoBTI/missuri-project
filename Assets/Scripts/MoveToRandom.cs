using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class MoveToRandom : MonoBehaviour
{
    public Animator animator;
    NavMeshAgent agent;
    public GameObject target;
    public bool isStopped;
    public Vector3 distance;
    public float distAbs;
    public float distAbsX;
    public float distAbsZ;
    public bool isWalking;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        isStopped = agent.isStopped;
        distance = agent.destination - transform.position;
        distAbsX = Math.Abs(distance.x);
        distAbsZ = Math.Abs(distance.z);
        distAbs = distAbsX + distAbsZ;
        if (distAbs > 1)
        { // SE ESTA MOVIENDO
            agent.isStopped = false;
            animator.SetBool("isWalking", true);
            animator.SetBool("isStopped", false);
        }
        else
        { // ESTA QUIETO
            animator.SetBool("isWalking", false);
            animator.SetBool("isStopped", true);
            agent.isStopped = true;
        }
        agent.SetDestination(target.transform.position);
    }
}
