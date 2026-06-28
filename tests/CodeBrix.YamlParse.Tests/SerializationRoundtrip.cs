using System.Collections.Generic;
using CodeBrix.YamlParse.Serialization;
using CodeBrix.YamlParse.Serialization.NamingConventions;
using SilverAssertions;
using Xunit;

namespace CodeBrix.YamlParse.Tests;

// Scenario tests that exercise the serializer and deserializer together
// (intentionally no "Tests" suffix -- this is a multi-class round-trip scenario).
public class SerializationRoundtrip
{
    [Fact]
    public void Typed_object_survives_serialize_then_deserialize()
    {
        //Arrange
        var serializer = new SerializerBuilder().Build();
        var deserializer = new DeserializerBuilder().Build();
        var original = new Person { Name = "Inanna", Age = 42 };

        //Act
        var yaml = serializer.Serialize(original);
        var restored = deserializer.Deserialize<Person>(yaml);

        //Assert
        restored.Name.Should().Be(original.Name);
        restored.Age.Should().Be(original.Age);
    }

    [Fact]
    public void Nested_object_survives_roundtrip_with_camel_case()
    {
        //Arrange
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var original = new Catalog { Title = "Temple", Items = new List<string> { "tablet", "seal", "lyre" } };

        //Act
        var yaml = serializer.Serialize(original);
        var restored = deserializer.Deserialize<Catalog>(yaml);

        //Assert
        restored.Title.Should().Be("Temple");
        restored.Items.Should().ContainInOrder("tablet", "seal", "lyre");
    }

    [Fact]
    public void Dictionary_survives_roundtrip()
    {
        //Arrange
        var serializer = new SerializerBuilder().Build();
        var deserializer = new DeserializerBuilder().Build();
        var original = new Dictionary<string, string> { ["city"] = "Uruk", ["river"] = "Euphrates" };

        //Act
        var yaml = serializer.Serialize(original);
        var restored = deserializer.Deserialize<Dictionary<string, string>>(yaml);

        //Assert
        restored.Should().HaveCount(2);
        restored["city"].Should().Be("Uruk");
    }
}
