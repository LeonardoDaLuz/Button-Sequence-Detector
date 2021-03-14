using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour {
    public int Player=1;

    void Update () {

        if (CommandSequences.SequenceIsCompleted("Fire Badouken", Player))
        {
            GetComponent<Animator>().CrossFade("Fire Badouken", 0f);
        }
        else if (CommandSequences.SequenceIsCompleted("Badouken", Player))
        {
            GetComponent<Animator>().CrossFade("Badouken", 0f);
        }
    }
}
