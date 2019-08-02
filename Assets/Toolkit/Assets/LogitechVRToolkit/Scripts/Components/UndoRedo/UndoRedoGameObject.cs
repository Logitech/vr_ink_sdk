namespace Logitech.XRToolkit.UndoRedo
{
    using UnityEngine;

    /// <summary>
    /// Enable, disable or remove this GameObject.
    /// </summary>
    public class UndoRedoGameObject : MonoBehaviour, IUndoRedo
    {
        public void Clear()
        {
            Destroy(this.gameObject);
        }

        public void Redo()
        {
            gameObject.SetActive(true);
        }

        public void Undo()
        {
            gameObject.SetActive(false);
        }

        void Start()
        {
            UndoRedoManager.Instance.RegisterNewAction(this);
        }
    }
}
