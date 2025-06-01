using UnityEngine;

namespace FFLParaw
{
    public class Helpers
    {
        public static GameObject FindWalkColObject(Transform target, Transform traversedTransform)
        {   //recursively searches for the right walk col object. Name based. Names must be unique.
            foreach (Transform child in traversedTransform)
            {
                if (child.name == target.name)
                {
                    Debug.Log("Found the corresponding walk col object for " + target.name);
                    return child.gameObject;
                }
                GameObject found = FindWalkColObject(target, child);
                if (found != null) return found;
            }

            return null;
        }
    }
}
