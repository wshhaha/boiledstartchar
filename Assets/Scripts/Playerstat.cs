using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerstat : MonoBehaviour 
{
    public int hp=100;
    public int dothp;
    public int helmetlv;

    private void Start()
    {
        GetComponentInChildren<Itemmanager>().p = this;
    }
}
