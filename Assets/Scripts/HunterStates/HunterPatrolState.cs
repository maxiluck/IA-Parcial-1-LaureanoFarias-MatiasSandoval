using UnityEngine;

public class HunterPatrolState : State<HunterAgent.HunterStates>
{
    private readonly HunterAgent _hunter;
    private int _currentWaypointIndex;
    private float _spawnTimer;

    public HunterPatrolState(HunterAgent hunter)
    {
        _hunter = hunter;
    }

    public override void Enter()
    {
        Debug.Log("[Cazador] Entró a PATROL");
    }

    public override void Update()
    {
        // 1. Chequear transición a GATHER (Prioridad: boid muerto dentro de percepción)
        if (CheckGatherTransition()) return;

        // 2. Chequear transición a ATTACK (TBA listo y boid vivo en rango)
        if (_hunter.TBATimer >= _hunter.TBA && CheckAttackTransition()) return;

        // 3. Lógica de Spawner (menos de 5 POIs)
        HandlePOISpawn();

        // 4. Movimiento por Waypoints
        PatrolMovement();
    }

    private void PatrolMovement()
    {
        var data = _hunter.PatrolConfig;
        if (data == null || data.waypoints == null || data.waypoints.Count == 0) return;

        Transform targetWP = data.waypoints[_currentWaypointIndex];

        Vector3 difference = targetWP.position - _hunter.transform.position;

        difference.y = 0f;

        //if (Vector3.Distance(targetWP.position, _hunter.transform.position) <= data.waypointCheckDistance)
        if (difference.magnitude <= data.waypointCheckDistance)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % data.waypoints.Count;
            targetWP = data.waypoints[_currentWaypointIndex];
        }

        Vector3 dir = (targetWP.position - _hunter.transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            _hunter.transform.position += _hunter.Speed * Time.deltaTime * dir.normalized;
            _hunter.transform.forward = dir.normalized;
        }
    }

    private void HandlePOISpawn()
    {
        if (_hunter.POIPrefab == null) return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _hunter.SpawnInterval)
        {
            _spawnTimer = 0f;
            if (PointOfInterest.ActivePOIs.Count < 5)
            {
                Vector3 spawnPos = _hunter.transform.position + (_hunter.transform.forward * 2f);
                Object.Instantiate(_hunter.POIPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    private bool CheckGatherTransition()
    {
        foreach (var deadBoid in BoidHealth.DeadBoids)
        {
            if (deadBoid == null) continue;
            float dist = Vector3.Distance(deadBoid.transform.position, _hunter.transform.position);
            if (dist <= _hunter.GatherRadius)
            {
                _fsm.ChangeState(HunterAgent.HunterStates.Gather);
                return true;
            }
        }
        return false;
    }

    private bool CheckAttackTransition()
    {
        Collider[] colliders = Physics.OverlapSphere(_hunter.transform.position, _hunter.VisionRadius);

        foreach (Collider col in colliders)
        {
            BoidHealth boid = col.GetComponentInParent<BoidHealth>();

            if (boid == null || boid.IsDead)
                continue;

            float distance = Vector3.Distance(boid.transform.position, _hunter.transform.position);

            if (distance <= _hunter.VisionRadius)
            {
                _fsm.ChangeState(HunterAgent.HunterStates.Attack);

                return true;
            }
        }

        return false;
    }

    //private bool CheckAttackTransition()
    //{
    //    var colliders = Physics.OverlapSphere(_hunter.transform.position, _hunter.VisionRadius);
    //    foreach (var col in colliders)
    //    {
    //        if (col.TryGetComponent<BoidHealth>(out var boid) && !boid.IsDead)
    //        {
    //            _fsm.ChangeState(HunterAgent.HunterStates.Attack);
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public override void Exit() { }
}