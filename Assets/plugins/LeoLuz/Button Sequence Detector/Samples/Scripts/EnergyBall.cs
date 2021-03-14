using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnergyBall : MonoBehaviour {
    public float speed;
    private Rigidbody rb;
    public GameObject ExplosionPrefab;
    private bool exploded;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        rb.velocity = new Vector3(Mathf.Sign(transform.localScale.x) * speed, 0f, 0f);
    }

    void OnCollisionEnter(Collision col)
    {
        if (!exploded)
        {
            exploded = true;
            GameObject obj = Instantiate(ExplosionPrefab, transform.position, transform.rotation);
            if (Shake.i!=null)
                Shake.i.shake();
            Destroy(obj, 5f);
            Destroy(gameObject);
        }
    }
}
