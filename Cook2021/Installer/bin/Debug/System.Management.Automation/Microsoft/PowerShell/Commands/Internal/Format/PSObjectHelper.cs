// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.PSObjectHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal static class PSObjectHelper
  {
    internal const string ellipses = "...";
    private static readonly PSObject emptyPSObject = new PSObject((object) "");

    internal static string PSObjectIsOfExactType(Collection<string> typeNames) => typeNames.Count != 0 ? typeNames[0] : (string) null;

    internal static bool PSObjectIsEnum(Collection<string> typeNames) => typeNames.Count >= 2 && !string.IsNullOrEmpty(typeNames[1]) && string.Equals(typeNames[1], "System.Enum", StringComparison.Ordinal);

    internal static bool IsWriteErrorStream(PSObject so) => so.Properties["WriteErrorStream"] != null && so.Properties["WriteErrorStream"].Value is bool && (bool) so.Properties["WriteErrorStream"].Value;

    internal static MshExpression GetDisplayNameExpression(
      PSObject target,
      MshExpressionFactory expressionFactory)
    {
      MshExpression defaultNameExpression = PSObjectHelper.GetDefaultNameExpression(target);
      if (defaultNameExpression != null)
        return defaultNameExpression;
      string[] strArray = new string[6]
      {
        "name",
        "id",
        "key",
        "*key",
        "*name",
        "*id"
      };
      foreach (string s in strArray)
      {
        List<MshExpression> mshExpressionList = new MshExpression(s).ResolveNames(target);
        while (mshExpressionList.Count > 0 && (mshExpressionList[0].ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) || mshExpressionList[0].ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) || mshExpressionList[0].ToString().Equals(RemotingConstants.RunspaceIdNoteProperty, StringComparison.OrdinalIgnoreCase)))
          mshExpressionList.RemoveAt(0);
        if (mshExpressionList.Count != 0)
          return mshExpressionList[0];
      }
      return (MshExpression) null;
    }

    internal static MshExpressionResult GetDisplayName(
      PSObject target,
      MshExpressionFactory expressionFactory)
    {
      MshExpression displayNameExpression = PSObjectHelper.GetDisplayNameExpression(target, expressionFactory);
      if (displayNameExpression == null)
        return (MshExpressionResult) null;
      List<MshExpressionResult> values = displayNameExpression.GetValues(target);
      return values.Count == 0 || values[0].Exception != null ? (MshExpressionResult) null : values[0];
    }

    internal static IEnumerable GetEnumerable(object obj)
    {
      if (obj is PSObject psObject)
        obj = psObject.BaseObject;
      return obj is IDictionary ? (IEnumerable) obj : LanguagePrimitives.GetEnumerable(obj);
    }

    private static string GetSmartToStringDisplayName(
      object x,
      MshExpressionFactory expressionFactory)
    {
      MshExpressionResult displayName = PSObjectHelper.GetDisplayName(PSObjectHelper.AsPSObject(x), expressionFactory);
      return displayName != null && displayName.Exception == null ? PSObjectHelper.AsPSObject(displayName.Result).ToString() : PSObjectHelper.AsPSObject(x).ToString();
    }

    internal static string SmartToString(
      PSObject so,
      MshExpressionFactory expressionFactory,
      int enumerationLimit,
      StringFormatError formatErrorObject)
    {
      if (so == null)
        return "";
      try
      {
        IEnumerable enumerable = PSObjectHelper.GetEnumerable((object) so);
        if (enumerable == null)
          return so.ToString();
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("{");
        bool flag = true;
        int num = 0;
        if (enumerable.GetEnumerator() != null)
        {
          foreach (object obj in enumerable)
          {
            if (LocalPipeline.GetExecutionContextFromTLS().CurrentPipelineStopping)
              throw new PipelineStoppedException();
            if (enumerationLimit >= 0)
            {
              if (num == enumerationLimit)
              {
                stringBuilder.Append("...");
                break;
              }
              ++num;
            }
            string str;
            if (obj is PSObject && (LanguagePrimitives.IsBoolOrSwitchParameterType(((PSObject) obj).BaseObject.GetType()) || LanguagePrimitives.IsNumeric(Type.GetTypeCode(((PSObject) obj).BaseObject.GetType())) || LanguagePrimitives.IsNull(obj)))
              str = obj.ToString();
            else if (obj == null)
            {
              str = "$null";
            }
            else
            {
              MethodInfo method = obj.GetType().GetMethod("ToString", Type.EmptyTypes, (ParameterModifier[]) null);
              if (method.DeclaringType.Equals(method.ReflectedType))
              {
                str = PSObjectHelper.AsPSObject(obj).ToString();
              }
              else
              {
                MshExpressionResult displayName = PSObjectHelper.GetDisplayName(PSObjectHelper.AsPSObject(obj), expressionFactory);
                str = displayName == null || displayName.Exception != null ? PSObjectHelper.AsPSObject(obj).ToString() : PSObjectHelper.AsPSObject(displayName.Result).ToString();
              }
            }
            if (!flag)
              stringBuilder.Append(", ");
            stringBuilder.Append(str);
            if (flag)
              flag = false;
          }
        }
        stringBuilder.Append("}");
        return stringBuilder.ToString();
      }
      catch (ExtendedTypeSystemException ex)
      {
        if (formatErrorObject != null)
        {
          formatErrorObject.sourceObject = (object) so;
          formatErrorObject.exception = (Exception) ex;
        }
        return "";
      }
    }

    internal static PSObject AsPSObject(object obj) => obj != null ? PSObject.AsPSObject(obj) : PSObjectHelper.emptyPSObject;

    internal static string FormatField(
      FieldFormattingDirective directive,
      object val,
      int enumerationLimit,
      StringFormatError formatErrorObject,
      MshExpressionFactory expressionFactory)
    {
      PSObject so = PSObjectHelper.AsPSObject(val);
      if (directive != null)
      {
        if (!string.IsNullOrEmpty(directive.formatString))
        {
          try
          {
            if (!directive.formatString.Contains("{0") && !directive.formatString.Contains("}"))
              return so.ToString(directive.formatString, (IFormatProvider) null);
            return string.Format((IFormatProvider) CultureInfo.CurrentCulture, directive.formatString, (object) so);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            if (formatErrorObject != null)
            {
              formatErrorObject.sourceObject = (object) so;
              formatErrorObject.exception = ex;
              formatErrorObject.formatString = directive.formatString;
              return "";
            }
          }
        }
      }
      return PSObjectHelper.SmartToString(so, expressionFactory, enumerationLimit, formatErrorObject);
    }

    private static PSMemberSet MaskDeserializedAndGetStandardMembers(PSObject so)
    {
      Collection<string> strings = Deserializer.MaskDeserializationPrefix(so.TypeNames);
      if (strings == null)
        return (PSMemberSet) null;
      TypeTable typeTable = so.GetTypeTable();
      return typeTable == null ? (PSMemberSet) null : typeTable.GetMembers<PSMemberInfo>(new ConsolidatedString(strings))["PSStandardMembers"] as PSMemberSet;
    }

    private static List<MshExpression> GetDefaultPropertySet(
      PSMemberSet standardMembersSet)
    {
      if (standardMembersSet == null || !(standardMembersSet.Members["DefaultDisplayPropertySet"] is PSPropertySet member))
        return new List<MshExpression>();
      List<MshExpression> mshExpressionList = new List<MshExpression>();
      foreach (string referencedPropertyName in member.ReferencedPropertyNames)
      {
        if (!string.IsNullOrEmpty(referencedPropertyName))
          mshExpressionList.Add(new MshExpression(referencedPropertyName));
      }
      return mshExpressionList;
    }

    internal static List<MshExpression> GetDefaultPropertySet(PSObject so)
    {
      List<MshExpression> defaultPropertySet = PSObjectHelper.GetDefaultPropertySet(so.PSStandardMembers);
      if (defaultPropertySet.Count == 0)
        defaultPropertySet = PSObjectHelper.GetDefaultPropertySet(PSObjectHelper.MaskDeserializedAndGetStandardMembers(so));
      return defaultPropertySet;
    }

    private static MshExpression GetDefaultNameExpression(
      PSMemberSet standardMembersSet)
    {
      if (standardMembersSet == null || !(standardMembersSet.Members["DefaultDisplayProperty"] is PSNoteProperty member))
        return (MshExpression) null;
      string s = member.Value.ToString();
      return string.IsNullOrEmpty(s) ? (MshExpression) null : new MshExpression(s);
    }

    private static MshExpression GetDefaultNameExpression(PSObject so) => PSObjectHelper.GetDefaultNameExpression(so.PSStandardMembers) ?? PSObjectHelper.GetDefaultNameExpression(PSObjectHelper.MaskDeserializedAndGetStandardMembers(so));

    internal static string GetExpressionDisplayValue(
      PSObject so,
      int enumerationLimit,
      MshExpression ex,
      FieldFormattingDirective directive,
      StringFormatError formatErrorObject,
      MshExpressionFactory expressionFactory,
      out MshExpressionResult result)
    {
      result = (MshExpressionResult) null;
      List<MshExpressionResult> values = ex.GetValues(so);
      if (values.Count == 0)
        return "";
      result = values[0];
      return result.Exception != null ? "" : PSObjectHelper.FormatField(directive, result.Result, enumerationLimit, formatErrorObject, expressionFactory);
    }

    internal static bool ShouldShowComputerNameProperty(PSObject so)
    {
      bool result = false;
      if (so != null)
      {
        PSPropertyInfo property1 = so.Properties[RemotingConstants.ComputerNameNoteProperty];
        PSPropertyInfo property2 = so.Properties[RemotingConstants.ShowComputerNameNoteProperty];
        if (property1 != null && property2 != null)
          LanguagePrimitives.TryConvertTo<bool>(property2.Value, out result);
      }
      return result;
    }
  }
}
