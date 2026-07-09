namespace WinFormsApp6
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtTerminal;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtTerminal = new TextBox();
            SuspendLayout();
            // 
            // txtTerminal
            // 
            txtTerminal.BackColor = Color.Black;
            txtTerminal.BorderStyle = BorderStyle.None;
            txtTerminal.Dock = DockStyle.Fill;
            txtTerminal.Font = new Font("Consolas", 12F);
            txtTerminal.ForeColor = Color.White;
            txtTerminal.Location = new Point(0, 0);
            txtTerminal.Margin = new Padding(3, 4, 3, 4);
            txtTerminal.Name = "txtTerminal";
            txtTerminal.ScrollBars = ScrollBars.Vertical;
            txtTerminal.Size = new Size(1054, 24);
            txtTerminal.TabIndex = 0;
            txtTerminal.KeyDown += txtTerminal_KeyDown;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1054, 654);
            Controls.Add(txtTerminal);
            ForeColor = Color.White;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "gio-git";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}