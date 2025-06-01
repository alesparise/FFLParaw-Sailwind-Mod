using UnityEngine;

namespace FFLParawScripts
{
    public class SpeedTracker : MonoBehaviour
    {
        public Rigidbody rb;
        public TextMesh text;
        private Transform player;

        public void Awake()
        {
            rb = rb ?? GetComponentInParent<Rigidbody>();
            text = text ?? GetComponentInChildren<TextMesh>();

            if (text == null) enabled = false;

            player = Camera.main.transform;
        }
        public void FixedUpdate()
        {
            float speed = rb.velocity.magnitude * 1.94384f;
            text.text = $"Speed: {speed:F1} kn"; //convert m/s to knots

            text.transform.LookAt(player);
        }
    }
}
