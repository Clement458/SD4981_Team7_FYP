using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, IDataPersistence
{
    public static GameManager instance {  get; private set; }
    public List<SolarPanel> solarPanels;

    public Text ironOreText;
    public Text rocksText;
    public GameData gameData;

    private int ironOre;
    private int rocks;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GameManager found in the scene.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BuildSolarPanels();
        }        
    }

    private void Update()
    {
        UpdateIronOreUI();
        UpdateRocksUI();
    }

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadData(GameData data)
    {
        ironOre = data.ironOre;
        rocks = data.rocks;
        UpdateSolarPanels();
    }

    public void SaveData(ref GameData data)
    {
        data.ironOre = ironOre;
        data.rocks = rocks;
    }

    private void UpdateSolarPanels()
    {
        foreach (SolarPanel panel in solarPanels)
        {
            panel.LoadData(gameData);
        }
    }

    private void BuildSolarPanels()
    {
        if (gameData.ironOre >= 2 && gameData.rocks >= 1)
        {
            int activatedCount = 0;

            foreach (SolarPanel panel in solarPanels)
            {
                if (!panel.gameObject.activeSelf)
                {
                    panel.gameObject.SetActive(true);
                    panel.panelSet = true;
                    activatedCount++;
                    if (activatedCount >= 2) break;
                }
            }

            if (activatedCount >= 2)
            {
                gameData.ironOre -= 2;
                gameData.rocks -= 1;
                Debug.Log("Build successful! Iron: " + gameData.ironOre + ", Rocks: " + gameData.rocks);

                // Save updated data
                DataPersistenceManager.instance.SaveGame();
            }
            else
            {
                Debug.Log("Solar Panels: Maximum reached");
            }
        }
        else
        {
            Debug.Log("Solar Panels: Not enough resources");
        }
    }

    public int GetCollectedIron()
    {
        return gameData.ironOre;
    }

    public void SetCollectedIron(int value)
    {
        gameData.ironOre = value;
    }

    public int GetCollectedRocks()
    {
        return gameData.rocks;
    }

    public void SetCollectedRocks(int value)
    {
        gameData.rocks = value;
    }

    private void UpdateIronOreUI()
    {
        ironOreText.text = "Iron Ore: " + ironOre;
    }

    private void UpdateRocksUI()
    {
        rocksText.text = "Rocks: " + rocks;
    }
}
