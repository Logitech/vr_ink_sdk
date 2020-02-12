namespace Logitech.XRToolkit.Components
{
    using UnityEngine;

    public class MarkerMenuBackInteractable : MarkerMenuItem
    {
        private bool _isPositionReached = false;

        private void Update()
        {
            UIContainer.transform.position = Vector3.Lerp(UIContainer.transform.position, transform.position, Time.deltaTime * UISpeedFactor);
        }

        public override void ResetItemToDefaultState()
        {
            UIContainer.GetComponent<Renderer>().material.color = InitialColor;
        }

        private void OnEnable()
        {
            ResetItemToDefaultState();
        }

        public override void SetVisualStartingPoint(Vector3 startingPoint)
        {
            UIContainer.transform.position = startingPoint;
        }
    }
}
