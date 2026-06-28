using System.Collections.Generic;

namespace CodeBrix.YamlParse.Tests;

// Shared POCOs used across the test suite (helper file -- intentionally no "Tests" suffix).

public class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

public class Catalog
{
    public string Title { get; set; } = "";
    public List<string> Items { get; set; } = new();
}
