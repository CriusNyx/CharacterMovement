using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.PhysicsExtensions;

namespace UnityEngine.CharacterMovement
{
    /// <summary>
    /// Sample class to help illistrate the behaviour of the raycast wrapper class
    /// </summary>
    public class RaycastTest : MonoBehaviour
    {
        public Vector3 direction = Vector3.down;
        public float distance = 1f;
        public float radius = -1f;
        public bool correctSphereCast = false;

        private Raycast raycast;
        private GroundDetector detector;

        private void Awake()
        {
            raycast = new Raycast() { Origin = () => transform.position };
            detector = new GroundDetector(raycast, distance, 0f, distance);
        }

        private void Update()
        {
            raycast.direction = direction;
            raycast.distance = distance;
            raycast.sphereCastRadius = radius;
            raycast.correctSphereCast = correctSphereCast;

            detector.detectionDistance = distance;
            detector.hoverDistance = distance;

            Debug.DrawRay(raycast.origin, raycast.direction.normalized * raycast.distance);

            RaycastHit hit;
            if(raycast.Cast(out hit))
            {
                Debug.DrawRay(hit.point, hit.normal, Color.red);
            }

            Vector3 point, normal;
            if(detector.GetGround(out point, out normal, false))
            {
                Debug.DrawRay(point, normal, Color.green);
            }
        }
    }
}