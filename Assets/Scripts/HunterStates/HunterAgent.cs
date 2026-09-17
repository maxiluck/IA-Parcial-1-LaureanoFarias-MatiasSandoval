using UnityEngine;

public class HunterAgent : Agent
{
    [Header("Movimiento")]
    [SerializeField] private float hunterSpeed = 4f; 
    public float Speed => hunterSpeed;

    [Header("Rangos y Variables Obligatorias")]
    [SerializeField] private float tba = 2f; 
    [SerializeField] private float meleeAttackRadius = 1.5f;
    [SerializeField] private float rangeAttackRadius = 6f;
    [SerializeField] private float visionRadius = 10f;
    [SerializeField] private float gatherRadius = 8f;

    [Header("Patrulla & Spawner")]
    [SerializeField] private PatrolData patrolData;
    [SerializeField] private GameObject poiPrefab;
    [SerializeField] private float spawnInterval = 5f;

    public enum HunterStates { Patrol, Attack, Gather }

    public HunterStates CurrentStateEnum
    {
        get { return _fsm.CurrentState; }
    }

    private readonly FiniteStateMachine<HunterStates> _fsm = new();

    private float _tbaTimer;

    public float TBA => tba;
    public float TBATimer => _tbaTimer;
    public float MeleeRadius => meleeAttackRadius;
    public float RangeRadius => rangeAttackRadius;
    public float VisionRadius => visionRadius;
    public float GatherRadius => gatherRadius;
    public PatrolData PatrolConfig => patrolData;
    public GameObject POIPrefab => poiPrefab;
    public float SpawnInterval => spawnInterval;

    private void Start()
    {
        if (patrolData != null)
            patrolData.transform = transform;
        _tbaTimer = tba;

        _fsm.AddState(HunterStates.Patrol, new HunterPatrolState(this));
        _fsm.AddState(HunterStates.Attack, new HunterAttackState(this));
        _fsm.AddState(HunterStates.Gather, new HunterGatherState(this));

        _fsm.ChangeState(HunterStates.Patrol);
    }

    protected override void Update()
    {
        _tbaTimer += Time.deltaTime;

        _fsm.Update();
    }

    public void ResetTBA()
    {
        _tbaTimer = 0f;
    }

    public void ChangeState(HunterStates newState)
    {
        _fsm.ChangeState(newState);
    }

}