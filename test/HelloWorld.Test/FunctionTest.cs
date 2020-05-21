using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using HelloWorld;

namespace LambdaChaosInjection.Tests
{
  public class FunctionTest
  {
    private static readonly HttpClient client = new HttpClient();

    private static async Task<string> GetCallingIP()
    {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var stringTask = client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

            var msg = await stringTask;
            return msg.Replace("\n","");
    }

    [Fact]
    public async Task TestThatConfigLoadsFromString()
    {
        // Arrange
        var testJsonString =
            "{ \"delay\": 400, \"isEnabled\": true, \"error_code\": 404, \"exception_msg\": \"I really failed seriously\", \"rate\": 1 }";
        var policy = new LambdaChaosInjection.ChaosWrap<InjectDelay>(testJsonString);

        // Act
        var p = policy.CurrentPolicy;

        // Assert
        Assert.True(p.IsEnabled);
        Assert.Equal(404, p.ErrorCode);
        Assert.Equal("I really failed seriously", p.ExceptionMsg);
        Assert.Equal(1, p.Rate);
    }
    
    [Fact]
    public async Task TestThatConfigLoadsFromStringWithFalse()
    {
        // Arrange
        var testJsonString =
            "{ \"delay\": 400, \"isEnabled\": false, \"error_code\": 404, \"exception_msg\": \"I really failed seriously\", \"rate\": 1 }";
        var policy = new LambdaChaosInjection.ChaosWrap<InjectDelay>(testJsonString);

        // Act
        var p = policy.CurrentPolicy;

        // Assert
        Assert.False(p.IsEnabled);
    }

    [Fact]
    public async Task TestHelloWorldFunctionHandler()
    {
            var request = new APIGatewayProxyRequest();
            var context = new TestLambdaContext();
            string location = GetCallingIP().Result;
            Dictionary<string, string> body = new Dictionary<string, string>
            {
                { "message", "hello world" },
                { "location", location },
            };

            var expectedResponse = new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(body),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            var function = new Function();
            var response = await function.FunctionHandler(request, context);

            Console.WriteLine("Lambda Response: \n" + response.Body);
            Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

            Assert.Equal(expectedResponse.Body, response.Body);
            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }
    
    [Fact]
    public async Task TestHelloWorldFunctionHandlerException()
    {
        var testJsonString =
            "{ \"delay\": 400, \"isEnabled\": true, \"error_code\": 404, \"exception_msg\": \"I really failed seriously\", \"rate\": 1 }";

        Environment.SetEnvironmentVariable("CHAOS_PARAM",testJsonString);
        var request = new APIGatewayProxyRequest();
        var context = new TestLambdaContext();
        string location = GetCallingIP().Result;
        Dictionary<string, string> body = new Dictionary<string, string>
        {
            { "message", "hello world" },
            { "location", location },
        };

        var expectedResponse = new APIGatewayProxyResponse
        {
            Body = JsonConvert.SerializeObject(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        var function = new Function();
        var response = await function.FunctionHandlerException(request, context);

        Console.WriteLine("Lambda Response: \n" + response.Body);
        Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

        Assert.Equal(404, response.StatusCode);
    }
    
    [Fact]
    public async Task TestHelloWorldFunctionHandlerExceptionEnabledFalse()
    {var testJsonString =
            "{ \"delay\": 400, \"isEnabled\": false, \"error_code\": 404, \"exception_msg\": \"I really failed seriously\", \"rate\": 1 }";

        Environment.SetEnvironmentVariable("CHAOS_PARAM",testJsonString);
        var request = new APIGatewayProxyRequest();
        var context = new TestLambdaContext();
        string location = GetCallingIP().Result;
        Dictionary<string, string> body = new Dictionary<string, string>
        {
            { "message", "hello world" },
            { "location", location },
        };

        var expectedResponse = new APIGatewayProxyResponse
        {
            Body = JsonConvert.SerializeObject(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        var function = new Function();
        var response = await function.FunctionHandlerException(request, context);

        Console.WriteLine("Lambda Response: \n" + response.Body);
        Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task IsMyFailureRateTestWorking()
    {
        // Arrange
        var c = new ChaosWrap<InjectDelay>();
        
        // Act
        
        double countOfSuccess= 0;
        double countOfSuccess2= 0;
        double countOfSuccess9= 0;

        c.CurrentPolicy.Rate = 0.1;
        for (int i = 0; i < 100000; i++)
        {
            if (c.CurrentPolicy.RateOfFailureTestMet())
            {
                
                countOfSuccess++;
            }
        }
        
        c.CurrentPolicy.Rate = 0.2;
        for (int i = 0; i < 100000; i++)
        {
            if (c.CurrentPolicy.RateOfFailureTestMet())
            {
                countOfSuccess2++;
            }
        }
        
        c.CurrentPolicy.Rate = 0.9;
        for (int i = 0; i < 100000; i++)
        {
            if (c.CurrentPolicy.RateOfFailureTestMet())
            {
                countOfSuccess9++;
            }
        }
        
        // Assert
        Assert.InRange(countOfSuccess/100000,0.09,0.11);
        Assert.InRange(countOfSuccess2/100000,0.19,0.21);
        Assert.InRange(countOfSuccess9/100000, 0.89,0.91);
        
    }
  }
}