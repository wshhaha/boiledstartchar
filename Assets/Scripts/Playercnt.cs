using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class Playercnt : MonoBehaviour
{
    public GameObject itembox;
    Inventory iv;
    public bool firing;
    public UIJoystick stick;
    Vector3 stickpos;
    public float spd = 5;
    public float rotspd = 120;
    public Transform t;
    PhotonView pv;
    Vector3 curpos;
    Quaternion currot;
    public Transform firepos;
    public GameObject bullet;
    public UIEventTrigger firebtn;
    EventDelegate fireon;
    EventDelegate fireoff;
    public int killscore;
    public float cooltime;
    public UIButton pickbtn;
    EventDelegate picking;
    public UILabel dmgtxt;
    EventDelegate cngbw;
    public UIButton backweaponbtn;
    EventDelegate cngfw;
    public UIButton frontweaponbtn;
    public List<GameObject> grounditem;
    float reloadtime = 3;
    float reloadcool;
    int slotnum;
    bool reload;
    Transform checktr;

    void Start()
    {
        iv = GetComponentInChildren<Inventory>();
        frontweaponbtn = GameObject.Find("Frontweapon").GetComponent<UIButton>();
        backweaponbtn = GameObject.Find("Backweapon").GetComponent<UIButton>();
        pickbtn = GameObject.Find("Pickupbtn").GetComponent<UIButton>();
        dmgtxt = GameObject.Find("dmglabel").GetComponent<UILabel>();
        firebtn = GameObject.Find("Firebtn").GetComponent<UIEventTrigger>();
        stick = GameObject.Find("handle").GetComponent<UIJoystick>();
        stick.pl = this;
        pv = GetComponent<PhotonView>();
        t = GetComponent<Transform>();
        pv.ObservedComponents[0] = this;
        if (pv.isMine)
        {
            GameObject.Find("Main Camera").GetComponent<SmoothFollow>().target = t;
            gameObject.tag = "Player";
        }
        else
        {
            gameObject.tag = "Enemy";
        }
        fireon = new EventDelegate(this, "Firebtnon");
        fireoff = new EventDelegate(this, "Firebtnoff");
        firebtn.onPress.Add(fireon);
        firebtn.onRelease.Add(fireoff);
        picking = new EventDelegate(this, "Pickupitem");
        pickbtn.onClick.Add(picking);
        cngbw = new EventDelegate(this, "Backweapon");
        backweaponbtn.onClick.Add(cngbw);
        cngfw = new EventDelegate(this, "Frontweapon");
        frontweaponbtn.onClick.Add(cngfw);
    }
    void Update()
    {
        if (pv.isMine)
        {
            stickpos.x = stick.position.x;
            stickpos.y = 0;
            stickpos.z = stick.position.y;
            Vector3 dir = stickpos;
            dir.y = 0;
            dir.Normalize();
            transform.Translate(stick.position.x / 8 * Time.deltaTime * 0.5f, 0, stick.position.y / 8 * Time.deltaTime * 0.5f, Space.World);
            if (stick.roton == true)
            {   
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotspd * Time.deltaTime);
            }
            if(firing)
            {
                switch (GetComponentInChildren<Inventory>().cursort)
                {
                    case "melee":
                        break;
                    case "range":
                        cooltime += Time.deltaTime;
                        if (cooltime >= iv.usingtime)
                        {
                            cooltime = 0;
                            if (iv.curammo > 0)
                            {
                                Fire();
                            }
                        }
                        break;
                    case "insheal":
                        if(GetComponent<Playerstat>().hp>=100)
                        {
                            break;
                        }
                        if(checktr==transform)
                        {
                            cooltime += Time.deltaTime;
                            if (cooltime >= iv.usingtime)
                            {
                                cooltime = 0;
                                if (iv.curammo > 0)
                                {
                                    Eat();
                                }
                            }
                        }
                        else
                        {
                            cooltime = 0;
                        }
                        break;
                    case "dotheal":
                        if (checktr == transform)
                        {
                            cooltime += Time.deltaTime;
                            if (cooltime >= iv.usingtime)
                            {
                                cooltime = 0;
                                if (iv.curammo > 0)
                                {
                                    Eat();
                                }
                            }
                        }
                        else
                        {
                            cooltime = 0;
                        }
                        break;
                }
            }
            if (reload && slotnum == iv.curslot)
            {
                if (iv.remainammoininventory[slotnum] > 0)
                {
                    reloadcool += Time.deltaTime;
                    if (reloadcool >= reloadtime)
                    {
                        reloadcool = 0;
                        reload = false;
                        if (iv.remainammoininventory[slotnum] >= iv.cartridge[slotnum])
                        {
                            iv.curammo = iv.cartridge[slotnum];
                            iv.remainammoinweapon[slotnum] = iv.cartridge[slotnum];
                            iv.remainammoininventory[slotnum] -= iv.cartridge[slotnum];
                        }
                        else
                        {
                            iv.curammo = iv.remainammoininventory[slotnum];
                            iv.remainammoinweapon[slotnum] = iv.remainammoininventory[slotnum];
                            iv.remainammoininventory[slotnum] = 0;
                        }
                    }
                }
                else
                {
                    reloadcool = 0;
                    reload = false;
                    iv.weaponlist[slotnum] = null;
                    iv.weaponindex[slotnum] = 0;
                    iv.cartridge[slotnum] = 0;
                }
            }
            else
            {
                reloadcool = 0;
            }
        }
        else
        {
            t.position = Vector3.Lerp(t.position, curpos, Time.deltaTime * 10);
            t.rotation = Quaternion.Lerp(t.rotation, currot, Time.deltaTime * 10);
        }
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(t.position);
            stream.SendNext(t.rotation);
        }
        else
        {
            curpos = (Vector3)stream.ReceiveNext();
            currot = (Quaternion)stream.ReceiveNext();
        }
    }
    public void Firebtnon()
    {
        checktr = transform;
        firing = true;
    }
    public void Firebtnoff()
    {
        firing = false;
        cooltime = 0;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemybullet")
        {
            int dmgtop = 0;
            int dmgtoh = 0;
            switch (GetComponentInChildren<Inventory>().helmetlv)
            {
                case 0:
                    dmgtop = other.GetComponent<Bulletmoving>().dmg;
                    break;
                case 1:
                    dmgtop = (int)(other.GetComponent<Bulletmoving>().dmg * .8f);
                    dmgtoh = (int)(other.GetComponent<Bulletmoving>().dmg * .2f);
                    break;
                case 2:
                    dmgtop = (int)(other.GetComponent<Bulletmoving>().dmg * .7f);
                    dmgtoh = (int)(other.GetComponent<Bulletmoving>().dmg * .3f);
                    break;
                case 3:
                    dmgtop = (int)(other.GetComponent<Bulletmoving>().dmg * .6f);
                    dmgtoh = (int)(other.GetComponent<Bulletmoving>().dmg * .4f);
                    break;
            }
            Hit(dmgtop, dmgtoh, other.GetComponent<Bulletmoving>().master.name);
            dmgtxt.text = other.GetComponent<Bulletmoving>().dmg.ToString();
            Destroy(other.gameObject);
        }
        if(other.gameObject.tag=="Item")
        {   
            grounditem.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            grounditem.Remove(other.gameObject);
        }
    }
    void Hit(int dmg, int dmg2, string username)
    {
        StartCoroutine(Hitbullet(dmg, dmg2, username));
        pv.RPC("HitRPC", PhotonTargets.Others, dmg, dmg2, username);
    }
    IEnumerator Hitbullet(int dmg, int dmg2, string username)
    {
        GetComponent<Playerstat>().hp -= dmg;
        iv.helmethp -= dmg2;
        if (iv.helmethp <= 0)
        {
            iv.helmetlv = 0;
            iv.helmethp = 0;
        }
        if (GetComponent<Playerstat>().hp <= 0)
        {
            Creategrave();
            GameObject.Find(username).GetComponent<Playercnt>().killscore++;
            gameObject.SetActive(false);
        }
        yield return null;
    }
    void Creategrave()
    {
        int num = 0;
        List<int> indexlist = new List<int>();
        List<int> ammolist = new List<int>();
        List<GameObject> gravelist = new List<GameObject>();
        List<Vector3> gravepos = new List<Vector3>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                gravepos.Add(transform.position + new Vector3(i, 0, j));
            }
        }
        for (int i = 0; i < 7; i++)
        {
            if (iv.weaponlist[i] != "")
            {
                num++;
                indexlist.Add(iv.weaponindex[i]);
                ammolist.Add(iv.remainammoinweapon[i] + iv.remainammoininventory[i]);
            }
        }
        for (int i = 0; i < num; i++)
        {
            GameObject grave = Instantiate(itembox,transform.position,Quaternion.identity);
            grave.GetComponent<Itemstat>().index = indexlist[i];
            grave.GetComponent<Itemstat>().bullet = ammolist[i];
            gravelist.Add(grave);
        }
        if (num > 1)
        {
            for (int i = 0; i < num; i++)
            {
                gravelist[i].transform.position = gravepos[i];
            }
        }
    }
    [PunRPC]
    void HitRPC(int dmg, int dmg2, string username)
    {
        StartCoroutine(Hitbullet(dmg, dmg2, username));
    }
    void Fire()
    {
        StartCoroutine(Createbullet());
        pv.RPC("FireRPC", PhotonTargets.Others);
    }
    IEnumerator Createbullet()
    {
        GameObject b = Instantiate(bullet, firepos.position, firepos.rotation);
        b.GetComponent<Bulletmoving>().dmg = GetComponentInChildren<Inventory>().curdmg;
        iv.curammo--;
        iv.remainammoinweapon[iv.curslot]--;
        b.GetComponent<Bulletmoving>().master = gameObject;
        if (pv.isMine)
        {   
            b.tag = "Playerbullet";
        }
        else
        {
            b.tag = "Enemybullet";
        }
        if(iv.curammo<=0)
        {
            reload = true;
            slotnum = iv.curslot;
        }
        yield return null;
    }
    [PunRPC]
    void FireRPC()
    {
        StartCoroutine(Createbullet());
    }

    void Eat()
    {
        StartCoroutine(Healing());
        pv.RPC("HealingRPC", PhotonTargets.Others);
    }
    IEnumerator Healing()
    {
        switch (iv.cursort)
        {
            case "insheal":
                GetComponent<Playerstat>().hp += iv.curdmg;
                if (GetComponent<Playerstat>().hp > 100)
                {
                    GetComponent<Playerstat>().hp = 100;
                }
                break;
            case "dotheal":
                GetComponent<Playerstat>().dothp += iv.curdmg;
                if (GetComponent<Playerstat>().dothp > 100)
                {
                    GetComponent<Playerstat>().dothp = 100;
                }
                break;

        }
        iv.curammo--;
        iv.remainammoinweapon[iv.curslot]--;
        
        if (iv.curammo <= 0)
        {
            reload = true;
            slotnum = iv.curslot;
        }
        yield return null;
    }
    [PunRPC]
    void HealingRPC()
    {
        StartCoroutine(Healing());
    }

    void Backweapon()
    {
        if (pv.isMine)
        {
            cooltime = 0;
            StartCoroutine(Cd());
            pv.RPC("CdRPC", PhotonTargets.Others);
        }
    }
    IEnumerator Cd()
    {   
        iv.Backweapon();
        yield return null;
    }
    [PunRPC]
    void CdRPC()
    {
        StartCoroutine(Cd());
    }

    void Frontweapon()
    {
        if (pv.isMine)
        {
            cooltime = 0;
            StartCoroutine(Fw());
            pv.RPC("FwRPC", PhotonTargets.Others);
        }
    }
    IEnumerator Fw()
    {   
        iv.Frontweapon();
        yield return null;
    }
    [PunRPC]
    void FwRPC()
    {
        StartCoroutine(Fw());
    }

    void Pickupitem()
    {
        if (grounditem.Count == 0)
        {
            return;
        }
        StartCoroutine(Pick());
        pv.RPC("PickRPC", PhotonTargets.Others);
    }
    IEnumerator Pick()
    {
        Copyitemname(grounditem[0]);
        Destroy(grounditem[0]);
        grounditem.RemoveAt(0);
        yield return null;
    }
    [PunRPC]
    void PickRPC()
    {
        StartCoroutine(Pick());
    }
    
    void Copyitemname(GameObject item)
    {   
        Itemstat i = item.GetComponent<Itemstat>();
        iv.Createitem(i.index, i.bullet);
    }
}