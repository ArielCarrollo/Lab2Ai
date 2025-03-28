using System.Collections.Generic;
using UnityEngine;

public class PathFollower
{
    private List<Transform> pathPoints;
    private int currentPathIndex = 0;
    private float pathRadius;
    private bool loopPath;
    private Transform agentTransform;

    public PathFollower(List<Transform> pathPoints, float pathRadius, bool loopPath, Transform agentTransform)
    {
        this.pathPoints = pathPoints;
        this.pathRadius = pathRadius;
        this.loopPath = loopPath;
        this.agentTransform = agentTransform;
    }

    public Vector3 GetSteering(Vector3 velocity, float maxSpeed)
    {
        if (pathPoints.Count == 0) return Vector3.zero;

        Vector3 currentPoint = pathPoints[currentPathIndex].position;
        if (Vector3.Distance(agentTransform.position, currentPoint) < pathRadius)
        {
            if (loopPath)
            {
                currentPathIndex = (currentPathIndex + 1) % pathPoints.Count;
            }
            else if (currentPathIndex < pathPoints.Count - 1)
            {
                currentPathIndex++;
            }
        }
        return Seek(pathPoints[currentPathIndex].position, velocity, maxSpeed);
    }

    private Vector3 Seek(Vector3 targetPosition, Vector3 velocity, float maxSpeed)
    {
        Vector3 desiredVelocity = (targetPosition - agentTransform.position).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }
}
