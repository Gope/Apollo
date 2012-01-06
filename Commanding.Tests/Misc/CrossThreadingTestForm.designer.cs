namespace Commanding.Tests.Misc
{
    partial class CrossThreadingTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.target = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // target
            // 
            this.target.FormattingEnabled = true;
            this.target.Location = new System.Drawing.Point(12, 12);
            this.target.Name = "target";
            this.target.Size = new System.Drawing.Size(121, 21);
            this.target.TabIndex = 0;
            // 
            // CrossThreadingTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(145, 44);
            this.Controls.Add(this.target);
            this.Name = "CrossThreadingTestForm";
            this.Text = "CrossThreadingTestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox target;


    }
}