using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation.Anotation;
using Autofac.Annotation.Util;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Castle.DynamicProxy;

namespace Autofac.Annotation
{
    /// <summary>
    /// 自启动注册Configuration
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance,AutoActivate = true,InitMethod = "Start")]
    public class AutoConfiguration
    {
        /// <summary>
        /// 执行自动注册
        /// </summary>
        /// <param name="context"></param>
        public void Start(IComponentContext context)
        {
            var autoConfigurationList = context.Resolve<AutoConfigurationList>();
            if (autoConfigurationList == null || autoConfigurationList.AutoConfigurationDetailList == null || !autoConfigurationList.AutoConfigurationDetailList.Any()) return;
            foreach (var autoConfigurationDetail in autoConfigurationList.AutoConfigurationDetailList)
            {
                context.TryResolve(autoConfigurationDetail.AutoConfigurationClassType,out var autoConfigurationInstance);
                if(autoConfigurationInstance == null) continue;


                foreach (var beanMethod in autoConfigurationDetail.BeanMethodInfoList)
                {
                    if (beanMethod.Item2.IsVirtual)
                    {
                        throw new InvalidOperationException(
                            $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` can not be virtual!");
                    }

                    if (beanMethod.Item2.GetParameters().Length != 0)
                    {
                        throw new InvalidOperationException(
                            $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` can only none parameters!");
                    }
                    if (!ProxyUtil.IsAccessible(beanMethod.Item2.ReturnType))
                    {
                        throw new InvalidOperationException(
                            $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` returnType is not accessible!");
                    }
                    if (beanMethod.Item2.ReturnType.IsValueType || beanMethod.Item2.ReturnType.IsEnum)
                    {
                        throw new InvalidOperationException(
                            $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` returnType is invalid!");
                    }

                    var result = beanMethod.Item2.Invoke(autoConfigurationInstance, null);
                    if(result == null) continue;
                    context.ComponentRegistry.Register(AutoConfigurationHelper.RegisterInstance(context.ComponentRegistry, beanMethod.Item2.ReturnType,result, beanMethod.Item1.Key).CreateRegistration());
                }
                
            }
        }
    }


    internal static class AutoConfigurationHelper
    {

        public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle>RegisterInstance(IComponentRegistry cr, Type returnType,object instance,string key)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var activator = new ProvidedInstanceActivator(instance);
            var instanceType = instance.GetType();
            var rb = RegistrationBuilder.ForDelegate(instanceType, ((context, parameters) => instance ));
            if (returnType != instanceType)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    rb.Keyed(key, returnType);
                }
                else
                {
                    rb.As(returnType);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(key))
                {
                    rb.Keyed(key, instanceType);
                }
                else
                {
                    rb.As(instanceType);
                }
            }
            
            rb.SingleInstance();
            if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) ||
                rb.RegistrationData.Sharing != InstanceSharing.Shared)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The instance  registration `{0}` can support SingleInstance() sharing only", instance));
            }

            activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;

            RegistrationBuilder.RegisterSingleComponent(cr, rb);

            return rb;
        }
    }

    /// <summary>
    /// AutoConfiguration装配集合数据源
    /// </summary>
    internal class AutoConfigurationList
    {

        /// <summary>
        /// AutoConfiguration装配集合数据源
        /// </summary>
        public List<AutoConfigurationDetail> AutoConfigurationDetailList { get; set; }

    }

    /// <summary>
    /// AutoConfiguration装配集合数据源
    /// </summary>
    internal class AutoConfigurationDetail
    {
        /// <summary>
        /// Configuration 所在的类的类型
        /// </summary>
        public Type AutoConfigurationClassType { get; set; }

        /// <summary>
        /// Configuration 所在的类的里面有Bean标签的所有方法
        /// </summary>
        public List<Tuple<Bean,MethodInfo>> BeanMethodInfoList { get; set; }

    }
}
