using System;

using Commanding.EventAggregator;

namespace Commanding
{
    /// <summary>
    /// Encapsulates a user command (actually two actions: Do and Undo)
    /// Can be anything.
    /// You can give your implementation any information it needs to be able to
    /// execute and rollback what it needs.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Apply changes encapsulated by this object.
        /// </summary>
        /// <remarks>
        /// ExecuteCount++
        /// </remarks>
        void Execute();

        /// <summary>
        /// Undo changes made by a previous Execute call.
        /// </summary>
        /// <remarks>
        /// ExecuteCount--
        /// </remarks>
        void UnExecute();

        /// <summary>
        /// For most Actions, CanExecute is true when ExecuteCount = 0 (not yet executed)
        /// and false when ExecuteCount = 1 (already executed once)
        /// </summary>
        /// <returns>
        ///     <c>true</c> if an encapsulated command can be applied; otherwise <c>false</c>
        /// </returns>
        bool CanExecute();

        /// <summary>
        /// Determines whether this instance [can un execute].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a command was already executed and can be undone; otherwise <c>false</c>
        /// </returns>
        bool CanUnExecute();

        /// <summary>
        /// Attempts to take a new incoming command and instead of recording that one
        /// as a new command, just modify the current one so that it's summary effect is 
        /// a combination of both.
        /// </summary>
        /// <param name="followingCommand">The command that should be merged into the current command</param>
        /// <returns>true if the command agreed to merge, false if we want the followingCommand
        /// to be tracked separately</returns>
        bool TryToMerge(ICommand followingCommand);

        /// <summary>
        /// Defines if the command can be merged with the previous one in the Undo buffer
        /// This is useful for long chains of consecutive operations of the same type,
        /// e.g. dragging something or typing some text
        /// </summary>
        /// <value>
        ///     <c>true</c> if command can be merged; otherwise <c>false</c>
        /// </value>
        bool AllowToMergeWithPrevious { get; }

        /// <summary>
        /// Triggers a callback when the command finished executing. Used to get feedback about
        /// wether command execution was successful, an exception occured or execution was aborted
        /// </summary>
        /// <param name="state">The <see cref="CompletedState"/>.</param>
        void Completed(CompletedState state);

        /// <summary>
        /// Gets or sets the completed handler which gets triggered by the Completed(CompletedState state) method.
        /// </summary>
        /// <value>
        /// The completed handler.
        /// </value>
        Action<CompletedState> CompletedHandler { get; set; }

        /// <summary>
        /// Gets or sets the EventAggregator for this Command
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        IEventAggregator EventAggregator { get; set; }
    }
}
