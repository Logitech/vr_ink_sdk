namespace Logitech.XRToolkit.Components
{
    using UnityEngine;

    abstract public class MarkerMenuItem : MonoBehaviour
    {
        [SerializeField]
        public GameObject UIContainer;

        [SerializeField]
        public Color InitialColor;

        [SerializeField]
        public float UISpeedFactor = 10f;

        public abstract void ResetItemToDefaultState();

        public abstract void SetVisualStartingPoint(Vector3 startingPoint);
    }
}
