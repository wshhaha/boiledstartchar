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
                cooltime += Time.deltaTime;
                if (cooltime >= iv.usingtime) 
                {
                    cooltime = 0;
                    switch (GetComponentInChildren<Inventory>().cursort)
                    {
                        case "melee":
                            break;
                        case "range":
                            if (iv.curammo > 0)
                            {
                                Fire();
                            }
                            break;
                    }
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
            int dmg = 0;
            switch (GetComponentInChildren<Inventory>().helmetlv)
            {
                case 0:
                    dmg = other.GetComponent<Bulletmoving>().dmg;
                    break;
                case 1:
                    dmg = (int)(other.GetComponent<Bulletmoving>().dmg*.8f);
                    break;
                case 2:
                    dmg = (int)(other.GetComponent<Bulletmoving>().dmg*.7f);
                    break;
                case 3:
                    dmg = (int)(other.GetComponent<Bulletmoving>().dmg*.5f);
                    break;
            }
            Hit(dmg);
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
    void Hit(int dmg)
    {   
        StartCoroutine(Hitbullet(dmg));
        pv.RPC("HitRPC", PhotonTargets.Others,dmg);
    }
    IEnumerator Hitbullet(int num)
    {
        GetComponent<Playerstat>().hp -= num;
        if (GetComponent<Playerstat>().hp <= 0)
        {
            killscore++;
            Destroy(gameObject);
        }
        yield return null;
    }
    void Creategrave()
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Instantiate(itembox, transform.position + new Vector3(i, 0, j), transform.rotation);
            }
        }
    }
    [PunRPC]
    void HitRPC(int dmg)
    {
        StartCoroutine(Hitbullet(dmg));
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
    
    void Backweapon()
    {
        StartCoroutine(Cd());
        pv.RPC("CdRPC", PhotonTargets.Others);
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
        StartCoroutine(Fw());
        pv.RPC("FwRPC", PhotonTargets.Others);
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