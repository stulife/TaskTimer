using System;

namespace TaskLib
{
    public class TaskConfig
    {
        private string especialDay_off = string.Empty;
        private string especialDay_on = string.Empty;


        /// <summary>
        /// 配置信息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="especialDayOff">范围内</param>
        /// <param name="especialDayOn">范围外</param>
        /// <param name="isLoop">是否循环执行</param>
        /// <param name="intervalSeconds">执行间隔</param>
        /// <param name="isWeekEndStop">周末是否停止</param>
        public TaskConfig(string startTime, string endTime, string especialDayOff, string especialDayOn, bool isLoop, int intervalSeconds,bool isWeekEndStop)
        {
            StartTime = startTime;
            EndTime = endTime;
            this.especialDay_off = especialDayOff;
            this.especialDay_on = especialDayOn;
            IsLoop = isLoop;
            IntervalSeconds = intervalSeconds;
            IsWeekEndStop = isWeekEndStop;
            NextStartTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + StartTime);
            NextStopTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + EndTime);
        }

        /// <summary>
        /// 配置信息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="especialDayOff">范围内</param>
        /// <param name="especialDayOn">范围外</param>
        /// <param name="isLoop">是否循环执行</param>
        /// <param name="intervalSeconds">执行间隔</param>
        public TaskConfig(string startTime, string endTime, string especialDayOff, string especialDayOn, bool isLoop, int intervalSeconds)
        {
            StartTime = startTime;
            EndTime = endTime;
            this.especialDay_off = especialDayOff;
            this.especialDay_on = especialDayOn;
            IsLoop = isLoop;
            IntervalSeconds = intervalSeconds;
            IsWeekEndStop = true;
            NextStartTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + StartTime);
            NextStopTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + EndTime);
        }


        /// <summary>
        /// 开始时间 格式 00:00:00
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间 格式 00:00:00
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 特殊日期,关闭服务(10.1长假,春节)
        /// </summary>
        public string[] EspecialDayOff
        {
            get
            {
                if (string.IsNullOrEmpty(especialDay_off))
                {
                    return null;
                }
                return especialDay_off.Split(',');
            }
        }

        /// <summary>
        /// 特殊日期,启动服务(周六,周日,换休)
        /// </summary>
        public string[] EspecialDayOn
        {
            get
            {
                if (string.IsNullOrEmpty(especialDay_on))
                {
                    return null;
                }
                return especialDay_on.Split(',');
            }
        }

        public DateTime NextStartTime { get; set; }

        public DateTime NextStopTime { get; set; }

        /// <summary>
        /// 是否实时循环扫描
        /// </summary>
        public bool IsLoop { get; set; }

        /// <summary>
        /// 间隔时间
        /// </summary>
        public int IntervalSeconds { get; set; }

        public bool IsWeekEndStop { get; set; }


    }
}
