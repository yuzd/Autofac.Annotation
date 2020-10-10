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
    /// 后置返回拦截处理器
    /// </summary>
    internal class AspectAfterInterceptor:IAdvice
    {
        private readonly AspectAfter _afterAttribute;

        private readonly (object instance,After after , MethodInfo methodInfo) _pointCutMethod;
        public AspectAfterInterceptor(AspectAfter afterAttribute)
        {
            _afterAttribute = afterAttribute;
        }

        public AspectAfterInterceptor((object instance,After after, MethodInfo methodInfo) pointCutMethod)
        {
            _pointCutMethod = pointCutMethod;
        }
        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            await next.Invoke(aspectContext);
            
           
            //执行异常了不执行after 去执行Throw
            if (aspectContext.Exception != null)
            {
                return;
            }
            
            
            if (_afterAttribute != null)
            {
                await this._afterAttribute.After(aspectContext,aspectContext.Result);
            }
            else
            {
                var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                    _pointCutMethod.instance,
                    _pointCutMethod.methodInfo,
                    aspectContext.ComponentContext,
                    aspectContext,returnValue:aspectContext.InvocationContext.ReturnValue,returnParam:_pointCutMethod.after.Returing);
                if (typeof(Task).IsAssignableFrom(_pointCutMethod.methodInfo.ReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }
            }
        }
    }

}