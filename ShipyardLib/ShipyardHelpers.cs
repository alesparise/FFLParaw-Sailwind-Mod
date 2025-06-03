using System.IO;
using System.Reflection;
using UnityEngine;

namespace ShipyardLib
{
    public static class ShipyardHelpers
    {
        //Data
        private static AssetBundle bundle;
        
        //Prefabs
        public static GameObject slider;

        //References
        public static Transform baseUI;
        public static CustomShipyardUI modUI;

        public static void Setup()
        {
            //load bundle
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bundlePath = Path.Combine(folder, "shipyardlib");
            bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Debug.LogError("ShipyarLib: Could not load the bundle!");
                return;
            }

            //load prefabs
            string sliderPath = "Assets/ShipyardLib/shipyard_slider.prefab";
            slider = bundle.LoadAsset<GameObject>(sliderPath);

            //find vanilla UI
            baseUI = GameObject.Find("shipyard UI").transform.Find("UI");

            //Prepare the UI
            modUI = new GameObject("custom_shipyard_ui").AddComponent<CustomShipyardUI>();
            Transform modUITransform = modUI.transform;
            modUITransform.SetParent(baseUI);
            modUITransform.localPosition = Vector3.zero;
            modUITransform.localRotation = Quaternion.identity;

            //debug slider:
            GameObject s = Object.Instantiate(slider, modUITransform);
            modUI.attachmentSlider1 = s.GetComponent<Slider>();

            Debug.LogWarning("ShipyardLib: Setup done!");
        }

        //HELPER METHODS
        public static void EnableUI()
        {
            
        }

        //Vector3 stuff:
        public static Vector3 Clamp(Vector3 v, float min, float max, Axis a)
        {   //helper method to clamp a vector
            if (a == Axis.X)
            {
                return new Vector3 (Mathf.Clamp(v.x, min, max), v.y, v.z);
            }
            else if (a == Axis.Y)
            {
                return new Vector3(v.x, Mathf.Clamp(v.y, min, max), v.z);
            }
            else //(a == Axis.Z)
            {
                return new Vector3(v.x, v.y, Mathf.Clamp(v.z, min, max));
            }
        }
        public static Vector3 Clamp(Vector3 v, float limit, Axis a)
        {   //helper method to clamp a vector with only one limit

            return Clamp(v, -limit, limit, a);
        }
        public static Vector3 GetAxis(Axis a)
        {
            if (a == Axis.X) return Vector3.right;
            else if (a == Axis.Y) return Vector3.up;
            else return Vector3.forward; //if (a == Axis.Z)
        }
        public enum Axis
        {   //helper for storing axis
            X = 0,
            Y = 1,
            Z = 2,
        }
    }
}
