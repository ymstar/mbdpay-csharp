using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MbdPay
{
    /// <summary>
    /// 面包多Pay签名算法实现
    /// 文档: https://doc.mbd.pub/api/qian-ming-suan-fa
    /// </summary>
    public static class Sign
    {
        /// <summary>
        /// 对参数进行签名
        /// </summary>
        /// <param name="attributes">请求参数（不含sign）</param>
        /// <param name="appKey">面包多Pay app_key</param>
        /// <returns>MD5签名字符串</returns>
        public static string ComputeSign(Dictionary<string, object> attributes, string appKey)
        {
            var sortedKeys = attributes.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

            var sb = new StringBuilder();
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append('&');
                }
                sb.Append(sortedKeys[i]);
                sb.Append('=');
                sb.Append(attributes[sortedKeys[i]]);
            }
            sb.Append("&key=").Append(appKey);

            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                hex.Append(b.ToString("x2"));
            }
            return hex.ToString();
        }
    }
}
