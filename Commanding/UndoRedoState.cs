namespace Commanding
{
    /// <summary>
    /// The kind of action (undo or redo).
    /// </summary>
    public enum UndoRedoState
    {
        /// <summary>
        /// Used when an action is undone.
        /// </summary>
        Undo,

        /// <summary>
        /// Used when an action is redone or executed for the first time.
        /// </summary>
        Redo
    }
}