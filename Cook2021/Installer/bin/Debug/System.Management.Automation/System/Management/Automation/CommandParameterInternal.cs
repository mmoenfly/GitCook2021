// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandParameterInternal
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal sealed class CommandParameterInternal
  {
    [TraceSource("CommandParameterInternal", "Internal definition of a parameter")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandParameterInternal), "Internal definition of a parameter");
    private string name;
    private bool isValidPair;
    private object value1 = (object) UnboundParameter.Value;
    private object value2 = (object) UnboundParameter.Value;
    private bool _treatLikeToken;

    internal string Name
    {
      get => this.name;
      set => this.name = value;
    }

    internal bool IsValidPair
    {
      get => this.isValidPair;
      set => this.isValidPair = value;
    }

    internal object Value1
    {
      get => this.value1;
      set => this.value1 = value;
    }

    internal Token Token => this.value1 as Token;

    internal object Value2
    {
      get => this.value2;
      set => this.value2 = value;
    }

    internal bool TreatLikeToken => this._treatLikeToken;

    internal CommandParameterInternal(Token token, object value)
    {
      using (CommandParameterInternal.tracer.TraceConstructor((object) this))
      {
        this.name = token != null ? (string) token.Data : throw CommandParameterInternal.tracer.NewArgumentNullException(nameof (token));
        this.value1 = (object) token;
        this.isValidPair = true;
        this.value2 = value;
      }
    }

    internal CommandParameterInternal(object value)
      : this(value, false)
    {
    }

    internal CommandParameterInternal(object value, bool treatLikeToken)
    {
      using (CommandParameterInternal.tracer.TraceConstructor((object) this))
      {
        this.value1 = value;
        if (value is Token token)
        {
          this.name = token.Data as string;
          this._treatLikeToken = true;
        }
        else
        {
          if (!treatLikeToken)
            return;
          this.name = value as string;
          if (this.name == null)
            return;
          this._treatLikeToken = true;
        }
      }
    }

    internal CommandParameterInternal(string name, object value)
    {
      if (string.IsNullOrEmpty(name))
        throw CommandParameterInternal.tracer.NewArgumentException(nameof (name));
      this.isValidPair = true;
      this.value1 = (object) name;
      this.value2 = value;
      if (SpecialCharacters.IsDash(name[0]))
        name = name[name.Length - 1] != ':' ? name.Substring(1) : name.Substring(1, name.Length - 2);
      this.name = name;
    }
  }
}
