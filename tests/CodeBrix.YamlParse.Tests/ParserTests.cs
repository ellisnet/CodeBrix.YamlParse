using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBrix.YamlParse.Core;
using CodeBrix.YamlParse.Core.Events;
using SilverAssertions;
using Xunit;

namespace CodeBrix.YamlParse.Tests;

public class ParserTests
{
    private static List<ParsingEvent> Parse(string yaml)
    {
        var parser = new Parser(new StringReader(yaml));
        var events = new List<ParsingEvent>();
        while (parser.MoveNext())
        {
            events.Add(parser.Current!);
        }
        return events;
    }

    [Fact]
    public void MoveNext_emits_stream_start_first()
        => Parse("a: 1\n")[0].Should().BeOfType<StreamStart>();

    [Fact]
    public void MoveNext_yields_scalar_events_for_keys_and_values()
    {
        //Arrange
        var events = Parse("name: Inanna\n");

        //Act
        var scalarValues = events.OfType<Scalar>().Select(s => s.Value).ToList();

        //Assert
        scalarValues.Should().ContainInOrder("name", "Inanna");
    }

    [Fact]
    public void MoveNext_emits_mapping_start_for_block_mapping()
        => Parse("name: Inanna\n").OfType<MappingStart>().Should().HaveCount(1);

    [Fact]
    public void Parser_on_malformed_input_throws_yaml_exception()
    {
        //Arrange
        var act = () => Parse("[1, 2, 3");

        //Assert
        act.Should().Throw<YamlException>();
    }
}
