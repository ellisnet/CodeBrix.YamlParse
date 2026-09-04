================================================================================
AGENT-README: CodeBrix.YamlParse
A Guide for AI Coding Agents — CONSUMING the CodeBrix.YamlParse.MitLicenseForever NuGet package
================================================================================

OVERVIEW
========

CodeBrix.YamlParse is a fully managed, cross-platform YAML library for .NET 10
or later. It reads, writes, parses, emits and (de)serializes YAML, and it has
no dependencies other than .NET itself -- no native libraries, no third-party
NuGet packages, no platform-specific code.

The library is organised as three layers that can be used independently or
together:

  1. LOW-LEVEL STREAMING (namespace CodeBrix.YamlParse.Core)
     `Scanner` turns characters into tokens, `Parser` turns tokens into parsing
     events, `Emitter` turns parsing events back into text. Constant-memory,
     forward-only, no object graph is ever built. Use this when you need to
     process a huge document, transform a stream, or implement your own
     mapping.

  2. REPRESENTATION MODEL (namespace CodeBrix.YamlParse.RepresentationModel)
     A DOM-like tree -- `YamlStream` holds `YamlDocument`s, each with a
     `RootNode` that is a `YamlScalarNode`, `YamlSequenceNode` or
     `YamlMappingNode`. Analogous to `System.Xml`'s `XmlDocument`. Use this
     when you want to load, inspect, edit and save YAML whose shape you do not
     have a CLR class for.

  3. OBJECT SERIALIZATION (namespace CodeBrix.YamlParse.Serialization)
     `SerializerBuilder` / `Serializer` write CLR objects as YAML;
     `DeserializerBuilder` / `Deserializer` read YAML back into CLR objects.
     Naming conventions, attributes, custom type converters, tag mappings and
     a full pipeline of replaceable components sit in this namespace and its
     sub-namespaces. This is the layer most consumers want.

Provenance: CodeBrix.YamlParse is a faithful port of YamlDotNet (MIT), which
itself derives its scanner/parser/emitter from the libyaml C library (MIT).
The exact upstream release is recorded in THIRD-PARTY-NOTICES.txt. Type names, member names and behaviour follow the upstream library, but
the namespaces do NOT: everything lives under `CodeBrix.YamlParse.*`. Do not
write `using YamlDotNet...` -- those namespaces do not exist in this package,
and do not add the upstream package alongside this one.

Note for agents: because this is a port, it is tempting to answer questions
about it from memory of the upstream library. Do not. This file documents what
is actually in the package; where the two would disagree, this file is right.


INSTALLATION
============

  Package id:        CodeBrix.YamlParse.MitLicenseForever
  Install:           dotnet add package CodeBrix.YamlParse.MitLicenseForever
  Assembly:          CodeBrix.YamlParse.dll
  Root namespace:    CodeBrix.YamlParse
  Target framework:  .NET 10 or later
  License:           MIT
  Dependencies:      none (no NuGet dependencies, no native assets)

The package id carries the `.MitLicenseForever` suffix; the assembly and the
namespaces never do. Reference the package by the id above, then write
`using CodeBrix.YamlParse.Serialization;` -- not
`using CodeBrix.YamlParse.MitLicenseForever;`, which is not a namespace.

The library is a derivative work of YamlDotNet and, through it, libyaml; both
are MIT. The attribution and licence texts required by the MIT License ship in
THIRD-PARTY-NOTICES.txt inside the package.

Everything is pure IL and works on every platform .NET 10 runs on: Windows,
macOS, Linux, and any runtime flavour including single-file publish. See
"WHAT THIS PACKAGE DOES NOT DO" for the trimming/AOT caveat.


KEY NAMESPACES / USINGS
=======================

The three namespaces a typical consumer needs:

    using CodeBrix.YamlParse.Serialization;
        // the builders, Serializer, Deserializer, the attributes
    using CodeBrix.YamlParse.Serialization.NamingConventions;
        // CamelCase, PascalCase, Hyphenated, Underscored, LowerCase, Null
    using CodeBrix.YamlParse.RepresentationModel;
        // YamlStream, YamlDocument, YamlNode and its subclasses

The low-level layer:

    using CodeBrix.YamlParse.Core;
        // Scanner, Parser, MergingParser, Emitter, EmitterSettings,
        // AnchorName, TagName, Mark, YamlException and its subclasses
    using CodeBrix.YamlParse.Core.Events;
        // ParsingEvent and its subclasses (Scalar, MappingStart, ...)
    using CodeBrix.YamlParse.Core.Tokens;
        // Token and its subclasses -- needed only with Scanner directly

Extension points, one namespace per pipeline stage:

    ...Serialization.Callbacks              On(De)Serializ* attributes
    ...Serialization.Converters             IYamlTypeConverter implementations
    ...Serialization.EventEmitters          IEventEmitter implementations
    ...Serialization.NodeDeserializers      INodeDeserializer implementations
    ...Serialization.NodeTypeResolvers      INodeTypeResolver implementations
    ...Serialization.ObjectFactories        IObjectFactory implementations
    ...Serialization.ObjectGraphTraversalStrategies
    ...Serialization.ObjectGraphVisitors
    ...Serialization.Schemas                the standard YAML tag names
    ...Serialization.TypeInspectors         ITypeInspector implementations
    ...Serialization.TypeResolvers          ITypeResolver implementations
    ...Serialization.Utilities              SerializerState, ITypeConverter,
                                            IPostDeserializationCallback
    ...Serialization.ValueDeserializers     IValueDeserializer implementations
    ...Serialization.BufferedDeserialization
    ...Serialization.BufferedDeserialization.TypeDiscriminators
                                            polymorphic deserialization
    ...Helpers                              IOrderedDictionary, the type of
                                            YamlMappingNode.Children

(each of those is a full namespace beginning `CodeBrix.YamlParse.`)

The bare `CodeBrix.YamlParse` root namespace contains NO public types -- the
reflection helpers that live there are all internal. Never write
`using CodeBrix.YamlParse;` expecting to find API in it.


WHICH LAYER DO I USE
====================

  * "Read a config file into my settings class"            -> Deserializer
  * "Write my object out as YAML"                           -> Serializer
  * "Read YAML whose shape I do not know at compile time"   -> Deserializer with
        `Deserialize<Dictionary<string, object>>(...)`, or the representation model
  * "Edit a YAML file and write it back"                    -> YamlStream
  * "Walk a 500 MB YAML stream without loading it"          -> Parser
  * "Emit YAML by hand, event by event"                     -> Emitter
  * "Pretty-print / reformat"                               -> Parser + Emitter
  * "Support `<<` merge keys"                               -> MergingParser wrapper

The layers compose: `Deserializer.Deserialize<T>(IParser)` and
`YamlStream.Load(IParser)` both accept any `IParser`, which is how the merge-key
and multi-document patterns below work.


CORE API REFERENCE
==================

The reference is split by layer, in the order most consumers need them:

  CORE API REFERENCE -- OBJECT SERIALIZATION   the builders, Serializer,
      Deserializer, attributes, naming conventions, converters, tags,
      polymorphic deserialization and every replaceable pipeline component
  CORE API REFERENCE -- REPRESENTATION MODEL   YamlStream, YamlDocument and
      the YamlNode tree
  CORE API REFERENCE -- LOW-LEVEL STREAMING    Scanner, Parser, MergingParser,
      Emitter, the parsing events, anchors, comments, positions and errors

Signatures below are transcribed from the shipped source. Optional parameters
and their defaults are shown as written.


CORE API REFERENCE -- OBJECT SERIALIZATION
==========================================

SERIALIZING: ISerializer, Serializer, SerializerBuilder
-------------------------------------------------------

`CodeBrix.YamlParse.Serialization.ISerializer` (implemented by `Serializer`):

    string Serialize(object? graph);
    string Serialize(object? graph, Type type);
    void   Serialize(TextWriter writer, object? graph);
    void   Serialize(TextWriter writer, object? graph, Type type);
    void   Serialize(IEmitter emitter, object? graph);
    void   Serialize(IEmitter emitter, object? graph, Type type);

`Serializer` is sealed and also exposes:

    public Serializer();                                       // default configuration
    public static Serializer FromValueSerializer(IValueSerializer valueSerializer,
                                                 EmitterSettings emitterSettings);

`new Serializer()` is equivalent to `new SerializerBuilder().Build()` with all
defaults. Prefer the builder whenever you need any configuration at all.

`SerializerBuilder` is sealed, derives from `BuilderSkeleton<SerializerBuilder>`
and returns `ISerializer` from `Build()`:

    public SerializerBuilder();
    public ISerializer      Build();
    public IValueSerializer BuildValueSerializer();
    public ITypeInspector   BuildTypeInspector();

    // output shape
    public SerializerBuilder WithDefaultScalarStyle(ScalarStyle style);
    public SerializerBuilder WithQuotingNecessaryStrings(bool quoteYaml1_1Strings = false);
    public SerializerBuilder WithNewLine(string newLine);
    public SerializerBuilder WithIndentedSequences();
    public SerializerBuilder JsonCompatible();

    // value handling
    public SerializerBuilder ConfigureDefaultValuesHandling(DefaultValuesHandling configuration);
    public SerializerBuilder EmitDefaults();
        // exactly ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
    public SerializerBuilder EnsureRoundtrip();
    public SerializerBuilder DisableAliases();
    public SerializerBuilder WithMaximumRecursion(int maximumRecursion);

    // tags
    public override SerializerBuilder WithTagMapping(TagName tag, Type type);
    public SerializerBuilder WithoutTagMapping(Type type);

    // pipeline components (each has Without* counterparts and location-selector overloads)
    public SerializerBuilder WithEventEmitter<TEventEmitter>(
                                 Func<IEventEmitter, TEventEmitter> eventEmitterFactory);
    public SerializerBuilder WithEventEmitter<TEventEmitter>(
                                 Func<IEventEmitter, ITypeInspector, TEventEmitter> factory);
    public SerializerBuilder WithoutEventEmitter<TEventEmitter>();
    public SerializerBuilder WithoutEventEmitter(Type eventEmitterType);
    public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
                                 TObjectGraphVisitor objectGraphVisitor);
    public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
                                 Func<IEnumerable<IYamlTypeConverter>, TObjectGraphVisitor> factory);
    public SerializerBuilder WithoutPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>();
    public SerializerBuilder WithoutPreProcessingPhaseObjectGraphVisitor(Type objectGraphVisitorType);
    public SerializerBuilder WithEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>(
                                 Func<EmissionPhaseObjectGraphVisitorArgs, TObjectGraphVisitor> factory);
    public SerializerBuilder WithoutEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>();
    public SerializerBuilder WithoutEmissionPhaseObjectGraphVisitor(Type objectGraphVisitorType);
    public SerializerBuilder WithObjectGraphTraversalStrategyFactory(
                                 ObjectGraphTraversalStrategyFactory factory);

Semantics worth knowing:

  * `EnsureRoundtrip()` forces tags to be emitted and restricts emission to
    properties that have setters, so that the output can be read back into the
    same CLR types. It swaps the traversal strategy to
    `RoundtripObjectGraphTraversalStrategy` and adds a
    `ReadableAndWritablePropertiesTypeInspector`.
  * `DisableAliases()` makes a repeated object be emitted in full each time
    instead of as an anchor + alias. With aliases enabled (the default) the
    object graph is walked TWICE -- once to assign anchors, once to emit. With
    aliases disabled a circular reference causes a `StackOverflowException`
    rather than an alias.
  * `JsonCompatible()` makes the output valid JSON: it removes anchor names,
    uses UTF-16 surrogate pairs, lifts the simple-key length cap and swaps in
    the JSON-flavoured `GuidConverter`, `TimeSpanConverter`, `UriConverter`,
    `DateTime8601Converter`, `DateOnlyConverter`, `TimeOnlyConverter` and
    `JsonEventEmitter`.
  * `WithQuotingNecessaryStrings()` is OFF by default -- see COMMON PITFALLS.
  * The default maximum recursion for serialization is 50 levels; exceeding it
    throws `MaximumRecursionLevelReachedException`.

DESERIALIZING: IDeserializer, Deserializer, DeserializerBuilder
---------------------------------------------------------------

`CodeBrix.YamlParse.Serialization.IDeserializer` (implemented by `Deserializer`):

    T       Deserialize<T>(string input);
    T       Deserialize<T>(TextReader input);
    T       Deserialize<T>(IParser parser);
    object? Deserialize(string input);
    object? Deserialize(TextReader input);
    object? Deserialize(IParser parser);
    object? Deserialize(string input, Type type);
    object? Deserialize(TextReader input, Type type);
    object? Deserialize(IParser parser, Type type);

`Deserializer` is sealed and also exposes:

    public Deserializer();                                      // default configuration
    public static Deserializer FromValueDeserializer(IValueDeserializer valueDeserializer);

`DeserializerBuilder` is sealed, derives from
`BuilderSkeleton<DeserializerBuilder>` and returns `IDeserializer` from
`Build()`:

    public DeserializerBuilder();
    public IDeserializer      Build();
    public IValueDeserializer BuildValueDeserializer();
    public ITypeInspector     BuildTypeInspector();

    // matching rules
    public DeserializerBuilder IgnoreUnmatchedProperties();
    public DeserializerBuilder WithCaseInsensitivePropertyMatching();
    public DeserializerBuilder WithEnforceRequiredMembers();
    public DeserializerBuilder WithEnforceNullability();
    public DeserializerBuilder WithDuplicateKeyChecking();
    public DeserializerBuilder WithAttemptingUnquotedStringTypeDeserialization();
    public DeserializerBuilder WithMaximumRecursion(int maximumRecursion);

    // type selection
    public override DeserializerBuilder WithTagMapping(TagName tag, Type type);
    public DeserializerBuilder WithoutTagMapping(TagName tag);
    public DeserializerBuilder WithTypeMapping<TInterface, TConcrete>();
    public DeserializerBuilder WithTypeDiscriminatingNodeDeserializer(
                                   Action<ITypeDiscriminatingNodeDeserializerOptions> configure,
                                   int maxDepth = -1, int maxLength = -1);

    // instance creation
    public DeserializerBuilder WithObjectFactory(IObjectFactory objectFactory);
    public DeserializerBuilder WithObjectFactory(Func<Type, object> objectFactory);

    // pipeline components (each has Without* counterparts and location-selector overloads)
    public DeserializerBuilder WithNodeDeserializer(INodeDeserializer nodeDeserializer);
    public DeserializerBuilder WithoutNodeDeserializer<TNodeDeserializer>();
    public DeserializerBuilder WithoutNodeDeserializer(Type nodeDeserializerType);
    public DeserializerBuilder WithNodeTypeResolver(INodeTypeResolver nodeTypeResolver);
    public DeserializerBuilder WithoutNodeTypeResolver<TNodeTypeResolver>();
    public DeserializerBuilder WithoutNodeTypeResolver(Type nodeTypeResolverType);

Defaults that matter (all of these are OFF unless you turn them on):

  * A YAML key with no matching property THROWS. Call
    `IgnoreUnmatchedProperties()` to make it a no-op instead.
  * Property matching is case-SENSITIVE after the naming convention has been
    applied. `WithCaseInsensitivePropertyMatching()` relaxes it.
  * Duplicate keys in a mapping are accepted (last one wins) unless
    `WithDuplicateKeyChecking()` is on, which makes them throw a
    `YamlException` carrying the key's position.
  * `required` members are not enforced unless `WithEnforceRequiredMembers()`
    is on; a null value assigned to a non-nullable member is not rejected
    unless `WithEnforceNullability()` is on.
  * The default maximum recursion for deserialization is 130 levels.

Shape resolution when you deserialize into `object` (or into a
`Dictionary<string, object>` value slot): a block/flow mapping becomes
`Dictionary<object, object>`, a sequence becomes `List<object>`, and a scalar
becomes `string` -- unless `WithAttemptingUnquotedStringTypeDeserialization()`
is enabled, which tries to convert unquoted scalars to their natural CLR type
and falls back to `string`. Mapping keys are exempt from that inference and
always arrive as `string`.

For a target type that is neither a built-in primitive nor covered by a
registered converter, the scalar deserializer looks for a static
`Parse(string, IFormatProvider)` method -- so `IParsable<T>` types
(`TimeSpan`, `DateTimeOffset`, `Guid`, `IPAddress`, your own types) read from a
plain scalar without any registration. `ITypeInspector.HasParseMethod(Type)`
and `ITypeInspector.Parse(string, Type)` are that mechanism's public face.

SHARED BUILDER METHODS: BuilderSkeleton<TBuilder>
--------------------------------------------------

`SerializerBuilder` and `DeserializerBuilder` both inherit these from
`public abstract class BuilderSkeleton<TBuilder>`:

    public TBuilder IgnoreFields();
    public TBuilder IncludeNonPublicProperties();
    public TBuilder EnablePrivateConstructors();
    public TBuilder WithNamingConvention(INamingConvention namingConvention);
    public TBuilder WithEnumNamingConvention(INamingConvention enumNamingConvention);
    public TBuilder WithTypeResolver(ITypeResolver typeResolver);
    public abstract TBuilder WithTagMapping(TagName tag, Type type);
    public TBuilder WithAttributeOverride<TClass>(
                        Expression<Func<TClass, object>> propertyAccessor, Attribute attribute);
    public TBuilder WithAttributeOverride(Type type, string member, Attribute attribute);
    public TBuilder WithTypeConverter(IYamlTypeConverter typeConverter);
    public TBuilder WithoutTypeConverter<TYamlTypeConverter>();
    public TBuilder WithoutTypeConverter(Type converterType);
    public TBuilder WithTypeInspector<TTypeInspector>(
                        Func<ITypeInspector, TTypeInspector> typeInspectorFactory);
    public TBuilder WithoutTypeInspector<TTypeInspector>();
    public TBuilder WithoutTypeInspector(Type inspectorType);
    public TBuilder WithYamlFormatter(YamlFormatter formatter);

PUBLIC FIELDS ARE INCLUDED BY DEFAULT. Both builders add a
`ReadableFieldsTypeInspector` unless you call `IgnoreFields()`.

COMPONENT ORDERING: IRegistrationLocationSelectionSyntax
---------------------------------------------------------

Most `With*` methods that register a pipeline component have an overload that
takes a location selector, `Action<IRegistrationLocationSelectionSyntax<TBase>>`:

    public interface IRegistrationLocationSelectionSyntax<TBaseRegistrationType>
    {
        void InsteadOf<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;
        void Before<TRegistrationType>()    where TRegistrationType : TBaseRegistrationType;
        void After<TRegistrationType>()     where TRegistrationType : TBaseRegistrationType;
        void OnTop();
        void OnBottom();
    }

    public interface ITrackingRegistrationLocationSelectionSyntax<TBaseRegistrationType>
    {
        void InsteadOf<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;
    }

Usage: `.WithNodeDeserializer(new MyDeserializer(), s => s.Before<ObjectNodeDeserializer>())`.
Without a selector, converters and node deserializers are registered on top
(they run first). The wrapper-factory overloads take a
`WrapperFactory<TComponentBase, TComponent>` or
`WrapperFactory<TArgument, TComponentBase, TComponent>` delegate, both declared
in `CodeBrix.YamlParse.Serialization`.

NAMING CONVENTIONS
------------------

`public interface INamingConvention { string Apply(string value); string Reverse(string value); }`

Six implementations in `CodeBrix.YamlParse.Serialization.NamingConventions`, each
sealed with a public parameterless constructor and a
`public static readonly INamingConvention Instance` field:

    CamelCaseNamingConvention        MyProperty -> myProperty
    PascalCaseNamingConvention       myProperty -> MyProperty
    HyphenatedNamingConvention       MyProperty -> my-property
    UnderscoredNamingConvention      MyProperty -> my_property
    LowerCaseNamingConvention        MyProperty -> myproperty
    NullNamingConvention             MyProperty -> MyProperty  (identity; the default)

`WithNamingConvention` affects property names; `WithEnumNamingConvention`
affects enum member names, and is separate.

ATTRIBUTES
----------

In `CodeBrix.YamlParse.Serialization`:

    [AttributeUsage(...)] public sealed class YamlMemberAttribute : Attribute
    {
        public YamlMemberAttribute();
        public YamlMemberAttribute(Type serializeAs);
        public string? Description { get; set; }        // emitted as a YAML comment above the key
        public Type?   SerializeAs { get; set; }
        public int     Order { get; set; }
        public string? Alias { get; set; }              // the YAML key name to use
        public bool    ApplyNamingConventions { get; set; }
        public ScalarStyle ScalarStyle { get; set; }
        public DefaultValuesHandling DefaultValuesHandling { get; set; }
        public bool    IsDefaultValuesHandlingSpecified { get; }
    }

    public sealed class YamlIgnoreAttribute : Attribute            // skip this member entirely
    public sealed class YamlConverterAttribute : Attribute         // per-member converter
    {
        public YamlConverterAttribute(Type converterType);
        public Type ConverterType { get; }
    }
    public sealed class YamlSerializableAttribute : Attribute      // legacy IYamlSerializable opt-in
    {
        public YamlSerializableAttribute();
        public YamlSerializableAttribute(Type serializableType);
    }
    public sealed class YamlStaticContextAttribute : Attribute     // marks a generated StaticContext

In `CodeBrix.YamlParse.Serialization.Callbacks`, four parameterless marker
attributes applied to methods on the object being (de)serialized:

    OnSerializingAttribute    OnSerializedAttribute
    OnDeserializingAttribute  OnDeserializedAttribute

They are invoked through `IObjectFactory.ExecuteOnSerializing` /
`ExecuteOnSerialized` / `ExecuteOnDeserializing` / `ExecuteOnDeserialized`.
A separate hook,
`CodeBrix.YamlParse.Serialization.Utilities.IPostDeserializationCallback`, is
called after the whole graph (including forward aliases) has been resolved.

DEFAULT AND NULL VALUE HANDLING
--------------------------------

    [Flags] public enum DefaultValuesHandling
    {
        Preserve             = 0,   // emit everything (the default)
        OmitNull             = 1,
        OmitDefaults         = 2,   // default(T) or the value in [DefaultValue]
        OmitEmptyCollections = 4
    }

Set it globally with `ConfigureDefaultValuesHandling(...)`, or per member with
`[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]`.

CUSTOM TYPE CONVERTERS
----------------------

    public interface IYamlTypeConverter
    {
        bool    Accepts(Type type);
        object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer);
        void    WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);
    }

    public delegate object? ObjectDeserializer(Type type);
    public delegate void    ObjectSerializer(object? value, Type? type = null);

Register with `.WithTypeConverter(new MyConverter())` on either builder, or per
member with `[YamlConverter(typeof(MyConverter))]`.

For a converter over a scalar, derive from
`CodeBrix.YamlParse.Serialization.Converters.ScalarConverterBase<T>`, which
implements `Accepts` for you:

    public abstract class ScalarConverterBase<T> : IYamlTypeConverter
    {
        public bool Accepts(Type type);                              // type == typeof(T)
        public abstract object ReadYaml(IParser parser, Type type,
                                        ObjectDeserializer rootDeserializer);
        public abstract void   WriteYaml(IEmitter emitter, object? value, Type type,
                                         ObjectSerializer serializer);
    }

Ten converters ship in `CodeBrix.YamlParse.Serialization.Converters`:

    DateOnlyConverter(IFormatProvider? provider = null, bool doubleQuotes = false,
                      params string[] formats)
    TimeOnlyConverter(IFormatProvider? provider = null, bool doubleQuotes = false,
                      params string[] formats)
    DateTimeConverter(DateTimeKind kind = DateTimeKind.Utc, IFormatProvider? provider = null,
                      bool doubleQuotes = false, params string[] formats)
    DateTime8601Converter()  /  DateTime8601Converter(ScalarStyle scalarStyle)
    DateTimeOffsetConverter(...)
    GuidConverter(bool jsonCompatible)
    TimeSpanConverter(bool jsonCompatible = false)
    UriConverter(bool jsonCompatible = false)
    SystemTypeConverter                       // System.Type <-> assembly-qualified name
    ScalarConverterBase<T>                    // the base class above

To replace a built-in rather than add to the chain, use the location selector:
`.WithTypeConverter(new DateTimeConverter(DateTimeKind.Local), w => w.InsteadOf<DateTimeConverter>())`.

SELF-DESCRIBING TYPES: IYamlConvertible
----------------------------------------

A type can drive its own (de)serialization by implementing

    public interface IYamlConvertible
    {
        void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer);
        void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer);
    }

No registration is needed; `YamlConvertibleNodeDeserializer` and
`YamlConvertibleTypeResolver` are in both default pipelines. The older
`IYamlSerializable` (`void ReadYaml(IParser)`, `void WriteYaml(IEmitter)`) is
marked `[Obsolete("Please use IYamlConvertible instead")]` -- do not use it in
new code.

`StreamFragment` is a ready-made `IYamlConvertible` that captures a subtree as
raw events: `public sealed class StreamFragment : IYamlConvertible` with
`public IList<ParsingEvent> Events { get; }`.

ATTRIBUTES WITHOUT TOUCHING THE TYPE: YamlAttributeOverrides
-------------------------------------------------------------

    public sealed partial class YamlAttributeOverrides
    {
        public T? GetAttribute<T>(Type type, string member) where T : Attribute;
        public void Add(Type type, string member, Attribute attribute);
        public void Add<TClass>(Expression<Func<TClass, object>> propertyAccessor, Attribute attribute);
        public YamlAttributeOverrides Clone();
    }

You normally reach this through the builder rather than directly:
`.WithAttributeOverride<Person>(p => p.Name, new YamlMemberAttribute { Alias = "full-name" })`.
`YamlAttributeOverridesInspector` applies the overrides in the inspector chain.

SCALAR FORMATTING: YamlFormatter
---------------------------------

    public class YamlFormatter
    {
        public static YamlFormatter Default { get; }
        public NumberFormatInfo NumberFormat { get; set; }
        public string FormatNumber(object number);
        public string FormatNumber(double number);
        public string FormatNumber(float number);
        public string FormatBoolean(object boolean);
        public string FormatDateTime(object dateTime);
        public string FormatTimeSpan(object timeSpan);
        public virtual Func<object, ITypeInspector, INamingConvention, string> FormatEnum { get; set; }
        public virtual Func<object, bool> PotentiallyQuoteEnums { get; set; }
    }

Install with `.WithYamlFormatter(myFormatter)` on either builder.

TAGS AND SCHEMAS
----------------

    public readonly struct TagName : IEquatable<TagName>
    {
        public static readonly TagName Empty;
        public TagName(string value);
        public string Value { get; }          // throws InvalidOperationException when non-specific
        public bool IsEmpty { get; }
        public bool IsNonSpecific { get; }    // "!" or "?"
        public bool IsLocal { get; }          // starts with '!'
        public bool IsGlobal { get; }
        public static implicit operator TagName(string? value);
    }

Standard tag names are exposed as `static readonly TagName` fields on nested
`Tags` classes in `CodeBrix.YamlParse.Serialization.Schemas`:

    FailsafeSchema.Tags.Map    tag:yaml.org,2002:map
    FailsafeSchema.Tags.Seq    tag:yaml.org,2002:seq
    FailsafeSchema.Tags.Str    tag:yaml.org,2002:str
    JsonSchema.Tags.Null       tag:yaml.org,2002:null
    JsonSchema.Tags.Bool       tag:yaml.org,2002:bool
    JsonSchema.Tags.Int        tag:yaml.org,2002:int
    JsonSchema.Tags.Float      tag:yaml.org,2002:float
    CoreSchema.Tags            (inherits the JSON schema tag set)
    DefaultSchema.Tags.Timestamp  tag:yaml.org,2002:timestamp

`DeserializerBuilder` pre-registers `Map -> Dictionary<object, object>`,
`Str -> string`, `Bool -> bool`, `Float -> double`, `Int -> int` and
`Timestamp -> DateTime`. Add your own with
`.WithTagMapping("!myType", typeof(MyType))` -- the string converts implicitly
to `TagName`. On the serializer side the same call makes `MyType` be emitted
with that tag. `TagMappings` (a small `IDictionary<string, Type>` wrapper with
`Add(string tag, Type mapping)`) exists for components that want the mapping
set directly.

POLYMORPHIC DESERIALIZATION (BufferedDeserialization)
------------------------------------------------------

To pick a concrete type from the content of the mapping, buffer the node and
inspect it:

    public interface ITypeDiscriminatingNodeDeserializerOptions
    {
        void AddTypeDiscriminator(ITypeDiscriminator discriminator);
        void AddKeyValueTypeDiscriminator<T>(string discriminatorKey,
                                             IDictionary<string, Type> valueTypeMapping);
        void AddKeyValueTypeDiscriminator<T>(string discriminatorKey,
                                             params (string, Type)[] valueTypeMapping);
        void AddUniqueKeyTypeDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping);
        void AddUniqueKeyTypeDiscriminator<T>(params (string, Type)[] uniqueKeyTypeMapping);
    }

    public interface ITypeDiscriminator
    {
        Type BaseType { get; }
        bool TryDiscriminate(IParser buffer, out Type? suggestedType);
    }

Shipped implementations, both in
`CodeBrix.YamlParse.Serialization.BufferedDeserialization.TypeDiscriminators`:

    public class KeyValueTypeDiscriminator : ITypeDiscriminator
    {
        public KeyValueTypeDiscriminator(Type baseType, string targetKey,
                                         IDictionary<string, Type> typeMapping);
    }
    public class UniqueKeyTypeDiscriminator : ITypeDiscriminator
    {
        public UniqueKeyTypeDiscriminator(Type baseType, IDictionary<string, Type> typeMapping);
    }

The machinery: `TypeDiscriminatingNodeDeserializer` (an `INodeDeserializer`),
`TypeDiscriminatingNodeDeserializerOptions`, and `ParserBuffer`
(`public class ParserBuffer : IParser` with
`ParserBuffer(IParser parserToBuffer, int maxDepth, int maxLength)` and a
`Reset()` that rewinds the buffered events).

Wire it up on the builder:

    .WithTypeDiscriminatingNodeDeserializer(options =>
        options.AddKeyValueTypeDiscriminator<Shape>(
            "kind", ("circle", typeof(Circle)), ("square", typeof(Square))))

The `maxDepth`/`maxLength` arguments (both `-1` = unlimited) cap how much of the
stream is buffered.

REPLACEABLE PIPELINE COMPONENTS
--------------------------------

Every stage of both pipelines is an interface with shipped implementations you
can add to, reorder or replace. The interfaces (all in
`CodeBrix.YamlParse.Serialization`):

    public interface INodeDeserializer
    {
        bool Deserialize(IParser reader, Type expectedType,
                         Func<IParser, Type, object?> nestedObjectDeserializer,
                         out object? value, ObjectDeserializer rootDeserializer);
    }

    public interface INodeTypeResolver
    { bool Resolve(NodeEvent? nodeEvent, ref Type currentType); }

    public interface ITypeInspector
    {
        IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container);
        IPropertyDescriptor GetProperty(Type type, object? container, string name,
                                        bool ignoreUnmatched, bool caseInsensitivePropertyMatching);
        string GetEnumName(Type enumType, string name);
        string GetEnumValue(object enumValue);
        bool   HasParseMethod(Type type);
        object? Parse(string value, Type expectedType);
    }

    public interface IPropertyDescriptor
    {
        string Name { get; } bool AllowNulls { get; } bool CanWrite { get; } Type Type { get; }
        Type? TypeOverride { get; set; } int Order { get; set; }
        ScalarStyle ScalarStyle { get; set; }
        bool Required { get; } Type? ConverterType { get; }
        T? GetCustomAttribute<T>() where T : Attribute;
        IObjectDescriptor Read(object target);
        void Write(object target, object? value);
    }

    public interface IObjectDescriptor
    { object? Value { get; } Type Type { get; } Type StaticType { get; }
      ScalarStyle ScalarStyle { get; } }

    public interface IObjectFactory
    {
        object  Create(Type type);
        object? CreatePrimitive(Type type);
        bool    GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary,
                              out Type[]? genericArguments);
        Type    GetValueType(Type type);
        void    ExecuteOnDeserializing(object value);   void ExecuteOnDeserialized(object value);
        void    ExecuteOnSerializing(object value);     void ExecuteOnSerialized(object value);
    }

    public interface IEventEmitter
    {
        void Emit(AliasEventInfo eventInfo, IEmitter emitter);
        void Emit(ScalarEventInfo eventInfo, IEmitter emitter);
        void Emit(MappingStartEventInfo eventInfo, IEmitter emitter);
        void Emit(MappingEndEventInfo eventInfo, IEmitter emitter);
        void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter);
        void Emit(SequenceEndEventInfo eventInfo, IEmitter emitter);
    }

    public interface IValueSerializer
    { void SerializeValue(IEmitter emitter, object? value, Type? type); }
    public interface IValueDeserializer
    { object? DeserializeValue(IParser parser, Type expectedType, SerializerState state,
                               IValueDeserializer nestedObjectDeserializer); }
    public interface ITypeResolver       { }   // resolves the runtime type of a value
    public interface IObjectGraphVisitor<TContext> { }
    public interface IObjectGraphTraversalStrategy { }
    public interface IAliasProvider      { }
    public interface IValuePromise       { }   // forward-alias placeholder
    public interface IObjectAccessor     { }

    public delegate IObjectGraphTraversalStrategy ObjectGraphTraversalStrategyFactory(
        ITypeInspector typeInspector, ITypeResolver typeResolver,
        IEnumerable<IYamlTypeConverter> typeConverters, int maximumRecursion);

The shipped implementations, by namespace -- use these names with the
`Without*` / `InsteadOf<>` / `Before<>` / `After<>` methods:

  NodeDeserializers:      ArrayNodeDeserializer, CollectionDeserializer,
                          CollectionNodeDeserializer, DictionaryDeserializer,
                          DictionaryNodeDeserializer, EnumerableNodeDeserializer,
                          FsharpListNodeDeserializer, NullNodeDeserializer,
                          ObjectNodeDeserializer, ScalarNodeDeserializer,
                          StaticArrayNodeDeserializer, StaticCollectionNodeDeserializer,
                          StaticDictionaryNodeDeserializer, TypeConverterNodeDeserializer,
                          YamlConvertibleNodeDeserializer, YamlSerializableNodeDeserializer
  NodeTypeResolvers:      DefaultContainersNodeTypeResolver, MappingNodeTypeResolver,
                          PreventUnknownTagsNodeTypeResolver, TagNodeTypeResolver,
                          TypeNameInTagNodeTypeResolver, YamlConvertibleTypeResolver,
                          YamlSerializableTypeResolver
  TypeInspectors:         CachedTypeInspector, CompositeTypeInspector,
                          NamingConventionTypeInspector, ReadableAndWritablePropertiesTypeInspector,
                          ReadableFieldsTypeInspector, ReadablePropertiesTypeInspector,
                          ReflectionTypeInspector, TypeInspectorSkeleton,
                          WritablePropertiesTypeInspector
                          (plus YamlAttributesTypeInspector and YamlAttributeOverridesInspector,
                           which live in CodeBrix.YamlParse.Serialization itself)
  TypeResolvers:          DynamicTypeResolver (serializer default),
                          StaticTypeResolver (deserializer default)
  ObjectFactories:        DefaultObjectFactory, LambdaObjectFactory(Func<Type, object>),
                          ObjectFactoryBase (abstract base), StaticObjectFactory (abstract, AOT)
  EventEmitters:          ChainedEventEmitter, JsonEventEmitter, TypeAssigningEventEmitter,
                          WriterEventEmitter
  ObjectGraphVisitors:    AnchorAssigner, AnchorAssigningObjectGraphVisitor,
                          ChainedObjectGraphVisitor, CommentsObjectGraphVisitor,
                          CustomSerializationObjectGraphVisitor, DefaultExclusiveObjectGraphVisitor,
                          DefaultValuesObjectGraphVisitor, EmittingObjectGraphVisitor,
                          PreProcessingPhaseObjectGraphVisitorSkeleton
  ObjectGraphTraversalStrategies:
                          FullObjectGraphTraversalStrategy (default),
                          RoundtripObjectGraphTraversalStrategy (after EnsureRoundtrip)
  ValueDeserializers:     AliasValueDeserializer, MaximumRecursionValueDeserializer,
                          NodeValueDeserializer
  Utilities:              SerializerState (IDisposable; Get<T>(), Get<T>(Func<T>), OnDeserialization()),
                          IPostDeserializationCallback, ITypeConverter, TypeConverter,
                          NullTypeConverter, ReflectionTypeConverter
  Serialization root:     ObjectDescriptor, ObjectDescriptorExtensions, PropertyDescriptor,
                          OverridePropertyDescriptor, Settings (AllowPrivateConstructors),
                          EmissionPhaseObjectGraphVisitorArgs, Nothing (the empty context type),
                          LazyComponentRegistration, TrackingLazyComponentRegistration
  Event info (used by IEventEmitter):
                          EventInfo (abstract), AliasEventInfo, ObjectEventInfo, ScalarEventInfo,
                          MappingStartEventInfo, MappingEndEventInfo, SequenceStartEventInfo,
                          SequenceEndEventInfo
  Helpers:                IOrderedDictionary<TKey, TValue> (YamlMappingNode.Children),
                          ExpressionExtensions, IFsharpHelper, FsharpHelper, DefaultFsharpHelper,
                          NullFsharpHelper

AOT / SOURCE-GENERATED CONTEXT
-------------------------------

`StaticSerializerBuilder` and `StaticDeserializerBuilder` are reflection-free
counterparts driven by a `StaticContext`:

    public abstract class StaticContext
    {
        public virtual bool IsKnownType(Type type);
        public virtual ITypeResolver GetTypeResolver();
        public virtual StaticObjectFactory GetFactory();
        public virtual ITypeInspector GetTypeInspector();
    }

    public sealed class StaticSerializerBuilder
        : StaticBuilderSkeleton<StaticSerializerBuilder>
    { public StaticSerializerBuilder(StaticContext context);
      public ISerializer Build(); ... }

    public sealed class StaticDeserializerBuilder
        : StaticBuilderSkeleton<StaticDeserializerBuilder>
    { public StaticDeserializerBuilder(StaticContext context);
      public IDeserializer Build(); ... }

They carry the same `With*` surface as the reflection builders (see
`StaticBuilderSkeleton<TBuilder>`), minus `IgnoreFields`,
`IncludeNonPublicProperties`, `EnablePrivateConstructors`,
`WithAttributeOverride` and `WithObjectFactory`. To use them you must write the
`StaticContext` subclass and the `StaticObjectFactory` yourself -- this package
does NOT ship a Roslyn source generator that writes them for you (see
"WHAT THIS PACKAGE DOES NOT DO").


CORE API REFERENCE -- REPRESENTATION MODEL
==========================================

Namespace `CodeBrix.YamlParse.RepresentationModel`.

    public class YamlStream : IEnumerable<YamlDocument>
    {
        public YamlStream();
        public YamlStream(params YamlDocument[] documents);
        public YamlStream(IEnumerable<YamlDocument> documents);
        public IList<YamlDocument> Documents { get; }
        public void Add(YamlDocument document);
        public void Load(TextReader input);
        public void Load(IParser parser);
        public void Save(TextWriter output);                              // assignAnchors: true
        public void Save(TextWriter output, bool assignAnchors);
        public void Save(IEmitter emitter, bool assignAnchors);
        public void Accept(IYamlVisitor visitor);
        public IEnumerator<YamlDocument> GetEnumerator();
    }

    public class YamlDocument
    {
        public YamlDocument(YamlNode rootNode);
        public YamlDocument(string rootNode);              // a scalar-rooted document
        public YamlNode RootNode { get; }
        public IEnumerable<YamlNode> AllNodes { get; }
        public void Accept(IYamlVisitor visitor);
    }

    public abstract class YamlNode
    {
        public AnchorName Anchor { get; set; }
        public TagName Tag { get; set; }
        public Mark Start { get; }                          // position in the source, or Mark.Empty
        public Mark End { get; }
        public abstract YamlNodeType NodeType { get; }
        public IEnumerable<YamlNode> AllNodes { get; }
        public abstract void Accept(IYamlVisitor visitor);
        public YamlNode this[int index] { get; }      // sequence element only
        public YamlNode this[YamlNode key] { get; }   // mapping value only
        public static implicit operator YamlNode(string value);     // -> YamlScalarNode
        public static implicit operator YamlNode(string[] sequence);// -> YamlSequenceNode
        public static explicit operator string?(YamlNode node);     // scalar only; throws otherwise
    }

    public enum YamlNodeType { Alias, Mapping, Scalar, Sequence }

    public sealed class YamlScalarNode : YamlNode, IYamlConvertible
    {
        public YamlScalarNode();
        public YamlScalarNode(string? value);
        public string? Value { get; set; }
        public ScalarStyle Style { get; set; }
        public static explicit operator string?(YamlScalarNode value);
    }

    public sealed class YamlSequenceNode : YamlNode, IEnumerable<YamlNode>, IYamlConvertible
    {
        public YamlSequenceNode();
        public YamlSequenceNode(params YamlNode[] children);
        public YamlSequenceNode(IEnumerable<YamlNode> children);
        public IList<YamlNode> Children { get; }
        public SequenceStyle Style { get; set; }
        public void Add(YamlNode child);
        public void Add(string child);
    }

    public sealed class YamlMappingNode
        : YamlNode, IEnumerable<KeyValuePair<YamlNode, YamlNode>>, IYamlConvertible
    {
        public YamlMappingNode();
        public YamlMappingNode(params KeyValuePair<YamlNode, YamlNode>[] children);
        public YamlMappingNode(IEnumerable<KeyValuePair<YamlNode, YamlNode>> children);
        public YamlMappingNode(params YamlNode[] children);     // alternating key, value
        public YamlMappingNode(IEnumerable<YamlNode> children);      // ditto
        public IOrderedDictionary<YamlNode, YamlNode> Children { get; }
            // IDictionary<TKey, TValue> plus, from CodeBrix.YamlParse.Helpers:
            //   KeyValuePair<TKey, TValue> this[int index] { get; set; }
            //   void Insert(int index, TKey key, TValue value);
            //   void RemoveAt(int index);
        public MappingStyle Style { get; set; }
        public void Add(YamlNode key, YamlNode value);
        public void Add(string key, YamlNode value);
        public void Add(YamlNode key, string value);
        public void Add(string key, string value);
        public static YamlMappingNode FromObject(object mapping);   // anonymous obj or POCO
    }

Visiting:

    public interface IYamlVisitor { ... }
        // Visit overloads for stream / document / scalar / sequence / mapping
    public abstract class YamlVisitorBase : IYamlVisitor
    {
        public virtual void Visit(YamlStream stream);
        public virtual void Visit(YamlDocument document);
        public virtual void Visit(YamlScalarNode scalar);
        public virtual void Visit(YamlSequenceNode sequence);
        public virtual void Visit(YamlMappingNode mapping);
        protected virtual void VisitPair(YamlNode key, YamlNode value);
        protected virtual void VisitChildren(YamlStream stream);
        protected virtual void VisitChildren(YamlDocument document);
        protected virtual void VisitChildren(YamlSequenceNode sequence);
        protected virtual void VisitChildren(YamlMappingNode mapping);
    }
    public abstract class YamlVisitor : IYamlVisitor { ... }
        // the older visitor base; YamlVisitorBase is the one to derive from

Also here:

    public sealed class YamlNodeIdentityEqualityComparer : IEqualityComparer<YamlNode>
    { public bool Equals(YamlNode? x, YamlNode? y); public int GetHashCode(YamlNode obj); }

    public class LibYamlEventStream      // diagnostic: dump events in libyaml format
    { public LibYamlEventStream(IParser iParser);
      public void WriteTo(TextWriter textWriter); }

Node equality: `YamlScalarNode`, `YamlSequenceNode` and `YamlMappingNode`
override `Equals`/`GetHashCode` by VALUE. Use
`YamlNodeIdentityEqualityComparer` when you need reference identity instead
(for example, to tell two structurally identical nodes apart when tracking
anchors).

Alias handling on load: `YamlStream.Load` resolves `*aliases` to the SAME node
instance the `&anchor` produced, so the loaded tree is a graph, not a pure
tree. `Save(output)` re-assigns anchors to any node reachable more than once,
preserving existing anchor names where it can; `Save(output, assignAnchors:
false)` skips that step and will throw if the graph actually needs anchors. A
document whose root is nothing but an alias is rejected with a
`YamlException`.


CORE API REFERENCE -- LOW-LEVEL STREAMING
=========================================

Namespace `CodeBrix.YamlParse.Core`.

SCANNER (characters -> tokens)
-------------------------------

    public interface IScanner
    {
        Mark CurrentPosition { get; }
        Token? Current { get; }
        bool MoveNext();
        bool MoveNextWithoutConsuming();
        void ConsumeCurrent();
    }

    public class Scanner : IScanner
    {
        public Scanner(TextReader input, bool skipComments = true);
        public Scanner(TextReader input, bool skipComments, int maxKeySize);
        public bool SkipComments { get; }
        public Token? Current { get; }
        public Mark CurrentPosition { get; }
        public bool MoveNext();
        public bool MoveNextWithoutConsuming();
        public void ConsumeCurrent();
    }

Token types in `CodeBrix.YamlParse.Core.Tokens`, all deriving from `Token`
(which carries `Start`/`End` `Mark`s): `Anchor`, `AnchorAlias`, `BlockEnd`,
`BlockEntry`, `BlockMappingStart`, `BlockSequenceStart`, `Comment`,
`DocumentEnd`, `DocumentStart`, `Error`, `FlowEntry`, `FlowMappingEnd`,
`FlowMappingStart`, `FlowSequenceEnd`, `FlowSequenceStart`, `Key`, `Scalar`,
`StreamEnd`, `StreamStart`, `Tag`, `TagDirective`, `Value`, `VersionDirective`.

You only need the token layer if you are writing a syntax highlighter or a
linter. For everything else start at `Parser`.

PARSER (tokens -> parsing events)
----------------------------------

    public interface IParser
    {
        ParsingEvent? Current { get; }   // null before the first MoveNext, and
                                         // again after MoveNext returns false
        bool MoveNext();
    }

    public class Parser : IParser
    {
        public Parser(TextReader input);   // wraps a Scanner with skipComments: true
        public Parser(IScanner scanner);   // pass your own Scanner to keep comments
        public ParsingEvent? Current { get; }
        public bool MoveNext();
    }

    public sealed class MergingParser : IParser
    {
        public MergingParser(IParser innerParser);
        public MergingParser(IParser innerParser, int maxParsingEvents = 100_000);
        public ParsingEvent? Current { get; }
        public bool MoveNext();
    }

`MergingParser` is how you get YAML merge keys (`<<: *defaults`). It buffers
the stream, expands every merge key against its anchor, and replays the result.
Nothing else in the library expands merge keys, so if your documents use them
you must wrap: `new MergingParser(new Parser(reader))`, then hand that to
`Deserialize<T>(IParser)` or `YamlStream.Load(IParser)`.

`ParserExtensions` (a `static class` in `CodeBrix.YamlParse.Core`) is what makes
hand-written parsing bearable:

    public static T    Consume<T>(this IParser parser) where T : ParsingEvent;
    public static bool TryConsume<T>(this IParser parser, out T @event) where T : ParsingEvent;
    public static bool Accept<T>(this IParser parser, out T @event) where T : ParsingEvent;
    public static bool Accept<T>(this IParser parser) where T : ParsingEvent;
    public static T    Require<T>(this IParser parser) where T : ParsingEvent;
    public static void SkipThisAndNestedEvents(this IParser parser);
    public static bool TryFindMappingEntry(this IParser parser, Func<Scalar, bool> selector,
                                           out Scalar? key, out ParsingEvent? value);
    public static T?   Peek<T>(this IParser parser) where T : ParsingEvent;
    public static T    Expect<T>(this IParser parser) where T : ParsingEvent;
    public static T?   Allow<T>(this IParser parser) where T : ParsingEvent;
        // Expect and Allow are older spellings of Require and TryConsume

`Consume<T>` advances and throws if the next event is not a `T`; `Accept<T>`
tests without consuming; `SkipThisAndNestedEvents` skips a whole subtree.

PARSING EVENTS
--------------

Namespace `CodeBrix.YamlParse.Core.Events`. All derive from

    public abstract class ParsingEvent
    { public Mark Start { get; } public Mark End { get; }
      public virtual int NestingIncrease { get; }
      public abstract void Accept(IParsingEventVisitor visitor); }

    public abstract class NodeEvent : ParsingEvent
    { public AnchorName Anchor { get; } public TagName Tag { get; }
      public abstract bool IsCanonical { get; } }

Concrete events and their most useful constructors:

    public sealed class StreamStart : ParsingEvent   { public StreamStart(); }
    public sealed class StreamEnd   : ParsingEvent   { public StreamEnd(); }
    public sealed class DocumentStart : ParsingEvent
    {
        public DocumentStart();
        public DocumentStart(VersionDirective? version, TagDirectiveCollection? tags, bool isImplicit);
        public TagDirectiveCollection? Tags { get; }
        public VersionDirective? Version { get; }
        public bool IsImplicit { get; }
    }
    public sealed class DocumentEnd : ParsingEvent
    { public DocumentEnd(bool isImplicit); public bool IsImplicit { get; } }
    public sealed class MappingStart : NodeEvent
    {
        public MappingStart();
        public MappingStart(AnchorName anchor, TagName tag, bool isImplicit, MappingStyle style);
        public bool IsImplicit { get; } public MappingStyle Style { get; }
    }
    public class MappingEnd : ParsingEvent           { public MappingEnd(); }
    public sealed class SequenceStart : NodeEvent
    {
        public SequenceStart(AnchorName anchor, TagName tag, bool isImplicit, SequenceStyle style);
        public bool IsImplicit { get; } public SequenceStyle Style { get; }
    }
    public sealed class SequenceEnd : ParsingEvent   { public SequenceEnd(); }
    public sealed class Scalar : NodeEvent
    {
        public Scalar(string value);
        public Scalar(TagName tag, string value);
        public Scalar(AnchorName anchor, TagName tag, string value);
        public Scalar(AnchorName anchor, TagName tag, string value, ScalarStyle style,
                      bool isPlainImplicit, bool isQuotedImplicit);
        public string Value { get; } public ScalarStyle Style { get; }
        public bool IsPlainImplicit { get; } public bool IsQuotedImplicit { get; }
        public bool IsKey { get; }
    }
    public sealed class AnchorAlias : ParsingEvent
    { public AnchorAlias(AnchorName value); public AnchorName Value { get; } }
    public sealed class Comment : ParsingEvent
    {
        public Comment(string value, bool isInline);
        public Comment(string value, bool isInline, Mark start, Mark end);
        public string Value { get; } public bool IsInline { get; }
    }

    public interface IParsingEventVisitor { ... }    // double-dispatch over the events above

Styles:

    public enum ScalarStyle
    { Any, Plain, SingleQuoted, DoubleQuoted, Literal, Folded, ForcePlain }
    public enum SequenceStyle { Any, Block, Flow }
    public enum MappingStyle  { Any, Block, Flow }

EMITTER (parsing events -> text)
---------------------------------

    public interface IEmitter { void Emit(ParsingEvent @event); }

    public class Emitter : IEmitter
    {
        public Emitter(TextWriter output);
        public Emitter(TextWriter output, int bestIndent);
        public Emitter(TextWriter output, int bestIndent, int bestWidth);
        public Emitter(TextWriter output, int bestIndent, int bestWidth, bool isCanonical);
        public Emitter(TextWriter output, EmitterSettings settings);
        public void Emit(ParsingEvent @event);
    }

    public sealed class EmitterSettings
    {
        public static readonly EmitterSettings Default;
        public EmitterSettings();
        public EmitterSettings(int bestIndent, int bestWidth, bool isCanonical, int maxSimpleKeyLength,
                               bool skipAnchorName = false, bool indentSequences = false,
                               string? newLine = null, bool useUtf16SurrogatePairs = false);
        public int    BestIndent { get; }            // default 2
        public int    BestWidth { get; }             // default int.MaxValue (no wrapping)
        public string NewLine { get; }               // default Environment.NewLine
        public bool   IsCanonical { get; }
        public bool   SkipAnchorName { get; }
        public int    MaxSimpleKeyLength { get; }    // default 1024
        public bool   IndentSequences { get; }
        public bool   UseUtf16SurrogatePairs { get; }
        public EmitterSettings WithBestIndent(int bestIndent);
        public EmitterSettings WithBestWidth(int bestWidth);
        public EmitterSettings WithMaxSimpleKeyLength(int maxSimpleKeyLength);
        public EmitterSettings WithNewLine(string newLine);
        public EmitterSettings Canonical();
        public EmitterSettings WithoutAnchorName();
        public EmitterSettings WithIndentedSequences();
        public EmitterSettings WithUtf16SurrogatePairs();
    }

The `With*` methods return a NEW `EmitterSettings`; they do not mutate.

ANCHORS AND ALIASES
-------------------

    public readonly struct AnchorName : IEquatable<AnchorName>
    {
        public static readonly AnchorName Empty;
        public AnchorName(string value);
        public string Value { get; }        // throws InvalidOperationException when empty
        public bool IsEmpty { get; }
        public static implicit operator AnchorName(string? value);
    }

An anchor name may not be empty, may not contain `[`, `]`, `{`, `}` or `,`, and
is validated by the constructor. At each layer:

  * Core: `NodeEvent.Anchor` on the emitted/parsed event, `AnchorAlias` for a
    reference.
  * Representation model: `YamlNode.Anchor`, resolved to shared instances on
    load, re-assigned on save.
  * Serialization: on by default (an object appearing twice is emitted once
    with an anchor and referenced by alias); turn it off with
    `DisableAliases()`. `AnchorNotFoundException` is thrown for a dangling
    alias, `ForwardAnchorNotSupportedException` where a forward reference
    cannot be satisfied.

COMMENTS
--------

Comments are DISCARDED by default at every layer. To read them, construct the
scanner yourself with `skipComments: false` and the parser will surface
`CodeBrix.YamlParse.Core.Events.Comment` events (`Value`, `IsInline`):

    var parser = new Parser(new Scanner(reader, skipComments: false));

To WRITE a comment above a mapping key during serialization, set
`[YamlMember(Description = "...")]`; `CommentsObjectGraphVisitor` turns it into
a `Comment` event. There is no facility for round-tripping arbitrary comments
through the representation model or through deserialize-then-serialize -- see
"WHAT THIS PACKAGE DOES NOT DO".

POSITIONS AND ERRORS
--------------------

    public readonly struct Mark : IEquatable<Mark>, IComparable<Mark>, IComparable
    {
        public static readonly Mark Empty;      // index 0, line 1, column 1
        public Mark(long index, long line, long column);
        public long Index { get; }              // 0-based character offset
        public long Line { get; }               // 1-based
        public long Column { get; }             // 1-based
        // == != < <= > >= operators, CompareTo, ToString
    }

    public class YamlException : Exception
    {
        public YamlException(string message);
        public YamlException(string message, Exception inner);
        public YamlException(in Mark start, in Mark end, string message);
        public YamlException(in Mark start, in Mark end, string message, Exception? innerException);
        public Mark Start { get; }
        public Mark End { get; }
        public string ToMessage();
    }

Every failure in the library is a `YamlException` or one of these subclasses,
all in `CodeBrix.YamlParse.Core`:

    SyntaxErrorException                    sealed -- malformed YAML text
    SemanticErrorException                          -- well-formed but meaningless
                                                       (bad tag, bad directive)
    AnchorNotFoundException                         -- alias with no matching anchor
    ForwardAnchorNotSupportedException      sealed  -- alias resolved before its anchor
    MaximumRecursionLevelReachedException   sealed  -- recursion cap hit

Catch `YamlException` to catch them all, and report `Start.Line`/`Start.Column`
to the user. A deserialization failure inside a property setter or a converter
is wrapped in a `YamlException` positioned at the offending key, with the
original exception as `InnerException`.

`TagDirectiveCollection` and `Version` (with `VersionDirective` in
`CodeBrix.YamlParse.Core.Tokens`) carry `%TAG` and `%YAML` directives.
`Constants`, `Cursor`, `InsertionQueue` and `LookAheadBuffer` are public
plumbing types that consumers do not normally touch.


COMPLETE EXAMPLES
=================

Every example below is a complete, compilable file.

EXAMPLE 1 -- Deserialize a config file into a class
----------------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.IO;
    using CodeBrix.YamlParse.Serialization;
    using CodeBrix.YamlParse.Serialization.NamingConventions;

    namespace YamlDemo;

    public class ServerConfig
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool UseTls { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public static class Program
    {
        private const string Yaml = """
            host: db.example.com
            port: 5432
            useTls: true
            tags:
              - primary
              - eu-west
            """;

        public static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            ServerConfig config = deserializer.Deserialize<ServerConfig>(Yaml);

            Console.WriteLine($"{config.Host}:{config.Port} tls={config.UseTls}");
            Console.WriteLine(string.Join(", ", config.Tags));

            // From a file instead of a string:
            // using var reader = File.OpenText("config.yaml");
            // config = deserializer.Deserialize<ServerConfig>(reader);
        }
    }

EXAMPLE 2 -- Serialize a class, then read it back (round trip)
---------------------------------------------------------------

    using System;
    using System.Collections.Generic;
    using CodeBrix.YamlParse.Serialization;
    using CodeBrix.YamlParse.Serialization.NamingConventions;

    namespace YamlDemo;

    public class Catalog
    {
        [YamlMember(Alias = "catalog-title", Description = "Shown at the top of the page")]
        public string Title { get; set; } = "";

        [YamlMember(Order = 1)]
        public List<string> Items { get; set; } = new();

        [YamlIgnore]
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }

    public static class Program
    {
        public static void Main()
        {
            var catalog = new Catalog { Title = "Tablets", Items = { "clay", "wax" } };

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .WithQuotingNecessaryStrings()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            string yaml = serializer.Serialize(catalog);
            Console.WriteLine(yaml);
            // # Shown at the top of the page
            // catalog-title: Tablets
            // items:
            // - clay
            // - wax

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            Catalog again = deserializer.Deserialize<Catalog>(yaml);
            Console.WriteLine(again.Items.Count);   // 2
        }
    }

EXAMPLE 3 -- Load, edit and save with the representation model
---------------------------------------------------------------

    using System;
    using System.IO;
    using CodeBrix.YamlParse.Core.Events;      // SequenceStyle lives here
    using CodeBrix.YamlParse.RepresentationModel;

    namespace YamlDemo;

    public static class Program
    {
        public static void Main()
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(
                "city: Uruk\nriver: Euphrates\nwalls:\n  - inner\n  - outer\n"));

            var root = (YamlMappingNode)stream.Documents[0].RootNode;

            // Read a value. Children is keyed by YamlNode, and string converts implicitly.
            var city = (YamlScalarNode)root.Children["city"];
            Console.WriteLine(city.Value);                         // Uruk

            // The YamlNode indexer is the shorter form of the same lookup.
            Console.WriteLine((string?)root["river"]);             // Euphrates

            // Edit
            city.Value = "Uruk (Warka)";
            root.Add("founded", "-4000");
            ((YamlSequenceNode)root.Children["walls"]).Add("moat");

            // Style: force the sequence to flow style
            ((YamlSequenceNode)root.Children["walls"]).Style = SequenceStyle.Flow;

            // Save
            using var writer = new StringWriter();
            stream.Save(writer, assignAnchors: false);
            Console.WriteLine(writer.ToString());

            // Walk every node
            foreach (YamlNode node in stream.Documents[0].AllNodes)
            {
                Console.WriteLine($"{node.NodeType} at line {node.Start.Line}");
            }
        }
    }

Note the usings: `YamlStream` and the node types are in
`CodeBrix.YamlParse.RepresentationModel`, but the three style enums
(`ScalarStyle` in `CodeBrix.YamlParse.Core`, `SequenceStyle` and `MappingStyle`
in `CodeBrix.YamlParse.Core.Events`) are not.

EXAMPLE 4 -- Parse to events and emit them again (streaming transform)
-----------------------------------------------------------------------

    using System;
    using System.IO;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Core.Events;

    namespace YamlDemo;

    public static class Program
    {
        public static void Main()
        {
            using var reader = new StringReader("name: Inanna\ncity: Uruk\n");
            using var writer = new StringWriter();

            IParser parser = new Parser(reader);
            IEmitter emitter = new Emitter(writer, new EmitterSettings()
                .WithBestIndent(4)
                .WithIndentedSequences());

            while (parser.MoveNext())
            {
                ParsingEvent current = parser.Current!;

                // Upper-case every scalar VALUE, leaving keys alone.
                if (current is Scalar scalar && !scalar.IsKey)
                {
                    current = new Scalar(scalar.Anchor, scalar.Tag, scalar.Value.ToUpperInvariant(),
                                         scalar.Style, scalar.IsPlainImplicit, scalar.IsQuotedImplicit);
                }

                emitter.Emit(current);
            }

            Console.WriteLine(writer.ToString());
        }
    }

EXAMPLE 5 -- Emit YAML by hand, event by event
------------------------------------------------

    using System;
    using System.IO;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Core.Events;

    namespace YamlDemo;

    public static class Program
    {
        public static void Main()
        {
            using var writer = new StringWriter();
            var emitter = new Emitter(writer);

            emitter.Emit(new StreamStart());
            emitter.Emit(new DocumentStart());
            emitter.Emit(new MappingStart());

            emitter.Emit(new Scalar("name"));
            emitter.Emit(new Scalar("Inanna"));

            emitter.Emit(new Scalar("titles"));
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));
            emitter.Emit(new Scalar("Queen of Heaven"));
            emitter.Emit(new Scalar("Lady of Uruk"));
            emitter.Emit(new SequenceEnd());

            emitter.Emit(new MappingEnd());
            emitter.Emit(new DocumentEnd(isImplicit: true));
            emitter.Emit(new StreamEnd());

            Console.WriteLine(writer.ToString());
        }
    }

EXAMPLE 6 -- Multi-document streams, both ways
------------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.IO;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Core.Events;
    using CodeBrix.YamlParse.RepresentationModel;
    using CodeBrix.YamlParse.Serialization;

    namespace YamlDemo;

    public class Record { public string Name { get; set; } = ""; }

    public static class Program
    {
        private const string Multi = "---\nname: one\n---\nname: two\n---\nname: three\n";

        public static void Main()
        {
            // (a) Representation model: every document at once.
            var stream = new YamlStream();
            stream.Load(new StringReader(Multi));
            Console.WriteLine(stream.Documents.Count);            // 3

            // (b) Deserializer: one CLR object per document, streaming.
            IDeserializer deserializer = new DeserializerBuilder().Build();
            IParser parser = new Parser(new StringReader(Multi));

            parser.Consume<StreamStart>();
            var records = new List<Record>();
            while (parser.Accept<DocumentStart>())
            {
                records.Add(deserializer.Deserialize<Record>(parser));
            }
            parser.Consume<StreamEnd>();

            Console.WriteLine(records.Count);                     // 3

            // (c) Writing several documents to one stream.
            ISerializer serializer = new SerializerBuilder().Build();
            using var writer = new StringWriter();
            var emitter = new Emitter(writer);
            emitter.Emit(new StreamStart());
            foreach (Record record in records)
            {
                serializer.Serialize(emitter, record);
            }
            emitter.Emit(new StreamEnd());
            Console.WriteLine(writer.ToString());
        }
    }

Consuming `StreamStart` yourself before the loop matters:
`Deserialize<T>(IParser)` consumes a trailing `StreamEnd` only if it consumed
the `StreamStart` itself, so taking `StreamStart` first is what lets you call
it once per document.

EXAMPLE 7 -- Anchors, aliases and merge keys
----------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.IO;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Serialization;

    namespace YamlDemo;

    public class Job
    {
        public string Image { get; set; } = "";
        public int Retries { get; set; }
        public string Name { get; set; } = "";
    }

    public static class Program
    {
        private const string Yaml = """
            defaults: &defaults
              image: alpine
              retries: 3
            build:
              <<: *defaults
              name: build
            """;

        public static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            // MergingParser is REQUIRED for "<<" -- a plain Parser leaves it as a literal key.
            IParser parser = new MergingParser(new Parser(new StringReader(Yaml)));
            var all = deserializer.Deserialize<Dictionary<string, Job>>(parser);

            Job build = all["build"];
            Console.WriteLine($"{build.Name} {build.Image} {build.Retries}");   // build alpine 3

            // On the way out, a repeated object becomes an anchor + alias automatically.
            var shared = new Job { Image = "alpine", Retries = 3, Name = "shared" };
            string yaml = new SerializerBuilder().Build()
                .Serialize(new Dictionary<string, Job> { ["a"] = shared, ["b"] = shared });
            Console.WriteLine(yaml);      // b: *o0   (or similar alias)

            // ...unless you switch it off:
            string expanded = new SerializerBuilder().DisableAliases().Build()
                .Serialize(new Dictionary<string, Job> { ["a"] = shared, ["b"] = shared });
            Console.WriteLine(expanded);  // both entries written out in full
        }
    }

EXAMPLE 8 -- A custom type converter
--------------------------------------

    using System;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Core.Events;
    using CodeBrix.YamlParse.Serialization;

    namespace YamlDemo;

    public readonly record struct Rgb(byte R, byte G, byte B);

    public sealed class RgbConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Rgb);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            Scalar scalar = parser.Consume<Scalar>();
            string hex = scalar.Value.TrimStart('#');
            return new Rgb(Convert.ToByte(hex[..2], 16),
                           Convert.ToByte(hex.Substring(2, 2), 16),
                           Convert.ToByte(hex.Substring(4, 2), 16));
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var rgb = (Rgb)value!;
            emitter.Emit(new Scalar($"#{rgb.R:x2}{rgb.G:x2}{rgb.B:x2}"));
        }
    }

    public class Theme { public Rgb Accent { get; set; } }

    public static class Program
    {
        public static void Main()
        {
            var serializer = new SerializerBuilder().WithTypeConverter(new RgbConverter()).Build();
            var deserializer = new DeserializerBuilder().WithTypeConverter(new RgbConverter()).Build();

            string yaml = serializer.Serialize(new Theme { Accent = new Rgb(0x33, 0x66, 0x99) });
            Console.WriteLine(yaml);                          // Accent: '#336699'

            Theme back = deserializer.Deserialize<Theme>(yaml);
            Console.WriteLine(back.Accent);                   // Rgb { R = 51, G = 102, B = 153 }
        }
    }

EXAMPLE 9 -- Polymorphic deserialization
------------------------------------------

    using System;
    using System.Collections.Generic;
    using CodeBrix.YamlParse.Serialization;

    namespace YamlDemo;

    public abstract class Shape { public string Kind { get; set; } = ""; }
    public sealed class Circle : Shape { public double Radius { get; set; } }
    public sealed class Square : Shape { public double Side { get; set; } }

    public static class Program
    {
        private const string Yaml = """
            - kind: circle
              radius: 2.5
            - kind: square
              side: 4
            """;

        public static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .WithTypeDiscriminatingNodeDeserializer(options =>
                    options.AddKeyValueTypeDiscriminator<Shape>(
                        "kind",
                        ("circle", typeof(Circle)),
                        ("square", typeof(Square))))
                .Build();

            var shapes = deserializer.Deserialize<List<Shape>>(Yaml);
            foreach (Shape shape in shapes)
            {
                Console.WriteLine(shape.GetType().Name);      // Circle, Square
            }
        }
    }

EXAMPLE 10 -- Error handling with positions
--------------------------------------------

    using System;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Serialization;

    namespace YamlDemo;

    public class Settings { public int Retries { get; set; } }

    public static class Program
    {
        public static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();

            TryLoad(deserializer, "retries: [1, 2");        // SyntaxErrorException
            TryLoad(deserializer, "retries: many");         // YamlException wrapping a FormatException
            TryLoad(deserializer, "retreis: 3");            // YamlException: property not found
            TryLoad(deserializer, "value: *missing");       // AnchorNotFoundException
        }

        private static void TryLoad(IDeserializer deserializer, string yaml)
        {
            try
            {
                Settings settings = deserializer.Deserialize<Settings>(yaml);
                Console.WriteLine($"ok: {settings.Retries}");
            }
            catch (SyntaxErrorException ex)
            {
                Console.WriteLine($"malformed YAML at line {ex.Start.Line}, "
                                  + $"col {ex.Start.Column}: {ex.Message}");
            }
            catch (YamlException ex)     // base class: catches every failure this library raises
            {
                Console.WriteLine($"line {ex.Start.Line}, col {ex.Start.Column}: {ex.Message}");
                if (ex.InnerException is not null)
                {
                    Console.WriteLine($"  caused by {ex.InnerException.GetType().Name}");
                }
            }
        }
    }

Order the catch blocks most-derived first: `SyntaxErrorException`,
`SemanticErrorException`, `AnchorNotFoundException`,
`ForwardAnchorNotSupportedException` and
`MaximumRecursionLevelReachedException` all derive from `YamlException`.


MINIMUM VIABLE PROJECT
======================

Two files. `YamlDemo.csproj`:

    <Project Sdk="Microsoft.NET.Sdk">

      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
      </PropertyGroup>

      <ItemGroup>
        <PackageReference Include="CodeBrix.YamlParse.MitLicenseForever" Version="*" />
      </ItemGroup>

    </Project>

(Replace `*` with the version you intend to pin.)

`Program.cs`:

    using System;
    using CodeBrix.YamlParse.Serialization;
    using CodeBrix.YamlParse.Serialization.NamingConventions;

    namespace YamlDemo;

    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    public static class Program
    {
        public static void Main()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Person person = deserializer.Deserialize<Person>("name: Inanna\nage: 5000\n");
            Console.WriteLine($"{person.Name} is {person.Age}");

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Console.Write(serializer.Serialize(person));
        }
    }

    // dotnet run
    // Inanna is 5000
    // name: Inanna
    // age: 5000


PERFORMANCE TIPS
================

  * BUILD ONCE, REUSE. `SerializerBuilder.Build()` and
    `DeserializerBuilder.Build()` construct the whole component chain and a
    `CachedTypeInspector`. Building a serializer per call throws that cache
    away every time. Hold the `ISerializer` / `IDeserializer` in a static
    readonly field. Both are safe to reuse across calls; treat them as
    immutable once built, and do not mutate the builder afterwards.

  * PREFER THE `TextReader` / `TextWriter` OVERLOADS FOR FILES.
    `Deserialize<T>(string)` materialises the whole document as a string first;
    `Deserialize<T>(File.OpenText(path))` streams it.

  * FOR LARGE DOCUMENTS, SKIP THE OBJECT MODEL. `Parser` + `Emitter` work in
    constant memory. `YamlStream` and the serializer both build a full graph.
    `ParserExtensions.SkipThisAndNestedEvents` lets you jump over subtrees you
    do not care about, and `TryFindMappingEntry` lets you pull a single key out
    of a big mapping without materialising the rest.

  * `DisableAliases()` MAKES SERIALIZATION FASTER when you know the graph has
    no shared references: with aliases on, the graph is traversed twice (once
    by `AnchorAssigner` in the pre-processing phase, once to emit). The
    trade-off is that a circular reference then becomes a
    `StackOverflowException` instead of an alias.

  * `IgnoreFields()` shortens the inspector chain by removing
    `ReadableFieldsTypeInspector`, and is usually what you want for
    property-based DTOs anyway.

  * KEEP THE BUFFER BOUNDED FOR POLYMORPHIC READS.
    `WithTypeDiscriminatingNodeDeserializer(..., maxDepth, maxLength)` defaults
    to unlimited buffering. On untrusted or very large input, set both.

  * BOUND RECURSION ON UNTRUSTED INPUT. The defaults (50 levels serializing,
    130 deserializing) already stop runaway nesting with
    `MaximumRecursionLevelReachedException`; lower them with
    `WithMaximumRecursion(n)` if you have a tighter budget.

  * `Emitter`'s `BestWidth` defaults to `int.MaxValue`, i.e. no line wrapping.
    Leave it there unless you need wrapped output -- wrapping costs analysis
    per scalar.

  * `MergingParser` buffers the ENTIRE stream (capped by `maxParsingEvents`,
    default 100,000). Only wrap it around input that actually uses merge keys.


COMMON PITFALLS TO AVOID
========================

  * UNMATCHED PROPERTIES THROW BY DEFAULT. A key in the YAML with no matching
    member raises a `YamlException`. This surprises people coming from JSON
    serializers, which usually ignore extras. Call `IgnoreUnmatchedProperties()`
    if forward-compatible config files matter to you.

  * PROPERTY MATCHING IS CASE-SENSITIVE. `Name` in YAML does not bind to
    `name` unless you set a naming convention or
    `WithCaseInsensitivePropertyMatching()`. The naming convention must match
    on BOTH the serializer and the deserializer, or a round trip fails.

  * PUBLIC FIELDS ARE INCLUDED. Both builders read fields as well as
    properties unless you call `IgnoreFields()`. A public field you thought was
    private implementation detail will appear in your output.

  * STRINGS THAT LOOK LIKE OTHER TYPES ARE EMITTED UNQUOTED BY DEFAULT.
    A `string` whose value is `"true"`, `"123"` or `"null"` is written as a
    plain scalar and reads back as a bool / int / null. Call
    `WithQuotingNecessaryStrings()` on the serializer (and
    `WithQuotingNecessaryStrings(quoteYaml1_1Strings: true)` if consumers might
    apply YAML 1.1 rules, where `Yes`, `No`, `On`, `Off` and base-60 numbers
    are also special).

  * `YamlMappingNode.Children` IS KEYED BY `YamlNode`, NOT BY `string`.
    `root.Children["city"]` works only because `string` converts implicitly to
    `YamlScalarNode`. A missing key throws `KeyNotFoundException`, NOT a
    `YamlException`, so check with `Children.ContainsKey("city")` or
    `TryGetValue` first.

  * `Children` HAS TWO INDEXERS AND AN `int` PICKS BY POSITION.
    `IOrderedDictionary<YamlNode, YamlNode>` adds
    `KeyValuePair<TKey, TValue> this[int index]` on top of the key indexer, so
    `mapping.Children[0]` returns the FIRST ENTRY of the mapping, not the value
    stored under the key `0`. To look up a numeric key you must say
    `mapping.Children[new YamlScalarNode("0")]`.

  * NODE EQUALITY IS BY VALUE. Two `YamlScalarNode`s with the same value are
    `Equals`. If you are keeping a set of nodes by identity, use
    `YamlNodeIdentityEqualityComparer`.

  * MERGE KEYS (`<<`) ARE NOT EXPANDED unless you wrap the parser in
    `MergingParser`. Without it, `<<` is just an ordinary key whose value is an
    alias, and deserialization into a typed class fails with "property `<<` not
    found".

  * COMMENTS ARE DISCARDED. `new Parser(reader)` builds a `Scanner` with
    `skipComments: true`. To see `Comment` events you must construct
    `new Parser(new Scanner(reader, skipComments: false))`. Deserialize-then-
    serialize never preserves comments.

  * DUPLICATE KEYS ARE SILENTLY ACCEPTED (last wins) unless
    `WithDuplicateKeyChecking()` is enabled.

  * DESERIALIZING INTO `object` GIVES `Dictionary<object, object>`, NOT
    `Dictionary<string, object>`. Cast keys accordingly, or ask for
    `Dictionary<string, object>` explicitly at the top level -- the nested
    values are still `Dictionary<object, object>` / `List<object>` / `string`.

  * SCALARS COME BACK AS `string` WHEN THE TARGET IS `object`. Enable
    `WithAttemptingUnquotedStringTypeDeserialization()` if you want numbers and
    booleans inferred -- and note that even then MAPPING KEYS stay `string`:
    the inference is applied only to scalars that are not keys.

  * A ROUND TRIP THROUGH AN INTERFACE OR BASE-CLASS PROPERTY LOSES THE TYPE
    unless you use `EnsureRoundtrip()` (emit tags), a tag mapping, or a type
    discriminator. `EnsureRoundtrip()` also stops emitting read-only
    properties, which changes your output.

  * `DisableAliases()` PLUS A CIRCULAR REFERENCE IS A `StackOverflowException`,
    which no `catch` will save you from. Leave aliases on for graphs that might
    contain cycles.

  * `TagName.Value` AND `AnchorName.Value` THROW on an empty/non-specific
    instance. Check `IsEmpty` (and `IsNonSpecific` for tags) first.

  * `(string?)node` THROWS `ArgumentException` for a non-scalar node, and
    `node[0]` / `node["key"]` throw `ArgumentException` when the node is not a
    sequence / mapping respectively. Check `NodeType` or pattern-match first.

  * DO NOT MIX IN THE UPSTREAM PACKAGE. If a project references both this
    package and YamlDotNet, the types are distinct and incompatible even though
    they have the same names; an `IYamlTypeConverter` written against one will
    not register with the other.

  * THE `.MitLicenseForever` SUFFIX IS PART OF THE PACKAGE ID ONLY. It is never
    part of a namespace or an assembly name.


WHAT THIS PACKAGE DOES NOT DO
=============================

  * NO ROSLYN SOURCE GENERATOR FOR AOT. `StaticSerializerBuilder`,
    `StaticDeserializerBuilder`, `StaticContext` and `StaticObjectFactory` are
    present, but the compile-time generator that would write the
    `StaticContext` for your types is not part of this package. Reflection-free
    use means writing that context by hand.

  * NOT TRIM/AOT-SAFE OUT OF THE BOX. The default pipelines are reflection-
    based (`DynamicTypeResolver`, `ReflectionTypeInspector`,
    `DefaultObjectFactory`, `Activator.CreateInstance`). Under
    `PublishTrimmed`/`PublishAot` you must either preserve your model types or
    go the static-context route.

  * NO COMMENT ROUND-TRIPPING. Comments can be read as events and written from
    `[YamlMember(Description = ...)]`, but no layer preserves the comments of a
    document you loaded when you save it again.

  * NO FORMATTING PRESERVATION. Load-and-save through `YamlStream` re-emits
    from the node tree; original quoting, key order within flow collections,
    blank lines and line breaks are regenerated, not preserved.

  * NO SCHEMA VALIDATION. There is no JSON-Schema-style validator; the
    `Schemas` namespace only supplies the standard tag names.

  * NO ASYNC API. Every entry point is synchronous. Read the text with async
    I/O yourself and hand the resulting string or `TextReader` to the library.

  * NO JSON PARSER. `JsonCompatible()` makes the OUTPUT valid JSON; it does not
    add a JSON reader. (YAML 1.2 is a superset of JSON, so the parser accepts
    JSON input, but nothing here is optimised for it.)

  * NO XML, TOML, INI OR PROPERTIES SUPPORT. YAML only.

  * NO THREAD AFFINITY GUARANTEES ON THE BUILDERS. A built `ISerializer` /
    `IDeserializer` is fine to share; a `SerializerBuilder` /
    `DeserializerBuilder` being configured is not, and neither is a single
    `Parser`, `Scanner`, `Emitter` or `YamlStream` instance.

  * NO ENCODING DETECTION. The library works on `TextReader` / `TextWriter`;
    choosing an encoding and handling a BOM is the caller's job.

  * NO YAML 1.1 SEMANTICS BY DEFAULT. `Yes`/`No`/`On`/`Off` are strings, not
    booleans; `WithQuotingNecessaryStrings(quoteYaml1_1Strings: true)` only
    affects how such strings are QUOTED on output, it does not make the reader
    interpret them as booleans.


WORKING EXAMPLES ON GITHUB
==========================

The test suite is small, readable and is the executable specification for the
headline paths above:

  https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests

  DeserializerTests.cs
      `Deserialize<T>` into a typed object; camelCase key mapping;
      `Dictionary<string, string>` and `List<string>` targets; the
      unmatched-property throw and the `IgnoreUnmatchedProperties()` escape;
      malformed flow sequence raising a `YamlException`.
  SerializerTests.cs
      Scalar properties; camelCase output; dictionaries; block sequences;
      nested objects; serializing `null`.
  SerializationRoundtrip.cs
      Serialize-then-deserialize for a flat object, a nested object with a
      naming convention, and a dictionary.
  NamingConventionTests.cs
      `Apply` behaviour for all six `INamingConvention` implementations.
  YamlStreamTests.cs
      `YamlStream.Load` for a single document, mapping value lookup through
      `Children[new YamlScalarNode(key)]`, sequence ordering, `Save` producing
      re-loadable YAML, and `YamlScalarNode` value round-tripping.
  ParserTests.cs
      Event order from `Parser.MoveNext` (`StreamStart` first, `MappingStart`
      for a block mapping, `Scalar` for keys and values) and the exception on
      malformed input.
  TestModels.cs
      The two POCOs (`Person`, `Catalog`) the other files share.

The package README.md carries three shorter snippets: serialize an anonymous
object, deserialize into a dictionary, and walk a sequence with the
representation model.


QUICK REFERENCE CARD
====================

    // ---- serialize / deserialize -------------------------------------------
    ISerializer   ser = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    IDeserializer des = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    string yaml = ser.Serialize(obj);        ser.Serialize(textWriter, obj);
    T      obj  = des.Deserialize<T>(yaml);  des.Deserialize<T>(textReader);
                                             des.Deserialize<T>(parser);

    // ---- most-used builder switches ----------------------------------------
    SerializerBuilder:   WithNamingConvention  WithEnumNamingConvention  WithQuotingNecessaryStrings
                         WithDefaultScalarStyle  WithIndentedSequences  WithNewLine  JsonCompatible
                         ConfigureDefaultValuesHandling  EmitDefaults  EnsureRoundtrip  DisableAliases
                         WithTagMapping  WithTypeConverter  WithYamlFormatter  WithMaximumRecursion
                         IgnoreFields  IncludeNonPublicProperties
    DeserializerBuilder: WithNamingConvention  IgnoreUnmatchedProperties  WithCaseInsensitivePropertyMatching
                         WithDuplicateKeyChecking  WithEnforceRequiredMembers  WithEnforceNullability
                         WithAttemptingUnquotedStringTypeDeserialization
                         WithTypeMapping<TInterface, TConcrete>
                         WithTagMapping  WithTypeConverter  WithObjectFactory  WithMaximumRecursion
                         WithTypeDiscriminatingNodeDeserializer  EnablePrivateConstructors  IgnoreFields

    // ---- attributes ---------------------------------------------------------
    [YamlMember(Alias = "key", Order = 1, Description = "comment", ScalarStyle = ScalarStyle.DoubleQuoted,
                DefaultValuesHandling = DefaultValuesHandling.OmitNull, ApplyNamingConventions = false)]
    [YamlIgnore]  [YamlConverter(typeof(MyConverter))]
    [OnSerializing] [OnSerialized] [OnDeserializing] [OnDeserialized]      // ...Serialization.Callbacks

    // ---- naming conventions (each has a static .Instance) -------------------
    CamelCase  PascalCase  Hyphenated  Underscored  LowerCase  Null       // ...NamingConvention

    // ---- representation model -----------------------------------------------
    var s = new YamlStream(); s.Load(textReader); s.Save(textWriter, assignAnchors: true);
    YamlNode root = s.Documents[0].RootNode;                      // .NodeType, .Anchor, .Tag, .Start, .End
    var map = (YamlMappingNode)root;  map.Children["key"];  map.Add("k", "v");
                                      map.Style = MappingStyle.Flow;
    var seq = (YamlSequenceNode)root; seq.Children[0];      seq.Add("item");
    var sca = (YamlScalarNode)root;   sca.Value;            sca.Style = ScalarStyle.DoubleQuoted;
    YamlMappingNode.FromObject(anonymousObject);      foreach (var n in doc.AllNodes) { ... }

    // ---- low level -----------------------------------------------------------
    IParser  p = new Parser(reader);                       // comments skipped
    IParser  p = new Parser(new Scanner(reader, skipComments: false));   // comments kept
    IParser  p = new MergingParser(new Parser(reader));    // "<<" merge keys expanded
    IEmitter e = new Emitter(writer, new EmitterSettings().WithBestIndent(4).WithIndentedSequences());
    while (p.MoveNext()) { e.Emit(p.Current!); }
    p.Consume<StreamStart>();  p.Accept<DocumentStart>();  p.TryConsume<Scalar>(out var sc);
    p.SkipThisAndNestedEvents();

    // ---- events ---------------------------------------------------------------
    StreamStart StreamEnd DocumentStart DocumentEnd MappingStart MappingEnd
    SequenceStart SequenceEnd Scalar AnchorAlias Comment
    ScalarStyle{Any,Plain,SingleQuoted,DoubleQuoted,Literal,Folded,ForcePlain}
    SequenceStyle{Any,Block,Flow}   MappingStyle{Any,Block,Flow}
    YamlNodeType{Alias,Mapping,Scalar,Sequence}

    // ---- errors ---------------------------------------------------------------
    catch (SyntaxErrorException)                    // malformed text
    catch (SemanticErrorException)                  // bad tag / directive
    catch (AnchorNotFoundException)                 // dangling *alias
    catch (ForwardAnchorNotSupportedException)      // alias before its anchor
    catch (MaximumRecursionLevelReachedException)   // nesting cap
    catch (YamlException ex)                        // base of all of the above; ex.Start/.End are Marks

================================================================================
END OF AGENT-README
================================================================================
