using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using LambdaChaosInjection;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HelloWorld
{

    public class Function
    {

        private static readonly HttpClient client = new HttpClient();

        private static async Task<string> GetCallingIP()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await client.GetStringAsync("http://checkip.amazonaws.com/")
                .ConfigureAwait(continueOnCapturedContext: false);

            return msg.Replace("\n", "");
        }

        [InjectDelayPolicy]
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            var location = await GetCallingIP();
            var body = new Dictionary<string, string>
            {
                {"message", "hello world"},
                {"location", location}
            };

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(body),
                StatusCode = 200,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }

        public async Task<APIGatewayProxyResponse> FunctionHandlerException(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            return await new ChaosWrap<InjectException>().Execute(async () =>
                {
                    var location = await GetCallingIP();
                    var body = new Dictionary<string, string>
                    {
                        {"message", "hello world"},
                        {"location", location}
                    };

                    return new APIGatewayProxyResponse
                    {
                        Body = JsonConvert.SerializeObject(body),
                        StatusCode = 200,
                        Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
                    };
                }
            );
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandlerStatusCode(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            return await new ChaosWrap<InjectDelay>().Execute(async () =>
                {
                    var location = await GetCallingIP();
                    var body = new Dictionary<string, string>
                    {
                        {"message", "hello world"},
                        {"location", location}
                    };

                    return new APIGatewayProxyResponse
                    {
                        Body = JsonConvert.SerializeObject(body),
                        StatusCode = 200,
                        Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
                    };
                }
            );
        }
    }

    
}