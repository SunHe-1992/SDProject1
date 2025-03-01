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
        public Text carCountText;
        public InputField timeInputField;
        public Toggle autoAddCarToggle;
        float autoAddCarTimeInterval = 5f;
        float autoAddCarTimer = 0f;
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
        public float nearDistance2 = 0.5f;
        public bool IsNearOtherCarTail(Vehicle self)
        {
            var headPos = self.headPos();
            foreach (var v in vehicles)
            {
                if (v == self) continue;
                if (v.IsReady() == false) continue;
                if (Vector3.Distance(v.tailPos(), headPos) < nearDistance)
                    return true;
                //if (Vector3.Distance(v.transform.position, self.transform.position) < nearDistance2)
                //    return true;
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
            if (timeInputField != null)
                if (float.TryParse(timeInputField.text, out float inputTime))
                {
                    autoAddCarTimeInterval = inputTime;
                }
            if (autoAddCarToggle != null)
                if (autoAddCarToggle.isOn && autoAddCarTimeInterval > 2f)
                {
                    autoAddCarTimer += Time.deltaTime;
                    if (autoAddCarTimer >= autoAddCarTimeInterval)
                    {
                        autoAddCarTimer = 0;
                        AddACar();
                    }
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
}