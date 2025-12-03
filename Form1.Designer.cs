using System;
using System.Windows.Forms;
using System.Drawing;

namespace Rundenzeiten
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
            this.raceDuration = new System.Windows.Forms.NumericUpDown();
            this.confirmDurationBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.starterListBtn = new System.Windows.Forms.Button();
            this.starterListLabel = new System.Windows.Forms.Label();
            this.startRaceBtn = new System.Windows.Forms.Button();
            this.remainingTime = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.enterRoundBtn = new System.Windows.Forms.Button();
            this.roundResultLabel = new System.Windows.Forms.Label();
            this.resultGrid = new System.Windows.Forms.DataGridView();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.classStatusLabel = new System.Windows.Forms.Label();
            this.classMultiList = new System.Windows.Forms.CheckedListBox();
            this.startClassBtn = new System.Windows.Forms.Button();
            this.startNumberInput = new System.Windows.Forms.TextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.raceNameComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.raceDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resultGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // raceDuration
            // 
            this.raceDuration.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.raceDuration.Location = new System.Drawing.Point(17, 178);
            this.raceDuration.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.raceDuration.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.raceDuration.Name = "raceDuration";
            this.raceDuration.Size = new System.Drawing.Size(160, 30);
            this.raceDuration.TabIndex = 0;
            // 
            // confirmDurationBtn
            // 
            this.confirmDurationBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.confirmDurationBtn.Location = new System.Drawing.Point(190, 150);
            this.confirmDurationBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.confirmDurationBtn.Name = "confirmDurationBtn";
            this.confirmDurationBtn.Size = new System.Drawing.Size(110, 58);
            this.confirmDurationBtn.TabIndex = 1;
            this.confirmDurationBtn.Text = "Bestätigen";
            this.confirmDurationBtn.UseVisualStyleBackColor = true;
            this.confirmDurationBtn.Click += new System.EventHandler(this.confirmDurationBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(17, 150);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Renndauer Minuten:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 226);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Startliste Laden:";
            // 
            // starterListBtn
            // 
            this.starterListBtn.Enabled = false;
            this.starterListBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.starterListBtn.Location = new System.Drawing.Point(190, 226);
            this.starterListBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.starterListBtn.Name = "starterListBtn";
            this.starterListBtn.Size = new System.Drawing.Size(110, 59);
            this.starterListBtn.TabIndex = 4;
            this.starterListBtn.Text = "Auswahl";
            this.starterListBtn.UseVisualStyleBackColor = true;
            this.starterListBtn.Click += new System.EventHandler(this.starterListBtn_Click);
            // 
            // starterListLabel
            // 
            this.starterListLabel.AutoSize = true;
            this.starterListLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.starterListLabel.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.starterListLabel.Location = new System.Drawing.Point(17, 258);
            this.starterListLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.starterListLabel.Name = "starterListLabel";
            this.starterListLabel.Size = new System.Drawing.Size(149, 27);
            this.starterListLabel.TabIndex = 6;
            this.starterListLabel.Text = "Nicht ausgewählt";
            // 
            // startRaceBtn
            // 
            this.startRaceBtn.Enabled = false;
            this.startRaceBtn.Location = new System.Drawing.Point(949, 123);
            this.startRaceBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.startRaceBtn.Name = "startRaceBtn";
            this.startRaceBtn.Size = new System.Drawing.Size(205, 51);
            this.startRaceBtn.TabIndex = 7;
            this.startRaceBtn.Text = "Rennen starten";
            this.startRaceBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.startRaceBtn.UseVisualStyleBackColor = true;
            this.startRaceBtn.Click += new System.EventHandler(this.startRaceBtn_Click);
            // 
            // remainingTime
            // 
            this.remainingTime.AutoSize = true;
            this.remainingTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.remainingTime.Location = new System.Drawing.Point(970, 22);
            this.remainingTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.remainingTime.Name = "remainingTime";
            this.remainingTime.Size = new System.Drawing.Size(184, 69);
            this.remainingTime.TabIndex = 8;
            this.remainingTime.Text = "60:00";
            this.remainingTime.Click += new System.EventHandler(this.remainingTime_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(586, 123);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(231, 28);
            this.label3.TabIndex = 20;
            this.label3.Text = "Startnummern Eingabe";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // enterRoundBtn
            // 
            this.enterRoundBtn.Enabled = false;
            this.enterRoundBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enterRoundBtn.Location = new System.Drawing.Point(709, 167);
            this.enterRoundBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.enterRoundBtn.Name = "enterRoundBtn";
            this.enterRoundBtn.Size = new System.Drawing.Size(152, 51);
            this.enterRoundBtn.TabIndex = 11;
            this.enterRoundBtn.Text = "Eintragen";
            this.enterRoundBtn.UseVisualStyleBackColor = true;
            this.enterRoundBtn.Click += new System.EventHandler(this.enterRoundBtn_Click);
            // 
            // roundResultLabel
            // 
            this.roundResultLabel.AutoSize = true;
            this.roundResultLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundResultLabel.ForeColor = System.Drawing.Color.Green;
            this.roundResultLabel.Location = new System.Drawing.Point(586, 228);
            this.roundResultLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.roundResultLabel.Name = "roundResultLabel";
            this.roundResultLabel.Size = new System.Drawing.Size(0, 28);
            this.roundResultLabel.TabIndex = 12;
            // 
            // resultGrid
            // 
            this.resultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultGrid.Location = new System.Drawing.Point(18, 401);
            this.resultGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.resultGrid.Name = "resultGrid";
            this.resultGrid.RowHeadersWidth = 51;
            this.resultGrid.RowTemplate.Height = 24;
            this.resultGrid.Size = new System.Drawing.Size(1392, 523);
            this.resultGrid.TabIndex = 14;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(1178, 16);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(196, 202);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.classStatusLabel);
            this.panel1.Controls.Add(this.classMultiList);
            this.panel1.Controls.Add(this.startClassBtn);
            this.panel1.Controls.Add(this.startNumberInput);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.roundResultLabel);
            this.panel1.Controls.Add(this.remainingTime);
            this.panel1.Controls.Add(this.enterRoundBtn);
            this.panel1.Controls.Add(this.startRaceBtn);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Location = new System.Drawing.Point(18, 30);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1392, 353);
            this.panel1.TabIndex = 16;
            // 
            // classStatusLabel
            // 
            this.classStatusLabel.AutoSize = true;
            this.classStatusLabel.Location = new System.Drawing.Point(1178, 236);
            this.classStatusLabel.Name = "classStatusLabel";
            this.classStatusLabel.Size = new System.Drawing.Size(0, 20);
            this.classStatusLabel.TabIndex = 25;
            this.classStatusLabel.Click += new System.EventHandler(this.classStatusLabel_Click);
            // 
            // classMultiList
            // 
            this.classMultiList.Enabled = false;
            this.classMultiList.FormattingEnabled = true;
            this.classMultiList.Location = new System.Drawing.Point(949, 238);
            this.classMultiList.Name = "classMultiList";
            this.classMultiList.Size = new System.Drawing.Size(205, 92);
            this.classMultiList.TabIndex = 24;
            // 
            // startClassBtn
            // 
            this.startClassBtn.Enabled = false;
            this.startClassBtn.Location = new System.Drawing.Point(949, 188);
            this.startClassBtn.Name = "startClassBtn";
            this.startClassBtn.Size = new System.Drawing.Size(205, 37);
            this.startClassBtn.TabIndex = 22;
            this.startClassBtn.Text = "Klassen Starten";
            this.startClassBtn.UseVisualStyleBackColor = true;
            this.startClassBtn.Click += new System.EventHandler(this.startClassBtn_Click);
            // 
            // startNumberInput
            // 
            this.startNumberInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.startNumberInput.Font = new System.Drawing.Font("Segoe UI", 19.8000011F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startNumberInput.Location = new System.Drawing.Point(529, 167);
            this.startNumberInput.Name = "startNumberInput";
            this.startNumberInput.Size = new System.Drawing.Size(146, 51);
            this.startNumberInput.TabIndex = 21;
            this.startNumberInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.startNumberInput_KeyDown);
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.label6);
            this.panel4.Controls.Add(this.raceNameComboBox);
            this.panel4.Controls.Add(this.raceDuration);
            this.panel4.Controls.Add(this.confirmDurationBtn);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.starterListLabel);
            this.panel4.Controls.Add(this.starterListBtn);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Location = new System.Drawing.Point(15, 16);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(319, 318);
            this.panel4.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(17, 76);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 23);
            this.label6.TabIndex = 18;
            this.label6.Text = "Veranstaltung:";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // raceNameComboBox
            // 
            this.raceNameComboBox.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.raceNameComboBox.FormattingEnabled = true;
            this.raceNameComboBox.Location = new System.Drawing.Point(17, 102);
            this.raceNameComboBox.Name = "raceNameComboBox";
            this.raceNameComboBox.Size = new System.Drawing.Size(283, 39);
            this.raceNameComboBox.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(17, 17);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(166, 29);
            this.label4.TabIndex = 16;
            this.label4.Text = "Eingabefeld:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel2.Location = new System.Drawing.Point(186, 404);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(241, 165);
            this.panel2.TabIndex = 18;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(417, 16);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(530, 78);
            this.label5.TabIndex = 17;
            this.label5.Text = "Runden-Tracker";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1451, 939);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.resultGrid);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Runden-Tracker";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.raceDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resultGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TextBox startNumberInput;
        private System.Windows.Forms.ComboBox raceNameComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button startClassBtn;
        private System.Windows.Forms.CheckedListBox classMultiList;
        private System.Windows.Forms.Label classStatusLabel;
    }
}
