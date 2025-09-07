using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BoardAnchor : MonoBehaviour
{
    [Header("Grid")]
    public int rows = 3;
    public int cols = 4;
    public float slotMargin = 0.01f; // margen entre slots (m)

    [Header("Visual (opcional)")]
    public bool drawQuads = true;
    public Material slotMaterial;     // asigna un material simple si quieres ver los quads

    [HideInInspector] public Vector2 boardSize; // (x = ancho, y = alto) en metros
    [HideInInspector] public List<Transform> slots = new();

    // Llama esto desde fuera pasándole el ARTrackedImage detectado
    public void InitializeFromTrackedImage(ARTrackedImage tracked)
    {
        // El tamaño real de la imagen en AR Foundation
        boardSize = tracked.size; // (width, height) en metros

        // Alinear al centro de la imagen: este BoardAnchor será hijo de la imagen,
        // así que su (0,0,0) ya coincide con su centro.
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Limpia si había algo
        foreach (Transform child in transform) Destroy(child.gameObject);
        slots.Clear();

        // Celdas internas (descontando márgenes)
        float cellW = (boardSize.x - (cols + 1) * slotMargin) / cols;
        float cellH = (boardSize.y - (rows + 1) * slotMargin) / rows;

        // Origen local (esquina inferior-izquierda del tablero en local)
        float startX = -boardSize.x * 0.5f + slotMargin + cellW * 0.5f;
        float startZ = -boardSize.y * 0.5f + slotMargin + cellH * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var go = new GameObject($"Slot_{r}_{c}");
                go.transform.SetParent(transform, false);

                float x = startX + c * (cellW + slotMargin);
                float z = startZ + r * (cellH + slotMargin);
                go.transform.localPosition = new Vector3(x, 0f, z);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                slots.Add(go.transform);

                // Visual opcional: quad del tamaño de la celda
                if (drawQuads && slotMaterial != null)
                {
                    var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = "Quad";
                    quad.transform.SetParent(go.transform, false);
                    quad.transform.localRotation = Quaternion.Euler(90, 0, 0); // acostado
                    quad.transform.localScale = new Vector3(cellW, cellH, 1f);
                    var mr = quad.GetComponent<MeshRenderer>();
                    mr.material = slotMaterial;
                    var col = quad.GetComponent<Collider>();
                    if (col) Destroy(col); // no necesitamos colisión aquí
                }
            }
        }
    }

    // Bounds locales del tablero (para limitar arrastre)
    public Bounds GetLocalBounds()
    {
        // centrado en (0,0,0), ancho=boardSize.x, alto=boardSize.y (en z)
        var center = Vector3.zero;
        var size = new Vector3(boardSize.x, 0.001f, boardSize.y);
        return new Bounds(center, size);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (boardSize != Vector2.zero)
        {
            Gizmos.color = Color.cyan;
            var b = GetLocalBounds();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
#endif
}
