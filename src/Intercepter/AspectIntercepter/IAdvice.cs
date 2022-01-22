using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter
{
    internal interface IAdvice
    {
        /// <summary>
        ///  拦截器方法
        /// </summary>
        /// <param name="aspectContext">执行上下文</param>
        /// <param name="next">下一个增强器</param>
        /// <returns></returns>
        Task OnInvocation(AspectContext aspectContext, AspectDelegate next);
    }
}