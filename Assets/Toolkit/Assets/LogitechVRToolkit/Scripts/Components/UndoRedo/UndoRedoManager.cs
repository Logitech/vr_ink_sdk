namespace Logitech.XRToolkit.UndoRedo
{
    using Logitech.XRToolkit.Utils;
    using Logitech.XRToolkit.Triggers;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Handles the undo, redo and clear actions for registered classes.
    /// </summary>
    public class UndoRedoManager : SingletonBehaviour<UndoRedoManager>
    {
        [SerializeField]
        private InputTrigger _undoTrigger;
        [SerializeField]
        private InputTrigger _redoTrigger;
        [SerializeField]
        private InputTrigger _undoAllTrigger;

        private Stack<IUndoRedo> _undoStack, _redoStack;

        /// <summary>
        /// Register a new action to be undone.
        /// </summary>
        public void RegisterNewAction(IUndoRedo action)
        {
            _undoStack.Push(action);
            ClearStack(_redoStack);
        }

        /// <summary>
        /// Undo the last action, if any.
        /// </summary>
        public bool Undo()
        {
            if (_undoStack.Count == 0)
            {
                return false;
            }

            IUndoRedo action = _undoStack.Pop();
            action.Undo();
            _redoStack.Push(action);

            return true;
        }

        /// <summary>
        /// Redo the last undone action, if any.
        /// </summary>
        public bool Redo()
        {
            if (_redoStack.Count == 0)
            {
                return false;
            }

            IUndoRedo action = _redoStack.Pop();
            action.Redo();
            _undoStack.Push(action);

            return true;
        }

        /// <summary>
        /// Clear all actions from undo and redo stacks. Use this when loading a new scene for instance.
        /// </summary>
        public void Clear()
        {
            ClearStack(_redoStack);
            ClearStack(_undoStack);
        }

        private void ClearStack(Stack<IUndoRedo> stack)
        {
            while (stack.Count != 0)
            {
                var action = stack.Pop();
                action.Clear();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _undoStack = new Stack<IUndoRedo>();
            _redoStack = new Stack<IUndoRedo>();
        }

        private void Update()
        {
            if (_undoTrigger.IsValid())
            {
                Undo();
            }
            else if (_redoTrigger.IsValid())
            {
                Redo();
            }
            else if (_undoAllTrigger.IsValid())
            {
                bool undoAll;
                do
                {
                    undoAll = Undo();
                }
                while (undoAll);
            }
        }
    }
}
