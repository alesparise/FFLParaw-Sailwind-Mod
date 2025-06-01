using UnityEngine;
using FFLParaw;

namespace FFLParawScripts
{
    public class TrapdoorBridge : MonoBehaviour
    {
        [Tooltip("The boat walk col")]
        public Transform boatWalkCol;
        [Tooltip("The axis around which the trapdoor rotates")]
        public Vector3 axis = new Vector3(0f, 1f, 0f);
        
        private void Awake()
        {
            gameObject.AddComponent<Trapdoor>().Init(boatWalkCol, axis);
            Destroy(this);
        }
        private void OnDrawGizmosSelected()
        {   //Draw the axis of rotation in the editor

            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, axis * 2f);
        }
    }
}
