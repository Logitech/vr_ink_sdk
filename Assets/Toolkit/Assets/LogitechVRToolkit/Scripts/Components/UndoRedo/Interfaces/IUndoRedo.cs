namespace Logitech.XRToolkit.UndoRedo
{
    /// <summary>
    /// Undo, redo or clear an action or state.
    /// </summary>
    public interface IUndoRedo
    {
        void Undo();
        void Redo();
        void Clear();
    }
}
