// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.MshExpression
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal class MshExpression
  {
    [TraceSource("MshExpression", "MshExpression")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (MshExpression), nameof (MshExpression));
    private string _stringValue;
    private ScriptBlock _script;
    private bool _isResolved;

    internal MshExpression(string s)
      : this(s, false)
    {
    }

    internal MshExpression(string s, bool isResolved)
    {
      this._stringValue = !string.IsNullOrEmpty(s) ? s : throw MshExpression.tracer.NewArgumentNullException(nameof (s));
      this._isResolved = isResolved;
    }

    internal MshExpression(ScriptBlock scriptBlock) => this._script = scriptBlock != null ? scriptBlock : throw MshExpression.tracer.NewArgumentNullException(nameof (scriptBlock));

    public ScriptBlock Script => this._script;

    public override string ToString() => this._script != null ? this._script.ToString() : this._stringValue;

    internal List<MshExpression> ResolveNames(PSObject target) => this.ResolveNames(target, true);

    internal bool HasWildCardCharacters => this._script == null && WildcardPattern.ContainsWildcardCharacters(this._stringValue);

    internal List<MshExpression> ResolveNames(PSObject target, bool expand)
    {
      List<MshExpression> mshExpressionList = new List<MshExpression>();
      if (this._isResolved)
      {
        mshExpressionList.Add(this);
        return mshExpressionList;
      }
      if (this._script != null)
      {
        mshExpressionList.Add(new MshExpression(this._script)
        {
          _isResolved = true
        });
        return mshExpressionList;
      }
      IEnumerable<PSMemberInfo> psMemberInfos;
      if (this.HasWildCardCharacters)
      {
        psMemberInfos = (IEnumerable<PSMemberInfo>) target.Members.Match(this._stringValue, PSMemberTypes.Properties | PSMemberTypes.PropertySet);
      }
      else
      {
        PSMemberInfo member = target.Members[this._stringValue];
        List<PSMemberInfo> psMemberInfoList = new List<PSMemberInfo>();
        if (member != null)
          psMemberInfoList.Add(member);
        psMemberInfos = (IEnumerable<PSMemberInfo>) psMemberInfoList;
      }
      List<PSMemberInfo> psMemberInfoList1 = new List<PSMemberInfo>();
      foreach (PSMemberInfo psMemberInfo in psMemberInfos)
      {
        if (psMemberInfo is PSPropertySet psPropertySet)
        {
          if (expand)
          {
            Collection<string> referencedPropertyNames = psPropertySet.ReferencedPropertyNames;
            for (int index1 = 0; index1 < referencedPropertyNames.Count; ++index1)
            {
              ReadOnlyPSMemberInfoCollection<PSPropertyInfo> memberInfoCollection = target.Properties.Match(referencedPropertyNames[index1]);
              for (int index2 = 0; index2 < memberInfoCollection.Count; ++index2)
                psMemberInfoList1.Add((PSMemberInfo) memberInfoCollection[index2]);
            }
          }
        }
        else if (psMemberInfo is PSPropertyInfo)
          psMemberInfoList1.Add(psMemberInfo);
      }
      Hashtable hashtable = new Hashtable();
      foreach (PSMemberInfo psMemberInfo in psMemberInfoList1)
      {
        if (!hashtable.ContainsKey((object) psMemberInfo.Name))
        {
          mshExpressionList.Add(new MshExpression(psMemberInfo.Name)
          {
            _isResolved = true
          });
          hashtable.Add((object) psMemberInfo.Name, (object) null);
        }
      }
      return mshExpressionList;
    }

    internal List<MshExpressionResult> GetValues(PSObject target) => this.GetValues(target, true, true);

    internal List<MshExpressionResult> GetValues(
      PSObject target,
      bool expand,
      bool eatExceptions)
    {
      List<MshExpressionResult> expressionResultList = new List<MshExpressionResult>();
      if (this._script != null)
      {
        MshExpressionResult expressionResult = new MshExpression(this._script).GetValue(target, eatExceptions);
        expressionResultList.Add(expressionResult);
        return expressionResultList;
      }
      foreach (MshExpression resolveName in this.ResolveNames(target, expand))
      {
        MshExpressionResult expressionResult = resolveName.GetValue(target, eatExceptions);
        expressionResultList.Add(expressionResult);
      }
      return expressionResultList;
    }

    private MshExpressionResult GetValue(PSObject target, bool eatExceptions)
    {
      try
      {
        object res;
        if (this._script != null)
        {
          res = this._script.InvokeUsingCmdlet((Cmdlet) null, true, false, (object) target, (object) AutomationNull.Value, (object) AutomationNull.Value);
        }
        else
        {
          PSMemberInfo property = (PSMemberInfo) target.Properties[this._stringValue];
          if (property == null)
            return new MshExpressionResult((object) null, this, (Exception) null);
          res = property.Value;
        }
        return new MshExpressionResult(res, this, (Exception) null);
      }
      catch (RuntimeException ex)
      {
        MshExpression.tracer.TraceException((Exception) ex);
        if (eatExceptions)
          return new MshExpressionResult((object) null, this, (Exception) ex);
        throw;
      }
    }
  }
}
