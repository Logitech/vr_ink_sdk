namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Utils;
    using System.Linq;
    using UnityEngine;

    public enum EObjectToCenterTo
    {
        GivenObject,
        MiddleOfTwoObjects,
        CenterOfChildren
    }
    /// <summary>
    /// Will center the given object to a the chosen position, without moving the children in the process.
    /// </summary>
    public class CenterOnObject : MonoBehaviour
    {
        [SerializeField]
        private EObjectToCenterTo _centerTo;
        [SerializeField, ShowIf("_centerTo", EObjectToCenterTo.GivenObject)]
        private Transform _givenObject;
        [SerializeField, ShowIf("_centerTo", EObjectToCenterTo.MiddleOfTwoObjects)]
        private Transform _firstGivenObject;
        [SerializeField, ShowIf("_centerTo", EObjectToCenterTo.MiddleOfTwoObjects)]
        private Transform _secondGivenObject;

        private void Update()
        {
            var children = transform.Cast<Transform>().ToList();
            Vector3 childrenAverageCenter = Vector3.zero;
            foreach (Transform child in children)
            {
                child.parent = null;
                childrenAverageCenter += child.position;
            }

            switch (_centerTo)
            {
                case EObjectToCenterTo.GivenObject:
                    transform.position = _givenObject.position;
                    break;
                case EObjectToCenterTo.MiddleOfTwoObjects:
                    transform.position = (_firstGivenObject.position + _secondGivenObject.position) / 2;
                    break;
                case EObjectToCenterTo.CenterOfChildren:
                    transform.position = childrenAverageCenter / children.Count;
                    break;
            }

            foreach (Transform child in children)
            {
                child.parent = transform;
            }
        }
    }
}
