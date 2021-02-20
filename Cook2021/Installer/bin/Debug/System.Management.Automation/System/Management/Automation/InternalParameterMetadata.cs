// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InternalParameterMetadata
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Reflection;

namespace System.Management.Automation
{
  internal class InternalParameterMetadata
  {
    [TraceSource("ParameterMetadata", "The metadata associated with a bindable object type in MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterMetadata", "The metadata associated with a bindable object type in MSH.");
    private string typeName = string.Empty;
    private Dictionary<string, CompiledCommandParameter> bindableParameters = new Dictionary<string, CompiledCommandParameter>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, CompiledCommandParameter> aliasedParameters = new Dictionary<string, CompiledCommandParameter>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Type type;
    internal static readonly BindingFlags metaDataBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

    internal static InternalParameterMetadata Get(
      RuntimeDefinedParameterDictionary runtimeDefinedParameters,
      bool processingDynamicParameters,
      bool checkNames)
    {
      if (runtimeDefinedParameters == null)
        throw InternalParameterMetadata.tracer.NewArgumentNullException("runtimeDefinedParameter");
      return new InternalParameterMetadata(runtimeDefinedParameters, processingDynamicParameters, checkNames);
    }

    internal static InternalParameterMetadata Get(
      Type type,
      ExecutionContext context,
      bool processingDynamicParameters)
    {
      if (type == null)
        throw InternalParameterMetadata.tracer.NewArgumentNullException(nameof (type));
      InternalParameterMetadata parameterMetadata;
      if (context != null && context.ParameterMetadataCache.ContainsKey(type.AssemblyQualifiedName))
      {
        parameterMetadata = context.ParameterMetadataCache[type.AssemblyQualifiedName];
        InternalParameterMetadata.tracer.WriteLine("The type metadata was found in the cache", new object[0]);
      }
      else
      {
        InternalParameterMetadata.tracer.WriteLine("The type metadata must be constructed from the type", new object[0]);
        parameterMetadata = new InternalParameterMetadata(type, processingDynamicParameters);
        context?.ParameterMetadataCache.Add(type.AssemblyQualifiedName, parameterMetadata);
      }
      return parameterMetadata;
    }

    internal InternalParameterMetadata(
      RuntimeDefinedParameterDictionary runtimeDefinedParameters,
      bool processingDynamicParameters,
      bool checkNames)
    {
      if (runtimeDefinedParameters == null)
        throw InternalParameterMetadata.tracer.NewArgumentNullException(nameof (runtimeDefinedParameters));
      this.ConstructCompiledParametersUsingRuntimeDefinedParameters(runtimeDefinedParameters, processingDynamicParameters, checkNames);
    }

    internal InternalParameterMetadata(Type type, bool processingDynamicParameters)
    {
      this.type = type != null ? type : throw InternalParameterMetadata.tracer.NewArgumentNullException(nameof (type));
      this.typeName = type.Name;
      this.ConstructCompiledParametersUsingReflection(processingDynamicParameters);
    }

    internal string TypeName => this.typeName;

    internal Dictionary<string, CompiledCommandParameter> BindableParameters => this.bindableParameters;

    internal Dictionary<string, CompiledCommandParameter> AliasedParameters => this.aliasedParameters;

    private void ConstructCompiledParametersUsingRuntimeDefinedParameters(
      RuntimeDefinedParameterDictionary runtimeDefinedParameters,
      bool processingDynamicParameters,
      bool checkNames)
    {
      foreach (RuntimeDefinedParameter runtimeDefinedParameter in runtimeDefinedParameters.Values)
      {
        if (runtimeDefinedParameter != null)
          this.AddParameter(new CompiledCommandParameter(runtimeDefinedParameter, processingDynamicParameters), checkNames);
      }
    }

    private void ConstructCompiledParametersUsingReflection(bool processingDynamicParameters)
    {
      PropertyInfo[] properties = this.type.GetProperties(InternalParameterMetadata.metaDataBindingFlags);
      FieldInfo[] fields = this.type.GetFields(InternalParameterMetadata.metaDataBindingFlags);
      foreach (PropertyInfo propertyInfo in properties)
      {
        if (InternalParameterMetadata.IsMemberAParameter((MemberInfo) propertyInfo))
          this.AddParameter((MemberInfo) propertyInfo, processingDynamicParameters);
      }
      foreach (FieldInfo fieldInfo in fields)
      {
        if (InternalParameterMetadata.IsMemberAParameter((MemberInfo) fieldInfo))
          this.AddParameter((MemberInfo) fieldInfo, processingDynamicParameters);
      }
    }

    private void CheckForReservedParameter(string name)
    {
      if (name.Equals("SelectProperty", StringComparison.OrdinalIgnoreCase) || name.Equals("SelectObject", StringComparison.OrdinalIgnoreCase))
        throw new MetadataException("ReservedParameterName", (Exception) null, "DiscoveryExceptions", "ReservedParameterName", new object[1]
        {
          (object) name
        });
    }

    private void AddParameter(MemberInfo member, bool processingDynamicParameters)
    {
      bool flag1 = false;
      bool flag2 = false;
      this.CheckForReservedParameter(member.Name);
      if (this.bindableParameters.ContainsKey(member.Name))
      {
        CompiledCommandParameter bindableParameter = this.bindableParameters[member.Name];
        Type declaringType = bindableParameter.DeclaringType;
        if (declaringType == null)
        {
          flag1 = true;
        }
        else
        {
          InternalParameterMetadata.tracer.WriteLine("Existing parameter DeclaringType = {0}", (object) declaringType.FullName);
          InternalParameterMetadata.tracer.WriteLine("New parameter DeclaringType = {0}", (object) member.DeclaringType);
          if (declaringType.IsSubclassOf(member.DeclaringType))
            flag2 = true;
          else if (member.DeclaringType.IsSubclassOf(declaringType))
            this.RemoveParameter(bindableParameter);
          else
            flag1 = true;
        }
      }
      if (flag1)
        throw new MetadataException("DuplicateParameterDefinition", (Exception) null, "ParameterBinderStrings", "DuplicateParameterDefinition", new object[1]
        {
          (object) member.Name
        });
      if (flag2)
        return;
      this.AddParameter(new CompiledCommandParameter(member, processingDynamicParameters), true);
    }

    private void AddParameter(CompiledCommandParameter parameter, bool checkNames)
    {
      if (checkNames)
        this.CheckForReservedParameter(parameter.Name);
      this.bindableParameters.Add(parameter.Name, parameter);
      foreach (string alias in parameter.Aliases)
      {
        if (this.aliasedParameters.ContainsKey(alias))
          throw new MetadataException("AliasDeclaredMultipleTimes", (Exception) null, "DiscoveryExceptions", "AliasDeclaredMultipleTimes", new object[1]
          {
            (object) alias
          });
        this.aliasedParameters.Add(alias, parameter);
      }
    }

    private void RemoveParameter(CompiledCommandParameter parameter)
    {
      this.bindableParameters.Remove(parameter.Name);
      foreach (string alias in parameter.Aliases)
        this.aliasedParameters.Remove(alias);
    }

    private static bool IsMemberAParameter(MemberInfo member)
    {
      bool flag = false;
      object[] customAttributes;
      try
      {
        customAttributes = member.GetCustomAttributes(typeof (CmdletMetadataAttribute), false);
      }
      catch (MetadataException ex)
      {
        throw new MetadataException("GetCustomAttributesMetadataException", (Exception) ex, "Metadata", "MetadataMemberInitialization", new object[2]
        {
          (object) member.Name,
          (object) ex.Message
        });
      }
      catch (ArgumentException ex)
      {
        throw new MetadataException("GetCustomAttributesArgumentException", (Exception) ex, "Metadata", "MetadataMemberInitialization", new object[2]
        {
          (object) member.Name,
          (object) ex.Message
        });
      }
      if (customAttributes != null && customAttributes.Length > 0)
        flag = true;
      InternalParameterMetadata.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }
  }
}
