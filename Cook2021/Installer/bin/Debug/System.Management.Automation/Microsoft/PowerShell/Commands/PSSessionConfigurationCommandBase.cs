// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSSessionConfigurationCommandBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Security.AccessControl;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  public class PSSessionConfigurationCommandBase : PSCmdlet
  {
    internal const string AssemblyNameParameterSetName = "AssemblyNameParameterSet";
    internal string shellName;
    internal string assemblyName;
    internal bool isAssemblyNameSpecified;
    internal string applicationBase;
    internal bool isApplicationBaseSpecified;
    internal string configurationTypeName;
    internal bool isConfigurationTypeNameSpecified;
    internal ApartmentState? threadAptState;
    internal PSThreadOptions? threadOptions;
    internal string configurationScript;
    internal bool isConfigurationScriptSpecified;
    internal double? maxCommandSizeMB;
    internal bool isMaxCommandSizeMBSpecified;
    internal double? maxObjectSizeMB;
    internal bool isMaxObjectSizeMBSpecified;
    internal string sddl;
    internal bool isSddlSpecified;
    private bool showUI;
    internal bool showUISpecified;
    internal bool force;
    internal bool noRestart;

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0)]
    public string Name
    {
      get => this.shellName;
      set => this.shellName = value;
    }

    [Parameter(Mandatory = true, ParameterSetName = "AssemblyNameParameterSet", Position = 1)]
    public string AssemblyName
    {
      get => this.assemblyName;
      set
      {
        this.assemblyName = value;
        this.isAssemblyNameSpecified = true;
      }
    }

    [Parameter]
    public string ApplicationBase
    {
      get => this.applicationBase;
      set
      {
        this.applicationBase = value;
        this.isApplicationBaseSpecified = true;
      }
    }

    [Parameter(Mandatory = true, ParameterSetName = "AssemblyNameParameterSet", Position = 2)]
    public string ConfigurationTypeName
    {
      get => this.configurationTypeName;
      set
      {
        this.configurationTypeName = value;
        this.isConfigurationTypeNameSpecified = true;
      }
    }

    [Parameter]
    public ApartmentState ThreadApartmentState
    {
      get => this.threadAptState.HasValue ? this.threadAptState.Value : ApartmentState.Unknown;
      set => this.threadAptState = new ApartmentState?(value);
    }

    [Parameter]
    public PSThreadOptions ThreadOptions
    {
      get => this.threadOptions.HasValue ? this.threadOptions.Value : PSThreadOptions.UseCurrentThread;
      set => this.threadOptions = new PSThreadOptions?(value);
    }

    [Parameter]
    public string StartupScript
    {
      get => this.configurationScript;
      set
      {
        this.configurationScript = value;
        this.isConfigurationScriptSpecified = true;
      }
    }

    [Parameter]
    [AllowNull]
    public double? MaximumReceivedDataSizePerCommandMB
    {
      get => this.maxCommandSizeMB;
      set
      {
        if (value.HasValue && value.Value < 0.0)
          throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.CSCDoubleParameterOutOfRange, (object) value.Value, (object) nameof (MaximumReceivedDataSizePerCommandMB)));
        this.maxCommandSizeMB = value;
        this.isMaxCommandSizeMBSpecified = true;
      }
    }

    [AllowNull]
    [Parameter]
    public double? MaximumReceivedObjectSizeMB
    {
      get => this.maxObjectSizeMB;
      set
      {
        if (value.HasValue && value.Value < 0.0)
          throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.CSCDoubleParameterOutOfRange, (object) value.Value, (object) nameof (MaximumReceivedObjectSizeMB)));
        this.maxObjectSizeMB = value;
        this.isMaxObjectSizeMBSpecified = true;
      }
    }

    [Parameter]
    public string SecurityDescriptorSddl
    {
      get => this.sddl;
      set
      {
        this.sddl = string.IsNullOrEmpty(value) || new CommonSecurityDescriptor(false, false, value) != null ? value : throw new NotSupportedException();
        this.isSddlSpecified = true;
      }
    }

    [Parameter]
    public SwitchParameter ShowSecurityDescriptorUI
    {
      get => (SwitchParameter) this.showUI;
      set
      {
        this.showUI = (bool) value;
        this.showUISpecified = true;
      }
    }

    [Parameter]
    public SwitchParameter Force
    {
      get => (SwitchParameter) this.force;
      set => this.force = (bool) value;
    }

    [Parameter]
    public SwitchParameter NoServiceRestart
    {
      get => (SwitchParameter) this.noRestart;
      set => this.noRestart = (bool) value;
    }

    internal PSSessionConfigurationCommandBase()
    {
    }
  }
}
