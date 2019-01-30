using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class test : MonoBehaviour
{
    public GameObject circle;
    public GameObject circle2;
    public Vector3 before;
    public Vector3 after;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Movenextdeadzone());
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            asd();
        }
    }
    void asd()
    {
        float range = (circle.transform.localScale.x - circle2.transform.localScale.x) / 2 * 1250;
        float radius = Random.Range(0, range);
        float rad = Random.Range(0, Mathf.PI * 2);
        float newx = radius * Mathf.Cos(rad);
        float newy = radius * Mathf.Sin(rad);
        circle2.transform.position = circle.transform.position + new Vector3(newx, 0, newy);
    }
    IEnumerator Movenextdeadzone()
    {
        Vector3 ori = circle.transform.localScale;
        Vector3 bb = (circle2.transform.localScale - circle.transform.localScale);
        before = circle.transform.position;
        after = circle2.transform.position;
        Vector3 aa = after - before;
        for (float i = 0; i < 1.01; i += 0.001f)
        {
            circle.transform.position = before + i * aa;
            circle.transform.localScale = ori + i * bb;
            yield return new WaitForEndOfFrame();
        }
    }
}
