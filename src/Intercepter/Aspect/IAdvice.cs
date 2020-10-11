using System.Threading.Tasks;

namespace Autofac.Aspect
{
    internal interface IAdvice
    {
        Task OnInvocation(AspectContext aspectContext, AspectDelegate next);
    }
}