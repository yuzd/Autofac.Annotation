using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// Autodiscovery, to automatically load annotation from assembly 
namespace Autofac.Annotation.Autodiscovery
{
    /// AutodiscoveryBuilder, main class
    /// You only need Autofac.Annotation and this class in your project's dependency 
    public sealed class AutodiscoveryBuilder
    {

        private static IContainer container = autodiscovery();



        private static IContainer autodiscovery()
        {
            ContainerBuilder builder = new ContainerBuilder();
            List<Assembly> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Select(
                a =>
                {
                    // prevent exception accessing Location
                    try
                    {
                        return a.Location;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            ).ToArray();
            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            toLoad.ForEach(
                path =>
                {
                    // prevent exception loading some assembly
                    try
                    {
                        loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                    }
                    catch (Exception)
                    {
                        ; // DO NOTHING
                    }
                }
            );

            // prevent loading of dynamic assembly, autofac doesn't support dynamic assembly
            loadedAssemblies.RemoveAll(i => i.IsDynamic);

            // do the magic registering all the assembly
            builder.RegisterModule(new AutofacAnnotationModule(loadedAssemblies.ToArray()));
            return builder.Build();
        }

        /// <summary>
        /// Incapsulate Autofac Resolve
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService Resolve<TService>() /*where TService : notnull*/
        {
            if (container == null)
                container = autodiscovery();
            TService i = container.Resolve<TService>();
            if (i == null)
                throw new Exception("Impossibile istanziare");
            return i;
        }

        /// <summary>
        /// incapsulate Autofac BeginLifetimeScope
        /// </summary>
        /// <returns></returns>
        public static IDisposable BeginLifetimeScope()
        {
            return container.BeginLifetimeScope();
        }

        /// <summary>
        /// return the container, if you want to access it directly
        /// </summary>
        /// <returns></returns>
        public static IContainer getContainer()
        {
            return container;
        }
    }

}