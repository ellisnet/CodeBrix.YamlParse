# CodeBrix.YamlParse

A fully managed, cross-platform YAML library for .NET. It provides low-level parsing and emitting of YAML, a high-level representation model similar to `XmlDocument`, and a serialization library that reads and writes objects from and to YAML streams.
CodeBrix.YamlParse has no dependencies other than .NET, and is provided as a .NET 10 library and associated `CodeBrix.YamlParse.MitLicenseForever` NuGet package.

CodeBrix.YamlParse supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

```
dotnet add package CodeBrix.YamlParse.MitLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.YamlParse`:

* NuGet package ID: `CodeBrix.YamlParse.MitLicenseForever`
* Assembly and primary namespace: `CodeBrix.YamlParse` - i.e. `using CodeBrix.YamlParse.Serialization;`

No XML documentation (IntelliSense) file ships with the assembly.

The package has no NuGet dependencies at all - nothing beyond .NET itself is pulled in.

## CodeBrix.YamlParse supports:

* **Object serialization** — convert .NET objects to YAML with `Serializer` / `SerializerBuilder`.
* **Object deserialization** — convert YAML to strongly-typed .NET objects with `Deserializer` / `DeserializerBuilder`.
* **A high-level representation model** — load, inspect, modify, and save YAML documents with `YamlStream`, `YamlDocument`, `YamlScalarNode`, `YamlSequenceNode`, and `YamlMappingNode`.
* **Low-level parsing and emitting** — stream-based YAML processing with `Parser`, `Scanner`, and `Emitter`.
* **Naming conventions** — camelCase, PascalCase, hyphenated, underscored, and lower-case property naming.
* **Custom type converters, type inspectors, node deserializers, and YAML schemas** for full control over the (de)serialization pipeline.

## Sample Code

### Serialize a .NET object to YAML

```csharp
using CodeBrix.YamlParse.Serialization;
using CodeBrix.YamlParse.Serialization.NamingConventions;

var person = new { Name = "Inanna", City = "Uruk" };

var serializer = new SerializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

string yaml = serializer.Serialize(person);
// name: Inanna
// city: Uruk
```

### Deserialize YAML into a .NET object

```csharp
using System.Collections.Generic;
using CodeBrix.YamlParse.Serialization;
using CodeBrix.YamlParse.Serialization.NamingConventions;

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

var result = deserializer.Deserialize<Dictionary<string, string>>("name: Inanna\ncity: Uruk\n");
```

### Walk the representation model

```csharp
using System.IO;
using CodeBrix.YamlParse.RepresentationModel;

var yaml = new YamlStream();
yaml.Load(new StringReader("- one\n- two\n- three\n"));

var root = (YamlSequenceNode)yaml.Documents[0].RootNode;
foreach (var item in root.Children)
{
    System.Console.WriteLine(((YamlScalarNode)item).Value);
}
```

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library.

Additional sample code and usage examples are available in the `CodeBrix.YamlParse.Tests` project:
https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests

## License

CodeBrix.YamlParse is licensed under the MIT License - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/LICENSE) file.

For licensing and provenance information about the open source code included in
this package - the copyright notices and license texts of everything it is built
from - see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/THIRD-PARTY-NOTICES.txt).
