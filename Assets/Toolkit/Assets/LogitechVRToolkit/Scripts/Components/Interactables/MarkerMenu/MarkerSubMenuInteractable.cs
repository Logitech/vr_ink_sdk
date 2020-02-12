namespace Logitech.XRToolkit.Components
{
    using System.Collections.Generic;

    using UnityEngine;

    public class MarkerSubMenuInteractable : MarkerMenuItem
    {
        [SerializeField]
        private List<MarkerMenuItem> _subLayerItems;

        private bool _isListProvided = false;

        private bool _isPositionReached = false;

        private void Start()
        {
            for (int i = 0; i < _subLayerItems.Count; i++)
            {
                _subLayerItems[i].gameObject.SetActive(false);
            }
        }

        public override void ResetItemToDefaultState()
        {
            UIContainer.GetComponent<Renderer>().material.color = InitialColor;
        }

        private void Awake()
        {
            for (int i = 0; i < _subLayerItems.Count; i++)
            {
                _subLayerItems[i].gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            ResetItemToDefaultState();
            _isListProvided = false;
        }

        private void Update()
        {
            UIContainer.transform.position = Vector3.Lerp(UIContainer.transform.position, transform.position, Time.deltaTime * UISpeedFactor);
        }

        public bool IsListProvided()
        {
            return _isListProvided;
        }

        public List<MarkerMenuItem> GetSubItems()
        {
            _isListProvided = true;
            return _subLayerItems;
        }

        public void DisplaySubElement(Vector3 centerOffset)
        {
            for (int i = 0; i < _subLayerItems.Count; i++)
            {
                _subLayerItems[i].gameObject.SetActive(true);
                _subLayerItems[i].SetVisualStartingPoint(transform.position + centerOffset);
            }
        }

        public override void SetVisualStartingPoint(Vector3 startingPoint)
        {
            UIContainer.transform.position = startingPoint;
        }
    }
}
