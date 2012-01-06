using System.Diagnostics;

namespace Commanding
{
    /// <summary>
    /// Configures a synchronous ICommand execution.
    /// </summary>
    public static class Execute
    {
        /// <summary>
        /// Creates a new Configuration for an existing ICommand instance.
        /// </summary>
        /// <param name="command">The command to use for this configuration.</param>
        /// <returns>
        /// A new configuration.
        /// </returns>
        [DebuggerStepThrough]
        public static CommandExecutionConfiguration The(ICommand command)
        {
            var config = new CommandExecutionConfiguration { ExecutingCommand = command };
            if (command is AbstractCommand)
            {
                (command as AbstractCommand).ExecutionConfiguration = config;
            }

            return config;
        }
    }
}