namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Triggers;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Enables or disables a GameObject based on the state of another script's specified property or field.
    /// </summary>
    public class SetGameObjectActiveConditional : MonoBehaviour
    {
        public GameObject[] Targets;
        public bool SetActive = true;
        public PropertyStateTrigger[] PropertyStateTriggers;

        private void Update()
        {
            if (PropertyStateTriggers.All(x => x.IsValid()))
            {
                foreach (GameObject target in Targets)
                {
                    target.SetActive(SetActive);
                }
            }
            else
            {
                foreach (GameObject target in Targets)
                {
                    target.SetActive(!SetActive);
                }
            }
        }
    }
}
