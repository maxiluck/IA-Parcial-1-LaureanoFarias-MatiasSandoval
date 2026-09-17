using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Base Agent Settings")]
    [SerializeField] private float speed = 3f;
    public Vector3 Velocity { get; protected set; }
    protected virtual void Update()
    {
        Velocity = transform.forward * speed;
        transform.position += Velocity * Time.deltaTime;
    }
}