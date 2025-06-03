using UnityEngine;

namespace ShipyardLib
{
    public class WheelColliderButton : GoPointerButton
    {
        public Transform cursor;

        public float limit;
        public float sensitivity;

        private bool inControl;

        public void Update()
        {
            if (isLookedAt)
            {   //disable the camera and move the cursor
                inControl = true;
                BoatCamera.on = false;

                float deltaY = Input.mouseScrollDelta.y * sensitivity;
                Vector3 targetPos = cursor.localPosition + Vector3.right * deltaY;
                cursor.localPosition = ShipyardHelpers.Clamp(targetPos, limit, ShipyardHelpers.Axis.X);
            }
            else if (!isLookedAt && inControl)
            {   //re-enables the camera
                inControl = false;
                BoatCamera.on = true;
            }
        }
    }
}
