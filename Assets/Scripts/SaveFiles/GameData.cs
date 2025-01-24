using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameData
{
    public int ironOre;
    public int rocks;
    public float veggies;
    public float water;
    public Dictionary<string, bool> solarPanelsSet;

    public float maxWater = 10f;


    public GameData()
    {
        this.ironOre = 0;
        this.rocks = 0; 
        this.veggies = 0;
        this.water = 1f;
    }
}
