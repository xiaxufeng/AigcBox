# AigcBox

基于 WinForms + WebView2 的 AIGC 图像/视频生成客户端，通过多米 API 调用 Sora、Gemini 等模型。

## 功能

- **视频生成**：支持 sora-2、sora-2-pro 模型，可配置长宽比与时长
- **图像生成**：支持 gemini-3-pro-image-preview、gemini-3.1-flash-image-preview 模型，支持文生图与图生图
- **任务管理**：卡片式展示所有生成任务，每 3 秒自动查询进度
- **本地保存**：生成完成后自动下载至程序目录下的 `Results/` 文件夹
- **参考图**：支持上传参考图（阿里云 OSS 中转）

## 开发环境

- Visual Studio 2022
- .NET Framework 4.8
- 还原 NuGet 包后直接编译运行

## 配置

首次运行时，点击左侧设置按钮配置以下项目：

**多米 API**（必填）
- API Key：前往 [duomiapi.com](https://duomiapi.com/user/register?cps=dhwJyHCh) 注册获取
- Base URL：默认 `https://api.duomiapi.com`

**阿里云 OSS**（使用参考图功能时需要）
- AccessKey ID / AccessKey Secret
- Bucket 名称与地域

配置保存于程序目录下的 `config.txt`。

## 项目结构

```
BananaBox/
├── Config/
│   └── AppConfig.cs              # 配置管理
├── Services/
│   ├── DuomiApiService.cs        # 多米 API 调用
│   └── OssService.cs             # 阿里云 OSS 上传
├── Models/
│   └── GenerationTask.cs         # 任务数据模型
├── Data/
│   └── DatabaseHelper.cs         # SQLite 数据访问
├── wwwroot/
│   └── index.html                # WebView2 结果面板
├── Form1.cs                      # 主窗体
├── SettingsForm.cs               # 设置窗体
├── ApiKeyPromptDialog.cs         # API Key 提示对话框
└── Program.cs                    # 入口（含 WebView2 检测）
```

## 依赖

| 包 | 版本 |
|----|------|
| Microsoft.Web.WebView2 | 1.0.3405.78 |
| Microsoft.Data.Sqlite.Core | 10.0.2 |
| Newtonsoft.Json | 13.0.4 |

WebView2 运行时需单独安装，程序启动时会自动检测并提示。

## 许可证

MIT License
