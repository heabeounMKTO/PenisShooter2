using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateObject : MonoBehaviour
{
   Vector3 rotationDirection;
   float smoothTime;
   private float convertedTime = 0.05f;
   private float smooth;
 
   // Use this for initialization
   void Start () {
       
       rotationDirection = new Vector3(Random.Range(-360f, 360f), Random.Range(-360f, 360f), Random.Range(-360f, 360f));
   }
 
   // Update is called once per frame
   void Update () {
    smoothTime = Random.Range(0.01f,4f);
    print($"smoothtime{smoothTime}");
     smooth = Time.deltaTime * smoothTime * convertedTime;
     transform.Rotate(rotationDirection * smooth);
   }
}
