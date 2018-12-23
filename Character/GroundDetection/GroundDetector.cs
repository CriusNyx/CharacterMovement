using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PhysicsExtensions;

namespace UnityEngine.CharacterMovement
{
    /// <summary>
    /// Used to detect ground for entitys which need to detect ground every frame, and correct their positions
    /// </summary>
    public class GroundDetector
    {
        /// <summary>
        /// The raycast to use to detect the ground
        /// </summary>
        private Raycast raycast;


        /// <summary>
        /// The distance the object should hover off the ground.
        /// Used for ground corrections
        /// </summary>
        public float hoverDistance;

        /// <summary>
        /// If the object was grounded last frame, the raycast will be cast over some additional distance, specified by this buffer.
        /// This allows the cast to work for objects going down ramps and stairs
        /// </summary>
        public float groundDetectionBuffer;

        /// <summary>
        /// The distance to preform the detection over
        /// </summary>
        public float detectionDistance;

        /// <summary>
        /// The base distance to detect over
        /// In many cases, this should be the same as hover distance
        /// Setting this different then hover distance can cause stuttering when jumping and landing
        ///    Note: This effect is very noticible in the game Sonic The Hedgehog (2006), where the cammera and character stutter when jumping and landing
        ///    Note: Sonic 06 has numerous other problems causing jumping to be problematic
        /// </summary>
        public float radius;

        /// <summary>
        /// The layer mask this ground detector should cast over
        /// </summary>
        public int layerMask
        {
            get
            {
                return raycast.layerMask;
            }
            set
            {
                raycast.layerMask = value;
            }
        }

        /// <summary>
        /// The direction to cast for ground
        /// In many cases this will just be Vector3.down, but this can be assigned to accomidate shifting gravity, or movement across verticle surfaces
        /// </summary>
        public Vector3 direction
        {
            get
            {
                return raycast.direction;
            }
            set
            {
                raycast.direction = value.normalized;
            }
        }

        /// <summary>
        /// Create a new ground detector with the specified raycast type, hover distance, ground detection buffer, and detection distance
        /// </summary>
        /// <param name="raycast"></param>
        /// <param name="hoverDistance"></param>
        /// <param name="groundDetectionBuffer"></param>
        /// <param name="detectionDistance"></param>
        public GroundDetector(Raycast raycast, float hoverDistance, float groundDetectionBuffer, float detectionDistance)
        {
            this.raycast = raycast;

            this.hoverDistance = hoverDistance;
            this.groundDetectionBuffer = groundDetectionBuffer;
            this.detectionDistance = detectionDistance;
        }

        /// <summary>
        /// A simple method to find the ground
        /// </summary>
        /// <param name="grounded"></param>
        /// <returns></returns>
        public bool GetGround(bool grounded)
        {
            if(raycast.isSphereCast)
            {
                Vector3 temp1, temp2;
                return GetGround(out temp1, out temp2, grounded);
            }
            else
            {
                SetDistance(grounded);
                return raycast.Cast();
            }
        }

        /// <summary>
        /// Find the ground, and return the corrected position and surface normal matching the ground
        ///     Note: The ground detector assumes that ground which is not under the characters feet is essencially flat.
        ///     This could potentioally cause problems if the character is standing off the edge of a cliff with a sloped edge
        /// </summary>
        /// <param name="correctedPosition"></param>
        /// <param name="surfaceNormal"></param>
        /// <param name="grounded"></param>
        /// <returns></returns>
        public bool GetGround(out Vector3 correctedPosition, out Vector3 surfaceNormal, bool grounded)
        {
            //Determine if this is a raycast or sphere cast is nessessary
            if(radius > 0f)
            {
                //Do a simple raycast before preforming a sphere cast.
                //This allows for correct movement when standing on steps
                if(GetGroundRay(out correctedPosition, out surfaceNormal, grounded))
                {
                    return true;
                }
                //If the raycast fails, follow up with a sphere cast
                return GetGroundCapsule(out correctedPosition, out surfaceNormal, grounded);
            }
            //In this case, a raycast is enough
            else
                return GetGroundRay(out correctedPosition, out surfaceNormal, grounded);
        }

        /// <summary>
        /// Preform a raycast, and return the results
        /// </summary>
        /// <param name="correctedPosition"></param>
        /// <param name="surfaceNormal"></param>
        /// <param name="grounded"></param>
        /// <returns></returns>
        private bool GetGroundRay(out Vector3 correctedPosition, out Vector3 surfaceNormal, bool grounded)
        {
            //Set the raycast to do a raycast (rather then a sphere cast)
            raycast.sphereCastRadius = -1f;

            //Adjust the distance based on whether or not the character is grounded
            SetDistance(grounded);

            //Preform the raycast
            RaycastHit hit;
            if(raycast.Cast(out hit))
            {
                //Successful, unwrap output
                surfaceNormal = hit.normal;
                correctedPosition = hit.point + -direction * hoverDistance;
                return true;
            }

            //Failed, set output to default values, and return
            correctedPosition = surfaceNormal = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Preform a spherecast, and return the result
        /// </summary>
        /// <param name="correctedPosition"></param>
        /// <param name="surfaceNormal"></param>
        /// <param name="grounded"></param>
        /// <returns></returns>
        private bool GetGroundCapsule(out Vector3 correctedPosition, out Vector3 surfaceNormal, bool grounded)
        {
            //Set up raycast to preform a sphere cast
            raycast.sphereCastRadius = radius;
            //Set the distance of the cast based on grounded
            SetDistance(grounded);

            //Get a set of hits for the raycast
            RaycastHit[] hits = raycast.CastAll();
            foreach(var hit in hits)
            {
                //Debug.Log(hit.ToString());
                //Debug.Log(hit.point.ToString());
                //Debug.Log(hit.normal.ToString());
                //Validate each hit

                //Determine the distance to the hit (on the verticle axis only)
                float dot = Vector3.Dot(direction, hit.point - raycast.origin);
                //If the hit is extended past the expected distance, skip this hit
                if(dot > raycast.distance)
                    continue;

                //Correct the point to be under the characters feet
                Vector3 groundPoint;
                Math3d.LinePlaneIntersection(out groundPoint, raycast.origin, direction, direction, hit.point);

                //Determine the corrected position, and surface normal
                //Note: Even though a sphere cast is preformed, the results are treated more like a cylender cast
                correctedPosition = groundPoint + -direction * hoverDistance;
                surfaceNormal = hit.normal;

                //Determine if the character is floating, an set the surface normal directly up
                if((groundPoint != hit.point))
                    surfaceNormal = -direction;

                return true;
            }

            //Initialize default output
            correctedPosition = surfaceNormal = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Sets the distance for the raycast
        /// If the call is grounded, then add some additional distance.
        /// This allows movement down stairs and slopes
        /// </summary>
        /// <param name="grounded"></param>
        private void SetDistance(bool grounded)
        {
            if(grounded)
                raycast.distance = this.detectionDistance + this.groundDetectionBuffer;
            else
                raycast.distance = this.detectionDistance;
        }
    }
}