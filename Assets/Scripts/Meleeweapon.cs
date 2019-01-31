using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meleeweapon : MonoBehaviour
{
    public bool ishand;
    public GameObject master;

    public void OnTriggerEnter(Collider other)
    {
        if(ishand)
        {
            if(other.gameObject.tag=="Enemy")
            {
                int dmgtop = 0;
                int dmgtoh = 0;
                switch (other.GetComponentInChildren<Inventory>().helmetlv)
                {
                    case 0:
                        dmgtop = master.GetComponentInChildren<Inventory>().curdmg;
                        break;
                    case 1:
                        dmgtop = (int)(master.GetComponentInChildren<Inventory>().curdmg * .8f);
                        dmgtoh = (int)(master.GetComponentInChildren<Inventory>().curdmg * .2f);
                        break;
                    case 2:
                        dmgtop = (int)(master.GetComponentInChildren<Inventory>().curdmg * .7f);
                        dmgtoh = (int)(master.GetComponentInChildren<Inventory>().curdmg * .3f);
                        break;
                    case 3:
                        dmgtop = (int)(master.GetComponentInChildren<Inventory>().curdmg * .6f);
                        dmgtoh = (int)(master.GetComponentInChildren<Inventory>().curdmg * .4f);
                        break;
                }
                other.GetComponent<Playercnt>().Hit(dmgtop, dmgtoh, master.name);
            }
        }
    }
}
