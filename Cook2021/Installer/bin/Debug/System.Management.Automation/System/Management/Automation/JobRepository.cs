// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.JobRepository
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  public class JobRepository : Repository<Job>
  {
    public List<Job> Jobs => this.Items;

    public Job GetJob(Guid instanceId) => this.GetItem(instanceId);

    internal JobRepository()
      : base("job")
    {
    }

    internal override Guid GetKey(Job item) => item != null ? item.InstanceId : Guid.Empty;
  }
}
