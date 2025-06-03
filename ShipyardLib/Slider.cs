using System;
using UnityEngine;

namespace ShipyardLib
{
    /// <summary>
    /// Controls the slider object, allowing the use of mouse (drag the cursor or scroll to adjust a value)
    /// </summary>
    public class Slider : MonoBehaviour
    {
        public GameObject cursorObject;
        public GameObject wheelColliderObject;

        public TextMesh label;

        private CursorButton cursor;
        private WheelColliderButton wheelCollider;

        public float limit = 2f;
        public float sensitivity = 0.1f;

        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnValueChanged?.Invoke(_value); 
                }
            }
        }

        public event Action<float> OnValueChanged;

        public void Awake()
        {
            cursor = cursorObject.AddComponent<CursorButton>();
            cursor.limit = limit;
            cursor.sensitivity = sensitivity;

            wheelCollider = wheelColliderObject.AddComponent<WheelColliderButton>();
            wheelCollider.limit = limit;
            wheelCollider.sensitivity = sensitivity / 10f;
            wheelCollider.cursor = cursor.t;
        }
        public void SetUpSlider(string sliderName)
        {
            name = sliderName;
            label.text = sliderName;
        }
        public void Update()
        {
            float x = cursor.t.localPosition.x;
            
            Value = Mathf.InverseLerp(-limit, limit, x);
        }
    }
}
