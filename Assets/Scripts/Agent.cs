using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Base Agent Settings")]
    [SerializeField] private float speed = 3f;

    // Propiedad pública que tu AgentSteering lee en CalculateFuture
    public Vector3 Velocity { get; protected set; }

    // Movimiento simple hacia adelante para probar que el Cazador se desplace y tenga velocidad
    protected virtual void Update()
    {
        Velocity = transform.forward * speed;
        transform.position += Velocity * Time.deltaTime;
    }
}