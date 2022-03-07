using Microsoft.AspNetCore.Http;
using PluginCore;
using PluginCore.IPlugins;
using System;
using System.Threading.Tasks;
using System.Linq;
using OpenQA.Selenium.Chrome;
using System.Threading;

namespace WebMonitorPlugin
{
    public class WebMonitorPlugin : BasePlugin/*, ITimeJobPlugin*/
    {
        public WebMonitorPlugin()
        {
        }

        public override (bool IsSuccess, string Message) AfterEnable()
        {
            Console.WriteLine($"{nameof(WebMonitorPlugin)}: {nameof(AfterEnable)}");

            return base.AfterEnable();
        }

        public override (bool IsSuccess, string Message) BeforeDisable()
        {
            Console.WriteLine($"{nameof(WebMonitorPlugin)}: {nameof(BeforeDisable)}");
            return base.BeforeDisable();
        }

        public long SecondsPeriod
        {
            get
            {
                var settings = PluginSettingsModelFactory.Create<SettingsModel>(nameof(WebMonitorPlugin));

                return settings.SecondsPeriod;
            }
        }

        public Task ExecuteAsync()
        {
            var settings = PluginSettingsModelFactory.Create<SettingsModel>(nameof(WebMonitorPlugin));

            var enabledTasks = settings.Tasks.Where(m => m.Enable).ToList();
            foreach (var task in enabledTasks)
            {
                #region 初始化参数选项
                var options = new ChromeOptions();
                // https://stackoverflow.com/questions/59186984/selenium-common-exceptions-sessionnotcreatedexception-message-session-not-crea
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--headless");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--disable-gpu");

                var driver = new ChromeDriver(chromeDriverDirectory: Environment.CurrentDirectory, options, commandTimeout: TimeSpan.FromMinutes(5));
                #endregion

                driver.Navigate().GoToUrl(task.Url);

                #region 强制 wait
                if (task.ForceWait > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(task.ForceWait));
                }
                #endregion

                #region 设置窗口大小
                int width = task.WindowWidth;
                int height = task.WindowHeight;
                if (task.WindowWidth <= 0)
                {
                    // 默认 width
                    string widthStr = driver.ExecuteScript("return document.documentElement.scrollWidth").ToString();
                    width = Convert.ToInt32(widthStr);
                }
                if (task.WindowHeight <= 0)
                {
                    // 默认 height
                    string heightStr = driver.ExecuteScript("return document.documentElement.scrollHeight").ToString();
                    height = Convert.ToInt32(heightStr);
                }
                // https://www.selenium.dev/documentation/webdriver/browser/windows/
                driver.Manage().Window.Size = new System.Drawing.Size(width, height);
                #endregion

                #region js条件
                // 注入 JavaScriptCondition
                if (!string.IsNullOrEmpty(task.JavaScriptCondition))
                {
                    string resultStr = driver.ExecuteScript($"return {task.JavaScriptCondition}").ToString();
                    bool result = Convert.ToBoolean(resultStr);
                    if (result)
                    {
                        // 条件成立, 执行通知
                        //Utils.MailUtil.SendMail(new Utils.MailOptions
                        //{
                        //    Host = settings.Mail.SMTPHost,
                        //    Content = task.Message,
                        //    EnableSsl = settings.Mail.EnableSsl,
                        //    Password = settings.Mail.Password,
                        //    Port = settings.Mail.Port,
                        //    ReceiveAddress = task.ReceiveMail,
                        //    SenderDisplayAddress = settings.Mail.SenderDisplayAddress,
                        //    SenderDisplayName = settings.Mail.SenderDisplayName,
                        //    Subject = task.Message,
                        //    UserName = settings.Mail.UerName
                        //}, out string errorMsg);

                        //if (!string.IsNullOrEmpty(errorMsg))
                        //{
                        //    Console.WriteLine("发送邮件失败: ");
                        //    Console.WriteLine(errorMsg);
                        //}
                        //else
                        //{
                        //    Console.WriteLine("发送邮件成功: ");
                        //    Console.WriteLine(task.ReceiveMail);
                        //    Console.WriteLine(task.Message);
                        //}
                    }
                }
                #endregion

            }

            return Task.CompletedTask;
        }
    }
}
