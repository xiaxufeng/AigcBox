using System;
using System.Drawing;
using System.Windows.Forms;

namespace BananaBox
{
    internal class ApiKeyPromptDialog : Form
    {
        private const string RegisterUrl = "https://duomiapi.com/user/register?cps=dhwJyHCh";

        public ApiKeyPromptDialog(string message)
        {
            this.Text = "提示";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(360, 130);
            this.Font = new Font("Microsoft YaHei UI", 9F);

            var icon = new PictureBox
            {
                Image = SystemIcons.Information.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(32, 32),
                Location = new Point(16, 20),
            };

            var lbl = new Label
            {
                Text = message,
                AutoSize = false,
                Size = new Size(278, 52),
                Location = new Point(58, 14),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            var btnRegister = new Button
            {
                Text = "去注册",
                Size = new Size(80, 30),
                Location = new Point(116, 86),
                DialogResult = DialogResult.None,
            };
            btnRegister.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(RegisterUrl);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnOK = new Button
            {
                Text = "确定",
                Size = new Size(80, 30),
                Location = new Point(206, 86),
                DialogResult = DialogResult.OK,
            };

            this.AcceptButton = btnOK;
            this.Controls.AddRange(new Control[] { icon, lbl, btnRegister, btnOK });
        }
    }
}
