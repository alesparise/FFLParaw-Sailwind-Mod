using UnityEngine;
using Crest;

namespace FFLParawScripts
{
    /// <summary>
    /// Controls the outrigger behaviour
    /// </summary>
    public class Outrigger : MonoBehaviour
    {
        public GameObject foam;
        public GameObject interactionFront;    //interaction sphere at the front of the outrigger
        public GameObject interactionBack;     //interaction sphere at the back of the outrigger

        private Transform boat;
        private Transform embarkCollider;
        private Transform t;

        private Rigidbody rb;

        private SampleHeightHelper helper = new SampleHeightHelper();

        private BoatPerformanceSwitcher bps;

        private Rudder rudder;

        private float forceMultiplier = 5f;
        private float dampingMultiplier = 5f;
        private float magnitude;
        public float height;

        private bool inWater => height < 0.8f;

        public void Awake()
        {
            boat = GetComponentInParent<BoatHorizon>().transform;
            embarkCollider = boat.GetComponentInChildren<BoatEmbarkCollider>().transform;
            rudder = boat.GetComponentInChildren<Rudder>();
            rb = boat.parent.GetComponent<Rigidbody>();
            bps = boat.parent.GetComponent<BoatPerformanceSwitcher>();

            t = transform;

            if (foam == null) foam = t.Find("WaterFoam").gameObject;
            if (interactionFront == null) interactionFront = t.Find("WaterSphereLeftFront")?.gameObject ?? t.Find("WaterSphereRightFront").gameObject;
            if (interactionBack == null) interactionBack = t.Find("WaterSphereLeftBack")?.gameObject ?? t.Find("WaterSphereRightBack").gameObject;
        }
        private void OnEnable()
        {   //widen the embark collider when the outriggers are installed
            //lower rudder power when outriggers are installed
            embarkCollider.localScale = new Vector3(1f, 1.8f, 1f);
            rudder.rudderPower = 60;
        }
        private void OnDisable()
        {   //revert the embark collider to its original size when the outriggers are removed
            //restore rudder power when outriggers are removed
            embarkCollider.localScale = Vector3.one;
            rudder.rudderPower = 80;
        }
        public void FixedUpdate()
        {
            if ((bool)bps && bps.performanceModeIsOn()) return;

            //if (GameState.currentBoat != boat) return;

            float waterHeight = OceanHeight.GetHeight(helper, t.position);  //get ocean height at the outrigger position
            height = t.position.y - waterHeight; //calculate the height of the outrigger above the water

            //Calculate non linear force based on submersion level as y = forceMultiplier * x^2
            float submersion = 1 - Mathf.InverseLerp(0, 0.8f, height);  //0.8 is the height of the outrigger hull
            float buoyancy = forceMultiplier * submersion * submersion;

            //Calculate damping force
            Vector3 outriggerVel = rb.GetPointVelocity(t.position);
            float verticalVel = outriggerVel.y;

            float damping = -verticalVel * dampingMultiplier * submersion;

            magnitude = buoyancy + damping;

            rb.AddForceAtPosition(Vector3.up * magnitude, t.position, ForceMode.Acceleration);
            AdjustWaterInteractions();
        }
        private void AdjustWaterInteractions()
        {   //adjust the foam based on the height of the outrigger

            if (inWater && !interactionFront.activeInHierarchy)
            {   //if the outrigger is submerged, enable foam and adjust its position
                foam.SetActive(true);
                interactionFront.SetActive(true);
                interactionBack.SetActive(true);
            }
            else if (!inWater && interactionFront.activeInHierarchy)
            {   //if the outrigger is above the water, disable foam
                foam.SetActive(false);
                interactionFront.SetActive(false);
                interactionBack.SetActive(false);
            }
        }
    }
}
