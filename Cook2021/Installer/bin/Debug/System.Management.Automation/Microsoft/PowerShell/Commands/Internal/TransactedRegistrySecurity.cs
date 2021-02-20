// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.TransactedRegistrySecurity
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace Microsoft.PowerShell.Commands.Internal
{
  public sealed class TransactedRegistrySecurity : NativeObjectSecurity
  {
    private const string resBaseName = "RegistryProviderStrings";

    public TransactedRegistrySecurity()
      : base(true, ResourceType.RegistryKey)
    {
    }

    [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
    internal TransactedRegistrySecurity(
      SafeRegistryHandle hKey,
      string name,
      AccessControlSections includeSections)
      : base(true, ResourceType.RegistryKey, (SafeHandle) hKey, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(TransactedRegistrySecurity._HandleErrorCode), (object) null)
    {
      new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.View, name).Demand();
    }

    private static Exception _HandleErrorCode(
      int errorCode,
      string name,
      SafeHandle handle,
      object context)
    {
      Exception exception = (Exception) null;
      switch (errorCode)
      {
        case 2:
          exception = (Exception) new IOException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegKeyNotFound"));
          break;
        case 6:
          exception = (Exception) new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "AccessControl_InvalidHandle"));
          break;
        case 123:
          exception = (Exception) new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegInvalidKeyName"));
          break;
      }
      return exception;
    }

    public override AccessRule AccessRuleFactory(
      IdentityReference identityReference,
      int accessMask,
      bool isInherited,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AccessControlType type)
    {
      return (AccessRule) new TransactedRegistryAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
    }

    public override AuditRule AuditRuleFactory(
      IdentityReference identityReference,
      int accessMask,
      bool isInherited,
      InheritanceFlags inheritanceFlags,
      PropagationFlags propagationFlags,
      AuditFlags flags)
    {
      return (AuditRule) new TransactedRegistryAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
    }

    internal AccessControlSections GetAccessControlSectionsFromChanges()
    {
      AccessControlSections accessControlSections = AccessControlSections.None;
      if (this.AccessRulesModified)
        accessControlSections = AccessControlSections.Access;
      if (this.AuditRulesModified)
        accessControlSections |= AccessControlSections.Audit;
      if (this.OwnerModified)
        accessControlSections |= AccessControlSections.Owner;
      if (this.GroupModified)
        accessControlSections |= AccessControlSections.Group;
      return accessControlSections;
    }

    [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
    internal void Persist(SafeRegistryHandle hKey, string keyName)
    {
      new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.Change, keyName).Demand();
      this.WriteLock();
      try
      {
        AccessControlSections sectionsFromChanges = this.GetAccessControlSectionsFromChanges();
        if (sectionsFromChanges == AccessControlSections.None)
          return;
        this.Persist((SafeHandle) hKey, sectionsFromChanges);
        this.OwnerModified = this.GroupModified = this.AuditRulesModified = this.AccessRulesModified = false;
      }
      finally
      {
        this.WriteUnlock();
      }
    }

    public void AddAccessRule(TransactedRegistryAccessRule rule) => this.AddAccessRule((AccessRule) rule);

    public void SetAccessRule(TransactedRegistryAccessRule rule) => this.SetAccessRule((AccessRule) rule);

    public void ResetAccessRule(TransactedRegistryAccessRule rule) => this.ResetAccessRule((AccessRule) rule);

    public bool RemoveAccessRule(TransactedRegistryAccessRule rule) => this.RemoveAccessRule((AccessRule) rule);

    public void RemoveAccessRuleAll(TransactedRegistryAccessRule rule) => this.RemoveAccessRuleAll((AccessRule) rule);

    public void RemoveAccessRuleSpecific(TransactedRegistryAccessRule rule) => this.RemoveAccessRuleSpecific((AccessRule) rule);

    public void AddAuditRule(TransactedRegistryAuditRule rule) => this.AddAuditRule((AuditRule) rule);

    public void SetAuditRule(TransactedRegistryAuditRule rule) => this.SetAuditRule((AuditRule) rule);

    public bool RemoveAuditRule(TransactedRegistryAuditRule rule) => this.RemoveAuditRule((AuditRule) rule);

    public void RemoveAuditRuleAll(TransactedRegistryAuditRule rule) => this.RemoveAuditRuleAll((AuditRule) rule);

    public void RemoveAuditRuleSpecific(TransactedRegistryAuditRule rule) => this.RemoveAuditRuleSpecific((AuditRule) rule);

    public override Type AccessRightType => typeof (RegistryRights);

    public override Type AccessRuleType => typeof (TransactedRegistryAccessRule);

    public override Type AuditRuleType => typeof (TransactedRegistryAuditRule);
  }
}
