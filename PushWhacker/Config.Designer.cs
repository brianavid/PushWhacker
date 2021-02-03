
namespace PushWhacker
{
    partial class Config
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
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxOctave = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxLayout = new System.Windows.Forms.ComboBox();
            this.comboBoxKey = new System.Windows.Forms.ComboBox();
            this.comboBoxScale = new System.Windows.Forms.ComboBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxKeyChangeAmount = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBoxPadStartNote = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxTouchStrip = new System.Windows.Forms.ComboBox();
            this.comboBoxPressure = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.buttonCalibrateToe = new System.Windows.Forms.Button();
            this.buttonCalibrateHeel = new System.Windows.Forms.Button();
            this.buttonCalibrateDown = new System.Windows.Forms.Button();
            this.buttonCalibrateUp = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxPedal = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBoxUserModeOnly = new System.Windows.Forms.CheckBox();
            this.comboBoxOutput = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.buttonColHi = new System.Windows.Forms.Button();
            this.buttonColLo = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(307, 186);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(44, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(354, 169);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.comboBoxOctave);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.comboBoxLayout);
            this.tabPage1.Controls.Add(this.comboBoxKey);
            this.tabPage1.Controls.Add(this.comboBoxScale);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(346, 143);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Pads";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(274, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Oct";
            // 
            // comboBoxOctave
            // 
            this.comboBoxOctave.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOctave.FormattingEnabled = true;
            this.comboBoxOctave.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.comboBoxOctave.Location = new System.Drawing.Point(298, 42);
            this.comboBoxOctave.Name = "comboBoxOctave";
            this.comboBoxOctave.Size = new System.Drawing.Size(30, 21);
            this.comboBoxOctave.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Layout";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Scale";
            // 
            // comboBoxLayout
            // 
            this.comboBoxLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLayout.FormattingEnabled = true;
            this.comboBoxLayout.Location = new System.Drawing.Point(71, 10);
            this.comboBoxLayout.Name = "comboBoxLayout";
            this.comboBoxLayout.Size = new System.Drawing.Size(257, 21);
            this.comboBoxLayout.TabIndex = 20;
            // 
            // comboBoxKey
            // 
            this.comboBoxKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKey.FormattingEnabled = true;
            this.comboBoxKey.Location = new System.Drawing.Point(71, 43);
            this.comboBoxKey.MaxDropDownItems = 12;
            this.comboBoxKey.Name = "comboBoxKey";
            this.comboBoxKey.Size = new System.Drawing.Size(44, 21);
            this.comboBoxKey.TabIndex = 22;
            // 
            // comboBoxScale
            // 
            this.comboBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScale.FormattingEnabled = true;
            this.comboBoxScale.Location = new System.Drawing.Point(124, 43);
            this.comboBoxScale.Name = "comboBoxScale";
            this.comboBoxScale.Size = new System.Drawing.Size(151, 21);
            this.comboBoxScale.TabIndex = 24;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.label12);
            this.tabPage5.Controls.Add(this.comboBoxKeyChangeAmount);
            this.tabPage5.Controls.Add(this.label11);
            this.tabPage5.Controls.Add(this.comboBoxPadStartNote);
            this.tabPage5.Controls.Add(this.label7);
            this.tabPage5.Controls.Add(this.comboBoxTouchStrip);
            this.tabPage5.Controls.Add(this.comboBoxPressure);
            this.tabPage5.Controls.Add(this.label6);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(346, 143);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Controls";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 50);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 13);
            this.label12.TabIndex = 47;
            this.label12.Text = "Key Change";
            // 
            // comboBoxKeyChangeAmount
            // 
            this.comboBoxKeyChangeAmount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKeyChangeAmount.FormattingEnabled = true;
            this.comboBoxKeyChangeAmount.Items.AddRange(new object[] {
            "Sequentially by Semitone",
            "Circle of Fifths"});
            this.comboBoxKeyChangeAmount.Location = new System.Drawing.Point(84, 42);
            this.comboBoxKeyChangeAmount.Name = "comboBoxKeyChangeAmount";
            this.comboBoxKeyChangeAmount.Size = new System.Drawing.Size(251, 21);
            this.comboBoxKeyChangeAmount.TabIndex = 46;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(1, 23);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 13);
            this.label11.TabIndex = 45;
            this.label11.Text = "Pad Start Note";
            // 
            // comboBoxPadStartNote
            // 
            this.comboBoxPadStartNote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPadStartNote.FormattingEnabled = true;
            this.comboBoxPadStartNote.Items.AddRange(new object[] {
            "Root note of current key or scale",
            "Fixed at C or nearest in-key note"});
            this.comboBoxPadStartNote.Location = new System.Drawing.Point(84, 15);
            this.comboBoxPadStartNote.Name = "comboBoxPadStartNote";
            this.comboBoxPadStartNote.Size = new System.Drawing.Size(251, 21);
            this.comboBoxPadStartNote.TabIndex = 44;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 104);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 39;
            this.label7.Text = "Touch Strip";
            // 
            // comboBoxTouchStrip
            // 
            this.comboBoxTouchStrip.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTouchStrip.FormattingEnabled = true;
            this.comboBoxTouchStrip.Location = new System.Drawing.Point(84, 96);
            this.comboBoxTouchStrip.Name = "comboBoxTouchStrip";
            this.comboBoxTouchStrip.Size = new System.Drawing.Size(251, 21);
            this.comboBoxTouchStrip.TabIndex = 38;
            // 
            // comboBoxPressure
            // 
            this.comboBoxPressure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPressure.FormattingEnabled = true;
            this.comboBoxPressure.Location = new System.Drawing.Point(84, 69);
            this.comboBoxPressure.Name = "comboBoxPressure";
            this.comboBoxPressure.Size = new System.Drawing.Size(251, 21);
            this.comboBoxPressure.TabIndex = 37;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 77);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "Pressure";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.buttonCalibrateToe);
            this.tabPage4.Controls.Add(this.buttonCalibrateHeel);
            this.tabPage4.Controls.Add(this.buttonCalibrateDown);
            this.tabPage4.Controls.Add(this.buttonCalibrateUp);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.label5);
            this.tabPage4.Controls.Add(this.comboBoxPedal);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(346, 143);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Foot Pedal";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 116);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(318, 13);
            this.label10.TabIndex = 41;
            this.label10.Text = "For a controller, press Heel then Toe with the pedal at its extremes";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 99);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(309, 13);
            this.label9.TabIndex = 40;
            this.label9.Text = "For a switch, press Up with the pedal Up and Down with it down";
            // 
            // buttonCalibrateToe
            // 
            this.buttonCalibrateToe.Location = new System.Drawing.Point(204, 67);
            this.buttonCalibrateToe.Name = "buttonCalibrateToe";
            this.buttonCalibrateToe.Size = new System.Drawing.Size(75, 23);
            this.buttonCalibrateToe.TabIndex = 39;
            this.buttonCalibrateToe.Text = "Toe";
            this.buttonCalibrateToe.UseVisualStyleBackColor = true;
            this.buttonCalibrateToe.Click += new System.EventHandler(this.buttonCalibrateToe_Click);
            // 
            // buttonCalibrateHeel
            // 
            this.buttonCalibrateHeel.Location = new System.Drawing.Point(123, 68);
            this.buttonCalibrateHeel.Name = "buttonCalibrateHeel";
            this.buttonCalibrateHeel.Size = new System.Drawing.Size(75, 23);
            this.buttonCalibrateHeel.TabIndex = 38;
            this.buttonCalibrateHeel.Text = "Heel";
            this.buttonCalibrateHeel.UseVisualStyleBackColor = true;
            this.buttonCalibrateHeel.Click += new System.EventHandler(this.buttonCalibrateHeel_Click);
            // 
            // buttonCalibrateDown
            // 
            this.buttonCalibrateDown.Location = new System.Drawing.Point(204, 38);
            this.buttonCalibrateDown.Name = "buttonCalibrateDown";
            this.buttonCalibrateDown.Size = new System.Drawing.Size(75, 23);
            this.buttonCalibrateDown.TabIndex = 37;
            this.buttonCalibrateDown.Text = "Down";
            this.buttonCalibrateDown.UseVisualStyleBackColor = true;
            this.buttonCalibrateDown.Click += new System.EventHandler(this.buttonCalibrateDown_Click);
            // 
            // buttonCalibrateUp
            // 
            this.buttonCalibrateUp.Location = new System.Drawing.Point(123, 38);
            this.buttonCalibrateUp.Name = "buttonCalibrateUp";
            this.buttonCalibrateUp.Size = new System.Drawing.Size(75, 23);
            this.buttonCalibrateUp.TabIndex = 36;
            this.buttonCalibrateUp.Text = "Up";
            this.buttonCalibrateUp.UseVisualStyleBackColor = true;
            this.buttonCalibrateUp.Click += new System.EventHandler(this.buttonCalibrateUp_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 13);
            this.label8.TabIndex = 35;
            this.label8.Text = "Calibrate:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 34;
            this.label5.Text = "Mode";
            // 
            // comboBoxPedal
            // 
            this.comboBoxPedal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPedal.FormattingEnabled = true;
            this.comboBoxPedal.Location = new System.Drawing.Point(74, 6);
            this.comboBoxPedal.Name = "comboBoxPedal";
            this.comboBoxPedal.Size = new System.Drawing.Size(204, 21);
            this.comboBoxPedal.TabIndex = 33;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.checkBoxUserModeOnly);
            this.tabPage2.Controls.Add(this.comboBoxOutput);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(346, 143);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Midi";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBoxUserModeOnly
            // 
            this.checkBoxUserModeOnly.AutoSize = true;
            this.checkBoxUserModeOnly.Location = new System.Drawing.Point(9, 64);
            this.checkBoxUserModeOnly.Name = "checkBoxUserModeOnly";
            this.checkBoxUserModeOnly.Size = new System.Drawing.Size(229, 17);
            this.checkBoxUserModeOnly.TabIndex = 22;
            this.checkBoxUserModeOnly.Text = "Shared with Ableton Live (User Mode Only)";
            this.checkBoxUserModeOnly.UseVisualStyleBackColor = true;
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Location = new System.Drawing.Point(72, 17);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(268, 21);
            this.comboBoxOutput.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Output Midi";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.buttonColHi);
            this.tabPage3.Controls.Add(this.buttonColLo);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(346, 143);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Development";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // buttonColHi
            // 
            this.buttonColHi.Location = new System.Drawing.Point(87, 6);
            this.buttonColHi.Name = "buttonColHi";
            this.buttonColHi.Size = new System.Drawing.Size(75, 23);
            this.buttonColHi.TabIndex = 104;
            this.buttonColHi.Text = "Colours Hi";
            this.buttonColHi.UseVisualStyleBackColor = true;
            this.buttonColHi.Click += new System.EventHandler(this.buttonColHi_Click);
            // 
            // buttonColLo
            // 
            this.buttonColLo.Location = new System.Drawing.Point(6, 6);
            this.buttonColLo.Name = "buttonColLo";
            this.buttonColLo.Size = new System.Drawing.Size(75, 23);
            this.buttonColLo.TabIndex = 103;
            this.buttonColLo.Text = "Colours Lo";
            this.buttonColLo.UseVisualStyleBackColor = true;
            this.buttonColLo.Click += new System.EventHandler(this.buttonColLo_Click);
            // 
            // Config
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 221);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Config";
            this.Text = "Config";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxOctave;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxLayout;
        private System.Windows.Forms.ComboBox comboBoxKey;
        private System.Windows.Forms.ComboBox comboBoxScale;
        private System.Windows.Forms.ComboBox comboBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button buttonColHi;
        private System.Windows.Forms.Button buttonColLo;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button buttonCalibrateHeel;
        private System.Windows.Forms.Button buttonCalibrateDown;
        private System.Windows.Forms.Button buttonCalibrateUp;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxPedal;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonCalibrateToe;
        private System.Windows.Forms.CheckBox checkBoxUserModeOnly;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxTouchStrip;
        private System.Windows.Forms.ComboBox comboBoxPressure;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBoxPadStartNote;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBoxKeyChangeAmount;
    }
}