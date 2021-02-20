// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.CommandParameter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Runspaces
{
  public sealed class CommandParameter
  {
    private string _name;
    private object _value;
    [TraceSource("Parameter", "Simple name/value pair")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("Parameter", "Simple name/value pair");

    public CommandParameter(string name)
      : this(name, (object) null)
    {
      using (CommandParameter._trace.TraceMethod())
      {
        if (name == null)
          throw CommandParameter._trace.NewArgumentNullException(nameof (name));
      }
    }

    public CommandParameter(string name, object value)
    {
      using (CommandParameter._trace.TraceConstructor((object) this))
      {
        if (name != null)
          this._name = name.Trim().Length != 0 ? name : throw CommandParameter._trace.NewArgumentException(nameof (name));
        else
          this._name = name;
        this._value = value;
      }
    }

    public string Name
    {
      get
      {
        using (CommandParameter._trace.TraceProperty())
          return this._name;
      }
    }

    public object Value
    {
      get
      {
        using (CommandParameter._trace.TraceProperty())
          return this._value;
      }
    }

    internal static CommandParameter FromCommandParameterInternal(
      CommandParameterInternal internalParameter)
    {
      if (internalParameter == null)
        throw CommandParameter._trace.NewArgumentNullException(nameof (internalParameter));
      string name = (string) null;
      if (internalParameter.Name != null)
        name = internalParameter.Token == null ? internalParameter.Name : (!internalParameter.Token.FollowedBySpace ? internalParameter.Token.ToString() : internalParameter.Token.ToString() + " ");
      if (internalParameter.IsValidPair)
      {
        object obj = internalParameter.Value2;
        return new CommandParameter(name, obj);
      }
      return name != null ? new CommandParameter(name) : new CommandParameter((string) null, internalParameter.Value1);
    }

    internal static CommandParameterInternal ToCommandParameterInternal(
      CommandParameter publicParameter)
    {
      string str = publicParameter != null ? publicParameter.Name : throw CommandParameter._trace.NewArgumentNullException(nameof (publicParameter));
      object obj = publicParameter.Value;
      if (str == null)
        return new CommandParameterInternal(obj);
      if (str[0] != '-')
        return new CommandParameterInternal(str, obj);
      int length = str.Length;
      while (length > 0 && char.IsWhiteSpace(str[length - 1]))
        --length;
      string text = str.Substring(0, length);
      Token token = new Token(text, TokenId.ParameterToken);
      token.SetPosition((string) null, str, 0, length, (Tokenizer) null);
      token.Data = str[length - 1] != ':' ? (object) text.Substring(1, text.Length - 1) : (object) text.Substring(1, text.Length - 2);
      return str[length - 1] != ':' && obj == null ? new CommandParameterInternal((object) token) : new CommandParameterInternal(token, obj);
    }

    internal static CommandParameter FromPSObjectForRemoting(
      PSObject parameterAsPSObject)
    {
      return parameterAsPSObject != null ? new CommandParameter(RemotingDecoder.GetPropertyValue<string>(parameterAsPSObject, "N"), RemotingDecoder.GetPropertyValue<object>(parameterAsPSObject, "V")) : throw CommandParameter._trace.NewArgumentNullException(nameof (parameterAsPSObject));
    }

    internal PSObject ToPSObjectForRemoting()
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("N", (object) this.Name));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("V", this.Value));
      return emptyPsObject;
    }
  }
}
