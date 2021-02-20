// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemotingDestination
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [System.Flags]
  internal enum RemotingDestination : uint
  {
    InvalidDestination = 0,
    Client = 1,
    Server = 2,
    Listener = 4,
  }
}
