using System;

namespace Commanding.Tests.Misc
{
    /// <summary>
    /// Only for unittesting abroting commands.
    /// </summary>
    public class AbortingCommand : AbstractCommand
    {
        #region Overrides of AbstractCommand

        protected override void ExecuteCore()
        {
            this.AbortCommand("No You can't!");
        }

        protected override void UnExecuteCore()
        {
            throw new NotImplementedException();
        }

        protected override string GetParameterDescription()
        {
            return "This command is only for unittesting.";
        }

        #endregion
    }
}