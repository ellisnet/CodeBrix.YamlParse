# CodeBrix.YamlParse

A fully managed, cross-platform YAML library for .NET — a faithful port of [YamlDotNet](https://github.com/aaubry/YamlDotNet) 18.1.0 into the `CodeBrix.YamlParse` namespace. It provides low-level parsing and emitting of YAML, a high-level representation model similar to `XmlDocument`, and a serialization library that reads and writes objects from and to YAML streams.
CodeBrix.YamlParse has no dependencies other than .NET, and is provided as a .NET 10 library and associated `CodeBrix.YamlParse.MitLicenseForever` NuGet package.

CodeBrix.YamlParse supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

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

## License

The project is licensed under the MIT License. see: https://en.wikipedia.org/wiki/MIT_License

CodeBrix.YamlParse is a derivative work of YamlDotNet (MIT) and, through it, libyaml (MIT). Their copyright notices and license texts are reproduced in [THIRD-PARTY-NOTICES.txt](./THIRD-PARTY-NOTICES.txt).
