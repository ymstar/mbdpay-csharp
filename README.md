# 面包多Pay C# SDK

面向个人的支付集成平台 [面包多Pay](https://mbd.pub) 的 C# / .NET SDK。

## 安装

### NuGet

```bash
dotnet add package MbdPay
```

### 手动编译

```bash
git clone https://github.com/yangming/mbdpay-csharp.git
cd mbdpay-csharp
dotnet build
```

## 使用

### 初始化

```csharp
using MbdPay;

var client = new Client("your_app_id", "your_app_key");
```

### 微信 JSAPI 支付

```csharp
var result = await client.WxJsapiAsync(
    openid: "用户openid",
    description: "商品描述",
    amountTotal: 100,               // 金额，单位：分
    callbackUrl: "https://callback",
    outTradeNo: "ORDER_001"          // 可为null
);
// result["appId"], result["timeStamp"], result["nonceStr"], ...
```

### 微信 H5 支付

```csharp
var result = await client.WxH5Async("商品描述", 100, "ORDER_001");
var h5Url = result["h5_url"].GetString();
```

### 支付宝支付

```csharp
var result = await client.AlipayAsync(
    url: "https://callback",
    description: "商品描述",
    amountTotal: 100,
    outTradeNo: "ORDER_001"
);
var formHtml = result["body"].GetString();
```

### 退款

```csharp
var result = await client.RefundAsync("面包多订单号");
```

### 查询订单

```csharp
var result = await client.SearchOrderAsync("你的订单号");
```

### 获取用户 OpenID 跳转链接

```csharp
var url = client.GetOpenidRedirectUrl("https://your-site.com/openid_callback");
```

## 签名算法

参数按 key 排序后拼接为 `key1=value1&key2=value2`，末尾追加 `&key=APP_KEY`，取 MD5。

文档：https://doc.mbd.pub/api/qian-ming-suan-fa

## License

MIT
