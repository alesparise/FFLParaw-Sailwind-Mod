using UnityEngine;
using FFLParaw;

namespace FFLParawScripts
{
    public class TillerBridge : MonoBehaviour
    {
        public Rudder rudder;
        public HingeJoint joint;
        public AudioSource audio;
        public HingeJoint leftRudder;
        public HingeJoint rightRudder;

        private void Awake()
        {
            gameObject.AddComponent<ParawTiller>().Init(rudder, joint, audio, leftRudder, rightRudder);
            Destroy(this);
        }
    }
}
