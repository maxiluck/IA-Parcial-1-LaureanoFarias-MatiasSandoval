using UnityEngine;

public class HunterAttackState : State<HunterAgent.HunterStates>
{
    private readonly HunterAgent _hunter;
    private BoidHealth _targetBoid;

    public HunterAttackState(HunterAgent hunter)
    {
        _hunter = hunter;
    }

    public override void Enter()
    {
        Debug.Log("[Cazador] Entró a ATTACK");
        _targetBoid = FindNearestAliveBoid();
    }

    public override void Update()
    {
        if (_targetBoid == null || _targetBoid.IsDead)
        {
            _fsm.ChangeState(HunterAgent.HunterStates.Patrol);
            return;
        }

        float distance = Vector3.Distance(_targetBoid.transform.position, _hunter.transform.position);

        if (distance > _hunter.VisionRadius)
        {
            _fsm.ChangeState(HunterAgent.HunterStates.Patrol);
            return;
        }

        if (distance <= _hunter.MeleeRadius)
        {
            ExecuteMeleeAttack();
        }
        else if (distance <= _hunter.RangeRadius)
        {
            ExecuteRangeAttack();
        }
        else
        {
            PursueTarget();
        }
    }

    private void PursueTarget()
    {
        Vector3 dir = (_targetBoid.transform.position - _hunter.transform.position);
        dir.y = 0f;
        _hunter.transform.position += _hunter.Speed * Time.deltaTime * dir.normalized;
        if (dir.sqrMagnitude > 0.01f)
            _hunter.transform.forward = dir.normalized;
    }

    private void ExecuteMeleeAttack()
    {
        Debug.Log("[Cazador] Ataque Melee exitoso");
        _targetBoid.TakeDamage(50f);
        _hunter.ResetTBA();
        _fsm.ChangeState(HunterAgent.HunterStates.Patrol);
    }

    private void ExecuteRangeAttack()
    {
        Debug.Log("[Cazador] Ataque a Distancia exitoso");
        _targetBoid.TakeDamage(25f);
        _hunter.ResetTBA();
        _fsm.ChangeState(HunterAgent.HunterStates.Patrol);
    }

    private BoidHealth FindNearestAliveBoid()
    {
        Collider[] colliders = Physics.OverlapSphere(_hunter.transform.position, _hunter.VisionRadius);

        BoidHealth nearest = null;

        float minDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            BoidHealth boid = col.GetComponentInParent<BoidHealth>();

            if (boid == null || boid.IsDead)
                continue;

            float distance = Vector3.Distance(boid.transform.position, _hunter.transform.position);

            if (distance > _hunter.VisionRadius)
                continue;

            if (distance < minDistance)
            {
                minDistance = distance;

                nearest = boid;
            }
        }

        return nearest;
    }

    public override void Exit()
    {
        _targetBoid = null;
    }
}