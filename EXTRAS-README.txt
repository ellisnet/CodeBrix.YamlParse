================================================================================
EXTRAS-README: CodeBrix.YamlParse
Samples, tools and other content in this repository that is not part of a NuGet package
================================================================================

This repository ships no samples, no demo applications and no tools. It
contains exactly one packable project and one test project.


THE TEST PROJECT
================

  Path:  tests/CodeBrix.YamlParse.Tests/

The unit-test project is the only non-package content in the repository. It is
not packed and not published; it exists to lock down the library's behaviour,
and it doubles as the worked-example set that AGENT-README.txt points consumers
at under "WORKING EXAMPLES ON GITHUB".

  dotnet test CodeBrix.YamlParse.slnx

Six test classes plus a shared model file:

  DeserializerTests.cs      typed objects, camelCase key mapping, dictionary
                            and list targets, the unmatched-property throw and
                            the IgnoreUnmatchedProperties escape, malformed
                            input
  SerializerTests.cs        scalars, camelCase output, dictionaries, block
                            sequences, nested objects, a null graph
  SerializationRoundtrip.cs serialize-then-deserialize for a flat object, a
                            nested object with a naming convention, and a
                            dictionary
  NamingConventionTests.cs  all six INamingConvention implementations
  YamlStreamTests.cs        representation-model load, mapping and sequence
                            access, save, scalar round-trip
  ParserTests.cs            low-level parsing event order and error path
  TestModels.cs             the shared Person and Catalog POCOs

The suite needs no test-data files, no environment variables and no network
access. Everything runs offline and in-process, and nothing under the
repository is written to while it runs.

See MAINTAINER-README.txt for the test framework, naming conventions and the
list of areas the suite does not yet cover.


WHAT IS NOT IN THIS REPOSITORY
==============================

No `samples/`, `libs/` or `tools/` folder, no benchmark project, no optional
test-data set, and no scripts. `bin/`, `obj/` and `tests/*/TestResults/`
folders that appear after a build or a coverage run are ignored build output,
not repository content.
