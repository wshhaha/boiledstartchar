using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class Inventory : MonoBehaviour 
{
    public TextAsset itemlist;
    public Playerstat p;
    public int helmetlv;
    public int helmethp;
    public int curslot;
    public string cursort;
    public string cureft;
    public int curdmg;
    public int curammo;
    public float usingtime;
    public GameObject itembox;
    public List<string> weaponlist;
    public List<int> weaponindex;
    public List<int> remainammoinweapon;
    public List<int> remainammoininventory;
    public List<int> cartridge;

    private void Start()
    {
        weaponlist.Add("dagger");
        weaponindex.Add(0);
        remainammoinweapon.Add(0);
        remainammoininventory.Add(0);
        cartridge.Add(0);
        for (int i = 0; i < 6; i++)
        {
            weaponlist.Add("");
            weaponindex.Add(0);
            remainammoinweapon.Add(0);
            remainammoininventory.Add(0);
            cartridge.Add(0);
        }
        Selectweapon(0, 0);
    }

    public void Frontweapon()
    {
        Beforeslot(curslot);
        Selectweapon(curslot, weaponindex[curslot]);
    }
    public void Beforeslot(int slot)
    {
        int s = slot - 1;
        if(s<0)
        {
            s = 6;
        }
        if (weaponlist[s]=="")
        {   
            Beforeslot(s);
            return;
        }
        curslot = s;
    }
    public void Backweapon()
    {
        Nextslot(curslot);
        Selectweapon(curslot, weaponindex[curslot]);
    }
    public void Nextslot(int slot)
    {
        int s = slot + 1;
        if(s>6)
        {
            s = 0;
        }
        if (weaponlist[s] == "")
        {
            Nextslot(s);
            return;
        }
        curslot = s;
    }

    public void Selectweapon(int slotnum,int itemindex)
    {
        Loaditem(slotnum, itemindex);
    }
    public void Loaditem(int slotnum, int itemindex)
    {
        var item = JSON.Parse(itemlist.text);
        cureft = item[itemindex]["eft"];
        cursort = item[itemindex]["sort"];
        curdmg = item[itemindex]["val"];
        curammo = remainammoinweapon[slotnum];
        usingtime = item[itemindex]["usingtime"];
    }
    
    public void Createitem(int itemindex,int itemstack)
    {
        var item = JSON.Parse(itemlist.text);
        string sort = item[itemindex]["sort"];
        string n = item[itemindex]["name"];
        switch (sort)
        {
            case "melee":
                Throwitem(0);
                Createweapon(0, itemindex, itemstack);
                break;
            case "range":
                if (weaponlist[1] == "" && weaponlist[2] == "")
                {
                    Createweapon(1, itemindex, itemstack);
                    break;
                }
                else if((weaponlist[1] != "" && weaponlist[2] == ""))
                {
                    if (weaponlist[1] == n)
                    {
                        Addammo(1, itemstack);
                    }
                    else
                    {
                        Createweapon(2, itemindex, itemstack);
                    }
                    break;
                }
                else if ((weaponlist[1] != "" && weaponlist[2] != ""))
                {
                    if(n!= weaponlist[1]&&n!= weaponlist[2])
                    {
                        switch (curslot)
                        {
                            case 2:
                                Throwitem(2);
                                Createweapon(2, itemindex, itemstack);
                                break;
                            default:
                                Throwitem(1);
                                Createweapon(1, itemindex, itemstack);
                                break;
                        }
                    }
                    else
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            if (weaponlist[i] == n)
                            {
                                Addammo(i, itemstack);
                                break;
                            }
                        }
                    }
                }
                break;
            case "armor":
                if(helmetlv!=0)
                {
                    GameObject i = Instantiate(itembox, transform.position, transform.rotation);
                    i.GetComponent<Itemstat>().index = helmetlv + 6;
                    i.GetComponent<Itemstat>().bullet = helmethp;
                }
                helmetlv = itemindex - 6;
                helmethp = itemstack;
                break;
            case "insheal":
                switch (itemindex)
                {
                    case 10:
                        if(weaponlist[3] == "")
                        {
                            Createweapon(3, itemindex, itemstack);
                        }
                        else
                        {
                            Addammo(3, itemstack);
                        }
                        break;
                    case 11:
                        if (weaponlist[4] == "")
                        {
                            Createweapon(4, itemindex, itemstack);
                        }
                        else
                        {
                            Addammo(4, itemstack);
                        }
                        break;
                }
                break;
            case "dotheal":
                switch (itemindex)
                {
                    case 12:
                        if (weaponlist[5] == "")
                        {
                            Createweapon(5, itemindex, itemstack);
                        }
                        else
                        {
                            Addammo(5, itemstack);
                        }
                        break;
                    case 13:
                        if (weaponlist[6] == "")
                        {
                            Createweapon(6, itemindex, itemstack);
                        }
                        else
                        {
                            Addammo(6, itemstack);
                        }
                        break;
                }
                break;
        }
    }
    public void Createweapon(int slot,int itemindex,int ammo)
    {
        var item = JSON.Parse(itemlist.text);
        weaponlist[slot] = item[itemindex]["name"];
        weaponindex[slot] = itemindex;
        cartridge[slot] = item[itemindex]["bullet"];
        if(ammo>= item[itemindex]["bullet"])
        {
            remainammoinweapon[slot] = item[itemindex]["bullet"];
            remainammoininventory[slot] = ammo - item[itemindex]["bullet"];
        }
        else
        {
            remainammoinweapon[slot] = ammo;
        }
    }
    public void Throwitem(int slot)
    {
        GameObject i = Instantiate(itembox,transform.position,transform.rotation);
        i.GetComponent<Itemstat>().index = weaponindex[slot];
        i.GetComponent<Itemstat>().bullet = remainammoinweapon[slot]+remainammoininventory[slot];
    }
    public void Addammo(int slot,int ammo)
    {
        remainammoininventory[slot] += ammo;
    }
}