// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CallStackFrame
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;

namespace System.Management.Automation
{
  public sealed class CallStackFrame
  {
    private InvocationInfo invocationInfo;
    private string scriptName;
    private int scriptLineNumber;

    internal CallStackFrame(InvocationInfo locationInfo, InvocationInfo invocationInfo)
    {
      this.invocationInfo = invocationInfo;
      this.scriptName = locationInfo?.ScriptName;
      this.scriptLineNumber = locationInfo != null ? locationInfo.ScriptLineNumber : 0;
    }

    public string ScriptName => this.scriptName;

    public int ScriptLineNumber => this.scriptLineNumber;

    public InvocationInfo InvocationInfo => this.invocationInfo;

    public string GetScriptLocation()
    {
      if (string.IsNullOrEmpty(this.ScriptName))
        return "prompt";
      return Debugger.FormatResourceString("LocationFormat", (object) Path.GetFileName(this.ScriptName), (object) this.ScriptLineNumber);
    }
  }
}
