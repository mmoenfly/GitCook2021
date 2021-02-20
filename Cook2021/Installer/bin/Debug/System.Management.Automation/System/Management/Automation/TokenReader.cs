// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TokenReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal abstract class TokenReader
  {
    protected Tokenizer _tokenizer;
    private bool _enabled = true;

    protected TokenReader(Tokenizer tokenizer) => this._tokenizer = tokenizer;

    internal Tokenizer Tokenizer => this._tokenizer;

    internal bool Enabled
    {
      get => this._enabled;
      set => this._enabled = value;
    }

    internal abstract TokenClass TokenClass { get; }

    internal abstract Token GetToken(string input, ref int offset);
  }
}
