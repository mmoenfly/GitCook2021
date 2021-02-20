// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.PSHost
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation.Host
{
  public abstract class PSHost
  {
    internal const int MaximumNestedPromptLevel = 128;
    private bool shouldSetThreadUILanguageToZero;

    public abstract string Name { get; }

    public abstract Version Version { get; }

    public abstract Guid InstanceId { get; }

    public abstract PSHostUserInterface UI { get; }

    public abstract CultureInfo CurrentCulture { get; }

    public abstract CultureInfo CurrentUICulture { get; }

    public abstract void SetShouldExit(int exitCode);

    public abstract void EnterNestedPrompt();

    public abstract void ExitNestedPrompt();

    public virtual PSObject PrivateData => (PSObject) null;

    public abstract void NotifyBeginApplication();

    public abstract void NotifyEndApplication();

    internal bool ShouldSetThreadUILanguageToZero
    {
      get => this.shouldSetThreadUILanguageToZero;
      set => this.shouldSetThreadUILanguageToZero = value;
    }
  }
}
