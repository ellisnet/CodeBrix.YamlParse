using System.Collections.Generic;
using CodeBrix.YamlParse.Core;
using CodeBrix.YamlParse.Serialization;
using CodeBrix.YamlParse.Serialization.NamingConventions;
using SilverAssertions;
using Xunit;

namespace CodeBrix.YamlParse.Tests;

public class DeserializerTests
{
    [Fact]
    public void Deserialize_typed_object_populates_properties()
    {
        //Arrange
        var deserializer = new DeserializerBuilder().Build();

        //Act
        var person = deserializer.Deserialize<Person>("Name: Inanna\nAge: 7\n");

        //Assert
        person.Name.Should().Be("Inanna");
        person.Age.Should().Be(7);
    }

    [Fact]
    public void Deserialize_with_camel_case_maps_lowercase_keys()
    {
        //Arrange
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        //Act
        var person = deserializer.Deserialize<Person>("name: Uruk\nage: 5\n");

        //Assert
        person.Name.Should().Be("Uruk");
        person.Age.Should().Be(5);
    }

    [Fact]
    public void Deserialize_dictionary_reads_all_entries()
    {
        //Arrange
        var deserializer = new DeserializerBuilder().Build();

        //Act
        var map = deserializer.Deserialize<Dictionary<string, string>>("city: Uruk\nriver: Euphrates\n");

        //Assert
        map.Should().HaveCount(2);
        map["city"].Should().Be("Uruk");
        map["river"].Should().Be("Euphrates");
    }

    [Fact]
    public void Deserialize_list_reads_all_items()
    {
        //Arrange
        var deserializer = new DeserializerBuilder().Build();

        //Act
        var items = deserializer.Deserialize<List<string>>("- alpha\n- beta\n- gamma\n");

        //Assert
        items.Should().HaveCount(3);
        items.Should().ContainInOrder("alpha", "beta", "gamma");
    }

    [Fact]
    public void Deserialize_unmatched_property_throws_by_default()
    {
        //Arrange
        var deserializer = new DeserializerBuilder().Build();

        //Act
        var act = () => deserializer.Deserialize<Person>("Name: x\nUnknown: y\n");

        //Assert
        act.Should().Throw<YamlException>();
    }

    [Fact]
    public void Deserialize_ignoring_unmatched_properties_succeeds()
    {
        //Arrange
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        //Act
        var person = deserializer.Deserialize<Person>("Name: x\nUnknown: y\n");

        //Assert
        person.Name.Should().Be("x");
    }

    [Fact]
    public void Deserialize_malformed_flow_sequence_throws_yaml_exception()
    {
        //Arrange
        var deserializer = new DeserializerBuilder().Build();

        //Act
        var act = () => deserializer.Deserialize<List<int>>("[1, 2, 3");

        //Assert
        act.Should().Throw<YamlException>();
    }
}
