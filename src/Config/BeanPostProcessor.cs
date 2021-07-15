namespace Autofac.Annotation.Config
{
    /// <summary>
    ///  Bean增强 处理器
    ///  不能和Bean/AutoConfiguration/PointCut注解一起使用
    /// </summary>
    public interface BeanPostProcessor
    {
        /// <summary>
        /// before
        /// 该方法在bean实例化完毕（且已经注入完毕），在afterPropertiesSet或自定义init方法执行之前
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        object PostProcessBeforeInitialization(object bean);


        /// <summary>
        /// after
        /// 在afterPropertiesSet或自定义init方法执行之后
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        object PostProcessAfterInitialization(object bean);
    }
}