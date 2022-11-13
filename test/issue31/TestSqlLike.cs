//-----------------------------------------------------------------------
// <copyright file="TestSqlLike .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using Autofac.Annotation.Util;
using Xunit;

namespace Autofac.Annotation.Test.issue31;


/// <summary>
/// 
/// </summary>
public class TestSqlLike
{
    [Fact]
    public void Test_Type_01()
    {
        Assert.True(SqlLikeStringUtilities.SqlLike("[^(Api)(DEF)]*Controller", "ActionGroupController")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[^(Api)]*Controller", "ActionGroupController")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[^(DEF)]*Controller", "ActionGroupController")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[^(Act)]*Controller", "ActionGroupController")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[(Act)]*Controller", "ActionGroupController")); 
    }
    [Fact]
    public void Test_Type_Normal()
    {
        Assert.True(SqlLikeStringUtilities.SqlLike("%", "")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%", " ")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%", "asdfa asdf asdf")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%", "%")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("_", "")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("_", " ")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("_", "4")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("_", "C")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("_", "CX")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[ABCD]", "")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[ABCD]", "A")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[ABCD]", "b")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[ABCD]", "B")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[ABCD]", "X")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[ABCD]", "AB")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[B-D]", "C")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[B-D]", "D")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[B-D]", "A")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[^B-D]", "C")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("[^B-D]", "D")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("[^B-D]", "A")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%TEST[ABCD]XXX", "lolTESTBXXX")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("%TEST[ABCD]XXX", "lolTESTZXXX")); 
        Assert.False(SqlLikeStringUtilities.SqlLike("%TEST[^ABCD]XXX", "lolTESTBXXX")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%TEST[^ABCD]XXX", "lolTESTZXXX")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%TEST[B-D]XXX", "lolTESTBXXX")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%TEST[^B-D]XXX", "lolTESTZXXX")); 
        Assert.True(SqlLikeStringUtilities.SqlLike("%Stuff.txt", "Stuff.txt")); 
        
    }
}