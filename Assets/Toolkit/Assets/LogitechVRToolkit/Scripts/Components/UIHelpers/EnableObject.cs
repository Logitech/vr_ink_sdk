namespace Logitech.XRToolkit.UIHelpers
{
    using UnityEngine;

    /// <summary>
    /// Enable an object.
    /// </summary>
    public class EnableObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject _target;

        public void Toggle()
        {
            _target.SetActive(!_target.activeSelf);
        }
    }
}
