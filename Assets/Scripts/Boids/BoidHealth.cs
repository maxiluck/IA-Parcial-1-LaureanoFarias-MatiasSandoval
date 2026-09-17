using System.Collections;
using UnityEngine;

public class BoidHealth : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private float maxHealth = 50f;
    private float _currentHealth;

    [Header("Ataque al POI")]
    [SerializeField] private float damageToPOI = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float interactionRadius = 1.5f;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 4f;

    public static int HuntedCount;
    private AdvanceAgent _agent;
    private Collider _collider;
    private Renderer _renderer;
    private float _damageTimer;
    private float _spawnHeight;

    public bool IsDead { get; private set; }
    public static System.Collections.Generic.List<BoidHealth> DeadBoids { get; private set; } = new();

    private void Awake()
    {
        _agent = GetComponent<AdvanceAgent>();
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<Renderer>();
        _currentHealth = maxHealth;
        _spawnHeight = transform.position.y;
    }

    private void Update()
    {
        if (IsDead) return;

        PointOfInterest nearestPOI = PointOfInterest.GetNearestPOI(transform.position);
        if (nearestPOI != null)
        {
            float distSqr = (nearestPOI.transform.position - transform.position).sqrMagnitude;
            if (distSqr <= interactionRadius * interactionRadius)
            {
                _damageTimer += Time.deltaTime;
                if (_damageTimer >= damageInterval)
                {
                    nearestPOI.TakeDamage(damageToPOI);
                    _damageTimer = 0f;
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _currentHealth -= amount;
        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;

        _currentHealth = 0f;

        HuntedCount++;

        if (_agent != null) _agent.enabled = false;

        DeadBoids.Add(this);
    }

    public void Collect()
    {
        Debug.Log("Collect ejecutado: " + gameObject.name);

        DeadBoids.Remove(this);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        float randomX = Random.Range(-25f, 25f);
        float randomZ = Random.Range(-15f, 15f);
        transform.position = new Vector3(randomX, _spawnHeight, randomZ);

        _currentHealth = maxHealth;
        IsDead = false;

        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
        if (_agent != null) _agent.enabled = true;
    }

    private void OnDestroy()
    {
        DeadBoids.Remove(this);
    }
}