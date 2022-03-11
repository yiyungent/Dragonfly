

# 入门

- [任务控制台](/plugins/WebMonitorPlugin)


- [设置](/plugincore/admin/index.html#/plugins/settings/WebMonitorPlugin) 里配置提醒通知



# JavaScript 条件 API

```javascript
// 注意: 此方法已废弃
// window.WebMonitorPlugin.JavaScriptConditionResult = false;

// 设置 JavaScript 条件 的结果, 若为 true, 则执行预定(通知)任务
localStorage.setItem("WebMonitorPlugin.JavaScriptConditionResult", false);

// 通过在 js 条件中设置 ForceWaitAfterJsConditionExecute 来改变预先设置的 js 条件 执行后强制等待, 默认为 预先设置的值
localStorage.setItem("WebMonitorPlugin.ForceWaitAfterJsConditionExecute", false);

// 仅当 预定(通知)任务 成功后, 才会判断此设置, 否则一律不禁用本任务
// 默认 false, 即 当预定(通知)任务 成功后, 本任务禁用
localStorage.setItem("WebMonitorPlugin.Enable", false);

// 更多 API 蓄力中...
```


# 执行流程

`浏览器打开 Url` -> `强制等待` -> `设置浏览器窗口大小` -> `初始化 JavaScript 条件 API` -> `浏览器在当前页面 执行 JavaScript 条件` 
-> `从 条件API 中获取结果` -> `强制等待` -> `从 JavaScriptConditionResult 获取是否执行 预定(通知)任务`

`若执行预定(通知)任务` -> `网页截图` -> `执行通知`

`若通知成功` -> `从 Enable 中获取是否禁用此任务 (防止条件后续依旧成立, 而导致重复提醒)`



# 例子


## 百度贴吧自动签到并通知 (使用 Cookie 免账号密码登录)

> 首先在本机上登录百度，F12 获取 名为 `BDUSS` 的 Cookie, 这个就是用于维持百度登录状态的关键

> 设置 `Cookies` 如下

```
name=BDUSS;value=替换为你的BDUSS;domain=.baidu.com;path=/
```

> 注意: Cookie 如需添加多个, 则一行一个

> 设置目标 Url 为 `https://tieba.baidu.com/f?kw=c%23&ie=utf-8`

> 设置 强制等待为 5,     
> 因为我们需要在 JavaScript 条件中，设置 Cookie, 并刷新网页，需要的是登录后的网页，因此没有必要在这里耗费多余时间



> 设置 `JavaScript 条件` 如下:

```javascript
// 此时就处于登录状态了
// 贴吧签到
// 检测是否已经签到
var canSignDom = document.querySelector(".j_signbtn.sign_btn_bright.j_cansign");
if (canSignDom) {
    canSignDom.click();

    // 签到成功, 执行预定通知任务
    localStorage.setItem("WebMonitorPlugin.JavaScriptConditionResult", true);
} else {
    // 已经签到
    localStorage.setItem("WebMonitorPlugin.JavaScriptConditionResult", false);
}


// 标记为 true, 下次依旧执行
localStorage.setItem("WebMonitorPlugin.Enable", true);
```




> 设置 `执行 JavaScript 条件 后 强制等待` 为 5

> 保存

> 效果图

> 暂无