using UnityEngine;

public class BoardPlacer : MonoBehaviour
{
    public BoardAnchor boardAnchor;     // Referencia al tablero
    public GameObject[] objectPrefabs;  // Prefabs que quieres poner en los slots

    void Start()
    {
        PlaceObjectsOnBoard();
    }

    void PlaceObjectsOnBoard()
    {
        if (boardAnchor == null || objectPrefabs.Length == 0) return;

        for (int i = 0; i < boardAnchor.slots.Count; i++)
        {
            // Escoger el prefab (rotamos entre ellos con el módulo %)
            GameObject prefab = objectPrefabs[i % objectPrefabs.Length];

            // Instanciar en el slot
            Transform slot = boardAnchor.slots[i];
            GameObject obj = Instantiate(prefab, slot.position, slot.rotation);

            // Hacer hijo del slot (así queda fijo en el tablero)
            obj.transform.SetParent(slot, true);
        }
    }
}
