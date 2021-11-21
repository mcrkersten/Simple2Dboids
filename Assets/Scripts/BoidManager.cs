using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public BoidAgent boidAgentPrefab;
    private List<BoidAgent> instantiatedAgents = new List<BoidAgent>();

    [Range(10, 500)]
    public int boidCount = 250;
    const float AgentDensity = .8f;

    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(1f, 10f)]
    public float visionRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = .5f;

    [Space, Header("Weights")]
    [Range(0f, 5f)]
    public float avoidanceWeight = 2f;
    [Range(0f, 5f)]
    public float allignmentWeight = 1f;
    [Range(0f, 5f)]
    public float cohesionWeight = 3f;
    [Range(0f, 1f)]
    public float stayInCircleWeight = .1f;

    [Space, Header("Movement area")]
    public Vector2 circleCenter;
    public float circleRadius;

    private float squareMaxSpeed;
    private float squareVisionRadius;
    private float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }

    void Start()
    {
        //UTILITY
        {
            squareMaxSpeed = maxSpeed * maxSpeed;
            squareVisionRadius = visionRadius * visionRadius;
            squareAvoidanceRadius = squareVisionRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
        }

        for (int i = 0; i < boidCount; i++)
        {
            BoidAgent newBoidAgent = Instantiate(
                boidAgentPrefab,
                Random.insideUnitCircle * boidCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * Random.Range(0f,360f)),
                this.transform
                );
            newBoidAgent.name = "Boid_" + i;
            instantiatedAgents.Add(newBoidAgent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (BoidAgent agent in instantiatedAgents)
        {
            Vector2 move = CalculateMovement(agent);
            agent.Move(move);
        }
    }

    private List<Transform> GetNearbyObjects(BoidAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextCollider = Physics2D.OverlapCircleAll(agent.transform.position, visionRadius);
        foreach (Collider2D collider in contextCollider)
            if(collider != agent.AgentCollider)
                context.Add(collider.transform);
        return context;
    }

    private Vector2 CalculateMovement(BoidAgent agent)
    {
        Vector2 move = Vector2.zero;
        List<Transform> context = GetNearbyObjects(agent);

        move += CalculateAvoidance(agent, context) * avoidanceWeight;
        move += CalculateAllignment(agent, context) * allignmentWeight;
        move += CalculateCohesion(agent, context) * cohesionWeight;
        move += StayInsideSphere(agent) * stayInCircleWeight;

        move *= driveFactor;
        if (move.sqrMagnitude > squareMaxSpeed)
            move = move.normalized * maxSpeed;

        return move;
    }

    private Vector2 CalculateAvoidance(BoidAgent agent, List<Transform> context)
    {
        //if no context, return no adjustment
        if (context.Count == 0)
            return Vector2.zero;

        //Add context and average out
        Vector2 avoidanceMove = Vector2.zero;
        int nAvoid = 0;
        foreach (Transform item in context)
        {
            if (Vector2.SqrMagnitude(item.position - agent.transform.position) < squareAvoidanceRadius)
            {
                nAvoid++;
                avoidanceMove += (Vector2)(agent.transform.position - item.position);
            }
        }

        if (nAvoid > 0)
            avoidanceMove /= nAvoid;

        if (avoidanceMove.sqrMagnitude > avoidanceWeight * avoidanceWeight)
        {
            avoidanceMove.Normalize();
            avoidanceMove *= avoidanceWeight;
        }

        return avoidanceMove;
    }

    private Vector2 CalculateAllignment(BoidAgent agent, List<Transform> context)
    {
        //if no context, maintain current heading
        if (context.Count == 0)
            return agent.transform.up;

        //Add context and average out
        Vector2 allignmentMove = Vector2.zero;
        foreach (Transform item in context)
            allignmentMove += (Vector2)item.transform.up;
        allignmentMove /= context.Count;

        if (allignmentMove.sqrMagnitude > allignmentWeight * allignmentWeight)
        {
            allignmentMove.Normalize();
            allignmentMove *= allignmentWeight;
        }

        return allignmentMove;
    }

    private Vector2 CalculateCohesion(BoidAgent agent, List<Transform> context)
    {
        //if no context, return no adjustment
        if (context.Count == 0)
            return Vector2.zero;

        //Add context and average out
        Vector2 cohesionMove = Vector2.zero;
        foreach (Transform item in context)
            cohesionMove += (Vector2)item.position;
        cohesionMove /= context.Count;

        //offset from boid
        cohesionMove -= (Vector2)agent.transform.position;
        cohesionMove = Vector2.SmoothDamp(agent.transform.up, cohesionMove, ref agent.currentVelocity, .5f);

        if (cohesionMove.sqrMagnitude > cohesionWeight * cohesionWeight)
        {
            cohesionMove.Normalize();
            cohesionMove *= cohesionWeight;
        }

        return cohesionMove;
    }

    private Vector2 StayInsideSphere(BoidAgent agent)
    {
        Vector2 centerOffset = circleCenter - (Vector2)agent.transform.position;
        float t = centerOffset.magnitude / circleRadius;
        if (t < .9f)
            return Vector2.zero;
        return centerOffset * t * t;
    }
}
