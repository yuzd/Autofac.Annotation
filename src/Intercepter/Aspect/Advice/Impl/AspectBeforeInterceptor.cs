using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.Aspect.Advice.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 前置拦截处理器
    /// </summary>
    internal class AspectBeforeInterceptor:IAdvice
    {
        private readonly AspectBefore _beforeAttribute;
        private readonly (object instance, MethodInfo methodInfo) _pointCutMethod;
        public AspectBeforeInterceptor(AspectBefore beforeAttribute)
        {
            _beforeAttribute = beforeAttribute;
        }
        
        public AspectBeforeInterceptor((object instance, MethodInfo methodInfo) pointCutMethod)
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
                var rt = AutoConfigurationHelper.InvokeInstanceMethod(_pointCutMethod.instance, _pointCutMethod.methodInfo, aspectContext.ComponentContext, aspectContext);
                if (typeof(Task).IsAssignableFrom(_pointCutMethod.methodInfo.ReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }
            }
         
            await next.Invoke(aspectContext);
        }
    }
}