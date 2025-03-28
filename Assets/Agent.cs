using System.Collections.Generic;
using UnityEngine;

public enum TypeSteeringBehavior
{
    Seek,
    Flee,
    Evade,
    Arrive,
    Pursuit,
    Wander,
    PathFollowing,
    ObstacleAvoidance,
    Separation,
    Cohesion,
    Alignment
}
public class Agent : MonoBehaviour
{
    public TypeSteeringBehavior type;
    public float pathRadius = 1.0f;
    public bool loopPath = true;
    public List<Transform> pathPoints;
    public Transform target;
    public float maxSpeed = 2.0f;
    public float slowingRadius = 2.0f;
    public float wanderRadius = 1.0f;
    public float wanderDistance = 2.0f;
    public float wanderJitter = 0.2f;
    public float separationRadius = 3.0f;
    public float cohesionRadius = 5.0f;
    public float alignmentRadius = 5.0f;
    public Vector3 velocity;
    private Vector3 wanderTarget;
    private PathFollower pathFollower;

    void Start()
    {
        wanderTarget = transform.position + Random.insideUnitSphere * wanderRadius;
        // Registrar este agente en el AgentManager
        AgentManager.Instance.RegisterAgent(this);

        pathFollower = new PathFollower(pathPoints, pathRadius, loopPath, transform);
    }

    void OnDestroy()
    {
        // Desregistrar este agente del AgentManager
        if (AgentManager.Instance != null)
        {
            AgentManager.Instance.UnregisterAgent(this);
        }
    }

    void Update()
    {
        HandleInput();
        Vector3 steering = Vector3.zero;

        switch (type)
        {
            case TypeSteeringBehavior.Seek:
                steering = Seek(target.position);
                break;
            case TypeSteeringBehavior.Flee:
                steering = Flee(target.position);
                break;
            case TypeSteeringBehavior.Evade:
                steering = Evade(target);
                break;
            case TypeSteeringBehavior.Arrive:
                steering = Arrive(target.position);
                break;
            case TypeSteeringBehavior.Pursuit:
                steering = Pursuit(target);
                break;
            case TypeSteeringBehavior.Wander:
                steering = Wander();
                break;
            case TypeSteeringBehavior.PathFollowing:
                steering = pathFollower.GetSteering(velocity, maxSpeed);
                break;
            case TypeSteeringBehavior.ObstacleAvoidance:
                steering = ObstacleAvoidance();
                break;
            case TypeSteeringBehavior.Separation:
                steering = AgentManager.Instance.Separation(this, separationRadius, maxSpeed, velocity);
                break;
            case TypeSteeringBehavior.Cohesion:
                steering = AgentManager.Instance.Cohesion(this, cohesionRadius);
                break;
            case TypeSteeringBehavior.Alignment:
                steering = AgentManager.Instance.Alignment(this, alignmentRadius, maxSpeed, velocity);
                break;
        }

        velocity += steering * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            transform.forward = velocity.normalized;
        }
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            type = TypeSteeringBehavior.Seek;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            type = TypeSteeringBehavior.Flee;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            type = TypeSteeringBehavior.Evade;
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            type = TypeSteeringBehavior.Arrive;
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            type = TypeSteeringBehavior.Pursuit;
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {
            type = TypeSteeringBehavior.Wander;
        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {
            type = TypeSteeringBehavior.PathFollowing;
        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {
            type = TypeSteeringBehavior.ObstacleAvoidance;
        }
        else if (Input.GetKey(KeyCode.Alpha9))
        {
            type = TypeSteeringBehavior.Separation;
        }
        else if (Input.GetKey(KeyCode.Alpha0))
        {
            type = TypeSteeringBehavior.Cohesion;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            type = TypeSteeringBehavior.Alignment;
        }
    }
    // Método para obtener la velocidad actual
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    Vector3 Flee(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (transform.position - targetPosition).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    Vector3 Evade(Transform target)
    {
        // Obtener el componente MovimientoObjeto del target
        MovimientoObjeto movimientoTarget = target.GetComponent<MovimientoObjeto>();

        // Verificar si el movimientoTarget es nulo
        if (movimientoTarget == null)
        {
            Debug.LogWarning("El target no tiene un componente MovimientoObjeto.");
            return Vector3.zero; // Manejar este caso según sea necesario
        }

        // Obtener la velocidad actual del target
        Vector3 velocidadTarget = movimientoTarget.VelocidadActual;

        // Calcular el vector hacia el objetivo
        Vector3 toTarget = target.position - transform.position;

        // Calcular el tiempo de anticipación
        float lookAheadTime = toTarget.magnitude / (maxSpeed + velocidadTarget.magnitude);

        // Calcular la posición futura del objetivo
        Vector3 futurePosition = target.position + velocidadTarget * lookAheadTime;

        // Llamar al método Flee con la posición futura
        return Flee(futurePosition);
    }

    Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;
        if (distance > 0)
        {
            float decelerationTweaker = 2f;
            float speed = distance / decelerationTweaker;
            speed = Mathf.Min(speed, maxSpeed);
            Vector3 desiredVelocity = toTarget * speed / distance;
            return desiredVelocity - velocity;
        }
        return Vector3.zero;
    }
    Vector3 Pursuit(Transform target)
    {
        MovimientoObjeto movimientoTarget = target.GetComponent<MovimientoObjeto>();
        Vector3 velocidadTarget = Vector3.zero;

        if (movimientoTarget != null)
        {
            velocidadTarget = movimientoTarget.VelocidadActual;
        }
        else
        {
            Debug.LogWarning("El target no tiene un componente MovimientoObjeto.");
            return Seek(target.position);
        }

        Vector3 toTarget = target.position - transform.position;
        float distancia = toTarget.magnitude;
        float velocidadRelativa = maxSpeed + velocidadTarget.magnitude;
        float lookAheadTime = distancia / velocidadRelativa;

        Vector3 futurePosition = target.position + velocidadTarget * lookAheadTime;
        return Seek(futurePosition);
    }
    Vector3 Wander()
    {
        // Actualizar wanderTarget con un desplazamiento aleatorio en el plano XZ
        wanderTarget += new Vector3(Random.Range(-wanderJitter, wanderJitter), 0, Random.Range(-wanderJitter, wanderJitter));

        // Normalizar y escalar al radio de wander
        wanderTarget = wanderTarget.normalized * wanderRadius;

        // Asegurarse de que wanderTarget esté en el plano XZ
        wanderTarget.y = 0;

        // Calcular la posición del objetivo en el espacio local
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);

        // Convertir la posición del objetivo al espacio mundial
        Vector3 targetWorld = transform.TransformPoint(targetLocal);

        // Llamar a Seek para mover el agente hacia la posición objetivo
        return Seek(targetWorld);
    }
    Vector3 ObstacleAvoidance()
    {
        float avoidDistance = 5.0f; // Distancia para detectar obstáculos
        float rayLength = 2.0f; // Longitud del rayo
        Vector3[] rayDirections = {
        transform.forward,
        Quaternion.Euler(0, 30, 0) * transform.forward,
        Quaternion.Euler(0, -30, 0) * transform.forward
    };

        foreach (var direction in rayDirections)
        {
            Ray ray = new Ray(transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
            {
                Vector3 avoidForce = Vector3.Reflect(direction, hit.normal) * avoidDistance;
                avoidForce.y = 0; // Restringir el movimiento en el eje Y
                return avoidForce - velocity;
            }
        }
        return Vector3.zero;
    }
}


