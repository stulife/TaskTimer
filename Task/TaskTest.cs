using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskLib;

namespace Task
{
    class TaskTest:TaskBase
    {
        public TaskTest(string taskName, TaskConfig tconfig)
            : base(taskName, tconfig)
        { }
        public override void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("test");
        }
    }
}
