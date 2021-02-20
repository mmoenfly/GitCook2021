// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandParameterSetInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
  public class CommandParameterSetInfo
  {
    [TraceSource("CmdletInfo", "The command information for MSH cmdlets that are directly executable by MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CmdletInfo", "The command information for MSH cmdlets that are directly executable by MSH.");
    private string name = string.Empty;
    private bool isDefault = true;
    private ReadOnlyCollection<CommandParameterInfo> parameters;

    internal CommandParameterSetInfo(
      string name,
      bool isDefaultParameterSet,
      uint parameterSetFlag,
      MergedCommandParameterMetadata parameterMetadata)
    {
      if (string.IsNullOrEmpty(name))
        throw CommandParameterSetInfo.tracer.NewArgumentException(nameof (name));
      if (parameterMetadata == null)
        throw CommandParameterSetInfo.tracer.NewArgumentNullException(nameof (parameterMetadata));
      this.name = name;
      this.isDefault = isDefaultParameterSet;
      this.Initialize(parameterMetadata, parameterSetFlag);
    }

    public string Name => this.name;

    public bool IsDefault => this.isDefault;

    public ReadOnlyCollection<CommandParameterInfo> Parameters => this.parameters;

    public override string ToString()
    {
      StringBuilder result = new StringBuilder();
      List<CommandParameterInfo> commandParameterInfoList1 = new List<CommandParameterInfo>();
      List<CommandParameterInfo> commandParameterInfoList2 = new List<CommandParameterInfo>();
      foreach (CommandParameterInfo parameter in this.parameters)
      {
        if (parameter.Position == int.MinValue)
        {
          commandParameterInfoList2.Add(parameter);
        }
        else
        {
          if (parameter.Position >= commandParameterInfoList1.Count)
          {
            for (int count = commandParameterInfoList1.Count; count <= parameter.Position; ++count)
              commandParameterInfoList1.Add((CommandParameterInfo) null);
          }
          commandParameterInfoList1[parameter.Position] = parameter;
        }
      }
      foreach (CommandParameterInfo parameter in commandParameterInfoList1)
      {
        if (parameter != null)
          CommandParameterSetInfo.AppendFormatCommandParameterInfo(parameter, ref result);
      }
      foreach (CommandParameterInfo parameter in commandParameterInfoList2)
      {
        if (parameter != null)
          CommandParameterSetInfo.AppendFormatCommandParameterInfo(parameter, ref result);
      }
      CommandParameterSetInfo.tracer.WriteLine("ToString = {0}", (object) result.ToString());
      return result.ToString();
    }

    private static void AppendFormatCommandParameterInfo(
      CommandParameterInfo parameter,
      ref StringBuilder result)
    {
      if (result.Length > 0)
        result.Append(" ");
      if (parameter.ParameterType == typeof (bool) || parameter.ParameterType == typeof (SwitchParameter))
      {
        if (parameter.IsMandatory)
          result.AppendFormat("-{0}", (object) parameter.Name);
        else
          result.AppendFormat("[-{0}]", (object) parameter.Name);
      }
      else if (parameter.IsMandatory)
      {
        if (parameter.Position != int.MinValue)
          result.AppendFormat("[-{0}] <{1}>", (object) parameter.Name, (object) parameter.ParameterType.Name.ToString());
        else
          result.AppendFormat("-{0} <{1}>", (object) parameter.Name, (object) parameter.ParameterType.Name.ToString());
      }
      else if (parameter.Position != int.MinValue)
        result.AppendFormat("[[-{0}] <{1}>]", (object) parameter.Name, (object) parameter.ParameterType.Name.ToString());
      else
        result.AppendFormat("[-{0} <{1}>]", (object) parameter.Name, (object) parameter.ParameterType.Name.ToString());
    }

    private void Initialize(MergedCommandParameterMetadata parameterMetadata, uint parameterSetFlag)
    {
      Collection<CommandParameterInfo> collection = new Collection<CommandParameterInfo>();
      foreach (MergedCompiledCommandParameter parametersInParameter in parameterMetadata.GetParametersInParameterSet(parameterSetFlag))
      {
        if (parametersInParameter != null)
          collection.Add(new CommandParameterInfo(parametersInParameter.Parameter, parameterSetFlag));
      }
      this.parameters = new ReadOnlyCollection<CommandParameterInfo>((IList<CommandParameterInfo>) collection);
    }
  }
}
