// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SerializationOptions
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [System.Flags]
  internal enum SerializationOptions
  {
    None = 0,
    UseDepthFromTypes = 1,
    NoRootElement = 2,
    NoNamespace = 4,
    NoObjectRefIds = 8,
    PreserveSerializationSettingOfOriginal = 16, // 0x00000010
    RemotingOptions = PreserveSerializationSettingOfOriginal | NoNamespace | NoRootElement | UseDepthFromTypes, // 0x00000017
  }
}
