using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform cameraPosition;

        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}

