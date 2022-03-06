using PluginCore.Models;
using System.Collections.Generic;

namespace WebMonitorPlugin
{
    public class SettingsModel : PluginSettingsModel
    {
        public long SecondsPeriod { get; set; }

        public MailModel Mail { get; set; }

        public List<TaskModel> Tasks { get; set; }

        public class MailModel
        {
            public string SMTPHost { get; set; }
            public string UerName { get; set; }
            public string Password { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
            public string SenderDisplayAddress { get; set; }
            public string SenderDisplayName { get; set; }
        }

        public class TaskModel
        {
            public string Message { get; set; }
            public string Url { get; set; }
            public string JavaScriptCondition { get; set; }
            public int ForceWait { get; set; }
            public bool Enable { get; set; }
            public int WindowWidth { get; set; }
            public int WindowHeight { get; set; }

            public string ReceiveMail { get; set; }
        }


    }
}
