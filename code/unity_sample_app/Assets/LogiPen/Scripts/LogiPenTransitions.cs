/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/
 
namespace LogiPen.Scripts
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// This class will manage the button transitions on the pen, such as changing color 
    /// of the buttons as well as the button animation.
    /// </summary>
    public class LogiPenTransitions: MonoBehaviour
    {
        private SteamVR_TrackedController _controller;

        [SerializeField] private float _offsetMovement=0.0006f;
        [SerializeField] private Color _defaultColor;
        
        [Header("Button Gameobjects")]
        public GameObject MainButton;
        public GameObject MenuButton;
        public GameObject PadButton;
        public GameObject GripButtonLeft;
        public GameObject GripButtonRight;

        
        private void Start()
        {

            _controller = LogiPen.Instance.Controller;

            if (_controller == null)
            {
                Debug.LogError("Could not find Logipen instance in the scene. Make sure " +
                               "LogiPen.cs is attached to a GameObject in the scene");
                return;
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

        }

        /// <summary>
        /// Change Color of material on target renderer
        /// Change both regular and emission color. 
        /// Can use the static methods outside of this class to change color of anything else 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="newColor"></param>
        public static void ChangeColor(Renderer target,Color newColor)
        {
            target.material.color = newColor;
            if (newColor != Color.black)
            {
                //if emission is turned off in the target.material setting nothing will happen at that line. 
                target.material.SetColor("_EmissionColor", newColor);
            }
        }

        /// <summary>
        /// Overload for Gameobject Parameter
        /// </summary>
        public static void ChangeColor(GameObject obj, Color newColor)
        {
            if (obj.GetComponent<Renderer>() == null)
            {
                Debug.LogError("The game object doens't have a renderer, can't change it's color !");
                return;
            }
            ChangeColor(obj.GetComponent<Renderer>(),newColor);
            
        }
        
        //Callbacks
        private void PadButtonReleasedAnimation(object sender, ClickedEventArgs e)
        {
            //Use Dotween plugin--PadButton.transform.DOLocalMoveY(0.01046997f, 0.1f);
            //StartCoroutine(DoLocalMoveY(PadButton, 0.01046997f, 0.1f)); uncomment for animation of buttons
            ChangeColor(PadButton,_defaultColor);
            
        }

        private void PadButtonClickedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveY(PadButton,-_offsetMovement, 0.1f,isRelative:true));
            ChangeColor(PadButton,LogiPen.Instance.GetPenColor());
        }

        private void MenuButtonReleasedAnimation(object sender, ClickedEventArgs e)
        {
          //  StartCoroutine(DoLocalMoveY(MenuButton,0.01046997f, 0.1f));
            ChangeColor(MenuButton,_defaultColor);
        }

        private void MenuButtonClickedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveY(MenuButton,-_offsetMovement, 0.1f,isRelative:true));
            ChangeColor(MenuButton,LogiPen.Instance.GetPenColor());
        }

        private void MainButtonReleasedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveY(MainButton, 0.01046997f, 0.1f));
            ChangeColor(MainButton,_defaultColor);
        }

        private void MainButtonClickedAnimation(object sender, ClickedEventArgs e)
        {
            //tartCoroutine(DoLocalMoveY(MainButton,-_offsetMovement, 0.1f,isRelative:true));
            ChangeColor(MainButton,LogiPen.Instance.GetPenColor());            
        }

        private void UngrippedClikedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveX(GripButton, -0.01007005f, 0.1f));
            ChangeColor(GripButtonLeft,_defaultColor);
            ChangeColor(GripButtonRight,_defaultColor);
        }

        private void GrippedClikedAnimation(object sender, ClickedEventArgs e)
        {
            //StartCoroutine(DoLocalMoveX(GripButton, _offsetMovement, 0.1f,isRelative:true));
            ChangeColor(GripButtonLeft,LogiPen.Instance.GetPenColor());
            ChangeColor(GripButtonRight,LogiPen.Instance.GetPenColor());
        }
        
        /// <summary>
        /// Example of Coroutine animation.
        /// </summary>
        /// <param name="obj">Gameobject to move</param>
        /// <param name="target">target position to move towards to</param>
        /// <param name="duration">duration of the animation</param>
        /// <param name="isRelative">relative means adding target to current transfrom as opposed
        /// to give an absolute value as a target for the animation</param>
        /// <returns></returns>
        IEnumerator DoLocalMove(GameObject obj,Vector3 target,float duration,bool isRelative = false)
        {
            float start=0;
            Vector3 startPositon;
            Vector3 targetPosition;
            if (isRelative)
            {
                startPositon = obj.transform.localPosition;
                targetPosition = startPositon + target;
            }
            else
            {
                startPositon = obj.transform.localPosition;
                targetPosition = target;
            }
            while (start < duration)
            {
                start += Time.deltaTime;
                var lerp = Mathf.Clamp01(start / duration);
                obj.transform.localPosition = Vector3.Lerp(startPositon, targetPosition, lerp);
                yield return null;
            }
        }

        IEnumerator DoLocalMoveY(GameObject obj,float offset,float duration,bool isRelative = false)
        {
            var startPositon = obj.transform.localPosition;
            var target = isRelative ? new Vector3(0,offset,0) : new Vector3(startPositon.x,offset,startPositon.z);
            return DoLocalMove(obj, target, duration,isRelative);
        }
        
        IEnumerator DoLocalMoveX(GameObject obj,float offset,float duration,bool isRelative = false)
        {
            var startPositon = obj.transform.localPosition;
            var target = isRelative ? new Vector3(offset,0,0) : new Vector3(offset,startPositon.y,startPositon.z);
            return DoLocalMove(obj, target, duration,isRelative);
        }
    }
}