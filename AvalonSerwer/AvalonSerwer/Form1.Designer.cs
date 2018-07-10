namespace AvalonSerwer
{
    partial class Serwer
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ServerStartButton = new System.Windows.Forms.Button();
            this.ServerStopButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(74, 6);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(100, 20);
            this.PortTextBox.TabIndex = 0;
            this.PortTextBox.Text = "33454";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port:";
            // 
            // ServerStartButton
            // 
            this.ServerStartButton.Location = new System.Drawing.Point(53, 33);
            this.ServerStartButton.Name = "ServerStartButton";
            this.ServerStartButton.Size = new System.Drawing.Size(100, 23);
            this.ServerStartButton.TabIndex = 2;
            this.ServerStartButton.Text = "Uruchom Serwer";
            this.ServerStartButton.UseVisualStyleBackColor = true;
            this.ServerStartButton.Click += new System.EventHandler(this.ServerStartButton_Click);
            // 
            // ServerStopButton
            // 
            this.ServerStopButton.Location = new System.Drawing.Point(53, 62);
            this.ServerStopButton.Name = "ServerStopButton";
            this.ServerStopButton.Size = new System.Drawing.Size(100, 23);
            this.ServerStopButton.TabIndex = 3;
            this.ServerStopButton.Text = "Zatrzymaj Serwer";
            this.ServerStopButton.UseVisualStyleBackColor = true;
            this.ServerStopButton.Visible = false;
            this.ServerStopButton.Click += new System.EventHandler(this.ServerStopButton_Click);
            // 
            // Serwer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 94);
            this.Controls.Add(this.ServerStopButton);
            this.Controls.Add(this.ServerStartButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PortTextBox);
            this.Name = "Serwer";
            this.Text = "Avalon Serwer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Serwer_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ServerStartButton;
        private System.Windows.Forms.Button ServerStopButton;
    }
}

