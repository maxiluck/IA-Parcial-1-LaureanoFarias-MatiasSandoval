using System.Collections.Generic;
using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    public static List<PointOfInterest> ActivePOIs { get; private set; } = new List<PointOfInterest>();

    private void Awake()
    {
        _currentHealth = maxHealth;
        ActivePOIs.Add(this);
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }
    public static PointOfInterest GetNearestPOI(Vector3 fromPosition)
    {
        return GetNearestPOI(fromPosition, float.PositiveInfinity);
    }

    public static PointOfInterest GetNearestPOI(Vector3 fromPosition, float perceptionRadius)
    {
        if (ActivePOIs.Count == 0) return null;

        PointOfInterest nearest = null;
        float minSqrDist = float.MaxValue;
        float maxSqrDist = perceptionRadius * perceptionRadius;

        for (int i = 0; i < ActivePOIs.Count; i++)
        {
            if (ActivePOIs[i] == null)
            {
                ActivePOIs.RemoveAt(i--);
                continue;
            }

            float sqrDist = (ActivePOIs[i].transform.position - fromPosition).sqrMagnitude;
            if (sqrDist <= maxSqrDist && sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                nearest = ActivePOIs[i];
            }
        }

        return nearest;
    }

    private void OnDestroy()
    {
        ActivePOIs.Remove(this);
    }
}
