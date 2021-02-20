// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FlowControlException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal abstract class FlowControlException : SystemException
  {
    protected Token nodeToken;
    protected object argument;
    protected string label;

    public Token NodeToken => this.nodeToken;

    public object Argument => this.argument;

    public string Label => this.label;

    internal void SetArgument(object Argument) => this.argument = Argument;
  }
}
