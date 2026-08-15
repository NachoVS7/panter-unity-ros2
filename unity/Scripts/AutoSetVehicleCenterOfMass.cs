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
        if (targetRigidbody == null || wheelFL == null || wheelFR == null || wheelRL == null || wheelRR == null || chassisBoxCollider == null)
        {
            Debug.LogWarning("Faltan referencias para calcular el centro de masas.");
            return;
        }

        Vector3 wheelCenterWorld = (wheelFL.position + wheelFR.position + wheelRL.position + wheelRR.position) / 4f;
        Bounds chassisBounds = chassisBoxCollider.bounds;

        float comY = Mathf.Lerp(chassisBounds.min.y, chassisBounds.max.y, heightPercentFromBottom);

        Vector3 comWorld = new Vector3(wheelCenterWorld.x, comY, wheelCenterWorld.z);
        Vector3 comLocal = targetRigidbody.transform.InverseTransformPoint(comWorld);

        targetRigidbody.centerOfMass = comLocal;

        Debug.Log("Center Of Mass aplicado. Local: " + comLocal.ToString("F3") + " | World: " + comWorld.ToString("F3"));
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmo || targetRigidbody == null) return;

        Vector3 comWorld = targetRigidbody.transform.TransformPoint(targetRigidbody.centerOfMass);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(comWorld, gizmoSize);
    }
}
