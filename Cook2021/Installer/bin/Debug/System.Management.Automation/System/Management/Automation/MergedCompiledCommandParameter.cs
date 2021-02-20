// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MergedCompiledCommandParameter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class MergedCompiledCommandParameter
  {
    private CompiledCommandParameter parameter;
    private ParameterBinderAssociation binderAssociation;

    internal MergedCompiledCommandParameter(
      CompiledCommandParameter parameter,
      ParameterBinderAssociation binderAssociation)
    {
      this.parameter = parameter != null ? parameter : throw new ArgumentNullException(nameof (parameter));
      this.binderAssociation = binderAssociation;
    }

    internal CompiledCommandParameter Parameter => this.parameter;

    internal ParameterBinderAssociation BinderAssociation => this.binderAssociation;

    public override string ToString() => this.Parameter.ToString();
  }
}
