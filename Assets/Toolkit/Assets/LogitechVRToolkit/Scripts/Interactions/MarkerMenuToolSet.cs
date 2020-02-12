namespace Logitech.XRToolkit.Scripts.Interactions
{
    using Logitech.XRToolkit.Inking;
    using Logitech.XRToolkit.Providers;

    using UnityEngine;

    public class MarkerMenuToolSet : MonoBehaviour
    {
        private enum EMarkerMenuColorCode
        {
            MarkerMenuCyan = 0,
            MarkerMenuRed = 1,
            MarkerMenuYellow = 2
        }

        [SerializeField]
        private TrackedDeviceProvider _pointingDevice;

        [SerializeField]
        private GameObject _blueLight;

        [SerializeField]
        private GameObject _redLight;

        private int _lightStatus;

        public void TriggerHaptic()
        {
            _pointingDevice.GetOutput().SendHapticPulse(0.0f, 0.8f, 0.4f, 0.4f);
        }

        public void ToogleLights()
        {
            _lightStatus = (_lightStatus + 1) % 3;
            switch (_lightStatus)
            {
                case 0:
                    _blueLight.SetActive(true);
                    _redLight.SetActive(true);
                    break;
                case 1:
                    _blueLight.SetActive(false);
                    _redLight.SetActive(true);
                    break;
                case 2:
                    _blueLight.SetActive(true);
                    _redLight.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public void ChangeDeviceColor(int colorCode)
        {
            switch (colorCode)
            {
                case (int) EMarkerMenuColorCode.MarkerMenuCyan:
                    DrawingVariables.Instance.Colour = Color.cyan;
                    break;
                case (int) EMarkerMenuColorCode.MarkerMenuRed:
                    DrawingVariables.Instance.Colour = Color.red;
                    break;
                case (int) EMarkerMenuColorCode.MarkerMenuYellow:
                    DrawingVariables.Instance.Colour = Color.yellow;
                    break;
                default:
                    break;
            }
        }
    }
}
