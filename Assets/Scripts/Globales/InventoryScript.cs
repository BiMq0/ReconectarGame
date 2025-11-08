using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    private Dictionary<string, GameObject> itemsEnInventario = new Dictionary<string, GameObject>();

    private static InventoryScript instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static InventoryScript Instance => instance;

    public void AgregarItem(string nombreItem, GameObject item)
    {
        if (!itemsEnInventario.ContainsKey(nombreItem))
        {
            itemsEnInventario.Add(nombreItem, item);
            Debug.Log($"Item agregado: {nombreItem}");
        }
        else
        {
            Debug.Log($"El item '{nombreItem}' ya está en el inventario, esta vaina no deberia pasar porque los items desaparecen o se usan una unica vez xd");
        }
    }

    public void EliminarItem(string nombreItem)
    {
        if (itemsEnInventario.ContainsKey(nombreItem))
        {
            itemsEnInventario.Remove(nombreItem);
        }
        else
        {
            Debug.Log($"No se encontró el item '{nombreItem}' en el inventario, debe estar en otra escena xd");
        }
    }

    public bool TieneItem(string nombreItem)
    {
        return itemsEnInventario.ContainsKey(nombreItem);
    }

    public void VaciarInventario()
    {
        itemsEnInventario.Clear();
    }

    public void MostrarInventario()
    {
        foreach (var kvp in itemsEnInventario)
        {
            Debug.Log($"• {kvp.Key}");
        }
    }
}
