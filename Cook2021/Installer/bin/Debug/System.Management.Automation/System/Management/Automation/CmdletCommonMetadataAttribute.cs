// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletCommonMetadataAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Class)]
  public abstract class CmdletCommonMetadataAttribute : CmdletMetadataAttribute
  {
    private string defaultParameterSetName;
    private bool supportsShouldProcess;
    private bool supportsTransactions;
    private ConfirmImpact confirmImpact = ConfirmImpact.Medium;

    public string DefaultParameterSetName
    {
      get => this.defaultParameterSetName;
      set => this.defaultParameterSetName = value;
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
  }
}
