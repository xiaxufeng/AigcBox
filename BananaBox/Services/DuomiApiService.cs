using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BananaBox.Models;
using Newtonsoft.Json;

namespace BananaBox.Services
{
    /// <summary>
    /// 多米 API 服务类
    /// </summary>
    public class DuomiApiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private const string VideoBaseUrl = "https://duomiapi.com/v1";
        private const string ImageBaseUrl = "https://duomiapi.com/api/gemini";

        public DuomiApiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// 创建视频生成任务
        /// </summary>
        public async Task<string> CreateVideoGenerationAsync(
            string model,
            string prompt,
            string aspectRatio = "16:9",
            int duration = 10,
            string imageUrl = null)
        {
            try
            {
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    aspect_ratio = aspectRatio,
                    duration = duration,
                    image_urls = string.IsNullOrEmpty(imageUrl) ? null : new[] { imageUrl }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{VideoBaseUrl}/videos/generations")
                {
                    Content = content
                };

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.Headers.Add("Authorization", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API 请求失败: {response.StatusCode}\n{responseContent}");
                }

                var result = JsonConvert.DeserializeObject<CreateTaskResponse>(responseContent);
                return result?.id;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建视频生成任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建文生图任务
        /// </summary>
        public async Task<string> CreateTextToImageAsync(
            string model,
            string prompt,
            string aspectRatio = "auto",
            string imageSize = null)
        {
            try
            {
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    aspect_ratio = aspectRatio,
                    image_size = imageSize
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{ImageBaseUrl}/nano-banana")
                {
                    Content = content
                };

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.Headers.Add("Authorization", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API 请求失败: {response.StatusCode}\n{responseContent}");
                }

                var result = JsonConvert.DeserializeObject<ImageTaskResponse>(responseContent);
                return result?.data?.task_id;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建文生图任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建图生图任务
        /// </summary>
        public async Task<string> CreateImageToImageAsync(
            string model,
            string prompt,
            List<string> imageUrls,
            string aspectRatio = "auto",
            string imageSize = null)
        {
            try
            {
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    image_urls = imageUrls,
                    aspect_ratio = aspectRatio,
                    image_size = imageSize
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{ImageBaseUrl}/nano-banana-edit")
                {
                    Content = content
                };

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.Headers.Add("Authorization", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API 请求失败: {response.StatusCode}\n{responseContent}");
                }

                var result = JsonConvert.DeserializeObject<ImageTaskResponse>(responseContent);
                return result?.data?.task_id;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建图生图任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查询视频任务进度
        /// </summary>
        public async Task<TaskProgressResponse> QueryTaskProgressAsync(string taskId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{VideoBaseUrl}/videos/tasks/{taskId}");

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.Headers.Add("Authorization", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API 请求失败: {response.StatusCode}\n{responseContent}");
                }

                var result = JsonConvert.DeserializeObject<TaskProgressResponse>(responseContent);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"查询任务进度失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查询图像任务进度
        /// </summary>
        public async Task<ImageTaskProgressResponse> QueryImageTaskProgressAsync(string taskId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{ImageBaseUrl}/nano-banana/{taskId}");

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.Headers.Add("Authorization", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API 请求失败: {response.StatusCode}\n{responseContent}");
                }

                var result = JsonConvert.DeserializeObject<ImageTaskProgressResponse>(responseContent);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"查询图像任务进度失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 下载生成的视频/图片
        /// </summary>
        public async Task<string> DownloadResultAsync(string url, string savePath)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fileStream = File.Create(savePath))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"下载文件失败: {ex.Message}");
            }
        }

        #region API 响应模型

        // 视频生成响应模型
        private class CreateTaskResponse
        {
            public string id { get; set; }
        }

        public class TaskProgressResponse
        {
            public string id { get; set; }
            public string state { get; set; }
            public TaskData data { get; set; }
            public int progress { get; set; }
            public long create_time { get; set; }
            public long update_time { get; set; }
            public string message { get; set; }
            public string action { get; set; }
        }

        public class TaskData
        {
            public List<VideoItem> videos { get; set; }
        }

        public class VideoItem
        {
            public string url { get; set; }
        }

        // 图像生成响应模型
        public class ImageTaskResponse
        {
            public int code { get; set; }
            public string msg { get; set; }
            public ImageTaskData data { get; set; }
        }

        public class ImageTaskData
        {
            public string task_id { get; set; }
            public string state { get; set; }
        }

        public class ImageTaskProgressResponse
        {
            public int code { get; set; }
            public string msg { get; set; }
            public double exec_time { get; set; }
            public string ip { get; set; }
            public ImageTaskProgressData data { get; set; }
        }

        public class ImageTaskProgressData
        {
            public string task_id { get; set; }
            public string state { get; set; }  // pending, running, succeeded, error
            public ImageResultData data { get; set; }
            public string create_time { get; set; }
            public string update_time { get; set; }
            public string msg { get; set; }
            public string status { get; set; }
            public string action { get; set; }
        }

        public class ImageResultData
        {
            public List<ImageItem> images { get; set; }
            public string description { get; set; }
        }

        public class ImageItem
        {
            public string url { get; set; }
        }

        #endregion
    }
}
