using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using BananaBox.Models;

namespace BananaBox.Data
{
    /// <summary>
    /// SQLite 数据库管理类
    /// </summary>
    public class DatabaseHelper
    {
        private static string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chatbox.db");
        private static string connectionString = $"Data Source={dbPath}";

        /// <summary>
        /// 初始化数据库，创建表结构
        /// </summary>
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // 创建生成任务表
                    string createTasksTable = @"
                        CREATE TABLE IF NOT EXISTS GenerationTasks (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            SessionId INTEGER NOT NULL,
                            TaskId TEXT,
                            Type INTEGER NOT NULL,
                            Provider TEXT NOT NULL,
                            Model TEXT NOT NULL,
                            Prompt TEXT NOT NULL,
                            Parameters TEXT,
                            ReferenceImagePath TEXT,
                            ResultUrl TEXT,
                            LocalPath TEXT,
                            State INTEGER NOT NULL,
                            Progress INTEGER DEFAULT 0,
                            ErrorMessage TEXT,
                            CreateTime TEXT NOT NULL,
                            UpdateTime TEXT NOT NULL,
                            ParentTaskId INTEGER,
                            Quantity INTEGER DEFAULT 1,
                            ItemIndex INTEGER DEFAULT 0
                        )";

                    using (var command = new SqliteCommand(createTasksTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 迁移：添加 ReferenceImageOssUrls 列（旧库兼容）
                    try
                    {
                        using (var command = new SqliteCommand(
                            "ALTER TABLE GenerationTasks ADD COLUMN ReferenceImageOssUrls TEXT DEFAULT ''",
                            connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch { /* 列已存在，忽略 */ }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"数据库初始化失败: {ex.Message}");
            }
        }

        #region 生成任务管理

        /// <summary>
        /// 插入新生成任务
        /// </summary>
        public static int InsertTask(GenerationTask task)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = @"
                        INSERT INTO GenerationTasks (
                            SessionId, TaskId, Type, Provider, Model, Prompt,
                            Parameters, ReferenceImagePath, ReferenceImageOssUrls,
                            ResultUrl, LocalPath,
                            State, Progress, ErrorMessage, CreateTime, UpdateTime,
                            ParentTaskId, Quantity, ItemIndex
                        ) VALUES (
                            @SessionId, @TaskId, @Type, @Provider, @Model, @Prompt,
                            @Parameters, @ReferenceImagePath, @ReferenceImageOssUrls,
                            @ResultUrl, @LocalPath,
                            @State, @Progress, @ErrorMessage, @CreateTime, @UpdateTime,
                            @ParentTaskId, @Quantity, @ItemIndex
                        );
                        SELECT last_insert_rowid();";

                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@SessionId", task.SessionId);
                        command.Parameters.AddWithValue("@TaskId", task.TaskId ?? "");
                        command.Parameters.AddWithValue("@Type", (int)task.Type);
                        command.Parameters.AddWithValue("@Provider", task.Provider);
                        command.Parameters.AddWithValue("@Model", task.Model);
                        command.Parameters.AddWithValue("@Prompt", task.Prompt);
                        command.Parameters.AddWithValue("@Parameters", task.Parameters ?? "");
                        command.Parameters.AddWithValue("@ReferenceImagePath", task.ReferenceImagePath ?? "");
                        command.Parameters.AddWithValue("@ReferenceImageOssUrls", task.ReferenceImageOssUrls ?? "");
                        command.Parameters.AddWithValue("@ResultUrl", task.ResultUrl ?? "");
                        command.Parameters.AddWithValue("@LocalPath", task.LocalPath ?? "");
                        command.Parameters.AddWithValue("@State", (int)task.State);
                        command.Parameters.AddWithValue("@Progress", task.Progress);
                        command.Parameters.AddWithValue("@ErrorMessage", task.ErrorMessage ?? "");
                        command.Parameters.AddWithValue("@CreateTime", task.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@UpdateTime", task.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@ParentTaskId", task.ParentTaskId.HasValue ? (object)task.ParentTaskId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", task.Quantity);
                        command.Parameters.AddWithValue("@ItemIndex", task.ItemIndex);

                        var result = command.ExecuteScalar();
                        int taskId = Convert.ToInt32(result);

                        return taskId;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"插入生成任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新任务状态
        /// </summary>
        public static void UpdateTask(GenerationTask task)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = @"
                        UPDATE GenerationTasks SET
                            TaskId = @TaskId,
                            State = @State,
                            Progress = @Progress,
                            ResultUrl = @ResultUrl,
                            LocalPath = @LocalPath,
                            ErrorMessage = @ErrorMessage,
                            UpdateTime = @UpdateTime
                        WHERE Id = @Id";

                    using (var command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", task.TaskId ?? "");
                        command.Parameters.AddWithValue("@State", (int)task.State);
                        command.Parameters.AddWithValue("@Progress", task.Progress);
                        command.Parameters.AddWithValue("@ResultUrl", task.ResultUrl ?? "");
                        command.Parameters.AddWithValue("@LocalPath", task.LocalPath ?? "");
                        command.Parameters.AddWithValue("@ErrorMessage", task.ErrorMessage ?? "");
                        command.Parameters.AddWithValue("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@Id", task.Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取子任务列表
        /// </summary>
        public static List<GenerationTask> GetChildTasks(int parentTaskId)
        {
            var tasks = new List<GenerationTask>();

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM GenerationTasks WHERE ParentTaskId = @ParentTaskId ORDER BY ItemIndex ASC";

                    using (var command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ParentTaskId", parentTaskId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new GenerationTask
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    SessionId = Convert.ToInt32(reader["SessionId"]),
                                    TaskId = reader["TaskId"].ToString(),
                                    Type = (GenerationType)Convert.ToInt32(reader["Type"]),
                                    Provider = reader["Provider"].ToString(),
                                    Model = reader["Model"].ToString(),
                                    Prompt = reader["Prompt"].ToString(),
                                    Parameters = reader["Parameters"].ToString(),
                                    ReferenceImagePath = reader["ReferenceImagePath"].ToString(),
                                    ReferenceImageOssUrls = reader["ReferenceImageOssUrls"].ToString(),
                                    ResultUrl = reader["ResultUrl"].ToString(),
                                    LocalPath = reader["LocalPath"].ToString(),
                                    State = (TaskState)Convert.ToInt32(reader["State"]),
                                    Progress = Convert.ToInt32(reader["Progress"]),
                                    ErrorMessage = reader["ErrorMessage"].ToString(),
                                    CreateTime = DateTime.Parse(reader["CreateTime"].ToString()),
                                    UpdateTime = DateTime.Parse(reader["UpdateTime"].ToString()),
                                    ParentTaskId = reader["ParentTaskId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ParentTaskId"]) : null,
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    ItemIndex = Convert.ToInt32(reader["ItemIndex"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取子任务失败: {ex.Message}");
            }

            return tasks;
        }

        /// <summary>
        /// 获取指定 ID 的任务
        /// </summary>
        public static GenerationTask GetTask(int id)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM GenerationTasks WHERE Id = @Id";

                    using (var command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new GenerationTask
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    SessionId = Convert.ToInt32(reader["SessionId"]),
                                    TaskId = reader["TaskId"].ToString(),
                                    Type = (GenerationType)Convert.ToInt32(reader["Type"]),
                                    Provider = reader["Provider"].ToString(),
                                    Model = reader["Model"].ToString(),
                                    Prompt = reader["Prompt"].ToString(),
                                    Parameters = reader["Parameters"].ToString(),
                                    ReferenceImagePath = reader["ReferenceImagePath"].ToString(),
                                    ReferenceImageOssUrls = reader["ReferenceImageOssUrls"].ToString(),
                                    ResultUrl = reader["ResultUrl"].ToString(),
                                    LocalPath = reader["LocalPath"].ToString(),
                                    State = (TaskState)Convert.ToInt32(reader["State"]),
                                    Progress = Convert.ToInt32(reader["Progress"]),
                                    ErrorMessage = reader["ErrorMessage"].ToString(),
                                    CreateTime = DateTime.Parse(reader["CreateTime"].ToString()),
                                    UpdateTime = DateTime.Parse(reader["UpdateTime"].ToString()),
                                    ParentTaskId = reader["ParentTaskId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ParentTaskId"]) : null,
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    ItemIndex = Convert.ToInt32(reader["ItemIndex"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取任务失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 获取所有生成任务（不按会话过滤，仅返回父任务）
        /// </summary>
        public static List<GenerationTask> GetAllTasks()
        {
            var tasks = new List<GenerationTask>();

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM GenerationTasks WHERE ParentTaskId IS NULL ORDER BY CreateTime ASC";

                    using (var command = new SqliteCommand(selectQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new GenerationTask
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                SessionId = Convert.ToInt32(reader["SessionId"]),
                                TaskId = reader["TaskId"].ToString(),
                                Type = (GenerationType)Convert.ToInt32(reader["Type"]),
                                Provider = reader["Provider"].ToString(),
                                Model = reader["Model"].ToString(),
                                Prompt = reader["Prompt"].ToString(),
                                Parameters = reader["Parameters"].ToString(),
                                ReferenceImagePath = reader["ReferenceImagePath"].ToString(),
                                ReferenceImageOssUrls = reader["ReferenceImageOssUrls"].ToString(),
                                ResultUrl = reader["ResultUrl"].ToString(),
                                LocalPath = reader["LocalPath"].ToString(),
                                State = (TaskState)Convert.ToInt32(reader["State"]),
                                Progress = Convert.ToInt32(reader["Progress"]),
                                ErrorMessage = reader["ErrorMessage"].ToString(),
                                CreateTime = DateTime.Parse(reader["CreateTime"].ToString()),
                                UpdateTime = DateTime.Parse(reader["UpdateTime"].ToString()),
                                ParentTaskId = reader["ParentTaskId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ParentTaskId"]) : null,
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                ItemIndex = Convert.ToInt32(reader["ItemIndex"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取所有任务失败: {ex.Message}");
            }

            return tasks;
        }

        /// <summary>
        /// 清空所有生成任务（不按会话过滤）
        /// </summary>
        public static void ClearAllTasks()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM GenerationTasks";

                    using (var command = new SqliteCommand(deleteQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"清空所有任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除指定任务
        /// </summary>
        public static void DeleteTask(int taskId)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM GenerationTasks WHERE Id = @Id";

                    using (var command = new SqliteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", taskId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"删除任务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查某个 OSS URL 是否被除指定任务外的其他父任务引用
        /// </summary>
        public static bool IsOssUrlUsedByOtherTasks(string ossUrl, int excludeTaskId)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM GenerationTasks
                                     WHERE ParentTaskId IS NULL
                                       AND Id != @ExcludeId
                                       AND ReferenceImageOssUrls LIKE @Pattern";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ExcludeId", excludeTaskId);
                        command.Parameters.AddWithValue("@Pattern", "%" + ossUrl + "%");
                        long count = (long)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return true; // 查询失败时保守处理，不删除
            }
        }

        #endregion
    }
}
