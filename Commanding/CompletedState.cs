using System;

namespace Commanding
{
    /// <summary>
    /// Encapsulates information about the execution of an <see cref="ICommand"/>. E.g. wether command was
    /// executed successfully, failed with an exception or was aborted
    /// </summary>
    public class CompletedState
    {
        #region Member 

        private WeakReference command;

        #endregion

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedState"/> class.
        /// </summary>
        private CompletedState()
        {
            UndoRedoState = UndoRedoState.Redo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedState"/> class.
        /// </summary>
        /// <param name="command">The command associated with this state.</param>
        public CompletedState(ICommand command)
            : this()
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            Command = command;
        }

        #endregion

        #region Properties 

        /// <summary>
        /// Gets and sets value indicating wether command execution was aborted
        /// </summary>
        /// <value>
        ///     <c>true</c> if command execution was aborted; otherwise <c>false</c>
        /// </value>
        public bool Aborted { get; set; }

        /// <summary>
        /// Gets and sets an error that occured during command execution
        /// </summary>
        /// <value>
        ///     <c>null</c> if no error occured during execution of the command
        /// </value>
        public Exception Error { get; set; }

        /// <summary>
        /// Gets or sets the operation kind (undo or redo (execute)) for this operation.
        /// </summary>
        /// <value>
        /// The operation kind of this CompletedState.
        /// </value>
        public UndoRedoState UndoRedoState { get; set; }

        /// <summary>
        /// Gets the associated command
        /// </summary>
        /// <value>
        /// The associated command
        /// </value>
        public ICommand Command
        {
            get
            {
                if (command.IsAlive)
                {
                    return (ICommand) command.Target;
                }

                return null;
            }

            private set
            {
                command = new WeakReference(value);
            }
        }

        /// <summary>
        /// Gets or sets a possible error message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        #endregion
    }
}