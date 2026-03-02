namespace BananaBox
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.lblDuomiApiKey = new System.Windows.Forms.Label();
            this.txtDuomiApiKey = new System.Windows.Forms.TextBox();
            this.btnCheckBalance = new System.Windows.Forms.Button();
            this.lnkDuomiTitle = new System.Windows.Forms.LinkLabel();
            this.lnkOSSTitle = new System.Windows.Forms.LinkLabel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblSavePath = new System.Windows.Forms.Label();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.btnBrowseSavePath = new System.Windows.Forms.Button();
            this.lblOSSEndpoint = new System.Windows.Forms.Label();
            this.txtOSSEndpoint = new System.Windows.Forms.TextBox();
            this.lblOSSAccessKeyId = new System.Windows.Forms.Label();
            this.txtOSSAccessKeyId = new System.Windows.Forms.TextBox();
            this.lblOSSAccessKeySecret = new System.Windows.Forms.Label();
            this.txtOSSAccessKeySecret = new System.Windows.Forms.TextBox();
            this.lblOSSBucketName = new System.Windows.Forms.Label();
            this.txtOSSBucketName = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            //
            // lblDuomiApiKey
            //
            this.lblDuomiApiKey.AutoSize = true;
            this.lblDuomiApiKey.Location = new System.Drawing.Point(20, 30);
            this.lblDuomiApiKey.Name = "lblDuomiApiKey";
            this.lblDuomiApiKey.Size = new System.Drawing.Size(95, 12);
            this.lblDuomiApiKey.TabIndex = 0;
            this.lblDuomiApiKey.Text = "多米 API Key:";
            //
            // txtDuomiApiKey
            //
            this.txtDuomiApiKey.Location = new System.Drawing.Point(130, 27);
            this.txtDuomiApiKey.Name = "txtDuomiApiKey";
            this.txtDuomiApiKey.Size = new System.Drawing.Size(240, 21);
            this.txtDuomiApiKey.TabIndex = 1;
            //
            // btnCheckBalance
            //
            this.btnCheckBalance.Location = new System.Drawing.Point(380, 25);
            this.btnCheckBalance.Name = "btnCheckBalance";
            this.btnCheckBalance.Size = new System.Drawing.Size(75, 23);
            this.btnCheckBalance.TabIndex = 6;
            this.btnCheckBalance.Text = "查询余额";
            this.btnCheckBalance.UseVisualStyleBackColor = true;
            this.btnCheckBalance.Click += new System.EventHandler(this.btnCheckBalance_Click);
            //
            // lnkDuomiTitle
            //
            this.lnkDuomiTitle.AutoSize = true;
            this.lnkDuomiTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkDuomiTitle.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkDuomiTitle.Location = new System.Drawing.Point(8, 5);
            this.lnkDuomiTitle.Name = "lnkDuomiTitle";
            this.lnkDuomiTitle.TabIndex = 20;
            this.lnkDuomiTitle.TabStop = false;
            this.lnkDuomiTitle.Text = "多米 API 配置";
            this.lnkDuomiTitle.Click += new System.EventHandler(this.lnkDuomiTitle_Click);
            //
            // lnkOSSTitle
            //
            this.lnkOSSTitle.AutoSize = true;
            this.lnkOSSTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkOSSTitle.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkOSSTitle.Location = new System.Drawing.Point(8, 5);
            this.lnkOSSTitle.Name = "lnkOSSTitle";
            this.lnkOSSTitle.TabIndex = 21;
            this.lnkOSSTitle.TabStop = false;
            this.lnkOSSTitle.Text = "阿里云 OSS 配置（用于上传参考图）";
            this.lnkOSSTitle.Click += new System.EventHandler(this.lnkOSSTitle_Click);
            //
            // groupBox2 - 阿里云 OSS 配置
            //
            this.groupBox2.Controls.Add(this.lnkOSSTitle);
            this.groupBox2.Controls.Add(this.lblOSSEndpoint);
            this.groupBox2.Controls.Add(this.txtOSSEndpoint);
            this.groupBox2.Controls.Add(this.lblOSSAccessKeyId);
            this.groupBox2.Controls.Add(this.txtOSSAccessKeyId);
            this.groupBox2.Controls.Add(this.lblOSSAccessKeySecret);
            this.groupBox2.Controls.Add(this.txtOSSAccessKeySecret);
            this.groupBox2.Controls.Add(this.lblOSSBucketName);
            this.groupBox2.Controls.Add(this.txtOSSBucketName);
            this.groupBox2.Location = new System.Drawing.Point(12, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(470, 180);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "";
            //
            // lblOSSEndpoint
            //
            this.lblOSSEndpoint.AutoSize = true;
            this.lblOSSEndpoint.Location = new System.Drawing.Point(20, 30);
            this.lblOSSEndpoint.Name = "lblOSSEndpoint";
            this.lblOSSEndpoint.Size = new System.Drawing.Size(95, 12);
            this.lblOSSEndpoint.TabIndex = 0;
            this.lblOSSEndpoint.Text = "Endpoint:";
            //
            // txtOSSEndpoint
            //
            this.txtOSSEndpoint.Location = new System.Drawing.Point(130, 27);
            this.txtOSSEndpoint.Name = "txtOSSEndpoint";
            this.txtOSSEndpoint.Size = new System.Drawing.Size(320, 21);
            this.txtOSSEndpoint.TabIndex = 1;
            //
            // lblOSSAccessKeyId
            //
            this.lblOSSAccessKeyId.AutoSize = true;
            this.lblOSSAccessKeyId.Location = new System.Drawing.Point(20, 65);
            this.lblOSSAccessKeyId.Name = "lblOSSAccessKeyId";
            this.lblOSSAccessKeyId.Size = new System.Drawing.Size(95, 12);
            this.lblOSSAccessKeyId.TabIndex = 2;
            this.lblOSSAccessKeyId.Text = "AccessKeyId:";
            //
            // txtOSSAccessKeyId
            //
            this.txtOSSAccessKeyId.Location = new System.Drawing.Point(130, 62);
            this.txtOSSAccessKeyId.Name = "txtOSSAccessKeyId";
            this.txtOSSAccessKeyId.Size = new System.Drawing.Size(320, 21);
            this.txtOSSAccessKeyId.TabIndex = 3;
            //
            // lblOSSAccessKeySecret
            //
            this.lblOSSAccessKeySecret.AutoSize = true;
            this.lblOSSAccessKeySecret.Location = new System.Drawing.Point(20, 100);
            this.lblOSSAccessKeySecret.Name = "lblOSSAccessKeySecret";
            this.lblOSSAccessKeySecret.Size = new System.Drawing.Size(113, 12);
            this.lblOSSAccessKeySecret.TabIndex = 4;
            this.lblOSSAccessKeySecret.Text = "AccessKeySecret:";
            //
            // txtOSSAccessKeySecret
            //
            this.txtOSSAccessKeySecret.Location = new System.Drawing.Point(130, 97);
            this.txtOSSAccessKeySecret.Name = "txtOSSAccessKeySecret";
            this.txtOSSAccessKeySecret.Size = new System.Drawing.Size(320, 21);
            this.txtOSSAccessKeySecret.TabIndex = 5;
            this.txtOSSAccessKeySecret.UseSystemPasswordChar = true;
            //
            // lblOSSBucketName
            //
            this.lblOSSBucketName.AutoSize = true;
            this.lblOSSBucketName.Location = new System.Drawing.Point(20, 135);
            this.lblOSSBucketName.Name = "lblOSSBucketName";
            this.lblOSSBucketName.Size = new System.Drawing.Size(95, 12);
            this.lblOSSBucketName.TabIndex = 6;
            this.lblOSSBucketName.Text = "BucketName:";
            //
            // txtOSSBucketName
            //
            this.txtOSSBucketName.Location = new System.Drawing.Point(130, 132);
            this.txtOSSBucketName.Name = "txtOSSBucketName";
            this.txtOSSBucketName.Size = new System.Drawing.Size(320, 21);
            this.txtOSSBucketName.TabIndex = 7;
            //
            // btnSave
            //
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(270, 375);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.BackColor = System.Drawing.Color.Gray;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(370, 375);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.lnkDuomiTitle);
            this.groupBox1.Controls.Add(this.lblDuomiApiKey);
            this.groupBox1.Controls.Add(this.txtDuomiApiKey);
            this.groupBox1.Controls.Add(this.btnCheckBalance);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 80);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "";
            //
            // groupBox3 - 通用配置
            //
            this.groupBox3.Controls.Add(this.lblSavePath);
            this.groupBox3.Controls.Add(this.txtSavePath);
            this.groupBox3.Controls.Add(this.btnBrowseSavePath);
            this.groupBox3.Location = new System.Drawing.Point(12, 295);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(470, 70);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "通用配置";
            //
            // lblSavePath
            //
            this.lblSavePath.AutoSize = true;
            this.lblSavePath.Location = new System.Drawing.Point(20, 30);
            this.lblSavePath.Name = "lblSavePath";
            this.lblSavePath.Size = new System.Drawing.Size(95, 12);
            this.lblSavePath.TabIndex = 0;
            this.lblSavePath.Text = "文件保存路径:";
            //
            // txtSavePath
            //
            this.txtSavePath.Location = new System.Drawing.Point(130, 27);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(260, 21);
            this.txtSavePath.TabIndex = 1;
            //
            // btnBrowseSavePath
            //
            this.btnBrowseSavePath.Location = new System.Drawing.Point(400, 26);
            this.btnBrowseSavePath.Name = "btnBrowseSavePath";
            this.btnBrowseSavePath.Size = new System.Drawing.Size(50, 23);
            this.btnBrowseSavePath.TabIndex = 2;
            this.btnBrowseSavePath.Text = "浏览...";
            this.btnBrowseSavePath.UseVisualStyleBackColor = true;
            this.btnBrowseSavePath.Click += new System.EventHandler(this.btnBrowseSavePath_Click);
            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 425);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "API 设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label lblDuomiApiKey;
        private System.Windows.Forms.TextBox txtDuomiApiKey;
        private System.Windows.Forms.Button btnCheckBalance;
        private System.Windows.Forms.LinkLabel lnkDuomiTitle;
        private System.Windows.Forms.LinkLabel lnkOSSTitle;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblOSSEndpoint;
        private System.Windows.Forms.TextBox txtOSSEndpoint;
        private System.Windows.Forms.Label lblOSSAccessKeyId;
        private System.Windows.Forms.TextBox txtOSSAccessKeyId;
        private System.Windows.Forms.Label lblOSSAccessKeySecret;
        private System.Windows.Forms.TextBox txtOSSAccessKeySecret;
        private System.Windows.Forms.Label lblOSSBucketName;
        private System.Windows.Forms.TextBox txtOSSBucketName;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblSavePath;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Button btnBrowseSavePath;
    }
}
