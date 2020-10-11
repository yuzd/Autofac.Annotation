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
                return;
            }
            
            var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                _pointCutMethod.Instance,
                _pointCutMethod.MethodInfo,
                _pointCutMethod.MethodParameters,
                aspectContext.ComponentContext,
                aspectContext,
                next,
                injectAnotation:_pointCutMethod.PointcutInjectAnotation);

            if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodReturnType))
            {
                await ((Task) rt).ConfigureAwait(false);
            }
            
        }
    }

}