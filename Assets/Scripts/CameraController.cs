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

        private Vector3 offset;

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
    }
}
