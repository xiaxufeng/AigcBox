using System;
using System.IO;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using BananaBox.Config;

namespace BananaBox
{
    public partial class SettingsForm : Form
    {
        private AppConfig _config;

        public SettingsForm(AppConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadConfig();
        }

        private void LoadConfig()
        {
            txtDuomiApiKey.Text = _config.Duomi_ApiKey;

            txtOSSEndpoint.Text = _config.Aliyun_OSS_Endpoint;
            txtOSSAccessKeyId.Text = _config.Aliyun_OSS_AccessKeyId;
            txtOSSAccessKeySecret.Text = _config.Aliyun_OSS_AccessKeySecret;
            txtOSSBucketName.Text = _config.Aliyun_OSS_BucketName;

            txtSavePath.Text = _config.SavePath;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _config.Duomi_ApiKey = txtDuomiApiKey.Text.Trim();

                _config.Aliyun_OSS_Endpoint = txtOSSEndpoint.Text.Trim();
                _config.Aliyun_OSS_AccessKeyId = txtOSSAccessKeyId.Text.Trim();
                _config.Aliyun_OSS_AccessKeySecret = txtOSSAccessKeySecret.Text.Trim();
                _config.Aliyun_OSS_BucketName = txtOSSBucketName.Text.Trim();

                _config.SavePath = txtSavePath.Text.Trim();
                if (string.IsNullOrWhiteSpace(_config.SavePath))
                {
                    _config.SavePath = "result";
                }

                _config.Save();

                MessageBox.Show("配置已保存！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void lnkDuomiTitle_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://duomiapi.com/user/register?cps=dhwJyHCh");
        }

        private void lnkOSSTitle_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.aliyun.com/product/oss?userCode=hmpznx49");
        }

        private void btnBrowseSavePath_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择文件保存目录";
                folderDialog.ShowNewFolderButton = true;

                // 如果已经有路径，尝试设置为初始路径
                if (!string.IsNullOrWhiteSpace(txtSavePath.Text))
                {
                    string fullPath = txtSavePath.Text;
                    if (!Path.IsPathRooted(fullPath))
                    {
                        fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullPath);
                    }
                    if (Directory.Exists(fullPath))
                    {
                        folderDialog.SelectedPath = fullPath;
                    }
                }

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    string basePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

                    // 尝试转换为相对路径
                    if (selectedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedPath = selectedPath.Substring(basePath.Length).TrimStart('\\');
                        if (string.IsNullOrEmpty(selectedPath))
                        {
                            selectedPath = ".";
                        }
                    }

                    txtSavePath.Text = selectedPath;
                }
            }
        }

        private async void btnCheckBalance_Click(object sender, EventArgs e)
        {
            string apiKey = txtDuomiApiKey.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("请先输入 API Key！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnCheckBalance.Enabled = false;
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", apiKey);

                    var response = await client.GetAsync("https://api.wike.cc/api/account/get");
                    var json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<WikeApiResponse>(json);
                        if (result != null && result.code == 200 && result.data != null)
                        {
                            double balancePoints;
                            string balanceDisplay = result.data.total;
                            if (double.TryParse(result.data.total, out balancePoints))
                            {
                                balanceDisplay = (balancePoints / 1000.0).ToString("F2") + " 元";
                            }

                            MessageBox.Show($"用户名: {result.data.name}\n余额: {balanceDisplay}", "查询成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"查询失败: {result?.msg ?? "未知错误"}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"请求失败: {response.StatusCode}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckBalance.Enabled = true;
            }
        }

        private class WikeApiResponse
        {
            public int code { get; set; }
            public string msg { get; set; }
            public WikeApiData data { get; set; }
        }

        private class WikeApiData
        {
            public string name { get; set; }
            public string total { get; set; }
            public string time { get; set; }
        }
    }
}
