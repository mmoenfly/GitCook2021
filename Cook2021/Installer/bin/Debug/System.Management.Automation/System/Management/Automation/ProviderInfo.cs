// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Provider;
using System.Reflection;

namespace System.Management.Automation
{
  public class ProviderInfo
  {
    [TraceSource("CoreCommandProvider", "The namespace navigation tracer")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CoreCommandProvider", "The namespace navigation tracer");
    private Type implementingType;
    private string helpFile = "";
    private SessionState sessionState;
    private string name;
    private PSSnapInInfo pssnapin;
    private PSModuleInfo _module;
    private string description;
    private ProviderCapabilities capabilities;
    private bool capabilitiesRead;
    private string home;
    private PSDriveInfo hiddenDrive;
    private Dictionary<string, List<PSTypeName>> providerOutputType;

    public Type ImplementingType => this.implementingType;

    public string HelpFile => this.helpFile;

    public string Name => this.name;

    internal string FullName
    {
      get
      {
        string str = this.Name;
        if (!string.IsNullOrEmpty(this.PSSnapInName))
          str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}\\{1}", (object) this.PSSnapInName, (object) this.Name);
        return str;
      }
    }

    public PSSnapInInfo PSSnapIn => this.pssnapin;

    internal string PSSnapInName
    {
      get
      {
        string str = (string) null;
        if (this.pssnapin != null)
          str = this.pssnapin.Name;
        return str;
      }
    }

    public string ModuleName
    {
      get
      {
        if (this.pssnapin != null)
          return this.pssnapin.Name;
        return this._module != null ? this._module.Name : string.Empty;
      }
    }

    public PSModuleInfo Module => this._module;

    internal void SetModule(PSModuleInfo module) => this._module = module;

    public string Description
    {
      get => this.description;
      set => this.description = value;
    }

    public ProviderCapabilities Capabilities
    {
      get
      {
        if (!this.capabilitiesRead)
        {
          try
          {
            object[] customAttributes = this.ImplementingType.GetCustomAttributes(typeof (CmdletProviderAttribute), false);
            if (customAttributes != null)
            {
              if (customAttributes.Length == 1)
              {
                this.capabilities = ((CmdletProviderAttribute) customAttributes[0]).ProviderCapabilities;
                this.capabilitiesRead = true;
              }
            }
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
          }
        }
        return this.capabilities;
      }
    }

    public string Home
    {
      get => this.home;
      set => this.home = value;
    }

    public Collection<PSDriveInfo> Drives => this.sessionState.Drive.GetAllForProvider(this.FullName);

    internal PSDriveInfo HiddenDrive => this.hiddenDrive;

    public override string ToString() => this.FullName;

    protected ProviderInfo(ProviderInfo providerInfo)
    {
      this.name = providerInfo != null ? providerInfo.Name : throw ProviderInfo.tracer.NewArgumentNullException(nameof (providerInfo));
      this.implementingType = providerInfo.ImplementingType;
      this.capabilities = providerInfo.capabilities;
      this.description = providerInfo.description;
      this.hiddenDrive = providerInfo.hiddenDrive;
      this.home = providerInfo.home;
      this.helpFile = providerInfo.helpFile;
      this.pssnapin = providerInfo.pssnapin;
      this.sessionState = providerInfo.sessionState;
    }

    internal ProviderInfo(
      SessionState sessionState,
      Type implementingType,
      string name,
      string helpFile,
      PSSnapInInfo psSnapIn)
      : this(sessionState, implementingType, name, string.Empty, string.Empty, helpFile, psSnapIn)
    {
    }

    internal ProviderInfo(
      SessionState sessionState,
      Type implementingType,
      string name,
      string description,
      string home,
      string helpFile,
      PSSnapInInfo psSnapIn)
    {
      if (sessionState == null)
        throw ProviderInfo.tracer.NewArgumentNullException(nameof (sessionState));
      if (implementingType == null)
        throw ProviderInfo.tracer.NewArgumentNullException(nameof (implementingType));
      if (string.IsNullOrEmpty(name))
        throw ProviderInfo.tracer.NewArgumentException(nameof (name));
      if (string.IsNullOrEmpty(name))
        throw ProviderInfo.tracer.NewArgumentException(nameof (name));
      this.sessionState = sessionState;
      this.name = name;
      this.description = description;
      this.home = home;
      this.implementingType = implementingType;
      this.helpFile = helpFile;
      this.pssnapin = psSnapIn;
      this.hiddenDrive = new PSDriveInfo(this.FullName, this, "", "", (PSCredential) null);
      this.hiddenDrive.Hidden = true;
    }

    internal bool NameEquals(string providerName)
    {
      PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(providerName);
      bool flag = false;
      if (instance != null)
      {
        if (string.IsNullOrEmpty(instance.PSSnapInName) || string.Equals(instance.PSSnapInName, this.PSSnapInName, StringComparison.OrdinalIgnoreCase))
          flag = string.Equals(instance.ShortName, this.Name, StringComparison.OrdinalIgnoreCase);
      }
      else
        flag = string.Equals(providerName, this.Name, StringComparison.OrdinalIgnoreCase);
      ProviderInfo.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool IsMatch(string providerName)
    {
      PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(providerName);
      WildcardPattern namePattern = (WildcardPattern) null;
      if (instance != null && WildcardPattern.ContainsWildcardCharacters(instance.ShortName))
        namePattern = new WildcardPattern(instance.ShortName, WildcardOptions.IgnoreCase);
      return this.IsMatch(namePattern, instance);
    }

    internal bool IsMatch(WildcardPattern namePattern, PSSnapinQualifiedName psSnapinQualifiedName)
    {
      bool flag = false;
      if (psSnapinQualifiedName == null)
        flag = true;
      else if (namePattern == null)
      {
        if (string.Equals(this.Name, psSnapinQualifiedName.ShortName, StringComparison.OrdinalIgnoreCase) && this.IsPSSnapinNameMatch(psSnapinQualifiedName))
          flag = true;
      }
      else if (namePattern.IsMatch(this.Name) && this.IsPSSnapinNameMatch(psSnapinQualifiedName))
        flag = true;
      return flag;
    }

    private bool IsPSSnapinNameMatch(PSSnapinQualifiedName psSnapinQualifiedName)
    {
      bool flag = false;
      if (string.IsNullOrEmpty(psSnapinQualifiedName.PSSnapInName) || string.Equals(psSnapinQualifiedName.PSSnapInName, this.PSSnapInName, StringComparison.OrdinalIgnoreCase))
        flag = true;
      return flag;
    }

    internal CmdletProvider CreateInstance()
    {
      object obj = (object) null;
      Exception exception = (Exception) null;
      try
      {
        obj = Activator.CreateInstance(this.ImplementingType);
      }
      catch (TargetInvocationException ex)
      {
        exception = ex.InnerException;
      }
      catch (MissingMethodException ex)
      {
      }
      catch (MemberAccessException ex)
      {
      }
      catch (ArgumentException ex)
      {
      }
      if (obj == null)
      {
        ProviderNotFoundException notFoundException;
        if (exception != null)
          notFoundException = new ProviderNotFoundException(this.Name, SessionStateCategory.CmdletProvider, "ProviderCtorException", new object[1]
          {
            (object) exception.Message
          });
        else
          notFoundException = new ProviderNotFoundException(this.Name, SessionStateCategory.CmdletProvider, "ProviderNotFoundInAssembly", new object[0]);
        ProviderInfo.tracer.TraceException((Exception) notFoundException);
        throw notFoundException;
      }
      CmdletProvider cmdletProvider = obj as CmdletProvider;
      cmdletProvider.SetProviderInformation(this);
      return cmdletProvider;
    }

    internal void GetOutputTypes(string cmdletname, List<PSTypeName> listToAppend)
    {
      if (this.providerOutputType == null)
      {
        this.providerOutputType = new Dictionary<string, List<PSTypeName>>();
        foreach (OutputTypeAttribute customAttribute in this.implementingType.GetCustomAttributes(typeof (OutputTypeAttribute), false))
        {
          if (!string.IsNullOrEmpty(customAttribute.ProviderCmdlet))
          {
            List<PSTypeName> psTypeNameList;
            if (!this.providerOutputType.TryGetValue(customAttribute.ProviderCmdlet, out psTypeNameList))
            {
              psTypeNameList = new List<PSTypeName>();
              this.providerOutputType[customAttribute.ProviderCmdlet] = psTypeNameList;
            }
            psTypeNameList.AddRange((IEnumerable<PSTypeName>) customAttribute.Type);
          }
        }
      }
      List<PSTypeName> psTypeNameList1 = (List<PSTypeName>) null;
      if (!this.providerOutputType.TryGetValue(cmdletname, out psTypeNameList1))
        return;
      listToAppend.AddRange((IEnumerable<PSTypeName>) psTypeNameList1);
    }
  }
}
