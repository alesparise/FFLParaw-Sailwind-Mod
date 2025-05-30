using UnityEngine;

namespace FFLParawScripts
{
    public class ObjectToggler : MonoBehaviour
    {
        [Header("Objects to be enabled with this")]
        [Tooltip("Objects to enable when this is enabled. If this is disabled, they will be disabled too.")]
        public GameObject[] enableWithThis; //objects to enable when this is enabled

        public void OnEnable()
        {   //enable the objects when this is enabled
            foreach (GameObject obj in enableWithThis)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        public void OnDisable()
        {   //disable the objects when this is disabled
            foreach (GameObject obj in enableWithThis)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
