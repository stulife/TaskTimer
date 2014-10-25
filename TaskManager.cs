using System;
using System.Collections.Generic;
using System.Data;
using System.Timers;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace TPLib.Task
{
   

    public class TaskManager
    {

        /// <summary>
        /// 任务计时器
        /// </summary>
        private Timer tt;
   
        /// <summary>
        /// 已经加载过的任务
        /// </summary>
        public Dictionary<string, TaskBase> TaskList = new Dictionary<string, TaskBase>();

        /// <summary>
        /// 任务配置列表
        /// </summary>
        public static Dictionary<string, TaskConfig> ConfigItemList = new Dictionary<string, TaskConfig>();

        public static Dictionary<string, DateTime> LastEditedTime = new Dictionary<string, DateTime>();


        /// <summary>
        /// 错误事件
        /// </summary>
        public event ErrorAgent ErrorEvent;


        /// <summary>
        /// 错误事件
        /// </summary>
        public event DebugAgent DeBugEvent;

        /// <summary>
        /// 获取配置文件锁
        /// </summary>
        private object _xmlOptLock = new object();

        /// <summary>
        /// 当前路径
        /// </summary>
        private string _CurrentPath = "";

        /// <summary>
        /// 
        /// </summary>
        private bool _IsTaskEdited = false;

        /// <summary>
        /// 初始化任务管理器
        /// </summary>
        /// <param name="interval">任务扫描间隔</param>
        /// <param name="assemblyPath"> </param>
        public TaskManager(double interval, string assemblyPath)
        {
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                assemblyPath = "\\" + assemblyPath;
            }
            _CurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + assemblyPath);
            tt = new Timer(interval);
            tt.Elapsed += new ElapsedEventHandler(tt_Elapsed);
           
        }

        /// <summary>
        /// 任务管理器启动
        /// </summary>
        public void StartService()
        {
            try
            {
                tt.Start();
            }
            catch (Exception ex)
            {
                if (ErrorEvent != null)
                {
                    ErrorEvent(ex);
                }
            }
        }

        /// <summary>
        /// 任务服务停止（所有任务停止）
        /// </summary>
        /// <returns></returns>
        public void CloseService()
        {
            try
            {
                if (TaskList.Count > 0)
                {
                    foreach (var taskBase in TaskList)
                    {
                        taskBase.Value.Stop();
                    }
                }
                TaskList = null;
               
            }
            catch (Exception ex)
            {
                if (ErrorEvent != null)
                {
                    ErrorEvent(ex);
                }
            }
        }
      
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tt_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                tt.Stop();

                _InitTaskConfig();

                _InitTask();

                #region 启动/停止 服务项

                foreach (var taskBase in TaskList)
                {
                    taskBase.Value.CurrentTime = DateTime.Now;
                    taskBase.Value.ActionTask();
                }

                #endregion

                tt.Start();

            }
            catch (Exception ex)
            {
                if (ErrorEvent != null)
                {
                    ErrorEvent(ex);
                }
            }
        }

        private Type[] GetAssemblyFromMemory(string strFile)
        {

            try
            {
                Assembly a = Assembly.LoadFrom(strFile);
                Type[] mytypes = a.GetTypes();
                return mytypes;
            }
            catch (Exception ex)
            {
                if (ErrorEvent != null)
                {
                    ErrorEvent(ex);
                }
                return null;
            }
        }

        /// <summary>
        /// 判断文件是否刚刚被修改过 时间范围为 60秒内
        /// </summary>
        private bool _FileJustModify(string _filePath)
        {
            bool ismodify = false;
            if (File.Exists(_filePath))
            {
                FileInfo fi = new FileInfo(_filePath);
                if (!LastEditedTime.ContainsKey(_filePath))
                {
                    LastEditedTime.Add(_filePath, fi.LastWriteTime);
                }
                if (fi.LastWriteTime != LastEditedTime[_filePath])
                {
                    ismodify = true;
                    LastEditedTime[_filePath] = fi.LastWriteTime;
                }
            }
            return ismodify;
        }

        /// <summary>
        /// 初始化 Task配置字典
        /// </summary>
        private void _InitTaskConfig()
        {
            #region 初始化 配置字典

            lock (_xmlOptLock)
            {
                string xmlPath = Path.Combine(_CurrentPath + "\\config.xml");
                //了解配置文件信息 ,判断是否操作
                if (_FileJustModify(xmlPath) || ConfigItemList.Count == 0)
                {
                    _IsTaskEdited = true;
                    ConfigItemList.Clear();
                    TaskList.Clear();

                    XmlDocument doc = new XmlDocument();
                    if (File.Exists(xmlPath))
                    {
                        doc.Load(xmlPath);
                        XmlNodeList ScheduleItems = doc.SelectNodes("config/ScheduleItem");
                        foreach (XmlNode SNode in ScheduleItems)
                        {
                            string tastName = SNode.Attributes["Name"].Value;
                            string startTime = "";
                            string endTime = "";
                            string EOffDay = "";
                            string EOnDay = "";
                            bool IsLoop = false;
                            bool IsWeekEndStop = false;
                            int interval = 5;

                            XmlNodeList nodes = SNode.ChildNodes;
                            foreach (XmlNode childNode in nodes)
                            {
                                if (childNode.FirstChild != null)
                                {
                                    switch (childNode.Name)
                                    {
                                        case "StartTime":
                                            startTime = childNode.FirstChild.Value.ToLower();
                                            break;

                                        case "EndTime":
                                            endTime = childNode.FirstChild.Value.ToLower();
                                            break;

                                        case "EspecialDay_Off":
                                            EOffDay = childNode.FirstChild.Value.ToLower();
                                            break;

                                        case "EspecialDay_On":
                                            EOnDay = childNode.FirstChild.Value.ToLower();

                                            break;
                                        case "IsLoop":
                                            IsLoop = childNode.FirstChild.Value.ToLower() == "true";
                                            break;
                                        case "IsWeekEndStop":
                                            IsWeekEndStop = childNode.FirstChild.Value.ToLower() == "true";
                                            break;
                                        case "IntervalSeconds":
                                            interval = Convert.ToInt32(childNode.FirstChild.Value.ToLower());
                                            break;
                                    }
                                }

                            }
                            TaskConfig tConfig = new TaskConfig(startTime, endTime,
                                                                EOffDay, EOnDay,
                                                                IsLoop, interval, IsWeekEndStop);
                            if (DeBugEvent != null)
                            {
                                DeBugEvent("Config加载：" + tastName + "-启动：" + tConfig.NextStartTime + "-" + "结束：" + tConfig.NextStopTime);
                            }
                            ConfigItemList.Add(tastName, tConfig);
                        }

                    }
                    else
                    {
                        _IsTaskEdited = false;
                    }
                }
            }
            #endregion
        }

        /// <summary>
        ///  初始化 加载服务项
        /// </summary>
        private void _InitTask()
        {
            #region 初始化 服务项

            if (_IsTaskEdited)
            {
                DirectoryInfo d = new DirectoryInfo(_CurrentPath);
                if (d.GetFiles("*.dll").Length != 0)
                {
                    string[] strFiles = Directory.GetFiles(_CurrentPath, "*.dll");

                    foreach (string strFile in strFiles)
                    {
                        Type[] mytypes = null;

                        mytypes = GetAssemblyFromMemory(strFile);

                        if (mytypes == null)
                        {
                            continue;
                        }

                        foreach (Type t in mytypes)
                        {
                            if (!string.IsNullOrEmpty(t.FullName) && !TaskList.ContainsKey(t.FullName))
                            {
                                var _baseType = t.BaseType;
                                var _tackBase = typeof (TaskBase);
                                if (_baseType == _tackBase && ConfigItemList.ContainsKey(t.Name))
                                {
                                    TaskConfig _config = ConfigItemList[t.Name];
                                    object obj = Activator.CreateInstance(t, t.Name, _config);
                                    TaskBase tBase = (TaskBase) obj;
                                    tBase.DeBugEvent += new DebugAgent(tBase_DeBugEvent);                                  
                                    TaskList.Add(t.FullName, tBase);
                                }
                            }
                        }
                    }
                }
            }

            #endregion
        }

        void tBase_DeBugEvent(string debug)
        {
            if (DeBugEvent != null)
            {
                DeBugEvent(debug);
            }
        }

        protected virtual  void InitLog(string str)
        {

        }

    }



    /// <summary>
    /// 代理
    /// </summary>
    /// <param name="ex"> </param>
    public delegate void ErrorAgent(Exception ex);

    /// <summary>
    /// 代理
    /// </summary>
    /// <param name="ex"> </param>
    public delegate void DebugAgent(string debug);
}
