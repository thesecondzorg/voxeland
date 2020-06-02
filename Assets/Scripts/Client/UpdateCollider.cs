using System;
using UnityEngine;

namespace Test.Netowrker
{
    public class UpdateCollider : MonoBehaviour
    {
        public bool State;

        private void Start()
        {
            if (State)
            {
                gameObject.AddComponent<MeshCollider>();
            }
            else
            {
                Destroy(GetComponent<MeshCollider>());
            }

            Destroy(this);
        }
    }
}