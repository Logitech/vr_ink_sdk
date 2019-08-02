namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// An object that can be interacted with.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [EnumFlag]
        public EInteractable Tags;

        public bool ContainsTag(EInteractable tag)
        {
            return Tags.HasFlag(tag);
        }
    }
}
