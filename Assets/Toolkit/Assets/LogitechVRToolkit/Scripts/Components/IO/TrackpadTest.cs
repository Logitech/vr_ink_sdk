namespace Logitech.XRToolkit.Testing
{
    using Logitech.XRToolkit.Utils;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.IO;
    using UnityEngine;

    /// <summary>
    /// Used to print test output for a <see cref="TrackedDevice_Trackpad"/>.
    /// </summary>
    public class TrackpadTest : MonoBehaviour
    {
        [SerializeField] private TrackedDeviceProvider _trackedDeviceProvider;
        [Header("Tap")] [SerializeField] private bool _printTap;

        [Header("Double Tap")] [SerializeField]
        private bool _printDoubleTap;

        [Header("IsInZone")] [SerializeField] private bool _printIsInZone;
        [SerializeField] private ETouchZone _touchZone;

        [Header("Touch Remained In Zone")] [SerializeField]
        private bool _printTouchRemainedInZone;

        [SerializeField] private bool _requireAllTrue;
        [SerializeField] private ETouchZone[] _touchZones;
        [Header("Swipe")] [SerializeField] private bool _printSwipe;
        [SerializeField] private bool _anySwipe;
        [SerializeField] private ESwipeDirection _swipeDirection;
        [Header("Scroll")] [SerializeField] private bool _printScrollX;
        [SerializeField] private bool _printScrollY;

        public Transform TestTransform;

        private void Update()
        {
            if (_printTap && _trackedDeviceProvider.GetOutput().OnTap())
            {
                print("Tap");
                TestTransform.GetComponent<Renderer>().material.color = Random.ColorHSV();
            }

            if (_printDoubleTap && _trackedDeviceProvider.GetOutput().OnDoubleTap())
            {
                print("Double Tap");
                TestTransform.GetComponent<Renderer>().material.color = Random.ColorHSV();
            }

            if (_printIsInZone && _trackedDeviceProvider.GetOutput().IsInZone(_touchZone))
            {
                print("Is in zone " + _touchZone);
            }

            if (_printTouchRemainedInZone &&
                _trackedDeviceProvider.GetOutput().TouchRemainedInZone(_requireAllTrue, _touchZones))
            {
                print(_requireAllTrue
                    ? "Touch position remained in all zones"
                    : "Touch position remained in at least one zone");
            }

            if (_printSwipe)
            {
                if (_anySwipe)
                {
                    print(_trackedDeviceProvider.GetOutput().OnSwipe());
                }
                else if (_trackedDeviceProvider.GetOutput().OnSwipe(_swipeDirection))
                {
                    print("Swipe " + _swipeDirection);
                }
            }

            if (_printScrollX)
            {
                print(_trackedDeviceProvider.GetOutput().Scroll().x);
            }

            if (_printScrollY)
            {
                print(_trackedDeviceProvider.GetOutput().Scroll().y);
            }
        }
    }
}
