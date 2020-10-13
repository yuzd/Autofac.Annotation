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
    internal class AspectAfterReturnInterceptor:IAdvice
    {
        private readonly AspectAfterReturn _afterAttribute;

        private readonly RunTimePointcutMethod<AfterReturn> _pointCutMethod;
        public AspectAfterReturnInterceptor(AspectAfterReturn afterAttribute)
        {
            _afterAttribute = afterAttribute;
        }

        public AspectAfterReturnInterceptor(RunTimePointcutMethod<AfterReturn> pointCutMethod)
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
                await this._afterAttribute.AfterReturn(aspectContext,aspectContext.ReturnValue);
            }
            else
            {
                var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                    _pointCutMethod.Instance,
                    _pointCutMethod.MethodInfo,
                    _pointCutMethod.MethodParameters,
                    aspectContext.ComponentContext,
                    aspectContext,
                    returnValue:aspectContext.ReturnValue,
                    returnParam:_pointCutMethod.PointcutBasicAttribute.Returing,
                    injectAnotation:_pointCutMethod.PointcutInjectAnotation);

                if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }
            }
        }
    }

}