using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
namespace LambdaChaosInjection
{
    public class InjectStatusCode: IInjection
    { 
        public InjectionConfig InjectionConfig { get; set; }

        public Task<APIGatewayProxyResponse> Execute(Func<Task<APIGatewayProxyResponse>> func)
        {
            Task.Delay(InjectionConfig.DelayTimeSpan).Wait();
            return func.Invoke();
        }
    }
}