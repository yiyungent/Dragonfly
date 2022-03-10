﻿using Microsoft.AspNetCore.Http;
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
using WebMonitorPlugin.Infrastructure;
using WebMonitorPlugin.Models;
using System.Collections.Generic;

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

            var enabledTasks = TaskManager.Tasks().Where(m => m.Enable).ToList();
            for (int i = 0; i < enabledTasks.Count; i++)
            {
                ExecuteTask(settings, enabledTasks[i]);
            }
            IsRunning = false;

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {nameof(WebMonitorPlugin)}.ExecuteAsync 运行完成");

            return Task.CompletedTask;
        }


        public void ExecuteTask(SettingsModel settings, TaskModel task)
        {
            #region 防止多线程同时执行, A线程使用时, 其它线程阻塞直到A线程完成, 保证 ChromeDriver 单个执行
            lock (_taskLock)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 执行任务 {task.Name}");

                #region 测试
#if DEBUG
                //Thread.Sleep(30 * 1000);
                //return;
#endif
                #endregion

                #region 初始化参数选项
                var options = new ChromeOptions();
                // https://stackoverflow.com/questions/59186984/selenium-common-exceptions-sessionnotcreatedexception-message-session-not-crea
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
#if DEBUG
                // 本地开发
#else
                options.AddArgument("--headless");
#endif

                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--disable-gpu");

                var driver = new ChromeDriver(chromeDriverDirectory: Environment.CurrentDirectory, options, commandTimeout: TimeSpan.FromMinutes(5));
                #endregion

                try
                {
                    #region 设置 Cookie
                    // 在 访问 Url 之前 设置 Cookie
                    // Adds the cookie into current browser context
                    //foreach (var cookie in task.Cookies)
                    //{

                    //}
                    //driver.Manage().Cookies.AddCookie(new Cookie("key", "value"));
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
                    task.JsCondition = TaskManager.Task(task.Name).JsCondition;
                    if (!string.IsNullOrEmpty(task.JsCondition))
                    {
                        #region 废弃, 使用 localStorage.setItem 代替
                        //driver.ExecuteScript("window.WebMonitorPlugin = {};");
                        //driver.ExecuteScript("window.WebMonitorPlugin.JavaScriptConditionResult = false;"); 
                        #endregion

                        driver.ExecuteScript("localStorage.setItem(\"WebMonitorPlugin.JavaScriptConditionResult\", false)");

                        // 废弃: 在 内部 改变 window.WebMonitorPlugin.JavaScriptConditionResult 的值
                        // 注意: 在 JsCondition 中执行刷新页面操作, 会导致 之前执行的 js 变量清空, 从而导致获取不到 JavaScriptConditionResult
                        // localStorage 不会因为刷新页面清空, 因此推荐使用 localStorage.setItem("WebMonitorPlugin.JavaScriptConditionResult", true);
                        try
                        {
                            driver.ExecuteScript(task.JsCondition);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("JavaScript 条件 执行出错:");
                            Console.WriteLine(ex.ToString());
                        }

                        //string resultStr = driver.ExecuteScript($"return window.WebMonitorPlugin.JavaScriptConditionResult")?.ToString() ?? null;
                        string resultStr = driver.ExecuteScript($"return localStorage.getItem(\"WebMonitorPlugin.JavaScriptConditionResult\")")?.ToString() ?? null;
                        Console.WriteLine($"JavaScriptConditionResult: {resultStr}");

                        #region 执行 JavaScript 条件后, 强制等待
                        if (task.ForceWaitAfterJsConditionExecute > 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(task.ForceWaitAfterJsConditionExecute));
                        }
                        #endregion

                        if (!string.IsNullOrEmpty(resultStr))
                        {
                            bool result = Convert.ToBoolean(resultStr);
                            if (result)
                            {
                                // 条件成立
                                Console.WriteLine($"JavaScript 条件 成立, 执行预订任务");

                                #region 预订任务
                                try
                                {

                                    #region 截图
                                    // 保存截图
                                    // https://www.selenium.dev/documentation/webdriver/browser/windows/#takescreenshot
                                    byte[] screenshotBytes = null;
                                    try
                                    {
                                        Screenshot screenshot = null;
                                        screenshot = (driver as ITakesScreenshot).GetScreenshot();
                                        // 直接用 图片数据
                                        screenshotBytes = screenshot.AsByteArray;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                    #endregion

                                    // 条件成立, 执行通知
                                    TaskNotify(settings, task, screenshotBytes);

                                    // 任务完成，设置为禁用
                                    task.Enable = false;
                                    TaskManager.AddTask(task);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("预订任务执行出错:");
                                    Console.WriteLine(ex.ToString());
                                }
                                #endregion
                            }
                            else
                            {
                                Console.WriteLine($"JavaScript 条件 不成立, 放弃预订任务");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"JavaScript 条件 不成立, 放弃预订任务");
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


        public void TaskNotify(SettingsModel settings, TaskModel task, byte[] screenshotBytes)
        {
            #region 邮件
            if (settings.Mail != null)
            {
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
            }
            #endregion

            #region Telegram
            if (settings.Telegram != null)
            {
                if (settings.Telegram.Enable)
                {
                    string chatId = settings.Telegram.ChatId;
                    var botClient = new TelegramBotClient(settings.Telegram.Token);
                    Console.WriteLine($"Telegram.ChatId: {chatId}");
                    Console.WriteLine($"Telegram.Token: {settings.Telegram.Token}");
                    // TODO: Telegram.Bot.Exceptions.ApiRequestException: Bad Request: can't parse entities: Character '-' is reserved and must be escaped with the preceding '\'
                    // text: 不能使用 ---
                    // https://core.telegram.org/bots/api#formatting-options
                    Message message = botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $@"<strong>任务: {task.Name?.Trim() ?? "匿名"}</strong>
                         <a href='{task.Url}'>{task.Url}</a>

                         {task.Message}",
                        parseMode: ParseMode.Html,
                        //parseMode: ParseMode.MarkdownV2,
                        //disableNotification: true,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithUrl(
                                "Click Url",
                                $"{task.Url}")))
                        .Result;
                    if (screenshotBytes != null && screenshotBytes.Length > 0)
                    {
                        using (MemoryStream stream = new MemoryStream(screenshotBytes))
                        {
                            InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, $"screenshot-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.png");
                            botClient.SendDocumentAsync(chatId: chatId, inputOnlineFile).Wait();
                        }
                    }
                }
            }
            #endregion

        }
    }
}
