using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class Playercnt : MonoBehaviour
{
    float basespd;
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
    public UILabel hptxt;
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
    bool swing;
    float ptime;
    float ftime;
    float itime;

    void Start()
    {
        basespd = spd;
        iv = GetComponentInChildren<Inventory>();
        frontweaponbtn = GameObject.Find("Frontweapon").GetComponent<UIButton>();
        backweaponbtn = GameObject.Find("Backweapon").GetComponent<UIButton>();
        pickbtn = GameObject.Find("Pickupbtn").GetComponent<UIButton>();
        dmgtxt = GameObject.Find("dmglabel").GetComponent<UILabel>();
        hptxt = GameObject.Find("hplabel").GetComponent<UILabel>();
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
        //버튼 동적 할당
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
            hptxt.text = GetComponent<Playerstat>().hp.ToString();
            //내 캐릭터 움직임
            stickpos.x = stick.position.x;
            stickpos.y = 0;
            stickpos.z = stick.position.y;
            Vector3 dir = stickpos;
            dir.y = 0;
            dir.Normalize();
            transform.Translate(dir * spd * Time.deltaTime, Space.World);
            if (stick.roton)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotspd * Time.deltaTime);
            }
            //상태이상 프로세스
            if(GetComponent<Playerstat>().poison)
            {
                ptime += Time.deltaTime;
                if (ptime >= 1)
                {
                    ptime = 0;
                    Poisoning();
                }
            }
            if (GetComponent<Playerstat>().fire)
            {
                ftime += Time.deltaTime;
                if (ftime >= 1)
                {
                    ftime = 0;
                    Firing();
                }
            }
            if (GetComponent<Playerstat>().ice)
            {
                itime += Time.deltaTime;
                if (itime >= 1)
                {
                    itime = 0;
                    Firing();
                }
            }
            //도트힐 실행
            if (GetComponent<Playerstat>().dothp > 0)
            {
                Dotheal();
            }
            //근접무기 쿨타임 적용
            if (swing)
            {
                cooltime += Time.deltaTime;
                if(cooltime>=iv.usingtime)
                {
                    cooltime = 0;
                    swing = false;
                }
            }
            //근접무기 피격판정 켜기
            //if(휘두르는 모션.isplaying)
            //{
            //    GetComponentInChildren<Meleeweapon>().ishand = true;
            //}
            //else
            //{
            //    GetComponentInChildren<Meleeweapon>().ishand = false;
            //}
            //아이템 및 무기 사용 프로세스
            if(firing)
            {
                switch (GetComponentInChildren<Inventory>().cursort)
                {
                    case "melee":
                        if (!swing)
                        {
                            //휘두르는 모션 넣기
                            swing = true;
                        }
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
            //재장전 프로세스
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
        //다른 캐릭터 움직임 동기화
        else
        {
            t.position = Vector3.Lerp(t.position, curpos, Time.deltaTime * 10);
            t.rotation = Quaternion.Lerp(t.rotation, currot, Time.deltaTime * 10);
        }
    }
    //다른 캐릭터 움직임 데이터 받아오기
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
    //발사버튼(onpress)
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
    //피격처리 및 아이템 줍기 준비
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemybullet")
        {
            Bulletsort(other.GetComponent<Bulletmoving>().eft);
            if (other.GetComponent<Bulletmoving>().eft == "knock")
            {
                Vector3 vec = other.transform.forward;
                transform.Translate(vec, Space.World);
            }
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
            dmgtxt.text = GetComponent<Playerstat>().poisonstack.ToString();
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
    //피격 프로세스
    public void Hit(int dmg, int dmg2, string username)
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
    //피격 프로세스 중 사망시 무덤 생성
    void Creategrave()
    {
        int num = 0;
        List<int> indexlist = new List<int>();
        List<int> ammolist = new List<int>();
        List<GameObject> gravelist = new List<GameObject>();
        List<Vector3> gravepos = new List<Vector3>();
        for (float i = -1.5f; i < 2; i += 1.5f)
        {
            for (float j = -1.5f; j < 2; j += 1.5f)
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
    //발사 프로세스
    void Fire()
    {
        StartCoroutine(Createbullet());
        pv.RPC("FireRPC", PhotonTargets.Others);
    }
    IEnumerator Createbullet()
    {
        GameObject b = Instantiate(bullet, firepos.position, firepos.rotation);
        b.GetComponent<Bulletmoving>().dmg = GetComponentInChildren<Inventory>().curdmg;
        b.GetComponent<Bulletmoving>().eft = GetComponentInChildren<Inventory>().cureft;
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
    //회복템 사용 프로세스
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
    //도트힐 체력회복 프로세스
    void Dotheal()
    {
        StartCoroutine(Dh());
        pv.RPC("DhRPC", PhotonTargets.Others);
    }
    IEnumerator Dh()
    {
        GetComponent<Playerstat>().dothealtime += Time.deltaTime;
        if (GetComponent<Playerstat>().dothealtime >= GetComponent<Playerstat>().dothealcool)
        {
            GetComponent<Playerstat>().dothealtime = 0;
            GetComponent<Playerstat>().dothp -= 5;
            if (GetComponent<Playerstat>().dothp < 0)
            {
                GetComponent<Playerstat>().dothp = 0;
            }
            GetComponent<Playerstat>().hp += 5;
            if (GetComponent<Playerstat>().hp >= 100)
            {
                GetComponent<Playerstat>().hp = 100;
            }
        }
        yield return null;
    }
    [PunRPC]
    void DhRPC()
    {
        StartCoroutine(Dh());
    }
    //전무기 선택 프로세스
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
    //후무기 선택 프로세스
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
    //아이템 줍기 프로세스
    void Pickupitem()
    {
        if (pv.isMine)
        {
            if (grounditem.Count == 0)
            {
                return;
            }
            StartCoroutine(Pick());
            pv.RPC("PickRPC", PhotonTargets.Others);
        }
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
    //줍기 중 인벤토리에 아이템 이름을 전송하여 인벤 리스트에 아이템 생성
    void Copyitemname(GameObject item)
    {   
        Itemstat i = item.GetComponent<Itemstat>();
        iv.Createitem(i.index, i.bullet);
    }
    //피격시 탄종류 구별 프로세스
    void Bulletsort(string eft)
    {
        if (pv.isMine)
        {   
            StartCoroutine(Sorting(eft));
            pv.RPC("SortingRPC", PhotonTargets.Others, eft);
        }
    }
    IEnumerator Sorting(string eft)
    {
        switch (eft)
        {
            case "poison":
                GetComponent<Playerstat>().poison = true;
                GetComponent<Playerstat>().poisonstack++;
                break;
            case "dot":
                GetComponent<Playerstat>().fire = true;
                GetComponent<Playerstat>().firestack++;
                break;
            case "slow":
                GetComponent<Playerstat>().ice = true;
                GetComponent<Playerstat>().icestack++;
                break;
            default:
                break;
        }
        yield return null;
    }
    [PunRPC]
    void SortingRPC(string eft)
    {
        StartCoroutine(Sorting(eft));
    }
    //중독 프로세스
    void Poisoning()
    {
        if (pv.isMine)
        {
            StartCoroutine(Poing());
            pv.RPC("PoingRPC", PhotonTargets.Others);
        }
    }
    IEnumerator Poing()
    {
        GetComponent<Playerstat>().hp -= 10;
        if (GetComponent<Playerstat>().hp <= 0)
        {
            Creategrave();
            gameObject.SetActive(false);
        }
        GetComponent<Playerstat>().poisonstack--;
        if (GetComponent<Playerstat>().poisonstack == 0)
        {
            GetComponent<Playerstat>().poison = false;
        }
        yield return null;
    }
    [PunRPC]
    void PoingRPC()
    {
        StartCoroutine(Poing());
    }
    //화상 프로세스
    void Firing()
    {
        if (pv.isMine)
        {
            StartCoroutine(Fing());
            pv.RPC("FingRPC", PhotonTargets.Others);
        }
    }
    IEnumerator Fing()
    {
        GetComponent<Playerstat>().hp -= 10;
        if (GetComponent<Playerstat>().hp <= 0)
        {
            Creategrave();
            gameObject.SetActive(false);
        }
        GetComponent<Playerstat>().firestack--;
        if (GetComponent<Playerstat>().firestack == 0)
        {
            GetComponent<Playerstat>().fire = false;
        }
        yield return null;
    }
    [PunRPC]
    void FingRPC()
    {
        StartCoroutine(Fing());
    }
    //빙결 프로세스
    void Icing()
    {
        if (pv.isMine)
        {
            StartCoroutine(Iing());
            pv.RPC("IingRPC", PhotonTargets.Others);
        }
    }
    IEnumerator Iing()
    {
        spd = basespd * .5f;
        GetComponent<Playerstat>().icestack--;
        if (GetComponent<Playerstat>().icestack == 0)
        {
            GetComponent<Playerstat>().ice = false;
        }
        yield return null;
    }
    [PunRPC]
    void IingRPC()
    {
        StartCoroutine(Iing());
    }
}