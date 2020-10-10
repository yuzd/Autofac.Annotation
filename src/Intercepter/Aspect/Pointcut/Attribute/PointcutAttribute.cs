using System;
using System.Reflection;
using Autofac.Annotation.Util;

namespace Autofac.Annotation
{

  /// <summary>
    /// 在打了AOP配置类的方法上面 可以配置切入点 这样就不用一个个class上去配置了
    /// sql like的匹配模式 % 代表通配符 _代表匹配任意一个字符
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
    public class Pointcut : System.Attribute
    {
        
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="groupName">名称 唯一</param>
        public Pointcut(string groupName)
        {
            this.GroupName = groupName;
        }

        /// <summary>
        /// 空构造
        /// </summary>
        public Pointcut()
        {
            
        }

        /// <summary>
        /// 唯一的名称
        /// </summary>
        public string GroupName { get; set; } = "";
        /// <summary>
        /// 被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }
        /// <summary>
        /// 被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }
        
        /// <summary>
        /// 值越低，优先级越高
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 用于匹配返回类型
        /// </summary>
        private string _retType = "%";
        /// <summary>
        /// 用于匹配返回类型 支持sql的like表达式
        /// </summary>
        public string RetType {
            get => _retType;
            set => _retType = value.Replace("*", "%").Replace("?","_");
        } 
        
        /// <summary>
        /// 用于匹配包类型
        /// </summary>
        private string _nameSpace = "%";
        /// <summary>
        /// 用于匹配包类型 支持sql的like表达式
        /// </summary>
        public string NameSpace {
            get => _nameSpace;
            set => _nameSpace = value.Replace("*", "%").Replace("?","_");
        }

        
        /// <summary>
        /// class的名称
        /// </summary>
        private string _clas;
        /// <summary>
        /// class的名称  支持sql的like表达式
        /// </summary>
        public string Class
        {
            get => _clas;
            set => _clas = value.Replace("*", "%").Replace("?","_");
        } 

        /// <summary>
        /// class的名称
        /// </summary>
        private string _method = "%";
        /// <summary>
        /// 方法名称 支持sql的like表达式
        /// </summary>
        public string Method 
        {
            get => _method;
            set => _method = value.Replace("*", "%").Replace("?","_");
        }


        /// <summary>
        /// 是否当前class满足
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public bool IsVaildClass(Type classType)
        {
            if (!SqlLikeStringUtilities.SqlLike(this.NameSpace, classType.Namespace))
            {
                return false;
            }
            
            if (!SqlLikeStringUtilities.SqlLike(this.Class, classType.Name))
            {
                return false;
            }

            return true;
        }
        

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsVaild(MethodInfo methodInfo)
        {
            var classType = methodInfo.DeclaringType;
            
            //如果没有设定clasname的匹配 就不继续往下了
            if (string.IsNullOrEmpty(Class) || classType == null) return false;
            
            //如果本身带了_的话
            //test_a  
            if (!SqlLikeStringUtilities.SqlLike(this.RetType, methodInfo.ReturnType.Name))
            {
                return false;
            }
            
            if (!SqlLikeStringUtilities.SqlLike(this.NameSpace, classType.Namespace))
            {
                return false;
            }
            
            if (!SqlLikeStringUtilities.SqlLike(this.Class, classType.Name))
            {
                return false;
            }

            if (!SqlLikeStringUtilities.SqlLike(this.Method, methodInfo.Name))
            {
                return false;
            }

            return true;
        }

    }

  
}