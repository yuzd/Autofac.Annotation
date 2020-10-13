using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter
{
    internal interface IAdvice
    {
        Task OnInvocation(AspectContext aspectContext, AspectDelegate next);
    }
}