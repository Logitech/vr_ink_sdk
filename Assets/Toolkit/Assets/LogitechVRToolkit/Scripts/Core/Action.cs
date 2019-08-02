namespace Logitech.XRToolkit.Core
{
    using Actions;
    using System;

    /// <summary>
    /// Actions may be used to perform simple events, and can be expected to execute while the input (typically a
    /// Trigger) is true.
    /// </summary>
    /// <remarks>
    /// It is expected that Update is called each frame to ensure all states occur. Otherwise, TriggerOnce may be used
    /// but may not be supported well by all actions. See <see cref="AirDrawingAction"/>, which should be continuously
    /// true to keep drawing.
    /// </remarks>
    /// <seealso cref="Trigger"/>
    /// <seealso cref="Provider"/>
    [Serializable]
    public abstract class Action
    {
        private bool _risingEdge;

        public void Update(bool isTriggerValid)
        {
            if (isTriggerValid)
            {
                if (!_risingEdge)
                {
                    OnTriggerValid();
                    _risingEdge = true;
                }

                TriggerValid();
            }
            else
            {
                if (_risingEdge)
                {
                    OnTriggerInvalid();
                    _risingEdge = false;
                }

                TriggerInvalid();
            }
        }

        public void TriggerOnce()
        {
            OnTriggerValid();
            TriggerValid();
            OnTriggerInvalid();
            TriggerInvalid();
        }

        /// <summary>
        /// Called on the rising edge of the trigger's validity.
        /// </summary>
        /// <remarks>
        /// Generally used for initialization.
        /// </remarks>
        protected virtual void OnTriggerValid() { }
        /// <summary>
        /// Called every frame that the trigger is valid.
        /// </summary>
        protected abstract void TriggerValid();
        /// <summary>
        /// Called on the falling edge of the trigger's validity.
        /// </summary>
        /// <remarks>
        /// Generally used for resetting variables or destroying objects.
        /// </remarks>
        protected virtual void OnTriggerInvalid() { }
        /// <summary>
        /// Called every frame that the trigger is invalid.
        /// </summary>
        protected virtual void TriggerInvalid() { }
    }
}
