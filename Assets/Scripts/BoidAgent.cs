using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoidAgent : MonoBehaviour
{
    [HideInInspector] public Vector2 currentVelocity;
    Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = this.GetComponent<Collider2D>();
    }

    public void Move(Vector2 velocity)
    {
        this.transform.up = velocity;
        this.transform.position += (Vector3)velocity * Time.deltaTime;
    }
}
