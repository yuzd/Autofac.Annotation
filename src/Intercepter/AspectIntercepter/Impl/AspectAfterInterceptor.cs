using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.AspectIntercepter.Advice;
using Autofac.AspectIntercepter.Pointcut;

namespace Autofac.AspectIntercepter.Impl
{
    /// <summary>
    /// 后置拦截处理器
    /// </summary>
    internal class AspectAfterInterceptor : IAdvice
    {
        private readonly AspectAfter _afterAttribute;
        private readonly bool _isAfterAround;

        private readonly RunTimePointcutMethod<After> _pointCutMethod;

        public AspectAfterInterceptor(AspectAfter afterAttribute, bool isAfterAround = false)
        {
            _afterAttribute = afterAttribute;
            _isAfterAround = isAfterAround;
        }

        public AspectAfterInterceptor(RunTimePointcutMethod<After> pointCutMethod, bool isAfterAround = false)
        {
            _pointCutMethod = pointCutMethod;
            _isAfterAround = isAfterAround;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            try
            {
                if (!_isAfterAround) await next.Invoke(aspectContext);
            }
            finally
            {
                //不管成功还是失败都会执行的 
                if (_afterAttribute != null)
                {
                    await this._afterAttribute.After(aspectContext, aspectContext.Exception ?? aspectContext.ReturnValue);
                }
                else
                {
                    var rt = MethodInvokeHelper.InvokeInstanceMethod(
                        _pointCutMethod.Instance,
                        _pointCutMethod.MethodInfo,
                        _pointCutMethod.MethodParameters,
                        aspectContext.ComponentContext,
                        aspectContext,
                        returnValue: aspectContext.Exception ?? aspectContext.ReturnValue,
                        returnParam: _pointCutMethod.PointcutBasicAttribute.Returing,
                        injectAnotation: _pointCutMethod.PointcutInjectAnotation);

                    if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodReturnType))
                    {
                        await ((Task)rt).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}