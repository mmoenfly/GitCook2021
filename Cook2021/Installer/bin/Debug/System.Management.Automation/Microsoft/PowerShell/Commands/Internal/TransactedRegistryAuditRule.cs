// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.TransactedRegistryAuditRule
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Security.AccessControl;
using System.Security.Principal;

namespace Microsoft.PowerShell.Commands.Internal
{
  public sealed class TransactedRegistryAuditRule : AuditRule
  {
    internal TransactedRegistryAuditRule(
      IdentityReference identity,
      RegistryRights registryRights,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AuditFlags flags)
      : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, flags)
    {
    }

    internal TransactedRegistryAuditRule(
      string identity,
      RegistryRights registryRights,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AuditFlags flags)
      : this((IdentityReference) new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, flags)
    {
    }

    internal TransactedRegistryAuditRule(
      IdentityReference identity,
      int accessMask,
      bool isInherited,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AuditFlags flags)
      : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
    {
    }

    public RegistryRights RegistryRights => (RegistryRights) this.AccessMask;
  }
}
