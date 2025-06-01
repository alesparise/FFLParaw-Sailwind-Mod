﻿using UnityEngine;
using static ONSPPropagationMaterial;

namespace FFLParaw
{   /// <summary>
    /// Controls the tiller behaviour
    /// </summary>
    public class ParawTiller : GoPointerButton
    {
        private Rudder rudder;

        private HingeJoint hingeJoint;

        private HingeJoint leftRudder;
        private HingeJoint rightRudder;

        private AudioSource audio;

        public bool locked;
        private bool held;

        public float input;
        private float lastInput;
        private float rotationAngleLimit;
        private const float volumeMult = 0.05f;
        private const float mult = 0.025f;        //makes the rudder more or less responsive

        public void Init(Rudder r, HingeJoint j, AudioSource a, HingeJoint lr, HingeJoint rr)
        {   //this is called by the TillerBridge component. Since this is called in TillerBridge.Awake() this can be treated
            //as this component Awake (not literally, but sort of for this usecase)
            rudder = r;
            hingeJoint = j;
            audio = a;
            input = 0f;
            lastInput = 0f;
            rotationAngleLimit = hingeJoint.limits.max;

            leftRudder = lr;
            rightRudder = rr;
        }
        public override void OnActivate(GoPointer activatingPointer)
        {   
            if (Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                MouseLook.ToggleMouseLook(newState: false);
            }

            if (!Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                StickyClick(activatingPointer);
            }

            if (locked)
            {
                Unlock();
            }
        }
        public override void OnUnactivate(GoPointer activatingPointer)
        {
            if (Settings.steeringWithMouse && activatingPointer.type == GoPointer.PointerType.crosshairMouse)
            {
                MouseLook.ToggleMouseLook(newState: true);
            }
        }
        private void ToggleLock()
        {
            if (!locked)
            {
                Lock();
            }
            else
            {
                Unlock();
            }
        }
        private void Lock()
        {
            if ((bool)stickyClickedBy)
            {
                UnStickyClick();
            }

            locked = true;
            Juicebox.juice.PlaySoundAt("lock unlock", base.transform.position, 0f, 0.66f, 0.88f);
        }
        private void Unlock()
        {
            locked = false;
            Juicebox.juice.PlaySoundAt("lock unlock", base.transform.position, 0f, 0.66f, 1f);
        }
        public override void ExtraLateUpdate()
        {   // controls the tiller and the rudder connected to it

            if ((bool)stickyClickedBy || isClicked)
            {   //when it's clicked we control it with the A and D keys
                int invert = ParawMain.invertedTillerConfig.Value ? -1 : 1;
                if ((bool)stickyClickedBy)
                {
                    if (stickyClickedBy.AltButtonDown())
                    {
                        ToggleLock();
                    }
                    if (!locked)
                    {
                        if (!held)
                        {   // increases 5 times damper and spring values so the tiller is more stable
                            held = true;
                            ChangeDamper(held);
                        }
                        input += stickyClickedBy.movement.GetKeyboardDelta().x * mult * invert;
                        if (stickyClickedBy.movement.GetKeyboardDelta().y != 0)
                        {   //this should detect pressing forward or backward buttons
                            input = 0;
                        }
                    }
                    ApplyRotationLimit();
                    RotateRudder();
                }
                else if (isClicked && Settings.steeringWithMouse)
                {
                    if (isClickedBy.pointer.AltButtonDown())
                    {
                        ToggleLock();
                    }
                    if (!locked)
                    {
                        input += isClickedBy.GetDeltaRotation().z * mult * 2 * invert;
                    }
                    ApplyRotationLimit();
                    RotateRudder();
                }
            }
            else if (locked)
            {   //keep applying the rotation if it's locked. Also the input does not get set to 0 if it's locked
                ApplyRotationLimit();
                RotateRudder();
            }
            else
            {   //not clicked and not locked, bring the spring values to vanilla ones
                if (held)
                {
                    held = false;
                    ChangeDamper(held);
                }
                //set the input value from the rudder angle so that it does not bounce when clicked
                input = rudder.currentAngle;
                if (!isLookedAt) ForceUnlook();
            }
            if ((bool)audio)
            {   //play creaking sound when moving rudder
                float num = Mathf.Abs(input - lastInput) / Time.deltaTime;
                audio.volume = Mathf.Lerp(audio.volume, num * volumeMult, Time.deltaTime * 3f);
            }
            lastInput = input;
        }
        private void RotateRudder()
        {   //old, taken from GPButtonSteeringWheel
            float num = input / rotationAngleLimit;
            JointSpring spring = hingeJoint.spring;
            spring.targetPosition = hingeJoint.limits.max * num;
            hingeJoint.spring = spring;
            rightRudder.spring = spring;
            spring.targetPosition *= -1f;
            leftRudder.spring = spring;
        }
        private void ApplyRotationLimit()
        {   //limits the input value betwee the min and max rotation limit
            if (input > rotationAngleLimit)
            {
                input = rotationAngleLimit;
            }

            if (input < 0f - rotationAngleLimit)
            {
                input = 0f - rotationAngleLimit;
            }
        }
        private void ChangeDamper(bool held)
        {   //Changes the damper value to make the tiller firmer when being held

            JointSpring spring = hingeJoint.spring;
            spring.spring = held ? 250f : 50f;
            spring.damper = held ? 50f : 10f;
            hingeJoint.spring = spring;
        }
    }
}
