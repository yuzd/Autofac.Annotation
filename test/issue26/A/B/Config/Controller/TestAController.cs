//-----------------------------------------------------------------------
// <copyright file="TestAController .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using Autofac.Annotation.Test.test13;

namespace Autofac.Annotation.Test.issue26.A.B.Config.Controller;

[Component]
public class TestAController
{
    [Autowired]
    private PointCutTestResult _pointCutTestResult;
    
    public virtual void Test()
    {
        _pointCutTestResult.result12.Add("A.B.Config.Controller.TestAController.Test");
    }
    
}