using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackingHandler : MonoBehaviour
{
    [Header("Prefabs")]
<<<<<<< Updated upstream
    [SerializeField] private GameObject boardAnchorPrefab;   // BoardAnchor (vacío con el script)
    [SerializeField] private GameObject spherePrefab;        // tu esfera

    [Header("Interaction")]
    [SerializeField] private DragOnBoard drag;               // tu script de arrastre sobre tablero

    [Header("Spawn")]
    [SerializeField] private float outsideOffset = 0.03f;    // 3 cm fuera del borde inferior

    private ARTrackedImageManager _mgr;
=======
    [SerializeField] private GameObject boardAnchorPrefab;   // BoardAnchor
    [SerializeField] private GameObject cardBasePrefab;      // CardBase (con CardController)

    [Header("Cartas disponibles")]
    [SerializeField] private List<CardData> availableCards = new(); // agrega aquí tus CardData

    [Tooltip("Cantidad inicial de cartas a crear al detectar la imagen")]
    [SerializeField] private int spawnCount = 3;

    [Tooltip("Elegir carta al instanciar")]
    [SerializeField] private bool randomPick = true; // false = secuencial

    [Header("Interacción (arrastre)")]
    [SerializeField] private DragOnBoardMulti drag;  // componente del paso 2

    [Header("Posición inicial (fuera del tablero)")]
    [SerializeField] private float outsideOffset = 0.03f; // 3 cm fuera del borde inferior
    [SerializeField] private float spawnSpacing = 0.05f;  // separación lateral entre cartas

    private ARTrackedImageManager _manager;
    private readonly Dictionary<TrackableId, BoardAnchor> _boards = new();
    private int _seqIndex = 0; // para modo secuencial
>>>>>>> Stashed changes

    void Awake() => _mgr = GetComponent<ARTrackedImageManager>();
    void OnEnable()  => _mgr.trackedImagesChanged += OnChanged;
    void OnDisable() => _mgr.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs e)
    {
<<<<<<< Updated upstream
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
=======
        foreach (var img in e.added)   CreateBoardAndCards(img);
        foreach (var img in e.updated) UpdateVisibility(img);
        foreach (var img in e.removed) RemoveBoard(img);
    }

    void CreateBoardAndCards(ARTrackedImage img)
    {
        if (boardAnchorPrefab == null || cardBasePrefab == null || drag == null)
        {
            Debug.LogError("[ITH] Falta asignar BoardAnchor / CardBase / DragOnBoardMulti.");
            return;
        }
        if (availableCards == null || availableCards.Count == 0)
        {
            Debug.LogError("[ITH] No hay CardData en 'availableCards'.");
            return;
        }

        // 1) Tablero
        var boardGO = Instantiate(boardAnchorPrefab, img.transform);
        var board   = boardGO.GetComponent<BoardAnchor>();
        board.InitializeFromTrackedImage(img);
        _boards[img.trackableId] = board;

        // 2) Conectar drag con este tablero
        drag.SetBoard(board);

        // 3) Instanciar varias cartas alineadas bajo el borde inferior
        float bottomEdge = -board.boardSize.y * 0.5f;
        float startX = -(spawnSpacing * (spawnCount - 1)) * 0.5f; // centrar en X

        for (int i = 0; i < spawnCount; i++)
        {
            var data = PickCardData();
            var cardGO = Instantiate(cardBasePrefab, board.transform);
            var ctrl = cardGO.GetComponent<CardController>();
            if (ctrl != null) ctrl.SetData(data);

            // posición local: x distribuido, z fuera del borde inferior
            float x = startX + i * spawnSpacing;
            cardGO.transform.localPosition = new Vector3(x, 0f, bottomEdge - outsideOffset);

            // levantar según su altura
            var r = cardGO.GetComponentInChildren<Renderer>();
            if (r != null) cardGO.transform.position += board.transform.up * r.bounds.extents.y;

            // registrar en el sistema de arrastre
            drag.RegisterMovable(cardGO.transform);
        }
    }

    CardData PickCardData()
    {
        if (randomPick)
            return availableCards[Random.Range(0, availableCards.Count)];

        // secuencial
        var data = availableCards[_seqIndex % availableCards.Count];
        _seqIndex++;
        return data;
    }

    void UpdateVisibility(ARTrackedImage img)
    {
        if (_boards.TryGetValue(img.trackableId, out var board))
        {
            bool visible = img.trackingState == TrackingState.Tracking;
            board.gameObject.SetActive(visible);
        }
    }

    void RemoveBoard(ARTrackedImage img)
    {
        if (_boards.TryGetValue(img.trackableId, out var board))
        {
            Destroy(board.gameObject);
            _boards.Remove(img.trackableId);
>>>>>>> Stashed changes
        }
    }
}
