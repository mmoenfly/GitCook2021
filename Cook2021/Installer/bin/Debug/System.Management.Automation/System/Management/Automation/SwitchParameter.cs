// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SwitchParameter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public struct SwitchParameter
  {
    private bool isPresent;

    public bool IsPresent => this.isPresent;

    public static implicit operator bool(SwitchParameter switchParameter) => switchParameter.IsPresent;

    public static implicit operator SwitchParameter(bool value) => new SwitchParameter(value);

    public bool ToBool() => this.isPresent;

    public SwitchParameter(bool isPresent) => this.isPresent = isPresent;

    public static SwitchParameter Present => new SwitchParameter(true);

    public override bool Equals(object obj)
    {
      switch (obj)
      {
        case bool flag:
          return this.isPresent == flag;
        case SwitchParameter switchParameter:
          return this.isPresent == switchParameter.IsPresent;
        default:
          return false;
      }
    }

    public override int GetHashCode() => this.isPresent.GetHashCode();

    public static bool operator ==(SwitchParameter first, SwitchParameter second) => first.Equals((object) second);

    public static bool operator !=(SwitchParameter first, SwitchParameter second) => !first.Equals((object) second);

    public static bool operator ==(SwitchParameter first, bool second) => first.Equals((object) second);

    public static bool operator !=(SwitchParameter first, bool second) => !first.Equals((object) second);

    public static bool operator ==(bool first, SwitchParameter second) => first.Equals((bool) second);

    public static bool operator !=(bool first, SwitchParameter second) => !first.Equals((bool) second);

    public override string ToString() => this.isPresent.ToString();
  }
}
