using System;

namespace Commanding.Tests.Misc
{
    /// <summary>
    /// A simple command for testing purposes which throws an exeption when executed.
    /// </summary>
    public class ThrowingCommand : AbstractCommand
    {
        /// <summary>
        /// Override execute core to provide your logic that actually performs the command
        /// </summary>
        protected override void ExecuteCore()
        {
            throw new InvalidOperationException("Something feels wrong");
        }

        /// <summary>
        /// Override this to provide the logic that undoes the command
        /// </summary>
        protected override void UnExecuteCore()
        {
            // nothing
        }

        /// <summary>
        /// Gets the parameters of a Command.
        /// </summary>
        /// <returns></returns>
        protected override string GetParameterDescription()
        {
            return "Only for Testing!";
        }
    }
}