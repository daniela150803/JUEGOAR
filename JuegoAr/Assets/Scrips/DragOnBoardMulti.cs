using System.Collections.Generic;
using UnityEngine;

public class DragOnBoardMulti : MonoBehaviour
{
    [SerializeField] bool lockAfterDrop = false;
    [SerializeField] float rayDistance = 25f;

    Camera _cam;
    BoardAnchor _board;

    // cartas registradas para poder arrastrarlas
    readonly List<Transform> _movables = new();
    // bloqueo por carta (opcional)
    readonly HashSet<Transform> _locked = new();

    Transform _dragging;
    Vector3  _offsetUp;

    public void SetBoard(BoardAnchor board)
    {
        _board = board;
        _movables.Clear();
        _locked.Clear();
        _dragging = null;
    }

    public void RegisterMovable(Transform t)
    {
        if (t != null && !_movables.Contains(t)) _movables.Add(t);
    }

    void Awake()
    {
        _cam = Camera.main;
        if (_cam == null) Debug.LogWarning("[DragMulti] MainCamera no encontrada (Tag=MainCamera).");
    }

    void Update()
    {
        if (_board == null || _cam == null) return;
        if (Input.touchCount == 0) return;

        var t0 = Input.GetTouch(0);

        if (t0.phase == TouchPhase.Began)
        {
            // tocar una carta existente para empezar a arrastrar
            if (Physics.Raycast(_cam.ScreenPointToRay(t0.position), out var hit, rayDistance))
            {
                var ctrl = hit.collider.GetComponentInParent<CardController>();
                if (ctrl != null)
                {
                    var root = ctrl.transform;
                    if (!_locked.Contains(root))
                    {
                        _dragging = root;
                        var r = _dragging.GetComponentInChildren<Renderer>();
                        _offsetUp = (r != null) ? Vector3.up * r.bounds.extents.y : Vector3.zero;
                    }
                }
            }
        }
        else if (t0.phase == TouchPhase.Moved)
        {
            if (_dragging != null && !_locked.Contains(_dragging))
                DragTo(t0.position);
        }
        else if (t0.phase == TouchPhase.Ended || t0.phase == TouchPhase.Canceled)
        {
            if (_dragging != null)
            {
                SnapToNearest(_dragging);
                if (lockAfterDrop) _locked.Add(_dragging);
                _dragging = null;
            }
        }
    }

    void DragTo(Vector2 screenPos)
    {
        var plane = new Plane(_board.transform.up, _board.transform.position);
        var ray = _cam.ScreenPointToRay(screenPos);

        if (plane.Raycast(ray, out float enter))
        {
            var world = ray.GetPoint(enter);
            var local = _board.transform.InverseTransformPoint(world);
            var b = _board.GetLocalBounds();
            local.x = Mathf.Clamp(local.x, b.min.x, b.max.x);
            local.z = Mathf.Clamp(local.z, b.min.z, b.max.z);
            local.y = 0f;

            _dragging.position = _board.transform.TransformPoint(local) + _offsetUp;
        }
    }

    void SnapToNearest(Transform card)
    {
        if (_board.slots == null || _board.slots.Count == 0) return;

        Transform best = null; float bestD = float.MaxValue;
        var pos = card.position;

        foreach (var s in _board.slots)
        {
            float d = (s.position - pos).sqrMagnitude;
            if (d < bestD) { bestD = d; best = s; }
        }
        if (best != null)
            card.position = best.position + _offsetUp;
    }
}
