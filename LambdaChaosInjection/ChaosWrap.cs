using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;
using AspectInjector;

namespace LambdaChaosInjection
{
    public class ChaosWrap<TInjection> where TInjection : IInjection
    {
        private InjectionConfig _policy;
        private IInjection _injection;

        public  InjectionConfig CurrentPolicy => _policy;

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
            this._policy = this.CreateInjectionFromString(jsonString);
            CreateInjection();
        }
        
        public ChaosWrap()
        {
            var chaos = Environment.GetEnvironmentVariable("CHAOS_PARAM");
            this._policy = chaos != null ? this.CreateInjectionFromString(chaos) : new InjectionConfig();
            CreateInjection();
        }

        private void CreateInjection()
        {
            this._injection = (IInjection)Activator.CreateInstance(typeof(TInjection));;
            this._injection.InjectionConfig = _policy;
        }
        
        public async Task<T> Execute<T>(Func<Task<T>> func)
        { 
            
            if (_policy.IsEnabled)
            {
                Console.WriteLine($"Chaos policy is enabled and of type {typeof(TInjection)}");
                if (_policy.RateOfFailureTestMet())
                {
                    Console.WriteLine($"Executed chaos policy with a failure rate of {_policy.Rate} meaning this policy will be executed {_policy.Rate * 100}% of the time");
                    return await _injection.Execute(func);
                }

                Console.WriteLine(
                    $"Chaos policy was not executed because the failure rate of {_policy.Rate} was not reached");
                Console.WriteLine($"With this failure rate they policy will be invoked {_policy.Rate * 100}% of the time");
                
            }
            else
            {
                Console.WriteLine($"Chaos policy of type {typeof(TInjection)} is attached but not invoked");
            }

            return await func.Invoke();
        }
    }
}