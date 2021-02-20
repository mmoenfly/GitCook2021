// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TraceSourceAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
  internal class TraceSourceAttribute : Attribute
  {
    private string category;
    private string description;

    internal TraceSourceAttribute(string category, string description)
    {
      this.category = category;
      this.description = description;
    }

    internal string Category => this.category;

    internal string Description
    {
      get => this.description;
      set => this.description = value;
    }
  }
}
