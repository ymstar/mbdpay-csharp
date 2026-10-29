using System.Collections.Generic;
using MbdPay;
using Xunit;

namespace MbdPay.Tests;

public class SignTests
{
    [Fact]
    public void TestSignProducesValidMd5()
    {
        var data = new Dictionary<string, object>
        {
            ["app_id"] = "194024590982810",
            ["amount_total"] = 100,
            ["channel"] = "h5",
            ["description"] = "312"
        };
        var key = "920ee68b4e16df01d0cd6b2ca161195d";

        var result = Sign.ComputeSign(data, key);
        Assert.Equal(32, result.Length);
        Assert.Matches("^[0-9a-f]{32}$", result);
    }

    [Fact]
    public void TestSignOrderIndependent()
    {
        var key = "test_key";

        var data1 = new Dictionary<string, object>
        {
            ["a"] = "1",
            ["b"] = "2",
            ["c"] = "3"
        };
        var data2 = new Dictionary<string, object>
        {
            ["c"] = "3",
            ["b"] = "2",
            ["a"] = "1"
        };

        Assert.Equal(Sign.ComputeSign(data1, key), Sign.ComputeSign(data2, key));
    }
}

public class ClientTests
{
    [Fact]
    public void TestGetOpenidRedirectUrl()
    {
        var client = new Client("test_app_id", "test_app_key");
        var url = client.GetOpenidRedirectUrl("https://example.com/cb");
        Assert.StartsWith("https://mbd.pub/openid?", url);
        Assert.Contains("app_id=test_app_id", url);
    }
}
