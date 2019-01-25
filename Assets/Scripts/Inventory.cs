using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class Inventory : MonoBehaviour 
{
    public TextAsset itemlist;
    public Playerstat p;
    public int helmetlv;

    public int curslot;
    public string cursort;
    public string cureft;
    public int curdmg;
    public int curammo;
    public int curstack;
    public float usingtime;

    //public string meleeweapon;
    //public string rangeweapon1;
    //public int range1ammo;
    //public int range1stack;
    //public string rangeweapon2;
    //public int range2ammo;
    //public int range2stack;
    //public string instantheal1;
    //public int heal1stack;
    //public string instantheal2;
    //public int heal2stack;
    //public string dotheal1;
    //public int dot1stack;
    //public string dotheal2;
    //public int dot2stack;
    public List<string> weaponlist;
    public List<int> weaponindex;
    public List<int> remainammoinweapon;
    public List<int> remainammoininventory;

    private void Start()
    {
        weaponlist.Add("dagger");
        weaponindex.Add(0);
        remainammoinweapon.Add(0);
        remainammoininventory.Add(0);
        for (int i = 0; i < 6; i++)
        {
            weaponlist.Add("");
            weaponindex.Add(0);
            remainammoinweapon.Add(0);
            remainammoininventory.Add(0);
        }
        Selectweapon(0, 0);
    }

    public void Frontweapon()
    {
        if (curslot > 0)
        {
            curslot--;
            Selectweapon(curslot,weaponindex[curslot]);
        }
    }
    public void Backweapon()
    {
        if(curslot<7)
        {
            curslot++;
            Selectweapon(curslot, weaponindex[curslot]);
        }
    }
    public void Selectweapon(int slotnum,int itemindex)
    {   
        switch (slotnum)
        {
            case 0:
                Loaditem(slotnum, itemindex);
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
        }
    }
    public void Loaditem(int slotnum, int itemindex)
    {
        var item = JSON.Parse(itemlist.text);
        cureft = item[itemindex]["eft"];
        cursort = item[itemindex]["sort"];
        curdmg = item[itemindex]["val"];
        curammo = remainammoinweapon[slotnum];
        curstack = remainammoininventory[slotnum];
        usingtime = item[itemindex]["usingtime"];
    }
}