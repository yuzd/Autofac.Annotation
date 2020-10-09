using System.Threading.Tasks;

namespace Autofac.Aspect.Advice.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 前置拦截处理器
    /// </summary>
    internal class AspectBeforeInterceptor:IAdvice
    {
        private readonly AspectBeforeAttribute _beforeAttribute;

        public AspectBeforeInterceptor(AspectBeforeAttribute beforeAttribute)
        {
            _beforeAttribute = beforeAttribute;
        }

        public async Task OnInvocation(AspectContext aspectContext, AspectDelegate next)
        {
            await this._beforeAttribute.Before(aspectContext);
            await next.Invoke(aspectContext);
        }
    }
}