using System;
using System.Collections.Generic;
using System.Text;

namespace WebMonitorPlugin.Models
{
    public class TaskModel
    {
        public string Name { get;  set; }
        public string JsCondition { get;  set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public int ForceWait { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }

        public bool Enable { get; set; }
    }
}
