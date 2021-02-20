// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterDeclarationNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal sealed class ParameterDeclarationNode : ParseTreeNode
  {
    private readonly ReadOnlyCollection<ParameterNode> _parameters;
    private readonly string _source;
    private RuntimeDefinedParameterDictionary _runtimeDefinedParameters;
    private List<RuntimeDefinedParameter> _runtimeDefinedParameterList;

    internal ParameterDeclarationNode(Token token, List<ParameterNode> parameters, string source)
    {
      this.NodeToken = token;
      this._parameters = new ReadOnlyCollection<ParameterNode>((IList<ParameterNode>) parameters);
      this.IsVoidable = true;
      this._source = source;
    }

    public override string ToString() => this._source;

    internal RuntimeDefinedParameterDictionary RuntimeDefinedParameters => this._runtimeDefinedParameters;

    internal List<RuntimeDefinedParameter> RuntimeDefinedParameterList => this._runtimeDefinedParameterList;

    internal bool IsEmpty => this._parameters.Count == 0;

    internal static void Validate(Parser parser, List<ParameterNode> parameters)
    {
      Dictionary<string, bool> dictionary = new Dictionary<string, bool>((IEqualityComparer<string>) StringComparer.CurrentCultureIgnoreCase);
      foreach (ParameterNode parameter in parameters)
      {
        if (dictionary.ContainsKey(parameter.ParameterName))
          parser.ReportException((object) parameter.NodeToken, typeof (ParseException), parameter.NodeToken, "DuplicateFormalParameter", (object) parameter.ParameterName);
        else
          dictionary.Add(parameter.ParameterName, true);
      }
    }

    internal void InitializeRuntimeDefinedParameters(ref bool useCmdletBinding)
    {
      if (this._runtimeDefinedParameters != null)
        return;
      this._runtimeDefinedParameters = new RuntimeDefinedParameterDictionary();
      this._runtimeDefinedParameterList = new List<RuntimeDefinedParameter>();
      bool flag = false;
      if (!useCmdletBinding)
      {
        for (int index = 0; index < this._parameters.Count; ++index)
        {
          if (this._parameters[index].HasParameterAttribute())
          {
            useCmdletBinding = true;
            break;
          }
        }
      }
      int num1 = 0;
      for (int index = 0; index < this._parameters.Count; ++index)
      {
        RuntimeDefinedParameter definedParameter = this._parameters[index].GetRuntimeDefinedParameter(useCmdletBinding);
        if (!useCmdletBinding)
        {
          ParameterAttribute parameterAttribute = new ParameterAttribute();
          if (!this._parameters[index].IsSwitchParameter())
            parameterAttribute.Position = num1++;
          definedParameter.Attributes.Add((Attribute) parameterAttribute);
        }
        else
        {
          foreach (Attribute attribute in definedParameter.Attributes)
          {
            if (attribute is ParameterAttribute parameterAttribute && (parameterAttribute.Position != int.MinValue || !parameterAttribute.ParameterSetName.Equals("__AllParameterSets", StringComparison.OrdinalIgnoreCase)))
            {
              flag = true;
              break;
            }
          }
        }
        try
        {
          this._runtimeDefinedParameters.Add(definedParameter.Name, definedParameter);
          this._runtimeDefinedParameterList.Add(definedParameter);
        }
        catch (ArgumentException ex)
        {
          throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), this._parameters[index].NodeToken, "DuplicateFormalParameter", (object) this._parameters[index].NodeToken.TokenText);
        }
      }
      if (!useCmdletBinding || flag)
        return;
      for (int index = 0; index < this._runtimeDefinedParameterList.Count; ++index)
      {
        foreach (Attribute attribute in this._runtimeDefinedParameterList[index].Attributes)
        {
          if (attribute is ParameterAttribute parameterAttribute && !this._parameters[index].IsSwitchParameter())
          {
            int num2 = num1++;
            parameterAttribute.Position = num2;
            break;
          }
        }
      }
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      foreach (ParseTreeNode parameter in this._parameters)
        parameter.Accept(visitor);
    }
  }
}
