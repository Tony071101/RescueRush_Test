using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class DoorBehaviour : MonoBehaviour
    {
        [SerializeField] Transform leftDoorTransform;
        [SerializeField] Transform rightDoorTransform;

        [Space]
        [SerializeField] float leftDoorAngle = 98;
        [SerializeField] float rightDoorAngle = -98;

        [Space]
        [SerializeField] float openTime;
        [SerializeField] Ease.Type openEasing;
        [SerializeField] float closeTime;
        [SerializeField] Ease.Type closeEasing;

        private bool isOpened;
        private bool isMoving;

        private List<VisitorBehaviour> activeVisitors = new List<VisitorBehaviour>();
        private int activeVisitorsCount;

        private bool isPlayerActive;

        private void Start()
        {

        }

        public void Open()
        {
            if (isMoving || isOpened)
                return;

            isMoving = true;

            leftDoorTransform.DOLocalRotate(Quaternion.Euler(0, leftDoorAngle, 0), openTime).SetEasing(openEasing);
            rightDoorTransform.DOLocalRotate(Quaternion.Euler(0, rightDoorAngle, 0), openTime).SetEasing(openEasing).OnComplete(delegate
            {
                isMoving = false;

                isOpened = true;
            });
        }

        public void Close()
        {
            if (isMoving || !isOpened)
                return;

            isMoving = true;

            leftDoorTransform.DOLocalRotate(Quaternion.Euler(0, 0, 0), closeTime).SetEasing(closeEasing);
            rightDoorTransform.DOLocalRotate(Quaternion.Euler(0, 0, 0), closeTime).SetEasing(closeEasing).OnComplete(delegate
            {
                isMoving = false;

                isOpened = false;
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_VISITOR))
            {
                VisitorBehaviour visitorBehaviour = other.GetComponent<VisitorBehaviour>();
                if (visitorBehaviour != null)
                {
                    // Check if visitor is already in list
                    if (activeVisitors.FindIndex(x => x == visitorBehaviour) != -1)
                        return;

                    activeVisitors.Add(visitorBehaviour);
                    activeVisitorsCount++;

                    Open();
                }
            }
            else if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                isPlayerActive = true;

                Open();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_VISITOR))
            {
                VisitorBehaviour visitorBehaviour = other.GetComponent<VisitorBehaviour>();
                if (visitorBehaviour != null)
                {
                    int visitorIndex = activeVisitors.FindIndex(x => x == visitorBehaviour);
                    if (visitorIndex != -1)
                    {
                        activeVisitors.RemoveAt(visitorIndex);
                        activeVisitorsCount--;

                        if (activeVisitorsCount == 0 && !isPlayerActive)
                            Close();
                    }
                }
            }
            else if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                isPlayerActive = false;

                if (activeVisitorsCount == 0)
                    Close();
            }
        }
    }
}