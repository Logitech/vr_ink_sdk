namespace Logitech.XRToolkit.Components
{
    using System.Collections.Generic;

    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;

    using UnityEngine;

    /// <summary>
    /// Display a contextual marker menu while the input trigger is pressed. Then handle the selection of the menu item depending on where the pointing device point at.

    /// </summary>
    public class MarkerMenuInteraction : MonoBehaviour
    {
        [SerializeField]
        private InputTrigger _activeMenuTrigger;

        [SerializeField]
        private TrackedDeviceProvider _pointingDevice;

        [Tooltip("The array of all marker menu items you will be able to interact with on the 1st layer. The items in a sub menu are to be put in the sub menu item list.")]
        [SerializeField]
        private List<MarkerMenuItem> _layerOneMenuObjects;

        private List<MarkerMenuItem> _activeMenuObjects;

        private List<List<MarkerMenuItem>> _previousActiveObjects;

        [Tooltip("The distance in meter forward to the chosen controller where the marker menu will be displayed.")]
        [SerializeField]
        private float _distanceToMenu = 0.15f;

        private float _initialUIdistance = 0.01f;

        private Vector3 _uiTransitionOffset = Vector3.zero;

        [Tooltip("The color to assign to the current selected marker menu item.")]
        [SerializeField]
        private Color _highlightedColor;

        private Transform _initialSelectionPose;
        private Plane _menuPlane;

        private bool _isDesactived = false;
        private int _activeItem = -1;
        private int _activeSubMenu = -1;

        private void Start()
        {
            _activeMenuObjects = new List<MarkerMenuItem>();
            _previousActiveObjects = new List<List<MarkerMenuItem>>();
            foreach (MarkerMenuItem layerOneItem in _layerOneMenuObjects)
            {
                _activeMenuObjects.Add(layerOneItem);
            }
        }

        private void Update()
        {
            if (_activeMenuTrigger.IsValid())
            {
                if (_isDesactived)
                {
                    _initialSelectionPose = _pointingDevice.GetOutput().transform;
                    transform.position = _pointingDevice.GetOutput().transform.position;
                    float rotX = _pointingDevice.GetOutput().transform.rotation.eulerAngles.x;
                    float rotY = _pointingDevice.GetOutput().transform.rotation.eulerAngles.y;
                    transform.rotation = Quaternion.Euler(rotX, rotY, 0.0f);
                    transform.position += _pointingDevice.GetOutput().transform.forward * _distanceToMenu;

                    _menuPlane = new Plane(_pointingDevice.GetOutput().transform.forward, transform.position);

                    // The UI will appears just behind the initial center pose.
                    Vector3 visualUIInitialPos = transform.position;
                    _uiTransitionOffset = _pointingDevice.GetOutput().transform.forward * _initialUIdistance;
                    visualUIInitialPos += _uiTransitionOffset;

                    for (int i = 0; i < _activeMenuObjects.Count; i++)
                    {
                        _activeMenuObjects[i].gameObject.SetActive(true);
                        _activeMenuObjects[i].SetVisualStartingPoint(visualUIInitialPos);
                    }
                    _isDesactived = false;
                }
                else
                {
                    int hitIndex = GetClosestItem(_activeMenuObjects);
                    if (hitIndex != -1)
                    {
                        NewActiveItem(hitIndex);
                    }
                }
            }
            else if (!_isDesactived)
            {
                if (_activeItem != -1 && _activeMenuObjects[_activeItem].GetType() == typeof(MarkerMenuInteractable))
                {
                    var endItem = _activeMenuObjects[_activeItem] as MarkerMenuInteractable;
                    endItem.OnMarkerMenuRelease.Invoke();
                }
                for (int i = 0; i < _activeMenuObjects.Count; i++)
                {
                    _activeMenuObjects[i].GetComponentInChildren<Transform>().gameObject.SetActive(false);
                    _activeMenuObjects[i].gameObject.SetActive(false);
                }
                _isDesactived = true;
                _activeItem = -1;
                _activeMenuObjects.Clear();
                _activeMenuObjects.AddRange(_layerOneMenuObjects);
                _previousActiveObjects = new List<List<MarkerMenuItem>>();
            }
        }

        public int GetClosestItem(List<MarkerMenuItem> menuItems)
        {
            int result = -1;
            float enter = 0.0f;
            var ray = new Ray(_pointingDevice.GetOutput().transform.position, _pointingDevice.GetOutput().transform.forward);
            if (_menuPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                float minimalDist = 999.0f;
                int hitIndex = -1;
                for (int i = 0; i < _activeMenuObjects.Count; i++)
                {
                    float dist = _activeMenuObjects[i].GetType() == typeof(MarkerMenuSliderInteractable) && _activeItem == i
                        ? GetDistFromSlider(_activeMenuObjects[i] as MarkerMenuSliderInteractable, hitPoint)
                        : Vector3.Distance(_activeMenuObjects[i].transform.position, hitPoint);

                    if (minimalDist > dist)
                    {
                        minimalDist = dist;
                        hitIndex = i;
                    }
                }
                result = hitIndex;
                if (_activeMenuObjects[hitIndex].GetType() == typeof(MarkerMenuSliderInteractable))
                {
                    var sliderItem = _activeMenuObjects[hitIndex] as MarkerMenuSliderInteractable;
                    sliderItem.SetHitPlanCoordinate(hitPoint);
                }
            }
            return result;
        }

        private void NewActiveItem(int itemIndex)
        {
            if (_activeItem == itemIndex)
            {
                return;
            }
            _pointingDevice.GetOutput().SendHapticPulse(0.0f, 0.01f, 320f, 0.35f);
            if (_activeItem != -1 && _activeItem != itemIndex)
            {
                _activeMenuObjects[_activeItem].ResetItemToDefaultState();
            }
            _activeItem = itemIndex;
            _activeMenuObjects[_activeItem].UIContainer.GetComponent<Renderer>().material.color = _highlightedColor;

            if (_activeMenuObjects[_activeItem].GetType() == typeof(MarkerSubMenuInteractable))
            {
                var subMenu = _activeMenuObjects[_activeItem] as MarkerSubMenuInteractable;
                if (!subMenu.IsListProvided())
                {
                    for (int i = 0; i < _activeMenuObjects.Count; i++)
                    {
                        if (i != _activeItem)
                        {
                            _activeMenuObjects[i].gameObject.SetActive(false);
                            _activeMenuObjects[i].SetVisualStartingPoint(transform.position + _uiTransitionOffset);
                        }
                    }
                    _activeMenuObjects.Clear();
                    _activeMenuObjects.Add(subMenu);
                    _activeItem = 0;
                    _activeMenuObjects.AddRange(subMenu.GetSubItems());
                    subMenu.DisplaySubElement(_uiTransitionOffset);
                    _previousActiveObjects.Add(_activeMenuObjects);
                }
            }
            else if (_activeMenuObjects[_activeItem].GetType() == typeof(MarkerMenuBackInteractable))
            {
                int subLayerNumber = _previousActiveObjects.Count;
                if (subLayerNumber == 1)
                {
                    for (int i = 0; i < _activeMenuObjects.Count; i++)
                    {
                        _activeMenuObjects[i].gameObject.SetActive(false);
                    }
                    _previousActiveObjects.Clear();
                    _activeMenuObjects.Clear();
                    _activeMenuObjects.AddRange(_layerOneMenuObjects);
                    _activeItem = -1;
                    for (int i = 0; i < _activeMenuObjects.Count; i++)
                    {
                        _activeMenuObjects[i].gameObject.SetActive(true);
                        _activeMenuObjects[i].SetVisualStartingPoint(transform.position + _uiTransitionOffset);
                    }
                }
                else if (subLayerNumber > 1)
                {
                    for (int i = 0; i < _activeMenuObjects.Count; i++)
                    {
                        _activeMenuObjects[i].gameObject.SetActive(false);
                    }
                    _activeMenuObjects.Clear();
                    _activeMenuObjects.AddRange(_previousActiveObjects[subLayerNumber - 1]);
                    _previousActiveObjects.RemoveAt(subLayerNumber - 1);
                    _activeItem = -1;
                    Vector3 initialTransitionPose = _activeMenuObjects[0].transform.position;
                    for (int i = 0; i < _activeMenuObjects.Count; i++)
                    {
                        _activeMenuObjects[i].gameObject.SetActive(true);
                        _activeMenuObjects[i].SetVisualStartingPoint(initialTransitionPose + _uiTransitionOffset);
                    }
                    MarkerMenuItem centerElemTransform = _activeMenuObjects[0];
                }
            }
            else if (_activeMenuObjects[_activeItem].GetType() == typeof(MarkerMenuInteractable))
            {
                var endItem = _activeMenuObjects[_activeItem] as MarkerMenuInteractable;
                endItem.SetAsActiveItem();
            }
            else if (_activeMenuObjects[_activeItem].GetType() == typeof(MarkerMenuSliderInteractable))
            {
                var endItem = _activeMenuObjects[_activeItem] as MarkerMenuSliderInteractable;
                endItem.SetAsActiveItem();
            }
        }

        private float GetDistFromSlider(MarkerMenuSliderInteractable sliderItem, Vector3 hitPoint)
        {
            Vector3 closestPoint;
            if (sliderItem.UIContainer.GetComponent<Renderer>() != null)
            {
                closestPoint = sliderItem.UIContainer.GetComponent<Renderer>().bounds.ClosestPoint(hitPoint);
            }
            else if (sliderItem.UIContainer.GetComponent<SpriteRenderer>() != null)
            {
                closestPoint = sliderItem.UIContainer.GetComponent<SpriteRenderer>().bounds.ClosestPoint(hitPoint);
            }
            else
            {
                return 999.0f;
            }

            return Vector3.Distance(closestPoint, hitPoint);
        }
    }
}
