using Microsoft.AspNetCore.Http;
using PluginCore;
using PluginCore.IPlugins;
using System;
using System.Threading.Tasks;
using System.Linq;
using OpenQA.Selenium.Chrome;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using OpenQA.Selenium;

namespace WebMonitorPlugin
{
    public class WebMonitorPlugin : BasePlugin, ITimeJobPlugin
    {
        public static bool IsRunning { get; set; }

        private static readonly object _taskLock = new object();

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
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {nameof(WebMonitorPlugin)}.ExecuteAsync 进入");

            #region 防止多线程同时执行, 同时又不会导致其它线程阻塞, 而是直接放弃本次执行
            // 注意: 其实经测试, 最新版 PluginCore 已修复问题, 不再需要
            if (IsRunning)
            {
                return Task.CompletedTask;
            }
            IsRunning = true;
            #endregion

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {nameof(WebMonitorPlugin)}.ExecuteAsync 准备执行任务");

            var settings = PluginSettingsModelFactory.Create<SettingsModel>(nameof(WebMonitorPlugin));

            var enabledTasks = settings.Tasks.Where(m => m.Enable).ToList();
            for (int i = 0; i < enabledTasks.Count; i++)
            {
                ExecuteTask(settings, taskIndex: i, enabledTasks[i]);
            }
            IsRunning = false;

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {nameof(WebMonitorPlugin)}.ExecuteAsync 运行完成");

            return Task.CompletedTask;
        }


        public void ExecuteTask(SettingsModel settings, int taskIndex, SettingsModel.TaskModel task)
        {
            #region 防止多线程同时执行, A线程使用时, 其它线程阻塞直到A线程完成, 保证 ChromeDriver 单个执行
            lock (_taskLock)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 执行任务 {task.Name}");

                #region 测试
                //Thread.Sleep(5000);
                //return;
                #endregion

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

                try
                {
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
                        //string resultStr = driver.ExecuteScript($"return {task.JavaScriptCondition}").ToString();
                        string resultStr = driver.ExecuteScript($"return {task.JavaScriptCondition}").ToString();
                        bool result = Convert.ToBoolean(resultStr);
                        Console.WriteLine($"JavaScriptCondition: resultStr: {resultStr}");
                        if (result)
                        {
                            try
                            {
                                #region 截图
                                // 保存截图
                                // https://www.selenium.dev/documentation/webdriver/browser/windows/#takescreenshot
                                Screenshot screenshot = (driver as ITakesScreenshot).GetScreenshot();
                                // 直接用 图片数据
                                byte[] screenshotBytes = screenshot.AsByteArray;
                                #endregion

                                // 条件成立, 执行通知
                                TaskNotify(settings, task, screenshotBytes);

                                // 任务完成，设置为禁用
                                task.Enable = false;
                                settings.Tasks[taskIndex] = task;
                                PluginSettingsModelFactory.Save(settings, nameof(WebMonitorPlugin));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }


                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    driver.Quit();
                }

                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 执行任务 完成 {task.Name}");
            }
            #endregion

        }


        public void TaskNotify(SettingsModel settings, SettingsModel.TaskModel task, byte[] screenshotBytes)
        {
            #region 邮件
            if (settings.Mail.Enable)
            {
                //Utils.MailUtil.SendMail(new Utils.MailOptions
                //{
                //    Host = settings.Mail.SMTPHost,
                //    Content = task.Message,
                //    EnableSsl = settings.Mail.EnableSsl,
                //    Password = settings.Mail.Password,
                //    Port = settings.Mail.Port,
                //    ReceiveAddress = settings.Mail.ReceiveMail,
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
                //    Console.WriteLine(settings.Mail.ReceiveMail);
                //    Console.WriteLine(task.Message);
                //}
            }
            #endregion

            #region Telegram
            if (settings.Telegram.Enable)
            {
                string chatId = settings.Telegram.ChatId;
                var botClient = new TelegramBotClient(settings.Telegram.Token);

                Message message = botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $@"任务: *{task.Name}*
                         ---
                         {task.Url}
                         ---
                         {task.Message}",
                    parseMode: ParseMode.MarkdownV2,
                    //disableNotification: true,
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl(
                            "Click Url",
                            $"{task.Url}")))
                    .Result;

                using (MemoryStream stream = new MemoryStream(screenshotBytes))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, $"screenshot-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.png");
                    botClient.SendDocumentAsync(chatId: chatId, inputOnlineFile).Wait();
                }
            }
            #endregion

        }
    }
}
