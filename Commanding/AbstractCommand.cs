using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Commanding.EventAggregator;

namespace Commanding
{
    /// <summary>
    /// Base class for custom command implementations
    /// </summary>
    public abstract class AbstractCommand : ICommand
    {
        #region Member 

        private bool completedAlreadyCalled = false;
        private List<ProgressInfo> progressInfos = new List<ProgressInfo>();
        private DateTime startTime;

        #endregion

        #region Properties 

        internal SynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// Gets or sets counter that indicates how often this command was executed
        /// </summary>
        /// <value>
        /// Counte for number of executions
        /// </value>
        protected int ExecuteCount { get; set; }

        /// <summary>
        /// Override execute core to provide your logic that actually performs the command
        /// </summary>
        protected abstract void ExecuteCore();

        /// <inheritdoc/>
        public virtual bool AllowToMergeWithPrevious { get; set; }

        /// <inheritdoc/>
        public Action<CompletedState> CompletedHandler { get; set; }

        private void CallCompletedFallbackHandler(CompletedState state)
        {
            if (state.Error != null)
            {
                throw state.Error;
            }
        }

        /// <summary>
        /// A synchronized completed call to marshall back to the calling thread.
        /// </summary>
        /// <param name="state">The state.</param>
        public void Completed(CompletedState state)
        {
            if (completedAlreadyCalled)
            {
                return;
            }

            completedAlreadyCalled = true;
            this.SynchronizedCompleted(state);
        }

        /// <summary>
        /// Informs attached handlers about a command's exit. Can be positive or negative.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SynchronizedCompleted(CompletedState state)
        {
            if (state.Aborted)
            {
                this.CommandManager.CommandAborted(state);
                //return;
            }

            if (this.CompletedHandler == null)
            {
                this.CallCompletedFallbackHandler(state);
                return;
            }

            if (this.CommandManager == null)
            {
                CompletedHandler(state);
                return;
            }

            this.CommandManager.MarshallBack(() => this.CompletedHandler(state));
        }

        /// <inheritdoc/>
        IEventAggregator ICommand.EventAggregator { get; set; }

        /// <summary>
        /// Gets the max degree of parallelism.
        /// </summary>
        /// <value>The max degree of parallelism.</value>
        /// <returns>Number of processors to use.</returns>
        protected static int MaxDegreeOfParallelism
        {
            get
            {
                switch (Environment.ProcessorCount)
                {
                    case 1: return 1;
                    case 2: return 2;
                    case 3: return 3;
                    case 4: return 3;
                    default: return Environment.ProcessorCount / 2;
                }
            }
        }

        /// <summary>
        /// Gets or sets the execution configuration.
        /// </summary>
        /// <value>
        /// The execution configuration.
        /// </value>
        public CommandExecutionConfiguration ExecutionConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the command manager.
        /// </summary>
        /// <value>
        /// The command manager.
        /// </value>
        public ICommandManager CommandManager { get; set; }

        /// <summary>
        /// Gets a value indicating whether [cancelation requested].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cancelation requested]; otherwise, <c>false</c>.
        /// </value>
        protected bool CancelationRequested
        {
            get
            {
                return this.CancellationTokenSource.Token.IsCancellationRequested;
            }
        }

        /// <summary>
        /// Runs a function with support for cancelation and updates progress information.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns>True if canceled, otherwise false.</returns>
        protected bool AsCancelable(Func<ProgressInfo> func)
        {
            if (!this.CancelationRequested)
            {
                this.UpdateProgress(func());
                return false;
            }

            this.AbortCommand("Die Ausführung der aktuellen Aktion wurde abgebrochen.");
            return true;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public virtual void Execute()
        {
            try
            {
                this.startTime = DateTime.Now;
                AutoComplete(this.ExecuteCore);
                //logger.WriteInfo("Command wurde ausgeführt: {0} mit Parametern: {1} ", this.GetType().Name, this.GetParameterDescription());
            }
            catch (Exception exception)
            {
                Completed(new CompletedState(this) { Error = exception });
                NotifyInDebug(exception);
            }

            ExecuteCount++;
        }

        private void AutoComplete(Action executeCore)
        {
            try
            {
                this.completedAlreadyCalled = false;
                executeCore();
                
                if (!this.completedAlreadyCalled)
                {
                    this.Completed(new CompletedState(this));
                }
            }
            finally
            {
                this.completedAlreadyCalled = false;
            }
        }

        /// <summary>
        /// Aborts the current command because of f.e. unsuccessfull validation.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void AbortCommand(string message)
        {
            this.Completed(new CompletedState(this) { Aborted = true, Message = message });
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public virtual void UnExecute()
        {
            try
            {
                this.startTime = DateTime.Now;
                AutoComplete(UnExecuteCore);
                //logger.WriteInfo("UNDONE: {0} mit Parametern: {1} ", this.GetType().Name, this.GetParameterDescription());
            }
            catch (Exception exception)
            {
                NotifyInDebug(exception);
                Completed(new CompletedState(this)
                              {
                                  UndoRedoState = UndoRedoState.Undo,
                                  Error = exception
                              });
            }

            ExecuteCount--;
        }

        /// <summary>
        /// Override this to provide the logic that undoes the command
        /// </summary>
        protected abstract void UnExecuteCore();

        /// <summary>
        /// Gets the parameters of a Command.
        /// </summary>
        /// <returns>
        /// String representation of the parameters used in this command
        /// </returns>
        protected abstract string GetParameterDescription();

        /// <inheritdoc/>
        public virtual bool CanExecute()
        {
            return ExecuteCount == 0;
        }

        /// <inheritdoc/>
        public virtual bool CanUnExecute()
        {
            return !CanExecute();
        }

        /// <inheritdoc/>
        public virtual bool TryToMerge(ICommand followingCommand)
        {
            return false;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("CMD {0} : {1} ", this.GetType().Name, this.GetParameterDescription());
        }

        [Conditional("DEBUG")]
        private void NotifyInDebug(Exception exception)
        {
            if (Debugger.IsAttached)
            {
                Debug.Fail(exception.Message, exception.StackTrace);
            }
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progressInfo">The progress info.</param>
        protected void UpdateProgress(ProgressInfo progressInfo)
        {
            if (this.ExecutionConfiguration == null || this.ExecutionConfiguration.ProgressAction == null)
            {
                return;
            }

            //progressInfo.TimeElapsed = DateTime.Now - this.startTime;

            if (this.CommandManager == null)
            {
                this.ExecutionConfiguration.ProgressAction(progressInfo);
                return;
            }

            this.CommandManager.MarshallBack(() => this.ExecutionConfiguration.ProgressAction(progressInfo));
        }

        #endregion Methods

        /// <summary>
        /// Gets or sets the state of the completed.
        /// </summary>
        /// <value>
        /// The state of the completed.
        /// </value>
        public CompletedState CompletedState { get; set; }

        internal CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        public void Cancel()
        {
            if (this.CancellationTokenSource != null)
            {
                this.CancellationTokenSource.Cancel();
            }
            else
            {
                throw new InvalidOperationException("This Command is not cancelable! Use AsCancelable(Action action)");
            }
        }
    }
}
