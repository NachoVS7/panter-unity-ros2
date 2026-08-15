using UnityEngine;

[ExecuteAlways]
public class AutoFitBodyCollider : MonoBehaviour
{
    [Header("Referencias")]
    public Transform bodyRoot;
    public Transform colliderObject;

    [Header("Ajuste extra")]
    public Vector3 sizePadding = new Vector3(0.15f, 0.10f, 0.15f);
    public Vector3 centerOffset = Vector3.zero;

    [Header("Opciones")]
    public bool ignoreDisabledRenderers = true;

    [ContextMenu("Ajustar Box Collider al Body")]
    public void FitColliderToBody()
    {
        if (bodyRoot == null)
        {
            Debug.LogError("Falta asignar bodyRoot.");
            return;
        }

        if (colliderObject == null)
        {
            Debug.LogError("Falta asignar colliderObject.");
            return;
        }

        Renderer[] renderers = bodyRoot.GetComponentsInChildren<Renderer>(true);

        bool hasBounds = false;
        Bounds totalBounds = new Bounds();

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            if (ignoreDisabledRenderers && !r.enabled)
                continue;

            if (!hasBounds)
            {
                totalBounds = r.bounds;
                hasBounds = true;
            }
            else
            {
                totalBounds.Encapsulate(r.bounds);
            }
        }

        if (!hasBounds)
        {
            Debug.LogError("No se han encontrado Renderers dentro del Body.");
            return;
        }

        colliderObject.position = totalBounds.center + centerOffset;
        colliderObject.rotation = bodyRoot.root.rotation;

        MeshCollider meshCollider = colliderObject.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.enabled = false;
        }

        BoxCollider box = colliderObject.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = colliderObject.gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = false;

        Vector3 localSize = colliderObject.InverseTransformVector(totalBounds.size);
        localSize = new Vector3(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z)
        );

        box.center = Vector3.zero;
        box.size = localSize + sizePadding;

        Debug.Log("Collider ajustado automáticamente al Body.");
        Debug.Log("Centro global: " + totalBounds.center);
        Debug.Log("Tamaño global: " + totalBounds.size);
        Debug.Log("BoxCollider size final: " + box.size);
    }
}
