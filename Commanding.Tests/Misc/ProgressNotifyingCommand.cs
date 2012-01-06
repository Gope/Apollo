using System.Threading;

namespace Commanding.Tests.Misc
{
    /// <summary>
    /// A command notifying about its progress.
    /// </summary>
    public class ProgressNotifyingCommand : AbstractCommand
    {
        #region Overrides of AbstractCommand

        protected override void ExecuteCore()
        {
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(5);
                this.UpdateProgress(new ProgressInfo("My Task", i * 10, "Tue gerade was"));
            }
        }

        protected override void UnExecuteCore()
        {
            throw new System.NotImplementedException();
        }

        protected override string GetParameterDescription()
        {
            return "Only for testing";
        }

        #endregion
    }
}