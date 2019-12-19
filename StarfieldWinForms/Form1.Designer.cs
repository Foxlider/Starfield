namespace StarfieldWinForms
{
    partial class Form1
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
            this.fpsCounter = new System.Windows.Forms.Label();
            this.fpsCounter2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // fpsCounter
            // 
            this.fpsCounter.AutoSize = true;
            this.fpsCounter.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.fpsCounter.Location = new System.Drawing.Point(13, 13);
            this.fpsCounter.Name = "fpsCounter";
            this.fpsCounter.Size = new System.Drawing.Size(27, 13);
            this.fpsCounter.TabIndex = 0;
            this.fpsCounter.Text = "FPS";
            // 
            // fpsCounter2
            // 
            this.fpsCounter2.AutoSize = true;
            this.fpsCounter2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.fpsCounter2.Location = new System.Drawing.Point(13, 30);
            this.fpsCounter2.Name = "fpsCounter2";
            this.fpsCounter2.Size = new System.Drawing.Size(27, 13);
            this.fpsCounter2.TabIndex = 1;
            this.fpsCounter2.Text = "FPS";
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar1.Location = new System.Drawing.Point(12, 393);
            this.trackBar1.Maximum = 500;
            this.trackBar1.Minimum = 128;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(776, 45);
            this.trackBar1.TabIndex = 2;
            this.trackBar1.Value = 128;
            this.trackBar1.ValueChanged += Slider_ValueChanged;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.fpsCounter2);
            this.Controls.Add(this.fpsCounter);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label fpsCounter;
        private System.Windows.Forms.Label fpsCounter2;
        private System.Windows.Forms.TrackBar trackBar1;
    }
}

