// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.TransactedRegistryAccessRule
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Security.AccessControl;
using System.Security.Principal;

namespace Microsoft.PowerShell.Commands.Internal
{
  public sealed class TransactedRegistryAccessRule : AccessRule
  {
    internal TransactedRegistryAccessRule(
      IdentityReference identity,
      RegistryRights registryRights,
      AccessControlType type)
      : this(identity, (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
    {
    }

    internal TransactedRegistryAccessRule(
      string identity,
      RegistryRights registryRights,
      AccessControlType type)
      : this((IdentityReference) new NTAccount(identity), (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
    {
    }

    public TransactedRegistryAccessRule(
      IdentityReference identity,
      RegistryRights registryRights,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AccessControlType type)
      : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, type)
    {
    }

    internal TransactedRegistryAccessRule(
      string identity,
      RegistryRights registryRights,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AccessControlType type)
      : this((IdentityReference) new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, type)
    {
    }

    internal TransactedRegistryAccessRule(
      IdentityReference identity,
      int accessMask,
      bool isInherited,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AccessControlType type)
      : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
    {
    }

    public RegistryRights RegistryRights => (RegistryRights) this.AccessMask;
  }
}
