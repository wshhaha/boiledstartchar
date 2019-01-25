using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerstat : MonoBehaviour 
{
    public int hp=100;
    public int dothp;
    public float dothealtime;
    float dothealcool = 5;
    private void Start()
    {
        GetComponentInChildren<Inventory>().p = this;
    }
    private void Update()
    {
        if(dothp>0)
        {
            dothealtime += Time.deltaTime;
            if(dothealtime>=dothealcool)
            {
                dothealtime = 0;
                dothp -= 5;
                if(dothp<0)
                {
                    dothp = 0;
                }
                hp += 5;
                if(hp>=100)
                {
                    hp = 100;
                }
            }
        }
    }
}
