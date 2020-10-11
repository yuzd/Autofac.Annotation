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
    /// 前置拦截处理器
    /// </summary>
    internal class AspectBeforeInterceptor:IAdvice
    {
        private readonly AspectBefore _beforeAttribute;
        private readonly RunTimePointcutMethod<Before> _pointCutMethod;
        public AspectBeforeInterceptor(AspectBefore beforeAttribute)
        {
            _beforeAttribute = beforeAttribute;
        }
        
        public AspectBeforeInterceptor(RunTimePointcutMethod<Before> pointCutMethod)
        {
            _pointCutMethod = pointCutMethod;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            if (_beforeAttribute != null)
            {
                await this._beforeAttribute.Before(aspectContext);
            }
            else
            {
                var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                    _pointCutMethod.Instance,
                    _pointCutMethod.MethodInfo, 
                    aspectContext.ComponentContext, aspectContext,injectAnotation:_pointCutMethod.PointcutInjectAnotation);
                if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodInfo.ReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }
            }
         
            await next.Invoke(aspectContext);
        }
    }
}