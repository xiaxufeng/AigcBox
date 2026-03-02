using System;

namespace BananaBox.Models
{
    /// <summary>
    /// 生成任务类型
    /// </summary>
    public enum GenerationType
    {
        Image,  // 图像生成
        Video   // 视频生成
    }

    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskState
    {
        Pending,    // 队列中
        Running,    // 生成中
        Succeeded,  // 已成功
        Error       // 失败
    }

    /// <summary>
    /// AIGC 生成任务实体类
    /// </summary>
    public class GenerationTask
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string TaskId { get; set; }  // 服务商返回的任务ID
        public GenerationType Type { get; set; }
        public string Provider { get; set; }  // 服务商（duomi、openai等）
        public string Model { get; set; }
        public string Prompt { get; set; }
        public string Parameters { get; set; }  // JSON格式的参数
        public string ReferenceImagePath { get; set; }    // 参考图片本地路径
        public string ReferenceImageOssUrls { get; set; } // 参考图 OSS URL（分号分隔）
        public string ResultUrl { get; set; }  // 生成结果URL
        public string LocalPath { get; set; }  // 本地保存路径
        public TaskState State { get; set; }
        public int Progress { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        // 父子任务关系
        public int? ParentTaskId { get; set; }  // 父任务ID，NULL表示这是父任务
        public int Quantity { get; set; }  // 请求生成的数量（仅父任务有效）
        public int ItemIndex { get; set; }  // 子任务索引（1-based），父任务为0

        public GenerationTask()
        {
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
            State = TaskState.Pending;
            Progress = 0;
            Quantity = 1;
            ItemIndex = 0;
        }
    }
}
