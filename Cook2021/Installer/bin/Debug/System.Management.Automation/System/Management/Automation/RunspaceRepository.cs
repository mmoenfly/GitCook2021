// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RunspaceRepository
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class RunspaceRepository : Repository<PSSession>
  {
    public List<PSSession> Runspaces => this.Items;

    internal RunspaceRepository()
      : base("runspace")
    {
    }

    internal override Guid GetKey(PSSession item) => item != null ? item.InstanceId : Guid.Empty;
  }
}
