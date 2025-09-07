using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DragOnBoard : MonoBehaviour
{
    ARTrackedImage _tracked;
    BoardAnchor _board;
    GameObject _movable;

    bool _dragging;
    Vector3 _offsetUp;

    [SerializeField] bool lockAfterDrop = true; // ← bloquear tras soltar
    bool _locked = false;                       // ← estado de bloqueo

    public void SetContext(ARTrackedImage tracked, BoardAnchor board, GameObject movable)
    {
        _tracked = tracked;
        _board   = board;
        _movable = movable;

        var r = _movable ? _movable.GetComponentInChildren<Renderer>() : null;
        _offsetUp = r ? Vector3.up * r.bounds.extents.y : Vector3.zero;
        _locked = false; // cada vez que se crea, inicia desbloqueado
    }

    void Update()
    {
        if (_tracked == null || _board == null || _movable == null) return;
        if (_tracked.trackingState != TrackingState.Tracking) return;
        if (_locked) return; // ← si está bloqueado, ignorar toques

        if (Input.touchCount == 0) return;
        var t0 = Input.GetTouch(0);

        if (t0.phase == TouchPhase.Began) _dragging = true;

        if (t0.phase == TouchPhase.Ended || t0.phase == TouchPhase.Canceled)
        {
            _dragging = false;
            SnapToNearestSlot();

            if (lockAfterDrop) _locked = true; // ← queda fijo al soltar
            return;
        }

        if (_dragging)
        {
            var plane = new Plane(_tracked.transform.up, _tracked.transform.position);
            var ray = Camera.main.ScreenPointToRay(t0.position);

            if (plane.Raycast(ray, out float enter))
            {
                var world = ray.GetPoint(enter);

                var local = _board.transform.InverseTransformPoint(world);
                var b = _board.GetLocalBounds();
                local.x = Mathf.Clamp(local.x, b.min.x, b.max.x);
                local.z = Mathf.Clamp(local.z, b.min.z, b.max.z);
                local.y = 0f;

                _movable.transform.position =
                    _board.transform.TransformPoint(local) + _offsetUp;
            }
        }

        // (Opcional) mantener si quieres escalar/rotar antes de soltar
        if (Input.touchCount >= 2 && !_locked)
        {
            var t1 = Input.GetTouch(1);
            if (t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
            {
                var prevDist = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
                var currDist = (t0.position - t1.position).magnitude;
                var k = currDist / Mathf.Max(prevDist, 1f);
                _movable.transform.localScale *= Mathf.Clamp(k, 0.9f, 1.1f);

                var prevAng = Vector2.SignedAngle(
                    (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition), Vector2.right);
                var currAng = Vector2.SignedAngle(t0.position - t1.position, Vector2.right);
                _movable.transform.Rotate(_tracked.transform.up, currAng - prevAng, Space.World);
            }
        }
    }

    void SnapToNearestSlot()
    {
        if (_board.slots == null || _board.slots.Count == 0) return;

        Transform best = null; float bestDist = float.MaxValue;
        var pos = _movable.transform.position;

        foreach (var s in _board.slots)
        {
            var d = (s.position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = s; }
        }
        if (best != null) _movable.transform.position = best.position + _offsetUp;
    }

    // (Opcional) si luego quieres permitir mover nuevamente:
    public void Unlock() => _locked = false;
}
