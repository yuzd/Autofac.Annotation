using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Configuration.Test.test7;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using Nacos.Config;
using Nacos.Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Autofac.Annotation.Test.test8
{
    public class UnitTest8
    {
        
        [Fact]
        public async Task Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest7).Assembly));
          

            NacosV2ConfigurationSource nacosConfig = new NacosV2ConfigurationSource
            {
                Tenant = "",
                ServerAddresses = new List<string> { "http://192.168.159.140:8848/" },
                UserName = "nacos",
                Password = "nacos",
                EndPoint = "acm.aliyun.com",
                Listeners = new List<ConfigListener>
                {
                    new ConfigListener
                    {
                        DataId = "jsontest",
                        Group = "DEFAULT_GROUP",
                        Optional = false
                    }
                },
                NacosConfigurationParser = DefaultJsonConfigurationStringParser.Instance
            };

            var configurationProvider = nacosConfig.Build(null);

            builder.RegisterInstance(configurationProvider).Keyed<IConfigurationProvider>("nacos").SingleInstance();


            var container = builder.Build();

            var testNacos = container.Resolve<TestNacos>();

            var config = testNacos.NacosConfigJsonModel.Value;

            Assert.Equal("22",config);

        }
        
    }




    [PropertySource(Dynamic = typeof(IConfigurationProvider),Key = "nacos")]
    [Component]
    public class TestNacos{

        [Value("${a}")]
        public IValue<string> NacosConfigJsonModel { get; set; }


    }

    public class NacosConfigJsonModel
    {
        public string a { get; set; }   
    }

    internal class DefaultJsonConfigurationStringParser : INacosConfigurationParser
    {
        private DefaultJsonConfigurationStringParser()
        {
        }

        internal static DefaultJsonConfigurationStringParser Instance = new DefaultJsonConfigurationStringParser();

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        private JsonTextReader _reader;

        public IDictionary<string, string> Parse(string input)
            => new DefaultJsonConfigurationStringParser().ParseString(input);

        private IDictionary<string, string> ParseString(string input)
        {
            _data.Clear();
            _reader = new JsonTextReader(new StringReader(input))
            {
                DateParseHandling = DateParseHandling.None
            };

            var jsonConfig = JObject.Load(_reader);

            VisitJObject(jsonConfig);

            return _data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException(
                        $"Unsupported JSON token '{_reader.TokenType}' was found. Path '{_reader.Path}', line {_reader.LineNumber} position {_reader.LinePosition}.");
            }
        }

        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException($"A duplicate key '{key}' was found.");
            }

            _data[key] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }

}