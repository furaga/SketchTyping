namespace TextInput
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.gestureComboBox = new System.Windows.Forms.ComboBox();
            this.loadButton = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.log = new System.Windows.Forms.Label();
            this.editButton = new System.Windows.Forms.Button();
            this.shiftCheckbox = new System.Windows.Forms.CheckBox();
            this.altCheckbox = new System.Windows.Forms.CheckBox();
            this.ctrlCheckbox = new System.Windows.Forms.CheckBox();
            this.keyCombobox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(12, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(56, 23);
            this.startButton.TabIndex = 11;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(74, 12);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(56, 23);
            this.stopButton.TabIndex = 12;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // gestureComboBox
            // 
            this.gestureComboBox.FormattingEnabled = true;
            this.gestureComboBox.Location = new System.Drawing.Point(12, 62);
            this.gestureComboBox.Name = "gestureComboBox";
            this.gestureComboBox.Size = new System.Drawing.Size(172, 20);
            this.gestureComboBox.TabIndex = 13;
            this.gestureComboBox.TextChanged += new System.EventHandler(this.gestureComboBox_TextChanged);
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(190, 59);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(49, 23);
            this.loadButton.TabIndex = 14;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.log);
            this.splitContainer1.Panel1.Controls.Add(this.editButton);
            this.splitContainer1.Panel1.Controls.Add(this.shiftCheckbox);
            this.splitContainer1.Panel1.Controls.Add(this.altCheckbox);
            this.splitContainer1.Panel1.Controls.Add(this.ctrlCheckbox);
            this.splitContainer1.Panel1.Controls.Add(this.keyCombobox);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.loadButton);
            this.splitContainer1.Panel1.Controls.Add(this.startButton);
            this.splitContainer1.Panel1.Controls.Add(this.gestureComboBox);
            this.splitContainer1.Panel1.Controls.Add(this.stopButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox);
            this.splitContainer1.Size = new System.Drawing.Size(307, 245);
            this.splitContainer1.SplitterDistance = 160;
            this.splitContainer1.TabIndex = 15;
            // 
            // log
            // 
            this.log.AutoSize = true;
            this.log.Location = new System.Drawing.Point(6, 145);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(35, 12);
            this.log.TabIndex = 26;
            this.log.Text = "label3";
            // 
            // editButton
            // 
            this.editButton.Location = new System.Drawing.Point(245, 59);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(49, 23);
            this.editButton.TabIndex = 25;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.editButton_Click);
            // 
            // shiftCheckbox
            // 
            this.shiftCheckbox.AutoSize = true;
            this.shiftCheckbox.Location = new System.Drawing.Point(136, 120);
            this.shiftCheckbox.Name = "shiftCheckbox";
            this.shiftCheckbox.Size = new System.Drawing.Size(48, 16);
            this.shiftCheckbox.TabIndex = 24;
            this.shiftCheckbox.Text = "Shift";
            this.shiftCheckbox.UseVisualStyleBackColor = true;
            this.shiftCheckbox.CheckedChanged += new System.EventHandler(this.shiftCheckbox_CheckedChanged);
            // 
            // altCheckbox
            // 
            this.altCheckbox.AutoSize = true;
            this.altCheckbox.Location = new System.Drawing.Point(185, 120);
            this.altCheckbox.Name = "altCheckbox";
            this.altCheckbox.Size = new System.Drawing.Size(39, 16);
            this.altCheckbox.TabIndex = 23;
            this.altCheckbox.Text = "Alt";
            this.altCheckbox.UseVisualStyleBackColor = true;
            this.altCheckbox.CheckedChanged += new System.EventHandler(this.altCheckbox_CheckedChanged);
            // 
            // ctrlCheckbox
            // 
            this.ctrlCheckbox.AutoSize = true;
            this.ctrlCheckbox.Checked = true;
            this.ctrlCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ctrlCheckbox.Location = new System.Drawing.Point(88, 120);
            this.ctrlCheckbox.Name = "ctrlCheckbox";
            this.ctrlCheckbox.Size = new System.Drawing.Size(43, 16);
            this.ctrlCheckbox.TabIndex = 22;
            this.ctrlCheckbox.Text = "Ctrl";
            this.ctrlCheckbox.UseVisualStyleBackColor = true;
            this.ctrlCheckbox.CheckedChanged += new System.EventHandler(this.ctrlCheckbox_CheckedChanged);
            // 
            // keyCombobox
            // 
            this.keyCombobox.FormattingEnabled = true;
            this.keyCombobox.Location = new System.Drawing.Point(230, 118);
            this.keyCombobox.Name = "keyCombobox";
            this.keyCombobox.Size = new System.Drawing.Size(64, 20);
            this.keyCombobox.TabIndex = 21;
            this.keyCombobox.Text = "X";
            this.keyCombobox.TextChanged += new System.EventHandler(this.keyCombobox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "認識開始ショートカットキー";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "ジェスチャファイル";
            // 
            // textBox
            // 
            this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox.Location = new System.Drawing.Point(0, 0);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(307, 81);
            this.textBox.TabIndex = 0;
            this.textBox.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 245);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.ComboBox gestureComboBox;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.CheckBox shiftCheckbox;
        private System.Windows.Forms.CheckBox altCheckbox;
        private System.Windows.Forms.CheckBox ctrlCheckbox;
        private System.Windows.Forms.ComboBox keyCombobox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox textBox;
        private System.Windows.Forms.Label log;
    }
}

