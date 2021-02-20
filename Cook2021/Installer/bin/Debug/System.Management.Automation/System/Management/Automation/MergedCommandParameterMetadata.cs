// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MergedCommandParameterMetadata
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace System.Management.Automation
{
  internal class MergedCommandParameterMetadata
  {
    [TraceSource("CommandMetadata", "The metadata associated with a bindable object in MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandMetadata", "The metadata associated with a bindable object in MSH.");
    private uint nextAvailableParameterSetIndex;
    private Collection<string> parameterSetMap = new Collection<string>();
    private string _defaultParameterSetName;
    private Dictionary<string, MergedCompiledCommandParameter> bindableParameters = new Dictionary<string, MergedCompiledCommandParameter>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, MergedCompiledCommandParameter> aliasedParameters = new Dictionary<string, MergedCompiledCommandParameter>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private CompareInfo nameCompareInfo = CompareInfo.GetCompareInfo(CultureInfo.InvariantCulture.LCID);

    internal MergedCommandParameterMetadata()
    {
    }

    internal Collection<MergedCompiledCommandParameter> ReplaceMetadata(
      MergedCommandParameterMetadata metadata)
    {
      Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
      this.bindableParameters.Clear();
      foreach (KeyValuePair<string, MergedCompiledCommandParameter> bindableParameter in metadata.BindableParameters)
      {
        this.bindableParameters.Add(bindableParameter.Key, bindableParameter.Value);
        collection.Add(bindableParameter.Value);
      }
      this.aliasedParameters.Clear();
      foreach (KeyValuePair<string, MergedCompiledCommandParameter> aliasedParameter in metadata.AliasedParameters)
        this.aliasedParameters.Add(aliasedParameter.Key, aliasedParameter.Value);
      return collection;
    }

    internal Collection<MergedCompiledCommandParameter> AddMetadataForBinder(
      InternalParameterMetadata parameterMetadata,
      ParameterBinderAssociation binderAssociation)
    {
      if (parameterMetadata == null)
        throw MergedCommandParameterMetadata.tracer.NewArgumentNullException(nameof (parameterMetadata));
      Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
      foreach (KeyValuePair<string, CompiledCommandParameter> bindableParameter in parameterMetadata.BindableParameters)
      {
        if (this.bindableParameters.ContainsKey(bindableParameter.Key))
        {
          MetadataException metadataException = new MetadataException("ParameterNameAlreadyExistsForCommand", (Exception) null, "Metadata", "ParameterNameAlreadyExistsForCommand", new object[1]
          {
            (object) bindableParameter.Key
          });
          MergedCommandParameterMetadata.tracer.TraceException((Exception) metadataException);
          throw metadataException;
        }
        if (this.aliasedParameters.ContainsKey(bindableParameter.Key))
        {
          MetadataException metadataException = new MetadataException("ParameterNameConflictsWithAlias", (Exception) null, "Metadata", "ParameterNameConflictsWithAlias", new object[2]
          {
            (object) bindableParameter.Key,
            (object) this.RetrieveParameterNameForAlias(bindableParameter.Key, this.aliasedParameters)
          });
          MergedCommandParameterMetadata.tracer.TraceException((Exception) metadataException);
          throw metadataException;
        }
        MergedCompiledCommandParameter commandParameter = new MergedCompiledCommandParameter(bindableParameter.Value, binderAssociation);
        this.bindableParameters.Add(bindableParameter.Key, commandParameter);
        collection.Add(commandParameter);
        foreach (string alias in bindableParameter.Value.Aliases)
        {
          if (this.aliasedParameters.ContainsKey(alias))
          {
            MetadataException metadataException = new MetadataException("AliasParameterNameAlreadyExistsForCommand", (Exception) null, "Metadata", "AliasParameterNameAlreadyExistsForCommand", new object[1]
            {
              (object) alias
            });
            MergedCommandParameterMetadata.tracer.TraceException((Exception) metadataException);
            throw metadataException;
          }
          if (this.bindableParameters.ContainsKey(alias))
          {
            MetadataException metadataException = new MetadataException("ParameterNameConflictsWithAlias", (Exception) null, "Metadata", "ParameterNameConflictsWithAlias", new object[2]
            {
              (object) this.RetrieveParameterNameForAlias(alias, this.bindableParameters),
              (object) bindableParameter.Value.Name
            });
            MergedCommandParameterMetadata.tracer.TraceException((Exception) metadataException);
            throw metadataException;
          }
          this.aliasedParameters.Add(alias, commandParameter);
        }
      }
      return collection;
    }

    internal int ParameterSetCount => this.parameterSetMap.Count;

    internal uint AllParameterSetFlags => (uint) ((1 << this.ParameterSetCount) - 1);

    private int AddParameterSetToMap(string parameterSetName, CompiledCommandParameter parameter)
    {
      int num = -1;
      if (!string.IsNullOrEmpty(parameterSetName))
      {
        num = this.parameterSetMap.IndexOf(parameterSetName);
        if (num == -1)
        {
          if (this.nextAvailableParameterSetIndex == uint.MaxValue)
          {
            ParsingMetadataException metadataException = new ParsingMetadataException("ParsingTooManyParameterSets", (Exception) null, "Metadata", "ParsingTooManyParameterSets", new object[0]);
            MergedCommandParameterMetadata.tracer.TraceException((Exception) metadataException);
            throw metadataException;
          }
          this.parameterSetMap.Add(parameterSetName);
          num = this.parameterSetMap.IndexOf(parameterSetName);
          ++this.nextAvailableParameterSetIndex;
        }
        MergedCommandParameterMetadata.tracer.WriteLine("ParameterSet: {0} Added At: {1}", (object) parameterSetName, (object) num);
      }
      return num;
    }

    internal uint GenerateParameterSetMappingFromMetadata(string defaultParameterSetName)
    {
      this.parameterSetMap.Clear();
      this.nextAvailableParameterSetIndex = 0U;
      uint num1 = 0;
      if (!string.IsNullOrEmpty(defaultParameterSetName))
      {
        this._defaultParameterSetName = defaultParameterSetName;
        num1 = (uint) (1 << this.AddParameterSetToMap(defaultParameterSetName, (CompiledCommandParameter) null));
      }
      foreach (MergedCompiledCommandParameter commandParameter in this.BindableParameters.Values)
      {
        uint num2 = 0;
        foreach (string key in commandParameter.Parameter.ParameterSetData.Keys)
        {
          if (string.Equals(key, "__AllParameterSets", StringComparison.OrdinalIgnoreCase))
          {
            ParameterSetSpecificMetadata specificMetadata = commandParameter.Parameter.ParameterSetData[key];
            specificMetadata.ParameterSetFlag = 0U;
            specificMetadata.IsInAllSets = true;
            commandParameter.Parameter.IsInAllSets = true;
          }
          else
          {
            uint num3 = (uint) (1 << this.AddParameterSetToMap(key, commandParameter.Parameter));
            num2 |= num3;
            commandParameter.Parameter.ParameterSetData[key].ParameterSetFlag = num3;
          }
        }
        commandParameter.Parameter.ParameterSetFlags = num2;
      }
      return num1;
    }

    internal string GetParameterSetName(uint parameterSet)
    {
      string str = this._defaultParameterSetName;
      if (string.IsNullOrEmpty(str))
        str = "__AllParameterSets";
      if (parameterSet != uint.MaxValue && parameterSet != 0U)
      {
        int index = 0;
        while (((int) (parameterSet >> index) & 1) == 0)
          ++index;
        str = ((int) (parameterSet >> index + 1) & 1) != 0 ? string.Empty : (index >= this.parameterSetMap.Count ? string.Empty : this.parameterSetMap[index]);
      }
      MergedCommandParameterMetadata.tracer.WriteLine("result = {0}", (object) str);
      return str;
    }

    private string RetrieveParameterNameForAlias(
      string key,
      Dictionary<string, MergedCompiledCommandParameter> dict)
    {
      MergedCompiledCommandParameter commandParameter = dict[key];
      if (commandParameter != null)
      {
        CompiledCommandParameter parameter = commandParameter.Parameter;
        if (parameter != null && !string.IsNullOrEmpty(parameter.Name))
          return parameter.Name;
      }
      return string.Empty;
    }

    internal MergedCompiledCommandParameter GetMatchingParameter(
      string name,
      bool throwOnParameterNotFound,
      InvocationInfo invocationInfo)
    {
      if (string.IsNullOrEmpty(name))
        throw MergedCommandParameterMetadata.tracer.NewArgumentException(nameof (name));
      Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
      if (name.Length > 0 && SpecialCharacters.IsDash(name[0]))
        name = name.Substring(1);
      foreach (string key in this.bindableParameters.Keys)
      {
        if (this.nameCompareInfo.IsPrefix(key, name, CompareOptions.IgnoreCase))
        {
          MergedCommandParameterMetadata.tracer.WriteLine("Found match: {0}", (object) key);
          if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            return this.bindableParameters[key];
          collection.Add(this.bindableParameters[key]);
        }
      }
      foreach (string key in this.aliasedParameters.Keys)
      {
        if (this.nameCompareInfo.IsPrefix(key, name, CompareOptions.IgnoreCase))
        {
          MergedCommandParameterMetadata.tracer.WriteLine("Found match: {0}", (object) key);
          if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            return this.aliasedParameters[key];
          if (!collection.Contains(this.aliasedParameters[key]))
            collection.Add(this.aliasedParameters[key]);
        }
      }
      if (collection.Count > 1)
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (MergedCompiledCommandParameter commandParameter in collection)
          stringBuilder.AppendFormat(" -{0}", (object) commandParameter.Parameter.Name);
        ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, invocationInfo, (Token) null, name, (Type) null, (Type) null, "ParameterBinderStrings", "AmbiguousParameter", new object[1]
        {
          (object) stringBuilder
        });
        MergedCommandParameterMetadata.tracer.TraceException((Exception) bindingException);
        throw bindingException;
      }
      if (collection.Count == 0 && throwOnParameterNotFound)
      {
        ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, invocationInfo, (Token) null, name, (Type) null, (Type) null, "ParameterBinderStrings", "NamedParameterNotFound", new object[0]);
        MergedCommandParameterMetadata.tracer.TraceException((Exception) bindingException);
        throw bindingException;
      }
      MergedCompiledCommandParameter commandParameter1 = (MergedCompiledCommandParameter) null;
      if (collection.Count > 0)
        commandParameter1 = collection[0];
      return commandParameter1;
    }

    internal Collection<MergedCompiledCommandParameter> GetParametersInParameterSet(
      uint parameterSetFlag)
    {
      Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
      foreach (MergedCompiledCommandParameter commandParameter in this.BindableParameters.Values)
      {
        if (((int) parameterSetFlag & (int) commandParameter.Parameter.ParameterSetFlags) != 0 || commandParameter.Parameter.IsInAllSets)
          collection.Add(commandParameter);
      }
      return collection;
    }

    internal Dictionary<string, MergedCompiledCommandParameter> BindableParameters => this.bindableParameters;

    internal Dictionary<string, MergedCompiledCommandParameter> AliasedParameters => this.aliasedParameters;
  }
}
