using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Aspect.Advice;
using Autofac.Aspect.Pointcut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Aspect.Impl
{


    /// <summary>
    /// 后置返回拦截处理器
    /// </summary>
    internal class AspectAroundInterceptor:IAdvice
    {
        private readonly AspectArround _aroundAttribute;

        private readonly RunTimePointcutMethod<Around> _pointCutMethod;
        public AspectAroundInterceptor(AspectArround aroundAttribute)
        {
            _aroundAttribute = aroundAttribute;
        }

        public AspectAroundInterceptor(RunTimePointcutMethod<Around> pointCutMethod)
        {
            _pointCutMethod = pointCutMethod;
        }
        
        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            if (_aroundAttribute != null)
            {
                await _aroundAttribute.OnInvocation(aspectContext, next);
                //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
                if (aspectContext.InvocationContext.ReturnValue != null)
                {
                    aspectContext.Result = aspectContext.InvocationContext.ReturnValue;
                }
                return;
            }
            
            var rt = AutoConfigurationHelper.InvokeInstanceMethod(_pointCutMethod.Instance, _pointCutMethod.MethodInfo,
                aspectContext.ComponentContext,
                aspectContext, next,injectAnotation:_pointCutMethod.PointcutInjectAnotation);
            if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodInfo.ReturnType))
            {
                await ((Task) rt).ConfigureAwait(false);
            }
            //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
            if (aspectContext.InvocationContext.ReturnValue != null)
            {
                aspectContext.Result = aspectContext.InvocationContext.ReturnValue;
            }
        }
    }

}