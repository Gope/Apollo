using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Commanding.EventAggregator;
using Commanding.Tests.Misc;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Commanding.Tests
{
    [TestClass]
    public class CommandsInBackgroundThreadFixture
    {
        /// <summary>
        /// Initializes the SynchronizationContext.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestMethod]
        public void Given_An_ICommand_When_Calling_Run_CommandInBackground_Should_alter_ExecutionConfiguration()
        {
            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            var incrementCommand = new IncrementByOneCommand(1);

            var configuration = Execute.The(incrementCommand).RunCommandInBackground();
            manager.Do(configuration);

            Assert.IsTrue(configuration.RunInBackground, "RunInBackground was not set to TRUE!");
        }

        [TestMethod]
        public void Given_An_ICommand_When_Calling_Run_CommandInBackground_Should_execute_properly()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            var incrementCommand = new IncrementByOneCommand(1);

            manager.Do(
                Execute.The(incrementCommand)
                    .RunCommandInBackground()
                    .AndWhenCompletedCall(state => resetEvent.Set()));

            resetEvent.WaitOne(1000, false);
            Assert.IsTrue(incrementCommand.Value == 2, "Command was not executed properly!");
        }

        [TestMethod]
        public void Given_An_ICommand_When_Calling_RunCommandInBackground_Should_call_Completed_in_MainThread()
        {
            // THIS TEST CREATES, SHOWS AND TEARS DOWN A FORM FOR TESTING - MESSAGE PUMP NEEDED!!!
            int completedThreadId = -1;
            var command = new ThreadAwareCommand{ CompletedHandler = state => { completedThreadId = Thread.CurrentThread.ManagedThreadId; } };
            var form = new CrossThreadingTestForm(command);

            Application.Run(form);
            
            Assert.IsTrue(Thread.CurrentThread.ManagedThreadId != command.ExecutingThreadId, "Command was executed on the same thread that created it!");
            Assert.IsTrue(Thread.CurrentThread.ManagedThreadId == completedThreadId, "Completed was not called on the mainthread!");
        }

        [TestMethod]
        public void Given_An_ICommand_When_Calling_OnProgressUpdatesDo_Should_Report_Progress_To_Caller()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            var updates = new List<double>();
            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            var command = new ProgressNotifyingCommand();

            manager.Do(
                Execute.The(command)
                    .RunCommandInBackground()
                    .OnProgressUpdatesDo(progress => updates.Add(progress.ProgressPercentage))
                    .AndWhenCompletedCall(state => resetEvent.Set()));

            resetEvent.WaitOne(1000, false);
            Assert.IsTrue(updates.Count == 9, "There should be 9 updates");
            Assert.IsTrue(updates[5] == 60, "This value should be 60 but is: " + updates[5]);
        }

        [TestMethod]
        public void Given_An_ICommand_When_Calling_OnProgressUpdatesDo_Should_Report_Progress_continuosly()
        {
            var updates = new List<double>();
            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            var command = new ProgressNotifyingCommand();
            double counter = 0;

            manager.Do(
                Execute.The(command)
                    .RunCommandInBackground()
                    .OnProgressUpdatesDo(progress =>
                        {
                            updates.Add(progress.ProgressPercentage);
                            Assert.IsTrue(updates[updates.Count - 1] == (counter += 10), "Percentage not updated correctly");
                        }));
        }

        [TestMethod]
        public void Given_An_ICommand_When_Calling_OnProgressUpdatesDo_Should_Report_EstimatedTime()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            var updates = new List<TimeSpan>();
            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            var command = new ProgressNotifyingCommand();

            manager.Do(
                Execute.The(command)
                    .RunCommandInBackground()
                    .OnProgressUpdatesDo(progress =>
                        {
                            updates.Add(progress.TimeEstimated);
                            System.Diagnostics.Debug.WriteLine(string.Format("({0:00}%) -> Elapsed: {1:00} ms / Estimated: {2:00} ms", progress.ProgressPercentage, progress.TimeElapsed.TotalMilliseconds, progress.TimeEstimated.TotalMilliseconds));
                        })
                    .AndWhenCompletedCall(state => resetEvent.Set()));

            resetEvent.WaitOne(1000, false);
            Assert.IsTrue(updates[0] > updates[5], "TimeElapsed is not calculated correctly.");
            Assert.IsTrue(updates[5] > updates[8], "TimeElapsed is not calculated correctly.");
        }

        [TestMethod]
        public void Given_a_CommandManager_When_canceling_execution_Should_Abort_command()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int timesCalledCompleted = 0;
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            CancelableTestCommand cmd = new CancelableTestCommand();

            manager.Do(Execute.The(cmd)
                .RunCommandInBackground()
                .OnProgressUpdatesDo(info =>
                    {
                        timesCalledCompleted++;
                        if (info.ProgressPercentage > 48)
                        {
                            cmd.Cancel();
                        }
                    })
                .AndWhenCompletedCall(state => resetEvent.Set()));

            resetEvent.WaitOne(1000, false);
            Assert.IsTrue(timesCalledCompleted == 50, "Completed was called multiple times.");
        }

        [TestMethod]
        public void Given_An_ICommand_When_executing_without_Undo_but_with_BackgroundThread_Should_create_a_new_Thread()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            var command = new ThreadAwareCommand();

            manager.Do(
                Execute.The(command)
                    .WithoutUndoRedo()
                    .RunCommandInBackground()
                    .AndWhenCompletedCall(state => resetEvent.Set()));

            resetEvent.WaitOne(1000, false);
            Assert.IsTrue(command.ExecutingThreadId != Thread.CurrentThread.ManagedThreadId, "ThreadIds must be different!");
        }
    }
}