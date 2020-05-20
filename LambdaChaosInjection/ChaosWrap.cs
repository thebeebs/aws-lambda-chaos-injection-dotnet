using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Text.Json;

namespace LambdaChaosInjection
{
    public class ChaosWrap<TInjection> where TInjection : IInjection
    {
        private readonly TInjection _injectionType;
        private InjectionConfig policy;
        private IInjection injection;

        public  InjectionConfig CurrentPolicy => policy;

        private InjectionConfig CreateInjectionFromString(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };
            
            return System.Text.Json.JsonSerializer.Deserialize<InjectionConfig>(jsonString, options);;
        }

        public ChaosWrap(string jsonString)
        {
            var chaos = jsonString;
            this.policy = this.CreateInjectionFromString(jsonString);
            CreateInjection();
        }
        
        public ChaosWrap()
        {
            var chaos = Environment.GetEnvironmentVariable("CHAOS_PARAM");
            this.policy = chaos != null ? this.CreateInjectionFromString(chaos) : new InjectionConfig();
            CreateInjection();
        }

        private void CreateInjection()
        {
            this.injection = (IInjection)Activator.CreateInstance(typeof(TInjection));;
            this.injection.InjectionConfig = policy;
        }

        //@inject_delay - add delay in the AWS Lambda execution
        //@inject_exception - Raise an exception during the AWS Lambda execution
        //@inject_statuscode
        public async Task<APIGatewayProxyResponse> Execute(Func<Task<APIGatewayProxyResponse>> func)
        {
            if (!policy.IsEnabled)
            {
                return await func.Invoke();
            }

            return await injection.Execute(func);

//            if (_injectionType is InjectDelay && policy.Delay > 0)
//            {
//                Task.Delay(policy.DelayTimeSpan).Wait();
//                return await func.Invoke();
//            }
//            
//            if (_injectionType is InjectException)
//            {
//                var body = new Dictionary<string, string>
//                {
//                    {"message", policy.ExceptionMsg},
//                };
//
//                return new APIGatewayProxyResponse
//                {
//                    Body = JsonConvert.SerializeObject(body),
//                    StatusCode = policy.ErrorCode,
//                    Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
//                };
//            }
//            
//            if (_injectionType is InjectStatusCode)
//            {
//                var body = new Dictionary<string, string>
//                {
//                };
//
//                return new APIGatewayProxyResponse
//                {
//                    Body = JsonConvert.SerializeObject(body),
//                    StatusCode = policy.ErrorCode,
//                    Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
//                };
//            }
//            return await func.Invoke();
        }
    }

    public interface IInjection
    {
        InjectionConfig InjectionConfig
        {
            get;
            set;
        }
        Task<APIGatewayProxyResponse> Execute( Func<Task<APIGatewayProxyResponse>> func);
    }
}