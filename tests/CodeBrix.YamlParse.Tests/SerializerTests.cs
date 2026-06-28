using System.Collections.Generic;
using CodeBrix.YamlParse.Serialization;
using CodeBrix.YamlParse.Serialization.NamingConventions;
using SilverAssertions;
using Xunit;

namespace CodeBrix.YamlParse.Tests;

public class SerializerTests
{
    [Fact]
    public void Serialize_writes_scalar_property()
    {
        //Arrange
        var serializer = new SerializerBuilder().Build();

        //Act
        var yaml = serializer.Serialize(new Person { Name = "Inanna", Age = 7 });

        //Assert
        yaml.Should().Contain("Name: Inanna");
        yaml.Should().Contain("Age: 7");
    }

    [Fact]
    public void Serialize_with_camel_case_lowercases_first_letter()
    {
        //Arrange
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        //Act
        var yaml = serializer.Serialize(new Person { Name = "Uruk", Age = 1 });

        //Assert
        yaml.Should().Contain("name: Uruk");
        yaml.Should().Contain("age: 1");
    }

    [Fact]
    public void Serialize_dictionary_writes_key_value_pairs()
    {
        //Arrange
        var serializer = new SerializerBuilder().Build();
        var data = new Dictionary<string, string> { ["city"] = "Uruk", ["river"] = "Euphrates" };

        //Act
        var yaml = serializer.Serialize(data);

        //Assert
        yaml.Should().Contain("city: Uruk");
        yaml.Should().Contain("river: Euphrates");
    }

    [Fact]
    public void Serialize_list_writes_block_sequence()
    {
        //Arrange
        var serializer = new SerializerBuilder().Build();

        //Act
        var yaml = serializer.Serialize(new List<string> { "one", "two" });

        //Assert
        yaml.Should().Contain("- one");
        yaml.Should().Contain("- two");
    }

    [Fact]
    public void Serialize_nested_object_emits_nested_mapping()
    {
        //Arrange
        var serializer = new SerializerBuilder().Build();
        var catalog = new Catalog { Title = "Temple", Items = new List<string> { "tablet", "seal" } };

        //Act
        var yaml = serializer.Serialize(catalog);

        //Assert
        yaml.Should().Contain("Title: Temple");
        yaml.Should().Contain("- tablet");
        yaml.Should().Contain("- seal");
    }

    [Fact]
    public void Serialize_null_graph_emits_empty_document()
        => new SerializerBuilder().Build().Serialize(null).Should().Contain("---");
}
