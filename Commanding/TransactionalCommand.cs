using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Commanding
{
    /// <summary>
    /// Internal 
    /// </summary>
    [DebuggerDisplay("{FormattedDisplay()}")]
    public class TransactionalCommand : AbstractCommand
    {
        #region Constructors 

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalCommand"/> class.
        /// </summary>
        public TransactionalCommand()
        {
            Commands = new List<AbstractCommand>();
        }

        #endregion

        #region Properties  

        /// <summary>
        /// Gets or sets the internal List of Commands for this Transaction.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        public List<AbstractCommand> Commands { get; set; }

        #endregion

        #region Overrides of AbstractCommand

        /// <summary>
        /// Override execute core to provide your logic that actually performs the command
        /// </summary>
        protected override void ExecuteCore()
        {
            foreach (var abstractCommand in this.Commands)
            {
                abstractCommand.Execute();
            }
        }

        /// <summary>
        /// Override this to provide the logic that undoes the command
        /// </summary>
        protected override void UnExecuteCore()
        {
            var reversedCommands = this.Commands.ToList();
            reversedCommands.Reverse();
            foreach (var abstractCommand in reversedCommands)
            {
                abstractCommand.UnExecute();
            }
        }

        /// <summary>
        /// Gets the parameters of a Command.
        /// </summary>
        /// <returns>
        /// String representation of the parameters used in this command
        /// </returns>
        protected override string GetParameterDescription()
        {
            return string.Empty;
        }

        /// <summary>
        /// Adds a new command to the internal collection.
        /// </summary>
        /// <param name="command">The command.</param>
        public void AddCommand(AbstractCommand command)
        {
            this.Commands.Add(command);
        }

        /// <summary>
        /// Formats the display.
        /// </summary>
        /// <returns>A formatted display string showing all the commands within this instance.</returns>
        public string FormattedDisplay()
        {
            var counter = 1;
            var builder = new StringBuilder();
            foreach (var command in Commands)
            {
                builder.Append(string.Format("{0}: {1} //", counter, command.GetType().Name));
                counter++;
            }

            return builder.ToString();
        }

        #endregion
    }
}