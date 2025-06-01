using System.Collections;
using UnityEngine;

namespace FFLParaw
{   ///Simple trapdoor, open/closes when clicked

    public class Trapdoor : GoPointerButton
    {
        private Vector3 axis;
        private Vector3 ogRotation;

        private Transform trapdoor;
        private Transform walkTrapdoor;

        private bool isMoving;
        private bool isOpen; // true = open, false = closed
        public void Init(Transform boatWalkCol, Vector3 a)
        {
            trapdoor = transform;
            walkTrapdoor = Helpers.FindWalkColObject(trapdoor, boatWalkCol)?.transform;

            ogRotation = trapdoor.localEulerAngles;

            axis = a;
        }

        public override void OnActivate()
        {
            Debug.Log("Clicked on trapdoor: " + gameObject.name);
            if (isMoving) return;
            StartCoroutine(OpenClose(!isOpen));
        }
        private IEnumerator OpenClose(bool open)
        {   //pass true to open, false to close
            isMoving = true;

            Vector3 targetRot = open ? ogRotation + axis * 90f : ogRotation;
            Vector3 startRot = trapdoor.localEulerAngles;

            float elapsedTime = 0f;
            float duration = 1f;
            while (elapsedTime < duration)
            {
                trapdoor.localEulerAngles = Vector3.Lerp(startRot, targetRot, elapsedTime / duration);
                walkTrapdoor.localEulerAngles = Vector3.Lerp(startRot, targetRot, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            trapdoor.localEulerAngles = targetRot;
            walkTrapdoor.localEulerAngles = targetRot;
            isOpen = !isOpen;
            isMoving = false;
        }
    }
}
