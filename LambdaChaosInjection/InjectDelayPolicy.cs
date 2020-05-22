using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectInjector.Broker;

namespace LambdaChaosInjection
{
    [Aspect(Scope.Global)]
    [Injection(typeof(InjectDelayPolicy))]
    public class InjectDelayPolicy : Attribute
    {
        private static readonly MethodInfo _asyncHandler = typeof(InjectDelayPolicy).GetMethod(nameof(InjectDelayPolicy.WrapAsync), BindingFlags.NonPublic | BindingFlags.Static);

        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Name)] string name,
            [Argument(Source.ReturnType)] Type retType
        )
        {
            var syncResultType = retType.GenericTypeArguments[0];
            var tgt = target;
            return _asyncHandler.MakeGenericMethod(syncResultType).Invoke(this, new object[] { tgt, args, name });

        }
        private static async Task<T> WrapAsync<T>(Func<object[], object> target, object[] args, string name)
        {
            try
            {
                return await new ChaosWrap<InjectDelay>().Execute<T>(() => (Task<T>) target(args));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Async method `{name}` throws {e.GetType()} exception.");
                return default;
            }
        }
    }
}