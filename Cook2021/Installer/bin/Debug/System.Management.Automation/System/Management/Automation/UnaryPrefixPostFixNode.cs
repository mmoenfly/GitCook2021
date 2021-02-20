// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.UnaryPrefixPostFixNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class UnaryPrefixPostFixNode : ParseTreeNode
  {
    private ParseTreeNode _expression;
    private readonly ReadOnlyCollection<Token> _unaries;
    private readonly Token _prefix;
    private readonly Token _postfix;
    private readonly int valueToAdd;

    public UnaryPrefixPostFixNode(
      ParseTreeNode expression,
      List<Token> preOperators,
      Token postfix)
    {
      this._expression = expression;
      this._postfix = postfix;
      if (preOperators != null && preOperators.Count > 0)
      {
        this.NodeToken = preOperators[0];
        if (preOperators[preOperators.Count - 1].IsPrePostFix())
        {
          this._prefix = preOperators[preOperators.Count - 1];
          preOperators.RemoveAt(preOperators.Count - 1);
        }
      }
      else
        this.NodeToken = this._postfix;
      this.Normalize(preOperators);
      this._unaries = new ReadOnlyCollection<Token>((IList<Token>) preOperators);
      if ((this._postfix != null || this._prefix != null) && (this._unaries == null || this._unaries.Count == 0))
        this.IsVoidable = true;
      Token token = this._prefix != null ? this._prefix : this._postfix;
      if (token == null)
        return;
      if (string.Equals(token.TokenText, "++", StringComparison.Ordinal))
        this.valueToAdd = 1;
      else
        this.valueToAdd = -1;
    }

    internal ParseTreeNode Expression => this._expression;

    internal bool Deprecated => (this._unaries == null || this._unaries.Count == 0) && this._prefix == null && this._postfix == null;

    internal void RestrictedLanguageCheck(Parser parser)
    {
      if (this._prefix != null)
        parser.ReportException((object) this._prefix, typeof (ParseException), this._prefix, "OperatorNotSupportedInDataSection", (object) this._prefix.TokenText);
      if (this._postfix != null)
        parser.ReportException((object) this._postfix, typeof (ParseException), this._postfix, "OperatorNotSupportedInDataSection", (object) this._postfix.TokenText);
      foreach (Token unary in this._unaries)
      {
        OperatorToken operatorToken = unary as OperatorToken;
        bool flag = true;
        if (operatorToken != null && !operatorToken.IsValidInRestrictedLanguage)
          flag = false;
        else if (unary.TokenId == TokenId.TypeToken)
        {
          try
          {
            Type type = Parser.ConvertTo<Type>((object) unary.TokenText, unary);
            if ((!type.IsArray ? Type.GetTypeCode(type) : Type.GetTypeCode(type.GetElementType())) == TypeCode.Object)
              flag = false;
          }
          catch
          {
            flag = false;
          }
        }
        if (!flag)
          parser.ReportException((object) unary, typeof (ParseException), unary, "OperatorNotSupportedInDataSection", (object) unary.FullText);
      }
    }

    internal static bool ValidateOperatorSequence(
      Parser parser,
      ParseTreeNode expression,
      List<Token> preOperators,
      Token postfix)
    {
      bool flag1 = expression is IAssignableParseTreeNode && !(expression is AssignableArrayLiteralNode);
      bool flag2 = false;
      int num = 0;
      for (int index = 0; index <= preOperators.Count - 1; ++index)
      {
        Token preOperator = preOperators[index];
        if (preOperator.IsPrePostFix())
        {
          if (!flag1 || index != preOperators.Count - 1)
          {
            parser?.ReportException((object) preOperator.TokenText, typeof (ParseException), preOperator, "OperatorRequiresVariableOrProperty", (object) preOperator.TokenText);
            return false;
          }
          flag2 = true;
        }
        else if (preOperator.Is(TokenId.TypeToken))
        {
          if (string.Compare(preOperator.TokenText, "ref", StringComparison.OrdinalIgnoreCase) == 0)
            ++num;
          if (num > 1)
            parser.ReportException((object) preOperator, typeof (ParseException), preOperator, "ReferenceNeedsToBeByItselfInTypeSequence");
        }
      }
      if (postfix == null || flag1 && !flag2)
        return true;
      parser?.ReportException((object) postfix.TokenText, typeof (ParseException), postfix, "OperatorRequiresVariableOrProperty", (object) postfix.TokenText);
      return false;
    }

    private void Normalize(List<Token> preOperators)
    {
      this.ApplyTypeConstraints(preOperators);
      this.ReduceConstantValues(preOperators);
    }

    private void ApplyTypeConstraints(List<Token> preOperators)
    {
      if (!(this._expression is IAssignableParseTreeNode expression) || this._prefix != null || this._postfix != null)
        return;
      for (int index = preOperators.Count - 1; index >= 0 && preOperators[index].Is(TokenId.TypeToken); --index)
      {
        TypeLiteral typeLiteral = new TypeLiteral(preOperators[index]);
        expression.TypeConstraint.Add(typeLiteral);
        preOperators.RemoveAt(index);
      }
    }

    private void ReduceConstantValues(List<Token> preOperators)
    {
      if (this._prefix != null || this._postfix != null)
        return;
      while (preOperators.Count > 0 && preOperators[preOperators.Count - 1].Is(TokenId.CommaToken) && this._expression is IAssignableParseTreeNode)
      {
        this._expression = (ParseTreeNode) new AssignableArrayLiteralNode(preOperators[preOperators.Count - 1], new List<ParseTreeNode>(1)
        {
          this._expression
        });
        preOperators.RemoveAt(preOperators.Count - 1);
      }
      if (!this._expression.IsConstant)
        return;
      object operand = PSObject.Base(this._expression.GetConstValue());
      if (operand is IEnumerable)
        return;
      try
      {
        for (int index = preOperators.Count - 1; index >= 0 && !preOperators[index].Is(TokenId.TypeToken); --index)
          operand = this.ExecuteUnary(operand, preOperators[index], (ExecutionContext) null);
        this._expression = (ParseTreeNode) new ConstantNode(this.NodeToken, operand);
        for (int index = preOperators.Count - 1; index >= 0 && !preOperators[index].Is(TokenId.TypeToken); --index)
          preOperators.RemoveAt(index);
      }
      catch (RuntimeException ex)
      {
      }
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object operand = this.ExecutePrefixPostFix(input, outputPipe, context) ?? this._expression.Execute(input, (Pipe) null, context);
      for (int index = this._unaries.Count - 1; index >= 0; --index)
        operand = this.ExecuteUnary(operand, this._unaries[index], context);
      return operand;
    }

    private object ExecutePrefixPostFix(Array input, Pipe outputPipe, ExecutionContext context)
    {
      if (this._prefix == null && this._postfix == null)
        return (object) null;
      Token errToken = this._prefix != null ? this._prefix : this._postfix;
      if (!(this._expression is IAssignableParseTreeNode expression))
        throw InterpreterError.NewInterpreterException((object) errToken.TokenText, typeof (RuntimeException), errToken, "OperatorRequiresVariableOrProperty", (object) errToken.TokenText);
      IAssignableValue assignableValue = expression.GetAssignableValue(input, context);
      object obj1 = PSObject.Base(assignableValue.GetValue(context)) ?? (object) 0;
      Type type = obj1.GetType();
      TypeCode typeCode = LanguagePrimitives.GetTypeCode(type);
      if (!LanguagePrimitives.IsNumeric(typeCode))
        throw InterpreterError.NewInterpreterException(obj1, typeof (RuntimeException), errToken, "OperatorRequiresNumber", (object) errToken, (object) type);
      object obj2;
      if (typeCode == TypeCode.Int32)
      {
        try
        {
          obj2 = (object) checked ((int) obj1 + this.valueToAdd);
        }
        catch
        {
          obj2 = ParserOps.PolyAdd(context, this.NodeToken, obj1, (object) this.valueToAdd);
        }
      }
      else
        obj2 = ParserOps.PolyAdd(context, this.NodeToken, obj1, (object) this.valueToAdd);
      assignableValue.SetValue(obj2, context);
      return this._prefix != null ? obj2 : obj1;
    }

    private object ExecuteUnary(object operand, Token op, ExecutionContext context)
    {
      if (op.Is(TokenId.TypeToken))
        return this.ExecuteConversion(operand, op);
      switch (op.TokenText)
      {
        case "-":
          return ParserOps.PolyMinus(context, op, (object) 0, operand);
        case "+":
          return ParserOps.PolyAdd(context, op, (object) 0, operand);
        case "!":
        case "-not":
          return (object) !LanguagePrimitives.IsTrue(operand);
        case "-bnot":
          try
          {
            return (object) ~Parser.ConvertTo<int>(operand, op);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            return (object) ~Parser.ConvertTo<long>(operand, op);
          }
        case ",":
          return (object) new object[1]{ operand };
        case "-split":
          return ParserOps.SplitOperator(context, op, operand);
        case "-join":
          return ParserOps.JoinOperator(context, op, operand);
        default:
          throw InterpreterError.NewInterpreterException((object) op, typeof (RuntimeException), op, "UnexpectedUnaryOperator", (object) op);
      }
    }

    private object ExecuteConversion(object operand, Token op)
    {
      Type type = new TypeLiteral(op).Type;
      if (!type.Equals(typeof (void)))
        return Parser.ConvertTo(operand, type, op);
      this.IsVoidable = true;
      return (object) AutomationNull.Value;
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._expression.Accept(visitor);
    }
  }
}
