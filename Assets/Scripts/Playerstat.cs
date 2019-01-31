using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerstat : MonoBehaviour 
{
    public int hp=100;
    public int dothp;
    public float dothealtime;
    public float dothealcool = 5;
    public bool poison;
    public int poisonstack;
    public bool fire;
    public int firestack;
    public bool ice;
    public int icestack;

    private void Start()
    {   
        GetComponentInChildren<Inventory>().p = this;
    }
}
