using UnityEngine;

namespace FFLParawScripts
{
    public class OutriggerFoam : MonoBehaviour
    {
        private GameObject foam;

        private Rigidbody rb;

        private Outrigger outrigger;

        private BoatPerformanceSwitcher pbs;

        private float noFoamVelocity = 1;
        private float fullFoamVelocity = 8;
        private float enabledDuration = 0.25f;
        private float disabledTimer;
        private float enabledTimer;

        public void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            pbs = GetComponentInParent<BoatPerformanceSwitcher>();
            outrigger = GetComponentInParent<Outrigger>();
            foam = transform.Find("WaterFoam").gameObject;
        }

        public void Update()
        {
            if ((bool)pbs && pbs.performanceModeIsOn())
            {
                foam.SetActive(value: false);
                return;
            }

            if (outrigger.height > 0.8f)
            {
                foam.SetActive(value: false);
                return;
            }

            if (foam.activeInHierarchy)
            {
                if (enabledTimer <= 0f)
                {
                    foam.SetActive(value: false);
                }
                else
                {
                    enabledTimer -= Time.deltaTime;
                }
            }

            disabledTimer -= Time.deltaTime;
            if (!(disabledTimer <= 0f))
            {
                return;
            }

            float num = Mathf.InverseLerp(fullFoamVelocity, noFoamVelocity, rb.velocity.magnitude);
            disabledTimer = num * 2f;
            if (rb.velocity.magnitude > noFoamVelocity)
            {
                foam.SetActive(value: true);

                enabledTimer = enabledDuration;
            }
        }
    }
}
