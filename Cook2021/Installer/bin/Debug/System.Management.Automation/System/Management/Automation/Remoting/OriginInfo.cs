// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.OriginInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  public class OriginInfo
  {
    private string computerName;
    private Guid runspaceID;

    public string PSComputerName => this.computerName;

    public Guid RunspaceID => this.runspaceID;

    public OriginInfo(string computerName, Guid runspaceID)
    {
      this.computerName = computerName;
      this.runspaceID = runspaceID;
    }

    public override string ToString() => this.PSComputerName;
  }
}
