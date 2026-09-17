using System.Collections.Generic;
using UnityEngine;

public class AdvanceAgent : Agent
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxSteering = 10f;
    [SerializeField] private bool blockY = true;

    [Header("Parámetros de Flocking")]
    private static readonly List<AdvanceAgent> allAgents = new List<AdvanceAgent>();
    [SerializeField] private float _separationRadius = 1.2f;
    [SerializeField] private float _alignmentRadius = 4.5f;
    [SerializeField] private float _cohesionRadius = 4.5f;

    [SerializeField, Range(0, 3f)] private float separationWhight = 1.5f;
    [SerializeField, Range(0, 3f)] private float alignmentWhight = 1f;
    [SerializeField, Range(0, 3f)] private float cohesionWhight = 0.8f;
    private Vector3 _velocity;

    private void Awake()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        _velocity += randomDirection.normalized * maxSpeed;
    }

    private void OnEnable()
    {
        if (!allAgents.Contains(this))
            allAgents.Add(this);
    }

    protected override void Update()
    {
        _velocity += Flocking();
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        if (blockY) _velocity.y = 0f;

        transform.position += _velocity * Time.deltaTime;
        if (_velocity != Vector3.zero)
            transform.forward = _velocity;

        Velocity = _velocity;

        if (Bounds.Instance != null)
            transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
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

    private Vector3 Seek(Vector3 targetPos)
    {
        var desired = CalculateDesired(targetPos, maxSpeed);
        return CalculateSteeringForce(desired);
    }

    private void OnDisable()
    {
        allAgents.Remove(this);
    }
}
