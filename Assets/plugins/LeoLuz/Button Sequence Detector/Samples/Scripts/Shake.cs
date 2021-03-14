using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour {
    public Vector3 ShakeDir = Vector3.left;
    public AnimationCurve ShakeCurve;
    public static Shake i;
	// Use this for initialization
	void Start () {
        i = this;
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void shake()
    {
        StartCoroutine(ShakeCo());
    }
    public IEnumerator ShakeCo()
    {

        float t = 0f;
        Vector3 initialPosition = transform.position;
        while (t< ShakeCurve.length)
        {
            t += Time.deltaTime;
            transform.position = initialPosition + (ShakeDir * ShakeCurve.Evaluate(t));
            yield return new WaitForEndOfFrame();
        }
        transform.position= initialPosition;
    }
}
