namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Draws lines in 3D space with a controller and an analog input.
    /// </summary>
    public class AirDrawing : MonoBehaviour
    {
        [SerializeField]
        private AxisValueProvider _drawingTrigger;
        [SerializeField]
        private AirDrawingAction _airDrawingAction;
        private bool _isSmoothActive;

        // Prevent drawing when aiming at UI.
        [SerializeField]
        private RaycastTrigger _preventAirDrawingOnRaycast;
        [SerializeField]
        private CollisionTrigger _preventAirDrawingOnCollision;

        [HideInInspector]
        public bool CanDraw;
        private EButtonEvent _buttonState = EButtonEvent.OnButtonUp;

        private void Update()
        {
            if (_drawingTrigger.GetOutput() > 0 && _buttonState == EButtonEvent.OnButtonUp)
            {
                _buttonState = EButtonEvent.OnButtonDown;
            }
            else if (_drawingTrigger.GetOutput() > 0)
            {
                _buttonState = EButtonEvent.OnButton;
            }

            if (CanDraw == false && _buttonState == EButtonEvent.OnButtonDown && !_preventAirDrawingOnRaycast.IsValid())
            {
                CanDraw = true;
            }

            if (_drawingTrigger.GetOutput() <= 0)
            {
                CanDraw = false;
                _buttonState = EButtonEvent.OnButtonUp;
            }

            _airDrawingAction.Update(CanDraw && !_preventAirDrawingOnCollision.IsValid());
        }

        public void SetSmoothingActive()
        {
            _isSmoothActive = !_isSmoothActive;
            _airDrawingAction.SetSmoothingMode(_isSmoothActive);
        }

        private void OnDisable()
        {
            CanDraw = false;
        }
    }
}
