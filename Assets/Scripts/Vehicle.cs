using UnityEngine;
using System.Collections.Generic;

namespace SDProject1
{

    public enum VehicleState
    {
        Default = 0,
        Running = 1,
        WaitingForService = 2,
        WaitingForJam = 3,

    }
    public class Vehicle : MonoBehaviour
    {
        public PathNode nextPathNode = null;

        public float speed = 11.0f;
        public float rotationSpeed = 8.0f; // 旋转速度
        private CollisionSpy collisionSpy;
        public bool needRecycle = false;

        //VehicleState vState = VehicleState.Default;
        public float waitTime = 0f;
        public Vector3 headPos()
        {
            return collisionSpy.headSphere.position;
        }
        public Vector3 tailPos()
        {
            return collisionSpy.tailSphere.position;
        }
        public void Start()
        {
            if (nextPathNode == null)
                nextPathNode = PathNodeMgr.Inst.GetNextNode(-1);

            //random car model
            SetCarModel(Random.Range(0, carModels.Count));
            //SetCarModel(0);
        }
        public bool IsReady()
        {
            if (nextPathNode == null) return false;
            if (collisionSpy == null) return false;
            if (collisionSpy.headSphere == null) return false;
            if (collisionSpy.tailSphere == null) return false;
            return true;
        }
        private void Update()
        {
            if (IsReady() == false) return;

            if (this.waitTime > 0)
            {
                this.waitTime -= Time.deltaTime;
                if (this.waitTime <= 0)
                {
                    this.waitTime = 0;
                }
                return;
            }

            if (IsColliding())
            {
                //Debug.Log($"waiting for the front car");
                return;
            }

            if (IsAtNode())
            {
                if (PathNodeMgr.Inst.IsFinalNode(nextPathNode.nodeId))
                {
                    //recycle this car
                    this.needRecycle = true;
                    return;
                }
                else
                {
                    nextPathNode = PathNodeMgr.Inst.GetNextNode(nextPathNode.nodeId);
                    if (nextPathNode.waitInNode)
                    {
                        this.waitTime = PathNodeMgr.Inst.GetRandomWaitTime();
                    }
                }
            }
            else
            {
                Transform target = nextPathNode.transform;

                float moveDist = Time.deltaTime * speed;
                float distToNext = Vector3.Distance(this.transform.position, nextPathNode.transform.position);
                moveDist = Mathf.Min(moveDist, distToNext);
                this.transform.position = Vector3.MoveTowards(this.transform.position, nextPathNode.transform.position, moveDist);

                // 计算目标方向的旋转
                Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

                // 使用 Lerp 进行平滑插值
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }


        bool IsAtNode()
        {
            return Vector3.Distance(this.transform.position, nextPathNode.transform.position) < 0.1f;
        }


        public List<GameObject> carModels = new List<GameObject>();
        public void SetCarModel(int index)
        {
            var newObj = GameObject.Instantiate(carModels[index]);
            newObj.transform.SetParent(this.transform);
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localRotation = Quaternion.identity;
            newObj.SetActive(true);
            this.collisionSpy = newObj.GetComponent<CollisionSpy>();
            this.collisionSpy.FindObj();
        }

        bool IsColliding()
        {
            return PathNodeMgr.Inst.IsNearOtherCarTail(this);
        }

        public void ResetAfterSpawn()
        {
            this.needRecycle = false;
            this.nextPathNode = PathNodeMgr.Inst.GetNextNode(-1);
            this.transform.position = nextPathNode.transform.position;
            this.transform.rotation = nextPathNode.transform.rotation;
            this.speed = Random.Range(5.0f, 11.0f);//random speed
            waitTime = 0f;
        }

    }
}