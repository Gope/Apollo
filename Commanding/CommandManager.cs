using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Commanding.EventAggregator;

namespace Commanding
{
    /// <summary>
    /// A custom CommandManager to execute <see cref="ICommand"/>s with Undo / Redo support.
    /// </summary>
    [DebuggerStepThrough]
    public class CommandManager : ICommandManager
    {
        #region Members

        private bool autocommit = true;
        private bool unboundedTransactionRunning = false;
        private bool unboundedTransactionCall = false;
        private Stack<TransactionalCommand> undoStack = new Stack<TransactionalCommand>();
        private Stack<TransactionalCommand> redoStack = new Stack<TransactionalCommand>();
        private SynchronizationContext syncContext;

        private bool commandRunning = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Prosim.Cargo.Planning.Command.CmdManager"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        public CommandManager(IEventAggregator eventAggregator, SynchronizationContext synchronizationContext)
        {
            this.EventAggregator = eventAggregator;
            this.syncContext = synchronizationContext;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Currently running command (during an Undo or Redo process)
        /// </summary>
        /// <value>
        ///   <c>null</c> if no Undo or Redo is taking place
        /// </value>
        public ICommand CurrentCommand
        {
            get
            {
                return this.UndoStack.Peek();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEventAggregator"/> for this CommandManager.
        /// </summary>
        /// <value>
        /// The Event Aggregator.
        /// </value>
        protected IEventAggregator EventAggregator { get; set; }

        private TransactionalCommand currentTransaction;

        /// <summary>
        /// Gets or sets the current transaction.
        /// </summary>
        /// <value>
        /// The current transaction.
        /// </value>
        protected internal TransactionalCommand CurrentTransaction
        {
            get
            {
                return this.currentTransaction;
            }

            set
            {
                this.currentTransaction = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether last command can be undone
        /// </summary>
        /// <value>
        /// <c>true</c> if the last command can be undone; otherwise, <c>false</c>.
        /// </value>
        public bool CanUndo
        {
            get
            {
                return this.UndoStack.Count > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the last undone command can be redone
        /// </summary>
        /// <value>
        /// <c>true</c> if the last undone command can be redone; otherwise, <c>false</c>.
        /// </value>
        public bool CanRedo
        {
            get
            {
                return RedoStack.Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets the UndoStack containing all undoable commands.
        /// </summary>
        /// <value>
        /// The undo stack.
        /// </value>
        protected internal Stack<TransactionalCommand> UndoStack
        {
            get
            {
                return this.undoStack;
            }

            set
            {
                this.undoStack = value;
            }
        }

        /// <summary>
        /// Gets or sets the RedoStack containing all redoable commands.
        /// </summary>
        /// <value>
        /// The redo stack.
        /// </value>
        protected internal Stack<TransactionalCommand> RedoStack
        {
            get
            {
                return this.redoStack;
            }

            set
            {
                this.redoStack = value;
            }
        }

        #endregion

        #region Publics

        /// <summary>
        /// Clears this instance's command history (Reset undo/redo History)
        /// </summary>
        public void Clear()
        {
            this.UndoStack.Clear();
            this.RedoStack.Clear();
        }

        /// <summary>
        /// Executes the specified Command with the given configuration.
        /// </summary>
        /// <param name="config">The configuration for the command.</param>
        [DebuggerStepThrough]
        private void Execute(CommandExecutionConfiguration config)
        {
            config.ExecutingCommand.EventAggregator = EventAggregator;
            var abstractCommand = config.ExecutingCommand as AbstractCommand;
            if (abstractCommand != null)
            {
                abstractCommand.CommandManager = this;
                abstractCommand.SynchronizationContext = this.syncContext;
            }

            if (config.Callback != null)
            {
                config.ExecutingCommand.CompletedHandler = config.Callback;
            }

            // choose wether to record the command or just fire it.
            if (config.RecordCommandWithHistoryForUndoRedo)
            {
                RecordCommand(config.ExecutingCommand as AbstractCommand);
            }
            else if (config.ExecutingCommand.CanExecute())
            {
                if (config.RunInBackground)
                {
                    this.ExecuteInSpawnedThread(config.ExecutingCommand as AbstractCommand);
                }
                else
                {
                    config.ExecutingCommand.Execute();
                }
            }
        }

        /// <summary>
        /// Central method to add and execute a new command.
        /// </summary>
        /// <param name="command">An command to be recorded in the buffer and executed</param>
        [DebuggerStepThrough]
        public void RecordCommand(AbstractCommand command)
        {
            if (command == null)
            {
                throw new ArgumentException("Command needs to be of type AbstractCommand", "command");
            }

            if (unboundedTransactionRunning && !unboundedTransactionCall)
            {
                throw new InvalidOperationException("An unbound transaction is currently running. Before executing a regular command you need to either commit or rollback the " +
                                                     "current transaction. Also you can add more commands to the current transaction via CommandManager.RunWithinTransaction().");
            }

            if (CurrentTransaction == null)
            {
                CurrentTransaction = new TransactionalCommand();
                this.UndoStack.Push(CurrentTransaction);
            }

            CurrentTransaction.AddCommand(command);

            if (command.ExecutionConfiguration.RunInBackground)
            {
                this.ExecuteInSpawnedThread(command);
            }
            else
            {
                command.Execute();
            }
        }

        /// <summary>
        /// Executes the specified command with its configuration
        /// </summary>
        /// <param name="configuration">The configuration for the execution.</param>
        [DebuggerStepThrough]
        public void Do(CommandExecutionConfiguration configuration)
        {
            RunCommandTillEndWith(() =>
            {
                RedoStack.Clear();

                Execute(configuration);
                if (autocommit)
                {
                    this.CommitTransaction();
                }

                RaiseCanUndoRedoChanged(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Undoes the last command
        /// </summary>
        public void Undo()
        {
            RunCommandTillEndWith(() =>
            {
                CurrentTransaction = this.UndoStack.Pop();

                if (this.CurrentTransaction.ExecutionConfiguration != null && this.CurrentTransaction.ExecutionConfiguration.RunInBackground)
                {
                    UnExecuteInSpawnedThread(CurrentTransaction);
                }
                else
                {
                    CurrentTransaction.UnExecute();
                }

                RedoStack.Push(CurrentTransaction);

                if (autocommit)
                {
                    this.CommitTransaction();
                }

                RaiseCanUndoRedoChanged(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Redoes the last undone command
        /// </summary>
        public void Redo()
        {
            RunCommandTillEndWith(() =>
            {
                var cmd = this.RedoStack.Pop();
                this.UndoStack.Push(cmd);
                cmd.Execute();

                RaiseCanUndoRedoChanged(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Commits the topmost transaction from the stack.
        /// </summary>
        public void CommitTransaction()
        {
            CurrentTransaction = null;
            this.unboundedTransactionRunning = false;
        }

        /// <summary>
        /// Performs a rollback of the topmost transaction from the stack
        /// </summary>
        public void RollBackTransaction()
        {
            this.unboundedTransactionRunning = false;

            if (this.CurrentTransaction != null)
            {
                if (this.UndoStack.Count > 0)
                {
                    this.UndoStack.Pop();
                }
                else
                {
                    // this should not happen!
                }

                CurrentTransaction.UnExecute();
            }
        }

        /// <summary>
        /// Runs multiple commands within an transaction.
        /// </summary>
        /// <param name="transactionalAction">The transactional action.</param>
        public void RunWithinTransactionAndCommit(Action<ICommandManager> transactionalAction)
        {
            this.RunWithinTransaction(transactionalAction);
            this.CommitTransaction();
        }

        /// <summary>
        /// Marshalls a call back to the main ui thread.
        /// </summary>
        /// <param name="action">The action.</param>
        public void MarshallBack(Action action)
        {
            if (this.syncContext != null)
            {
                this.syncContext.Send(oo => action(), null);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// To be called when a command was actively aborted (not via Exception).
        /// </summary>
        /// <param name="state">The state.</param>
        public void CommandAborted(CompletedState state)
        {
            var found = this.UndoStack.Any(command => command.Commands.Any(abstractCommand => abstractCommand == state.Command as AbstractCommand));
            if (found)
            {
                // loose the aborted command
                this.UndoStack.Pop();
            }
        }

        /// <summary>
        /// Adds an <see cref="ICommand"/> to an existing or new transaction and executes leaving the 
        /// transaction open for further commands.
        /// </summary>
        /// <param name="transactionalAction">The transactional action.</param>
        [DebuggerStepThrough] 
        public void RunWithinTransaction(Action<ICommandManager> transactionalAction)
        {
            try
            {
                this.unboundedTransactionCall = true;
                this.unboundedTransactionRunning = true;
                this.RunWithoutAutoCommit(transactionalAction);
            }
            finally
            {
                this.unboundedTransactionCall = false;
            }
        }

        #endregion

        #region Privates

        /// <summary>
        /// Temporarly disables the autocommitting until the action has been executed.
        /// </summary>
        /// <param name="transactionalAction">The transactional action.</param>
        [DebuggerStepThrough]
        private void RunWithoutAutoCommit(Action<ICommandManager> transactionalAction)
        {
            try
            {
                this.autocommit = false;
                transactionalAction(this);
            }
            finally
            {
                this.autocommit = true;
            }
        }

        /// <summary>
        /// Runs the command till end.
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerStepThrough]
        private void RunCommandTillEndWith(Action action)
        {
            if (this.commandRunning)
            {
                return;
            }

            try
            {
                this.commandRunning = true;
                action();
            }
            finally
            {
                this.commandRunning = false;
            }
        }

        /// <summary>
        /// Executes the in a spawned thread.
        /// </summary>
        /// <param name="command">The command.</param>
        [DebuggerStepThrough]
        private void ExecuteInSpawnedThread(AbstractCommand command)
        {
            command.CancellationTokenSource = new CancellationTokenSource();

            Task.Factory
                .StartNew(command.Execute);
        }

        /// <summary>
        /// Unexecutes the command in a spawned thread.
        /// </summary>
        /// <param name="command">The command.</param>
        private void UnExecuteInSpawnedThread(AbstractCommand command)
        {
            command.CancellationTokenSource = new CancellationTokenSource();

            Task.Factory
                .StartNew(command.UnExecute);
        }

        #endregion

        #region Events

        /// <summary>
        /// Listen to this event to be notified when a new command is added, executed, undone or redone
        /// </summary>
        public event EventHandler CanUndoRedoChanged = delegate { };

        /// <summary>
        /// Raises the can undo redo changed event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void RaiseCanUndoRedoChanged(object sender, EventArgs e)
        {
            CanUndoRedoChanged(this, e);
        }

        #endregion
    }
}
