using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.AspectIntercepter.Advice;
using Autofac.AspectIntercepter.Pointcut;

namespace Autofac.AspectIntercepter.Impl
{

    /// <summary>
    /// 异常返回拦截处理器
    /// </summary>
    internal class AspectAfterThrowsInterceptor : IAdvice
    {
        private readonly AspectAfterThrows _aspectThrowing;
        private readonly bool _isFromAround;
        private readonly RunTimePointcutMethod<AfterThrows> _pointcutThrowin;

        public AspectAfterThrowsInterceptor(AspectAfterThrows throwAttribute, bool isFromAround = false)
        {
            _aspectThrowing = throwAttribute;
            _isFromAround = isFromAround;
        }

        public AspectAfterThrowsInterceptor(RunTimePointcutMethod<AfterThrows> throwAttribute, bool isFromAround = false)
        {
            _pointcutThrowin = throwAttribute;
            _isFromAround = isFromAround;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            try
            {
                if(!_isFromAround) await next.Invoke(aspectContext);
            }
            finally
            {
                //只有目标方法出现异常才会走 增强的方法出异常不要走
                if (aspectContext.Exception != null)
                {
                    Exception ex = aspectContext.Exception;
                    if (aspectContext.Exception is TargetInvocationException targetInvocationException)
                    {
                        ex = targetInvocationException.InnerException;
                    }

                    if (ex == null)
                    {
                        ex = aspectContext.Exception;
                    }

                    var currentExType = ex.GetType();

                    if (_pointcutThrowin != null)
                    {
                        if (_pointcutThrowin.PointcutBasicAttribute.ExceptionType == null || _pointcutThrowin.PointcutBasicAttribute.ExceptionType == currentExType)
                        {
                            var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                                _pointcutThrowin.Instance,
                                _pointcutThrowin.MethodInfo,
                                _pointcutThrowin.MethodParameters,
                                aspectContext.ComponentContext,
                                aspectContext, 
                                returnValue: ex, 
                                returnParam: _pointcutThrowin.PointcutBasicAttribute.Throwing,
                                injectAnotation:_pointcutThrowin.PointcutInjectAnotation);
                            if (typeof(Task).IsAssignableFrom(_pointcutThrowin.MethodReturnType))
                            {
                                await ((Task) rt).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        if (_aspectThrowing.ExceptionType == null || _aspectThrowing.ExceptionType == currentExType)
                        {
                            await _aspectThrowing.AfterThrows(aspectContext, aspectContext.Exception);
                        }
                    }
                 
                }
            }
        }
    }
}