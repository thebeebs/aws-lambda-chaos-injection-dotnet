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
        
        public async Task<T> Execute<T>(Func<Task<T>> func)
        {
            var body = new Dictionary<string, string>
                {
                    {"message", InjectionConfig.ExceptionMsg},
                };

            dynamic execute = new APIGatewayProxyResponse();
            execute.Body = JsonConvert.SerializeObject(body);
            execute.StatusCode = InjectionConfig.ErrorCode;
            execute.Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}};
            return execute;
        }
    }
}