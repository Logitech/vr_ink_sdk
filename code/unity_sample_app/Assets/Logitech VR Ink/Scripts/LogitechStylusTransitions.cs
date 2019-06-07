/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    ///     This class will manage the button transitions on the pen, such as changing color
    ///     of the buttons as well as the button animation.
    /// </summary>
    public class LogitechStylusTransitions : MonoBehaviour
    {
        private SteamVR_TrackedController _controller;
        [SerializeField] private Color _defaultColor;

        [SerializeField] private float _offsetMovement = 0.0006f;
        public GameObject GripButtonLeft;
        public GameObject GripButtonRight;

        [Header("Button Gameobjects")] public GameObject MainButton;

        public GameObject MenuButton;
        public GameObject PadButton;


        private IEnumerator Start()
        {

            yield return new WaitForSeconds(0.5f);

            _controller = LogitechStylus.Instance.Controller;

            if (_controller == null)
            {
                Debug.LogError("Could not find LogitechStylus instance in the scene. Make sure " +
                               "LogitechStylus.cs is attached to a GameObject in the scene");
                yield return null;
            }


            //Register callbacks for animation events
            _controller.Gripped += GrippedClikedAnimation;
            _controller.Ungripped += UngrippedClikedAnimation;

            _controller.TriggerClicked += MainButtonClickedAnimation;
            _controller.TriggerUnclicked += MainButtonReleasedAnimation;

            _controller.MenuButtonClicked += MenuButtonClickedAnimation;
            _controller.MenuButtonUnclicked += MenuButtonReleasedAnimation;

            _controller.PadClicked += PadButtonClickedAnimation;
            _controller.PadUnclicked += PadButtonReleasedAnimation;

            yield return null;
        }

        /// <summary>
        ///     Change Color of material on target renderer
        ///     Change both regular and emission color.
        ///     Can use the static methods outside of this class to change color of anything else
        /// </summary>
        /// <param name="target"></param>
        /// <param name="newColor"></param>
        public static void ChangeColor(Renderer target, Color newColor)
        {
            target.material.color = newColor;
            if (newColor != Color.black)
            {
                //if emission is turned off in the target.material setting nothing will happen at that line.
                target.material.SetColor("_EmissionColor", newColor);
            }
        }

        /// <summary>
        ///     Overload for Gameobject Parameter
        /// </summary>
        public static void ChangeColor(GameObject obj, Color newColor)
        {
            if (obj.GetComponent<Renderer>() == null)
            {
                Debug.LogError("The game object doens't have a renderer, can't change it's color !");
                return;
            }
            ChangeColor(obj.GetComponent<Renderer>(), newColor);
        }

        //Callbacks
        private void PadButtonReleasedAnimation(object sender, ClickedEventArgs e)
        {
            //Use Dotween plugin--PadButton.transform.DOLocalMoveY(0.01046997f, 0.1f);
            //StartCoroutine(DoLocalMoveY(PadButton, 0.01046997f, 0.1f)); uncomment for animation of buttons
            ChangeColor(PadButton, _defaultColor);
        }

        private void PadButtonClickedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveY(PadButton,-_offsetMovement, 0.1f,isRelative:true));
            ChangeColor(PadButton, LogitechStylus.Instance.GetPenColor());
        }

        private void MenuButtonReleasedAnimation(object sender, ClickedEventArgs e)
        {
            //  StartCoroutine(DoLocalMoveY(MenuButton,0.01046997f, 0.1f));
            ChangeColor(MenuButton, _defaultColor);
        }

        private void MenuButtonClickedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveY(MenuButton,-_offsetMovement, 0.1f,isRelative:true));
            ChangeColor(MenuButton, LogitechStylus.Instance.GetPenColor());
        }

        private void MainButtonReleasedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveY(MainButton, 0.01046997f, 0.1f));
            ChangeColor(MainButton, _defaultColor);
        }

        private void MainButtonClickedAnimation(object sender, ClickedEventArgs e)
        {
            //tartCoroutine(DoLocalMoveY(MainButton,-_offsetMovement, 0.1f,isRelative:true));
            ChangeColor(MainButton, LogitechStylus.Instance.GetPenColor());
        }

        private void UngrippedClikedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveX(GripButton, -0.01007005f, 0.1f));
            ChangeColor(GripButtonLeft, _defaultColor);
            ChangeColor(GripButtonRight, _defaultColor);
        }

        private void GrippedClikedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveX(GripButton, _offsetMovement, 0.1f,isRelative:true));
            ChangeColor(GripButtonLeft, LogitechStylus.Instance.GetPenColor());
            ChangeColor(GripButtonRight, LogitechStylus.Instance.GetPenColor());
        }

        /// <summary>
        ///     Example of Coroutine animation.
        /// </summary>
        /// <param name="obj">Gameobject to move</param>
        /// <param name="target">target position to move towards to</param>
        /// <param name="duration">duration of the animation</param>
        /// <param name="isRelative">
        ///     relative means adding target to current transfrom as opposed
        ///     to give an absolute value as a target for the animation
        /// </param>
        /// <returns></returns>
        private IEnumerator DoLocalMove(GameObject obj, Vector3 target, float duration, bool isRelative = false)
        {
            float start = 0;
            Vector3 startPosition;
            Vector3 targetPosition;
            if (isRelative)
            {
                startPosition = obj.transform.localPosition;
                targetPosition = startPosition + target;
            }
            else
            {
                startPosition = obj.transform.localPosition;
                targetPosition = target;
            }
            while (start < duration)
            {
                start += Time.deltaTime;
                var lerp = Mathf.Clamp01(start / duration);
                obj.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, lerp);
                yield return null;
            }
        }

        private IEnumerator DoLocalMoveY(GameObject obj, float offset, float duration, bool isRelative = false)
        {
            var startPosition = obj.transform.localPosition;
            var target = isRelative ? new Vector3(0, offset, 0) : new Vector3(startPosition.x, offset, startPosition.z);
            return DoLocalMove(obj, target, duration, isRelative);
        }

        private IEnumerator DoLocalMoveX(GameObject obj, float offset, float duration, bool isRelative = false)
        {
            var startPosition = obj.transform.localPosition;
            var target = isRelative ? new Vector3(offset, 0, 0) : new Vector3(offset, startPosition.y, startPosition.z);
            return DoLocalMove(obj, target, duration, isRelative);
        }
    }
}