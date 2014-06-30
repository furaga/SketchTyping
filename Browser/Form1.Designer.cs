namespace Browser
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
            this.editButton = new System.Windows.Forms.Button();
            this.thresholdNumeriUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.RichTextBox();
            this.log = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.thresholdNumeriUpDown)).BeginInit();
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
            this.gestureComboBox.Location = new System.Drawing.Point(12, 85);
            this.gestureComboBox.Name = "gestureComboBox";
            this.gestureComboBox.Size = new System.Drawing.Size(190, 20);
            this.gestureComboBox.TabIndex = 13;
            this.gestureComboBox.TextChanged += new System.EventHandler(this.gestureComboBox_TextChanged);
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(208, 82);
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
            this.splitContainer1.Panel1.Controls.Add(this.thresholdNumeriUpDown);
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
            this.splitContainer1.Size = new System.Drawing.Size(344, 297);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 15;
            // 
            // editButton
            // 
            this.editButton.Location = new System.Drawing.Point(263, 82);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(49, 23);
            this.editButton.TabIndex = 25;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.editButton_Click);
            // 
            // thresholdNumeriUpDown
            // 
            this.thresholdNumeriUpDown.Location = new System.Drawing.Point(163, 122);
            this.thresholdNumeriUpDown.Name = "thresholdNumeriUpDown";
            this.thresholdNumeriUpDown.Size = new System.Drawing.Size(148, 19);
            this.thresholdNumeriUpDown.TabIndex = 19;
            this.thresholdNumeriUpDown.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.thresholdNumeriUpDown.ValueChanged += new System.EventHandler(this.thresholdNumeriUpDown_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "ジェスチャi一致度の閾値（％）";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 70);
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
            this.textBox.Size = new System.Drawing.Size(344, 93);
            this.textBox.TabIndex = 0;
            this.textBox.Text = "";
            // 
            // log
            // 
            this.log.AutoSize = true;
            this.log.Location = new System.Drawing.Point(3, 185);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(20, 12);
            this.log.TabIndex = 26;
            this.log.Text = "log";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 297);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "BrowserBySketchTyping";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.thresholdNumeriUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.ComboBox gestureComboBox;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.NumericUpDown thresholdNumeriUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox textBox;
        private System.Windows.Forms.Label log;
    }
}

