namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.XR;
    using UnityEngine;

    /// <summary>
    /// Unity abstraction of a <see cref="TrackedDevice"/> implementing <see cref="IIOTrackedDevice"/>. Note that for
    /// values based on axes (grab, primary, and trackpad), Unity must be configured accordingly. In order to do this go
    /// to Edit > Project Settings > Input and add the corresponding Axes.
    /// TODO This class has not been thoroughly tested, we highly recommend using the SteamVR or Oculus abstraction.
    /// </summary>e"/>
    public class TrackedDevice_Unity : IIOTrackedDevice
    {
        public Handedness StylusHand { get; set; }
        public Utils.DeviceType Type { get; set; }

        public int DeviceIndex
        {
            get
            {
                if (StylusHand == Handedness.Primary)
                {
                    return 1;
                }
                return 0;
            }
        }

        private readonly Dictionary<StylusButton, string> _buttonsToJoystickMapLeft = new Dictionary<StylusButton, string>()
        {
            {StylusButton.Primary, "joystick button 14"},
            {StylusButton.TouchstripTouch, "joystick button 16"},
            {StylusButton.TouchstripClick, "joystick button 8"},
            {StylusButton.Menu, "joystick button 2"},
            {StylusButton.Grip, "left grip"}
        };
        private readonly Dictionary<StylusButton, string> _buttonsToJoystickMapRight = new Dictionary<StylusButton, string>()
        {
            {StylusButton.Primary, "joystick button 15"},
            {StylusButton.TouchstripTouch, "joystick button 17"},
            {StylusButton.TouchstripClick, "joystick button 9"},
            {StylusButton.Menu, "joystick button 0"},
            {StylusButton.Grip, "right grip"}
        };

        private readonly Dictionary<StylusAxisInput, string> _axesToJoystickMapLeft = new Dictionary<StylusAxisInput, string>()
        {
            {StylusAxisInput.Primary, "9th axis"},
            {StylusAxisInput.TrackpadX, "X Axis"},
            {StylusAxisInput.TrackpadY, "Y Axis"}
        };
        private readonly Dictionary<StylusAxisInput, string> _axesToJoystickMapRight = new Dictionary<StylusAxisInput, string>()
        {
            {StylusAxisInput.Primary, "10th axis"},
            {StylusAxisInput.TrackpadX, "4th axis"},
            {StylusAxisInput.TrackpadY, "5th axis"}
        };

        private bool _grabDownLastFrame = false;
        private bool _grabUpLastFrame = true;

        public TrackedDevice_Unity()
        {
            // TODO Fix this algorithm - there are two Styli showing up (Left AND Right).
            // Look for "OpenVR Controller(logi) - Right" or "Left" in Input.GetJoystickNames() to determine handedness.
            StylusHand = Handedness.Primary;
            foreach (var controller in Input.GetJoystickNames())
            {
                if (controller.Contains("logi_pen"))
                {
                    if (controller.EndsWith("Left"))
                    {
                        StylusHand = Handedness.NonDominant;
                        break;
                    }
                }
            }
        }

        public float GetAxis(StylusAxisInput stylusAxisInput)
        {
            var native = GetAxisString(stylusAxisInput);
            return Input.GetAxisRaw(native);
        }

        // Note: grab button is mapped to an axis whose value is either 0.0 or 1.0 for Vive (for Oculus analog grab
        // compatibility) which makes it a bit more tedious to get the input below.
        public bool GetButton(StylusButton stylusButton)
        {
            var native = GetButtonString(stylusButton);
            if (native != null)
            {
                if (stylusButton == StylusButton.Grip)
                {
                    return Input.GetAxis(native) > 0f;
                }
                else
                {
                    return Input.GetKey(native);
                }
            }
            else
            {
                return false;
            }
        }

        private IEnumerator RunNextFrame(System.Action function)
        {
            yield return null;
            function();
        }

        public bool GetButtonDown(StylusButton stylusButton)
        {
            var native = GetButtonString(stylusButton);
            if (native != null)
            {
                if (stylusButton == StylusButton.Grip)
                {
                    if (Input.GetAxis(native) == 0f)
                    {
                        _grabDownLastFrame = false;
                    }
                    else if (Input.GetAxis(native) > 0f && !_grabDownLastFrame)
                    {
                        LogitechToolkitManager.Instance.StartCoroutine(RunNextFrame(() => _grabDownLastFrame = true));

                        return true;
                    }
                    return false;
                }
                else
                {
                    return Input.GetKeyDown(native);
                }
            }
            else
            {
                return false;
            }
        }

        public bool GetButtonUp(StylusButton stylusButton)
        {
            var native = GetButtonString(stylusButton);
            if (native != null)
            {
                if (stylusButton == StylusButton.Grip)
                {
                    if (Input.GetAxis(native) > 0f)
                    {
                        _grabUpLastFrame = false;
                    }
                    else if (Input.GetAxis(native) == 0f && !_grabUpLastFrame)
                    {
                        LogitechToolkitManager.Instance.StartCoroutine(RunNextFrame(() => _grabUpLastFrame = true));
                        return true;
                    }
                    return false;
                }
                else
                {
                    return Input.GetKeyUp(native);
                }
            }
            else
            {
                return false;
            }
        }

        public Pose GetDevicePose()
        {
            var node = XRNode.RightHand;
            if (StylusHand == Handedness.NonDominant)
            {
                node = XRNode.LeftHand;
            }
            return new Pose(
                InputTracking.GetLocalPosition(node),
                InputTracking.GetLocalRotation(node)
            );
        }

        public void OnDestroy()
        {
            // Nothing needed for Unity.
        }

        public void SendHapticPulse(float delayInSeconds, float durationInSeconds, float frequency, float amplitude)
        {
            Debug.LogError("Haptics is not available in the Native Unity abstraction.");
        }

        /// <summary>
        /// Tries to translate a given Stylus button to its native Unity string (e.g. "joystick button N").
        /// </summary>
        /// <param name="stylusButton">The button to translate.</param>
        /// <returns>The corresponding string, or null if none could be found.</returns>
        private string GetButtonString(StylusButton stylusButton)
        {
            var mapToUse = _buttonsToJoystickMapLeft;
            if (StylusHand == Handedness.Primary)
            {
                mapToUse = _buttonsToJoystickMapRight;
            }
            string native;
            if (mapToUse.TryGetValue(stylusButton, out native))
            {
                return native;
            }
            else
            {
                Debug.LogErrorFormat("No native Unity button could be found for {0}", stylusButton);
                return null;
            }
        }

        private string GetAxisString(StylusAxisInput stylusAxisInput)
        {
            var mapToUse = _axesToJoystickMapRight;
            if (StylusHand == Handedness.NonDominant)
            {
                mapToUse = _axesToJoystickMapLeft;
            }
            string native;
            if (mapToUse.TryGetValue(stylusAxisInput, out native))
            {
                return native;
            }
            else
            {
                Debug.LogErrorFormat("No native Unity axis could be found for {0}", stylusAxisInput);
                return null;
            }
        }
    }
}
