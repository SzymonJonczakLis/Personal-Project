using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
        elements.Sort((x, y) => x.priority.CompareTo(y.priority)); // Sort by priority
    }

    public (T, float) Dequeue()
    {
        if (elements.Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        T item = elements[0].item;
        float prio = elements[0].priority;
        elements.RemoveAt(0);
        return (item, prio);
    }
}

public class ChasePlayer : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private float fieldOfView = 20f, searchRadius = 5f;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float groundDrag;
    [SerializeField]
    private LayerMask whatToSee;


    private Rigidbody rb;
    public RaycastHit sight;
    private Vector3 moveDirection;
    
    private bool seeingPlayer;
    private bool searchingReady = true;
    private float searchingCooldown = 0.5f; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = groundDrag;
    }

    void FixedUpdate() {
        // Move();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C)) {
            SearchForClosestPath();
        }
        if(searchingReady) {
            SeeForPlayer();

            Invoke("ResetSearchingCooldown", searchingCooldown);
        }

        SpeedControl();
    }

    void SeeForPlayer() {
        searchingReady = false;
        if(Physics.Raycast(transform.position, player.position - transform.position, out sight, fieldOfView, whatToSee)) {
            if(sight.transform == player) {
                seeingPlayer = true;
            } else {
                seeingPlayer = false;
            }
        } else {
            seeingPlayer = false;
        }

        if(seeingPlayer) {
            Debug.DrawRay(transform.position, player.position - transform.position, Color.green, 1f);
            moveDirection = player.position - transform.position;
            moveDirection.y = 0;
        } else {
            Debug.DrawRay(transform.position, player.position - transform.position, Color.red, 1f);
            // moveDirection = SearchForClosestPath() - transform.position;
            // SearchForClosestPath();
        }
    }

    private Vector3 SearchForClosestPath() {
        // Vector3 closestPoint = transform.position;
        // float distance = (player.position - transform.position).magnitude;
        // for(int i = 0; i < 8; i++) {
        //     float angle = i*45f;
        //     angle *= Mathf.Deg2Rad;
        //     Vector3 direction;
        //     direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized * searchRadius;

        //     Debug.DrawRay(transform.position, direction, Color.yellow, 0.5f);
        //     if(!Physics.Raycast(transform.position, direction, out sight, searchRadius, whatToSee)) {
        //         Vector3 endPoint = transform.position + direction;
        //         if((player.position - endPoint).magnitude < distance) {
        //             distance = (player.position - endPoint).magnitude;
        //             closestPoint = endPoint;
        //         }
        //         // Debug.Log(endPoint);
        //     }
        // }

        HashSet<Vector3> visited = new HashSet<Vector3>();

        PriorityQueue<Vector3> priorityQ = new PriorityQueue<Vector3>();
        float distance = (player.position - transform.position).magnitude;
        priorityQ.Enqueue(transform.position, distance);
        Debug.Log(priorityQ.Count);

        float maxIterations = 1000;
        float iterations = 0;

        // A* algorithm
        while(priorityQ.Count > 0) {
            if(iterations++ > maxIterations) {
                Debug.Log("Maks iteracje");
                break;
            }
            
            (Vector3, float) queueTop = priorityQ.Dequeue();

            if(visited.Contains(queueTop.Item1))
                continue;

            visited.Add(queueTop.Item1);

            // Debug.Log(queueTop);
            if(Physics.Raycast(queueTop.Item1, player.position - queueTop.Item1, out sight, fieldOfView, whatToSee)) {
                if(sight.transform == player) {
                    Debug.Log("Tutaj git");
                    Debug.DrawLine(queueTop.Item1, player.position, Color.cyan, 0.5f);
                    break;
                }
            }
            
            // 8 directions
            for(int i = 0; i < 8; i++) {
                float angle = i * 45f;
                angle *= Mathf.Deg2Rad;
                Vector3 direction;
                direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized * searchRadius;
                Vector3 point;

                if(Physics.Raycast(queueTop.Item1, direction, out sight, searchRadius, whatToSee)) {
                    // here i will add something to jump

                    // bring our point back so it wont be in other gameObject
                    // point = sight.transform.position - direction * 0.1f;
                    
                    // calculate distance between point and player and get this on pq with distance as priority
                    // distance = (point - player.position).magnitude;
                } else {
                    // calculate distance between point and player and get this on pq with distance as priority
                    point = queueTop.Item1 + direction;
                    // if((player.position - point).magnitude > fieldOfView)
                    //     continue;
                    distance = queueTop.Item2 + direction.magnitude;
                    distance += (point - player.position).magnitude;
                    priorityQ.Enqueue(point, distance);
                    Debug.DrawLine(queueTop.Item1, point, Color.yellow, 1f);
                }
            }
        }
        return new Vector3(0, 0, 0);
    }

    void Move() {
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    void SpeedControl() {
        if(rb.linearVelocity.magnitude > moveSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
    }

    void ResetSearchingCooldown() {
        searchingReady = true;
    }
}
