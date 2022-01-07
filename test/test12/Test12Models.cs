//-----------------------------------------------------------------------
// <copyright file="Test12Models .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Autofac.Annotation.Test.test10;
using Xunit;

namespace Autofac.Annotation.Test.test12;

/// <summary>
/// 
/// </summary>
[AutoConfiguration]
public class Test12Models
{
    public static List<String> result = new List<string>();
    
    [Bean(AutofacScope = AutofacScope.InstancePerDependency)]
    public Test12Bean1 get12()
    {
        return new Test12Bean1 { Hello = "world" };
    }
    
    [Bean]
    [DependsOn(typeof(Test12Bean4),typeof(Test12Bean5))]
    public Test12Bean3 get13()
    {
        Debug.WriteLine("new Test12Bean3");
        result.Add("get13");
        return new Test12Bean3 { Hello = "world" };
    }
    
    [Bean]
    public Test12Bean4 get14()
    {
        Debug.WriteLine("new Test12Bean4");
        result.Add("get14");
        return new Test12Bean4 { Hello = "world" };
    }
    
    [Bean]
    [DependsOn(typeof(Test12Bean6))]
    public Test12Bean5 get15()
    {
        Debug.WriteLine("new Test12Bean5");
        result.Add("get15");
        return new Test12Bean5 { Hello = "world" };
    }
    
    [Bean]
    [DependsOn(typeof(Test12Bean7))]
    public Test12Bean6 get16()
    {
        Debug.WriteLine("new Test12Bean6");
        result.Add("get16");
        return new Test12Bean6 { Hello = "world" };
    }
}

public class Test12Bean1
{
    public string Hello { get; set; }
}

[Component]
public class Test12Bean2
{
    
    [Autowired]
    public Test12Bean1 Bean1 { get; set; }
    
    public string Hello { get; set; }
}

public class Test12Bean3
{
    public string Hello { get; set; }
}

public class Test12Bean4
{
    public string Hello { get; set; }
}

public class Test12Bean5
{
    public string Hello { get; set; }
}
public class Test12Bean6
{
    public string Hello { get; set; }
}

[Component]
[DependsOn(typeof(Test12Bean8))]
public class Test12Bean7
{
    public Test12Bean7()
    {
        Test12Models.result.Add("get17"); 
    }
    public string Hello { get; set; }
}


[Component]
public class Test12Bean8
{
    public Test12Bean8()
    {
        Test12Models.result.Add("get18"); 
    }
    public string Hello { get; set; }
}