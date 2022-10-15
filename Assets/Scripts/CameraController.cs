using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnknownWorldsTest
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float cameraPanSpeed = 1f;
        [SerializeField][Range(0f, 0.5f)] private float cameraFollowBorderPerc = 0.25f;
        
        private float cameraFollowX;
        private float cameraFollowY;
        private Vector3 screenBorders;

        private void Start()
        {
            cameraFollowX = Screen.width * cameraFollowBorderPerc;
            cameraFollowY = Screen.height * cameraFollowBorderPerc;
            screenBorders = new Vector3(Screen.width / 2f, Screen.height / 2f);
        }

        //Make the camera pan around the scene when the player moves the mouse towards the edge of the screen
        void Update()
        {
            var mousePos = Input.mousePosition;
#if UNITY_EDITOR
            if (mousePos.x == 0 || mousePos.y == 0 || mousePos.x >= Handles.GetMainGameViewSize().x - 1 || mousePos.y >= Handles.GetMainGameViewSize().y - 1) return;
#else
        if (mousePos.x == 0 || mousePos.y == 0 || mousePos.x >= Screen.width - 1 || mousePos.y >= Screen.height - 1) return;
#endif

            mousePos -= screenBorders;
            Vector3 dir = Vector3.zero;
            
            cameraFollowX = Screen.width * cameraFollowBorderPerc;
            cameraFollowY = Screen.height * cameraFollowBorderPerc;
            
            if (mousePos.x > cameraFollowX)
            {
                dir += Vector3.right;
            }
            if (mousePos.x < -cameraFollowX)
            {
                dir += Vector3.left;
            }
            if (mousePos.y > cameraFollowY)
            {
                dir += Vector3.forward;
            }
            if (mousePos.y < -cameraFollowY)
            {
                dir += Vector3.back;
            }

            transform.position += dir.normalized * cameraPanSpeed * Time.deltaTime;
        }
    }
}
