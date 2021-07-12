using Autofac.Core.Registration;

namespace Autofac.Annotation.Condition
{
    /// <summary>
    /// match
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// return true = skip Register
        /// </summary>
        bool match(IComponentRegistryBuilder context,object metadata);
    }
    
    
}