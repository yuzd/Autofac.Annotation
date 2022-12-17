using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Advice;
using Autofac.AspectIntercepter.Pointcut;

namespace Autofac.AspectIntercepter.Impl
{
    /// <summary>
    /// 后置返回拦截处理器
    /// </summary>
    internal class AspectAfterReturnInterceptor : IAdvice
    {
        private readonly AspectAfterReturn _afterReturnAttribute;
        private readonly string _afterReturnMethodName;

        private readonly RunTimePointcutMethod<AfterReturn> _pointCutMethod;

        public AspectAfterReturnInterceptor(AspectAfterReturn afterAttribute)
        {
            _afterReturnAttribute = afterAttribute;
            _afterReturnMethodName = afterAttribute.GetType().FullName + ".AfterReturn()";
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


            if (_afterReturnAttribute != null)
            {
                using (DeadLockCheck.Enable(_afterReturnMethodName))
                {
                    await _afterReturnAttribute.AfterReturn(aspectContext, aspectContext.ReturnValue);
                }
            }
            else
            {
                using (DeadLockCheck.Enable(_pointCutMethod.MethodInfo.GetMethodInfoUniqueName()))
                {
                    var rt = MethodInvokeHelper.InvokeInstanceMethod(
                        _pointCutMethod.Instance,
                        _pointCutMethod.MethodInfo,
                        _pointCutMethod.MethodParameters,
                        aspectContext.ComponentContext,
                        aspectContext,
                        returnValue: aspectContext.ReturnValue,
                        returnParam: _pointCutMethod.PointcutBasicAttribute.Returing,
                        injectAnotation: _pointCutMethod.PointcutInjectAnotation,
                        pointCutAnnotation: _pointCutMethod.Pointcut);

                    if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodReturnType))
                    {
                        await ((Task)rt).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}