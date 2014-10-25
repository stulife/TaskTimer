using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace TPLib.Task
{
  
    /// <summary>
    /// 计时任务基类
    /// </summary>
    public abstract class TaskBase
    {

        /// <summary>
        /// 错误事件
        /// </summary>
        public event DebugAgent DeBugEvent;

        private Timer Ti;
        public int Interval;
        public bool IsStop = true;
        public bool IsRun;
       
        public DateTime CurrentTime;
        public TaskConfig Config;
        public string TaskName;
        public bool IsAction = false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="tconfig">任务Config配置</param>
        protected TaskBase(string taskName,TaskConfig tconfig)
        {
            TaskName = taskName;
            Interval = tconfig.IntervalSeconds;
            CurrentTime = DateTime.Now;
            Config = tconfig;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (Ti == null)
            {
                Ti = new Timer(Interval * 1000);
                Ti.Elapsed += new ElapsedEventHandler(TimerEventHander);
            }
            Ti.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void StopDispose()
        {
            IsRun = false;
            if (Ti != null)
            {
                Ti.Stop();
                Ti.Dispose();
                Ti = null;
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Stop()
        {
            if (Ti != null)
            {
                Ti.Stop();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ActionTask()
        {
            if (!IsRun)
            {
                if (Config.IsWeekEndStop
                    && (CurrentTime.DayOfWeek == DayOfWeek.Sunday || CurrentTime.DayOfWeek == DayOfWeek.Saturday)
                    && !_CompareEspecialDay(Config.EspecialDayOn))
                {
                    if (Config.NextStartTime.Date == CurrentTime.Date)
                    {
                        Config.NextStartTime = Config.NextStartTime.AddDays(1);
                        BugRecord(Config.NextStartTime.ToString(), TaskName + "-节假日调整下次启动时间");
                        Config.NextStopTime = Config.NextStopTime.AddDays(1);
                        BugRecord(Config.NextStopTime.ToString(), TaskName + "-节假日调整下次结束时间");
                    }
                    return;
                }

                if (_CompareEspecialDay(Config.EspecialDayOff))
                {
                    if (Config.NextStartTime.Date == CurrentTime.Date)
                    {
                        Config.NextStartTime = Config.NextStartTime.AddDays(1);
                        BugRecord(Config.NextStartTime.ToString(), TaskName + "-特殊日调整下次启动时间");
                        Config.NextStopTime = Config.NextStopTime.AddDays(1);
                        BugRecord(Config.NextStopTime.ToString(), TaskName + "-特殊日调整下次结束时间");

                    }
                    return;
                }

                TimeSpan ts = CurrentTime - Config.NextStartTime;
                int iSeconds = Convert.ToInt32(ts.TotalSeconds);
                if (iSeconds > 60)
                {
                    Config.NextStartTime = Config.NextStartTime.AddDays(1);
                    if (Config.IsLoop)
                    {
                        BugRecord(Config.NextStartTime.ToString(), TaskName + "运行-下次启动时间");
                        StopDispose();
                        Start();
                        IsRun = true;
                    }
                    else
                    {
                        BugRecord(Config.NextStartTime.ToString(), TaskName + "-下次启动时间-当前不运行");
                    }
                  
                }

                if (iSeconds >= 0 && iSeconds <= 60)
                {
                    Config.NextStartTime = Config.NextStartTime.AddDays(1);
                    BugRecord(Config.NextStartTime.ToString(), TaskName + "运行-下次启动时间");
                    StopDispose();
                    Start();
                    IsRun = true;
                }
            }
            else
            {
                TimeSpan ts = CurrentTime - Config.NextStopTime;
                int iSeconds = Convert.ToInt32(ts.TotalSeconds);
                if (iSeconds >= 0 && iSeconds <= 60)
                {
                    Config.NextStopTime = Config.NextStopTime.AddDays(1);
                    BugRecord(Config.NextStopTime.ToString(), TaskName + "停止-下次停止时间");
                    StopDispose();
                    IsRun = false;
                }

                if (iSeconds > 60)
                {
                    Config.NextStopTime = Config.NextStopTime.AddDays(1);
                    BugRecord(Config.NextStopTime.ToString(), TaskName + "-下次停止时间-当前不停止运行");
                }
            }
        }


        /// <summary>
        /// 计时执行函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void TimerElapsed(object sender, ElapsedEventArgs e);

        /// <summary>
        /// 计时执行函数错误处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerEventHander(object sender, ElapsedEventArgs e)
        {
            try
            {

                Stop();
                TimerElapsed(sender,e);
                OptLogRecord();
                if (!Config.IsLoop)
                {
                    StopDispose();
                }
                else
                {
                    Start();
                }
            }
            catch (Exception ex)
            {
                ErrorSet(ex);
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ex">错误信息</param>
        public virtual void ErrorSet(Exception ex)
        {
            return;
        }

        /// <summary>
        /// 操作日志记录
        /// </summary>
        public virtual void OptLogRecord()
        {
            return;
        }


        /// <summary>
        /// 操作日志记录
        /// </summary>
        public virtual void BugRecord(string datetime, string des)
        {
            if (DeBugEvent!=null)
            {
                DeBugEvent(des + ":" + datetime);
            }
        }


        private bool _CompareEspecialDay(string[] especialDay)
        {
            if (especialDay != null)
            {
                foreach (var item in especialDay)
                {
                    if (item == CurrentTime.ToString("yyyy-MM-dd"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
