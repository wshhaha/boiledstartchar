using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class Playercnt : MonoBehaviour
{
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
    public float atktime=1f;
    public float cooltime;
    public UIButton pickbtn;
    EventDelegate picking;
    public UILabel dmgtxt;
    EventDelegate cngdmg;
    public UIButton cngbtn;
    public List<GameObject> grounditem;

    void Start()
    {   
        cngbtn = GameObject.Find("Changebtn").GetComponent<UIButton>();
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
        cngdmg = new EventDelegate(this, "Changedmg");
        cngbtn.onClick.Add(cngdmg);
    }
    void Update()
    {
        if (pv.isMine)
        {
            stickpos.x = stick.position.x;
            stickpos.y = 0;
            stickpos.z = stick.position.y;
            Vector3 dir = stickpos - transform.position;
            dir.y = 0;
            dir.Normalize();
            transform.Translate(stick.position.x / 8 * Time.deltaTime * 0.5f, 0, stick.position.y / 8 * Time.deltaTime * 0.5f, Space.World);
            if (stick.roton == true)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);
            }
            if(firing)
            {
                cooltime += Time.deltaTime;
                if(cooltime>=atktime)
                {
                    cooltime = 0;
                    switch (GetComponentInChildren<Inventory>().cursort)
                    {
                        case "melee":
                            break;
                        case "range":
                            Fire();
                            break;
                    }
                }
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
            print(dmg);
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
        print(dmg);
        StartCoroutine(Hitbullet(dmg));
        pv.RPC("HitRPC", PhotonTargets.Others,dmg);
    }
    IEnumerator Hitbullet(int num)
    {
        GetComponent<Playerstat>().hp -= num;
        if (GetComponent<Playerstat>().hp <= 0)
        {
            Destroy(gameObject);
        }
        yield return null;
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
        GameObject b= Instantiate(bullet, firepos.position, firepos.rotation);
        b.GetComponent<Bulletmoving>().dmg = GetComponentInChildren<Inventory>().curdmg;
        if (pv.isMine)
        {   
            b.tag = "Playerbullet";
        }
        else
        {
            b.tag = "Enemybullet";
        }
        yield return null;
    }
    [PunRPC]
    void FireRPC()
    {
        StartCoroutine(Createbullet());
    }

    void Changedmg()
    {   
        int num= Random.Range(5, 16);
        StartCoroutine(Cd(num));
        pv.RPC("CdRPC", PhotonTargets.Others, num);
    }
    IEnumerator Cd(int dmg)
    {
        GetComponentInChildren<Inventory>().curdmg = dmg;
        yield return null;
    }
    [PunRPC]
    void CdRPC(int num)
    {
        StartCoroutine(Cd(num));
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
        Inventory iv = GetComponentInChildren<Inventory>();
        switch (i.sort)
        {
            case "melee":
                //string throwname = iv.meleeweapon;
                //iv.meleeweapon = i.name;
                //iv.curdmg = i.val;
                //iv.usingtime = i.usingtime;
                break;
            case "range":
                break;
            case "armor":
                break;
            case "heal":
                break;
        }
    }
}