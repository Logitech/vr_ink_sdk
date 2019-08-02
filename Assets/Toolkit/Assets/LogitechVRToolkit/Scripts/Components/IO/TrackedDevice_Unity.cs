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
    /// TODO This class has not been thoroughly tested, we recommend using the SteamVR abstraction.
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
            {StylusButton.TouchstripTouch, "joystick button 2"}, // TODO button might be wrong? Need to check.
            {StylusButton.TouchstripClick, "joystick button 3"}, // TODO button might be wrong? Need to check.
            {StylusButton.Menu, "joystick button 8"}, // or 16
            {StylusButton.Grip, "left grip"}
        };
        private readonly Dictionary<StylusButton, string> _buttonsToJoystickMapRight = new Dictionary<StylusButton, string>()
        {
            {StylusButton.Primary, "joystick button 15"},
            {StylusButton.Menu, "joystick button 0"},
            {StylusButton.TouchstripTouch, "joystick button 9"}, // or 17, TODO button might be wrong? Need to check.
            {StylusButton.TouchstripClick, "joystick button 10"}, // TODO button might be wrong? Need to check.
            {StylusButton.Grip, "right grip"}
        };

        private readonly Dictionary<StylusAxisInput, string> _axesToJoystickMapLeft = new Dictionary<StylusAxisInput, string>()
        {
            {StylusAxisInput.Primary, "9th axis"},
            {StylusAxisInput.TrackpadX, "X Axis"},
            {StylusAxisInput.TrackpadY, "Y Axis"} // TODO Axis might be wrong? Need to check.
        };
        private readonly Dictionary<StylusAxisInput, string> _axesToJoystickMapRight = new Dictionary<StylusAxisInput, string>()
        {
            {StylusAxisInput.Primary, "10th axis"},
            {StylusAxisInput.TrackpadX, "5th axis"},
            {StylusAxisInput.TrackpadY, "6th axis"} // TODO Axis might be wrong? Need to check.
        };

        private bool grabDownLastFrame = false;
        private bool grabUpLastFrame = true;

        public TrackedDevice_Unity()
        {
            // TODO Fix this algorithm - there are two pens showing up (Left AND Right).
            // Look for "OpenVR Controller(logi_pen_v3.0) - Right" or "Left" in Input.GetJoystickNames() to determine handedness.
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

        // Note: grab button is mapped to an axis whose value is either 0.0 or 1.0 for vive (for Oculus analog grab
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
                        grabDownLastFrame = false;
                    }
                    else if (Input.GetAxis(native) > 0f && !grabDownLastFrame)
                    {
                        LogitechToolkitManager.Instance.StartCoroutine(RunNextFrame(() => grabDownLastFrame = true));

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
                        grabUpLastFrame = false;
                    }
                    else if (Input.GetAxis(native) == 0f && !grabUpLastFrame)
                    {
                        LogitechToolkitManager.Instance.StartCoroutine(RunNextFrame(() => grabUpLastFrame = true));
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
            return; // Nothing needed for Unity
        }

        // TODO Implement
        public void SendHapticPulse(float delayInSeconds, float durationInSeconds, float frequency, float amplitude)
        {
            Debug.LogError("Haptics are only available using the SteamVR abstraction");
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
