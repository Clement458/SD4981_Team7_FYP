using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarPanel : MonoBehaviour
{
    [SerializeField] public string id;
    public bool panelSet = false;

    [ContextMenu("Generate GUID for ID")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }


    public void SetActiveState(bool state)
    {
        panelSet = state;
        gameObject.SetActive(state);
    }

    public void LoadData(SerializableDictionary<string, bool> data)
    {
        if (data.TryGetValue(id, out bool storedPanelSet))
        {
            SetActiveState(storedPanelSet);
        }
        else
        {
            SetActiveState(false);
        }
    }


}
