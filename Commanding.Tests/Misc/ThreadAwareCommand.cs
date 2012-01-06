using System;
using System.Threading;

namespace Commanding.Tests.Misc
{
    public class ThreadAwareCommand : AbstractCommand
    {
        public string ExecutingThreadName { get; set; }
        public int ExecutingThreadId { get; set; }

        #region Overrides of AbstractCommand

        protected override void ExecuteCore()
        {
            ExecutingThreadName = Thread.CurrentThread.Name;
            ExecutingThreadId = Thread.CurrentThread.ManagedThreadId;

            this.Completed(new CompletedState(this));
        }

        protected override void UnExecuteCore()
        {
            throw new NotImplementedException();
        }

        protected override string GetParameterDescription()
        {
            return "Only for testing";
        }

        #endregion
    }
}