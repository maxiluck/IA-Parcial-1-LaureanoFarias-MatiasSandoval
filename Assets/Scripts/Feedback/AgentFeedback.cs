using UnityEngine;


public class AgentFeedback : MonoBehaviour
{

    private AdvanceAgent _boid;
    private BoidHealth _health;
    private HunterAgent _hunter;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _colors;

    private void Awake()
    {
        _boid = GetComponent<AdvanceAgent>();
        _health = GetComponent<BoidHealth>();
        _hunter = GetComponent<HunterAgent>();

        _renderers = GetComponentsInChildren<Renderer>();
        _colors = new MaterialPropertyBlock();

        if (_hunter != null)
            BoidHealth.HuntedCount = 0;
    }

    private void LateUpdate()
    {
        if (_hunter != null)
        {
            UpdateHunterColor();
        }

        else if (_boid != null && _health != null)
        {
            UpdateBoidColor();
        }
    }

    private void UpdateBoidColor()
    {
        if (_health.IsDead)
        {
            SetColor(Color.gray);
        }

        else if (_boid.currentSteering == AdvanceAgent.SteeringModes.Evade)
        {
            SetColor(new Color(1f, 0.5f, 0f));
        }

        else if (_boid.currentSteering == AdvanceAgent.SteeringModes.Arrive)
        {
            SetColor(Color.green);
        }

        else
        {
            SetColor(Color.blue);
        }
    }

    private void UpdateHunterColor()
    {
        switch (_hunter.CurrentStateEnum)
        {
            case HunterAgent.HunterStates.Patrol:
                SetColor(Color.yellow);

                break;

            case HunterAgent.HunterStates.Attack:
                SetColor(Color.red);

                break;

            case HunterAgent.HunterStates.Gather:
                SetColor(Color.magenta);

                break;
        }
    }

    private void SetColor(Color color)
    {
        foreach (Renderer body in _renderers)
        {
            if (body == null)
                continue;

            body.GetPropertyBlock(_colors);

            _colors.SetColor("_BaseColor", color);

            _colors.SetColor("_Color", color);

            body.SetPropertyBlock(_colors);
        }
    }

    private void OnGUI()
    {
        if (_hunter == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 370, 370), GUI.skin.box);

        GUILayout.Label("CAZADOR");

        GUILayout.Label("Estado: " + _hunter.CurrentStateEnum);

        float remaining = Mathf.Max(0f, _hunter.TBA - _hunter.TBATimer);

        GUILayout.Label("Proximo ataque disponible en: " + remaining.ToString("F1") + " s");

        GUILayout.Label("Boids cazados: " + BoidHealth.HuntedCount);

        GUILayout.Label("Cadaveres pendientes: " + BoidHealth.DeadBoids.Count);

        GUILayout.Label("Objetos de interes: " + PointOfInterest.ActivePOIs.Count + "/5");

        GUILayout.Space(10);
        GUILayout.Label("COLORES DEL CAZADOR");
        GUILayout.Label("Amarillo: patrullando");
        GUILayout.Label("Rojo: atacando / persiguiendo");
        GUILayout.Label("Violeta: buscando / recolectando cadaver");

        GUILayout.Space(10);
        GUILayout.Label("COLORES DE LOS BOIDS");
        GUILayout.Label("Azul: movimiento en grupo");
        GUILayout.Label("Naranja: huyendo");
        GUILayout.Label("Verde: acercandose al objeto / interactuando");
        GUILayout.Label("Gris: muerto");

        GUILayout.EndArea();
    }

    private void OnDrawGizmosSelected()
    {
        HunterAgent hunter = GetComponent<HunterAgent>();

        if (hunter != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(transform.position, hunter.VisionRadius);

            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(transform.position, hunter.RangeRadius);

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, hunter.MeleeRadius);

            Gizmos.color = Color.magenta;

            Gizmos.DrawWireSphere(transform.position, hunter.GatherRadius);
        }
    }
}
