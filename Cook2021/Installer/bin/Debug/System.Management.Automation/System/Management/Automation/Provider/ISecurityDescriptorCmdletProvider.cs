// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.ISecurityDescriptorCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Security.AccessControl;

namespace System.Management.Automation.Provider
{
  public interface ISecurityDescriptorCmdletProvider
  {
    void GetSecurityDescriptor(string path, AccessControlSections includeSections);

    void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor);

    ObjectSecurity NewSecurityDescriptorFromPath(
      string path,
      AccessControlSections includeSections);

    ObjectSecurity NewSecurityDescriptorOfType(
      string type,
      AccessControlSections includeSections);
  }
}
