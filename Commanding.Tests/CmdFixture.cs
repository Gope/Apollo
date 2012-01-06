using System;
using System.Collections.Generic;
using System.Threading;

using Commanding.EventAggregator;
using Commanding.Tests.Misc;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Commanding.Tests
{
    [TestClass]
    public class CmdFixture
    {
        [TestMethod]
        public void Given_a_CommandManager_When_Executing_Command_is_executed()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));

            Assert.IsTrue(cmd.Value == 2);
        }

        [TestMethod]
        public void Given_a_CommandManager_After_Executing_3_times_Command_is_executed_3_times()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));

            Assert.IsTrue(cmd.Value == 4, "Value is " + cmd.Value);
        }

        [TestMethod]
        public void Given_a_CommandManager_Before_Executing_CurrentTransaction_is_null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);

            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        public void Given_a_CommandManager_After_Executing_CurrentTransaction_is_null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));

            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        public void Given_a_CommandManager_After_Executing_multiple_times_CurrentTransaction_is_null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));

            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Executing_with_Exception_CurrentTransaction_is_null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            ThrowingCommand cmd = new ThrowingCommand();

            manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => { }));

            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        public void Given_a_CommandManager_After_Executing_And_Committing_CurrentTransaction_is__null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.CommitTransaction();

            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Executing_Within_Transaction_Then_CurrentTransaction_is__null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.RunWithinTransactionAndCommit(
                commandManager =>
                    {
                        manager.Do(Execute.The(cmd));
                        manager.Do(Execute.The(cmd));
                        manager.Do(Execute.The(cmd));
                    });
            
            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Executing_Within_Transaction_Then_CurrentTransaction_is_NOT_null_within_Transaction()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.RunWithinTransactionAndCommit(
                commandManager =>
                {
                    manager.Do(Execute.The(cmd));
                    manager.Do(Execute.The(cmd));

                    Assert.IsTrue(manager.CurrentTransaction != null);
                });
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_Command_is_rolled_back()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Undo();

            Assert.IsTrue(cmd.Value == 1);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_And_Redoing_Command_is_executed_normally()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Undo();
            manager.Redo();

            Assert.IsTrue(cmd.Value == 2);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_And_Redoing_And_again_Undoing_Command_is_rolled_back()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Undo();
            manager.Redo();
            manager.Undo();

            Assert.IsTrue(cmd.Value == 1);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_3_times_Can_Redo__3_times()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));
            manager.Undo();
            manager.Undo();
            manager.Undo();
            manager.Redo();
            manager.Redo();
            manager.Redo();

            Assert.IsTrue(cmd.Value == 4);
        }

        #region Multiple Commands 

        [TestMethod]
        public void Given_a_CommandManager_When_storing_different_Commands_Order_is_preserved()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd5));
            manager.Do(Execute.The(cmd));


            Assert.IsTrue(cmd.Value == 3);
            Assert.IsTrue(cmd5.Value == 5);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_storing_different_Commands_Everything_is_initialized_without_sideeffects()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            Assert.IsTrue(cmd.Value == 1);
            Assert.IsTrue(cmd5.Value == 0);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_storing_different_Commands_Undoing_is_in_order()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd5));
            manager.Do(Execute.The(cmd));

            Assert.IsTrue(cmd.Value == 3);
            Assert.IsTrue(cmd5.Value == 5);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_3_times_and_undoing_once_should_undo_last_command()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd5));
            manager.Do(Execute.The(cmd));

            manager.Undo();

            Assert.IsTrue(cmd.Value == 2);
            Assert.IsTrue(cmd5.Value == 5);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_3_times_and_undoing_twice_should_undo_last_2_commands()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd5));
            manager.Do(Execute.The(cmd));

            manager.Undo();
            manager.Undo();

            Assert.IsTrue(cmd.Value == 2);
            Assert.IsTrue(cmd5.Value == 0);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_3_times_and_undoing_3_times_should_undo_all_commands()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd5));
            manager.Do(Execute.The(cmd));

            manager.Undo();
            manager.Undo();
            manager.Undo();

            Assert.IsTrue(cmd.Value == 1);
            Assert.IsTrue(cmd5.Value == 0);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Transactionally_executing_3_times__and_undoing_once_should_undo_all_commands()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransactionAndCommit(commandManager =>
                {
                    manager.Do(Execute.The(cmd));
                    manager.Do(Execute.The(cmd5));
                    manager.Do(Execute.The(cmd));
                });

            manager.Undo();

            Assert.IsTrue(cmd.Value == 1);
            Assert.IsTrue(cmd5.Value == 0);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Transactionally_executing_3_times__and_undoing_and_Redoing_once_should_result_in_end_values()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransactionAndCommit(commandManager =>
            {
                manager.Do(Execute.The(cmd));
                manager.Do(Execute.The(cmd5));
                manager.Do(Execute.The(cmd));
            });

            manager.Undo();
            manager.Redo();

            Assert.IsTrue(cmd.Value == 3);
            Assert.IsTrue(cmd5.Value == 5);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_2_independend_StackTogetherTransactions_Should_be_put_in_one_TransactionalCommand()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd)));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5)));

            Assert.IsTrue(manager.CurrentTransaction.Commands.Count == 2);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_2_independend_StackTogetherTransactions_Should_apply_operations()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd)));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5)));
            manager.CommitTransaction();

            Assert.IsTrue(cmd.Value == 2);
            Assert.IsTrue(cmd5.Value == 5);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_2_independend_StackTogetherTransactions_And_Commit_Then_CurrenTransaction_Should_be_null()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd)));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5)));
            manager.CommitTransaction();

            Assert.IsTrue(manager.CurrentTransaction == null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Given_a_CommandManager_When_executing_an_unbounded_transaction_without_commiting_Should_throw_Exception()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd)));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5)));

            manager.Do(Execute.The(cmd));
        }

        [TestMethod]
        public void Given_a_CommandManager_When_executing_within_transaction_commits_with_RunWithinTransactionAndCommit()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd)));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5)));
            manager.RunWithinTransactionAndCommit(commandManager => manager.Do(Execute.The(cmd)));

            Assert.IsTrue(manager.CurrentTransaction == null);
            Assert.IsTrue(manager.UndoStack.Count == 1);
            Assert.IsTrue(manager.UndoStack.Peek().Commands.Count == 3);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_a_Transaction_Should_undo_grouped_commands_in_reversed_order()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);
            var calls = new List<ICommand>();

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => calls.Add(cmd))));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5).AndWhenCompletedCall(state => calls.Add(cmd5))));
            manager.CommitTransaction();
            manager.Undo();

            Assert.IsTrue(calls[2] == cmd5);
            Assert.IsTrue(calls[3] == cmd);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_undoing_an_Open_Transaction_should_call_completed_for_each_command_within_transaction()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);
            var calls = new List<ICommand>();

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => calls.Add(cmd))));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5).AndWhenCompletedCall(state => calls.Add(cmd5))));
            manager.Undo();

            Assert.IsTrue(calls[0] == cmd);
            Assert.IsTrue(calls[1] == cmd5);
            Assert.IsTrue(calls[2] == cmd5);
            Assert.IsTrue(calls[3] == cmd);
            Assert.IsTrue(calls.Count == 4);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_undoing_a_transaction_should_call_completed_for_each_command_within_transaction()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);
            var calls = new List<ICommand>();

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => calls.Add(cmd))));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5).AndWhenCompletedCall(state => calls.Add(cmd5))));
            manager.CommitTransaction();

            Assert.IsTrue(calls[0] == cmd);
            Assert.IsTrue(calls[1] == cmd5);
            Assert.IsTrue(calls.Count == 2);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_an_Open_Transaction_Should_Commit_and_Undo()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd)));
            manager.RunWithinTransaction(commandManager => manager.Do(Execute.The(cmd5)));
            manager.Undo();

            Assert.IsTrue(cmd.Value == 1);
            Assert.IsTrue(cmd5.Value == 0);
        }

        #endregion


        #region Exceptions

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Given_a_CommandManager_When_Executing_with_Exception_And_no_Completed_Handler_Should_throw_Exception()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            ThrowingCommand cmd = new ThrowingCommand();

            manager.Do(Execute.The(cmd));
        }

        #endregion

        #region Undo / Redo Notifications

        [TestMethod]
        public void Given_a_CommandManager_When_Executed_Should_notify_Undo_State_changed()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            var called = false;

            manager.CanUndoRedoChanged += (sender, args) => called = true;
            manager.Do(Execute.The(cmd));

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Once_Executed_Should_set_Undo_State_to_true()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));

            Assert.IsTrue(manager.CanUndo);
            Assert.IsFalse(manager.CanRedo);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Once_Executed_Should_set_Redo_State_to_true()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Undo();

            Assert.IsFalse(manager.CanUndo);
            Assert.IsTrue(manager.CanRedo);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Executed_and_Redone_Should_set_Undo_State_to_true()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);

            manager.Do(Execute.The(cmd));
            manager.Undo();
            manager.Redo();

            Assert.IsTrue(manager.CanUndo);
            Assert.IsFalse(manager.CanRedo);
        }

        #endregion

        #region Completed State tests

        [TestMethod]
        public void Given_a_CommandManager_When_Executing_Should_call_Completed()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            var called = false;

            manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => called = true));

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_Executing_Should_only_call_Completed_once()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            var called = 0;

            manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => called++));

            Assert.IsTrue(called == 1);
        }

        #endregion

        #region Transaction Abortion 

        [TestMethod]
        public void Given_a_CommandManager_When_Tranactionally_executing_3_commands_and_doing_Rollback_Should_Undo_all_commands()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.RunWithinTransactionAndCommit(commandManager =>
            {
                manager.Do(Execute.The(cmd));
                manager.Do(Execute.The(cmd5));
                manager.Do(Execute.The(cmd));

                manager.RollBackTransaction();
            });

            Assert.IsTrue(cmd.Value == 1);
            Assert.IsTrue(cmd5.Value == 0);
        }

        #endregion

        [TestMethod]
        public void Given_a_CommandManager_When_Undoing_and_executing_new_command_RedoStack_should_be_cleared()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand cmd = new IncrementByOneCommand(1);
            IncrementBy5Command cmd5 = new IncrementBy5Command(0);

            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(cmd));
            manager.Undo();
            manager.Do(Execute.The(cmd5));

            Assert.IsFalse(manager.CanRedo);
            Assert.IsTrue(manager.CanUndo);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_a_Command_Aborts_Should_Not_Register_for_Undo()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            AbortingCommand cmd = new AbortingCommand();

            manager.Do(Execute.The(cmd));

            Assert.IsFalse(manager.CanUndo);
        }

        [TestMethod]
        public void Given_a_CommandManager_When_one_of_multiple_Commands_Aborts_Should_only_Register_the_other_Commands_for_Undo()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            IncrementByOneCommand incCmd = new IncrementByOneCommand(1);
            AbortingCommand cmd = new AbortingCommand();
            IncrementBy5Command inc5Cmd = new IncrementBy5Command(1);

            manager.Do(Execute.The(incCmd));
            manager.Do(Execute.The(cmd));
            manager.Do(Execute.The(inc5Cmd));

            Assert.IsTrue(manager.UndoStack.Count == 2);
            Assert.IsTrue(manager.UndoStack.Pop().Commands[0] == inc5Cmd);
            Assert.IsTrue(manager.UndoStack.Pop().Commands[0] == incCmd);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Given_a_CommandManager_When_a_Command_executes_without_a_set_CompletedHandler_Should_promote_the_FallbackHandler()
        {
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);

            manager.Do(Execute.The(new ThrowingCommand()));
        }

        [TestMethod]
        public void Given_a_CommandManager_After_Execution_of_Command_Should_call_Completed_Automagically()
        {
            int timesCalledCompleted = 0;
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            CompletingCommand cmd = new CompletingCommand(0);

            manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => timesCalledCompleted++));

            Assert.IsTrue(timesCalledCompleted == 1, "Completed was never called automatically.");
        }

        [TestMethod]
        public void Given_a_CommandManager_When_calling_Completed_multiple_times_Should_fire_only_once()
        {
            int timesCalledCompleted = 0;
            var aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);
            CompletingCommand cmd = new CompletingCommand(5);

            manager.Do(Execute.The(cmd).AndWhenCompletedCall(state => timesCalledCompleted++));

            Assert.IsTrue(timesCalledCompleted == 1, "Completed was called multiple times.");
        }
    }
}