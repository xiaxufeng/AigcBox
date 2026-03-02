using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BananaBox.Config;
using BananaBox.Data;
using BananaBox.Models;
using BananaBox.Services;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;

namespace BananaBox
{
    public partial class Form1 : Form
    {
        private AppConfig _config;
        private DuomiApiService _duomiService;
        private List<string> _referenceImagePaths = new List<string>();
        private List<string> _existingRefOssUrls = new List<string>(); // 重新编辑时复用的已有 OSS URL
        private bool _isGenerating = false;
        private Timer _progressTimer;
        private Microsoft.Web.WebView2.WinForms.WebView2 _webView;
        private bool _webViewReady = false;

        public Form1()
        {
            InitializeComponent();
            _progressTimer = new Timer();
            _progressTimer.Interval = 3000;
            _progressTimer.Tick += ProgressTimer_Tick;

            _webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            _webView.Dock = DockStyle.Fill;
            resultsPanel.Controls.Add(_webView);

            this.Shown += Form1_Shown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string icoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (System.IO.File.Exists(icoPath))
                    this.Icon = new System.Drawing.Icon(icoPath);
            }
            catch { }

            try
            {
DatabaseHelper.InitializeDatabase();
                _config = AppConfig.Load();
                _duomiService = new DuomiApiService(_config.Duomi_ApiKey);

                if (!_config.IsValid())
                {
                    new ApiKeyPromptDialog("请先配置多米 API Key！\n点击左侧的【API 设置】按钮进行配置。").ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            await InitWebViewAsync();
        }

        private async Task InitWebViewAsync()
        {
            await _webView.EnsureCoreWebView2Async(null);
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.local", AppDomain.CurrentDomain.BaseDirectory,
                CoreWebView2HostResourceAccessKind.Allow);
            _webView.CoreWebView2.AddWebResourceRequestedFilter(
                "https://localfile.app/*", CoreWebView2WebResourceContext.All);
            _webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.WebResourceRequested += WebView2_LocalFileRequested;
            _webView.WebMessageReceived += WebView_WebMessageReceived;

            string html;
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("BananaBox.wwwroot.index.html"))
            using (var reader = new System.IO.StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            _webView.CoreWebView2.NavigateToString(html);
        }

        private void btnNavCreate_Paint(object sender, PaintEventArgs e)
        {
            // 已从 UI 中移除此按钮，不再需要绘制逻辑
        }

        private void btnSettings_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var iconFont = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold))
            using (var textFont = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular))
            using (var iconBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            using (var textBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                string icon = "⚙";
                var iconSize = e.Graphics.MeasureString(icon, iconFont);
                float iconX = (btn.Width - iconSize.Width) / 2f;
                float iconY = 4f;
                e.Graphics.DrawString(icon, iconFont, iconBrush, iconX, iconY);

                string label = "设置";
                var labelSize = e.Graphics.MeasureString(label, textFont);
                float labelX = (btn.Width - labelSize.Width) / 2f;
                float labelY = btn.Height - labelSize.Height - 4f;
                e.Graphics.DrawString(label, textFont, textBrush, labelX, labelY);
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            _webView.Focus();
            var settingsForm = new SettingsForm(_config);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _config = AppConfig.Load();
                _duomiService = new DuomiApiService(_config.Duomi_ApiKey);
            }
        }

        #region WebView 消息处理

        private void WebView2_LocalFileRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            try
            {
                var uri = new Uri(e.Request.Uri);
                string localPath = uri.LocalPath.TrimStart('/');
                if (!File.Exists(localPath)) return;

                string ext = Path.GetExtension(localPath).ToLower();
                string mime = (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg"
                            : ext == ".png"  ? "image/png"
                            : ext == ".gif"  ? "image/gif"
                            : ext == ".webp" ? "image/webp"
                            : "image/jpeg";

                var stream = new System.IO.MemoryStream(File.ReadAllBytes(localPath));
                e.Response = _webView.CoreWebView2.Environment.CreateWebResourceResponse(
                    stream, 200, "OK", $"Content-Type: {mime}");
            }
            catch { }
        }

        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();
                var msg = JObject.Parse(json);
                string action = msg["action"]?.ToString();
                int taskId = msg["taskId"] != null ? (int)msg["taskId"] : 0;

                switch (action)
                {
                    case "ready":
                        _webViewReady = true;
                        LoadAllTasks();
                        break;

                    case "generate":
                        HandleGenerateAction(msg);
                        break;

                    case "uploadRef":
                        HandleUploadRef();
                        break;

                    case "removeRef":
                        int removeIdx = msg["index"] != null ? (int)msg["index"] : -1;
                        HandleRemoveRef(removeIdx);
                        break;

                    case "reEdit":
                        var reEditTask = DatabaseHelper.GetTask(taskId);
                        if (reEditTask != null) _ = SetEditForm(reEditTask, false);
                        break;

                    case "regenerate":
                        var regenTask = DatabaseHelper.GetTask(taskId);
                        if (regenTask != null) _ = SetEditForm(regenTask, true);
                        break;

                    case "refresh":
                        var refreshTask = DatabaseHelper.GetTask(taskId);
                        if (refreshTask != null)
                            _ = RefreshTaskStatus(refreshTask);
                        break;

                    case "delete":
                        var deleteTask = DatabaseHelper.GetTask(taskId);
                        if (deleteTask != null) DeleteTask(deleteTask);
                        break;

                    case "download":
                        var dlTask = DatabaseHelper.GetTask(taskId);
                        if (dlTask != null && !string.IsNullOrEmpty(dlTask.LocalPath))
                            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dlTask.LocalPath}\"");
                        break;

                    case "openUrl":
                        string url = msg["url"]?.ToString();
                        if (!string.IsNullOrEmpty(url))
                            System.Diagnostics.Process.Start(url);
                        break;

                    case "open":
                        var openTask = DatabaseHelper.GetTask(taskId);
                        if (openTask != null)
                        {
                            var children = DatabaseHelper.GetChildTasks(taskId);
                            var firstChild = children.FirstOrDefault(c => !string.IsNullOrEmpty(c.LocalPath));
                            string targetPath = firstChild?.LocalPath;
                            if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{targetPath}\"");
                            else
                                System.Diagnostics.Process.Start("explorer.exe",
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results"));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView message error: {ex.Message}");
            }
        }

        private async void HandleUploadRef()
        {
            if (!_config.IsOSSConfigured())
            {
                var result = MessageBox.Show(
                    "上传参考图需要配置阿里云 OSS。\n\n是否现在配置？",
                    "需要 OSS 配置",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    var settingsForm = new SettingsForm(_config);
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        _config = AppConfig.Load();
                        _duomiService = new DuomiApiService(_config.Duomi_ApiKey);
                    }
                }
                return;
            }

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                dlg.Title = "选择参考图片";
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in dlg.FileNames)
                    {
                        if (!_referenceImagePaths.Contains(file))
                            _referenceImagePaths.Add(file);
                    }
                    await PushRefImagesToWebView();
                }
            }
        }

        private async void HandleRemoveRef(int index)
        {
            if (index >= 0 && index < _referenceImagePaths.Count)
            {
                _referenceImagePaths.RemoveAt(index);
                await PushRefImagesToWebView();
            }
        }

        private async Task PushRefImagesToWebView()
        {
            var images = _referenceImagePaths
                .Where(p => File.Exists(p))
                .Select(p => new
                {
                    url = "https://localfile.app/" + Uri.EscapeUriString(p.Replace('\\', '/')),
                    name = Path.GetFileName(p)
                })
                .ToList();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(images);
            await _webView.ExecuteScriptAsync($"setRefImages({json})");
        }

        private async Task SetEditForm(GenerationTask task, bool autoSubmit)
        {
            _referenceImagePaths.Clear();
            _existingRefOssUrls.Clear();

            if (!string.IsNullOrEmpty(task.ReferenceImagePath))
            {
                var paths = task.ReferenceImagePath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in paths)
                    if (File.Exists(p)) _referenceImagePaths.Add(p);
            }

            int typeIndex = task.Type == GenerationType.Video ? 0 : 1;
            string aspectRatio = ExtractAspectRatio(task.Parameters);

            List<object> refImages;
            if (_referenceImagePaths.Count > 0)
            {
                // 本地文件存在，显示本地图
                refImages = _referenceImagePaths.Select(p => (object)new
                {
                    url = "https://localfile.app/" + Uri.EscapeUriString(p.Replace('\\', '/')),
                    name = Path.GetFileName(p)
                }).ToList();
            }
            else if (!string.IsNullOrEmpty(task.ReferenceImageOssUrls))
            {
                // 本地文件不存在，回退到已有 OSS URL
                _existingRefOssUrls = task.ReferenceImageOssUrls
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
                refImages = _existingRefOssUrls.Select((url, i) => (object)new
                {
                    url = url,
                    name = $"参考图{i + 1}"
                }).ToList();
            }
            else
            {
                refImages = new List<object>();
            }

            var formData = new
            {
                type = typeIndex,
                model = task.Model,
                prompt = task.Prompt,
                aspectRatio = aspectRatio,
                refImages = refImages,
                autoSubmit = autoSubmit
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(formData);
            await _webView.ExecuteScriptAsync($"setEditForm({json})");
        }

        #endregion

        #region 生成任务

        private async void HandleGenerateAction(JObject msg)
        {
            string prompt = msg["prompt"]?.ToString();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                MessageBox.Show("请输入提示词", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_config.IsValid())
            {
                new ApiKeyPromptDialog("请先配置多米 API Key！\n点击左侧的【API 设置】按钮进行配置。").ShowDialog(this);
                return;
            }

            if (_isGenerating)
            {
                MessageBox.Show("正在生成中，请稍候...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int typeInt = msg["type"] != null ? (int)msg["type"] : 1;
            string model = msg["model"]?.ToString();
            string aspectRatio = msg["aspectRatio"]?.ToString() ?? "auto";
            int quantity = msg["quantity"] != null ? (int)msg["quantity"] : 1;
            int duration = msg["duration"] != null ? (int)msg["duration"] : 10;
            string resolution = msg["resolution"]?.ToString();

            try
            {
                _isGenerating = true;
                await _webView.ExecuteScriptAsync("setGenerating(true)");

                var genType = typeInt == 0 ? GenerationType.Video : GenerationType.Image;

                // 上传参考图一次，供所有子任务共用（有本地文件则上传，否则复用已有 OSS URL）
                var refOssUrls = new List<string>();
                if (_referenceImagePaths.Count > 0)
                    refOssUrls = await UploadReferenceImagesAsync(string.Join(";", _referenceImagePaths));
                else if (_existingRefOssUrls.Count > 0)
                    refOssUrls = new List<string>(_existingRefOssUrls);

                var parentTask = new GenerationTask
                {
                    SessionId = 0,
                    Type = genType,
                    Provider = "duomi",
                    Model = model,
                    Prompt = prompt,
                    ReferenceImagePath = _referenceImagePaths.Count > 0
                        ? string.Join(";", _referenceImagePaths) : null,
                    ReferenceImageOssUrls = refOssUrls.Count > 0
                        ? string.Join(";", refOssUrls) : null,
                    State = TaskState.Pending,
                    Quantity = quantity,
                    ParentTaskId = null,
                    ItemIndex = 0
                };

                int parentTaskId = DatabaseHelper.InsertTask(parentTask);
                parentTask.Id = parentTaskId;

                for (int i = 1; i <= quantity; i++)
                {
                    var childTask = new GenerationTask
                    {
                        SessionId = 0,
                        Type = genType,
                        Provider = "duomi",
                        Model = model,
                        Prompt = prompt,
                        ReferenceImagePath = parentTask.ReferenceImagePath,
                        State = TaskState.Pending,
                        ParentTaskId = parentTaskId,
                        ItemIndex = i,
                        Quantity = 1
                    };

                    int childTaskId = DatabaseHelper.InsertTask(childTask);
                    childTask.Id = childTaskId;

                    if (genType == GenerationType.Video)
                        await SubmitVideoGenerationTask(childTask, aspectRatio, duration, refOssUrls);
                    else
                        await SubmitImageGenerationTask(childTask, aspectRatio, resolution, refOssUrls);
                }

                parentTask.State = TaskState.Running;
                DatabaseHelper.UpdateTask(parentTask);

                _progressTimer.Tag = parentTaskId;
                _progressTimer.Start();

                LoadAllTasks();

                _referenceImagePaths.Clear();
                _existingRefOssUrls.Clear();
                await _webView.ExecuteScriptAsync("clearForm()");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isGenerating = false;
                await _webView.ExecuteScriptAsync("setGenerating(false)");
            }
        }

        private async Task SubmitVideoGenerationTask(GenerationTask task, string aspectRatio, int duration, List<string> refOssUrls)
        {
            try
            {
                var parameters = new { aspect_ratio = aspectRatio, duration = duration };
                task.Parameters = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);

                string imageUrl = refOssUrls.Count > 0 ? refOssUrls[0] : null;

                string apiTaskId = await _duomiService.CreateVideoGenerationAsync(
                    task.Model, task.Prompt, aspectRatio, duration, imageUrl);

                task.TaskId = apiTaskId;
                task.State = TaskState.Running;
                DatabaseHelper.UpdateTask(task);
            }
            catch (Exception ex)
            {
                task.State = TaskState.Error;
                task.ErrorMessage = ex.Message;
                DatabaseHelper.UpdateTask(task);
                throw;
            }
        }

        private async Task SubmitImageGenerationTask(GenerationTask task, string aspectRatio, string imageSize, List<string> refOssUrls)
        {
            try
            {
                var parameters = new { aspect_ratio = aspectRatio, image_size = imageSize };
                task.Parameters = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);

                string apiTaskId;
                if (refOssUrls.Count > 0)
                {
                    apiTaskId = await _duomiService.CreateImageToImageAsync(
                        task.Model, task.Prompt, refOssUrls, aspectRatio, imageSize);
                }
                else
                {
                    apiTaskId = await _duomiService.CreateTextToImageAsync(
                        task.Model, task.Prompt, aspectRatio, imageSize);
                }

                task.TaskId = apiTaskId;
                task.State = TaskState.Running;
                DatabaseHelper.UpdateTask(task);
            }
            catch (Exception ex)
            {
                task.State = TaskState.Error;
                task.ErrorMessage = ex.Message;
                DatabaseHelper.UpdateTask(task);
                throw;
            }
        }

        private async Task<List<string>> UploadReferenceImagesAsync(string referenceImagePath)
        {
            var urls = new List<string>();
            if (string.IsNullOrEmpty(referenceImagePath))
                return urls;

            if (!_config.IsOSSConfigured())
                throw new Exception("参考图需要先配置阿里云 OSS 信息，请在设置中填写 OSS 配置");

            var ossService = new OssService(
                _config.Aliyun_OSS_Endpoint,
                _config.Aliyun_OSS_AccessKeyId,
                _config.Aliyun_OSS_AccessKeySecret,
                _config.Aliyun_OSS_BucketName
            );

            var paths = referenceImagePath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    string url = await ossService.UploadFileAsync(path);
                    urls.Add(url);
                }
            }

            return urls;
        }

        #endregion

        #region 任务管理

        private async void DeleteTask(GenerationTask task)
        {
            var result = MessageBox.Show(
                "确定要删除这个生成任务吗？",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 删除 DB 记录前，先找出仅被本任务独占的 OSS 参考图 URL
                    var exclusiveOssUrls = new List<string>();
                    if (!string.IsNullOrEmpty(task.ReferenceImageOssUrls))
                    {
                        foreach (var url in task.ReferenceImageOssUrls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!DatabaseHelper.IsOssUrlUsedByOtherTasks(url, task.Id))
                                exclusiveOssUrls.Add(url);
                        }
                    }

                    // 删除子任务本地文件及 DB 记录
                    var childTasks = DatabaseHelper.GetChildTasks(task.Id);
                    foreach (var childTask in childTasks)
                    {
                        if (!string.IsNullOrEmpty(childTask.LocalPath) && File.Exists(childTask.LocalPath))
                            File.Delete(childTask.LocalPath);
                        DatabaseHelper.DeleteTask(childTask.Id);
                    }

                    if (!string.IsNullOrEmpty(task.LocalPath) && File.Exists(task.LocalPath))
                        File.Delete(task.LocalPath);

                    DatabaseHelper.DeleteTask(task.Id);

                    // 删除独占的 OSS 参考图
                    if (exclusiveOssUrls.Count > 0 && _config.IsOSSConfigured())
                    {
                        var ossService = new OssService(
                            _config.Aliyun_OSS_Endpoint,
                            _config.Aliyun_OSS_AccessKeyId,
                            _config.Aliyun_OSS_AccessKeySecret,
                            _config.Aliyun_OSS_BucketName);

                        foreach (var url in exclusiveOssUrls)
                        {
                            try { await ossService.DeleteObjectAsync(url); }
                            catch { /* 静默失败，不影响本地删除流程 */ }
                        }
                    }

                    LoadAllTasks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task RefreshTaskStatus(GenerationTask task)
        {
            try
            {
                var childTasks = DatabaseHelper.GetChildTasks(task.Id);
                if (childTasks.Count == 0)
                {
                    MessageBox.Show("没有子任务需要刷新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int updatedCount = 0;
                int completedCount = 0;

                foreach (var childTask in childTasks)
                {
                    if (childTask.State == TaskState.Succeeded || childTask.State == TaskState.Error)
                        continue;

                    if (string.IsNullOrEmpty(childTask.TaskId))
                        continue;

                    string state = null;
                    string resultUrl = null;
                    string errorMessage = null;

                    if (childTask.Type == GenerationType.Video)
                    {
                        var progress = await _duomiService.QueryTaskProgressAsync(childTask.TaskId);
                        childTask.Progress = progress.progress;
                        state = progress.state;
                        errorMessage = progress.message;

                        if (state == "succeeded" && progress.data?.videos != null && progress.data.videos.Count > 0)
                            resultUrl = progress.data.videos[0].url;
                    }
                    else
                    {
                        var progress = await _duomiService.QueryImageTaskProgressAsync(childTask.TaskId);
                        state = progress.data?.state;
                        errorMessage = progress.data?.msg;

                        if (state == "pending") childTask.Progress = 10;
                        else if (state == "running") childTask.Progress = 50;
                        else if (state == "succeeded") childTask.Progress = 100;

                        if (state == "succeeded" && progress.data?.data?.images != null && progress.data.data.images.Count > 0)
                            resultUrl = progress.data.data.images[0].url;
                    }

                    if (state == "succeeded")
                    {
                        childTask.State = TaskState.Succeeded;
                        childTask.Progress = 100;
                        completedCount++;

                        if (!string.IsNullOrEmpty(resultUrl))
                        {
                            childTask.ResultUrl = resultUrl;
                            string extension = GetFileExtension(childTask.Type, resultUrl);
                            string fileName = $"{childTask.Id}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", fileName);
                            childTask.LocalPath = await _duomiService.DownloadResultAsync(childTask.ResultUrl, localPath);
                        }
                        updatedCount++;
                    }
                    else if (state == "error")
                    {
                        childTask.State = TaskState.Error;
                        childTask.ErrorMessage = errorMessage ?? "生成失败";
                        updatedCount++;
                    }

                    DatabaseHelper.UpdateTask(childTask);
                }

                LoadAllTasks();

                if (updatedCount > 0)
                    MessageBox.Show($"刷新完成！有 {updatedCount} 个任务状态更新", "刷新成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("没有任务状态发生变化", "刷新完成",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (!(_progressTimer.Tag is int parentTaskId))
            {
                _progressTimer.Stop();
                return;
            }

            try
            {
                var childTasks = DatabaseHelper.GetChildTasks(parentTaskId);
                if (childTasks.Count == 0)
                {
                    _progressTimer.Stop();
                    return;
                }

                int completedCount = 0;
                int errorCount = 0;

                foreach (var childTask in childTasks)
                {
                    if (childTask.State == TaskState.Succeeded || childTask.State == TaskState.Error)
                    {
                        if (childTask.State == TaskState.Succeeded) completedCount++;
                        else errorCount++;
                        continue;
                    }

                    if (string.IsNullOrEmpty(childTask.TaskId)) continue;

                    string state = null;
                    string resultUrl = null;
                    string errorMessage = null;

                    if (childTask.Type == GenerationType.Video)
                    {
                        var progress = await _duomiService.QueryTaskProgressAsync(childTask.TaskId);
                        childTask.Progress = progress.progress;
                        state = progress.state;
                        errorMessage = progress.message;

                        if (state == "succeeded" && progress.data?.videos != null && progress.data.videos.Count > 0)
                            resultUrl = progress.data.videos[0].url;
                    }
                    else
                    {
                        var progress = await _duomiService.QueryImageTaskProgressAsync(childTask.TaskId);
                        state = progress.data?.state;
                        errorMessage = progress.data?.msg;

                        if (state == "pending") childTask.Progress = 10;
                        else if (state == "running") childTask.Progress = 50;
                        else if (state == "succeeded") childTask.Progress = 100;

                        if (state == "succeeded" && progress.data?.data?.images != null && progress.data.data.images.Count > 0)
                            resultUrl = progress.data.data.images[0].url;
                    }

                    if (state == "succeeded")
                    {
                        childTask.State = TaskState.Succeeded;
                        childTask.Progress = 100;
                        completedCount++;

                        if (!string.IsNullOrEmpty(resultUrl))
                        {
                            childTask.ResultUrl = resultUrl;
                            string extension = GetFileExtension(childTask.Type, resultUrl);
                            string fileName = $"{childTask.Id}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", fileName);
                            childTask.LocalPath = await _duomiService.DownloadResultAsync(childTask.ResultUrl, localPath);
                        }
                    }
                    else if (state == "error")
                    {
                        childTask.State = TaskState.Error;
                        childTask.ErrorMessage = errorMessage ?? "生成失败";
                        errorCount++;
                    }

                    DatabaseHelper.UpdateTask(childTask);
                }

                if (InvokeRequired)
                    Invoke(new Action(() => LoadAllTasks()));
                else
                    LoadAllTasks();

                if (completedCount + errorCount == childTasks.Count)
                {
                    var parentTask = DatabaseHelper.GetAllTasks().FirstOrDefault(t => t.Id == parentTaskId);
                    if (parentTask != null)
                    {
                        if (errorCount == childTasks.Count)
                        {
                            parentTask.State = TaskState.Error;
                            parentTask.ErrorMessage = "所有生成任务都失败了";
                        }
                        else if (completedCount > 0)
                        {
                            parentTask.State = TaskState.Succeeded;
                            parentTask.Progress = 100;
                        }
                        DatabaseHelper.UpdateTask(parentTask);
                    }

                    _progressTimer.Stop();

                    if (completedCount > 0)
                    {
                        string message = errorCount > 0
                            ? $"生成完成！成功 {completedCount} 个，失败 {errorCount} 个"
                            : $"全部生成完成！共 {completedCount} 个";
                        MessageBox.Show(message, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("生成失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (InvokeRequired)
                        Invoke(new Action(() => LoadAllTasks()));
                    else
                        LoadAllTasks();
                }
            }
            catch (Exception ex)
            {
                _progressTimer.Stop();
                MessageBox.Show($"查询进度失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetFileExtension(GenerationType type, string resultUrl)
        {
            if (type == GenerationType.Video) return ".mp4";
            try
            {
                var uri = new Uri(resultUrl);
                string ext = Path.GetExtension(uri.LocalPath);
                return string.IsNullOrEmpty(ext) ? ".jpg" : ext;
            }
            catch { return ".jpg"; }
        }

        #endregion

        #region 结果显示

        private async void LoadAllTasks()
        {
            if (!_webViewReady) return;

            try
            {
                var tasks = DatabaseHelper.GetAllTasks();
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                var data = tasks.Select(t => new
                {
                    id = t.Id,
                    model = t.Model,
                    prompt = t.Prompt,
                    type = (int)t.Type,
                    quantity = t.Quantity,
                    aspectRatio = ExtractAspectRatio(t.Parameters),
                    createTime = t.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                    refImageUrls = string.IsNullOrEmpty(t.ReferenceImageOssUrls)
                        ? new List<string>()
                        : t.ReferenceImageOssUrls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    children = DatabaseHelper.GetChildTasks(t.Id).Select(c => new
                    {
                        id = c.Id,
                        state = (int)c.State,
                        progress = c.Progress,
                        errorMessage = c.ErrorMessage,
                        localUrl = LocalPathToUrl(c.LocalPath, baseDir)
                    }).ToList()
                }).ToList();

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                await _webView.ExecuteScriptAsync($"renderTasks({json})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAllTasks error: {ex.Message}");
            }
        }

        private string LocalPathToUrl(string localPath, string baseDir)
        {
            if (string.IsNullOrEmpty(localPath)) return "";
            if (!File.Exists(localPath)) return "";
            try
            {
                if (localPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                {
                    string relative = localPath.Substring(baseDir.Length).Replace('\\', '/').TrimStart('/');
                    return "https://app.local/" + relative;
                }
                return "";
            }
            catch { return ""; }
        }

        private string ExtractAspectRatio(string parameters)
        {
            if (string.IsNullOrEmpty(parameters)) return "";
            try
            {
                var obj = JObject.Parse(parameters);
                return obj["aspect_ratio"]?.ToString() ?? "";
            }
            catch { return ""; }
        }

#endregion
    }
}
