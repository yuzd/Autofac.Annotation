using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac.Configuration.Test.test7;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Autofac.Annotation.Test.test8;

public class UnitTest8
{
    // [Fact]
    // public async Task Test_Type_01()
    // {
    //     var builder = new ContainerBuilder();
    //     // autofac打标签模式
    //     builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest8).Assembly));
    //
    //
    //     NacosV2ConfigurationSource nacosConfig = new NacosV2ConfigurationSource(null, null)
    //     {
    //         Namespace = "fb371f5e-78ac-45bf-aadf-076d61b865c2",
    //         AccessKey = "",
    //         SecretKey = "",
    //         ServerAddresses = new List<string> { "http://127.0.0.1:8848/" },
    //         UserName = "nacos",
    //         Password = "nacos",
    //         EndPoint = "acm.aliyun.com",
    //         Listeners = new List<ConfigListener>
    //         {
    //             new ConfigListener
    //             {
    //                 DataId = "jsontest",
    //                 Group = "DEFAULT_GROUP",
    //                 Optional = false
    //             }
    //         },
    //         NacosConfigurationParser = DefaultJsonConfigurationStringParser.Instance,
    //         ConfigUseRpc = true
    //     };
    //
    //     ServiceCollection services = new ServiceCollection();
    //     services.AddNacosV2Config(x =>
    //     {
    //         x.ServerAddresses = nacosConfig.ServerAddresses;
    //         x.Namespace = nacosConfig.Namespace;
    //         x.AccessKey = nacosConfig.AccessKey;
    //         x.ContextPath = nacosConfig.ContextPath;
    //         x.EndPoint = nacosConfig.EndPoint;
    //         x.DefaultTimeOut = nacosConfig.DefaultTimeOut;
    //         x.SecretKey = nacosConfig.SecretKey;
    //         x.Password = nacosConfig.Password;
    //         x.UserName = nacosConfig.UserName;
    //         x.ListenInterval = nacosConfig.ListenInterval;
    //         x.ConfigUseRpc = nacosConfig.ConfigUseRpc;
    //         x.ConfigFilterAssemblies = nacosConfig.ConfigFilterAssemblies;
    //         x.ConfigFilterExtInfo = nacosConfig.ConfigFilterExtInfo;
    //     }, null);
    //
    //     services.AddLogging((Action<ILoggingBuilder>)(x => x.AddConsole()));
    //     ServiceProvider provider = services.BuildServiceProvider();
    //     var client = provider.GetService<INacosConfigService>();
    //     var logFactory = provider.GetService<ILoggerFactory>();
    //     nacosConfig = new NacosV2ConfigurationSource(client, logFactory)
    //     {
    //         Namespace = "fb371f5e-78ac-45bf-aadf-076d61b865c2",
    //         AccessKey = "",
    //         SecretKey = "",
    //         ServerAddresses = new List<string> { "http://127.0.0.1:8848/" },
    //         UserName = "nacos",
    //         Password = "nacos",
    //         EndPoint = "acm.aliyun.com",
    //         Listeners = new List<ConfigListener>
    //         {
    //             new ConfigListener
    //             {
    //                 DataId = "jsontest",
    //                 Group = "DEFAULT_GROUP",
    //                 Optional = false
    //             }
    //         },
    //         NacosConfigurationParser = DefaultJsonConfigurationStringParser.Instance,
    //         ConfigUseRpc = true
    //     };
    //     var configurationProvider = nacosConfig.Build(null);
    //     builder.RegisterInstance(configurationProvider).Keyed<IConfigurationProvider>("nacos").SingleInstance();
    //
    //
    //     var container = builder.Build();
    //
    //     var testNacos = container.Resolve<TestNacos>();
    //
    //     var config = testNacos.NacosConfigJsonModel.Value;
    //
    //     Assert.Equal("22", config);
    // }
}

[PropertySource(Dynamic = typeof(IConfigurationProvider), Key = "nacos")]
[Component]
public class TestNacos
{
    [Value("${a}")] public IValue<string> NacosConfigJsonModel { get; set; }
}

public class NacosConfigJsonModel
{
    public string a { get; set; }
}
//
// internal class DefaultJsonConfigurationStringParser : INacosConfigurationParser
// {
//     internal static DefaultJsonConfigurationStringParser Instance = new DefaultJsonConfigurationStringParser();
//
//     private readonly IDictionary<string, string> _data =
//         (IDictionary<string, string>)new SortedDictionary<string, string>((IComparer<string>)StringComparer.OrdinalIgnoreCase);
//
//     private readonly Stack<string> _context = new Stack<string>();
//     private string _currentPath;
//     private JsonTextReader _reader;
//
//     private DefaultJsonConfigurationStringParser()
//     {
//     }
//
//     public IDictionary<string, string> Parse(string input) => new DefaultJsonConfigurationStringParser().ParseString(input);
//
//     private IDictionary<string, string> ParseString(string input)
//     {
//         if (string.IsNullOrEmpty(input))
//         {
//             return new Dictionary<string, string>();
//         }
//
//         this._data.Clear();
//         JsonTextReader jsonTextReader = new JsonTextReader((TextReader)new StringReader(input));
//         jsonTextReader.DateParseHandling = DateParseHandling.None;
//         this._reader = jsonTextReader;
//         this.VisitJObject(JObject.Load((JsonReader)this._reader));
//         return this._data;
//     }
//
//     private void VisitJObject(JObject jObject)
//     {
//         foreach (JProperty property in jObject.Properties())
//         {
//             this.EnterContext(property.Name);
//             this.VisitProperty(property);
//             this.ExitContext();
//         }
//     }
//
//     private void VisitProperty(JProperty property) => this.VisitToken(property.Value);
//
//     private void VisitToken(JToken token)
//     {
//         switch (token.Type)
//         {
//             case JTokenType.Object:
//                 this.VisitJObject(token.Value<JObject>());
//                 break;
//             case JTokenType.Array:
//                 this.VisitArray(token.Value<JArray>());
//                 break;
//             case JTokenType.Integer:
//             case JTokenType.Float:
//             case JTokenType.String:
//             case JTokenType.Boolean:
//             case JTokenType.Null:
//             case JTokenType.Raw:
//             case JTokenType.Bytes:
//                 this.VisitPrimitive(token.Value<JValue>());
//                 break;
//             default:
//                 DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 4);
//                 interpolatedStringHandler.AppendLiteral("Unsupported JSON token '");
//                 interpolatedStringHandler.AppendFormatted<JsonToken>(this._reader.TokenType);
//                 interpolatedStringHandler.AppendLiteral("' was found. Path '");
//                 interpolatedStringHandler.AppendFormatted(this._reader.Path);
//                 interpolatedStringHandler.AppendLiteral("', line ");
//                 interpolatedStringHandler.AppendFormatted<int>(this._reader.LineNumber);
//                 interpolatedStringHandler.AppendLiteral(" position ");
//                 interpolatedStringHandler.AppendFormatted<int>(this._reader.LinePosition);
//                 interpolatedStringHandler.AppendLiteral(".");
//                 throw new FormatException(interpolatedStringHandler.ToStringAndClear());
//         }
//     }
//
//     private void VisitArray(JArray array)
//     {
//         for (int index = 0; index < array.Count; ++index)
//         {
//             this.EnterContext(index.ToString());
//             this.VisitToken(array[index]);
//             this.ExitContext();
//         }
//     }
//
//     private void VisitPrimitive(JValue data)
//     {
//         string currentPath = this._currentPath;
//         if (this._data.ContainsKey(currentPath))
//             throw new FormatException("A duplicate key '" + currentPath + "' was found.");
//         this._data[currentPath] = data.ToString((IFormatProvider)CultureInfo.InvariantCulture);
//     }
//
//     private void EnterContext(string context)
//     {
//         this._context.Push(context);
//         this._currentPath = ConfigurationPath.Combine(this._context.Reverse<string>());
//     }
//
//     private void ExitContext()
//     {
//         this._context.Pop();
//         this._currentPath = ConfigurationPath.Combine(this._context.Reverse<string>());
//     }
// }