using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

namespace LambdaChaosInjection
{
    public class InjectDelay : IInjection
    {
        public InjectionConfig InjectionConfig { get; set; }

        public Task<T> Execute<T>(Func<Task<T>> func)
        {
            Console.WriteLine($"Adding delay of {InjectionConfig.DelayTimeSpan.TotalMilliseconds} milliseconds");
            Task.Delay(InjectionConfig.DelayTimeSpan).Wait();
            Console.WriteLine($"Delay complete, now invoking function");
            return func.Invoke();
        }
    }
}