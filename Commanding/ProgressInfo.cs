using System;

namespace Commanding
{
    /// <summary>
    /// A struct holding information about the current progress of a command.
    /// </summary>
    public struct ProgressInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressInfo"/> struct.
        /// </summary>
        /// <param name="task">The title of the task for this progress.</param>
        /// <param name="progressPercentage">The progress percentage.</param>
        /// <param name="progressMessage">The progress message.</param>
        public ProgressInfo(string task, double progressPercentage, string progressMessage)
            : this()
        {
            this.Task = task;
            this.ProgressPercentage = progressPercentage;
            this.ProgressMessage = progressMessage;
        }

        /// <summary>
        /// Gets or sets the progress percentage.
        /// </summary>
        /// <value>
        /// The progress percentage.
        /// </value>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// Gets or sets the progress message.
        /// </summary>
        /// <value>
        /// The progress message.
        /// </value>
        public string ProgressMessage { get; set; }

        /// <summary>
        /// Gets or sets the time elapsed.
        /// </summary>
        /// <value>
        /// The time elapsed.
        /// </value>
        public TimeSpan TimeElapsed { get; set; }

        /// <summary>
        /// Gets the time estimated until completion.
        /// </summary>
        /// <value>
        /// The time estimated until completion.
        /// </value>
        public TimeSpan TimeEstimated 
        { 
            get
            {
                if (ProgressPercentage <= 0)
                {
                    return new TimeSpan();
                }

                var factor = 100 / ProgressPercentage;
                var milliseconds = TimeElapsed.TotalMilliseconds * factor - TimeElapsed.TotalMilliseconds;
                return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(milliseconds));
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var restzeit = this.TimeEstimated < new TimeSpan(0, 0, 0, 0, 1)
                               ? "< 1 ms"
                               : string.Format("{0:hh\\:mm\\:ss\\.ff}", this.TimeEstimated);

            var output = string.Format(
                "[{0}]: {1:00}% - Restzeit: {2} - Aktion: {3})", 
                this.Task, 
                this.ProgressPercentage, 
                restzeit, 
                this.ProgressMessage);

            return output;
        }

        /// <summary>
        /// Gets or sets the title for the task this progressinfo is about.
        /// </summary>
        /// <value>
        /// The task currently being worked on.
        /// </value>
        public string Task { get; set; }
    }
}