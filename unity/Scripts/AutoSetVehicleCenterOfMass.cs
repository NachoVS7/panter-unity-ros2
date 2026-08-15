using UnityEngine;

[ExecuteAlways]
public class AutoSetVehicleCenterOfMass : MonoBehaviour
{
    [Header("Rigidbody del vehículo")]
    public Rigidbody targetRigidbody;

    [Header("WheelControllers")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    [Header("Collider principal del chasis")]
    public BoxCollider chassisBoxCollider;

    [Header("Ajuste vertical")]
    [Range(0f, 1f)]
    public float heightPercentFromBottom = 0.35f;

    [Header("Aplicar")]
    public bool applyOnStart = true;
    public bool applyInEditor = false;

    [Header("Debug")]
    public bool drawGizmo = true;
    public float gizmoSize = 0.25f;

    private void Reset()
    {
        targetRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (Application.isPlaying && applyOnStart)
        {
            ApplyCenterOfMass();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && applyInEditor)
        {
            ApplyCenterOfMass();
        }
    }

    [ContextMenu("Apply Center Of Mass")]
    public void ApplyCenterOfMass()
    {
        if (targetRigidbody == null)
        {
            Debug.LogWarning("Falta asignar el Rigidbody.");
            return;
        }

        if (wheelFL == null || wheelFR == null || wheelRL == null || wheelRR == null)
        {
            Debug.LogWarning("Falta asignar alguna rueda/WheelController.");
            return;
        }

        if (chassisBoxCollider == null)
        {
            Debug.LogWarning("Falta asignar el BoxCollider del chasis.");
            return;
        }

        Vector3 wheelCenterWorld =
            (wheelFL.position + wheelFR.position + wheelRL.position + wheelRR.position) / 4f;

        Bounds chassisBounds = chassisBoxCollider.bounds;

        float bottomY = chassisBounds.min.y;
        float topY = chassisBounds.max.y;
        float comY = Mathf.Lerp(bottomY, topY, heightPercentFromBottom);

        Vector3 comWorld = new Vector3(
            wheelCenterWorld.x,
            comY,
            wheelCenterWorld.z
        );

        Vector3 comLocal = targetRigidbody.transform.InverseTransformPoint(comWorld);

        targetRigidbody.centerOfMass = comLocal;

        Debug.Log(
            "Center Of Mass aplicado. Local: " + comLocal.ToString("F3") +
            " | World: " + comWorld.ToString("F3")
        );
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmo || targetRigidbody == null)
            return;

        Vector3 comWorld = targetRigidbody.transform.TransformPoint(targetRigidbody.centerOfMass);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(comWorld, gizmoSize);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(comWorld + Vector3.left * gizmoSize * 2f, comWorld + Vector3.right * gizmoSize * 2f);
        Gizmos.DrawLine(comWorld + Vector3.forward * gizmoSize * 2f, comWorld + Vector3.back * gizmoSize * 2f);
        Gizmos.DrawLine(comWorld + Vector3.up * gizmoSize * 2f, comWorld + Vector3.down * gizmoSize * 2f);
    }
}
