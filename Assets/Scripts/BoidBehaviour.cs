using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoidBehaviour : ScriptableObject
{
    public abstract Vector2 CalculateBoidMovement(BoidAgent agent, List<Transform> context, BoidManager manager);
}
