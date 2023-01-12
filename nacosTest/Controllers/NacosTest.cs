//-----------------------------------------------------------------------
// <copyright file="NacosTest .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------


using Autofac.Annotation;

namespace nacosTest.Controllers;

[Component]
[PropertySource(Dynamic = typeof(IConfigurationProvider), Key = "nacos")] // 这里代表使用nacos的source源文件
public class NacosTest
{
    [Value("${a}")] public IValue<string> a;
}