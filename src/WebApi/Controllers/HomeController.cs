using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Caching.Memory;
using WebApi.ResponseModels;

namespace WebApi.Controllers
{
    // 注意: 不能使用 [ApiController] + ControllerBase, 因为不支持 Controller.Action 可选参数 ( string jsurl = "" ), 始终会返回 json 格式的错误信息: 某参数是必需的

    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Route("")]
        [HttpGet]
        public async Task<BaseResponseModel> Get()
        {
            return await Task.FromResult(new BaseResponseModel
            {
                Code = 1,
                Message = "运行中"
            });
        }
    }
}
