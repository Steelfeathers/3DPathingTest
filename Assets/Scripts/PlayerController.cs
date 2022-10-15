using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1f;

        private List<Vector3> movementPath;
        private int curPathIndex;
        

        private void Update()
        {
            MovePlayer();
            
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 10);
               
                RaycastHit hitData;
                if (Physics.Raycast(ray, out hitData, Single.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
                {
                    var targetPos = new Vector3(hitData.point.x, transform.position.y, hitData.point.z);
                    var tempPath = Pathfinding.Instance.FindPath(transform.position, targetPos);
                    if (tempPath != null)
                    {
                        Vector3 prevPos = transform.position;
                        foreach (var pos in tempPath)
                        {
                            Debug.DrawLine(prevPos, pos, Color.green, 5f);
                            prevPos = pos;
                        }
                        //tempPath.RemoveAt(0); //Remove your current cell from the path
                       // movementPath = tempPath;
                       // curPathIndex = 0;
                    }
                }
            }
        }

        private void MovePlayer()
        {
            if (movementPath == null) return;

            Vector3 targetPos = movementPath[curPathIndex];
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                Vector3 moveDir = (targetPos - transform.position).normalized;
                transform.position += moveDir * moveSpeed * Time.deltaTime;
            }
            else
            {
                curPathIndex += 1;
                if (curPathIndex >= movementPath.Count)
                {
                    movementPath = null;
                }
            }

        }
    }
}
