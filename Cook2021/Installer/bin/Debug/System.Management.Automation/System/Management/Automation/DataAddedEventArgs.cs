// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DataAddedEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class DataAddedEventArgs : EventArgs
  {
    private int index;
    private Guid psInstanceId;

    internal DataAddedEventArgs(Guid psInstanceId, int index)
    {
      this.psInstanceId = psInstanceId;
      this.index = index;
    }

    public int Index => this.index;

    public Guid PowerShellInstanceId => this.psInstanceId;
  }
}
