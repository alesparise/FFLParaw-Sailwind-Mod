using UnityEngine;

namespace ShipyardLib
{
    public class CursorButton : GoPointerButton
    {
        public Transform t;

        public float limit;
        public float sensitivity;

        private bool isDragging;

        public void Awake()
        {
            t = transform;
        }
        public override void OnActivate()
        {   //disable the camera when clicking the thing)

            BoatCamera.instance.GetComponent<MouseLook>().enabled = false;
            isDragging = true;
        }
        public void Update()
        {
            if (isDragging)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    isDragging = false;
                    return;
                }
                //get mouse movement
                float deltaX = Input.GetAxis("Mouse X") * sensitivity;
                //move the cursor by deltaX
                Vector3 targetPos = t.localPosition + Vector3.right * deltaX;
                t.localPosition = ShipyardHelpers.Clamp(targetPos, limit, ShipyardHelpers.Axis.X);
            }
        }
    }
}
