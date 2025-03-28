using System.Collections.Generic;
using UnityEngine;
public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance { get; private set; }
    private List<Agent> agents = new List<Agent>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterAgent(Agent agent)
    {
        if (!agents.Contains(agent))
        {
            agents.Add(agent);
        }
    }

    public void UnregisterAgent(Agent agent)
    {
        if (agents.Contains(agent))
        {
            agents.Remove(agent);
        }
    }

    public List<Agent> GetAgents()
    {
        return agents;
    }

    // Separation
    public Vector3 Separation(Agent currentAgent, float separationRadius, float maxSpeed, Vector3 currentVelocity)
    {
        Vector3 separationForce = Vector3.zero;
        int neighborCount = 0;

        foreach (var neighbor in agents)
        {
            if (neighbor != currentAgent)
            {
                Vector3 toAgent = currentAgent.transform.position - neighbor.transform.position;
                toAgent.y = 0; // Evitar movimiento en Y
                float distance = toAgent.magnitude;

                if (distance < separationRadius && distance > 0)
                {
                    separationForce += toAgent.normalized / distance;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
            separationForce = separationForce.normalized * maxSpeed - currentVelocity;
        }

        separationForce.y = 0; // Restringir fuerza en Y
        return separationForce;
    }

    // Cohesion
    public Vector3 Cohesion(Agent currentAgent, float cohesionRadius)
    {
        Vector3 centerOfMass = Vector3.zero;
        int neighborCount = 0;

        foreach (var neighbor in agents)
        {
            if (neighbor != currentAgent)
            {
                Vector3 neighborPosition = neighbor.transform.position;
                neighborPosition.y = currentAgent.transform.position.y; // Mantener en el plano

                float distance = Vector3.Distance(currentAgent.transform.position, neighborPosition);
                if (distance < cohesionRadius)
                {
                    centerOfMass += neighborPosition;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            centerOfMass /= neighborCount;
            Vector3 cohesionForce = currentAgent.Seek(centerOfMass);
            cohesionForce.y = 0; // Restringir fuerza en Y
            return cohesionForce;
        }

        return Vector3.zero;
    }

    // Alignment
    public Vector3 Alignment(Agent currentAgent, float alignmentRadius, float maxSpeed, Vector3 currentVelocity)
    {
        Vector3 averageVelocity = Vector3.zero;
        int neighborCount = 0;

        foreach (var neighbor in agents)
        {
            if (neighbor != currentAgent)
            {
                float distance = Vector3.Distance(currentAgent.transform.position, neighbor.transform.position);
                if (distance < alignmentRadius)
                {
                    Vector3 neighborVelocity = neighbor.GetVelocity();
                    neighborVelocity.y = 0; // Evitar movimiento en Y
                    averageVelocity += neighborVelocity;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            averageVelocity /= neighborCount;
            averageVelocity = averageVelocity.normalized * maxSpeed;
            Vector3 alignmentForce = averageVelocity - currentVelocity;
            alignmentForce.y = 0; // Restringir fuerza en Y
            return alignmentForce;
        }

        return Vector3.zero;
    }
}


