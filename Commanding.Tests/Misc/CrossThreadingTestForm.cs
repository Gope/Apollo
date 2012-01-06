using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Commanding.EventAggregator;

using Moq;

namespace Commanding.Tests.Misc
{
    /// <summary>
    /// A testing form.
    /// </summary>
    public partial class CrossThreadingTestForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrossThreadingTestForm"/> class.
        /// </summary>
        public CrossThreadingTestForm(ThreadAwareCommand command)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = true;
            Values = new List<int>{1, 2};
            this.target.DataSource = Values;

            Mock<IEventAggregator> aggregator = new Mock<IEventAggregator>();
            CommandManager manager = new CommandManager(aggregator.Object, SynchronizationContext.Current);

            manager.Do(
                Execute.The(command)
                    .RunCommandInBackground());
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Shown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public List<int> Values { get; set; }
    }
}
