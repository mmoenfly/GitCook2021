// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandMetadata
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  public sealed class CommandMetadata
  {
    internal const string isSafeNameOrIdentifierRegex = "^[-._:\\\\\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Nd}\\p{Lm}]{1,100}$";
    [TraceSource("CommandMetadata", "The metadata associated with a cmdlet.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandMetadata), "The metadata associated with a cmdlet.");
    private string commandName = string.Empty;
    private Type type;
    private string defaultParameterSetName = "__AllParameterSets";
    private bool supportsShouldProcess;
    private bool supportsTransactions;
    private ConfirmImpact confirmImpact = ConfirmImpact.Medium;
    private Dictionary<string, ParameterMetadata> externalParameterMetadata;
    private MergedCommandParameterMetadata staticCommandParameterMetadata;
    private bool implementsDynamicParameters;
    private uint defaultParameterSetFlag;
    private Collection<Attribute> otherAttributes = new Collection<Attribute>();
    private string wrappedCommand;
    private CommandTypes wrappedCommandType;
    private bool wrappedAnyCmdlet;

    public CommandMetadata(Type commandType) => this.Init(commandType, false);

    public CommandMetadata(CommandInfo commandInfo)
      : this(commandInfo, false)
    {
    }

    public CommandMetadata(CommandInfo commandInfo, bool shouldGenerateCommonParameters)
    {
      if (commandInfo == null)
        throw CommandMetadata.tracer.NewArgumentNullException(nameof (commandInfo));
      while (commandInfo is AliasInfo)
      {
        commandInfo = ((AliasInfo) commandInfo).ResolvedCommand;
        if (commandInfo == null)
          throw CommandMetadata.tracer.NewNotSupportedException();
      }
      if (commandInfo is CmdletInfo cmdletInfo)
        this.Init(cmdletInfo.ImplementingType, shouldGenerateCommonParameters);
      else if (commandInfo is ExternalScriptInfo externalScriptInfo)
      {
        this.Init(externalScriptInfo.ScriptBlock, externalScriptInfo.Path, shouldGenerateCommonParameters);
        this.wrappedCommandType = CommandTypes.ExternalScript;
      }
      else
      {
        if (!(commandInfo is FunctionInfo functionInfo))
          throw CommandMetadata.tracer.NewNotSupportedException();
        this.Init(functionInfo.ScriptBlock, functionInfo.Name, shouldGenerateCommonParameters);
        this.wrappedCommandType = commandInfo.CommandType;
      }
    }

    public CommandMetadata(string path)
    {
      this.Init(new ExternalScriptInfo(Path.GetFileName(path), path).ScriptBlock, path, false);
      this.wrappedCommandType = CommandTypes.ExternalScript;
    }

    public CommandMetadata(CommandMetadata other)
    {
      this.commandName = other != null ? other.commandName : throw CommandMetadata.tracer.NewArgumentNullException(nameof (other));
      this.confirmImpact = other.confirmImpact;
      this.defaultParameterSetFlag = other.defaultParameterSetFlag;
      this.defaultParameterSetName = other.defaultParameterSetName;
      this.implementsDynamicParameters = other.implementsDynamicParameters;
      this.supportsShouldProcess = other.supportsShouldProcess;
      this.supportsTransactions = other.supportsTransactions;
      this.type = other.type;
      this.wrappedAnyCmdlet = other.wrappedAnyCmdlet;
      this.wrappedCommand = other.wrappedCommand;
      this.wrappedCommandType = other.wrappedCommandType;
      if (other.externalParameterMetadata == null)
      {
        this.externalParameterMetadata = (Dictionary<string, ParameterMetadata>) null;
      }
      else
      {
        this.externalParameterMetadata = new Dictionary<string, ParameterMetadata>(other.externalParameterMetadata.Count, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, ParameterMetadata> keyValuePair in other.externalParameterMetadata)
          this.externalParameterMetadata.Add(keyValuePair.Key, new ParameterMetadata(keyValuePair.Value));
      }
      if (other.otherAttributes == null)
      {
        this.otherAttributes = (Collection<Attribute>) null;
      }
      else
      {
        this.otherAttributes = new Collection<Attribute>((IList<Attribute>) new List<Attribute>(other.otherAttributes.Count));
        foreach (Attribute otherAttribute in other.otherAttributes)
          this.otherAttributes.Add(otherAttribute);
      }
      this.staticCommandParameterMetadata = (MergedCommandParameterMetadata) null;
    }

    internal CommandMetadata(
      string name,
      CommandTypes commandType,
      bool isProxyForCmdlet,
      string defaultParameterSetName,
      bool supportsShouldProcess,
      ConfirmImpact confirmImpact,
      bool supportsTransactions,
      Dictionary<string, ParameterMetadata> parameters)
    {
      this.commandName = this.wrappedCommand = name;
      this.wrappedCommandType = commandType;
      this.wrappedAnyCmdlet = isProxyForCmdlet;
      this.defaultParameterSetName = defaultParameterSetName;
      this.supportsShouldProcess = supportsShouldProcess;
      this.confirmImpact = confirmImpact;
      this.supportsTransactions = supportsTransactions;
      this.externalParameterMetadata = parameters;
    }

    private void Init(Type commandType, bool shouldGenerateCommonParameters)
    {
      this.type = commandType != null ? commandType : throw CommandMetadata.tracer.NewArgumentNullException("cmdletType");
      InternalParameterMetadata parameterMetadata = InternalParameterMetadata.Get(commandType, (ExecutionContext) null, false);
      this.ConstructCmdletMetadataUsingReflection();
      this.externalParameterMetadata = ParameterMetadata.GetParameterMetadata(this.MergeParameterMetadata((ExecutionContext) null, parameterMetadata, shouldGenerateCommonParameters));
      this.wrappedCommand = this.commandName;
      this.wrappedCommandType = CommandTypes.Cmdlet;
      this.wrappedAnyCmdlet = true;
    }

    private void Init(ScriptBlock scriptBlock, string name, bool shouldGenerateCommonParameters)
    {
      if (scriptBlock.UsesCmdletBinding)
        this.wrappedAnyCmdlet = true;
      else
        shouldGenerateCommonParameters = false;
      CmdletBindingAttribute bindingAttribute = scriptBlock.CmdletBindingAttribute;
      if (bindingAttribute != null)
        this.ProcessCmdletAttribute((CmdletCommonMetadataAttribute) bindingAttribute);
      else if (scriptBlock.UsesCmdletBinding)
        this.defaultParameterSetName = (string) null;
      this.externalParameterMetadata = ParameterMetadata.GetParameterMetadata(this.MergeParameterMetadata((ExecutionContext) null, InternalParameterMetadata.Get(scriptBlock.RuntimeDefinedParameters, false, scriptBlock.UsesCmdletBinding), shouldGenerateCommonParameters));
      this.wrappedCommand = this.commandName = name;
    }

    internal static CommandMetadata Get(
      string commandName,
      Type cmdletType,
      ExecutionContext context)
    {
      if (string.IsNullOrEmpty(commandName))
        throw CommandMetadata.tracer.NewArgumentException(nameof (commandName));
      string key = cmdletType != null ? cmdletType.AssemblyQualifiedName : throw CommandMetadata.tracer.NewArgumentNullException(nameof (cmdletType));
      CommandMetadata commandMetadata;
      if (context != null && context.CommandMetadataCache.ContainsKey(key))
      {
        CommandMetadata.tracer.WriteLine("The cmdlet metadata was found in the cache with type name: {0}", (object) key);
        commandMetadata = context.CommandMetadataCache[key];
      }
      else
      {
        CommandMetadata.tracer.WriteLine("The cmdlet metadata was not found in the cache. Constructing a new instance with type name: {0}.", (object) key);
        commandMetadata = new CommandMetadata(commandName, cmdletType, context);
        context?.CommandMetadataCache.Add(key, commandMetadata);
      }
      return commandMetadata;
    }

    internal CommandMetadata(string commandName, Type cmdletType, ExecutionContext context)
    {
      if (string.IsNullOrEmpty(commandName))
        throw CommandMetadata.tracer.NewArgumentException(nameof (commandName));
      if (cmdletType == null)
        throw CommandMetadata.tracer.NewArgumentNullException(nameof (cmdletType));
      this.commandName = commandName;
      this.type = cmdletType;
      InternalParameterMetadata parameterMetadata = InternalParameterMetadata.Get(cmdletType, context, false);
      this.ConstructCmdletMetadataUsingReflection();
      this.staticCommandParameterMetadata = this.MergeParameterMetadata(context, parameterMetadata, true);
      this.defaultParameterSetFlag = this.staticCommandParameterMetadata.GenerateParameterSetMappingFromMetadata(this.defaultParameterSetName);
    }

    internal CommandMetadata(ScriptBlock scriptblock, string commandName, ExecutionContext context)
    {
      CmdletBindingAttribute bindingAttribute = scriptblock != null ? scriptblock.CmdletBindingAttribute : throw CommandMetadata.tracer.NewArgumentException(nameof (scriptblock));
      if (bindingAttribute != null)
        this.ProcessCmdletAttribute((CmdletCommonMetadataAttribute) bindingAttribute);
      else
        this.defaultParameterSetName = (string) null;
      this.commandName = commandName;
      this.type = typeof (PSScriptCmdlet);
      if (scriptblock.DynamicParams != null)
        this.implementsDynamicParameters = true;
      InternalParameterMetadata parameterMetadata = InternalParameterMetadata.Get(scriptblock.RuntimeDefinedParameters, false, scriptblock.UsesCmdletBinding);
      this.staticCommandParameterMetadata = this.MergeParameterMetadata(context, parameterMetadata, scriptblock.UsesCmdletBinding);
      this.defaultParameterSetFlag = this.staticCommandParameterMetadata.GenerateParameterSetMappingFromMetadata(this.defaultParameterSetName);
    }

    public string Name
    {
      get => this.commandName;
      set => this.commandName = value;
    }

    public Type CommandType => this.type;

    public string DefaultParameterSetName
    {
      get => this.defaultParameterSetName;
      set
      {
        if (string.IsNullOrEmpty(value))
          ;
        this.defaultParameterSetName = value;
      }
    }

    public bool SupportsShouldProcess
    {
      get => this.supportsShouldProcess;
      set => this.supportsShouldProcess = value;
    }

    public bool SupportsTransactions
    {
      get => this.supportsTransactions;
      set => this.supportsTransactions = value;
    }

    public ConfirmImpact ConfirmImpact
    {
      get => this.confirmImpact;
      set => this.confirmImpact = value;
    }

    public Dictionary<string, ParameterMetadata> Parameters => this.externalParameterMetadata;

    internal MergedCommandParameterMetadata StaticCommandParameterMetadata => this.staticCommandParameterMetadata;

    internal bool ImplementsDynamicParameters => this.implementsDynamicParameters;

    internal uint DefaultParameterSetFlag
    {
      get => this.defaultParameterSetFlag;
      set => this.defaultParameterSetFlag = value;
    }

    internal bool WrappedAnyCmdlet => this.wrappedAnyCmdlet;

    internal CommandTypes WrappedCommandType => this.wrappedCommandType;

    private void ConstructCmdletMetadataUsingReflection()
    {
      if (this.type.GetInterface(typeof (IDynamicParameters).Name, true) != null)
        this.implementsDynamicParameters = true;
      foreach (Attribute customAttribute in this.type.GetCustomAttributes(false))
      {
        if (customAttribute is CmdletAttribute cmdletAttribute)
        {
          this.ProcessCmdletAttribute((CmdletCommonMetadataAttribute) cmdletAttribute);
          this.Name = cmdletAttribute.VerbName + "-" + cmdletAttribute.NounName;
        }
        else
          this.otherAttributes.Add(customAttribute);
      }
    }

    private void ProcessCmdletAttribute(CmdletCommonMetadataAttribute attribute)
    {
      this.defaultParameterSetName = attribute != null ? attribute.DefaultParameterSetName : throw CommandMetadata.tracer.NewArgumentNullException(nameof (attribute));
      this.supportsShouldProcess = attribute.SupportsShouldProcess;
      this.confirmImpact = attribute.ConfirmImpact;
      this.supportsTransactions = attribute.SupportsTransactions;
    }

    private MergedCommandParameterMetadata MergeParameterMetadata(
      ExecutionContext context,
      InternalParameterMetadata parameterMetadata,
      bool shouldGenerateCommonParameters)
    {
      MergedCommandParameterMetadata parameterMetadata1 = new MergedCommandParameterMetadata();
      parameterMetadata1.AddMetadataForBinder(parameterMetadata, ParameterBinderAssociation.DeclaredFormalParameters);
      if (shouldGenerateCommonParameters)
      {
        InternalParameterMetadata parameterMetadata2 = InternalParameterMetadata.Get(typeof (CommonParameters), context, false);
        parameterMetadata1.AddMetadataForBinder(parameterMetadata2, ParameterBinderAssociation.CommonParameters);
        if (this.SupportsShouldProcess)
        {
          InternalParameterMetadata parameterMetadata3 = InternalParameterMetadata.Get(typeof (ShouldProcessParameters), context, false);
          parameterMetadata1.AddMetadataForBinder(parameterMetadata3, ParameterBinderAssociation.ShouldProcessParameters);
        }
        if (this.SupportsTransactions)
        {
          InternalParameterMetadata parameterMetadata3 = InternalParameterMetadata.Get(typeof (TransactionParameters), context, false);
          parameterMetadata1.AddMetadataForBinder(parameterMetadata3, ParameterBinderAssociation.TransactionParameters);
        }
      }
      return parameterMetadata1;
    }

    internal static string EscapeBlockComment(string helpContent) => string.IsNullOrEmpty(helpContent) ? string.Empty : helpContent.Replace("<#", "<`#").Replace("#>", "#`>");

    internal static string EscapeSingleQuotedString(string stringContent)
    {
      if (string.IsNullOrEmpty(stringContent))
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder(stringContent.Length);
      foreach (char c in stringContent)
      {
        stringBuilder.Append(c);
        if (SpecialCharacters.IsSingleQuote(c))
          stringBuilder.Append(c);
      }
      return stringBuilder.ToString();
    }

    internal static string EscapeVariableName(string variableName) => string.IsNullOrEmpty(variableName) ? string.Empty : variableName.Replace("`", "``").Replace("}", "`}").Replace("{", "`{");

    internal string GetProxyCommand(string helpComment)
    {
      CommandMetadata.tracer.WriteLine("Generating proxy command from CommandMetaData", new object[0]);
      if (string.IsNullOrEmpty(helpComment))
        helpComment = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n.ForwardHelpTargetName {0}\r\n.ForwardHelpCategory {1}\r\n", (object) this.wrappedCommand, (object) this.wrappedCommandType);
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}\r\nparam({1})\r\n\r\nbegin\r\n{{{2}}}\r\n\r\nprocess\r\n{{{3}}}\r\n\r\nend\r\n{{{4}}}\r\n<#\r\n{5}\r\n#>\r\n", (object) this.GetDecl(), (object) this.GetParamBlock(), (object) this.GetBeginBlock(), (object) this.GetProcessBlock(), (object) this.GetEndBlock(), (object) CommandMetadata.EscapeBlockComment(helpComment));
    }

    internal string GetDecl()
    {
      string str1 = "";
      string str2 = "";
      if (this.wrappedAnyCmdlet)
      {
        StringBuilder stringBuilder = new StringBuilder("[CmdletBinding(");
        if (!string.IsNullOrEmpty(this.defaultParameterSetName))
        {
          stringBuilder.Append(str2);
          stringBuilder.AppendFormat("DefaultParameterSetName='{0}'", (object) CommandMetadata.EscapeSingleQuotedString(this.defaultParameterSetName));
          str2 = ", ";
        }
        if (this.supportsShouldProcess)
        {
          stringBuilder.Append(str2);
          stringBuilder.Append("SupportsShouldProcess=$true");
          str2 = ", ";
          stringBuilder.Append(str2);
          stringBuilder.AppendFormat("ConfirmImpact='{0}'", (object) this.confirmImpact);
        }
        if (this.supportsTransactions)
        {
          stringBuilder.Append(str2);
          stringBuilder.Append("SupportsTransactions=$true");
        }
        stringBuilder.Append(")]");
        str1 = stringBuilder.ToString();
      }
      return str1;
    }

    internal string GetParamBlock()
    {
      if (this.externalParameterMetadata.Keys.Count <= 0)
        return "";
      StringBuilder stringBuilder = new StringBuilder();
      string prefix = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}    ", (object) Environment.NewLine);
      string str = "";
      foreach (string key in this.externalParameterMetadata.Keys)
      {
        string proxyParameterData = this.externalParameterMetadata[key].GetProxyParameterData(prefix, key, this.wrappedAnyCmdlet);
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}", (object) str, (object) proxyParameterData);
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}", (object) ",", (object) Environment.NewLine);
      }
      return stringBuilder.ToString();
    }

    internal string GetBeginBlock()
    {
      if (string.IsNullOrEmpty(this.wrappedCommand))
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("ProxyCommandStrings", "CommandMetadataMissingCommandName"));
      string str;
      if (this.wrappedAnyCmdlet)
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    try {{\r\n        $outBuffer = $null\r\n        if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer))\r\n        {{\r\n            $PSBoundParameters['OutBuffer'] = 1\r\n        }}\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('{0}', [System.Management.Automation.CommandTypes]::{1})\r\n        $scriptCmd = {{& $wrappedCmd @PSBoundParameters }}\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n        $steppablePipeline.Begin($PSCmdlet)\r\n    }} catch {{\r\n        throw\r\n    }}\r\n", (object) CommandMetadata.EscapeSingleQuotedString(this.wrappedCommand), (object) this.wrappedCommandType.ToString());
      else
        str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n    try {{\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('{0}', [System.Management.Automation.CommandTypes]::{1})\r\n        $PSBoundParameters.Add('$args', $args)\r\n        $scriptCmd = {{& $wrappedCmd @PSBoundParameters }}\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n        $steppablePipeline.Begin($myInvocation.ExpectingInput, $ExecutionContext)\r\n    }} catch {{\r\n        throw\r\n    }}\r\n", (object) CommandMetadata.EscapeSingleQuotedString(this.wrappedCommand), (object) this.wrappedCommandType.ToString());
      return str;
    }

    internal string GetProcessBlock() => "\r\n    try {\r\n        $steppablePipeline.Process($_)\r\n    } catch {\r\n        throw\r\n    }\r\n";

    internal string GetEndBlock() => "\r\n    try {\r\n        $steppablePipeline.End()\r\n    } catch {\r\n        throw\r\n    }\r\n";

    internal string GetDynamicParamBlock() => "";

    private static CommandMetadata GetRestrictedCmdlet(
      string cmdletName,
      params ParameterMetadata[] parameters)
    {
      Dictionary<string, ParameterMetadata> parameters1 = new Dictionary<string, ParameterMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (ParameterMetadata parameter in parameters)
        parameters1.Add(parameter.Name, parameter);
      return new CommandMetadata(cmdletName, CommandTypes.Cmdlet, true, (string) null, false, ConfirmImpact.None, false, parameters1);
    }

    private static CommandMetadata GetRestrictedGetCommand()
    {
      ParameterMetadata parameterMetadata1 = new ParameterMetadata("Name", typeof (string[]));
      parameterMetadata1.Attributes.Add((Attribute) new ValidateLengthAttribute(0, 1000));
      parameterMetadata1.Attributes.Add((Attribute) new ValidateCountAttribute(0, 1000));
      ParameterMetadata parameterMetadata2 = new ParameterMetadata("Module", typeof (string[]));
      parameterMetadata2.Attributes.Add((Attribute) new ValidateLengthAttribute(0, 1000));
      parameterMetadata2.Attributes.Add((Attribute) new ValidateCountAttribute(0, 100));
      ParameterMetadata parameterMetadata3 = new ParameterMetadata("ArgumentList", typeof (object[]));
      parameterMetadata3.Attributes.Add((Attribute) new ValidateCountAttribute(0, 100));
      ParameterMetadata parameterMetadata4 = new ParameterMetadata("CommandType", typeof (CommandTypes));
      return CommandMetadata.GetRestrictedCmdlet("Get-Command", parameterMetadata1, parameterMetadata2, parameterMetadata3, parameterMetadata4);
    }

    private static CommandMetadata GetRestrictedGetFormatData() => CommandMetadata.GetRestrictedCmdlet("Get-FormatData", new ParameterMetadata("TypeName", typeof (string[]))
    {
      Attributes = {
        (Attribute) new ValidateLengthAttribute(0, 1000),
        (Attribute) new ValidateCountAttribute(0, 1000)
      }
    });

    private static CommandMetadata GetRestrictedGetHelp() => CommandMetadata.GetRestrictedCmdlet("Get-Help", new ParameterMetadata("Name", typeof (string))
    {
      Attributes = {
        (Attribute) new ValidatePatternAttribute("^[-._:\\\\\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Nd}\\p{Lm}]{1,100}$"),
        (Attribute) new ValidateLengthAttribute(0, 1000)
      }
    }, new ParameterMetadata("Category", typeof (string[]))
    {
      Attributes = {
        (Attribute) new ValidateSetAttribute(Enum.GetNames(typeof (HelpCategory))),
        (Attribute) new ValidateCountAttribute(0, 1)
      }
    });

    private static CommandMetadata GetRestrictedSelectObject()
    {
      string[] strArray = new string[11]
      {
        "ModuleName",
        "Namespace",
        "OutputType",
        "Count",
        "HelpUri",
        "Name",
        "CommandType",
        "ResolvedCommandName",
        "DefaultParameterSet",
        "CmdletBinding",
        "Parameters"
      };
      return CommandMetadata.GetRestrictedCmdlet("Select-Object", new ParameterMetadata("Property", typeof (string[]))
      {
        Attributes = {
          (Attribute) new ValidateSetAttribute(strArray),
          (Attribute) new ValidateCountAttribute(1, strArray.Length)
        }
      }, new ParameterMetadata("InputObject", typeof (object))
      {
        ParameterSets = {
          {
            "__AllParameterSets",
            new ParameterSetMetadata(int.MinValue, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline, (string) null)
          }
        }
      });
    }

    private static CommandMetadata GetRestrictedMeasureObject() => CommandMetadata.GetRestrictedCmdlet("Measure-Object", new ParameterMetadata("InputObject", typeof (object))
    {
      ParameterSets = {
        {
          "__AllParameterSets",
          new ParameterSetMetadata(int.MinValue, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline, (string) null)
        }
      }
    });

    private static CommandMetadata GetRestrictedOutDefault() => CommandMetadata.GetRestrictedCmdlet("Out-Default", new ParameterMetadata("InputObject", typeof (object))
    {
      ParameterSets = {
        {
          "__AllParameterSets",
          new ParameterSetMetadata(int.MinValue, ParameterSetMetadata.ParameterFlags.Mandatory | ParameterSetMetadata.ParameterFlags.ValueFromPipeline, (string) null)
        }
      }
    });

    private static CommandMetadata GetRestrictedExitPSSession() => CommandMetadata.GetRestrictedCmdlet("Exit-PSSession");

    public static Dictionary<string, CommandMetadata> GetRestrictedCommands(
      SessionCapabilities sessionCapabilities)
    {
      List<CommandMetadata> commandMetadataList = new List<CommandMetadata>();
      if (SessionCapabilities.RemoteServer == (sessionCapabilities & SessionCapabilities.RemoteServer))
      {
        commandMetadataList.Add(CommandMetadata.GetRestrictedGetCommand());
        commandMetadataList.Add(CommandMetadata.GetRestrictedGetFormatData());
        commandMetadataList.Add(CommandMetadata.GetRestrictedSelectObject());
        commandMetadataList.Add(CommandMetadata.GetRestrictedGetHelp());
        commandMetadataList.Add(CommandMetadata.GetRestrictedMeasureObject());
        commandMetadataList.Add(CommandMetadata.GetRestrictedExitPSSession());
        commandMetadataList.Add(CommandMetadata.GetRestrictedOutDefault());
      }
      Dictionary<string, CommandMetadata> dictionary = new Dictionary<string, CommandMetadata>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (CommandMetadata commandMetadata in commandMetadataList)
        dictionary.Add(commandMetadata.Name, commandMetadata);
      return dictionary;
    }
  }
}
