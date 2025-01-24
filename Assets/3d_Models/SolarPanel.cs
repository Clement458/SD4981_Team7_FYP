using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SolarPanel : MonoBehaviour
{
    [SerializeField] private string id;
    private bool panelSet = false;
    private GameObject gameObject;

    [ContextMenu("Generate GUID for ID")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    void Start()
    {
        gameObject = GetComponent<GameObject>();
        if (panelSet)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void LoadData(GameData data)
    {
        data.solarPanelsSet.TryGetValue(id, out panelSet);
        if (!panelSet)
        {
            gameObject.SetActive(false);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.solarPanelsSet.ContainsKey(id))
        {
            data.solarPanelsSet.Remove(id);
        }
        data.solarPanelsSet.Add(id, panelSet);
    }
}
