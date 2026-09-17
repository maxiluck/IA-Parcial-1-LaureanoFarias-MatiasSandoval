using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3f;
    public float Speed => _speed;

    [SerializeField]
    private PatrolData _patrolData;

    private readonly FiniteStateMachine<States> _fsm = new();
    public enum States { Idle, Patrol, Death }

    public int HealthPoints;


    private void Start()
    {
        var idle = new IdleState();
        var patrol = new PatrolState<States>(_patrolData, this, States.Idle);
        _fsm.AddState(States.Idle, idle);
        _fsm.AddState(States.Patrol, patrol);
        _fsm.ChangeState(States.Idle);
    }
    private void Update()
    {
        _fsm.Update();
    }


    public void TakeDamage(int damage)
    {
        HealthPoints -= damage;
        if (HealthPoints <= 0) _fsm.ChangeState(States.Death);
    }

}

