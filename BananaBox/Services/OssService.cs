using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BananaBox.Services
{
    /// <summary>
    /// 阿里云 OSS 上传服务
    /// </summary>
    public class OssService
    {
        private readonly string _endpoint;
        private readonly string _accessKeyId;
        private readonly string _accessKeySecret;
        private readonly string _bucketName;
        private readonly HttpClient _httpClient;

        public OssService(string endpoint, string accessKeyId, string accessKeySecret, string bucketName)
        {
            _endpoint = endpoint.TrimEnd('/');
            _accessKeyId = accessKeyId;
            _accessKeySecret = accessKeySecret;
            _bucketName = bucketName;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
        }

        /// <summary>
        /// 上传本地文件到 OSS，返回公开访问 URL
        /// </summary>
        public async Task<string> UploadFileAsync(string localFilePath)
        {
            string ext = Path.GetExtension(localFilePath).ToLower();
            string contentType = GetContentType(ext);

            byte[] fileBytes = File.ReadAllBytes(localFilePath);
            string md5Hash;
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(fileBytes);
                md5Hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            string objectName = $"reference-images/{md5Hash}{ext}";
            string date = DateTime.UtcNow.ToString("R");

            // x-oss-object-acl 必须加入 CanonicalizedOSSHeaders 参与签名
            const string objectAcl = "public-read";
            string canonicalizedOssHeaders = $"x-oss-object-acl:{objectAcl}\n";
            string canonicalizedResource = $"/{_bucketName}/{objectName}";
            string stringToSign = $"PUT\n\n{contentType}\n{date}\n{canonicalizedOssHeaders}{canonicalizedResource}";
            string signature = ComputeHmacSha1(_accessKeySecret, stringToSign);

            string url = $"https://{_bucketName}.{_endpoint}/{objectName}";

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = new ByteArrayContent(fileBytes);
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            request.Headers.TryAddWithoutValidation("Date", date);
            request.Headers.TryAddWithoutValidation("x-oss-object-acl", objectAcl);
            request.Headers.TryAddWithoutValidation("Authorization",
                $"OSS {_accessKeyId}:{signature}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                throw new Exception($"OSS 上传失败: {response.StatusCode}\n{body}");
            }

            return url;
        }

        /// <summary>
        /// 删除 OSS 上的对象
        /// </summary>
        public async Task DeleteObjectAsync(string objectUrl)
        {
            var uri = new Uri(objectUrl);
            string objectName = uri.AbsolutePath.TrimStart('/');

            string date = DateTime.UtcNow.ToString("R");
            string canonicalizedResource = $"/{_bucketName}/{objectName}";
            string stringToSign = $"DELETE\n\n\n{date}\n{canonicalizedResource}";
            string signature = ComputeHmacSha1(_accessKeySecret, stringToSign);

            var request = new HttpRequestMessage(HttpMethod.Delete, objectUrl);
            request.Headers.TryAddWithoutValidation("Date", date);
            request.Headers.TryAddWithoutValidation("Authorization", $"OSS {_accessKeyId}:{signature}");

            var response = await _httpClient.SendAsync(request);
            // 204 = 删除成功，404 = 对象不存在，均视为成功
            if (!response.IsSuccessStatusCode
                && response.StatusCode != System.Net.HttpStatusCode.NoContent
                && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                string body = await response.Content.ReadAsStringAsync();
                throw new Exception($"OSS 删除失败: {response.StatusCode}\n{body}");
            }
        }

        private string ComputeHmacSha1(string key, string data)
        {
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        private string GetContentType(string ext)
        {
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
