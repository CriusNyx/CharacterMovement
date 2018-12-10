using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.CharacterMovement
{
    public class PhysicsDemoCharacter : MonoBehaviour
    {
        PhysicsMotionController motionController;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Character");

            motionController = gameObject.AddComponent<PhysicsMotionController>();
            motionController.groundBuffer = 0.1f;
            motionController.hoverDistance = 0.6f;
            motionController.speed = 10f;
            motionController.acceleration = 5f;
            motionController.radius = 0.4f;
            motionController.layerMask = ~LayerMask.GetMask("Character");
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

            motionController.targetVelocity = Vector3.right;
        }
    }
}