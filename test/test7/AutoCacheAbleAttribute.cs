using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.AspectIntercepter.Advice;

namespace Autofac.Configuration.Test.test7
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class AutoCacheAbleAttribute : AspectArround
    {

        private static readonly ConcurrentDictionary<string, object> _cache  = new ConcurrentDictionary<string, object>();

        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            //如果缓存存在，则直接返回
            if (_cache.TryGetValue(aspectContext.TargetMethod.Name, out var _cacheResult))
            {
                Console.WriteLine("Use cache");
                aspectContext.ReturnValue = _cacheResult+"_Cache"; 
                return;
            }

            //执行原方法
            await _next(aspectContext);

            //缓存方法结果 
            _cache.TryAdd(aspectContext.TargetMethod.Name, aspectContext.ReturnValue);

        }
    }



    [Component(EnableAspect = true)]
    public class TestCacheAop
    {
        public string Name { get; set; } = "TestCacheAop";

        [AutoCacheAble]
        public virtual async Task<string> TestInterceptor1()
        {
            await Task.Delay(1000);
            Console.WriteLine("TestCacheAop");
            return Name;
        }
    }


    public interface ICacheAop2<T>
    {
        Task<string> TestInterceptor2();
    }
    
   
    // [Component]
    [Component(typeof(ICacheAop2<>),EnableAspect = true,InterceptorType= InterceptorType.Interface)]
    public class TestCacheAop2<T>:ICacheAop2<T>
    {
        public string Name { get; set; } = "TestCacheAop";

        [AutoCacheAble]
        public async Task<string> TestInterceptor2()
        {
            await Task.Delay(1000);
            Console.WriteLine("TestCacheAop");
            return Name;
        }
    }
    
    
    public interface ICacheAop23<T>
    {
        Task<string> TestInterceptor2();
    }

    
    [Component(typeof(ICacheAop23<>))]
    public class TestCacheAop3<T>:ICacheAop23<T>
    {
        public string Name { get; set; } = "TestCacheAop";

        public async Task<string> TestInterceptor2()
        {
            await Task.Delay(1000);
            Console.WriteLine("TestCacheAop");
            return Name;
        }
    }
}
