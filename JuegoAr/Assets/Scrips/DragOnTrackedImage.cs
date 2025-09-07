using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DragOnTrackedImage : MonoBehaviour
{
    [SerializeField] private GameObject movable;     // se asignará en runtime
    [SerializeField] private ARTrackedImage tracked; // se asignará en runtime

    float _heightOffset;

    // === Propiedades públicas para poder asignar desde otro script ===
    public GameObject Movable
    {
        get => movable;
        set { movable = value; RecalculateHeightOffset(); }
    }

    public ARTrackedImage Tracked
    {
        get => tracked;
        set => tracked = value;
    }

    void RecalculateHeightOffset()
    {
        var r = movable ? movable.GetComponentInChildren<Renderer>() : null;
        _heightOffset = r ? r.bounds.extents.y : 0f;
    }

    void Update()
    {
        if (movable == null || tracked == null) return;
        if (tracked.trackingState != TrackingState.Tracking) return;
        if (Input.touchCount == 0) return;

        var t0 = Input.GetTouch(0);
        if (t0.phase != TouchPhase.Began && t0.phase != TouchPhase.Moved) return;

        // Plano de la imagen
        var plane = new Plane(tracked.transform.up, tracked.transform.position);
        var ray = Camera.main.ScreenPointToRay(t0.position);

        if (plane.Raycast(ray, out float enter))
        {
            var worldPoint = ray.GetPoint(enter);

            // A espacio local de la imagen para limitar dentro del rectángulo físico
            var local = tracked.transform.InverseTransformPoint(worldPoint);

            var halfX = tracked.size.x * 0.5f; // ancho/2 (metros)
            var halfZ = tracked.size.y * 0.5f; // alto/2  (metros)

            local.x = Mathf.Clamp(local.x, -halfX, halfX);
            local.z = Mathf.Clamp(local.z, -halfZ, halfZ);

            var clampedWorld = tracked.transform.TransformPoint(local);
            clampedWorld += tracked.transform.up * _heightOffset;

            movable.transform.position = clampedWorld;
        }

        // Rotar/escalar con dos dedos (opcional)
        if (Input.touchCount >= 2)
        {
            var t1 = Input.GetTouch(1);
            if (t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
            {
                var prevDist = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
                var currDist = (t0.position - t1.position).magnitude;
                var scaleFactor = currDist / Mathf.Max(prevDist, 1f);
                movable.transform.localScale *= Mathf.Clamp(scaleFactor, 0.9f, 1.1f);

                var prevAngle = Vector2.SignedAngle(
                    (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition),
                    Vector2.right
                );
                var currAngle = Vector2.SignedAngle(t0.position - t1.position, Vector2.right);
                var deltaAngle = currAngle - prevAngle;
                movable.transform.Rotate(tracked.transform.up, deltaAngle, Space.World);
            }
        }
    }
}
