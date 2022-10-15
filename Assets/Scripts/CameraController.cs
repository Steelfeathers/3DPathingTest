using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        //private Vector3 offset;
        
        [SerializeField] private float mouseSensitivity = 1.0f;
        private Vector3 lastPosition;
 
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastPosition = Input.mousePosition;
            }
 
            if (Input.GetMouseButton(0))
            {
                var delta = Input.mousePosition - lastPosition;
                transform.Translate(delta.x * mouseSensitivity, delta.y * mouseSensitivity, 0);
                lastPosition = Input.mousePosition;
            }
        }

        /*
        private void Start()
        {
            if (target != null)
            {
                offset = transform.position - target.transform.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.transform.position + offset;
        }
        */
    }
}
