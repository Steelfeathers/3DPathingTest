using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [SerializeField] private float minMoveSpeed = 1f;
        [SerializeField] private float maxMoveSpeed = 5f;
        [SerializeField] private float accelerationDist = 1f;
        [SerializeField] private float decelarationDist = 1f;

        private List<Vector3> movementPathNodes;
        private int curPathNodeIndex;
        private float curPathTime;
        private Vector3 targetPos;

        private float totalPathLength;
        private float curPathProgress;
        
        private void Update()
        {
            MovePlayer();
            
            //When the player clicks, raycast towards the mouse point to see if we're clicking on ground. If so, find a path to the target point and start moving towards it.
            //If the player clicks on a different point, immediately abandon the previous path and start moving towards the new point
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                LayerMask mask = 1 << LayerMask.NameToLayer("Ground");
                RaycastHit hitData;
                if (Physics.Raycast(ray, out hitData, Single.MaxValue, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hitData.collider != null && hitData.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        var targetPos = new Vector3(hitData.point.x, transform.position.y, hitData.point.z);
                        var tempPath = Pathfinding.Instance.FindPath(transform.position, targetPos);
                        if (tempPath != null && tempPath.Count > 1)
                        {
                            tempPath.RemoveAt(0); //Remove your current cell from the path
                            movementPathNodes = tempPath;
                            curPathNodeIndex = 0;
                            curPathTime = 0f;

                            //Calculate the total path length for acceleration/deceleration smoothing
                            curPathProgress = 0;
                            totalPathLength = 0;
                            Vector3 prevPos = transform.position;
                            foreach (var pos in movementPathNodes)
                            {
                                totalPathLength += Vector3.Distance(pos, prevPos);
                                prevPos = pos;
                            }
                        }
                    }
                }
            }
        }

        private void MovePlayer()
        {
            if (movementPathNodes == null) return;

            Vector3 targetPos = movementPathNodes[curPathNodeIndex];
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                Vector3 moveDir = (targetPos - transform.position).normalized;
                
                //Calculate the acceleration/deceleration of the player based on their position along the path
                float curMoveSpeed = minMoveSpeed;
                if (curPathProgress < accelerationDist)
                    curMoveSpeed += accelerationCurve.Evaluate(curPathProgress / accelerationDist) * (maxMoveSpeed-minMoveSpeed);
                else if (curPathProgress > (totalPathLength - decelarationDist))
                    curMoveSpeed += decelerationCurve.Evaluate(curPathProgress / accelerationDist) * (maxMoveSpeed-minMoveSpeed);
                else
                    curMoveSpeed = maxMoveSpeed;
               
                float dist = curMoveSpeed * Time.deltaTime;
                
                //Move the player based on the calculated speed
                transform.position += moveDir * dist;
                //Update the path progress
                curPathProgress += dist;
            }
            else
            {
                curPathNodeIndex += 1;
                if (curPathNodeIndex >= movementPathNodes.Count)
                {
                    movementPathNodes = null;
                }
            }
            
#if UNITY_EDITOR
            if (GameRoot.Instance.ShowDebug)
                DebugDrawPath(Time.deltaTime);
#endif
        }

        private void DebugDrawPath(float dur)
        {
#if UNITY_EDITOR
            if (movementPathNodes == null) return;
            Vector3 prevPos = movementPathNodes[0];
            foreach (var pos in movementPathNodes)
            {
                Debug.DrawLine(prevPos, pos, Color.blue, dur);
                prevPos = pos;
            }
#endif
        }
    }
}
