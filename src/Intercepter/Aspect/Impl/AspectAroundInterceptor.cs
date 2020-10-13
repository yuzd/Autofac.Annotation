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
    /// 环绕返回拦截处理器
    /// </summary>
    internal class AspectAroundInterceptor:IAdvice
    {
        private readonly AspectArround _aroundAttribute;
        private readonly AspectAfterInterceptor _aspectAfter;
        private readonly AspectAfterThrowsInterceptor _aspectThrows;

        private readonly RunTimePointcutMethod<Around> _pointCutMethod;

        public AspectAroundInterceptor(AspectArround aroundAttribute, AspectAfter aspectAfter, AspectAfterThrows chainAspectAfterThrows)
        {
            _aroundAttribute = aroundAttribute;
            if (aspectAfter != null)
            {
                _aspectAfter = new AspectAfterInterceptor(aspectAfter,true);
            }

            if (chainAspectAfterThrows != null)
            {
                _aspectThrows= new AspectAfterThrowsInterceptor(chainAspectAfterThrows,true);
            }
        }

        public AspectAroundInterceptor(RunTimePointcutMethod<Around> pointCutMethod,AspectAfterInterceptor aspectAfter, AspectAfterThrowsInterceptor chainAspectAfterThrows)
        {
            _pointCutMethod = pointCutMethod;
            _aspectAfter = aspectAfter;
            _aspectThrows= chainAspectAfterThrows;
        }
        
        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            Exception exception = null;
            try
            {
                if (_aroundAttribute != null)
                {
                    await _aroundAttribute.OnInvocation(aspectContext, next);
                    return;
                }

                var rt = AutoConfigurationHelper.InvokeInstanceMethod(
                    _pointCutMethod.Instance,
                    _pointCutMethod.MethodInfo,
                    _pointCutMethod.MethodParameters,
                    aspectContext.ComponentContext,
                    aspectContext,
                    next,
                    injectAnotation: _pointCutMethod.PointcutInjectAnotation);

                if (typeof(Task).IsAssignableFrom(_pointCutMethod.MethodReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if(exception == null && _aspectAfter!=null)await _aspectAfter.OnInvocation(aspectContext, next);
            }
            
            try
            {
                if (exception != null && _aspectAfter!=null)
                {
                    await _aspectAfter.OnInvocation(aspectContext, next);
                }
                
                if (exception != null && _aspectThrows != null)
                {
                    await _aspectThrows.OnInvocation(aspectContext, next);
                }
            }
            finally
            {
                if (exception != null) throw exception;
            }
            
        }
    }

}