// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding;

public class PrefixContainerTest
{
    [Fact]
    public void ContainsPrefix_EmptyCollection_EmptyString_False()
    {
        var container = new PrefixContainer(Array.Empty<string>());
        Assert.False(container.ContainsPrefix(string.Empty));
    }

    [Fact]
    public void ContainsPrefix_HasEntries_EmptyString_True()
    {
        var container = new PrefixContainer(new[] { "some.prefix" });
        Assert.True(container.ContainsPrefix(string.Empty));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("b")]
    [InlineData("b.xy")]
    [InlineData("c.x")]
    public void ContainsPrefix_MatchesValidPrefixes(string prefix)
    {
        var container = new PrefixContainer(new[] { "a.x", "b.xy", "c.x.y" });
        Assert.True(container.ContainsPrefix(prefix));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a.b.c")]
    [InlineData("a.b[1]")]
    [InlineData("a.b0")]
    public void ContainsPrefix_RejectsInvalidPrefixes(string prefix)
    {
        var container = new PrefixContainer(new[] { "a.b", "a.bc", "a.b[c]", "a.b[0]" });
        Assert.False(container.ContainsPrefix(prefix));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void ContainsPrefix_HandlesPartialMatchesAroundValidMatch(int partialMatches)
    {
        var keys = new string[partialMatches + 1];
        for (var i = 0; i < partialMatches; i++)
        {
            keys[i] = $"aa[{i}]";
        }
        keys[partialMatches] = "a.b";

        Assert.True(new PrefixContainer(keys).ContainsPrefix("a"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("foo")]
    public void GetKeysFromPrefix_ReturnsEmptyWhenContainerIsEmpty(string prefix)
    {
        var container = new PrefixContainer(Array.Empty<string>());
        Assert.Empty(container.GetKeysFromPrefix(prefix));
    }

    [Fact]
    public void GetKeysFromPrefix_ReturnsUniqueTopLevelEntries_WhenPrefixIsEmpty()
    {
        var keys = new[] { "[0].name", "[0].address.street", "[item1].name", "[item1].age", "foo", "foo.bar" };
        var result = new PrefixContainer(keys).GetKeysFromPrefix(string.Empty);

        Assert.Equal("[0]", result["0"]);
        Assert.Equal("foo", result["foo"]);
        Assert.Equal("[item1]", result["item1"]);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetKeysFromPrefix_ReturnsEmptyDictionaryWhenNoKeysStartWithPrefix()
    {
        var keys = new[] { "foo[0].name", "foo.age", "[1].name", "[item].age" };
        Assert.Empty(new PrefixContainer(keys).GetKeysFromPrefix("baz"));
    }

    [Fact]
    public void GetKeysFromPrefix_ReturnsSubKeysThatStartWithPrefix()
    {
        var keys = new[] { "foo[0].name", "foo.age", "foo[1].name", "food[item].spice", "foo.name.first" };
        var result = new PrefixContainer(keys).GetKeysFromPrefix("foo");

        Assert.Equal("foo[0]", result["0"]);
        Assert.Equal("foo[1]", result["1"]);
        Assert.Equal("foo.age", result["age"]);
        Assert.Equal("foo.name", result["name"]);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void GetKeysFromPrefix_ReturnsNestedSubKeys()
    {
        var keys = new[] {
            "person[0].address[0].street",
            "person[0].address[1].street",
            "person[0].address[1].zip"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("person[0].address");

        Assert.Equal("person[0].address[0]", result["0"]);
        Assert.Equal("person[0].address[1]", result["1"]);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetKeysFromPrefix_IsCaseInsensitive()
    {
        var result = new PrefixContainer(new[] { "Foo.Bar" }).GetKeysFromPrefix("foo");
        Assert.Single(result);
        Assert.Equal("Foo.Bar", result["Bar"]);
    }

    [Fact]
    public void GetKeysFromPrefix_DoesNotTreatPartialNameAsChild()
    {
        var result = new PrefixContainer(new[] { "food.item", "foo.bar" }).GetKeysFromPrefix("foo");
        Assert.Single(result);
        Assert.Equal("foo.bar", result["bar"]);
    }

    [Fact]
    public void GetKeysFromPrefix_ExactEntryDoesNotCreateSubKey()
    {
        var result = new PrefixContainer(new[] { "foo", "foo.bar" }).GetKeysFromPrefix("foo");
        Assert.Single(result);
        Assert.Equal("foo.bar", result["bar"]);
    }

    [Fact]
    public void GetKeysFromPrefix_FindsPrefixAtBeginningOfSortedCollection()
    {
        var keys = new[]
        {
            "foo.first",
            "foo.second",
            "zoo.value"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("foo");

        Assert.Equal(2, result.Count);
        Assert.Equal("foo.first", result["first"]);
        Assert.Equal("foo.second", result["second"]);
    }


    [Fact]
    public void GetKeysFromPrefix_FindsPrefixInMiddleOfSortedCollection()
    {
        var keys = new[]
        {
            "aaa.value",
            "foo.first",
            "foo.second",
            "zzz.value"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("foo");

        Assert.Equal(2, result.Count);
        Assert.Equal("foo.first", result["first"]);
        Assert.Equal("foo.second", result["second"]);
    }


    [Fact]
    public void GetKeysFromPrefix_FindsPrefixAtEndOfSortedCollection()
    {
        var keys = new[]
        {
            "aaa.value",
            "bbb.value",
            "foo.last"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("foo");

        Assert.Single(result);
        Assert.Equal("foo.last", result["last"]);
    }


    [Fact]
    public void GetKeysFromPrefix_ReturnsOnlyContinuousPrefixRange()
    {
        var keys = new[]
        {
            "foo.a",
            "foo.b",
            "foo.c",
            "food.value",
            "foobar.value"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("foo");

        Assert.Equal(3, result.Count);
        Assert.Equal("foo.a", result["a"]);
        Assert.Equal("foo.b", result["b"]);
        Assert.Equal("foo.c", result["c"]);
    }

    [Fact]
    public void GetKeysFromPrefix_ReturnsEmptyWhenPrefixIsBetweenSortedValues()
    {
        var keys = new[]
        {
            "aaa.value",
            "ccc.value"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("bbb");

        Assert.Empty(result);
    }


    [Fact]
    public void GetKeysFromPrefix_MaintainsBehaviorWithDifferentCasingEntries()
    {
        var keys = new[]
        {
            "foo.Bar",
            "Foo.baz",
            "FOO.test"
        };

        var result = new PrefixContainer(keys).GetKeysFromPrefix("foo");

        Assert.Equal(3, result.Count);
        Assert.Contains("foo.Bar", result.Values);
        Assert.Contains("Foo.baz", result.Values);
        Assert.Contains("FOO.test", result.Values);
    }
}