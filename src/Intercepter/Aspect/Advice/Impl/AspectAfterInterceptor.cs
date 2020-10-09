using System.Threading.Tasks;

namespace Autofac.Aspect.Advice.Impl
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
        private readonly AspectAfterAttribute _afterAttribute;

        public AspectAfterInterceptor(AspectAfterAttribute afterAttribute)
        {
            _afterAttribute = afterAttribute;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            await next.Invoke(aspectContext);
            await this._afterAttribute.After(aspectContext,aspectContext.Result);
        }
    }

}