// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CompiledCommandParameter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation
{
  internal class CompiledCommandParameter
  {
    [TraceSource("CompiledCommandParameter", "The metadata associated with a parameter that is attached to a bindable object in MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CompiledCommandParameter), "The metadata associated with a parameter that is attached to a bindable object in MSH.");
    private string name = string.Empty;
    private Type type;
    private string typeName = string.Empty;
    private Type declaringType;
    private bool isDynamic;
    private ParameterCollectionTypeInformation collectionTypeInformation;
    private Collection<CompiledCommandAttribute> compiledAttributes;
    private Collection<ArgumentTransformationAttribute> argumentTransformationAttributes = new Collection<ArgumentTransformationAttribute>();
    private Collection<ValidateArgumentsAttribute> validationAttributes = new Collection<ValidateArgumentsAttribute>();
    private bool allowsNullArgument;
    private bool allowsEmptyStringArgument;
    private bool allowsEmptyCollectionArgument;
    private bool isInAllSets;
    private uint _parameterSetFlags;
    private Dictionary<string, ParameterSetSpecificMetadata> _parameterSetData;
    private Collection<string> aliases = new Collection<string>();

    internal CompiledCommandParameter(
      RuntimeDefinedParameter runtimeDefinedParameter,
      bool processingDynamicParameters)
    {
      this.name = runtimeDefinedParameter != null ? runtimeDefinedParameter.Name : throw CompiledCommandParameter.tracer.NewArgumentNullException(nameof (runtimeDefinedParameter));
      this.type = runtimeDefinedParameter.ParameterType;
      this.isDynamic = processingDynamicParameters;
      this.collectionTypeInformation = new ParameterCollectionTypeInformation(runtimeDefinedParameter.ParameterType);
      this.ConstructCompiledAttributesUsingRuntimeDefinedParameter(runtimeDefinedParameter);
    }

    internal CompiledCommandParameter(MemberInfo member, bool processingDynamicParameters)
    {
      this.name = member != null ? member.Name : throw CompiledCommandParameter.tracer.NewArgumentNullException(nameof (member));
      this.declaringType = member.DeclaringType;
      this.isDynamic = processingDynamicParameters;
      if (member.MemberType == MemberTypes.Property)
        this.type = ((PropertyInfo) member).PropertyType;
      else if (member.MemberType == MemberTypes.Field)
      {
        this.type = ((FieldInfo) member).FieldType;
      }
      else
      {
        ArgumentException argumentException = (ArgumentException) CompiledCommandParameter.tracer.NewArgumentException(nameof (member), "DiscoveryExceptions", "CompiledCommandParameterMemberMustBeFieldOrProperty");
        CompiledCommandParameter.tracer.TraceException((Exception) argumentException);
        throw argumentException;
      }
      this.collectionTypeInformation = new ParameterCollectionTypeInformation(this.type);
      this.ConstructCompiledAttributesUsingReflection(member);
    }

    internal string Name => this.name;

    internal Type Type
    {
      get
      {
        if (this.type == null)
          this.type = Type.GetType(this.typeName);
        return this.type;
      }
    }

    internal Type DeclaringType => this.declaringType;

    internal bool IsDynamic => this.isDynamic;

    internal ParameterCollectionTypeInformation CollectionTypeInformation
    {
      get
      {
        if (this.collectionTypeInformation == null)
          this.collectionTypeInformation = new ParameterCollectionTypeInformation(this.Type);
        return this.collectionTypeInformation;
      }
    }

    internal Collection<CompiledCommandAttribute> CompiledAttributes
    {
      get
      {
        if (this.compiledAttributes == null)
        {
          MemberInfo[] member = this.Type.GetMember(this.Name, InternalParameterMetadata.metaDataBindingFlags);
          if (member.Length > 0)
            this.ConstructCompiledAttributesUsingReflection(member[0]);
        }
        return this.compiledAttributes;
      }
    }

    internal ReadOnlyCollection<ArgumentTransformationAttribute> ArgumentTransformationAttributes => new ReadOnlyCollection<ArgumentTransformationAttribute>((IList<ArgumentTransformationAttribute>) this.argumentTransformationAttributes);

    internal ReadOnlyCollection<ValidateArgumentsAttribute> ValidationAttributes => new ReadOnlyCollection<ValidateArgumentsAttribute>((IList<ValidateArgumentsAttribute>) this.validationAttributes);

    internal bool AllowsNullArgument => this.allowsNullArgument;

    internal bool AllowsEmptyStringArgument => this.allowsEmptyStringArgument;

    internal bool AllowsEmptyCollectionArgument => this.allowsEmptyCollectionArgument;

    internal bool IsInAllSets
    {
      get => this.isInAllSets;
      set => this.isInAllSets = value;
    }

    internal uint ParameterSetFlags
    {
      get => this._parameterSetFlags;
      set => this._parameterSetFlags = value;
    }

    internal bool DoesParameterSetTakePipelineInput(uint validParameterSetFlags)
    {
      bool flag = false;
      foreach (ParameterSetSpecificMetadata specificMetadata in this.ParameterSetData.Values)
      {
        if ((specificMetadata.IsInAllSets || ((int) specificMetadata.ParameterSetFlag & (int) validParameterSetFlags) != 0) && (specificMetadata.ValueFromPipeline || specificMetadata.ValueFromPipelineByPropertyName))
        {
          flag = true;
          break;
        }
      }
      CompiledCommandParameter.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal ParameterSetSpecificMetadata GetParameterSetData(
      uint parameterSetFlag)
    {
      ParameterSetSpecificMetadata specificMetadata1 = (ParameterSetSpecificMetadata) null;
      foreach (ParameterSetSpecificMetadata specificMetadata2 in this.ParameterSetData.Values)
      {
        if (specificMetadata2.IsInAllSets)
          specificMetadata1 = specificMetadata2;
        else if (((int) specificMetadata2.ParameterSetFlag & (int) parameterSetFlag) != 0)
        {
          specificMetadata1 = specificMetadata2;
          break;
        }
      }
      return specificMetadata1;
    }

    internal Collection<ParameterSetSpecificMetadata> GetMatchingParameterSetData(
      uint parameterSetFlags)
    {
      Collection<ParameterSetSpecificMetadata> collection = new Collection<ParameterSetSpecificMetadata>();
      foreach (ParameterSetSpecificMetadata specificMetadata in this.ParameterSetData.Values)
      {
        if (specificMetadata.IsInAllSets)
          collection.Add(specificMetadata);
        else if (((int) specificMetadata.ParameterSetFlag & (int) parameterSetFlags) != 0)
          collection.Add(specificMetadata);
      }
      return collection;
    }

    internal Dictionary<string, ParameterSetSpecificMetadata> ParameterSetData => this._parameterSetData;

    internal ReadOnlyCollection<string> Aliases => new ReadOnlyCollection<string>((IList<string>) this.aliases);

    private void ConstructCompiledAttributesUsingRuntimeDefinedParameter(
      RuntimeDefinedParameter runtimeDefinedParameter)
    {
      this.compiledAttributes = new Collection<CompiledCommandAttribute>();
      this._parameterSetData = new Dictionary<string, ParameterSetSpecificMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (Attribute attribute in runtimeDefinedParameter.Attributes)
        this.ProcessAttribute(runtimeDefinedParameter.Name, attribute);
    }

    private void ConstructCompiledAttributesUsingReflection(MemberInfo member)
    {
      this.compiledAttributes = new Collection<CompiledCommandAttribute>();
      this._parameterSetData = new Dictionary<string, ParameterSetSpecificMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      object[] customAttributes = member.GetCustomAttributes(false);
      if (customAttributes == null || customAttributes.Length <= 0)
        return;
      foreach (Attribute attribute in customAttributes)
        this.ProcessAttribute(member.Name, attribute);
    }

    private void ProcessAttribute(string memberName, Attribute attribute)
    {
      if (attribute == null)
        return;
      this.compiledAttributes.Add(new CompiledCommandAttribute(attribute));
      if (attribute is ParameterAttribute parameter)
        this.ProcessParameterAttribute(memberName, parameter);
      else if (attribute is AliasAttribute attribute1)
        this.ProcessAliasAttribute(attribute1);
      else if (attribute is ArgumentTransformationAttribute transformationAttribute)
      {
        CompiledCommandParameter.tracer.WriteLine("Adding ArgumentTransformationAttribute", new object[0]);
        this.argumentTransformationAttributes.Add(transformationAttribute);
      }
      else if (attribute is ValidateArgumentsAttribute argumentsAttribute)
      {
        CompiledCommandParameter.tracer.WriteLine("Adding ValidateArgumentsAttribute", new object[0]);
        this.validationAttributes.Add(argumentsAttribute);
      }
      else if (attribute is AllowNullAttribute)
      {
        CompiledCommandParameter.tracer.WriteLine("AllowNullAttribute found", new object[0]);
        this.allowsNullArgument = true;
      }
      else if (attribute is AllowEmptyStringAttribute)
      {
        CompiledCommandParameter.tracer.WriteLine("AllowEmptyStringAttribute found", new object[0]);
        this.allowsEmptyStringArgument = true;
      }
      else
      {
        if (!(attribute is AllowEmptyCollectionAttribute))
          return;
        CompiledCommandParameter.tracer.WriteLine("AllowEmptyCollectionAttribute found", new object[0]);
        this.allowsEmptyCollectionArgument = true;
      }
    }

    private void ProcessParameterAttribute(string parameterName, ParameterAttribute parameter)
    {
      CompiledCommandParameter.tracer.WriteLine("Parameter set name = {0}", (object) parameter.ParameterSetName);
      CompiledCommandParameter.tracer.WriteLine("Parameter position = {0}", (object) parameter.Position);
      CompiledCommandParameter.tracer.WriteLine("Parameter mandator = {0}", (object) parameter.Mandatory);
      CompiledCommandParameter.tracer.WriteLine("Parameter remaining args = {0}", (object) parameter.ValueFromRemainingArguments);
      CompiledCommandParameter.tracer.WriteLine("Parameter ValueFromPipeline = {0}", (object) parameter.ValueFromPipeline);
      CompiledCommandParameter.tracer.WriteLine("Parameter ValueFromPipelineByPropertyName = {0}", (object) parameter.ValueFromPipelineByPropertyName);
      if (this._parameterSetData.ContainsKey(parameter.ParameterSetName))
      {
        MetadataException metadataException = new MetadataException("ParameterDeclaredInParameterSetMultipleTimes", (Exception) null, "DiscoveryExceptions", "ParameterDeclaredInParameterSetMultipleTimes", new object[2]
        {
          (object) parameterName,
          (object) parameter.ParameterSetName
        });
        CompiledCommandParameter.tracer.TraceException((Exception) metadataException);
        throw metadataException;
      }
      ParameterSetSpecificMetadata specificMetadata = new ParameterSetSpecificMetadata(parameter);
      this._parameterSetData.Add(parameter.ParameterSetName, specificMetadata);
    }

    private void ProcessAliasAttribute(AliasAttribute attribute)
    {
      foreach (string aliasName in attribute.aliasNames)
      {
        CompiledCommandParameter.tracer.WriteLine("Alias = {0}", (object) aliasName);
        this.aliases.Add(aliasName);
      }
    }

    public override string ToString() => this.Name;
  }
}
