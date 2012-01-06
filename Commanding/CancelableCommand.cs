using System;

using Commanding.Procedures;

namespace Commanding
{
    /// <summary>
    /// Supports the cancelation of a command.
    /// </summary>
    public abstract class CancelableCommand : AbstractCommand
    {
        #region Protected Methods 

        /// <summary>
        /// Creates the operator.
        /// </summary>
        /// <typeparam name="T">A type derived from CancelableOperator.</typeparam>
        /// <param name="creatorFunc">The creator func.</param>
        /// <param name="abortMessage">The abort message.</param>
        /// <returns>An <see cref="CancelableProcedure"/> derived class instance.</returns>
        protected T Create<T>(Func<CancelableProcedure> creatorFunc, string abortMessage)
            where T : CancelableProcedure
        {
            var cancelableOperator = creatorFunc();
            cancelableOperator.UpdateProgress = this.UpdateProgress;
            cancelableOperator.CancelationToken = this.CancellationTokenSource.Token;
            cancelableOperator.ProcedureCanceled += (sender, args) => this.AbortCommand(abortMessage);

            return (T)cancelableOperator;
        }

        #endregion

        #region Overrides of AbstractCommand

        /// <summary>
        /// Override execute core to provide your logic that actually performs the command
        /// </summary>
        protected abstract override void ExecuteCore();

        /// <summary>
        /// Override this to provide the logic that undoes the command
        /// </summary>
        protected abstract override void UnExecuteCore();

        /// <summary>
        /// Gets the parameters of a Command.
        /// </summary>
        /// <returns>
        /// String representation of the parameters used in this command
        /// </returns>
        protected abstract override string GetParameterDescription();

        #endregion
    }
}