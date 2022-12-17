using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Advice;
using Autofac.AspectIntercepter.Pointcut;

namespace Autofac.AspectIntercepter.Impl
{
    /// <summary>
    /// 前置拦截处理器
    /// </summary>
    internal class AspectBeforeInterceptor : IAdvice
    {
        private readonly AspectBefore _beforeAttribute;
        private readonly string _beforeAttributeMethodName;
        private readonly RunTimePointcutMethod<Before> _pointCutMethod;

        public AspectBeforeInterceptor(AspectBefore beforeAttribute)
        {
            _beforeAttribute = beforeAttribute;
            _beforeAttributeMethodName = beforeAttribute.GetType().FullName + ".Before()";
        }

        public AspectBeforeInterceptor(RunTimePointcutMethod<Before> pointCutMethod)
        {
            _pointCutMethod = pointCutMethod;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            if (_beforeAttribute != null)
            {
                using (DeadLockCheck.Enable(_beforeAttributeMethodName))
                {
                    await _beforeAttribute.Before(aspectContext);
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
                        aspectContext.ComponentContext, aspectContext,
                        injectAnotation: _pointCutMethod.PointcutInjectAnotation,
                        pointCutAnnotation: _pointCutMethod.Pointcut);
                    if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodReturnType))
                    {
                        await ((Task)rt).ConfigureAwait(false);
                    }
                }
            }

            await next.Invoke(aspectContext);
        }
    }
}