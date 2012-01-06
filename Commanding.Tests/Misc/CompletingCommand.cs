using System;

namespace Commanding.Tests.Misc
{
    /// <summary>
    /// A Command completing zero to multiple times
    /// </summary>
    public class CompletingCommand : AbstractCommand
    {
        private readonly int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletingCommand"/> class.
        /// </summary>
        /// <param name="count">The count.</param>
        public CompletingCommand(int count)
        {
            this.count = count;
        }

        #region Overrides of AbstractCommand

        /// <summary>
        /// Override execute core to provide your logic that actually performs the command
        /// </summary>
        protected override void ExecuteCore()
        {
            for (int i = 0; i < count; i++)
            {
                this.Completed(new CompletedState(this));
            }
        }

        /// <summary>
        /// Override this to provide the logic that undoes the command
        /// </summary>
        protected override void UnExecuteCore()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parameters of a Command.
        /// </summary>
        /// <returns>
        /// String representation of the parameters used in this command
        /// </returns>
        protected override string GetParameterDescription()
        {
            return "Only for testing";
        }

        #endregion
    }
}