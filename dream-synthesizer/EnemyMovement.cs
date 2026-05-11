using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed;
    private GameObject player;
    public bool isChasing;
    public float chaseDistance;
    public Transform[] patrolPoints;
    public int patrolDestination;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void FixedUpdate()
    {
        // stop chasing player if player is twice the chase distance away
        if (Vector2.Distance(transform.position, player.transform.position) > (chaseDistance * 2))
        {
            isChasing = false;
        }

        // fix where enemy is facing depending on it's path
        if (patrolDestination == 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (patrolDestination == 1)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (isChasing)
        {
            if (transform.position.x > player.transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (transform.position.x < player.transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            // chase player if within chase distance
            if (Vector2.Distance(transform.position, player.transform.position) < chaseDistance)
            {
                isChasing = true;
            }

            // otherwise, move in normal patrol route
            if (patrolDestination == 0)
            {
                transform.position = Vector2.MoveTowards(transform.position, patrolPoints[0].position, moveSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, patrolPoints[0].position) < .2f)
                {
                    patrolDestination = 1;
                }
            }
            else if (patrolDestination == 1)
            {
                transform.position = Vector2.MoveTowards(transform.position, patrolPoints[1].position, moveSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, patrolPoints[1].position) < .2f)
                {
                    patrolDestination = 0;
                }
            }
        }
    }
}
