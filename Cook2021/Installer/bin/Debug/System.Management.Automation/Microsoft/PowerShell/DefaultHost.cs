// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.DefaultHost
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell
{
  internal class DefaultHost : PSHost
  {
    private CultureInfo currentCulture;
    private CultureInfo currentUICulture;
    private Guid id = Guid.NewGuid();
    private Version ver = PSVersionInfo.PSVersion;
    [TraceSource("DefaultHost", "DefaultHost subclass of S.M.A.PSHost Tracer")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (DefaultHost), "DefaultHost subclass of S.M.A.PSHost Tracer");

    internal DefaultHost(CultureInfo currentCulture, CultureInfo currentUICulture)
    {
      this.currentCulture = currentCulture;
      this.currentUICulture = currentUICulture;
    }

    public override string Name
    {
      get
      {
        using (DefaultHost.tracer.TraceProperty("Default Host", new object[0]))
          return "Default Host";
      }
    }

    public override Version Version
    {
      get
      {
        using (DefaultHost.tracer.TraceProperty((object) this.ver))
          return this.ver;
      }
    }

    public override Guid InstanceId
    {
      get
      {
        using (DefaultHost.tracer.TraceProperty((object) this.id))
          return this.id;
      }
    }

    public override PSHostUserInterface UI
    {
      get
      {
        using (DefaultHost.tracer.TraceProperty())
          return (PSHostUserInterface) null;
      }
    }

    public override CultureInfo CurrentCulture
    {
      get
      {
        using (DefaultHost.tracer.TraceProperty((object) this.currentCulture))
          return this.currentCulture;
      }
    }

    public override CultureInfo CurrentUICulture
    {
      get
      {
        using (DefaultHost.tracer.TraceProperty((object) this.currentUICulture))
          return this.currentUICulture;
      }
    }

    public override void SetShouldExit(int exitCode)
    {
      using (DefaultHost.tracer.TraceMethod((object) exitCode))
        ;
    }

    public override void EnterNestedPrompt()
    {
      using (DefaultHost.tracer.TraceMethod())
        throw DefaultHost.tracer.NewNotSupportedException();
    }

    public override void ExitNestedPrompt()
    {
      using (DefaultHost.tracer.TraceMethod())
        throw DefaultHost.tracer.NewNotSupportedException();
    }

    public override void NotifyBeginApplication()
    {
      using (DefaultHost.tracer.TraceMethod())
        ;
    }

    public override void NotifyEndApplication()
    {
      using (DefaultHost.tracer.TraceMethod())
        ;
    }
  }
}
