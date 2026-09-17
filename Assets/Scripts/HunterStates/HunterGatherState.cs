using UnityEngine;

public class HunterGatherState : State<HunterAgent.HunterStates>
{
    private readonly HunterAgent _hunter;
    private BoidHealth _targetDeadBoid;
    private float _gatherTimer;
    private const float GatherDuration = 2f;

    public HunterGatherState(HunterAgent hunter)
    {
        _hunter = hunter;
    }

    public override void Enter()
    {
        Debug.Log("[Cazador] Entró a GATHER");
        _gatherTimer = 0f;
        _targetDeadBoid = FindNearestDeadBoid();
    }

    public override void Update()
    {
        // Si el objetivo ya no está disponible, volver a Patrol
        if (_targetDeadBoid == null)
        {
            Debug.Log("Gather: salgo porque no tengo un cadáver.");

            _fsm.ChangeState(HunterAgent.HunterStates.Patrol);
            return;
        }

        Vector3 difference = _targetDeadBoid.transform.position - _hunter.transform.position;

        difference.y = 0f;

        float distance = difference.magnitude;
        //float distance = Vector3.Distance(_targetDeadBoid.transform.position, _hunter.transform.position);

        // Desplazarse hacia el cadáver
        if (distance > 1f)
        {
            Vector3 dir = (_targetDeadBoid.transform.position - _hunter.transform.position);
            dir.y = 0f;
            _hunter.transform.position += _hunter.Speed * Time.deltaTime * dir.normalized;
            if (dir.sqrMagnitude > 0.01f)
                _hunter.transform.forward = dir.normalized;
        }
        else
        {
            // Ejecutar recolección por tiempo
            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= GatherDuration)
            {
                _targetDeadBoid.Collect();
                _fsm.ChangeState(HunterAgent.HunterStates.Patrol);
            }
        }
    }

    private BoidHealth FindNearestDeadBoid()
    {
        BoidHealth nearest = null;
        float minDist = float.MaxValue;

        foreach (var dead in BoidHealth.DeadBoids)
        {
            if (dead == null) continue;
            float dist = Vector3.Distance(dead.transform.position, _hunter.transform.position);
            if (dist <= _hunter.GatherRadius && dist < minDist)
            {
                minDist = dist;
                nearest = dead;
            }
        }
        return nearest;
    }

    public override void Exit()
    {
        _targetDeadBoid = null;
    }
}