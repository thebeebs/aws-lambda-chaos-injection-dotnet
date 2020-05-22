using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

namespace LambdaChaosInjection
{
    public interface IInjection
    {
        InjectionConfig InjectionConfig
        {
            get;
            set;
        }
        Task<T> Execute<T>( Func<Task<T>> func);
    }
}