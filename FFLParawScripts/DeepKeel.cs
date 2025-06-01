using UnityEngine;

namespace FFLParawScripts
{
    public class DeepKeel : MonoBehaviour
    {
        private BoatKeel keel;
        private Vector3 startDepth;
        private Vector3 deepDepth = new Vector3(0f, -2.6f, 0f);

        private void Awake()
        {
            keel = GetComponentInParent<BoatKeel>();
            startDepth = keel.centerOfMass;
        }
        private void OnEnable()
        {
            keel.centerOfMass = deepDepth;
        }
        private void OnDisable()
        {
            keel.centerOfMass = startDepth;
        }
    }
}
