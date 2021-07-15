
using System;
using System.Reflection;
using Autofac.Annotation.Config;

namespace Autofac.Annotation.Test.test11
{
    [Component]
    public class Test11Models1
    {
        [Soa(typeof(SoaTest1))]
        private ISoa Soa1;
        
        [Soa(typeof(SoaTest2))]
        private ISoa Soa2;

        public string getSoa1()
        {
            return Soa1.say();
        }
        public string getSoa2()
        {
            return Soa2.say();
        }
    }

    public interface ISoa
    {
        string say();
    }

    public class SoaTest1 : ISoa
    {
        public string say()
        {
            return nameof(SoaTest1);
        }
    }
    public class SoaTest2 : ISoa
    {
        public string say()
        {
            return nameof(SoaTest2);
        }
    }


    [Component]
    public class SoaProcessor : BeanPostProcessor
    {
        public object PostProcessBeforeInitialization(object bean)
        {
            //找到bean下所有
            Type type = bean.GetType();
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fieldInfos)
            {
                var soaAnnotation = field.GetCustomAttribute(typeof(Soa)) as Soa;
                if (soaAnnotation == null)
                {
                    continue;
                }

                var instance = Activator.CreateInstance(soaAnnotation.Type) as ISoa;
                if (instance == null)
                {
                    continue;
                }
                
                field.SetValue(bean,instance);
            }

            return bean;
        }

        public object PostProcessAfterInitialization(object bean)
        {
            return bean;
        }
    }
    
    /// <summary>
    /// 測試自己實現一個自定義註解
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class Soa : System.Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Soa(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// 注册的类型
        /// </summary>
        internal Type Type { get; set; }
    }
}