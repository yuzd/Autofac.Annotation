using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Builder;
using Autofac.Events;

// ReSharper disable once CheckNamespace
namespace Autofac
{
    /// <summary>
    /// 扩展类
    /// </summary>
    internal static class BuilderExtensions
    {
        /// <summary>
        /// 注册消息器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<EventPublisher, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterEventing(this ContainerBuilder builder)
        {
            return builder.RegisterType<EventPublisher>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}