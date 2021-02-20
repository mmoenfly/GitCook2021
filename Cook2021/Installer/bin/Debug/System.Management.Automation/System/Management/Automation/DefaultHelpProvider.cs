// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DefaultHelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class DefaultHelpProvider : HelpFileHelpProvider
  {
    [TraceSource("DefaultHelpProvider", "DefaultHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (DefaultHelpProvider), nameof (DefaultHelpProvider));

    internal DefaultHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (DefaultHelpProvider.tracer.TraceConstructor((object) this))
        ;
    }

    internal override string Name
    {
      get
      {
        using (DefaultHelpProvider.tracer.TraceProperty())
          return "Default Help Provider";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (DefaultHelpProvider.tracer.TraceProperty())
          return HelpCategory.DefaultHelp;
      }
    }

    internal override IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      using (DefaultHelpProvider.tracer.TraceMethod())
      {
        HelpRequest helpRequest1 = helpRequest.Clone();
        helpRequest1.Target = "default";
        return base.ExactMatchHelp(helpRequest1);
      }
    }
  }
}
