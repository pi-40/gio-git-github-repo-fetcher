namespace WinFormsApp6
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            terminal = new TextBox();
            SuspendLayout();
            // 
            // terminal
            // 
            terminal.BackColor = Color.FromArgb(12, 12, 12);
            terminal.BorderStyle = BorderStyle.None;
            terminal.Dock = DockStyle.Fill;
            terminal.Font = new Font("Consolas", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            terminal.ForeColor = Color.FromArgb(242, 242, 242);
            terminal.Location = new Point(0, 0);
            terminal.Margin = new Padding(3, 4, 3, 4);
            terminal.Multiline = true;
            terminal.Name = "terminal";
            terminal.ScrollBars = ScrollBars.Vertical;
            terminal.Size = new Size(914, 600);
            terminal.TabIndex = 0;
            terminal.KeyDown += terminal_KeyDown;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(12, 12, 12);
            ClientSize = new Size(914, 600);
            Controls.Add(terminal);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Gio";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox terminal;
    }
}