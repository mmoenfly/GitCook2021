// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TypeMatch
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TypeMatch
  {
    private const int BestMatchIndexUndefined = -1;
    private const int BestMatchIndexPerfect = 0;
    [TraceSource("TypeMatch", "F&O TypeMatch")]
    private static readonly PSTraceSource classTracer = PSTraceSource.GetTracer(nameof (TypeMatch), "F&O TypeMatch");
    private static PSTraceSource activeTracer = (PSTraceSource) null;
    private MshExpressionFactory _expressionFactory;
    private TypeInfoDataBase _db;
    private Collection<string> _typeNameHierarchy;
    private bool _useInheritance;
    private List<MshExpressionResult> _failedResultsList = new List<MshExpressionResult>();
    private int _bestMatchIndex = -1;
    private TypeMatchItem _bestMatchItem;

    private static PSTraceSource ActiveTracer => TypeMatch.activeTracer ?? TypeMatch.classTracer;

    internal static void SetTracer(PSTraceSource t) => TypeMatch.activeTracer = t;

    internal static void ResetTracer() => TypeMatch.activeTracer = TypeMatch.classTracer;

    internal TypeMatch(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Collection<string> typeNames)
    {
      this._expressionFactory = expressionFactory;
      this._db = db;
      this._typeNameHierarchy = typeNames;
      this._useInheritance = true;
    }

    internal TypeMatch(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Collection<string> typeNames,
      bool useInheritance)
    {
      this._expressionFactory = expressionFactory;
      this._db = db;
      this._typeNameHierarchy = typeNames;
      this._useInheritance = useInheritance;
    }

    internal bool PerfectMatch(TypeMatchItem item)
    {
      int bestMatch = this.ComputeBestMatch(item.AppliesTo, item.CurrentObject);
      if (bestMatch == -1)
        return false;
      if (this._bestMatchIndex == -1 || bestMatch < this._bestMatchIndex)
      {
        this._bestMatchIndex = bestMatch;
        this._bestMatchItem = item;
      }
      return this._bestMatchIndex == 0;
    }

    internal object BestMatch => this._bestMatchItem == null ? (object) null : this._bestMatchItem.Item;

    private int ComputeBestMatch(AppliesTo appliesTo, PSObject currentObject)
    {
      int num1 = -1;
      foreach (TypeOrGroupReference reference in appliesTo.referenceList)
      {
        MshExpression ex = (MshExpression) null;
        if (reference.conditionToken != null)
          ex = this._expressionFactory.CreateFromExpressionToken(reference.conditionToken);
        int num2 = -1;
        if (reference is TypeReference typeReference)
        {
          num2 = this.MatchTypeIndex(typeReference.name, currentObject, ex);
        }
        else
        {
          TypeGroupDefinition groupDefinition = DisplayDataQuery.FindGroupDefinition(this._db, (reference as TypeGroupReference).name);
          if (groupDefinition != null)
            num2 = this.ComputeBestMatchInGroup(groupDefinition, currentObject, ex);
        }
        if (num2 == 0)
          return num2;
        if (num1 == -1 || num1 < num2)
          num1 = num2;
      }
      return num1;
    }

    private int ComputeBestMatchInGroup(
      TypeGroupDefinition tgd,
      PSObject currentObject,
      MshExpression ex)
    {
      int num1 = -1;
      int num2 = 0;
      foreach (TypeOrGroupReference typeReference in tgd.typeReferenceList)
      {
        int num3 = this.MatchTypeIndex(typeReference.name, currentObject, ex);
        if (num3 == 0)
          return num3;
        if (num1 == -1 || num1 < num3)
          num1 = num3;
        ++num2;
      }
      return num1;
    }

    private int MatchTypeIndex(string typeName, PSObject currentObject, MshExpression ex)
    {
      if (string.IsNullOrEmpty(typeName))
        return -1;
      int num = 0;
      foreach (string a in this._typeNameHierarchy)
      {
        if (string.Equals(a, typeName, StringComparison.OrdinalIgnoreCase) && this.MatchCondition(currentObject, ex))
          return num;
        if (num == 0)
        {
          if (!this._useInheritance)
            break;
        }
        ++num;
      }
      return -1;
    }

    private bool MatchCondition(PSObject currentObject, MshExpression ex)
    {
      if (ex == null)
        return true;
      MshExpressionResult expressionResult;
      bool flag = DisplayCondition.Evaluate(currentObject, ex, out expressionResult);
      if (expressionResult != null && expressionResult.Exception != null)
        this._failedResultsList.Add(expressionResult);
      return flag;
    }
  }
}
