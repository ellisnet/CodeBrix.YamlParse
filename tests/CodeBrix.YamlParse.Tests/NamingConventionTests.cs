using CodeBrix.YamlParse.Serialization.NamingConventions;
using SilverAssertions;
using Xunit;

namespace CodeBrix.YamlParse.Tests;

public class NamingConventionTests
{
    [Fact]
    public void CamelCase_lowercases_first_letter()
        => CamelCaseNamingConvention.Instance.Apply("SomeProperty").Should().Be("someProperty");

    [Fact]
    public void PascalCase_uppercases_first_letter()
        => PascalCaseNamingConvention.Instance.Apply("someProperty").Should().Be("SomeProperty");

    [Fact]
    public void Hyphenated_inserts_hyphens_between_words()
        => HyphenatedNamingConvention.Instance.Apply("SomeProperty").Should().Be("some-property");

    [Fact]
    public void Underscored_inserts_underscores_between_words()
        => UnderscoredNamingConvention.Instance.Apply("SomeProperty").Should().Be("some_property");

    [Fact]
    public void LowerCase_lowercases_entire_name()
        => LowerCaseNamingConvention.Instance.Apply("Some").Should().Be("some");

    [Fact]
    public void Null_convention_returns_input_unchanged()
        => NullNamingConvention.Instance.Apply("SomeProperty").Should().Be("SomeProperty");
}
