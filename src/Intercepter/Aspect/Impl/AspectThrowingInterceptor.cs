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
    /// 异常返回拦截处理器
    /// </summary>
    internal class AspectThrowingInterceptor : IAdvice
    {
        private readonly AspectThrows _aspectThrowing;
        private readonly RunTimePointcutMethod<Throws> _pointcutThrowin;
        private readonly bool _isLast;

        public AspectThrowingInterceptor(AspectThrows throwAttribute,bool isLast)
        {
            _aspectThrowing = throwAttribute;
            _isLast = isLast;
        }

        public AspectThrowingInterceptor(RunTimePointcutMethod<Throws> throwAttribute,bool isLast)
        {
            _pointcutThrowin = throwAttribute;
            _isLast = isLast;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            try
            {
                await next.Invoke(aspectContext);
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
                                aspectContext.ComponentContext,
                                aspectContext, 
                                returnValue: ex, 
                                returnParam: _pointcutThrowin.PointcutBasicAttribute.Throwing,
                                injectAnotation:_pointcutThrowin.PointcutInjectAnotation);
                            if (typeof(Task).IsAssignableFrom(_pointcutThrowin.MethodInfo.ReturnType))
                            {
                                await ((Task) rt).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        if (_aspectThrowing.ExceptionType == null || _aspectThrowing.ExceptionType == currentExType)
                        {
                            await _aspectThrowing.Throwing(aspectContext, aspectContext.Exception);
                        }
                    }


                    if (_isLast)
                    {
                        throw ex;
                    }
                 
                }
            }
        }
    }
}