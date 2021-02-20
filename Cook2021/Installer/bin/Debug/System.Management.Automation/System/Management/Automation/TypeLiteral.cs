// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TypeLiteral
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class TypeLiteral
  {
    private Token _typeName;
    private Type _type;
    private bool _isRef;
    private bool _isSwitchParameter;

    public TypeLiteral(Token typeName)
    {
      this._typeName = typeName;
      this._isRef = typeName.TokenText.Equals("ref", StringComparison.OrdinalIgnoreCase);
    }

    public Type Type => this._type == null ? this.resolveType() : this._type;

    internal Token Token => this._typeName;

    private Type resolveType()
    {
      Exception exception = (Exception) null;
      Type type = LanguagePrimitives.ConvertStringToType(this._typeName.TokenText, out exception);
      if (type != null)
      {
        this._type = type;
        this._isSwitchParameter = this._type.Equals(typeof (SwitchParameter));
        return this._type;
      }
      if (exception != null)
        throw InterpreterError.NewInterpreterExceptionWithInnerException((object) this._typeName.TokenText, typeof (RuntimeException), this._typeName, "TypeNotFoundWithMessage", exception, (object) this._typeName.TokenText, (object) exception.Message);
      throw InterpreterError.NewInterpreterException((object) this._typeName.TokenText, typeof (RuntimeException), this._typeName, "TypeNotFound", (object) this._typeName.TokenText);
    }

    internal bool IsRef => this._isRef;

    internal bool IsSwitchParameter
    {
      get
      {
        if (this._type == null)
          this.resolveType();
        return this._isSwitchParameter;
      }
    }
  }
}
