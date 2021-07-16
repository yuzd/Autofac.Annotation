using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter
{
    internal class AspectMiddlewareBuilder
    {
        private readonly LinkedList<AspectMiddlewareComponentNode> Components = new LinkedList<AspectMiddlewareComponentNode>();

        /// <summary>
        /// 新增拦截器链
        /// </summary>
        /// <param name="component"></param>
        public void Use(Func<AspectDelegate, AspectDelegate> component)
        {
            var node = new AspectMiddlewareComponentNode
            {
                Component = component
            };

            Components.AddLast(node);
        }

        /// <summary>
        /// 构建拦截器链
        /// </summary>
        /// <returns></returns>
        public AspectDelegate Build()
        {
            var node = Components.Last;
            while (node != null)
            {
                node.Value.Next = GetNextFunc(node);
                node.Value.Process = node.Value.Component(node.Value.Next);
                node = node.Previous;
            }

            return Components.First.Value.Process;
        }

        /// <summary>
        /// 获取下一个
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private AspectDelegate GetNextFunc(LinkedListNode<AspectMiddlewareComponentNode> node)
        {
            return node.Next == null ? ctx => Task.CompletedTask : node.Next.Value.Process;
        }
    }
}