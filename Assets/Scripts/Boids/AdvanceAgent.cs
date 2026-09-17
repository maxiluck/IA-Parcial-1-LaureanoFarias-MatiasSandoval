using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidHealth))]
public class AdvanceAgent : Agent
{
    [Header("Objetivos")]
    [SerializeField] private Agent hunterThreat;

    [Header("Ajustes de Movimiento")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxSteering = 10f;
    [SerializeField] private bool blockY = true;

    [Header("Parámetros de Evade")]
    [SerializeField] private float visionRadius = 8f;
    [SerializeField] private float maxPredictionTime = 1.5f;

    [Header("Parámetros de Arrive")]
    [SerializeField] private float slowingDistance = 3f;
    [SerializeField] private float minDistance = 0.2f;
    [SerializeField] private float poiVisionRadius = 8f;

    [Header("Parámetros de Flocking")]
    private static readonly List<AdvanceAgent> allAgents = new List<AdvanceAgent>();
    [SerializeField] private float _separationRadius = 1.2f;
    [SerializeField] private float _alignmentRadius = 4.5f;
    [SerializeField] private float _cohesionRadius = 4.5f;

    [SerializeField, Range(0, 3f)] private float separationWhight = 1f;
    [SerializeField, Range(0, 3f)] private float alignmentWhight = 1f;
    [SerializeField, Range(0, 3f)] private float cohesionWhight = 1f;
    private Vector3 _velocity;

    public SteeringModes currentSteering;

    public enum SteeringModes
    {
        Seek,
        Flee,
        Arrive,
        Persuit,
        Evade,
        Flocking
    }

    private void Awake()
    {
        if (GetComponent<BoidHealth>() == null)
            gameObject.AddComponent<BoidHealth>();

        if (hunterThreat == null)
        {
            // hunterThreat = FindFirstObjectByType<HunterAgent>();
        }

        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity += randomDirection.normalized * maxSpeed;
    }

    private void OnEnable()
    {
        if (!allAgents.Contains(this))
            allAgents.Add(this);
    }

    protected override void Update()
    {
        _velocity += SteeringVector();
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        if (blockY) _velocity.y = 0f;
        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero)
            transform.forward = _velocity;

        Velocity = _velocity;

        if (Bounds.Instance != null)
            transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
    }

    private Vector3 SteeringVector()
    {
        if (IsHunterInVision())
        {
            currentSteering = SteeringModes.Evade;
            return Evade(hunterThreat);
        }

        PointOfInterest nearestPOI = PointOfInterest.GetNearestPOI(transform.position, poiVisionRadius);
        if (nearestPOI != null)
        {
            currentSteering = SteeringModes.Arrive;
            return Arrive(nearestPOI.transform.position);
        }

        currentSteering = SteeringModes.Flocking;
        return Flocking();
    }

    private Vector3 Flocking()
    {
        return CalculateSeparation(allAgents, _separationRadius) * separationWhight 
             + CalculateAlignment(allAgents, _alignmentRadius) * alignmentWhight 
             + CalculateCohesion(allAgents, _cohesionRadius) * cohesionWhight;
    }

    private Vector3 CalculateSeparation(List<AdvanceAgent> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in list)
        {
            if (agent == this) continue;
            if (InRange(agent.transform.position, radius))
            {
                desired += (agent.transform.position - transform.position);
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteeringForce(-desired.normalized * maxSpeed);
    }

    private Vector3 CalculateAlignment(List<AdvanceAgent> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in list)
        {
            if (agent == this) continue;
            if (InRange(agent.transform.position, radius))
            {
                desired += agent.Velocity;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteeringForce(desired.normalized * maxSpeed);
    }

    private Vector3 CalculateCohesion(List<AdvanceAgent> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in list)
        {
            if (agent == this) continue;
            if (InRange(agent.transform.position, radius))
            {
                desired += agent.transform.position;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return Seek(desired);
    }

    private bool InRange(Vector3 pos, float radius) => (pos - transform.position).sqrMagnitude <= radius * radius;

    private Vector3 CalculateSteeringForce(Vector3 desiredVelocity)
    {
        Vector3 steering = desiredVelocity - _velocity;
        return Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
    }

    private Vector3 CalculateDesired(Vector3 targetPos, float speed)
    {
        Vector3 desired = (targetPos - transform.position).normalized;
        desired *= speed;
        return desired;
    }

    private bool IsHunterInVision()
    {
        if (hunterThreat == null) return false;
        Vector3 hunterDirection = hunterThreat.transform.position - transform.position;
        if (blockY) hunterDirection.y = 0;

        return hunterDirection.sqrMagnitude <= visionRadius * visionRadius;
    }

    private Vector3 Seek(Vector3 targetPos)
    {
        var desired = CalculateDesired(targetPos, maxSpeed);
        return CalculateSteeringForce(desired);
    }

    private Vector3 Flee(Vector3 targetPos)
    {
        var desired = CalculateDesired(targetPos, maxSpeed);
        return CalculateSteeringForce(-desired);
    }

    private Vector3 Arrive(Vector3 targetPos)
    {
        Vector3 toTarget = targetPos - transform.position;
        if (blockY) toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance <= minDistance)
        {
            return CalculateSteeringForce(Vector3.zero);
        }

        float currentSpeed = (distance < slowingDistance)
            ? maxSpeed * (distance / slowingDistance)
            : maxSpeed;

        Vector3 desired = CalculateDesired(targetPos, currentSpeed);
        return CalculateSteeringForce(desired);
    }

    private Vector3 CalculateFuture(Agent target)
    {
        Vector3 direccion = target.transform.position - transform.position;
        if (blockY) direccion.y = 0f;

        float distance = direccion.magnitude;
        var predictedPosition = Mathf.Min(maxPredictionTime, distance / Mathf.Max(0.01f, maxSpeed + target.Velocity.magnitude));

        Vector3 futurePosition = target.transform.position + target.Velocity * predictedPosition;
        return futurePosition;
    }

    private Vector3 Persuit(Agent target)
    {
        var predictedPosition = CalculateFuture(target);
        return Seek(predictedPosition);
    }

    private Vector3 Evade(Agent target)
    {
        var predictedPosition = CalculateFuture(target);
        return Flee(predictedPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slowingDistance);
    }

    private void OnDisable()
    {
        allAgents.Remove(this);
    }
}