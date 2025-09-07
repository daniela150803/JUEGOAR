using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackingHandler : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject boardAnchorPrefab;   // BoardAnchor (vacío con el script)
    [SerializeField] private GameObject spherePrefab;        // tu esfera

    [Header("Interaction")]
    [SerializeField] private DragOnBoard drag;               // tu script de arrastre sobre tablero

    [Header("Spawn")]
    [SerializeField] private float outsideOffset = 0.03f;    // 3 cm fuera del borde inferior

    private ARTrackedImageManager _mgr;

    void Awake() => _mgr = GetComponent<ARTrackedImageManager>();
    void OnEnable()  => _mgr.trackedImagesChanged += OnChanged;
    void OnDisable() => _mgr.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs e)
    {
        foreach (var img in e.added)
        {
            // 1) Instancia y prepara el tablero
            var boardGO = Instantiate(boardAnchorPrefab, img.transform);
            var board   = boardGO.GetComponent<BoardAnchor>();
            board.InitializeFromTrackedImage(img); // ahora board.boardSize está listo

            // 2) Instancia la esfera como hija del tablero
            GameObject sphere = null;
            if (spherePrefab != null)
            {
                sphere = Instantiate(spherePrefab, board.transform);

                // Altura para que no se "hunda"
                var rend   = sphere.GetComponentInChildren<Renderer>();
                var upOffY = rend ? rend.bounds.extents.y : 0f;

                // Posición local: x=0 (centro), z = borde inferior - fuera
                float bottomEdge = -board.boardSize.y * 0.5f;          // borde inferior
                sphere.transform.localPosition = new Vector3(0f, 0f, bottomEdge - outsideOffset);

                // Sube un poquito en el eje "up" del tablero
                sphere.transform.position += board.transform.up * upOffY;
            }

            // 3) Pasar referencias al controlador de arrastre
            if (drag != null)
                drag.SetContext(img, board, sphere);
        }
    }
}
