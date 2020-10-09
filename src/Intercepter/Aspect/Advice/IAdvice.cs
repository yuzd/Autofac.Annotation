using System.Threading.Tasks;

namespace Autofac.Aspect.Advice
{
    internal interface IAdvice
    {
        Task OnInvocation(AspectContext aspectContext, AspectDelegate next);
    }
}