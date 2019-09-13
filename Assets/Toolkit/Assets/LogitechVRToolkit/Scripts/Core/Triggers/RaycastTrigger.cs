namespace Logitech.XRToolkit.Triggers
{
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Utils;
    using System;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Trigger that is valid when a specified raycast hits a target that meets a set of conditions.
    /// </summary>
    [Serializable]
    public class RaycastTrigger : Trigger
    {
        public Transform Source;
        public EInteractable Tag = EInteractable.Untagged;
        public float MaxDistance = 0.4f;
        [Tooltip("The forward offset of the raycast from source origin.")]
        public float Offset = -0.1f;
        public RaycastHit RaycastHit { get; private set; }

        [SerializeField]
        private bool _useRaycastAll = true;

        [HideInInspector]
        public Ray RaycastRay;

        public RaycastTrigger() { }

        public RaycastTrigger(Transform source, EInteractable tag, float maxDistance, float offset)
        {
            Source = source;
            Tag = tag;
            MaxDistance = maxDistance;
            Offset = offset;
        }

        public override bool IsValid()
        {
            if (_useRaycastAll)
            {
                return RaycastAll();
            }

            return SingleRaycast();
        }

        private bool RaycastAll()
        {
            RaycastHit[] hits;
            if (RaycastRay.origin != Vector3.zero)
            {
                hits = Physics.RaycastAll(RaycastRay, MaxDistance).OrderBy(h => h.distance).ToArray();
            }
            else
            {
                Vector3 position = Source.position + Source.forward * Offset;
                hits = Physics.RaycastAll(position, Source.forward, MaxDistance).OrderBy(h => h.distance).ToArray();
            }

            foreach (var hit in hits)
            {
                var interactable = hit.transform.GetComponent<Interactable>();
                if (interactable == null)
                {
                    continue;
                }

                if (interactable.ContainsTag(Tag))
                {
                    RaycastHit = hit;
                    return true;
                }
            }

            return false;
        }

        private bool SingleRaycast()
        {
            RaycastHit hit;
            bool isHitting;
            if (RaycastRay.origin != Vector3.zero)
            {
                isHitting = Physics.Raycast(RaycastRay, out hit, MaxDistance);
            }
            else
            {
                Vector3 position = Source.position + Source.forward * Offset;
                isHitting = Physics.Raycast(position, Source.forward, out hit, MaxDistance);
            }

            if (!isHitting)
            {
                return false;
            }

            var interactable = hit.transform.GetComponent<Interactable>();
            if (interactable == null)
            {
                return false;
            }

            if (interactable.ContainsTag(Tag))
            {
                RaycastHit = hit;
                return true;
            }

            return false;
        }
    }
}
