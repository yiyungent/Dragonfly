using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluginCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebMonitorPlugin.Infrastructure;
using WebMonitorPlugin.Models;
using WebMonitorPlugin.RequestModels;
using WebMonitorPlugin.ResponseModels;

namespace WebMonitorPlugin.Controllers
{
    [ApiController]
    [Authorize("PluginCoreAdmin")]
    [Route($"plugins/{nameof(WebMonitorPlugin)}")]
    public class HomeController : ControllerBase
    {
        [Route("")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            string pluginWwwroot = PluginPathProvider.PluginWwwRootDir(nameof(WebMonitorPlugin));
            string pluginTemplates = System.IO.Path.Combine(pluginWwwroot, "templates");
            string layoutStr = System.IO.File.ReadAllText(System.IO.Path.Combine(pluginTemplates, "layoutMain.html"), System.Text.Encoding.UTF8);
            string targetStr = System.IO.File.ReadAllText(System.IO.Path.Combine(pluginTemplates, "index.html"), System.Text.Encoding.UTF8);
            string rtnStr = layoutStr.Replace("{{content}}", targetStr);


            StringBuilder sb = new StringBuilder();
            var taskList = TaskManager.Tasks();
            foreach (var task in taskList)
            {
                // http://www.jsons.cn/htmltojsp/
                sb.AppendFormat($"<tr data-tr-id=\"{task.Name}\">");
                sb.AppendFormat($"	<td>{task.Name}</td>");
                sb.AppendFormat("	<td>");
                sb.AppendFormat($"		<span class=\"label label-info\">{task.WindowWidth}</span>");
                sb.AppendFormat("	</td>");
                sb.AppendFormat("	<td>");
                sb.AppendFormat($"		<span class=\"label label-info\">{task.WindowHeight}</span>");
                sb.AppendFormat("	</td>");
                sb.AppendFormat("	<td>");
                sb.AppendFormat($"		<span class=\"label label-info\">{task.ForceWait}</span>");
                sb.AppendFormat("	</td>");
                sb.AppendFormat("	<td>");
                sb.AppendFormat($"		<span class=\"label label-info\">{(task.Enable ? "true" : "false")}</span>");
                sb.AppendFormat("	</td>");
                sb.AppendFormat("	<td>");
                sb.AppendFormat("		<div class=\"btn-group\" role=\"group\" data-pjax>");
                sb.AppendFormat($"			<a class=\"btn btn-warning\" href=\"/plugins/{nameof(WebMonitorPlugin)}/Edit?id={task.Name}\">编辑</a>");
                sb.AppendFormat($"			<button class=\"btn btn-danger\" type=\"button\" onclick=\"deleteOp('{task.Name}')\">删除</button>");
                sb.AppendFormat("		</div>");
                sb.AppendFormat("	</td>");
                sb.AppendFormat("</tr>");
                sb.AppendFormat("");
            }
            rtnStr = rtnStr.Replace("{{Table.Trs}}", sb.ToString());

            return await Task.FromResult(Content(rtnStr, "text/html; charset=utf-8"));
        }

        [Route(nameof(Create))]
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            string pluginWwwroot = PluginPathProvider.PluginWwwRootDir(nameof(WebMonitorPlugin));
            string pluginTemplates = System.IO.Path.Combine(pluginWwwroot, "templates");
            string layoutStr = System.IO.File.ReadAllText(System.IO.Path.Combine(pluginTemplates, "layoutMain.html"), System.Text.Encoding.UTF8);
            string targetStr = System.IO.File.ReadAllText(System.IO.Path.Combine(pluginTemplates, "create.html"), System.Text.Encoding.UTF8);
            string rtnStr = layoutStr.Replace("{{content}}", targetStr);

            return await Task.FromResult(Content(rtnStr, "text/html; charset=utf-8"));
        }

        [Route(nameof(Create))]
        [HttpPost]
        public async Task<BaseResponseModel> Create([FromBody] TaskCreateRequestModel requestModel)
        {
            BaseResponseModel responseModel = new BaseResponseModel();
            try
            {
                TaskManager.AddTask(new Models.TaskModel
                {
                    Name = requestModel.Name,
                    Message = requestModel.Message,
                    ForceWait = requestModel.ForceWait,
                    JsCondition = requestModel.JsCondition,
                    Url = requestModel.Url,
                    WindowWidth = requestModel.WindowWidth,
                    WindowHeight = requestModel.WindowHeight,
                    Enable = requestModel.Enable
                });

                responseModel.Code = 1;
                responseModel.Message = "成功";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseModel.Code = -1;
                responseModel.Message = "失败";
            }

            return await Task.FromResult(responseModel);
        }

        [Route(nameof(Edit))]
        [HttpGet]
        public async Task<ActionResult> Edit([FromQuery] string id)
        {
            string pluginWwwroot = PluginPathProvider.PluginWwwRootDir(nameof(WebMonitorPlugin));
            string pluginTemplates = System.IO.Path.Combine(pluginWwwroot, "templates");
            string layoutStr = System.IO.File.ReadAllText(System.IO.Path.Combine(pluginTemplates, "layoutMain.html"), System.Text.Encoding.UTF8);
            string targetStr = System.IO.File.ReadAllText(System.IO.Path.Combine(pluginTemplates, "edit.html"), System.Text.Encoding.UTF8);
            string rtnStr = layoutStr.Replace("{{content}}", targetStr);
            TaskModel task = TaskManager.Task(id);
            rtnStr = rtnStr.Replace("{{name}}", task.Name)
                            .Replace("{{url}}", task.Url)
                            .Replace("{{message}}", task.Message)
                            .Replace("{{forceWait}}", task.ForceWait.ToString())
                            .Replace("{{windowWidth}}", task.WindowWidth.ToString())
                            .Replace("{{windowHeight}}", task.WindowHeight.ToString())
                            .Replace("{{enable}}", task.Enable ? "true" : "false");


            return await Task.FromResult(Content(rtnStr, "text/html; charset=utf-8"));
        }

        [Route(nameof(JsCondition))]
        [HttpGet]
        public async Task<ActionResult> JsCondition([FromQuery] string id)
        {
            TaskModel task = TaskManager.Task(id);
            string jsCondition = task.JsCondition;

            return await Task.FromResult(Content(jsCondition, "application/json"));
        }



        [Route(nameof(Edit))]
        [HttpPost]
        public async Task<BaseResponseModel> Edit([FromBody] TaskEditRequestModel requestModel)
        {
            BaseResponseModel responseModel = new BaseResponseModel();
            try
            {
                TaskManager.AddTask(new Models.TaskModel
                {
                    Name = requestModel.Name,
                    Message = requestModel.Message,
                    ForceWait = requestModel.ForceWait,
                    JsCondition = requestModel.JsCondition,
                    Url = requestModel.Url,
                    WindowWidth = requestModel.WindowWidth,
                    WindowHeight = requestModel.WindowHeight,
                    Enable = requestModel.Enable
                });

                responseModel.Code = 1;
                responseModel.Message = "成功";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseModel.Code = -1;
                responseModel.Message = "失败";
            }

            return await Task.FromResult(responseModel);
        }

        [Route(nameof(Delete))]
        [HttpPost]
        public async Task<BaseResponseModel> Delete([FromForm] string id)
        {
            BaseResponseModel responseModel = new BaseResponseModel();
            try
            {
                TaskManager.RemoveTask(id);

                responseModel.Code = 1;
                responseModel.Message = "成功";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseModel.Code = -1;
                responseModel.Message = "失败";
            }

            return await Task.FromResult(responseModel);
        }


    }
}
