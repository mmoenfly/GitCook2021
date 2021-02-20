// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSTypeName
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public class PSTypeName
  {
    private string _name;
    private Type _type;

    public PSTypeName(Type type)
    {
      this._type = type;
      if (this._type == null)
        return;
      this._name = this._type.FullName;
    }

    public PSTypeName(string name)
    {
      this._name = name;
      this._type = (Type) null;
    }

    public string Name => this._name;

    public Type Type
    {
      get
      {
        if (this._type == null)
          this._type = LanguagePrimitives.ConvertStringToType(this._name, out Exception _);
        return this._type;
      }
    }

    public override string ToString() => this._name ?? string.Empty;
  }
}
