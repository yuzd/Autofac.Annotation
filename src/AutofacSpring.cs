using System;
using Autofac.Builder;
using Autofac.Core.Registration;

namespace Autofac.Annotation
{
    /// <summary>
    ///  扩展
    /// </summary>
    public static class AutofacSpring
    {
        /// <summary>
        /// 针对某个类型防止注册
        /// </summary>
        internal static string DISABLE_AUTO_INCLUE_INTO_COMPOMENT = "disable_autofac_spring";

        /// <summary>
        /// 注册扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterSpring(this ContainerBuilder builder, AutofacAnnotationModule module)
        {
            return builder.RegisterModule(module);
        }

        /// <summary>
        /// 注册扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="springConfig"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterSpring(this ContainerBuilder builder, Action<AutofacAnnotationModule> springConfig)
        {
            AutofacAnnotationModule autofacAnnotationModule = new AutofacAnnotationModule();
            springConfig.Invoke(autofacAnnotationModule);
            return builder.RegisterModule(autofacAnnotationModule);
        }

        /// <summary>
        ///  调用autofac的api自注册的情况下不希望被扫描Autowired和Value的场合使用
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExclueFromSpring<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.WithMetadata(DISABLE_AUTO_INCLUE_INTO_COMPOMENT, true);
        }
    }
}