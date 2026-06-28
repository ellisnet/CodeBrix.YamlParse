using System.IO;
using CodeBrix.YamlParse.RepresentationModel;
using SilverAssertions;
using Xunit;

namespace CodeBrix.YamlParse.Tests;

public class YamlStreamTests
{
    [Fact]
    public void Load_parses_single_document()
    {
        //Arrange
        var stream = new YamlStream();

        //Act
        stream.Load(new StringReader("name: Inanna\n"));

        //Assert
        stream.Documents.Should().HaveCount(1);
    }

    [Fact]
    public void Load_mapping_exposes_scalar_values()
    {
        //Arrange
        var stream = new YamlStream();
        stream.Load(new StringReader("city: Uruk\nriver: Euphrates\n"));

        //Act
        var root = (YamlMappingNode)stream.Documents[0].RootNode;
        var city = (YamlScalarNode)root.Children[new YamlScalarNode("city")];

        //Assert
        city.Value.Should().Be("Uruk");
    }

    [Fact]
    public void Load_sequence_exposes_items_in_order()
    {
        //Arrange
        var stream = new YamlStream();
        stream.Load(new StringReader("- one\n- two\n- three\n"));

        //Act
        var root = (YamlSequenceNode)stream.Documents[0].RootNode;

        //Assert
        root.Children.Should().HaveCount(3);
        ((YamlScalarNode)root.Children[0]).Value.Should().Be("one");
        ((YamlScalarNode)root.Children[2]).Value.Should().Be("three");
    }

    [Fact]
    public void Save_emits_loadable_yaml()
    {
        //Arrange
        var stream = new YamlStream();
        stream.Load(new StringReader("greeting: hello\n"));
        var writer = new StringWriter();

        //Act
        stream.Save(writer, assignAnchors: false);

        //Assert
        writer.ToString().Should().Contain("greeting: hello");
    }

    [Fact]
    public void Scalar_node_constructed_from_value_roundtrips_value()
        => new YamlScalarNode("temple").Value.Should().Be("temple");
}
