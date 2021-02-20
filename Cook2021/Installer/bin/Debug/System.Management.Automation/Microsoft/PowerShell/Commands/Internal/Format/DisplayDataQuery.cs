// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.DisplayDataQuery
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal static class DisplayDataQuery
  {
    [TraceSource("DisplayDataQuery", "DisplayDataQuery")]
    private static readonly PSTraceSource classTracer = PSTraceSource.GetTracer(nameof (DisplayDataQuery), nameof (DisplayDataQuery));
    private static PSTraceSource activeTracer = (PSTraceSource) null;

    private static PSTraceSource ActiveTracer => DisplayDataQuery.activeTracer ?? DisplayDataQuery.classTracer;

    internal static void SetTracer(PSTraceSource t) => DisplayDataQuery.activeTracer = t;

    internal static void ResetTracer() => DisplayDataQuery.activeTracer = DisplayDataQuery.classTracer;

    internal static EnumerableExpansion GetEnumerableExpansionFromType(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Collection<string> typeNames)
    {
      TypeMatch typeMatch = new TypeMatch(expressionFactory, db, typeNames);
      foreach (EnumerableExpansionDirective expansionDirective in db.defaultSettingsSection.enumerableExpansionDirectiveList)
      {
        if (typeMatch.PerfectMatch(new TypeMatchItem((object) expansionDirective, expansionDirective.appliesTo)))
          return expansionDirective.enumerableExpansion;
      }
      if (typeMatch.BestMatch != null)
        return ((EnumerableExpansionDirective) typeMatch.BestMatch).enumerableExpansion;
      Collection<string> typeNames1 = Deserializer.MaskDeserializationPrefix(typeNames);
      return typeNames1 != null ? DisplayDataQuery.GetEnumerableExpansionFromType(expressionFactory, db, typeNames1) : EnumerableExpansion.EnumOnly;
    }

    internal static FormatShape GetShapeFromType(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Collection<string> typeNames)
    {
      ShapeSelectionDirectives selectionDirectives = db.defaultSettingsSection.shapeSelectionDirectives;
      TypeMatch typeMatch = new TypeMatch(expressionFactory, db, typeNames);
      foreach (FormatShapeSelectionOnType shapeSelectionOnType in selectionDirectives.formatShapeSelectionOnTypeList)
      {
        if (typeMatch.PerfectMatch(new TypeMatchItem((object) shapeSelectionOnType, shapeSelectionOnType.appliesTo)))
          return shapeSelectionOnType.formatShape;
      }
      if (typeMatch.BestMatch != null)
        return ((FormatShapeSelectionBase) typeMatch.BestMatch).formatShape;
      Collection<string> typeNames1 = Deserializer.MaskDeserializationPrefix(typeNames);
      return typeNames1 != null ? DisplayDataQuery.GetShapeFromType(expressionFactory, db, typeNames1) : FormatShape.Undefined;
    }

    internal static FormatShape GetShapeFromPropertyCount(
      TypeInfoDataBase db,
      int propertyCount)
    {
      return propertyCount <= db.defaultSettingsSection.shapeSelectionDirectives.PropertyCountForTable ? FormatShape.Table : FormatShape.List;
    }

    internal static ViewDefinition GetViewByShapeAndType(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      FormatShape shape,
      Collection<string> typeNames,
      string viewName)
    {
      if (shape == FormatShape.Undefined)
        return DisplayDataQuery.GetDefaultView(expressionFactory, db, typeNames);
      Type mainControlType;
      if (shape == FormatShape.Table)
        mainControlType = typeof (TableControlBody);
      else if (shape == FormatShape.List)
        mainControlType = typeof (ListControlBody);
      else if (shape == FormatShape.Wide)
      {
        mainControlType = typeof (WideControlBody);
      }
      else
      {
        if (shape != FormatShape.Complex)
          return (ViewDefinition) null;
        mainControlType = typeof (ComplexControlBody);
      }
      return DisplayDataQuery.GetView(expressionFactory, db, mainControlType, typeNames, viewName);
    }

    internal static ViewDefinition GetOutOfBandView(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Collection<string> typeNames)
    {
      TypeMatch typeMatch = new TypeMatch(expressionFactory, db, typeNames);
      foreach (ViewDefinition viewDefinition in db.viewDefinitionsSection.viewDefinitionList)
      {
        if (DisplayDataQuery.IsOutOfBandView(viewDefinition) && typeMatch.PerfectMatch(new TypeMatchItem((object) viewDefinition, viewDefinition.appliesTo)))
          return viewDefinition;
      }
      if (!(typeMatch.BestMatch is ViewDefinition viewDefinition))
      {
        Collection<string> typeNames1 = Deserializer.MaskDeserializationPrefix(typeNames);
        if (typeNames1 != null)
          viewDefinition = DisplayDataQuery.GetOutOfBandView(expressionFactory, db, typeNames1);
      }
      return viewDefinition;
    }

    private static ViewDefinition GetView(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Type mainControlType,
      Collection<string> typeNames,
      string viewName)
    {
      TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
      foreach (ViewDefinition viewDefinition in db.viewDefinitionsSection.viewDefinitionList)
      {
        if (viewDefinition == null || mainControlType != viewDefinition.mainControl.GetType())
          DisplayDataQuery.ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), viewDefinition != null ? (object) viewDefinition.name : (object) string.Empty);
        else if (DisplayDataQuery.IsOutOfBandView(viewDefinition))
          DisplayDataQuery.ActiveTracer.WriteLine("NOT MATCH OutOfBand {0}  NAME: {1}", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), (object) viewDefinition.name);
        else if (viewDefinition.appliesTo == null)
        {
          DisplayDataQuery.ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}  No applicable types", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), (object) viewDefinition.name);
        }
        else
        {
          if (viewName != null)
          {
            if (!string.Equals(viewDefinition.name, viewName, StringComparison.OrdinalIgnoreCase))
            {
              DisplayDataQuery.ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), (object) viewDefinition.name);
              continue;
            }
          }
          try
          {
            TypeMatch.SetTracer(DisplayDataQuery.ActiveTracer);
            if (match.PerfectMatch(new TypeMatchItem((object) viewDefinition, viewDefinition.appliesTo)))
            {
              DisplayDataQuery.TraceHelper(viewDefinition, true);
              return viewDefinition;
            }
          }
          finally
          {
            TypeMatch.ResetTracer();
          }
          DisplayDataQuery.TraceHelper(viewDefinition, false);
        }
      }
      ViewDefinition viewDefinition1 = DisplayDataQuery.GetBestMatch(match);
      if (viewDefinition1 == null)
      {
        Collection<string> typeNames1 = Deserializer.MaskDeserializationPrefix(typeNames);
        if (typeNames1 != null)
          viewDefinition1 = DisplayDataQuery.GetView(expressionFactory, db, mainControlType, typeNames1, viewName);
      }
      return viewDefinition1;
    }

    private static void TraceHelper(ViewDefinition vd, bool isMatched)
    {
      if ((DisplayDataQuery.ActiveTracer.Options & PSTraceSourceOptions.WriteLine) == PSTraceSourceOptions.None)
        return;
      foreach (TypeOrGroupReference reference in vd.appliesTo.referenceList)
      {
        StringBuilder stringBuilder = new StringBuilder();
        TypeReference typeReference = reference as TypeReference;
        stringBuilder.Append(isMatched ? "MATCH FOUND" : "NOT MATCH");
        if (typeReference != null)
        {
          stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, " {0} NAME: {1}  TYPE: {2}", (object) ControlBase.GetControlShapeName(vd.mainControl), (object) vd.name, (object) typeReference.name);
        }
        else
        {
          TypeGroupReference typeGroupReference = reference as TypeGroupReference;
          stringBuilder.AppendFormat((IFormatProvider) CultureInfo.InvariantCulture, " {0} NAME: {1}  GROUP: {2}", (object) ControlBase.GetControlShapeName(vd.mainControl), (object) vd.name, (object) typeGroupReference.name);
        }
        DisplayDataQuery.ActiveTracer.WriteLine(stringBuilder.ToString(), new object[0]);
      }
    }

    private static ViewDefinition GetBestMatch(TypeMatch match)
    {
      if (match.BestMatch is ViewDefinition bestMatch)
        DisplayDataQuery.TraceHelper(bestMatch, true);
      return bestMatch;
    }

    private static ViewDefinition GetDefaultView(
      MshExpressionFactory expressionFactory,
      TypeInfoDataBase db,
      Collection<string> typeNames)
    {
      TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
      foreach (ViewDefinition viewDefinition in db.viewDefinitionsSection.viewDefinitionList)
      {
        if (viewDefinition != null)
        {
          if (DisplayDataQuery.IsOutOfBandView(viewDefinition))
            DisplayDataQuery.ActiveTracer.WriteLine("NOT MATCH OutOfBand {0}  NAME: {1}", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), (object) viewDefinition.name);
          else if (viewDefinition.appliesTo == null)
          {
            DisplayDataQuery.ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}  No applicable types", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), (object) viewDefinition.name);
          }
          else
          {
            try
            {
              TypeMatch.SetTracer(DisplayDataQuery.ActiveTracer);
              if (match.PerfectMatch(new TypeMatchItem((object) viewDefinition, viewDefinition.appliesTo)))
              {
                DisplayDataQuery.TraceHelper(viewDefinition, true);
                return viewDefinition;
              }
            }
            finally
            {
              TypeMatch.ResetTracer();
            }
            DisplayDataQuery.TraceHelper(viewDefinition, false);
          }
        }
      }
      ViewDefinition viewDefinition1 = DisplayDataQuery.GetBestMatch(match);
      if (viewDefinition1 == null)
      {
        Collection<string> typeNames1 = Deserializer.MaskDeserializationPrefix(typeNames);
        if (typeNames1 != null)
          viewDefinition1 = DisplayDataQuery.GetDefaultView(expressionFactory, db, typeNames1);
      }
      return viewDefinition1;
    }

    private static bool IsOutOfBandView(ViewDefinition vd) => (vd.mainControl is ComplexControlBody || vd.mainControl is ListControlBody) && vd.outOfBand;

    internal static AppliesTo GetAllApplicableTypes(
      TypeInfoDataBase db,
      AppliesTo appliesTo)
    {
      Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      foreach (TypeOrGroupReference reference in appliesTo.referenceList)
      {
        if (reference is TypeReference typeReference)
        {
          if (!hashtable.ContainsKey((object) typeReference.name))
            hashtable.Add((object) typeReference.name, (object) null);
        }
        else if (reference is TypeGroupReference typeGroupReference)
        {
          TypeGroupDefinition groupDefinition = DisplayDataQuery.FindGroupDefinition(db, typeGroupReference.name);
          if (groupDefinition != null)
          {
            foreach (TypeReference typeReference in groupDefinition.typeReferenceList)
            {
              if (!hashtable.ContainsKey((object) typeReference.name))
                hashtable.Add((object) typeReference.name, (object) null);
            }
          }
        }
      }
      AppliesTo appliesTo1 = new AppliesTo();
      foreach (DictionaryEntry dictionaryEntry in hashtable)
        appliesTo1.AddAppliesToType(dictionaryEntry.Key as string);
      return appliesTo1;
    }

    internal static TypeGroupDefinition FindGroupDefinition(
      TypeInfoDataBase db,
      string groupName)
    {
      foreach (TypeGroupDefinition typeGroupDefinition in db.typeGroupSection.typeGroupDefinitionList)
      {
        if (string.Equals(typeGroupDefinition.name, groupName, StringComparison.OrdinalIgnoreCase))
          return typeGroupDefinition;
      }
      return (TypeGroupDefinition) null;
    }

    internal static ControlBody ResolveControlReference(
      TypeInfoDataBase db,
      List<ControlDefinition> viewControlDefinitionList,
      ControlReference controlReference)
    {
      return DisplayDataQuery.ResolveControlReferenceInList(controlReference, viewControlDefinitionList) ?? DisplayDataQuery.ResolveControlReferenceInList(controlReference, db.formatControlDefinitionHolder.controlDefinitionList);
    }

    private static ControlBody ResolveControlReferenceInList(
      ControlReference controlReference,
      List<ControlDefinition> controlDefinitionList)
    {
      foreach (ControlDefinition controlDefinition in controlDefinitionList)
      {
        if (controlDefinition.controlBody.GetType() == controlReference.controlType && string.Compare(controlReference.name, controlDefinition.name, StringComparison.OrdinalIgnoreCase) == 0)
          return controlDefinition.controlBody;
      }
      return (ControlBody) null;
    }
  }
}
