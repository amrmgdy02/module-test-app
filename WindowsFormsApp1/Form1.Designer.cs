using static System.Net.Mime.MediaTypeNames;

namespace WindowsFormsApp1
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.StartTestingButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.loadingOverlay = new System.Windows.Forms.Panel();
            this.loadingLabel = new System.Windows.Forms.Label();
            this.loadingProgress = new System.Windows.Forms.ProgressBar();
            this.panelMain.SuspendLayout();
            this.loadingOverlay.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.panelMain.Controls.Add(this.labelTitle);
            this.panelMain.Controls.Add(this.textBox1);
            this.panelMain.Controls.Add(this.comboBox1);
            this.panelMain.Controls.Add(this.StartTestingButton);
            this.panelMain.Controls.Add(this.progressBar1);
            this.panelMain.Location = new System.Drawing.Point(180, 60);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(440, 320);
            this.panelMain.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.labelTitle.Location = new System.Drawing.Point(0, 10);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(440, 40);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Tester";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(62)))), ((int)(((byte)(70)))));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(60, 70);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(320, 34);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "Number Of Cards";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(62)))), ((int)(((byte)(70)))));
            this.comboBox1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.comboBox1.ForeColor = System.Drawing.Color.White;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(60, 120);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(320, 36);
            this.comboBox1.TabIndex = 0;
            // 
            // StartTestingButton
            // 
            this.StartTestingButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.StartTestingButton.FlatAppearance.BorderSize = 0;
            this.StartTestingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartTestingButton.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.StartTestingButton.ForeColor = System.Drawing.Color.White;
            this.StartTestingButton.Location = new System.Drawing.Point(60, 180);
            this.StartTestingButton.Name = "StartTestingButton";
            this.StartTestingButton.Size = new System.Drawing.Size(320, 45);
            this.StartTestingButton.TabIndex = 2;
            this.StartTestingButton.Text = "Start Testing";
            this.StartTestingButton.UseVisualStyleBackColor = false;
            this.StartTestingButton.Click += new System.EventHandler(this.StartTestingButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(40)))), ((int)(((byte)(49)))));
            this.progressBar1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.progressBar1.Location = new System.Drawing.Point(60, 245);
            this.progressBar1.Maximum = 5;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(320, 28);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 3;
            // 
            // loadingOverlay
            // 
            this.loadingOverlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))), ((int)(((byte)(37)))));
            this.loadingOverlay.Controls.Add(this.loadingLabel);
            this.loadingOverlay.Controls.Add(this.loadingProgress);
            this.loadingOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingOverlay.Location = new System.Drawing.Point(0, 0);
            this.loadingOverlay.Name = "loadingOverlay";
            this.loadingOverlay.Size = new System.Drawing.Size(800, 450);
            this.loadingOverlay.TabIndex = 1;
            this.loadingOverlay.Visible = false;
            // 
            // loadingLabel
            // 
            this.loadingLabel.AutoSize = true;
            this.loadingLabel.BackColor = System.Drawing.Color.Transparent;
            this.loadingLabel.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.loadingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
            this.loadingLabel.Location = new System.Drawing.Point(320, 150);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new System.Drawing.Size(193, 50);
            this.loadingLabel.TabIndex = 0;
            this.loadingLabel.Text = "Loading...";
            // 
            // loadingProgress
            // 
            this.loadingProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.loadingProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
            this.loadingProgress.Location = new System.Drawing.Point(320, 210);
            this.loadingProgress.MarqueeAnimationSpeed = 30;
            this.loadingProgress.Name = "loadingProgress";
            this.loadingProgress.Size = new System.Drawing.Size(160, 20);
            this.loadingProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.loadingProgress.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(28)))), ((int)(((byte)(37)))));
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.loadingOverlay);
            this.Name = "Form1";
            this.Text = "Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.loadingOverlay.ResumeLayout(false);
            this.loadingOverlay.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button StartTestingButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel loadingOverlay;
        private System.Windows.Forms.Label loadingLabel;
        private System.Windows.Forms.ProgressBar loadingProgress;

        public void ShowLoading()
        {
            loadingOverlay.Visible = true;
            loadingOverlay.BringToFront();
            System.Windows.Forms.Application.DoEvents(); // Fully qualify the Application.DoEvents() call
        }

        public void HideLoading()
        {
            loadingOverlay.Visible = false;
        }

        public void PerformLoadingWork()
        {
            // Show loading overlay
            ShowLoading();

            // ... perform loading work ...

            // Hide loading overlay
            HideLoading();
        }
    }
}

