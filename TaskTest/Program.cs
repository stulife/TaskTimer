using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskLib;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskManager task = new TaskManager(5000, "task");
            task.StartService();
            Console.ReadLine();
        }
    }
}
