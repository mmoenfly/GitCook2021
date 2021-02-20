// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ComparisonToken
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class ComparisonToken : OperatorToken
  {
    private string _comparisonName;
    private bool _ignoreCase;

    internal ComparisonToken(
      string text,
      TokenId id,
      int precedence,
      string comparisonName,
      bool ignoreCase,
      PowerShellBinaryOperator operationDelegate)
      : base(text, id, precedence, operationDelegate)
    {
      this._comparisonName = comparisonName;
      this._ignoreCase = ignoreCase;
      this.BindDelegates(comparisonName, id);
    }

    internal ComparisonToken(
      string text,
      TokenId id,
      int precedence,
      string comparisonName,
      bool ignoreCase)
      : base(text, id, precedence)
    {
      this._comparisonName = comparisonName;
      this._ignoreCase = ignoreCase;
      this.BindDelegates(comparisonName, id);
    }

    internal string ComparisonName => this._comparisonName;

    internal bool IgnoreCase => this._ignoreCase;

    public override Token Clone() => (Token) new ComparisonToken(this.TokenText, this.TokenId, this.Precedence, this._comparisonName, this._ignoreCase, this.OperationDelegate);

    internal override bool IsValidInRestrictedLanguage => !this.ComparisonName.Equals("-match", StringComparison.OrdinalIgnoreCase) && !this.ComparisonName.Equals("-notmatch", StringComparison.OrdinalIgnoreCase) && (!this.ComparisonName.Equals("-join", StringComparison.OrdinalIgnoreCase) && !this.ComparisonName.Equals("-split", StringComparison.OrdinalIgnoreCase)) && (!this.ComparisonName.Equals("-replace", StringComparison.OrdinalIgnoreCase) && !this.ComparisonName.Equals("-as", StringComparison.OrdinalIgnoreCase));

    internal override bool IsUnaryOperator() => this.ComparisonName.Equals("-split", StringComparison.OrdinalIgnoreCase) || this.ComparisonName.Equals("-join", StringComparison.OrdinalIgnoreCase);
  }
}
