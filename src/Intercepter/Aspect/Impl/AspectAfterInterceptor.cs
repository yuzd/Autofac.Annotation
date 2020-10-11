using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Aspect.Advice;
using Autofac.Aspect.Pointcut;

namespace Autofac.Aspect.Impl
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

        private readonly RunTimePointcutMethod<After> _pointCutMethod;
        public AspectAfterInterceptor(AspectAfter afterAttribute)
        {
            _afterAttribute = afterAttribute;
        }

        public AspectAfterInterceptor(RunTimePointcutMethod<After> pointCutMethod)
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
                    _pointCutMethod.Instance,
                    _pointCutMethod.MethodInfo,
                    aspectContext.ComponentContext,
                    aspectContext,returnValue:aspectContext.InvocationContext.ReturnValue,
                    returnParam:_pointCutMethod.PointcutBasicAttribute.Returing,
                    injectAnotation:_pointCutMethod.PointcutInjectAnotation);
                if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodInfo.ReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }
            }
        }
    }

}