using System;
using System.Text.Json.Serialization;

namespace LambdaChaosInjection
{
    public class InjectionConfig
    {
        public InjectionConfig()
        {
        }

        public InjectionConfig(bool isEnabled = false, int delay = 0, int errorCode = 404, string exceptionMsg = "Error", int rate = 1)
        {
            IsEnabled = isEnabled;
            Delay = delay;
            ErrorCode = errorCode;
            ExceptionMsg = exceptionMsg;
            Rate = rate;
        }
        /// <summary>
        /// This method is used to determine if a chaos injection should happen.
        /// If the failure rate is set to 0.1 then this function should return true
        /// 1 in ten times.
        /// </summary>
        public bool RateOfFailureTestMet()
        {
            var random = new Random();
            if (random.NextDouble() <= this.Rate)
            {
                return true;
            }

            return false;
        }

        internal TimeSpan DelayTimeSpan => new TimeSpan(0, 0, this.Delay);
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
        [JsonPropertyName("delay")]
        public int Delay { get; set; }
        
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }
        
        [JsonPropertyName("exception_msg")]
        public string ExceptionMsg { get; set; }
        [JsonPropertyName("rate")]
        public double Rate { get; set; }
    }
}