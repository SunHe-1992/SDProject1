using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace SDProject1
{
    public class PathNodeMgr : MonoBehaviour
    {
        public static PathNodeMgr Inst;
        public List<PathNode> nodes = new List<PathNode>();
        public Transform pathNodeContainer;

        public List<Vehicle> vehicles = new List<Vehicle>();
        //float ArriveTimer = 5f;
        float ArriveTimeInterval_average = 5f;
        float autoAddCarTimer = 0f;
        float serviceWaitTime = 4f;
        #region UI 
        public Text carCountText;
        public Text text_arrive;
        public Text text_service;
        public Slider slider_arrive;
        public Slider slider_service;

        #endregion
        private void Awake()
        {
            Inst = this;
            FindAllNodes();
            FindAllVehicles();

            carObjPool = new ObjPool(carPrefab);
        }

        void FindAllNodes()
        {
            for (int i = 0; i < pathNodeContainer.childCount; i++)
            {
                PathNode node = pathNodeContainer.GetChild(i).GetComponent<PathNode>();
                node.nodeId = i;
                nodes.Add(node);
            }


        }
        void FindAllVehicles()
        {
            vehicles.Clear();
            var vArray = GameObject.FindGameObjectsWithTag("Vehicle");
            foreach (var v in vArray)
            {
                vehicles.Add(v.GetComponent<Vehicle>());
            }
        }

        public float nearDistance = 2f;
        public bool IsNearOtherCarTail(Vehicle self)
        {
            var headPos = self.headPos();
            foreach (var v in vehicles)
            {
                if (v == self) continue;
                if (v.IsReady() == false) continue;
                if (Vector3.Distance(v.tailPos(), headPos) < nearDistance)
                    return true;
            }
            return false;
        }

        public PathNode GetNextNode(int index)
        {
            index++;
            if (index >= nodes.Count)
            {
                index = 0;
            }
            return nodes[index];
        }
        public bool IsFinalNode(int index)
        {
            if (index + 1 >= nodes.Count)
            {
                return true;
            }
            return false;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //check if need to recycle cars
            for (int i = vehicles.Count - 1; i >= 0; i--)
            {
                if (vehicles[i].needRecycle)
                {
                    carObjPool.Recycle(vehicles[i].gameObject);
                    vehicles.RemoveAt(i);
                }
            }

            int waitingCarCount = 0;
            foreach (var car in vehicles)
            {
                if (car.waitTime > 0)
                {
                    waitingCarCount++;
                }
            }
            //update car count
            if (carCountText != null)
            {
                string msg = $"cars in service: {waitingCarCount}";
                carCountText.text = msg;
            }
            if (slider_arrive != null)
            {
                ArriveTimeInterval_average = slider_arrive.value;
            }
            if (slider_service != null)
            {
                serviceWaitTime = slider_service.value;
            }
            if (text_arrive != null)
            {
                text_arrive.text = $"Arrive interval: {ArriveTimeInterval_average.ToString("F2")}";
            }
            if (text_service != null)
            {
                text_service.text = $"Average Service time: {serviceWaitTime.ToString("F2")}";
            }
            autoAddCarTimer += Time.deltaTime;
            if (autoAddCarTimer >= ArriveTimeInterval_average)
            {
                autoAddCarTimer = 0;
                AddACar();
            }

        }
        public Transform carEnterPoint;
        public GameObject carPrefab;
        ObjPool carObjPool;
        public void AddACar()
        {
            var newCar = carObjPool.SpawnOne();
            var script = newCar.GetComponent<Vehicle>();
            script.ResetAfterSpawn();
            this.vehicles.Add(script);
            newCar.transform.position = carEnterPoint.position;
        }

        public float GetRandomWaitTime()
        {
            float mean = serviceWaitTime;// 240.19f / 50f;
            float stdDev = 118.32f / 50f; // Standard deviation

            // Box-Muller Transform to generate a normally distributed random number
            float u1 = 1.0f - Random.value; // Uniform(0,1] random number
            float u2 = 1.0f - Random.value;

            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            float randNormal = mean + stdDev * randStdNormal; // Scale to mean and std dev

            return randNormal;
        }
 
    }
}


public class ObjPool
{
    private List<GameObject> objList = new List<GameObject>();
    public GameObject objPrefab;
    public ObjPool(GameObject obj)
    {
        objPrefab = obj;
    }
    public GameObject SpawnOne()
    {
        GameObject obj = null;
        if (objList.Count > 0)
        {
            obj = objList[0];
            objList.RemoveAt(0);
        }
        else
        {
            obj = GameObject.Instantiate(objPrefab);
        }
        obj.SetActive(true);
        return obj;
    }
    public void Recycle(GameObject obj)
    {
        objList.Add(obj);
        obj.SetActive(false);
    }
}
