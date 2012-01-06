using System;
using System.Threading;

namespace Commanding.Procedures
{
    /// <summary>
    /// Base class for procedures that can be canceled.
    /// </summary>
    public class CancelableProcedure
    {
        #region Member 

        private Action<ProgressInfo> updateProgress;

        #endregion

        #region Events 

        /// <summary>
        /// Occurs when an operation gets canceled.
        /// </summary>
        public event EventHandler ProcedureCanceled = delegate { };

        #endregion

        #region Properties 

        /// <summary>
        /// Gets or sets the cancelation token.
        /// </summary>
        /// <value>
        /// The cancelation token.
        /// </value>
        public CancellationToken CancelationToken { get; set; }

        /// <summary>
        /// Gets or sets the action for reporting progress outside this class.
        /// </summary>
        /// <value>
        /// The update progress action.
        /// </value>
        public Action<ProgressInfo> UpdateProgress
        {
            get
            {
                return this.updateProgress ?? (this.updateProgress = delegate { });
            }

            set
            {
                this.updateProgress = value;
            }
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Wraps the single, cancelable sections of a method.
        /// </summary>
        /// <param name="funcs">The funcs.</param>
        protected void WithCancelation(params Func<ProgressInfo>[] funcs)
        {
            foreach (var func in funcs)
            {
                if (this.CancelationToken.IsCancellationRequested)
                {
                    this.ProcedureCanceled(this, EventArgs.Empty);
                    break;
                }

                var progressInfo = func();
                this.UpdateProgress(progressInfo);
            }
        }

        #endregion
    }
}