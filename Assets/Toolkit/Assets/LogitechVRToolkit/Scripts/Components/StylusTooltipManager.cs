namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// A tooltip manager specific to the Stylus i.e., abstract has context about buttons etc.
    /// </summary>
    public class StylusTooltipManager : SingletonBehaviour<StylusTooltipManager>
    {
        [Header("Tooltips")]
        [SerializeField]
        private TooltipComponent _nibTooltip;
        [SerializeField]
        private string _nibString;
        [SerializeField]
        private TooltipComponent _primaryTooltip;
        [SerializeField]
        private string _primaryString;
        [SerializeField]
        private TooltipComponent _touchpadTooltip;
        [SerializeField]
        private string _touchpadString;
        [SerializeField]
        private TooltipComponent _menuTooltip;
        [SerializeField]
        private string _menuString;
        [SerializeField]
        private TooltipComponent _gripTooltip;
        [SerializeField]
        private string _gripString;

        [Header("Behaviour")]
        [SerializeField]
        private bool _shakeToDiscard = true;
        [SerializeField]
        private bool _shakeToEnable = true;
        [SerializeField, ShowIf("_shakeToDiscard")]
        private ShakeTrigger _shakeTrigger;
        [SerializeField]
        private FollowObjectAction _followAction;
        [SerializeField]
        private FaceObjectAction _faceObjectAction;

        public void ChangeTipTooltip(string newText)
        {
            _nibTooltip.SetText(newText);
            _nibTooltip.gameObject.SetActive(true);
        }
        public void ChangePrimaryTooltip(string newText)
        {
            _primaryTooltip.SetText(newText);
            _primaryTooltip.gameObject.SetActive(true);
        }
        public void ChangeTouchstripTooltip(string newText)
        {
            _touchpadTooltip.SetText(newText);
            _touchpadTooltip.gameObject.SetActive(true);
        }
        public void ChangeMenuTooltip(string newText)
        {
            _menuTooltip.SetText(newText);
            _menuTooltip.gameObject.SetActive(true);
        }
        public void ChangeGripTooltip(string newText)
        {
            _gripTooltip.SetText(newText);
            _gripTooltip.gameObject.SetActive(true);
        }

        private void Start()
        {
            ChangeTipTooltip(_nibString);
            ChangePrimaryTooltip(_primaryString);
            ChangeTouchstripTooltip(_touchpadString);
            ChangeMenuTooltip(_menuString);
            ChangeGripTooltip(_gripString);
        }

        private void Update()
        {
            _followAction.Update(true);
            _faceObjectAction.Update(true);

            if (_shakeTrigger.IsValid())
            {
                if (_shakeToDiscard && (_nibTooltip.isActiveAndEnabled || _nibTooltip.isActiveAndEnabled ||
                                        _nibTooltip.isActiveAndEnabled || _nibTooltip.isActiveAndEnabled ||
                                        _nibTooltip.isActiveAndEnabled))
                {
                    _nibTooltip.gameObject.SetActive(false);
                    _primaryTooltip.gameObject.SetActive(false);
                    _touchpadTooltip.gameObject.SetActive(false);
                    _menuTooltip.gameObject.SetActive(false);
                    _gripTooltip.gameObject.SetActive(false);
                    return;
                }

                if (_shakeToEnable)
                {
                    _nibTooltip.gameObject.SetActive(true);
                    _primaryTooltip.gameObject.SetActive(true);
                    _touchpadTooltip.gameObject.SetActive(true);
                    _menuTooltip.gameObject.SetActive(true);
                    _gripTooltip.gameObject.SetActive(true);
                }
            }
        }
    }
}
