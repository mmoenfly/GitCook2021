// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RuntimeDefinedParameterDictionary
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  [Serializable]
  public class RuntimeDefinedParameterDictionary : Dictionary<string, RuntimeDefinedParameter>
  {
    [TraceSource("RuntimeDefinedParameters", "The classes representing the runtime-defined parameters")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("RuntimeDefinedParameters", "The classes representing the runtime-defined parameters");
    private string helpFile = string.Empty;
    private object data;

    public RuntimeDefinedParameterDictionary()
      : base((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
    {
    }

    public string HelpFile
    {
      get => this.helpFile;
      set
      {
        if (string.IsNullOrEmpty(value))
          this.helpFile = string.Empty;
        else
          this.helpFile = value;
      }
    }

    public object Data
    {
      get => this.data;
      set => this.data = value;
    }
  }
}
