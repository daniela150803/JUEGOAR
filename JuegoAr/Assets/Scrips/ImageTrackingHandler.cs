using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackingHandler : MonoBehaviour
{
    [SerializeField] public GameObject spherePrefab;   // tu prefab Sphere
    [SerializeField] public DragOnTrackedImage drag;    // arrastra el componente DragOnTrackedImage del XR Origin

    private ARTrackedImageManager _mgr;

    void Awake() => _mgr = GetComponent<ARTrackedImageManager>();
    void OnEnable()  => _mgr.trackedImagesChanged += OnChanged;
    void OnDisable() => _mgr.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs e)
    {
        foreach (var img in e.added)
        {
            // Instancia la esfera como hija de la imagen detectada
            var sphere = Instantiate(spherePrefab, img.transform);
            // Asigna referencias al script de “arrastrar”
            drag.Movable = sphere;
            drag.Tracked = img;
        }
    }
}
