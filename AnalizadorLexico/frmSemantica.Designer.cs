namespace AnalizadorLexico
{
    partial class frmSemantica
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
            this.txtSemantica = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtSemantica
            // 
            this.txtSemantica.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSemantica.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtSemantica.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.txtSemantica.Location = new System.Drawing.Point(12, 42);
            this.txtSemantica.Multiline = true;
            this.txtSemantica.Name = "txtSemantica";
            this.txtSemantica.Size = new System.Drawing.Size(512, 247);
            this.txtSemantica.TabIndex = 22;
            // 
            // frmSemantica
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 301);
            this.Controls.Add(this.txtSemantica);
            this.Name = "frmSemantica";
            this.Text = "Semantica";
            this.Load += new System.EventHandler(this.frmSemantica_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSemantica;
    }
}