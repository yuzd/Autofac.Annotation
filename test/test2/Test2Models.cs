using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Castle.DynamicProxy;

namespace Autofac.Configuration.Test.test2
{
   

    //注册自己 和 父类
    [Component]
    public class Model2:Model1
    {

    }

    //注册自己
    [Component]
    public class Model1
    {

    }

    //注册自己
    [Component("Model11")]
    public class Model11
    {

    }

    //注册自己 和 父类
    [Component]
    public class Model22 : Model11
    {

    }

    [Component(OrderIndex = 1000)]
    public class Model222 : Model12
    {

    }

    [Component(OrderIndex = 999)]
    public class Model12
    {

    }

    public interface Imodel1{

    }

    [Component()]
    public class Model3 : Imodel1
    {

    }

    [Component()]
    public class Model32 : Model3
    {
    }

    public interface Imodel2
    {

    }

    [Component(OrderIndex = 999)]
    public class Model4 : Imodel2
    {

    }

    [Component(OrderIndex = 1000)]
    public class Model42 : Model4
    {

    }

    public interface Imodel3
    {
        Task<string> hello();
    }

    [Component(Interceptor = typeof(Test2Interceptor),InterceptorType = InterceptorType.Interface)]
    public class Model5 : Imodel3
    {
        public async Task<string> hello()
        {
            await Task.Delay(1000);
            return await Task.FromResult(nameof(Model5));
        }
    }

    [Component(Interceptor = typeof(Test2Interceptor))]
    public class Model55
    {
        public virtual async Task<string> hello()
        {
            return await Task.FromResult(nameof(Model55));
        }
    }


    [Component(Interceptor = typeof(Test2Interceptor2))]
    public class Model6
    {
        public virtual async Task<string> hello()
        {
            return await Task.FromResult(nameof(Model6));
        }
    }

    [Component(Interceptor = typeof(AsyncInterceptor),InterceptorKey = nameof(Test2Interceptor2))]
    public class Model61
    {
        public virtual async Task<string> hello()
        {
            return await Task.FromResult(nameof(Model61));
        }
    }

    [Component]
    public class Model7
    {
        [Autowired]
        public IList<AsyncInterceptor> Interceptors;
    }
}
