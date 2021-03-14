using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeoLuz.PropertyAttributes;

namespace LeoLuz
{
    public class HilightButtons : MonoBehaviour
    {
        [InputAxesListDropdown]
        public string Axis;
        public float speed = 15;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown(Axis))
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
}
