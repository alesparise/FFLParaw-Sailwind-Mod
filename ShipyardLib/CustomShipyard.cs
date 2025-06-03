using UnityEngine;

namespace ShipyardLib
{
    public class CustomShipyard : MonoBehaviour
    {   //this component enables the custom shipyard system for the boat entering the shipyard

        [Header("Coloring")]
        [Tooltip("Set the reference to one of the objects using the hull material")]
        public MeshRenderer hull;
        [Tooltip("Set the reference to one the objects using various trims material")]
        public MeshRenderer[] trims;
        [Tooltip("Set the reference to one of the objects using the cloths material")]
        public MeshRenderer cloth;

        private Material hullMaterial;
        private Material[] trimMaterials;
        private Material clothMaterial;

        //things this should allow:
        //extra categories
        //sliding masts / shrouds / stays
        //boat coloring (store parts lists?)

        public void Awake()
        {
            hullMaterial = hull?.sharedMaterial;
            trimMaterials = new Material[trims.Length];
            for (int i = 0; i < trims.Length; i++)
            {
                trimMaterials[i] = trims[i]?.sharedMaterial;
            }
            clothMaterial = cloth?.sharedMaterial;
        }
    }
    public enum Category
    {
        //vanilla
        //sail
        masts = 0,
        other = 1,  //might be renamed to shrouds?
        stays = 2,
        //modded
        structures = 3,
        decorations = 4,
        shrouds = 5,
        color = 6,
        engines = 7,

    }
}
