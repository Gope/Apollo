using System.Threading;

using Commanding.EventAggregator;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Commanding.Tests
{
    [TestClass]
    public class MultiThreadingCommandManagerFixture
    {
        [TestMethod]
        // This is currently irrellevant...
        // One needs to calculate multiples of 10 for the first thread, multiples of 11 for the second and multiples of 12 for the third thread 
        // so the expected values belong to specific threads (most of them). This test is useless at the moment.
        public void Crush()
        {
            int timesCalledCompleted1 = 0;
            int timesCalledCompleted2 = 0;
            int timesCalledCompleted3 = 0;

            var aggregator = new Mock<IEventAggregator>();
            var manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);

            var cmd1 = new LastingCommand(10);
            var cmd2 = new LastingCommand(20);
            var cmd3 = new LastingCommand(30);

            for (int i = 0; i < 100; i++ )
            {
                manager.Do(Execute.The(cmd1).RunCommandInBackground().AndWhenCompletedCall(state => timesCalledCompleted1++));
                manager.Do(Execute.The(cmd2).RunCommandInBackground().AndWhenCompletedCall(state => timesCalledCompleted2++));
                manager.Do(Execute.The(cmd3).RunCommandInBackground().AndWhenCompletedCall(state => timesCalledCompleted3++));
            }
        }
    }

    public class LastingCommand : AbstractCommand
    {
        private readonly int sleepTime;

        public LastingCommand(int sleepTime)
        {
            this.sleepTime = sleepTime;
        }

        #region Overrides of AbstractCommand

        protected override void ExecuteCore()
        {
            Thread.Sleep(sleepTime);
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