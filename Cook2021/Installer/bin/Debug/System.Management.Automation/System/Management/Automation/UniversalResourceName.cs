// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.UniversalResourceName
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal sealed class UniversalResourceName
  {
    [TraceSource("UniversalResourceName", "The namespace navigation tracer")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (UniversalResourceName), "The namespace navigation tracer");
    private string namespaceID;
    private string namespaceSpecificString;

    internal UniversalResourceName(string value)
    {
      int length = value != null ? value.IndexOf(":", StringComparison.Ordinal) : throw UniversalResourceName.tracer.NewArgumentNullException(nameof (value));
      if (length < 0)
      {
        this.namespaceID = string.Empty;
        this.namespaceSpecificString = value;
      }
      else if (length == 0)
      {
        this.namespaceID = string.Empty;
        this.namespaceSpecificString = value.Substring(1);
      }
      else
      {
        this.namespaceID = value.Substring(0, length);
        this.namespaceSpecificString = value.Substring(length + 1);
        if (this.namespaceSpecificString != null && this.namespaceSpecificString.Length != 0)
          return;
        this.namespaceSpecificString = string.Empty;
      }
    }

    internal UniversalResourceName()
    {
    }

    internal string NamespaceID
    {
      get => this.namespaceID;
      set => this.namespaceID = value;
    }

    internal string NamespaceSpecificString
    {
      get => this.namespaceSpecificString;
      set => this.namespaceSpecificString = value;
    }

    public override string ToString() => !string.IsNullOrEmpty(this.namespaceID) ? this.namespaceID + ":" + this.namespaceSpecificString : this.namespaceSpecificString;
  }
}
