// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  public class CmdletInfo : CommandInfo
  {
    [TraceSource("CmdletInfo", "The command information for PowerShell cmdlets that are directly executable by PowerShell.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CmdletInfo), "The command information for PowerShell cmdlets that are directly executable by PowerShell.");
    private string verb = string.Empty;
    private string noun = string.Empty;
    private string helpFilePath = string.Empty;
    private PSSnapInInfo _PSSnapin;
    private Type implementingType;
    private List<PSTypeName> _outputType;
    private CommandMetadata cmdletMetadata;

    internal CmdletInfo(
      string name,
      Type implementingType,
      string helpFile,
      PSSnapInInfo PSSnapin,
      ExecutionContext context)
      : base(name, CommandTypes.Cmdlet, context)
    {
      if (string.IsNullOrEmpty(name))
        throw CmdletInfo.tracer.NewArgumentException(nameof (name));
      if (implementingType == null)
        throw CmdletInfo.tracer.NewArgumentNullException(nameof (implementingType));
      if (!CmdletInfo.SplitCmdletName(name, out this.verb, out this.noun))
        throw CmdletInfo.tracer.NewArgumentException(nameof (name), "DiscoveryExceptions", "InvalidCmdletNameFormat", (object) name);
      this.implementingType = implementingType;
      this.helpFilePath = helpFile;
      this._PSSnapin = PSSnapin;
    }

    internal CmdletInfo(CmdletInfo other)
      : base((CommandInfo) other)
    {
      this.verb = other.verb;
      this.noun = other.noun;
      this.implementingType = other.implementingType;
      this.helpFilePath = other.helpFilePath;
      this._PSSnapin = other._PSSnapin;
    }

    internal override CommandInfo CreateGetCommandCopy(object[] arguments)
    {
      CmdletInfo cmdletInfo = new CmdletInfo(this);
      cmdletInfo.IsGetCommandCopy = true;
      cmdletInfo.Arguments = arguments;
      return (CommandInfo) cmdletInfo;
    }

    public string Verb => this.verb;

    public string Noun => this.noun;

    internal static bool SplitCmdletName(string name, out string verb, out string noun)
    {
      noun = verb = string.Empty;
      if (string.IsNullOrEmpty(name))
        return false;
      int length = 0;
      for (int index = 0; index < name.Length; ++index)
      {
        if (SpecialCharacters.IsDash(name[index]))
        {
          length = index;
          break;
        }
      }
      if (length <= 0)
        return false;
      verb = name.Substring(0, length);
      noun = name.Substring(length + 1);
      return true;
    }

    public string HelpFile => this.helpFilePath;

    internal override HelpCategory HelpCategory => HelpCategory.Cmdlet;

    public PSSnapInInfo PSSnapIn => this._PSSnapin;

    internal string PSSnapInName
    {
      get
      {
        string str = (string) null;
        if (this._PSSnapin != null)
          str = this._PSSnapin.Name;
        return str;
      }
    }

    public Type ImplementingType => this.implementingType;

    public override string Definition
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (CommandParameterSetInfo parameterSet in this.ParameterSets)
          stringBuilder.AppendLine(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0}{1}{2} {3}", (object) this.verb, (object) '-', (object) this.noun, (object) parameterSet.ToString()));
        return stringBuilder.ToString();
      }
    }

    public string DefaultParameterSet => this.CommandMetadata.DefaultParameterSetName;

    public override ReadOnlyCollection<PSTypeName> OutputType
    {
      get
      {
        if (this._outputType == null)
        {
          this._outputType = new List<PSTypeName>();
          foreach (OutputTypeAttribute customAttribute in this.ImplementingType.GetCustomAttributes(typeof (OutputTypeAttribute), false))
            this._outputType.AddRange((IEnumerable<PSTypeName>) customAttribute.Type);
        }
        List<PSTypeName> listToAppend = new List<PSTypeName>();
        this.Context.SessionState.Path.CurrentLocation.Provider.GetOutputTypes(this.Name, listToAppend);
        if (listToAppend.Count <= 0)
          return new ReadOnlyCollection<PSTypeName>((IList<PSTypeName>) this._outputType);
        listToAppend.InsertRange(0, (IEnumerable<PSTypeName>) this._outputType);
        return new ReadOnlyCollection<PSTypeName>((IList<PSTypeName>) listToAppend);
      }
    }

    private static string GetFullName(string moduleName, string cmdletName)
    {
      string str = cmdletName;
      if (!string.IsNullOrEmpty(moduleName))
        str = moduleName + (object) '\\' + str;
      return str;
    }

    private static string GetFullName(CmdletInfo cmdletInfo) => CmdletInfo.GetFullName(cmdletInfo.ModuleName, cmdletInfo.Name);

    internal static string GetFullName(PSObject psObject)
    {
      if (psObject.BaseObject is CmdletInfo)
        return CmdletInfo.GetFullName((CmdletInfo) psObject.BaseObject);
      PSPropertyInfo property1 = psObject.Properties["Name"];
      PSPropertyInfo property2 = psObject.Properties["PSSnapIn"];
      string cmdletName = property1 == null ? "" : (string) property1.Value;
      return CmdletInfo.GetFullName(property2 == null ? "" : (string) property2.Value, cmdletName);
    }

    internal string FullName => CmdletInfo.GetFullName(this);

    internal override CommandMetadata CommandMetadata
    {
      get
      {
        if (this.cmdletMetadata == null)
          this.cmdletMetadata = CommandMetadata.Get(this.Name, this.ImplementingType, this.Context);
        return this.cmdletMetadata;
      }
    }

    internal override bool ImplementsDynamicParameters => this.ImplementingType.GetInterface(typeof (IDynamicParameters).Name, true) != null;
  }
}
