using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.PhysicsExtensions
{
    /// <summary>
    /// A functional wrapper class that wraps up a raycast call
    /// Note: This class can also wrap a sphere cast
    /// 
    /// This class is designed to add flexability, and dynamics to raycasts in the code, at the cost of preformance
    /// </summary>
    public class Raycast
    {
        public Raycast()
        {

        }

        public Raycast(Transform transform, Vector3 direction, float distance = 1f, int layerMask = -1, float sphereCastRadius = -1, bool correctSphereCast = true)
            : this(transform, () => direction, distance, layerMask, sphereCastRadius, correctSphereCast)
        {
        }

        public Raycast(Transform transform, Func<Vector3> direction, float distance = 1f, int layerMask = -1, float sphereCastRadius = -1, bool correctSphereCast = true)
        {
            this.Origin = () => transform.position;
            this.Direction = direction;
            this.distance = distance;
            this.layerMask = layerMask;
            this.sphereCastRadius = sphereCastRadius;
        }

        /// <summary>
        /// Assign a function to map the origin to the raycast
        /// </summary>
        public Func<Vector3> Origin = () => Vector3.zero;
        /// <summary>
        /// Assign a static point for the origin of the raycast
        /// </summary>
        public Vector3 origin
        {
            get
            {
                return Origin();
            }
            set
            {
                Origin = () => value;
            }
        }

        /// <summary>
        /// Assign a function to map the direction of the raycast
        /// </summary>
        public Func<Vector3> Direction = () => Vector3.down;
        /// <summary>
        /// Assign a static direction for the raycast
        /// </summary>
        public Vector3 direction
        {
            get
            {
                return Direction();
            }
            set
            {
                Direction = () => value;
            }
        }

        /// <summary>
        /// Assign a function to determine the layer mask of the raycast
        /// </summary>
        public Func<int> LayerMask = () => -1;
        /// <summary>
        /// Assign a static number for the raycast
        /// </summary>
        public int layerMask
        {
            get
            {
                return LayerMask();
            }
            set
            {
                LayerMask = () => value;
            }
        }

        /// <summary>
        /// Assign a function to map the distance of the raycast
        /// </summary>
        public Func<float> Distance = () => -1;
        /// <summary>
        /// Assign a static distance for the raycast
        /// </summary>
        public float distance
        {
            get
            {
                return Distance();
            }
            set
            {
                Distance = () => value;
            }
        }

        /// <summary>
        /// Assign a function
        /// </summary>
        public float sphereCastRadius = -1f;
        /// <summary>
        /// Determine if the raycast is a ray or sphere
        /// </summary>
        public bool isSphereCast
        {
            get
            {
                return sphereCastRadius >= 0f;
            }
        }

        /// <summary>
        /// If set, corrects the sphere cast to include collisions near the origin of the cast
        /// May also allow for some erronious calculations
        /// </summary>
        public bool correctSphereCast = true;

        /// <summary>
        /// Cast the ray, and return the output
        /// </summary>
        /// <returns></returns>
        public bool Cast()
        {
            if(isSphereCast)
            {
                Vector3 o = origin;
                float dis = distance;
                if(correctSphereCast)
                {
                    o += -direction * sphereCastRadius;
                }
                else
                {
                    dis -= sphereCastRadius;
                }
                return Physics.SphereCast(new Ray(o, direction), sphereCastRadius, dis, layerMask);
            }
            else
            {
                return Physics.Raycast(origin, direction, distance, layerMask);
            }
        }

        /// <summary>
        /// Cast the ray, and capture the raycast hit
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public bool Cast(out RaycastHit hit)
        {
            if(isSphereCast)
            {
                Vector3 o = origin;
                float dis = distance;
                if(correctSphereCast)
                {
                    o += -direction * sphereCastRadius;
                }
                else
                {
                    dis -= sphereCastRadius;
                }
                return Physics.SphereCast(new Ray(o, direction), sphereCastRadius, out hit, dis, layerMask);
            }
            else
            {
                return Physics.Raycast(origin, direction, out hit, distance, layerMask);
            }
        }

        /// <summary>
        /// Cast the ray, and capture all raycast hits
        /// Expecially useful for spherecasts
        /// </summary>
        /// <returns></returns>
        public RaycastHit[] CastAll()
        {
            Debug.Log(origin.ToString());
            if(isSphereCast)
            {
                return Physics.SphereCastAll(origin, sphereCastRadius, direction, distance, layerMask);
            }
            else
            {
                return Physics.RaycastAll(origin, direction, distance, layerMask);
            }
        }
    }
}