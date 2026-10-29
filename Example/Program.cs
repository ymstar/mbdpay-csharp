using System;
using System.Text.Json;
using MbdPay;

// 面包多Pay C# SDK 使用示例
// 运行前请替换为你自己的 app_id 和 app_key
// 获取地址: https://mbd.pub/dev

const string appId = "your_app_id";
const string appKey = "your_app_key";

using var client = new Client(appId, appKey);

// === 1. 获取用户 openid 跳转链接 ===
Console.WriteLine("=== 获取 openid 跳转链接 ===");
var url = client.GetOpenidRedirectUrl("https://your-site.com/openid_callback");
Console.WriteLine($"跳转URL: {url}");
Console.WriteLine();

// === 2. 微信 JSAPI 支付 ===
Console.WriteLine("=== 微信 JSAPI 支付 ===");
try
{
    var result = await client.WxJsapiAsync(
        openid: "用户openid",
        description: "测试商品",
        amountTotal: 100,
        callbackUrl: "https://your-site.com/pay/callback",
        outTradeNo: "ORDER_001"
    );
    Console.WriteLine($"JSAPI参数: {JsonSerializer.Serialize(result)}");
}
catch (MbdPayException e)
{
    Console.WriteLine($"错误: {e.Message}");
}
Console.WriteLine();

// === 3. 微信 H5 支付 ===
Console.WriteLine("=== 微信 H5 支付 ===");
try
{
    var result = await client.WxH5Async(
        description: "测试商品",
        amountTotal: 100,
        outTradeNo: "ORDER_002"
    );
    Console.WriteLine($"H5支付链接: {result["h5_url"].GetString()}");
}
catch (MbdPayException e)
{
    Console.WriteLine($"错误: {e.Message}");
}
Console.WriteLine();

// === 4. 支付宝支付 ===
Console.WriteLine("=== 支付宝支付 ===");
try
{
    var result = await client.AlipayAsync(
        url: "https://your-site.com/pay/callback",
        description: "测试商品",
        amountTotal: 100,
        outTradeNo: "ORDER_003"
    );
    var body = result["body"].GetString() ?? "";
    Console.WriteLine($"支付宝表单HTML: {body[..Math.Min(100, body.Length)]}...");
}
catch (MbdPayException e)
{
    Console.WriteLine($"错误: {e.Message}");
}
Console.WriteLine();

// === 5. 查询订单 ===
Console.WriteLine("=== 查询订单 ===");
try
{
    var result = await client.SearchOrderAsync("ORDER_001");
    Console.WriteLine($"订单信息: {JsonSerializer.Serialize(result)}");
}
catch (MbdPayException e)
{
    Console.WriteLine($"错误: {e.Message}");
}
Console.WriteLine();

// === 6. 退款 ===
Console.WriteLine("=== 退款 ===");
try
{
    var result = await client.RefundAsync("面包多订单号");
    Console.WriteLine($"退款结果: {JsonSerializer.Serialize(result)}");
}
catch (MbdPayException e)
{
    Console.WriteLine($"错误: {e.Message}");
}
