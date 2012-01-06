namespace Commanding.Tests.Misc
{
    /// <summary>
    /// A simple Command for testing purposes
    /// </summary>
    public class IncrementBy5Command : AbstractCommand
    {
        public IncrementBy5Command(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        /// <summary>
        /// Increment Value by one.
        /// </summary>
        protected override void ExecuteCore()
        {
            Value += 5;
            Completed(new CompletedState(this));
        }

        /// <summary>
        /// Decrement Value by one.
        /// </summary>
        protected override void UnExecuteCore()
        {
            Value -= 5;
            Completed(new CompletedState(this) { UndoRedoState = UndoRedoState.Undo });
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