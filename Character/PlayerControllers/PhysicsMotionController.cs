using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PhysicsExtensions;

namespace UnityEngine.CharacterMovement
{
    /// <summary>
    /// A movement controller with the following features
    /// 
    /// Snaps to ground
    /// Moves smothly up and down ramps
    /// Can handle collision with edges of terrain and surfaces (allowing character to float slightly off the edge of geometry)
    /// Tollerant of imperfections in terrain
    /// Can move up and down stairs, with a snappy motion (snappy motion can be smoothed with a good camera controller, or by turning stepped geometry into ramps)
    /// 
    /// Configurable top speed and acceleration
    /// 
    /// TODO: Can be paused for a few frames to prevent correction after jumping
    ///     Integration with moving playform physics
    ///     Integration with variable gravity
    ///     Inpulse method which applies an inpulese to the character, and automatically chooses to pause itself
    ///     Variation of inpulse that keeps ground adjustment
    ///     Variation of inpulse that works with a delta v
    ///     Variation of inpulse that assigns v directly
    ///     Implement jump via delta v
    ///     
    ///     Some static methods to automatically attach movement controller to an existing object
    /// </summary>
    public class PhysicsMotionController : MonoBehaviour
    {
        /// <summary>
        /// Represents a 2 dimensional velocity, which will get corrected with slopes and stuff
        /// TODO: Make local velocity switch to global when jumping and landing correctly
        /// </summary>
        public Vector3 localVelocity;

        /// <summary>
        /// The velocity this controller will attempt to match
        /// In most instances, this will be the input from the player, or the direction an NPC desiers to move
        /// </summary>
        public Vector3 targetVelocity;

        /// <summary>
        /// The maximum speed this controller will move on the ground
        /// </summary>
        public float speed;
        /// <summary>
        /// The acceleration of the controller.
        /// </summary>
        public float acceleration;

        /// <summary>
        /// This does the work of snapping the character to the ground, and matching slopes
        /// </summary>
        GroundDetector groundDetector;
        /// <summary>
        /// The rigidbody that actually controlls the characters motion
        /// </summary>
        new Rigidbody rigidbody;

        /// <summary>
        /// How far off the ground this character will float
        /// This allows them to move up and down slopes, and stairs
        /// </summary>
        public float hoverDistance
        {
            get
            {
                return groundDetector.hoverDistance;
            }
            set
            {
                groundDetector.hoverDistance = value;
                groundDetector.detectionDistance = value;
            }
        }

        /// <summary>
        /// Additional distance to raycast over when the character is known to be grounded
        /// </summary>
        public float groundBuffer
        {
            get
            {
                return groundDetector.groundDetectionBuffer;
            }
            set
            {
                groundDetector.groundDetectionBuffer = value;
            }
        }

        /// <summary>
        /// The radius over which this character floats
        /// Allows "cyotee" physics to be implemented, allowing character to stand off the edge of surfaces slightly
        /// This can be hidden via the animation system for the controller
        /// </summary>
        public float radius
        {
            get
            {
                return groundDetector.radius;
            }
            set
            {
                groundDetector.radius = value;
            }
        }

        /// <summary>
        /// The layer mask this character will functino over
        /// </summary>
        public int layerMask
        {
            get
            {
                return groundDetector.layerMask;
            }
            set
            {
                groundDetector.layerMask = value;
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        private void Awake()
        {
            //Create a new ground detector, which is linked to this characters transform
            //TODO: allow ground detector to be linked to a different trasnform, or a different origin tracking method all together
            groundDetector = new GroundDetector(new Raycast(transform, Vector3.down), 1f, 0.1f, 1f);
            //Attach a rigidbody to the character
            rigidbody = gameObject.AddComponent<Rigidbody>();
            //Turn off gravity, so that it can be simulated by the caracter controller
            //Leaving gravity on can cause the character to have movement studders with the framerate
            rigidbody.useGravity = false;
            //Freeze the rotation of the character, so that it can be controlled by the animation controller for the character
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void FixedUpdate()
        {
            //Calculate the accleration of the character
            localVelocity = Vector3.Lerp(localVelocity, targetVelocity * speed, acceleration * Time.deltaTime);

            //Initialize placeholder variables for surface correction, if the character is grounded
            Vector3 correctedPoint, surfaceNormal;

            //Determine if the character is grounded.
            //See ground detector for a description of the algorithm
            //  TODO, convert this algorithm to a method call, for maintainability
            if(groundDetector.GetGround(out correctedPoint, out surfaceNormal, true))
            {
                //Debug.DrawLine(transform.position, correctedPoint, Color.red);
                //correct the position, if the character is grounded
                transform.position = correctedPoint;

                //Correct the velocity for a slope
                //This operation works by projecting the velocity onto a plane representing the surface of the slope
                //When applied each frame it causes the controller to glide gracefully, and have correct speed and motion going up and down slopes
                //The current algorithm implmented could have issues going up very steep slopes. This can be fixed by setting radius to -1, but this will also disable the cyotee effect
                // TODO: allow cyotee effect and steep slope correction
                Vector3 tempVelocity;
                Math3d.LinePlaneIntersection(out tempVelocity, localVelocity, groundDetector.direction, surfaceNormal, Vector3.zero);

                //correct speed to match original speed expected
                tempVelocity = tempVelocity.normalized * localVelocity.magnitude;

                //Assign velocity to controller
                rigidbody.velocity = tempVelocity;
            }
            else
            {
                //Simple case, just apply velocity
                rigidbody.velocity = localVelocity;
            }
            //Debug.DrawRay(transform.position, rigidbody.velocity);
        }
    }
}