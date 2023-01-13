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
public class NacosTest
{
    [Value("${a}")] public IValue<string> a;
}