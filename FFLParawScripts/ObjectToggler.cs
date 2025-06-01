using UnityEngine;

namespace FFLParawScripts
{
    public class ObjectToggler : MonoBehaviour
    {
        public GameObject[] objectToEnable;

        private void OnEnable()
        {   //enable all objects in the array
            foreach (GameObject obj in objectToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        private void OnDisable()
        {   //disable all objects in the array
            foreach (GameObject obj in objectToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
