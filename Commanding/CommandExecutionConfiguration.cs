using System;
using System.Diagnostics;

namespace Commanding
{
    /// <summary>
    /// Configuration for the execution of a single ICommand.
    /// </summary>
    [DebuggerStepThrough]
    public class CommandExecutionConfiguration
    {
        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionConfiguration"/> class.
        /// </summary>
        public CommandExecutionConfiguration()
        {
            RecordCommandWithHistoryForUndoRedo = true;
            RunInBackground = false;
        }

        #endregion

        #region Properties 

        /// <summary>
        /// Gets or sets the command manager associated with the Command
        /// </summary>
        /// <value>
        /// The command manager.
        /// </value>
        internal ICommandManager CommandManager { get; set; }

        /// <summary>
        /// Gets or sets the executing ICommand.
        /// </summary>
        /// <value>
        /// The executing ICommand.
        /// </value>
        internal ICommand ExecutingCommand { get; set; }

        /// <summary>
        /// Gets or sets the callback to use with the Completed delegate.
        /// </summary>
        /// <value>
        /// The callback.
        /// </value>
        internal Action<CompletedState> Callback { get; set; }

        /// <summary>
        /// Gets or sets the action to execute when progress gets reported.
        /// </summary>
        /// <value>
        /// The progress action.
        /// </value>
        internal Action<ProgressInfo> ProgressAction { get; set; }

        /// <summary>
        /// Gets or sets the callback to use after calling undo.
        /// </summary>
        /// <value>
        /// The undo callback.
        /// </value>
        protected Action<CompletedState> UndoCallback { get; set; }

        /// <summary>
        /// Gets or sets the state of the completed operation.
        /// </summary>
        /// <value>
        /// The state of the completed.
        /// </value>
        internal CompletedState CompletedState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this command will be available within 
        /// the CommandManager's History entries. If set to false there will be no UNDO 
        /// operation available, but it's Fire and Forget.
        /// </summary>
        /// <value>
        /// <c>true</c> if command should be recorded for UNDO, else false.
        /// </value>
        internal bool RecordCommandWithHistoryForUndoRedo { get; set; }

        /// <summary>
        /// Determines wether a command should be run in a separate thread. All thread-marshalling 
        /// is taken care of by the infrastructure.
        /// </summary>
        /// <value>
        /// <c>true</c> if command should be in a background thread, else false.
        /// </value>
        internal bool RunInBackground { get; set; }

        #endregion

        #region Fluent API

        /// <summary>
        /// Sets a delegate to execute after the operation has finished.
        /// </summary>
        /// <param name="actionToCallWhenUndoOrRedoCompleted">The action to call when undo or redo completed.</param>
        /// <returns>The configuration for execution.</returns>
        [DebuggerStepThrough]
        public CommandExecutionConfiguration AndWhenCompletedCall(Action<CompletedState> actionToCallWhenUndoOrRedoCompleted)
        {
            if (Callback != null)
            {
                string message = string.Format(
                "CommandManager Configuration Error! The Callback for this CommandExecution "
                + "was already set somewhere else. It cannot be overwritten to help finding "
                + "unintended assignments for callbacks.\r\nAttached Callback is: {0}", this.Callback.Method);

                Debug.Assert(Callback != null, message);
            }

            Callback = actionToCallWhenUndoOrRedoCompleted;

            return this;
        }

        /// <summary>
        /// Disables Undo Support for this command. It will be Fire and Forget.
        /// </summary>
        /// <returns>The configuration for execution.</returns>
        [DebuggerStepThrough]
        public CommandExecutionConfiguration WithoutUndoRedo()
        {
            RecordCommandWithHistoryForUndoRedo = false;

            return this;
        }

        /// <summary>
        /// Runs the command in a separate background thread while taking care 
        /// of the marshalling.
        /// </summary>
        /// <returns>The configuration for execution.</returns>
        [DebuggerStepThrough]
        public CommandExecutionConfiguration RunCommandInBackground()
        {
            RunInBackground = true;

            return this;
        }

        /// <summary>
        /// Called when a progress update comes in.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>The configuration for execution.</returns>
        [DebuggerStepThrough]
        public CommandExecutionConfiguration OnProgressUpdatesDo(Action<ProgressInfo> action)
        {
            if (ProgressAction != null)
            {
                string message = string.Format(
                "CommandManager Configuration Error! The ProgressUpdate delegate for this CommandExecution "
                + "was already set somewhere else. It cannot be overwritten to help finding "
                + "unintended assignments for callbacks.\r\nAttached delegate is is: {0}", this.ProgressAction.Method);

                Debug.Assert(ProgressAction != null, message);
            }

            ProgressAction = action;

            return this;
        }

        #endregion
    }
}