namespace Commanding.Tests.Misc
{
    /// <summary>
    /// A command that is canceable.
    /// </summary>
    public class CancelableTestCommand : CancelableCommand
    {
        #region Overrides of AbstractCommand

        /// <summary>
        /// Override execute core to provide your logic that actually performs the command
        /// </summary>
        protected override void ExecuteCore()
        {
            for (int i = 0; i < 100; i++)
            {
                this.AsCancelable(() => new ProgressInfo("MyTask", i, "Doin' the do!"));
            }
        }

        /// <summary>
        /// Override this to provide the logic that undoes the command
        /// </summary>
        protected override void UnExecuteCore()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the parameters of a Command.
        /// </summary>
        /// <returns>
        /// String representation of the parameters used in this command
        /// </returns>
        protected override string GetParameterDescription()
        {
            return "Only for testing.";
        }

        #endregion
    }
}