// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DriveManagementIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public sealed class DriveManagementIntrinsics
  {
    [TraceSource("DriveCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating drives")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("DriveCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating drives");
    private SessionStateInternal sessionState;

    private DriveManagementIntrinsics()
    {
    }

    internal DriveManagementIntrinsics(SessionStateInternal sessionState)
    {
      using (DriveManagementIntrinsics.tracer.TraceConstructor((object) this))
        this.sessionState = sessionState != null ? sessionState : throw DriveManagementIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));
    }

    public PSDriveInfo Current
    {
      get
      {
        using (DriveManagementIntrinsics.tracer.TraceProperty())
          return this.sessionState.CurrentDrive;
      }
    }

    public PSDriveInfo New(PSDriveInfo drive, string scope)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.NewDrive(drive, scope);
    }

    internal void New(PSDriveInfo drive, string scope, CmdletProviderContext context)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        this.sessionState.NewDrive(drive, scope, context);
    }

    internal object NewDriveDynamicParameters(string providerId, CmdletProviderContext context)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.NewDriveDynamicParameters(providerId, context);
    }

    public void Remove(string driveName, bool force, string scope)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        this.sessionState.RemoveDrive(driveName, force, scope);
    }

    internal void Remove(
      string driveName,
      bool force,
      string scope,
      CmdletProviderContext context)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        this.sessionState.RemoveDrive(driveName, force, scope, context);
    }

    public PSDriveInfo Get(string driveName)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetDrive(driveName);
    }

    public PSDriveInfo GetAtScope(string driveName, string scope)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetDrive(driveName, scope);
    }

    public Collection<PSDriveInfo> GetAll()
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.Drives((string) null);
    }

    public Collection<PSDriveInfo> GetAllAtScope(string scope)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod(scope, new object[0]))
        return this.sessionState.Drives(scope);
    }

    public Collection<PSDriveInfo> GetAllForProvider(string providerName)
    {
      using (DriveManagementIntrinsics.tracer.TraceMethod())
        return this.sessionState.GetDrivesForProvider(providerName);
    }
  }
}
