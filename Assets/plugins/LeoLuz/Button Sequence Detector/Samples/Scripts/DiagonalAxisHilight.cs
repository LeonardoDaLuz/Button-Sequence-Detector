using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagonalAxisHilight : MonoBehaviour {

    public float speed = 15f;

	
	void Update () {
		if(Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal")>0.3f && Input.GetAxisRaw("Vertical")<-0.3f)
        {
            StartCoroutine(Hilight());
        }
	}
    bool CoroutineStarted;
    IEnumerator Hilight()
    {
        if (CoroutineStarted)
            yield break;

        CoroutineStarted = true;
        Vector3 InitialScale = transform.localScale;
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * speed;
            transform.localScale = InitialScale + (InitialScale * time);
            yield return new WaitForEndOfFrame();
        }
        while (time > 0f)
        {
            time -= Time.deltaTime * speed;
            transform.localScale = InitialScale + (InitialScale * time);
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = InitialScale;
        CoroutineStarted = false;
    }
}
