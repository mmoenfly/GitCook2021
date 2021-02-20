// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.DriveCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation.Provider
{
  public abstract class DriveCmdletProvider : CmdletProvider
  {
    internal PSDriveInfo NewDrive(PSDriveInfo drive, CmdletProviderContext context)
    {
      this.Context = context;
      if (drive.Credential != null && drive.Credential != PSCredential.Empty && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Credentials, this.ProviderInfo))
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "NewDriveCredentials_NotSupported");
      return this.NewDrive(drive);
    }

    internal object NewDriveDynamicParameters(CmdletProviderContext context)
    {
      this.Context = context;
      return this.NewDriveDynamicParameters();
    }

    internal PSDriveInfo RemoveDrive(PSDriveInfo drive, CmdletProviderContext context)
    {
      this.Context = context;
      return this.RemoveDrive(drive);
    }

    internal Collection<PSDriveInfo> InitializeDefaultDrives(
      CmdletProviderContext context)
    {
      this.Context = context;
      this.Context.Drive = (PSDriveInfo) null;
      return this.InitializeDefaultDrives();
    }

    protected virtual PSDriveInfo NewDrive(PSDriveInfo drive)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return drive;
    }

    protected virtual object NewDriveDynamicParameters()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    protected virtual PSDriveInfo RemoveDrive(PSDriveInfo drive)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return drive;
    }

    protected virtual Collection<PSDriveInfo> InitializeDefaultDrives()
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return new Collection<PSDriveInfo>();
    }
  }
}
