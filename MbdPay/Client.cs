using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MbdPay
{
    /// <summary>
    /// 面包多Pay客户端
    /// </summary>
    public class Client : IDisposable
    {
        private const string DefaultDomain = "https://api.mianbaoduo.com";
        private const string OpenidBaseUrl = "https://mbd.pub/openid";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        private readonly string _appId;
        private readonly string _appKey;
        private readonly string _domain;
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null
        };

        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <param name="appId">面包多Pay app_id</param>
        /// <param name="appKey">面包多Pay app_key</param>
        public Client(string appId, string appKey) : this(appId, appKey, DefaultDomain, DefaultTimeout) { }

        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <param name="appId">面包多Pay app_id</param>
        /// <param name="appKey">面包多Pay app_key</param>
        /// <param name="domain">API域名</param>
        /// <param name="timeout">请求超时时间</param>
        public Client(string appId, string appKey, string domain, TimeSpan timeout)
        {
            _appId = appId;
            _appKey = appKey;
            _domain = domain;
            _httpClient = new HttpClient { Timeout = timeout };
        }

        /// <summary>
        /// 获取用户openid的跳转URL
        /// 文档: https://doc.mbd.pub/api/huo-qu-yong-hu-openid
        /// </summary>
        /// <param name="targetUrl">回调URL</param>
        /// <returns>跳转URL</returns>
        public string GetOpenidRedirectUrl(string targetUrl)
        {
            var query = $"app_id={Uri.EscapeDataString(_appId)}&target_url={Uri.EscapeDataString(targetUrl)}";
            return $"{OpenidBaseUrl}?{query}";
        }

        /// <summary>
        /// 微信JSAPI支付
        /// 文档: https://doc.mbd.pub/api/wei-xin-zhi-fu
        /// </summary>
        /// <param name="openid">用户openid</param>
        /// <param name="description">商品描述</param>
        /// <param name="amountTotal">订单金额（单位：分）</param>
        /// <param name="callbackUrl">支付完成后跳转的URL</param>
        /// <param name="outTradeNo">外部系统订单号（可为null）</param>
        /// <returns>JSAPI支付参数</returns>
        public async Task<Dictionary<string, JsonElement>> WxJsapiAsync(
            string openid, string description, int amountTotal,
            string callbackUrl, string? outTradeNo = null)
        {
            var parameters = new Dictionary<string, object?>
            {
                ["openid"] = openid,
                ["description"] = description,
                ["amount_total"] = amountTotal,
                ["callback_url"] = callbackUrl,
                ["out_trade_no"] = outTradeNo
            };
            return await PostAsync("/release/wx/prepay", parameters);
        }

        /// <summary>
        /// 微信H5支付
        /// 文档: https://doc.mbd.pub/api/wei-xin-h5-zhi-fu
        /// </summary>
        /// <param name="description">商品描述</param>
        /// <param name="amountTotal">订单金额（单位：分）</param>
        /// <param name="outTradeNo">外部系统订单号（可为null）</param>
        /// <returns>包含h5_url的响应</returns>
        public async Task<Dictionary<string, JsonElement>> WxH5Async(
            string description, int amountTotal, string? outTradeNo = null)
        {
            var parameters = new Dictionary<string, object?>
            {
                ["channel"] = "h5",
                ["description"] = description,
                ["amount_total"] = amountTotal,
                ["out_trade_no"] = outTradeNo
            };
            return await PostAsync("/release/wx/prepay", parameters);
        }

        /// <summary>
        /// 支付宝支付
        /// 文档: https://doc.mbd.pub/api/zhi-fu-bao-zhi-fu
        /// </summary>
        /// <param name="url">支付后跳转URL</param>
        /// <param name="description">商品描述</param>
        /// <param name="amountTotal">订单金额（单位：分）</param>
        /// <param name="outTradeNo">外部系统订单号（可为null）</param>
        /// <param name="callbackUrl">回调URL（可为null）</param>
        /// <returns>包含body(HTML表单)的响应</returns>
        public async Task<Dictionary<string, JsonElement>> AlipayAsync(
            string url, string description, int amountTotal,
            string? outTradeNo = null, string? callbackUrl = null)
        {
            var parameters = new Dictionary<string, object?>
            {
                ["url"] = url,
                ["description"] = description,
                ["amount_total"] = amountTotal,
                ["out_trade_no"] = outTradeNo,
                ["callback_url"] = callbackUrl
            };
            return await PostAsync("/release/alipay/pay", parameters);
        }

        /// <summary>
        /// 退款
        /// 文档: https://doc.mbd.pub/api/tui-kuan
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <returns>退款结果</returns>
        public async Task<Dictionary<string, JsonElement>> RefundAsync(string orderId)
        {
            var parameters = new Dictionary<string, object?>
            {
                ["order_id"] = orderId
            };
            return await PostAsync("/release/main/refund", parameters);
        }

        /// <summary>
        /// 查询订单
        /// 文档: https://doc.mbd.pub/api/ding-dan-cha-xun
        /// </summary>
        /// <param name="outTradeNo">外部系统订单号</param>
        /// <returns>订单信息</returns>
        public async Task<Dictionary<string, JsonElement>> SearchOrderAsync(string outTradeNo)
        {
            var parameters = new Dictionary<string, object?>
            {
                ["out_trade_no"] = outTradeNo
            };
            return await PostAsync("/release/main/search_order", parameters);
        }

        // ==================== 内部方法 ====================

        private Dictionary<string, object> BuildRequest(Dictionary<string, object?> parameters)
        {
            var body = new Dictionary<string, object>();
            foreach (var kv in parameters)
            {
                if (kv.Value != null)
                {
                    body[kv.Key] = kv.Value;
                }
            }
            body["app_id"] = _appId;
            body["sign"] = Sign.ComputeSign(body, _appKey);
            return body;
        }

        private async Task<Dictionary<string, JsonElement>> PostAsync(string path, Dictionary<string, object?> parameters)
        {
            var body = BuildRequest(parameters);
            var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var url = $"{_domain}{path}";
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody, JsonOptions)
                ?? new Dictionary<string, JsonElement>();

            if (result.TryGetValue("error", out var error) && error.ValueKind != JsonValueKind.Null)
            {
                throw new MbdPayException(error.GetString() ?? "unknown error");
            }

            return result;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
