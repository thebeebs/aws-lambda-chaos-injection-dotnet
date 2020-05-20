using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace LambdaChaosInjection
{
    public class InjectException : IInjection
    {
        public InjectionConfig InjectionConfig { get; set; }

        public async Task<APIGatewayProxyResponse> Execute(Func<Task<APIGatewayProxyResponse>> func)
        {
            var body = new Dictionary<string, string>
                {
                    {"message", InjectionConfig.ExceptionMsg},
                };

                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(body),
                    StatusCode = InjectionConfig.ErrorCode,
                    Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
                };
        }
    }
}