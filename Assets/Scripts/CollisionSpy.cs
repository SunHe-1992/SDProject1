using UnityEngine;
namespace SDProject1
{
    public class CollisionSpy : MonoBehaviour
    {
        //public bool isColliding = false;
        public Transform headSphere;
        public Transform tailSphere;
        public void FindObj()
        {
            if (headSphere == null)
                this.headSphere = this.transform.Find("head");
            if (tailSphere == null)
                this.tailSphere = this.transform.Find("tail");
        }
        private void Awake()
        {
            FindObj();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}