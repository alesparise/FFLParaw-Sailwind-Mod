using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ShipyardLib
{
    public class DynamicPart : MonoBehaviour
    {
        private Transform t;
        private Transform walkCol;
        private Transform[] keepStill;
        public Transform start;  //point one. This is where the dynamic object start
        public Transform end;    //point two. This is where the dynamic object ends.

        private MeshFilter meshFilter;

        private Mesh mesh;

        private Mast mast;

        private CapsuleCollider capsule;

        private List<Sail> sails;
        
        private Vector3 originalScale;
        private Vector3 lastPos;
        private Vector3[] originalVertices;
        
        private float originalDistance;
        private float originalOffset;

        private bool isMast;
        
        public void Awake()
        {
            t = transform;
            int num = t.childCount;

            //get mesh so we can change the mesh scale instead of the transform scale
            //this allow the attached sail and all children to keep their original scale
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.sharedMesh;
            originalVertices = mesh.vertices;

            //get some of the original data so we can use it later
            Vector3 ogDir = (end.position - start.position);
            originalDistance = ogDir.magnitude;
            originalScale = t.localScale;
            originalOffset = ((start.position + ogDir * 0.5f) - t.position).magnitude;

            //figure out if it's a mast or not, get walk col, capsule, etc
            mast = GetComponent<Mast>();
            if (mast != null)
            {
                isMast = true;
                capsule = GetComponent<CapsuleCollider>();
                walkCol = mast.walkColMast;
            }
            else
            {
                walkCol = GetComponent<BoatPartOption>().walkColObject.transform;
            }
            
        }
        public void Update()
        {
            if (GameState.currentShipyard == null || lastPos == end.position) return;

            PreserveChildrenWhileMoving(t, () =>
            {
                Connect(); // Your transform/rotation/scaling logic
            });
        }
        public void Connect()
        {   //moves, rotates and scales the object so that it "connects" the two attachments

            Vector3 startPos = start.position;
            Vector3 endPos = end.position;
            Vector3 dir = endPos - startPos;

            float distance = dir.magnitude;

            //1) Find the midpoint and move there
            Vector3 midPoint = startPos + dir * 0.5f;
            t.position = midPoint;
            
            //2) Rotate to align with direction
            t.rotation = Quaternion.LookRotation(dir, t.up);

            //3) Scale the mesh to match distance
            Vector3 scale = originalScale;
            scale.z = distance / originalDistance;
            Vector3[] newVerts = new Vector3[originalVertices.Length];
            for (int i = 0; i < newVerts.Length; i++)
            {
                Vector3 v = originalVertices[i];
                v.z *= scale.z;
                newVerts[i] = v;
            }
            mesh.vertices = newVerts;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //4) Offset to match mesh offset
            float offset = originalOffset * scale.z;
            t.position += t.forward * offset;

            //5) Apply local rotation and position to the walkCol
            walkCol.localRotation = t.localRotation;
            walkCol.localPosition = t.localPosition;

            //6) if this is a mast, adjust mast height and capsule collider
            if (isMast)
            {
                Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
                Vector3 extents = bounds.extents;
                float radius = extents.x;
                float maxHeight = (float)Math.Round(extents.z * 2f - (extents.z + bounds.center.z), 2);

                float mastHeight = name.Contains("stay") ? (float)Math.Round(0.75f * maxHeight, 2) : (float)Math.Round(maxHeight, 2);

                capsule.direction = 2;
                capsule.radius = radius * 1.4f;  //keeping a bit of margin to avoid sails clipping
                capsule.height = mastHeight + 2f;
                capsule.center = new Vector3(0f, 0f, -(mastHeight / 2f));
                mast.mastCols = new CapsuleCollider[1];
                mast.mastCols[0] = capsule;

                foreach (Sail s in sails)
                {
                    UpdateSail(s);
                }
            }

            lastPos = endPos;
            Debug.LogWarning("Reached end of Connect...");
        }
        private void PreserveChildrenWhileMoving(Transform parent, Action doTransformChange)
        {
            List<Transform> children = new List<Transform>();
            List<Sail> ss = new List<Sail>();

            // 1) Cache world positions and rotations
            foreach (Transform child in parent)
            {
                Sail s = child.GetComponent<Sail>();
                if (s != null)
                {
                    ss.Add(s);
                    continue;
                }
                children.Add(child);
            }

            List<Vector3> childPositions = children.Select(c => c.position).ToList();
            List<Quaternion> childRotations = children.Select(c => c.rotation).ToList();

            // 2) Perform transformation
            doTransformChange();

            // 3) Restore world transforms
            for (int i = 0; i < children.Count; i++)
            {
                children[i].position = childPositions[i];
                children[i].rotation = childRotations[i];
            }
        }
        private void UpdateSail(Sail s)
        {
            //make sure the sail is in the allowed range of install positions
            //see ShipyardSailInstaller.MoveHeldSail()
            ShipyardSailInstaller.MoveHeldSail();
            s.UpdateInstallPosition();

            //update everyrhing
            mast.UpdateSailOrder();
            mast.UpdateControllerAttachments();

            ShipyardUI.instance.UpdateMastSailsList();
            ShipyardUI.instance.UpdateButtonSelections();

            Shipyard shipyard = GameState.currentShipyard;
            shipyard.sailInstaller.RecheckAllSailsCols();
            shipyard.UpdateOrder();
        }
    }
}
