using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.CharacterMovement
{
    public class PhysicsDemoCharacter : MonoBehaviour
    {
        PhysicsMotionController motionController;

        public float hoverDistance = 0.6f, groundBuffer = 0.1f, speed = 10f, acceleration = 50f, airAccelerationRatio = 0.25f, radius = 0.4f;

        private void Start()
        {
            

            int layer = LayerMask.NameToLayer("Character");
            motionController = PhysicsMotionController.CreateMotionController(gameObject, hoverDistance, groundBuffer, radius, layer, speed, acceleration, airAccelerationRatio);

            //gameObject.layer = LayerMask.NameToLayer("Character");
            //motionController = gameObject.AddComponent<PhysicsMotionController>();
            //motionController.groundBuffer = 0.1f;
            //motionController.hoverDistance = 0.6f;
            //motionController.speed = 10f;
            //motionController.acceleration = 50f;
            //motionController.airAccelerationRatio = 0.25f;
            //motionController.radius = 0.4f;
            //motionController.layerMask = ~LayerMask.GetMask("Character");
        }

        private void Update()
        {
            Vector3 input = Vector3.zero;
            if(Input.GetKey(KeyCode.A))
            {
                input.x += -1f;
            }
            if(Input.GetKey(KeyCode.D))
            {
                input.x += 1f;
            }
            if(Input.GetKey(KeyCode.S))
            {
                input.z += -1f;
            }
            if(Input.GetKey(KeyCode.W))
            {
                input.z += 1f;
            }

            float mag = input.magnitude;
            if(mag > 1f)
            {
                input = input / mag;
            }

            if(motionController.Grounded && Input.GetKeyDown(KeyCode.Space))
            {
                motionController.ApplyInpulse(Vector3.up * 10f);
            }

            motionController.targetVelocity = input;
        }
    }
}