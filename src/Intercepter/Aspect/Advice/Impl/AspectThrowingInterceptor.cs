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
    /// 异常返回拦截处理器
    /// </summary>
    internal class AspectThrowingInterceptor : IAdvice
    {
        private readonly AspectThrows _aspectThrowing;
        private readonly Tuple<object, Throws, MethodInfo> _pointcutThrowin;
        private readonly bool _isLast;

        public AspectThrowingInterceptor(AspectThrows throwAttribute,bool isLast)
        {
            _aspectThrowing = throwAttribute;
            _isLast = isLast;
        }

        public AspectThrowingInterceptor(Tuple<object, Throws, MethodInfo> throwAttribute,bool isLast)
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
                        if (_pointcutThrowin.Item2.ExceptionType == null || _pointcutThrowin.Item2.ExceptionType == currentExType)
                        {
                            var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                                _pointcutThrowin.Item1,
                                _pointcutThrowin.Item3,
                                aspectContext.ComponentContext,
                                aspectContext, returnValue: ex, returnParam: _pointcutThrowin.Item2.Throwing);
                            if (typeof(Task).IsAssignableFrom(_pointcutThrowin.Item3.ReturnType))
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