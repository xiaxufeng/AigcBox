using System;
using System.IO;
using System.Text;

namespace BananaBox.Config
{
    /// <summary>
    /// 应用配置类
    /// </summary>
    public class AppConfig
    {
        private static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");

        // 多米 API 配置
        public string Duomi_ApiKey { get; set; }

        // 阿里云 OSS 配置
        public string Aliyun_OSS_Endpoint { get; set; }
        public string Aliyun_OSS_AccessKeyId { get; set; }
        public string Aliyun_OSS_AccessKeySecret { get; set; }
        public string Aliyun_OSS_BucketName { get; set; }

        // 通用配置
        public string DefaultProvider { get; set; } = "duomi";
        public string SavePath { get; set; } = "result";

        /// <summary>
        /// 加载配置
        /// </summary>
        public static AppConfig Load()
        {
            var config = new AppConfig();

            if (File.Exists(configPath))
            {
                try
                {
                    var lines = File.ReadAllLines(configPath, Encoding.UTF8);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            continue;

                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length != 2)
                            continue;

                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        switch (key)
                        {
                            case "Duomi_ApiKey":
                                config.Duomi_ApiKey = value;
                                break;
                            case "Aliyun_OSS_Endpoint":
                                config.Aliyun_OSS_Endpoint = value;
                                break;
                            case "Aliyun_OSS_AccessKeyId":
                                config.Aliyun_OSS_AccessKeyId = value;
                                break;
                            case "Aliyun_OSS_AccessKeySecret":
                                config.Aliyun_OSS_AccessKeySecret = value;
                                break;
                            case "Aliyun_OSS_BucketName":
                                config.Aliyun_OSS_BucketName = value;
                                break;
                            case "DefaultProvider":
                                config.DefaultProvider = value;
                                break;
                            case "SavePath":
                                config.SavePath = value;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"加载配置失败: {ex.Message}");
                }
            }

            return config;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            try
            {
                var content = new StringBuilder();
                content.AppendLine("# BananaBox 配置文件");
                content.AppendLine("# 多米 API 配置");
                content.AppendLine($"Duomi_ApiKey={Duomi_ApiKey ?? ""}");
                content.AppendLine();
                content.AppendLine("# 阿里云 OSS 配置");
                content.AppendLine($"Aliyun_OSS_Endpoint={Aliyun_OSS_Endpoint ?? ""}");
                content.AppendLine($"Aliyun_OSS_AccessKeyId={Aliyun_OSS_AccessKeyId ?? ""}");
                content.AppendLine($"Aliyun_OSS_AccessKeySecret={Aliyun_OSS_AccessKeySecret ?? ""}");
                content.AppendLine($"Aliyun_OSS_BucketName={Aliyun_OSS_BucketName ?? ""}");
                content.AppendLine();
                content.AppendLine("# 通用配置");
                content.AppendLine($"DefaultProvider={DefaultProvider}");
                content.AppendLine($"SavePath={SavePath ?? "result"}");

                File.WriteAllText(configPath, content.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证配置是否完整
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Duomi_ApiKey);
        }

        /// <summary>
        /// 验证 OSS 配置是否完整
        /// </summary>
        public bool IsOSSConfigured()
        {
            return !string.IsNullOrWhiteSpace(Aliyun_OSS_Endpoint) &&
                   !string.IsNullOrWhiteSpace(Aliyun_OSS_AccessKeyId) &&
                   !string.IsNullOrWhiteSpace(Aliyun_OSS_AccessKeySecret) &&
                   !string.IsNullOrWhiteSpace(Aliyun_OSS_BucketName);
        }
    }
}
