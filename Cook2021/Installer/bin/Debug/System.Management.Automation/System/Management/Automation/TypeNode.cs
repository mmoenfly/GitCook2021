// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TypeNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class TypeNode : ParseTreeNode
  {
    private Type _type;

    public TypeNode(Token token)
    {
      this.NodeToken = token;
      this.IsExpression = true;
      this.IsConstant = false;
      this.ValidAttributeArgument = true;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      return (object) this.ResolveType();
    }

    internal override object GetConstValue() => (object) this.ResolveType();

    private Type ResolveType()
    {
      if (this._type == null)
        this._type = new TypeLiteral(this.NodeToken).Type;
      return this._type;
    }

    internal override void Accept(ParseTreeVisitor visitor) => visitor.Visit(this);
  }
}
