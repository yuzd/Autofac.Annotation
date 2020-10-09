using System.Reflection;
using System.Threading.Tasks;

namespace Autofac.Aspect.Advice.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 异常返回拦截处理器
    /// </summary>
    internal class AspectThrowingInterceptor:IAdvice
    {
        private readonly List<AspectThrowingAttribute> _aspectThrowingList;

        public AspectThrowingInterceptor(List<AspectThrowingAttribute> throwAttribute)
        {
            _aspectThrowingList = throwAttribute;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            try
            {
                //真正的方法
                await next.Invoke(aspectContext);
            }
            catch (Exception e)
            {
                aspectContext.Exception = e;
            }
           

            //只有目标方法出现异常才会走 增强的方法出异常不要走
            if (aspectContext.Exception == null)
            {
                return;
            }

            Exception ex = aspectContext.Exception ;
            if (aspectContext.Exception is TargetInvocationException targetInvocationException)
            {
                ex = targetInvocationException.InnerException;
            }

            if (ex == null)
            {
                throw aspectContext.Exception;
            }

            var currentExType = ex.GetType();
            foreach (var _throwAttribute in _aspectThrowingList)
            {
                if (_throwAttribute.ExceptionType == currentExType)
                {
                    await _throwAttribute.Throwing(aspectContext,aspectContext.Exception);
                    if (aspectContext.InvocationContext.ReturnValue != null)
                    {
                        //这里提供给异常处理器一个机会是否吃掉异常 返回一个异常处理器包装后的结果
                        aspectContext.Result = aspectContext.InvocationContext.ReturnValue;
                        return;
                    }
                }
            }
            
            throw ex;
        }
    }


}