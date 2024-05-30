﻿using Newtonsoft.Json;

namespace Xin.DB.DbLog
{
    public class LogSaver:ILogSaver
    {
        private static string JsonfilePath;
        private static LogConfig LogConfig;
        private string baseDirectory;
        private static Dictionary<LogType, Tuple<string, int>> LogPathDict = new ();
        private static bool isInit = false;
        public static string jsonFilePath;
        public LogSaver()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                throw new Exception("配置文件没有初始化");
            }
            
            //检查日志文件是否存在时间过长
            RefreshLog();

        }
        public LogSaver(string configJsonPath)
        {
            if (isInit)
            {
                throw new Exception("已经初始化过了，请使用无参构造方法");
            }
            if (!isInit)
            {
                jsonFilePath = configJsonPath;
                //初始化LogSaver
                Init();
            }
        }
        /// <summary>
        /// 初始化LogSaver
        /// </summary>
        private void Init()
        {
            //暂存存储BaseDirectory
            baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //解析LogConfig配置文件
            LogConfig = JsonConvert.DeserializeObject<LogConfig>(File.ReadAllText(jsonFilePath));

            //根据LogConfig配置文件,初始化四个日志文件
            LogPathDict.Add(LogType.Success, Tuple.Create(Path.Combine(baseDirectory, "Log", $"{LogConfig.SuccessLogName}.txt"), LogConfig.SuccessSaveDays));
            LogPathDict.Add(LogType.Warning, Tuple.Create(Path.Combine(baseDirectory, "Log", $"{LogConfig.WarningLogName}.txt"), LogConfig.WarningSaveDays));
            LogPathDict.Add(LogType.Error, Tuple.Create(Path.Combine(baseDirectory, "Log", $"{LogConfig.ErrorLogName}.txt"), LogConfig.ErrorSaveDays));
            LogPathDict.Add(LogType.Exception, Tuple.Create(Path.Combine(baseDirectory, "Log", $"{LogConfig.ExpectionLogName}.txt"), LogConfig.ExceptionSaveDays));

            if (!Directory.Exists(Path.Combine(baseDirectory, "Log")))
            {
                Directory.CreateDirectory(Path.Combine(baseDirectory, "Log"));
            }
            foreach (var tuple in LogPathDict.Values)
            {
                if (!File.Exists(tuple.Item1))
                {
                    using (File.Create(tuple.Item1))
                    { }
                }
                    
            }
            isInit = true;
        }

        public void WriteSuccessLog(string message)
        {
            using (StreamWriter sw = new StreamWriter(LogPathDict[LogType.Success].Item1, true))
            {
                string writerMessage = $"Success: [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}";

                sw.WriteLine(writerMessage);
            }
        }
        
        public void WriteWarningLog(string message)
        {
            using(StreamWriter sw = new StreamWriter(LogPathDict[LogType.Warning].Item1, true))
            {
                string writerMessage = $"Warning: [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}";
                sw.WriteLine(writerMessage);
            }
        }
        public void WriteErrorLog(string message)
        {
            using(StreamWriter sw = new StreamWriter(LogPathDict[LogType.Error].Item1, true))
            {
                string writerMessage = $"Error: [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}";
                sw.WriteLine(writerMessage);
            }
        }

        public void WriteExceptionLog(string message)
        {
            using(StreamWriter sw = new StreamWriter(LogPathDict[LogType.Exception].Item1, true))
            {
                string writerMessage = $"Exception: [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}";
                sw.WriteLine(writerMessage);
            }
        }

        /// <summary>
        /// 刷新日志文件，防止日志文件存在时间过长
        /// </summary>
        private void RefreshLog()
        {
            foreach(var key in LogPathDict.Keys)
            {
                DateTime LastWriteTime = File.GetLastWriteTime(LogPathDict[key].Item1);
                var days = (DateTime.Now - LastWriteTime).Days;
                if (days > LogPathDict[key].Item2)
                { 
                    using(FileStream fs = new FileStream(LogPathDict[key].Item1,FileMode.Truncate))
                    {
                    }
                }
            }
        }
    }
    
    public class LogConfig
    {
        public string SuccessLogName;
        public string ErrorLogName;
        public string ExpectionLogName;
        public string  WarningLogName;
        public string LogFileName;
        public int SuccessSaveDays;
        public int ErrorSaveDays;
        public int ExceptionSaveDays;
        public int WarningSaveDays;
    }

    public enum LogType : int
    {
        Success = 200,
        Error = 500,
        Exception = 501,
        Warning = 300,
    }
}