// //-----------------------------------------------------------------------
// // <copyright file="OpenGenericRegistrationSource .cs" company="Company">
// // Copyright (C) Company. All Rights Reserved.
// // </copyright>
// // <author>nainaigu</author>
// // <create>$Date$</create>
// // <summary></summary>
// //-----------------------------------------------------------------------
//
// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using System.Reflection;
// using Autofac.Builder;
// using Autofac.Core;
// using Autofac.Core.Activators.Reflection;
// using Autofac.Core.Resolving.Pipeline;
// using Autofac.Features.OpenGenerics;
//
// namespace Autofac.Annotation.Autofac.Generics
// {
//      /// <summary>
//     /// Generates activators for open generic types.
//     /// </summary>
//     internal class OpenGenericRegistrationSource : IRegistrationSource
//     {
//         private readonly RegistrationData _registrationData;
//         private readonly IResolvePipelineBuilder _existingPipelineBuilder;
//         private readonly ReflectionActivatorData _activatorData;
//
//         private static MethodInfo EnforceBindable = null;
//         private static MethodInfo TryBindOpenGenericService = null;
//         private static MethodInfo CreateGenericService = null;
//         private static Type BuilderType = null;
//         static OpenGenericRegistrationSource()
//         {
//             var builderType = Type.GetType("Autofac.Features.OpenGenerics.OpenGenericRegistrationExtensions,Autofac").Assembly.GetTypes()
//                 .Where(r => r.IsGenericTypeDefinition && r.Name.StartsWith("RegistrationBuilder")).FirstOrDefault();
//             BuilderType = builderType?.MakeGenericType(new Type[]
//                 { typeof(object), typeof(ConcreteReflectionActivatorData), typeof(DynamicRegistrationStyle) });
//             var genericBuilderType = Type.GetType("Autofac.Features.OpenGenerics.OpenGenericRegistrationExtensions,Autofac");
//             var autofacGenericServiceBinderType = Type.GetType("Autofac.Features.OpenGenerics.OpenGenericServiceBinder,Autofac");
//             EnforceBindable = autofacGenericServiceBinderType?.GetMethod("EnforceBindable",BindingFlags.Public | BindingFlags.Static);
//             TryBindOpenGenericService = autofacGenericServiceBinderType?.GetMethod("TryBindOpenGenericService",BindingFlags.Public | BindingFlags.Static);
//             CreateGenericService = genericBuilderType?.GetMethods().Where(r=>r.Name == "CreateGenericBuilder").FirstOrDefault(r=>r.GetParameters().Length==1);
//         }
//
//         public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, object> CreateGenericBuilder(Type type)
//         {
//             //new ConcreteReflectionActivatorData()
//             var obj2 = Activator.CreateInstance(BuilderType,new object[]{new TypedService(type),new ConcreteReflectionActivatorData(type),new DynamicRegistrationStyle()});
//             //var obj = CreateGenericService.Invoke(null, new object[] { type });
//             return obj2 as IRegistrationBuilder<object, ConcreteReflectionActivatorData, object> ;
//         }
//         
//         
//         internal static IComponentRegistration CreateRegistration<TLimit, TActivatorData, TSingleRegistrationStyle>(
//              IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> builder,Guid id, IComponentRegistration target)
//             where TActivatorData : IConcreteActivatorData
//         {
//             if (builder == null)
//             {
//                 throw new ArgumentNullException(nameof(builder));
//             }
//
//             return RegistrationBuilder.CreateRegistration(
//                 id,
//                 builder.RegistrationData,
//                 builder.ActivatorData.Activator,
//                 builder.ResolvePipeline,
//                 builder.RegistrationData.Services.ToArray(),
//                 target);
//         }
//         
//         
//         /// <summary>
//         /// Initializes a new instance of the <see cref="Features.OpenGenerics.OpenGenericRegistrationSource"/> class.
//         /// </summary>
//         /// <param name="registrationData">The registration data for the open generic.</param>
//         /// <param name="existingPipelineBuilder">The pipeline for the existing open generic registration.</param>
//         /// <param name="activatorData">The activator data.</param>
//         public OpenGenericRegistrationSource(
//             RegistrationData registrationData,
//             IResolvePipelineBuilder existingPipelineBuilder,
//             ReflectionActivatorData activatorData)
//         {
//             if (registrationData == null)
//             {
//                 throw new ArgumentNullException(nameof(registrationData));
//             }
//
//             if (activatorData == null)
//             {
//                 throw new ArgumentNullException(nameof(activatorData));
//             }
//
//             EnforceBindable.Invoke(null,new object[]{activatorData.ImplementationType, registrationData.Services});
//
//             _registrationData = registrationData;
//             _existingPipelineBuilder = existingPipelineBuilder;
//             _activatorData = activatorData;
//         }
//
//
//         public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
//         {
//             if (service == null)
//             {
//                 throw new ArgumentNullException(nameof(service));
//             }
//
//             if (registrationAccessor == null)
//             {
//                 throw new ArgumentNullException(nameof(registrationAccessor));
//             }
//
//             object[] parameters = new object[] { _registrationData.Services, _activatorData.ImplementationType ,null,null};
//             bool blResult = (bool)TryBindOpenGenericService.Invoke(service, parameters);
//             if (blResult)
//             {
//                 // Pass the pipeline builder from the original registration to the 'CreateRegistration'.
//                 // So the original registration will contain all of the pipeline stages originally added, plus anything we want to add.
//                 yield return RegistrationBuilder.CreateRegistration(
//                     Guid.NewGuid(),
//                     _registrationData,
//                     new ReflectionActivator(parameters[2] as Type, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, _activatorData.ConfiguredParameters, _activatorData.ConfiguredProperties),
//                     _existingPipelineBuilder,
//                     parameters[3] as Service[]);
//             }
//         }
//
//         /// <inheritdoc/>
//         public bool IsAdapterForIndividualComponents => false;
//
//         /// <inheritdoc/>
//         public override string ToString()
//         {
//             return string.Format(
//                 CultureInfo.CurrentCulture,
//                 "OpenGenericRegistrationSourceDescription_{0}-->{1}",
//                 _activatorData.ImplementationType.FullName,
//                 string.Join(", ", _registrationData.Services.Select(s => s.Description).ToArray()));
//         }
//     }
// }

