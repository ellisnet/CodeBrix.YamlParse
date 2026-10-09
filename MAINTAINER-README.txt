================================================================================
MAINTAINER-README: CodeBrix.YamlParse
Notes for people and agents MAINTAINING this repository — not for package consumers
================================================================================

If you are consuming the NuGet package, stop reading and open AGENT-README.txt
instead. This file is about the repository itself.


PURPOSE AND SCOPE
=================

This repository produces exactly one NuGet package:

  Package id:   CodeBrix.YamlParse.MitLicenseForever
  Assembly:     CodeBrix.YamlParse.dll
  Project:      src/CodeBrix.YamlParse/CodeBrix.YamlParse.csproj
  Consumer doc: AGENT-README.txt (repo root)

One package, one AGENT-README, no sub-packages and no per-project
AGENT-README files.

The library is a faithful port of YamlDotNet 18.1.0 into the
`CodeBrix.YamlParse.*` namespaces. Fidelity to the upstream behaviour is the
goal; feature growth is not.


REPOSITORY LAYOUT
=================

  CodeBrix.YamlParse.slnx             solution; its Solution Items folder
                                      carries .gitignore, AGENT-README.txt,
                                      EXTRAS-README.txt, global.json,
                                      icon-codebrix-128.png, LICENSE,
                                      MAINTAINER-README.txt, README-INDEX.txt,
                                      README.md and THIRD-PARTY-NOTICES.txt,
                                      and its Tests folder carries the test
                                      project. Update the list when a root file
                                      is added or removed.
  global.json                         selects the Microsoft.Testing.Platform
                                      test runner; does NOT pin an SDK version
  AGENT-README.txt                    consumer documentation; SHIPS in the
                                      nupkg
  MAINTAINER-README.txt               this file
  EXTRAS-README.txt                   non-package content in the repo
  README-INDEX.txt                    map of the README files
  README.md                           human-facing readme; SHIPS in the nupkg
  LICENSE                             MIT
  THIRD-PARTY-NOTICES.txt             upstream attribution; SHIPS in the nupkg
  icon-codebrix-128.png               package icon; SHIPS in the nupkg
  AGENTS.md, CLAUDE.md, .clinerules, .cursorrules,
  .cursor/rules/agent-readme.mdc, .windsurfrules,
  .github/copilot-instructions.md, .junie/guidelines.md
                                      the eight AI-agent pointer stubs; they
                                      are reconciled centrally against the
                                      CodeBrix.SkiaSvg canonical copies -- do
                                      not hand-edit them here

  src/CodeBrix.YamlParse/
    CodeBrix.YamlParse.csproj         the one packable project
    InternalsVisibleTo.cs             grants internals to the test assembly
    CultureInfoAdapter.cs             internal culture shim
    PropertyInfoExtensions.cs         internal reflection helpers
    ReflectionExtensions.cs           internal reflection helpers
    StandardRegexOptions.cs           internal regex option constants
    Core/                             Scanner, Parser, MergingParser, Emitter,
                                      EmitterSettings, AnchorName, TagName,
                                      Mark, the exception hierarchy,
                                      ParserExtensions and scanner plumbing
      Core/Events/                    ParsingEvent hierarchy + style enums
      Core/Tokens/                    Token hierarchy
      Core/ObjectPool/                internal pooling (no public types)
    Helpers/                          IOrderedDictionary + internal helpers,
                                      including Polyfills.cs
    RepresentationModel/              YamlStream / YamlDocument / YamlNode tree
    Serialization/                    Serializer, Deserializer, both builders,
                                      the attributes, and the extension
                                      sub-namespaces: BufferedDeserialization
                                      (+ TypeDiscriminators), Callbacks,
                                      Converters, EventEmitters,
                                      NamingConventions, NodeDeserializers,
                                      NodeTypeResolvers, ObjectFactories,
                                      ObjectGraphTraversalStrategies,
                                      ObjectGraphVisitors, Schemas,
                                      TypeInspectors, TypeResolvers,
                                      Utilities, ValueDeserializers

  tests/CodeBrix.YamlParse.Tests/
    CodeBrix.YamlParse.Tests.csproj
    DeserializerTests.cs, SerializerTests.cs, SerializationRoundtrip.cs,
    NamingConventionTests.cs, YamlStreamTests.cs, ParserTests.cs,
    TestModels.cs

There is NO `Portability/` folder and no `CodeBrix.YamlParse.Portability`
namespace in this repository -- see NOTES.

The public surface is roughly 230 public types. The four root-namespace source
files (`CultureInfoAdapter`, `PropertyInfoExtensions`, `ReflectionExtensions`,
`StandardRegexOptions`) are all `internal`, so the bare `CodeBrix.YamlParse`
namespace exports nothing; keep it that way. `YamlAliasNode` in
`RepresentationModel/` is likewise `internal` by design -- consumers see
aliases only as shared node instances.

Folder rules: `src/`, `tests/`, `samples/`, `libs/` are lower-case and
case-sensitive. Source files live in sub-folders that match their
sub-namespaces; the port preserves upstream's file-per-type layout.


BUILDING
========

  dotnet restore CodeBrix.YamlParse.slnx
  dotnet build   CodeBrix.YamlParse.slnx

`global.json` at the repo root does NOT pin an SDK version, so the newest
installed .NET 10 SDK is still used. It exists solely to select the test
runner:

  { "test": { "runner": "Microsoft.Testing.Platform" } }

Because that setting lives in global.json rather than in the csproj, it applies
to every `dotnet test` run anywhere in the repository, including CI. Keep the
file committed -- see TESTING.

Target framework is `net10.0` only, and it stays that way -- the CodeBrix
family is .NET 10 or later, full stop; `netstandard` targets exist elsewhere in
the family only for Roslyn analyzer hosts, which this repository does not have.

The library builds with zero warnings and zero errors, and there is no
project-level `<NoWarn>`. Keep it that way: where upstream suppressed
CS8602/CS8604 with `<NoWarn>`, the port resolved them at the source with
behaviour-preserving null handling instead.

`GeneratePackageOnBuild` is true, so an ordinary `dotnet build` also produces a
.nupkg under `src/CodeBrix.YamlParse/bin/`.

There are no build scripts, no .props/.targets files and no .nuspec: the
csproj is the whole build definition.


TESTING
=======

  dotnet test CodeBrix.YamlParse.slnx

THE TEST RUNNER IS Microsoft.Testing.Platform (MTP), selected by `global.json`
at the repo root. Do not delete that file; without it, `dotnet test` falls back
to the older VSTest bridge. You can tell which one ran: MTP output ends in a
"Test run summary:" block, while the VSTest bridge invokes MSBuild with
`--target:VSTest`.

Roughly 30 tests across six test classes, plus a shared `TestModels.cs`
helper. Stack: xUnit v3 (`xunit.v3`) with `xunit.runner.visualstudio`,
`Microsoft.NET.Test.Sdk` and SilverAssertions (fluent `.Should()` form). There
is no coverage collector in the test project. This is a fresh CodeBrix-convention suite, NOT a port of
YamlDotNet's own test project -- upstream's tests depend on FakeItEasy and
FluentAssertions, neither of which is used anywhere in the CodeBrix family.

Test naming is `<Class>Tests.cs` with snake_case method names describing the
behaviour, and bodies carry `//Arrange`, `//Act`, `//Assert` comments. Two
files deliberately break the `<Class>Tests.cs` rule and say so in a header
comment: `SerializationRoundtrip.cs` (a multi-class scenario) and
`TestModels.cs` (shared POCOs).

The suite covers: serialization of scalars, dictionaries, lists and nested
objects; deserialization into typed objects, dictionaries and lists; all six
naming conventions; representation-model load/inspect/save; low-level parser
event order; and the error paths (unmatched property throws, malformed input
throws `YamlException`).

No opt-in environment variables, no special prep, no external services, no
test-data files: every test runs offline and in-process.

`src/CodeBrix.YamlParse/InternalsVisibleTo.cs` grants internals to
`CodeBrix.YamlParse.Tests`, which is the family convention: every packaging
library ships one such file pointing at its `.Tests` assembly.

Gaps worth closing if you extend the suite: `MergingParser` (merge keys),
`Emitter`/`EmitterSettings` output shape, custom `IYamlTypeConverter`
registration, `EnsureRoundtrip`, `JsonCompatible`, the buffered
type-discriminating deserializer, anchors and aliases through both the
representation model and the serializer, and comment events from a
`Scanner` constructed with `skipComments: false`.


PACKAGING AND PUBLISHING
========================

Packaging is driven entirely by the csproj -- there is no pack script, no
.nuspec and no props/targets file in this repository.

What ships in the nupkg besides the assembly:

  icon-codebrix-128.png    -> PackageIcon
  README.md                -> PackageReadmeFile
  AGENT-README.txt         -> package root
  THIRD-PARTY-NOTICES.txt  -> package root

Keep the `<None Include="..\..\AGENT-README.txt" Pack="true" ... />` item in
the csproj: shipping the consumer documentation inside the package is the whole
point of the AGENT-README convention. MAINTAINER-README.txt, EXTRAS-README.txt
and README-INDEX.txt are NOT packed.

`PackageRequireLicenseAcceptance` is true and `PackageLicenseExpression` is
MIT.

Versioning is the family date-stamped scheme, computed in the csproj from
`System.DateTime.UtcNow`: `1.<years since base year>.<day of year>.<minute of
day>`, always strictly increasing, never SemVer. `_VersionBaseYear` in the
csproj re-baselines the minor field. Two builds inside the same UTC minute
produce the same version, so do not publish twice within one minute.

Publishing rules that apply to this repository:

  * Never write a version number into AGENT-README.txt -- not even the
    upstream source version. The provenance sentence there points at
    THIRD-PARTY-NOTICES.txt, which is where the exact upstream release is
    recorded.
  * The git tag for a release must match the published nuget.org version.
  * The package id keeps its `.MitLicenseForever` suffix; the assembly and the
    namespaces never carry it.


PROVENANCE AND VENDORED SOURCES
===============================

The entire `src/CodeBrix.YamlParse/` tree is a port of YamlDotNet 18.1.0 (MIT),
taken from the `v18.1.0` git tag of https://github.com/aaubry/YamlDotNet.
YamlDotNet in turn derives its scanner/parser/emitter from the libyaml C
library (MIT). Both attributions and both licence texts are reproduced in
THIRD-PARTY-NOTICES.txt, which must be updated if any further upstream code is
taken.

Rules for ported files:

  * Preserve the upstream MIT licence header verbatim at the top of each file.
  * Each namespace declaration carries a `//was previously: YamlDotNet.<X>;`
    trailing comment. Comment out replaced lines with `//was previously:`
    rather than deleting them.
  * Upstream block-scoped namespaces were converted to file-scoped namespaces.

Deliberate divergences from upstream:

  * Target framework is `net10.0` only (upstream multi-targets
    net10.0;net8.0;netstandard2.0;netstandard2.1;net47). The upstream
    framework-selection `#if` directives were PRESERVED rather than stripped,
    so on net10.0 the compiler picks the modern branches and the legacy
    branches remain as inert, auditable dead code. Do not strip them.
  * Strong-name signing was dropped. Upstream's
    `YamlDotNet/Properties/CustomAssemblyInfo.cs` and its `AssemblyInfo.template`
    were not ported; this repository uses the CodeBrix-standard
    `InternalsVisibleTo.cs` with no public key.
  * The upstream `Nullable` polyfill PackageReference was dropped -- .NET 10
    supplies the nullable-annotation attributes in the BCL.
  * The upstream Roslyn source generator project (the AOT static-context
    generator) was NOT ported. The `StaticSerializerBuilder` /
    `StaticDeserializerBuilder` / `StaticContext` / `StaticObjectFactory`
    surface is present and usable, but a consumer must hand-write the static
    context. This is the one advertised feature gap and it is documented in
    AGENT-README.txt under WHAT THIS PACKAGE DOES NOT DO.
  * A small number of nullable-flow warnings (CS8602/CS8604) that upstream
    suppresses project-wide were resolved at the source instead.

If upstream code is ever re-taken, re-apply every item above.


CODING CONVENTIONS
==================

These are the repo-specific rules; the first two are situational exceptions to
the family defaults and are also recorded as comments in the csproj.

  EXCEPTION 1 -- `<Nullable>enable</Nullable>` is ON for this library, where
  the normal family rule leaves NRT unset. The upstream public API depends
  pervasively on nullable-reference-type (`?`) annotations; stripping them
  would change observable signatures and break fidelity with the upstream
  package. Same precedent as the CodeBrix.Platform.OpenGL port. Because NRT is
  ON here, the null-forgiving `!` operator is permitted in this repository (it
  is forbidden only in NRT-off CodeBrix repos).

  EXCEPTION 2 -- `<GenerateDocumentationFile>false</GenerateDocumentationFile>`
  for this library, where the normal family rule is `true` and requires an XML
  doc comment on every public member. The ported public surface is hundreds of
  members and is undocumented upstream. With the doc file off, CS1591 is not
  reported, so no `<NoWarn>` is needed. Many ported types DO already carry
  upstream `///` comments; new public members added later should be
  documented.

The rest are the ordinary family rules:

  * File-scoped namespaces only; no block-scoped namespaces.
  * No `global using` directives and no `<ImplicitUsings>`; usings are explicit
    and per-file, above the file-scoped namespace.
  * Top-of-file layout: licence/provenance header, then usings, then the
    file-scoped namespace with its `//was previously:` comment.
  * Target framework is `net10.0` only; no multi-targeting.
  * Tests use xUnit v3 + SilverAssertions, never xUnit v2, FluentAssertions or
    FakeItEasy.
  * No project-level warning suppression.
  * The library project root carries `InternalsVisibleTo.cs`.
  * Do not modernize ported files wholesale -- upstream's older idioms are kept
    so that future upstream diffs stay readable.
  * Identifiers inherited from upstream are never renamed.


NOTES
=====

  * THIRD-PARTY-NOTICES.txt lists `CodeBrix.YamlParse.Portability` among the
    ported namespaces, and earlier revisions of the consumer documentation
    described a `Portability/` source folder of shims. Neither exists: there is
    no `Portability` folder, namespace or identifier anywhere under `src/`.
    The portability shims that were ported live in `Helpers/Polyfills.cs`. The
    notices file was left untouched because packaging and attribution files are
    edited centrally, not per-repo -- flag it there rather than fixing it here.
  * The library has no dependencies, no native assets and no platform-specific
    code, so there is nothing to test per-OS.
  * The whole API is synchronous by design (upstream has no async surface).
    Do not add async wrappers here; a consumer can read the text
    asynchronously and hand over a string or `TextReader`.
  * Default limits worth remembering when reading bug reports: serialization
    recursion cap 50, deserialization recursion cap 130,
    `MergingParser` buffer 100,000 events, `EmitterSettings.BestIndent` 2,
    `BestWidth` `int.MaxValue`, `EmitterSettings.MaxSimpleKeyLength` 1024, and
    `Scanner`'s max key size 1024. All are configurable.
  * `TestResults/` folders under `tests/` are build output from a previous
    `dotnet test --collect` run, not repository content.
