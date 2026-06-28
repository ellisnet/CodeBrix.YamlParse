================================================================================
AGENT-README: CodeBrix.YamlParse
A Comprehensive Guide for AI Coding Agents
================================================================================

OVERVIEW
--------------------------------------------------------------------------------
CodeBrix.YamlParse is a fully managed, cross-platform .NET library for YAML. It
is a faithful port of YamlDotNet 18.1.0 into the CodeBrix.YamlParse.* namespace,
targeting .NET 10. It offers three layers, mirroring YamlDotNet:

  1. A low-level streaming Scanner / Parser / Emitter (CodeBrix.YamlParse.Core),
     itself historically derived from the libyaml C library.
  2. A high-level representation model (CodeBrix.YamlParse.RepresentationModel),
     similar to System.Xml's XmlDocument, for loading/editing/saving documents.
  3. An object serialization library (CodeBrix.YamlParse.Serialization) that
     reads and writes .NET objects to and from YAML.

The library has no third-party NuGet dependencies; it depends only on .NET 10.


INSTALLATION
--------------------------------------------------------------------------------
NuGet package id:   CodeBrix.YamlParse.MitLicenseForever
Install:            dotnet add package CodeBrix.YamlParse.MitLicenseForever
Namespace:          CodeBrix.YamlParse (NOTE: the namespace has NO license
                    suffix; only the NuGet package id carries ".MitLicenseForever")
Target framework:   .NET 10.0 or higher
License:            MIT (this library), derived from YamlDotNet (MIT) and
                    libyaml (MIT) -- see THIRD-PARTY-NOTICES.txt


KEY NAMESPACES
--------------------------------------------------------------------------------
  using CodeBrix.YamlParse.Serialization;                   // Serializer/Deserializer + builders
  using CodeBrix.YamlParse.Serialization.NamingConventions; // CamelCase, PascalCase, etc.
  using CodeBrix.YamlParse.RepresentationModel;             // YamlStream/YamlDocument/YamlNode
  using CodeBrix.YamlParse.Core;                            // Parser/Scanner/Emitter
  using CodeBrix.YamlParse.Core.Events;                     // parsing/emitting events
  using CodeBrix.YamlParse.Core.Tokens;                     // scanner tokens

Other sub-namespaces (advanced extension points):
  CodeBrix.YamlParse.Serialization.Converters
  CodeBrix.YamlParse.Serialization.EventEmitters
  CodeBrix.YamlParse.Serialization.NodeDeserializers
  CodeBrix.YamlParse.Serialization.NodeTypeResolvers
  CodeBrix.YamlParse.Serialization.ObjectFactories
  CodeBrix.YamlParse.Serialization.ObjectGraphTraversalStrategies
  CodeBrix.YamlParse.Serialization.ObjectGraphVisitors
  CodeBrix.YamlParse.Serialization.Schemas
  CodeBrix.YamlParse.Serialization.TypeInspectors
  CodeBrix.YamlParse.Serialization.TypeResolvers
  CodeBrix.YamlParse.Serialization.Utilities
  CodeBrix.YamlParse.Serialization.ValueDeserializers
  CodeBrix.YamlParse.Serialization.BufferedDeserialization (+ .TypeDiscriminators)
  CodeBrix.YamlParse.Serialization.Callbacks


CORE API REFERENCE
--------------------------------------------------------------------------------
Serialization (most common entry points):

  SerializerBuilder / Serializer
    var serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    string yaml = serializer.Serialize(myObject);

  DeserializerBuilder / Deserializer
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    var obj = deserializer.Deserialize<MyType>(yamlText);

  The builders are fluent and expose extension hooks: WithTypeConverter,
  WithNodeDeserializer, WithTypeInspector, WithTypeMapping, IgnoreUnmatched-
  Properties, EnsureRoundtrip, and many more -- identical in shape to YamlDotNet.

  Naming conventions (CodeBrix.YamlParse.Serialization.NamingConventions):
    CamelCaseNamingConvention, PascalCaseNamingConvention,
    HyphenatedNamingConvention, UnderscoredNamingConvention,
    LowerCaseNamingConvention, NullNamingConvention -- each exposes .Instance.

Representation model:

  YamlStream    -- a set of YAML documents; .Load(TextReader) / .Save(TextWriter)
  YamlDocument  -- one document; .RootNode
  YamlNode      -- base; concrete: YamlScalarNode, YamlSequenceNode, YamlMappingNode

Low-level streaming:

  Scanner  -- tokenizes a TextReader into Core.Tokens
  Parser   -- produces Core.Events from a Scanner
  Emitter  -- writes Core.Events to a TextWriter

Error model:
  YamlException (base), SemanticErrorException, SyntaxErrorException, and
  related types in CodeBrix.YamlParse.Core signal malformed input or
  serialization failures.


CODING CONVENTIONS (CodeBrix family)
--------------------------------------------------------------------------------
This repository follows the CodeBrix family conventions, with TWO deliberate,
documented exceptions justified by the port (see ARCHITECTURE below):

  - File-scoped namespaces only; no block-scoped namespaces.
  - No `global using` directives; usings are explicit and per-file.
  - Target framework is net10.0 only; no multi-targeting.
  - Tests use xUnit v3 + SilverAssertions (NOT the upstream xUnit v2 /
    FluentAssertions / FakeItEasy stack).
  - No project-level warning suppression (<NoWarn>); the library builds with
    zero warnings and zero errors.
  - The library project root carries InternalsVisibleTo.cs granting internals
    access to CodeBrix.YamlParse.Tests.

  EXCEPTION 1 -- <Nullable>enable</Nullable> is ON for this library (the normal
  family rule leaves NRT unset). YamlDotNet's public API depends pervasively on
  nullable-reference-type (`?`) annotations; stripping them would change
  observable signatures and break fidelity with the upstream package. This is
  the same situational exception used by CodeBrix.Platform.OpenGL (a Silk.NET
  port). Because NRT is ON here, the null-forgiveness `!` operator is permitted
  in this repository (it is forbidden only in NRT-off CodeBrix repos).

  EXCEPTION 2 -- <GenerateDocumentationFile>false</GenerateDocumentationFile>
  for this library (the normal family rule is `true`, requiring XML doc comments
  on every public member). The ported public surface is hundreds of members,
  undocumented upstream (YamlDotNet itself suppresses CS1591). Retrofitting an
  XML doc comment onto every public member is out of scope for the v1 port. This
  is the same situational exception used by CodeBrix.AssemblyTools (a Mono.Cecil
  port). Many ported types DO already carry upstream `///` doc comments; new
  public members added later should be documented.


ARCHITECTURE
--------------------------------------------------------------------------------
Source layout under src/CodeBrix.YamlParse/ mirrors YamlDotNet:

  (root)            CultureInfoAdapter, PropertyInfoExtensions,
                    ReflectionExtensions, StandardRegexOptions, Polyfills helpers
  Core/             Scanner, Parser, Emitter, Events/, Tokens/, ObjectPool/
  Helpers/          internal helper types
  Portability/      shims (mostly inert dead branches on net10)
  RepresentationModel/  YamlStream / YamlDocument / YamlNode hierarchy
  Serialization/    Serializer/Deserializer, builders, and all the extension
                    sub-namespaces listed under KEY NAMESPACES

Port provenance: every ported file preserves YamlDotNet's original MIT license
header verbatim, and its namespace line carries a
`//was previously: YamlDotNet.<X>;` comment. Upstream framework-selection `#if`
directives were preserved (not stripped); on net10 the modern branches are
selected and the legacy netstandard / .NET Framework branches are inert.

Deferred from the v1 port:
  - YamlDotNet.Analyzers.StaticGenerator (the AOT Roslyn source generator). The
    StaticSerializerBuilder / StaticDeserializerBuilder API surface is present,
    but the compile-time static-context generator is not yet ported.


TESTING
--------------------------------------------------------------------------------
Tests live in tests/CodeBrix.YamlParse.Tests/ and use xUnit v3 + SilverAssertions.
The suite is a fresh CodeBrix-convention suite (not a port of YamlDotNet's own
test project, which depends on FakeItEasy and FluentAssertions). It exercises the
primary public API: serialization round-trips, naming conventions, the
representation model, low-level parse/emit, and error paths.

Run everything from the repository root:

  dotnet restore CodeBrix.YamlParse.slnx
  dotnet build   CodeBrix.YamlParse.slnx
  dotnet test    CodeBrix.YamlParse.slnx

================================================================================
END OF AGENT-README
================================================================================
