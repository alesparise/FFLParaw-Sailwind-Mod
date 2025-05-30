using UnityEngine;
using Crest;

namespace FFLParaw
{
    /// <summary>
    /// Controls the outrigger behaviour
    /// </summary>
    public class Outrigger : MonoBehaviour
    {
        private GameObject foam;

        private Transform boat;
        private Transform embarkCollider;
        private Transform t;

        private Rigidbody rb;

        private SampleHeightHelper helper = new SampleHeightHelper();

        private BoatPerformanceSwitcher bps;

        private float forceMultiplier = 10f;
        private float dampingMultiplier = 5f;
        private float magnitude;
        public float height;

        public void Awake()
        {
            boat = GetComponentInParent<BoatHorizon>().transform;
            embarkCollider = boat.GetComponentInChildren<BoatEmbarkCollider>().transform;
            rb = boat.parent.GetComponent<Rigidbody>();
            t = transform;
            foam = t.Find("WaterFoam").gameObject;
            bps = boat.parent.GetComponent<BoatPerformanceSwitcher>();
        }
        private void OnEnable()
        {   //widen the embark collider when the outriggers are installed
            embarkCollider.localScale = new Vector3(1f, 1.8f, 1f);
        }
        private void OnDisable()
        {   //revert the embark collider to its original size when the outriggers are removed
            embarkCollider.localScale = new Vector3(1f, 1f, 1f);
        }
        public void FixedUpdate()
        {
            if ((bool)bps && bps.performanceModeIsOn()) return;
            
            //if (GameState.currentBoat != boat) return;
            
            float waterHeight = OceanHeight.GetHeight(helper, t.position);  //get ocean height at the outrigger position
            height = t.position.y - waterHeight; //calculate the height of the outrigger above the water

            if (height > 0.8f) return; //if the outrigger is above the water, do nothing
            {
                foam.SetActive(false);
                
            }

            //Calculate non linear force based on submersion level as y = forceMultiplier * x^2
            float submersion = 1 - Mathf.InverseLerp(0, 0.8f, height);  //0.8 is the height of the outrigger hull
            float buoyancy = forceMultiplier * submersion * submersion;

            //Calculate damping force
            Vector3 outriggerVel = rb.GetPointVelocity(t.position);
            float verticalVel = outriggerVel.y;

            float damping = -verticalVel * dampingMultiplier * submersion;

            magnitude = buoyancy + damping;

            rb.AddForceAtPosition(Vector3.up * magnitude, t.position, ForceMode.Acceleration);
            AdjustFoam();
        }

        private void AdjustFoam()
        {   //adjust the foam based on the height of the outrigger
            float waterHeight = OceanHeight.GetHeight(helper, t.position);
            float height = t.position.y - waterHeight; //calculate the height of the outrigger above the water
            if (height < 0.8f)
            {   //if the outrigger is submerged, enable foam and adjust its position
                foam.SetActive(true);
                foam.transform.localPosition = new Vector3(0f, -height * 0.5f, 0f);
            }
            else
            {   //if the outrigger is above the water, disable foam
                foam.SetActive(false);
            }
        }
    }
}
