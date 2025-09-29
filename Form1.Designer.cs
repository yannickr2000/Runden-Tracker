﻿namespace Rundenzeiten
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            raceDuration = new NumericUpDown();
            confirmDurationBtn = new Button();
            label1 = new Label();
            label2 = new Label();
            starterListBtn = new Button();
            starterListLabel = new Label();
            startRaceBtn = new Button();
            remainingTime = new Label();
            label3 = new Label();
            enterRoundBtn = new Button();
            roundResultLabel = new Label();
            resultGrid = new DataGridView();
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            startNumberInput = new TextBox();
            panel4 = new Panel();
            label4 = new Label();
            panel2 = new Panel();
            label5 = new Label();
            ((System.ComponentModel.ISupportInitialize)raceDuration).BeginInit();
            ((System.ComponentModel.ISupportInitialize)resultGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // raceDuration
            // 
            raceDuration.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            raceDuration.Location = new Point(29, 84);
            raceDuration.Margin = new Padding(4, 5, 4, 5);
            raceDuration.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            raceDuration.Name = "raceDuration";
            raceDuration.Size = new Size(160, 30);
            raceDuration.TabIndex = 0;
            // 
            // confirmDurationBtn
            // 
            confirmDurationBtn.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            confirmDurationBtn.Location = new Point(211, 68);
            confirmDurationBtn.Margin = new Padding(4, 5, 4, 5);
            confirmDurationBtn.Name = "confirmDurationBtn";
            confirmDurationBtn.Size = new Size(101, 46);
            confirmDurationBtn.TabIndex = 1;
            confirmDurationBtn.Text = "Bestätigen";
            confirmDurationBtn.UseVisualStyleBackColor = true;
            confirmDurationBtn.Click += confirmDurationBtn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(26, 55);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(166, 23);
            label1.TabIndex = 2;
            label1.Text = "Renndauer Minuten:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(26, 126);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(130, 23);
            label2.TabIndex = 5;
            label2.Text = "Startliste Laden:";
            // 
            // starterListBtn
            // 
            starterListBtn.Enabled = false;
            starterListBtn.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            starterListBtn.Location = new Point(211, 135);
            starterListBtn.Margin = new Padding(4, 5, 4, 5);
            starterListBtn.Name = "starterListBtn";
            starterListBtn.Size = new Size(101, 47);
            starterListBtn.TabIndex = 4;
            starterListBtn.Text = "Auswahl";
            starterListBtn.UseVisualStyleBackColor = true;
            starterListBtn.Click += starterListBtn_Click;
            // 
            // starterListLabel
            // 
            starterListLabel.AutoSize = true;
            starterListLabel.BorderStyle = BorderStyle.FixedSingle;
            starterListLabel.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            starterListLabel.Location = new Point(29, 155);
            starterListLabel.Margin = new Padding(4, 0, 4, 0);
            starterListLabel.Name = "starterListLabel";
            starterListLabel.Size = new Size(149, 27);
            starterListLabel.TabIndex = 6;
            starterListLabel.Text = "Nicht ausgewählt";
            // 
            // startRaceBtn
            // 
            startRaceBtn.Enabled = false;
            startRaceBtn.Location = new Point(949, 228);
            startRaceBtn.Margin = new Padding(4, 5, 4, 5);
            startRaceBtn.Name = "startRaceBtn";
            startRaceBtn.Size = new Size(205, 84);
            startRaceBtn.TabIndex = 7;
            startRaceBtn.Text = "Rennen starten";
            startRaceBtn.TextImageRelation = TextImageRelation.ImageAboveText;
            startRaceBtn.UseVisualStyleBackColor = true;
            startRaceBtn.Click += startRaceBtn_Click;
            // 
            // remainingTime
            // 
            remainingTime.AutoSize = true;
            remainingTime.Font = new Font("Microsoft Sans Serif", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            remainingTime.Location = new Point(958, 136);
            remainingTime.Margin = new Padding(4, 0, 4, 0);
            remainingTime.Name = "remainingTime";
            remainingTime.Size = new Size(184, 69);
            remainingTime.TabIndex = 8;
            remainingTime.Text = "60:00";
            remainingTime.Click += remainingTime_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(586, 123);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(231, 28);
            label3.TabIndex = 20;
            label3.Text = "Startnummern Eingabe";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // enterRoundBtn
            // 
            enterRoundBtn.Enabled = false;
            enterRoundBtn.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            enterRoundBtn.Location = new Point(709, 167);
            enterRoundBtn.Margin = new Padding(4, 5, 4, 5);
            enterRoundBtn.Name = "enterRoundBtn";
            enterRoundBtn.Size = new Size(152, 51);
            enterRoundBtn.TabIndex = 11;
            enterRoundBtn.Text = "Eintragen";
            enterRoundBtn.UseVisualStyleBackColor = true;
            enterRoundBtn.Click += enterRoundBtn_Click;
            // 
            // roundResultLabel
            // 
            roundResultLabel.AutoSize = true;
            roundResultLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            roundResultLabel.ForeColor = Color.Green;
            roundResultLabel.Location = new Point(586, 228);
            roundResultLabel.Margin = new Padding(4, 0, 4, 0);
            roundResultLabel.Name = "roundResultLabel";
            roundResultLabel.Size = new Size(0, 28);
            roundResultLabel.TabIndex = 12;
            // 
            // resultGrid
            // 
            resultGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resultGrid.Location = new Point(18, 406);
            resultGrid.Margin = new Padding(4, 5, 4, 5);
            resultGrid.Name = "resultGrid";
            resultGrid.RowHeadersWidth = 51;
            resultGrid.Size = new Size(1392, 1230);
            resultGrid.TabIndex = 14;
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(1172, 72);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(196, 202);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 15;
            pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ButtonHighlight;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(startNumberInput);
            panel1.Controls.Add(panel4);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(roundResultLabel);
            panel1.Controls.Add(remainingTime);
            panel1.Controls.Add(enterRoundBtn);
            panel1.Controls.Add(startRaceBtn);
            panel1.Controls.Add(label3);
            panel1.Location = new Point(18, 30);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(1392, 353);
            panel1.TabIndex = 16;
            // 
            // startNumberInput
            // 
            startNumberInput.BorderStyle = BorderStyle.FixedSingle;
            startNumberInput.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            startNumberInput.Location = new Point(529, 167);
            startNumberInput.Name = "startNumberInput";
            startNumberInput.Size = new Size(146, 51);
            startNumberInput.TabIndex = 21;
            startNumberInput.KeyDown += startNumberInput_KeyDown;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(raceDuration);
            panel4.Controls.Add(confirmDurationBtn);
            panel4.Controls.Add(label2);
            panel4.Controls.Add(label4);
            panel4.Controls.Add(starterListLabel);
            panel4.Controls.Add(starterListBtn);
            panel4.Controls.Add(label1);
            panel4.Location = new Point(15, 16);
            panel4.Margin = new Padding(3, 4, 3, 4);
            panel4.Name = "panel4";
            panel4.Size = new Size(319, 318);
            panel4.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            label4.Location = new Point(26, 17);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(166, 29);
            label4.TabIndex = 16;
            label4.Text = "Eingabefeld:";
            label4.Click += label4_Click;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlDark;
            panel2.Location = new Point(186, 404);
            panel2.Margin = new Padding(3, 4, 3, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(241, 165);
            panel2.TabIndex = 18;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BorderStyle = BorderStyle.FixedSingle;
            label5.Font = new Font("Microsoft Sans Serif", 40F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(417, 16);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(530, 78);
            label5.TabIndex = 17;
            label5.Text = "Runden-Tracker";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1451, 939);
            Controls.Add(panel1);
            Controls.Add(resultGrid);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Form1";
            Text = "Runden-Tracker";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)raceDuration).EndInit();
            ((System.ComponentModel.ISupportInitialize)resultGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.NumericUpDown raceDuration;
        private System.Windows.Forms.Button confirmDurationBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button starterListBtn;
        private System.Windows.Forms.Label starterListLabel;
        private System.Windows.Forms.Button startRaceBtn;
        private System.Windows.Forms.Label remainingTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button enterRoundBtn;
        private System.Windows.Forms.Label roundResultLabel;
        private System.Windows.Forms.DataGridView resultGrid;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private TextBox startNumberInput;
    }
}

