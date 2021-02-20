// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RemotingCommandUtil
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
  internal static class RemotingCommandUtil
  {
    [TraceSource("RCU", "RemotingCommandUtil")]
    private static readonly PSTraceSource _trace = PSTraceSource.GetTracer("RCU", nameof (RemotingCommandUtil));

    internal static bool HasRepeatingRunspaces(PSSession[] runspaceInfos)
    {
      using (RemotingCommandUtil._trace.TraceMethod())
      {
        if (runspaceInfos == null)
          throw RemotingCommandUtil._trace.NewArgumentNullException(nameof (runspaceInfos));
        if (runspaceInfos.GetLength(0) == 0)
          throw RemotingCommandUtil._trace.NewArgumentException(nameof (runspaceInfos));
        for (int index1 = 0; index1 < runspaceInfos.GetLength(0); ++index1)
        {
          for (int index2 = 0; index2 < runspaceInfos.GetLength(0); ++index2)
          {
            if (index1 != index2 && runspaceInfos[index1].Runspace.InstanceId == runspaceInfos[index2].Runspace.InstanceId)
              return true;
          }
        }
        return false;
      }
    }

    internal static bool ExceedMaximumAllowableRunspaces(PSSession[] runspaceInfos)
    {
      using (RemotingCommandUtil._trace.TraceMethod())
      {
        if (runspaceInfos == null)
          throw RemotingCommandUtil._trace.NewArgumentNullException(nameof (runspaceInfos));
        if (runspaceInfos.GetLength(0) == 0)
          throw RemotingCommandUtil._trace.NewArgumentException(nameof (runspaceInfos));
        return false;
      }
    }

    internal static void CheckRemotingCmdletPrerequisites()
    {
      bool flag = true;
      string name = "Software\\Microsoft\\Windows\\CurrentVersion\\WSMAN\\";
      try
      {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
        if (registryKey != null)
        {
          if (((string) registryKey.GetValue("StackVersion", (object) string.Empty)).Trim().Equals("2.0", StringComparison.OrdinalIgnoreCase))
            flag = false;
        }
      }
      catch (ArgumentException ex)
      {
        flag = true;
      }
      catch (SecurityException ex)
      {
        flag = true;
      }
      catch (ObjectDisposedException ex)
      {
        flag = true;
      }
      if (flag)
        throw new InvalidOperationException("Windows PowerShell remoting features are not enabled or not supported on this machine.\nThis may be because you do not have the correct version of WS-Management installed or this version of Windows does not support remoting currently.\n For more information, type 'get-help about_remote_requirements'.");
    }
  }
}
