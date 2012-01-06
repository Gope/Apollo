using System;

namespace Commanding
{
    /// <summary>
    /// The command manager is the central class that handles commands
    /// </summary>
    public interface ICommandManager
    {
        #region Properties

        /// <summary>
        /// Currently running command (during an Undo or Redo process)
        /// </summary>
        /// <value>
        ///     <c>null</c> if no Undo or Redo is taking place
        /// </value>
        ICommand CurrentCommand { get; }

        #endregion Properties

        #region Events

        /// <summary>
        /// Listen to this event to be notified when a new command is added, executed, undone or redone
        /// </summary>
        event EventHandler CanUndoRedoChanged;

        #endregion Events

        #region Methods

        /// <summary>
        /// Clears this instance's command history (Reset undo/redo History)
        /// </summary>
        void Clear();

        /// <summary>
        /// Executes the specified command with its configuration
        /// </summary>
        /// <param name="configuration">The configuration for the execution.</param>
        void Do(CommandExecutionConfiguration configuration);

        /// <summary>
        /// To be called when a command was actively aborted (not via Exception).
        /// </summary>
        /// <param name="state">The state.</param>
        void CommandAborted(CompletedState state);

        /// <summary>
        /// Marshalls a call back to the main ui thread.
        /// </summary>
        /// <param name="action">The action.</param>
        void MarshallBack(Action action);

        #endregion Methods

        #region Undo / Redo

        /// <summary>
        /// Undoes the last command
        /// </summary>
        void Undo();

        /// <summary>
        /// Redoes the last undone command
        /// </summary>
        void Redo();

        /// <summary>
        /// Gets a value indicating whether last command can be undone
        /// </summary>
        /// <value>
        ///   <c>true</c> if the last command can be undone; otherwise, <c>false</c>.
        /// </value>
        bool CanUndo { get; }

        /// <summary>
        /// Gets a value indicating whether the last undone command can be redone
        /// </summary>
        /// <value>
        ///   <c>true</c> if the last undone command can be redone; otherwise, <c>false</c>.
        /// </value>
        bool CanRedo { get; }

        #endregion Undo / Redo

        #region Transactions

        /// <summary>
        /// Commits the topmost transaction from the stack.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Performs a rollback of the topmost transaction from the stack
        /// </summary>
        void RollBackTransaction();

        /// <summary>
        /// Adds an <see cref="ICommand"/> to an existing or new transaction and executes leaving the 
        /// transaction open for further commands.
        /// </summary>
        /// <param name="transactionalAction">The transactional action.</param>
        void RunWithinTransaction(Action<ICommandManager> transactionalAction);

        /// <summary>
        /// Runs multiple commands within transaction.
        /// </summary>
        /// <param name="transactionalAction">The transactional action.</param>
        void RunWithinTransactionAndCommit(Action<ICommandManager> transactionalAction);

        #endregion Transactions
    }
}
