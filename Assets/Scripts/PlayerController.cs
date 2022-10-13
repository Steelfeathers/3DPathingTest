using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class PlayerController : MonoBehaviour
    {
        public GameObject testObj;
        
        private Vector3 targetPos;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Creates a Ray from the mouse position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 10);
               
                RaycastHit hitData;
                if (Physics.Raycast(ray, out hitData, Single.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
                {
                    //var obj = Instantiate(testObj, transform);
                    //obj.transform.position = hitData.point;

                    targetPos = new Vector3(hitData.point.x, transform.position.y, hitData.point.z);
                }
            }
        }
    }
}
