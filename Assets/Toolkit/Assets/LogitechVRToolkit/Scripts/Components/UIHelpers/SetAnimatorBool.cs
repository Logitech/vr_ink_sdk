namespace Logitech.XRToolkit.UIHelpers
{
    using UnityEngine;

    /// <summary>
    /// Set an Animator bool state.
    /// </summary>
    public class SetAnimatorBool : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private string _boolName;


        private void Reset()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetBool(bool state)
        {
            _animator.SetBool(_boolName, state);
        }

        public void ToggleBool()
        {
            _animator.SetBool(_boolName, !_animator.GetBool(_boolName));
        }
    }
}
