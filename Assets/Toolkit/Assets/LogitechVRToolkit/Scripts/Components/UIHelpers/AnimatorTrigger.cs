namespace Logitech.XRToolkit.UIHelpers
{
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Trigger an Animator animation.
    /// </summary>
    public class AnimatorTrigger : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private string _triggerName;

        /// <summary>
        /// Block trigger when hovering a UIInteractable.
        /// </summary>
        [SerializeField, Label("Block While OnHover")]
        private bool _blockOnUIEvent;
        [SerializeField, ShowIf("_blockOnUIEvent")]
        private UIInteractable _interactable;

        private void Reset()
        {
            _animator = GetComponent<Animator>();
            _interactable = GetComponent<UIInteractable>();
        }

        public void Trigger()
        {
            if (_blockOnUIEvent)
            {
                if (_interactable != null && !_interactable.OnHover)
                {
                    _animator.SetTrigger(_triggerName);
                }
            }

            _animator.SetTrigger(_triggerName);
        }
    }
}
