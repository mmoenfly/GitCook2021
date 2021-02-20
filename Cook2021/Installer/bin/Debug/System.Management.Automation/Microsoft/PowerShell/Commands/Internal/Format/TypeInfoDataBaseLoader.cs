// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TypeInfoDataBaseLoader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Xml;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TypeInfoDataBaseLoader : XmlLoaderBase
  {
    private const string resBaseName = "TypeInfoDataBaseLoaderStrings";
    [TraceSource("TypeInfoDataBaseLoader", "TypeInfoDataBaseLoader")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (TypeInfoDataBaseLoader), nameof (TypeInfoDataBaseLoader));

    internal bool LoadXmlFile(
      XmlFileLoadInfo info,
      TypeInfoDataBase db,
      MshExpressionFactory expressionFactory,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      if (info == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (info));
      if (info.filePath == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException("info.filePath");
      if (db == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (db));
      if (expressionFactory == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (expressionFactory));
      this.displayResourceManagerCache = db.displayResourceManagerCache;
      this.expressionFactory = expressionFactory;
      this.SetDatabaseLoadingInfo(info);
      this.ReportTrace("loading file started");
      XmlDocument doc = this.LoadXmlDocumentFromFileLoadingInfo(authorizationManager, host);
      if (doc == null)
        return false;
      try
      {
        this.LoadData(doc, db);
      }
      catch (TooManyErrorsException ex)
      {
        return false;
      }
      catch (Exception ex)
      {
        this.ReportError(XmlLoadingResourceManager.FormatString("ErrorInFile", (object) this.FilePath, (object) ex.Message));
        TypeInfoDataBaseLoader.tracer.TraceException(ex);
        throw;
      }
      if (this.HasErrors)
        return false;
      this.ReportTrace("file loaded with no errors");
      return true;
    }

    private void LoadData(XmlDocument doc, TypeInfoDataBase db)
    {
      if (doc == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (doc));
      if (db == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (db));
      XmlElement documentElement = doc.DocumentElement;
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      if (this.MatchNodeName((XmlNode) documentElement, "Configuration"))
      {
        using (this.StackFrame((XmlNode) documentElement))
        {
          foreach (XmlNode childNode in documentElement.ChildNodes)
          {
            if (this.MatchNodeName(childNode, "DefaultSettings"))
            {
              if (flag1)
                this.ProcessDuplicateNode(childNode);
              flag1 = true;
              this.LoadDefaultSettings(db, childNode);
            }
            else if (this.MatchNodeName(childNode, "SelectionSets"))
            {
              if (flag2)
                this.ProcessDuplicateNode(childNode);
              flag2 = true;
              this.LoadTypeGroups(db, childNode);
            }
            else if (this.MatchNodeName(childNode, "ViewDefinitions"))
            {
              if (flag3)
                this.ProcessDuplicateNode(childNode);
              flag3 = true;
              this.LoadViewDefinitions(db, childNode);
            }
            else if (this.MatchNodeName(childNode, "Controls"))
            {
              if (flag4)
                this.ProcessDuplicateNode(childNode);
              flag4 = true;
              this.LoadControlDefinitions(childNode, db.formatControlDefinitionHolder.controlDefinitionList);
            }
            else
              this.ProcessUnknownNode(childNode);
          }
        }
      }
      else
        this.ProcessUnknownNode((XmlNode) documentElement);
    }

    private void LoadDefaultSettings(TypeInfoDataBase db, XmlNode defaultSettingsNode)
    {
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      bool flag5 = false;
      bool val1;
      using (this.StackFrame(defaultSettingsNode))
      {
        foreach (XmlNode childNode in defaultSettingsNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "ShowError"))
          {
            if (flag2)
              this.ProcessDuplicateNode(childNode);
            flag2 = true;
            if (this.ReadBooleanNode(childNode, out val1))
              db.defaultSettingsSection.formatErrorPolicy.ShowErrorsAsMessages = val1;
          }
          else if (this.MatchNodeName(childNode, "DisplayError"))
          {
            if (flag3)
              this.ProcessDuplicateNode(childNode);
            flag3 = true;
            if (this.ReadBooleanNode(childNode, out val1))
              db.defaultSettingsSection.formatErrorPolicy.ShowErrorsInFormattedOutput = val1;
          }
          else if (this.MatchNodeName(childNode, "PropertyCountForTable"))
          {
            if (flag1)
              this.ProcessDuplicateNode(childNode);
            flag1 = true;
            int val2;
            if (this.ReadPositiveIntegerValue(childNode, out val2))
              db.defaultSettingsSection.shapeSelectionDirectives.PropertyCountForTable = val2;
            else
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidNodeValue", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "PropertyCountForTable"));
          }
          else if (this.MatchNodeName(childNode, "WrapTables"))
          {
            if (flag5)
              this.ProcessDuplicateNode(childNode);
            flag5 = true;
            if (this.ReadBooleanNode(childNode, out val1))
              db.defaultSettingsSection.MultilineTables = val1;
            else
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidNodeValue", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "WrapTables"));
          }
          else if (this.MatchNodeName(childNode, "EnumerableExpansions"))
          {
            if (flag4)
              this.ProcessDuplicateNode(childNode);
            flag4 = true;
            db.defaultSettingsSection.enumerableExpansionDirectiveList = this.LoadEnumerableExpansionDirectiveList(childNode);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private List<EnumerableExpansionDirective> LoadEnumerableExpansionDirectiveList(
      XmlNode expansionListNode)
    {
      List<EnumerableExpansionDirective> expansionDirectiveList = new List<EnumerableExpansionDirective>();
      using (this.StackFrame(expansionListNode))
      {
        int num = 0;
        foreach (XmlNode childNode in expansionListNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "EnumerableExpansion"))
          {
            EnumerableExpansionDirective expansionDirective = this.LoadEnumerableExpansionDirective(childNode, num++);
            if (expansionDirective == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "EnumerableExpansion"));
              return (List<EnumerableExpansionDirective>) null;
            }
            expansionDirectiveList.Add(expansionDirective);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
      return expansionDirectiveList;
    }

    private EnumerableExpansionDirective LoadEnumerableExpansionDirective(
      XmlNode directive,
      int index)
    {
      using (this.StackFrame(directive, index))
      {
        EnumerableExpansionDirective expansionDirective = new EnumerableExpansionDirective();
        bool flag1 = false;
        bool flag2 = false;
        foreach (XmlNode childNode in directive.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "EntrySelectedBy"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (EnumerableExpansionDirective) null;
            }
            flag1 = true;
            expansionDirective.appliesTo = this.LoadAppliesToSection(childNode, true);
          }
          else if (this.MatchNodeName(childNode, "Expand"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (EnumerableExpansionDirective) null;
            }
            flag2 = true;
            string mandatoryInnerText = this.GetMandatoryInnerText(childNode);
            if (mandatoryInnerText == null)
              return (EnumerableExpansionDirective) null;
            if (!EnumerableExpansionConversion.Convert(mandatoryInnerText, out expansionDirective.enumerableExpansion))
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidNodeValue", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "Expand"));
              return (EnumerableExpansionDirective) null;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        return expansionDirective;
      }
    }

    private void LoadTypeGroups(TypeInfoDataBase db, XmlNode typeGroupsNode)
    {
      using (this.StackFrame(typeGroupsNode))
      {
        int num = 0;
        foreach (XmlNode childNode in typeGroupsNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "SelectionSet"))
            this.LoadTypeGroup(db, childNode, num++);
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private void LoadTypeGroup(TypeInfoDataBase db, XmlNode typeGroupNode, int index)
    {
      using (this.StackFrame(typeGroupNode, index))
      {
        TypeGroupDefinition typeGroupDefinition = new TypeGroupDefinition();
        bool flag = false;
        foreach (XmlNode childNode in typeGroupNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "Name"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(childNode);
            }
            else
            {
              flag = true;
              typeGroupDefinition.name = this.GetMandatoryInnerText(childNode);
            }
          }
          else if (this.MatchNodeName(childNode, "Types"))
            this.LoadTypeGroupTypeRefs(childNode, typeGroupDefinition);
          else
            this.ProcessUnknownNode(childNode);
        }
        if (!flag)
          this.ReportMissingNode("Name");
        db.typeGroupSection.typeGroupDefinitionList.Add(typeGroupDefinition);
      }
    }

    private void LoadTypeGroupTypeRefs(XmlNode typesNode, TypeGroupDefinition typeGroupDefinition)
    {
      using (this.StackFrame(typesNode))
      {
        int num = 0;
        foreach (XmlNode childNode in typesNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "TypeName"))
          {
            using (this.StackFrame(childNode, num++))
            {
              TypeReference typeReference = new TypeReference();
              typeReference.name = this.GetMandatoryInnerText(childNode);
              typeGroupDefinition.typeReferenceList.Add(typeReference);
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private AppliesTo LoadAppliesToSection(
      XmlNode appliesToNode,
      bool allowSelectionCondition)
    {
      using (this.StackFrame(appliesToNode))
      {
        AppliesTo appliesTo = new AppliesTo();
        foreach (XmlNode childNode in appliesToNode.ChildNodes)
        {
          using (this.StackFrame(childNode))
          {
            if (this.MatchNodeName(childNode, "SelectionSetName"))
            {
              TypeGroupReference typeGroupReference = this.LoadTypeGroupReference(childNode);
              if (typeGroupReference == null)
                return (AppliesTo) null;
              appliesTo.referenceList.Add((TypeOrGroupReference) typeGroupReference);
            }
            else if (this.MatchNodeName(childNode, "TypeName"))
            {
              TypeReference typeReference = this.LoadTypeReference(childNode);
              if (typeReference == null)
                return (AppliesTo) null;
              appliesTo.referenceList.Add((TypeOrGroupReference) typeReference);
            }
            else if (allowSelectionCondition && this.MatchNodeName(childNode, "SelectionCondition"))
            {
              TypeOrGroupReference orGroupReference = this.LoadSelectionConditionNode(childNode);
              if (orGroupReference == null)
                return (AppliesTo) null;
              appliesTo.referenceList.Add(orGroupReference);
            }
            else
              this.ProcessUnknownNode(childNode);
          }
        }
        if (appliesTo.referenceList.Count != 0)
          return appliesTo;
        this.ReportError(XmlLoadingResourceManager.FormatString("EmptyAppliesTo", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return (AppliesTo) null;
      }
    }

    private TypeReference LoadTypeReference(XmlNode n)
    {
      string mandatoryInnerText = this.GetMandatoryInnerText(n);
      if (mandatoryInnerText == null)
        return (TypeReference) null;
      TypeReference typeReference = new TypeReference();
      typeReference.name = mandatoryInnerText;
      return typeReference;
    }

    private TypeGroupReference LoadTypeGroupReference(XmlNode n)
    {
      string mandatoryInnerText = this.GetMandatoryInnerText(n);
      if (mandatoryInnerText == null)
        return (TypeGroupReference) null;
      TypeGroupReference typeGroupReference = new TypeGroupReference();
      typeGroupReference.name = mandatoryInnerText;
      return typeGroupReference;
    }

    private TypeOrGroupReference LoadSelectionConditionNode(
      XmlNode selectionConditionNode)
    {
      using (this.StackFrame(selectionConditionNode))
      {
        TypeOrGroupReference orGroupReference = (TypeOrGroupReference) null;
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        TypeInfoDataBaseLoader.ExpressionNodeMatch expressionNodeMatch = new TypeInfoDataBaseLoader.ExpressionNodeMatch(this);
        foreach (XmlNode childNode in selectionConditionNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "SelectionSetName"))
          {
            if (flag3)
            {
              this.ProcessDuplicateAlternateNode(childNode, "SelectionSetName", "TypeName");
              return (TypeOrGroupReference) null;
            }
            flag3 = true;
            TypeGroupReference typeGroupReference = this.LoadTypeGroupReference(childNode);
            if (typeGroupReference == null)
              return (TypeOrGroupReference) null;
            orGroupReference = (TypeOrGroupReference) typeGroupReference;
          }
          else if (this.MatchNodeName(childNode, "TypeName"))
          {
            if (flag2)
            {
              this.ProcessDuplicateAlternateNode(childNode, "SelectionSetName", "TypeName");
              return (TypeOrGroupReference) null;
            }
            flag2 = true;
            TypeReference typeReference = this.LoadTypeReference(childNode);
            if (typeReference == null)
              return (TypeOrGroupReference) null;
            orGroupReference = (TypeOrGroupReference) typeReference;
          }
          else if (expressionNodeMatch.MatchNode(childNode))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (TypeOrGroupReference) null;
            }
            flag1 = true;
            if (!expressionNodeMatch.ProcessNode(childNode))
              return (TypeOrGroupReference) null;
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (flag2 && flag3)
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("SelectionSetNameAndTypeName", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
          return (TypeOrGroupReference) null;
        }
        if (orGroupReference == null)
        {
          this.ReportMissingNodes(new string[2]
          {
            "SelectionSetName",
            "TypeName"
          });
          return (TypeOrGroupReference) null;
        }
        if (flag1)
        {
          orGroupReference.conditionToken = expressionNodeMatch.GenerateExpressionToken();
          return orGroupReference.conditionToken == null ? (TypeOrGroupReference) null : orGroupReference;
        }
        this.ReportError(XmlLoadingResourceManager.FormatString("ExpectExpression", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return (TypeOrGroupReference) null;
      }
    }

    private GroupBy LoadGroupBySection(XmlNode groupByNode)
    {
      using (this.StackFrame(groupByNode))
      {
        TypeInfoDataBaseLoader.ExpressionNodeMatch expressionNodeMatch = new TypeInfoDataBaseLoader.ExpressionNodeMatch(this);
        TypeInfoDataBaseLoader.ComplexControlMatch complexControlMatch = new TypeInfoDataBaseLoader.ComplexControlMatch(this);
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        GroupBy groupBy = new GroupBy();
        TextToken textToken = (TextToken) null;
        foreach (XmlNode xmlNode in groupByNode)
        {
          if (expressionNodeMatch.MatchNode(xmlNode))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (GroupBy) null;
            }
            flag1 = true;
            if (!expressionNodeMatch.ProcessNode(xmlNode))
              return (GroupBy) null;
          }
          else if (complexControlMatch.MatchNode(xmlNode))
          {
            if (flag2)
            {
              this.ProcessDuplicateAlternateNode(xmlNode, "CustomControl", "CustomControlName");
              return (GroupBy) null;
            }
            flag2 = true;
            if (!complexControlMatch.ProcessNode(xmlNode))
              return (GroupBy) null;
          }
          else if (this.MatchNodeNameWithAttributes(xmlNode, "Label"))
          {
            if (flag3)
            {
              this.ProcessDuplicateAlternateNode(xmlNode, "CustomControl", "CustomControlName");
              return (GroupBy) null;
            }
            flag3 = true;
            textToken = this.LoadLabel(xmlNode);
            if (textToken == null)
              return (GroupBy) null;
          }
          else
            this.ProcessUnknownNode(xmlNode);
        }
        if (flag2 && flag3)
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("ControlAndLabel", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
          return (GroupBy) null;
        }
        if (flag2 || flag3)
        {
          if (!flag1)
          {
            this.ReportError(XmlLoadingResourceManager.FormatString("ControlLabelWithoutExpression", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
            return (GroupBy) null;
          }
          if (flag2)
            groupBy.startGroup.control = complexControlMatch.Control;
          else if (flag3)
            groupBy.startGroup.labelTextToken = textToken;
        }
        if (flag1)
        {
          ExpressionToken expressionToken = expressionNodeMatch.GenerateExpressionToken();
          if (expressionToken == null)
            return (GroupBy) null;
          groupBy.startGroup.expression = expressionToken;
          return groupBy;
        }
        this.ReportError(XmlLoadingResourceManager.FormatString("ExpectExpression", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return (GroupBy) null;
      }
    }

    private TextToken LoadLabel(XmlNode textNode)
    {
      using (this.StackFrame(textNode))
        return this.LoadTextToken(textNode);
    }

    private TextToken LoadTextToken(XmlNode n)
    {
      TextToken textToken = new TextToken();
      if (!this.LoadStringResourceReference(n, out textToken.resource))
        return (TextToken) null;
      if (textToken.resource != null)
      {
        textToken.text = n.InnerText;
        return textToken;
      }
      textToken.text = this.GetMandatoryInnerText(n);
      return textToken.text == null ? (TextToken) null : textToken;
    }

    private bool LoadStringResourceReference(XmlNode n, out StringResourceReference resource)
    {
      resource = (StringResourceReference) null;
      if (!(n is XmlElement xmlElement))
      {
        this.ReportError(XmlLoadingResourceManager.FormatString("NonXmlElementNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return false;
      }
      if (xmlElement.Attributes.Count <= 0)
        return true;
      resource = this.LoadResourceAttributes(xmlElement.Attributes);
      return resource != null;
    }

    private StringResourceReference LoadResourceAttributes(
      XmlAttributeCollection attributes)
    {
      StringResourceReference resourceReference = new StringResourceReference();
      foreach (XmlAttribute attribute in (XmlNamedNodeMap) attributes)
      {
        if (this.MatchAttributeName(attribute, "AssemblyName"))
        {
          resourceReference.assemblyName = this.GetMandatoryAttributeValue(attribute);
          if (resourceReference.assemblyName == null)
            return (StringResourceReference) null;
        }
        else if (this.MatchAttributeName(attribute, "BaseName"))
        {
          resourceReference.baseName = this.GetMandatoryAttributeValue(attribute);
          if (resourceReference.baseName == null)
            return (StringResourceReference) null;
        }
        else if (this.MatchAttributeName(attribute, "ResourceId"))
        {
          resourceReference.resourceId = this.GetMandatoryAttributeValue(attribute);
          if (resourceReference.resourceId == null)
            return (StringResourceReference) null;
        }
        else
        {
          this.ProcessUnknownAttribute(attribute);
          return (StringResourceReference) null;
        }
      }
      if (resourceReference.assemblyName == null)
      {
        this.ReportMissingAttribute("AssemblyName");
        return (StringResourceReference) null;
      }
      if (resourceReference.baseName == null)
      {
        this.ReportMissingAttribute("BaseName");
        return (StringResourceReference) null;
      }
      if (resourceReference.resourceId == null)
      {
        this.ReportMissingAttribute("ResourceId");
        return (StringResourceReference) null;
      }
      resourceReference.loadingInfo = this.LoadingInfo;
      if (this.VerifyStringResources)
      {
        DisplayResourceManagerCache.LoadingResult result;
        DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus;
        this.displayResourceManagerCache.VerifyResource(resourceReference, out result, out bindingStatus);
        if (result != DisplayResourceManagerCache.LoadingResult.NoError)
        {
          this.ReportStringResourceFailure(resourceReference, result, bindingStatus);
          return (StringResourceReference) null;
        }
      }
      return resourceReference;
    }

    private void ReportStringResourceFailure(
      StringResourceReference resource,
      DisplayResourceManagerCache.LoadingResult result,
      DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus)
    {
      string str;
      switch (bindingStatus)
      {
        case DisplayResourceManagerCache.AssemblyBindingStatus.FoundInGac:
          str = XmlLoadingResourceManager.FormatString("AssemblyInGAC", (object) resource.assemblyName);
          break;
        case DisplayResourceManagerCache.AssemblyBindingStatus.FoundInPath:
          str = Path.Combine(resource.loadingInfo.fileDirectory, resource.assemblyName);
          break;
        default:
          str = resource.assemblyName;
          break;
      }
      string message = (string) null;
      switch (result)
      {
        case DisplayResourceManagerCache.LoadingResult.AssemblyNotFound:
          message = XmlLoadingResourceManager.FormatString("AssemblyNotFound", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) str);
          break;
        case DisplayResourceManagerCache.LoadingResult.ResourceNotFound:
          message = XmlLoadingResourceManager.FormatString("ResourceNotFound", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) resource.baseName, (object) str);
          break;
        case DisplayResourceManagerCache.LoadingResult.StringNotFound:
          message = XmlLoadingResourceManager.FormatString("StringResourceNotFound", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) resource.resourceId, (object) resource.baseName, (object) str);
          break;
      }
      this.ReportError(message);
    }

    internal bool VerifyScriptBlock(string scriptBlockText)
    {
      try
      {
        this.expressionFactory.VerifyScriptBlockText(scriptBlockText);
      }
      catch (ParseException ex)
      {
        this.ReportError(XmlLoadingResourceManager.FormatString("InvalidScriptBlock", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) ex.Message));
        return false;
      }
      catch (Exception ex)
      {
        TypeInfoDataBaseLoader.tracer.TraceException(ex);
        throw;
      }
      return true;
    }

    private ComplexControlBody LoadComplexControl(XmlNode controlNode)
    {
      using (this.StackFrame(controlNode))
      {
        ComplexControlBody complexBody = new ComplexControlBody();
        bool flag = false;
        foreach (XmlNode childNode in controlNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "CustomEntries"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(childNode);
            }
            else
            {
              flag = true;
              this.LoadComplexControlEntries(childNode, complexBody);
              if (complexBody.defaultEntry == null)
                return (ComplexControlBody) null;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (flag)
          return complexBody;
        this.ReportMissingNode("CustomEntries");
        return (ComplexControlBody) null;
      }
    }

    private void LoadComplexControlEntries(
      XmlNode complexControlEntriesNode,
      ComplexControlBody complexBody)
    {
      using (this.StackFrame(complexControlEntriesNode))
      {
        int num = 0;
        foreach (XmlNode childNode in complexControlEntriesNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "CustomEntry"))
          {
            ComplexControlEntryDefinition controlEntryDefinition = this.LoadComplexControlEntryDefinition(childNode, num++);
            if (controlEntryDefinition == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "CustomEntry"));
              complexBody.defaultEntry = (ComplexControlEntryDefinition) null;
              return;
            }
            if (controlEntryDefinition.appliesTo == null)
            {
              if (complexBody.defaultEntry == null)
              {
                complexBody.defaultEntry = controlEntryDefinition;
              }
              else
              {
                this.ReportError(XmlLoadingResourceManager.FormatString("TooManyDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "CustomEntry"));
                complexBody.defaultEntry = (ComplexControlEntryDefinition) null;
                return;
              }
            }
            else
              complexBody.optionalEntryList.Add(controlEntryDefinition);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (complexBody.defaultEntry != null)
          return;
        this.ReportError(XmlLoadingResourceManager.FormatString("NoDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "CustomEntry"));
      }
    }

    private ComplexControlEntryDefinition LoadComplexControlEntryDefinition(
      XmlNode complexControlEntryNode,
      int index)
    {
      using (this.StackFrame(complexControlEntryNode, index))
      {
        bool flag1 = false;
        bool flag2 = false;
        ComplexControlEntryDefinition controlEntryDefinition = new ComplexControlEntryDefinition();
        foreach (XmlNode childNode in complexControlEntryNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "EntrySelectedBy"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (ComplexControlEntryDefinition) null;
            }
            flag1 = true;
            controlEntryDefinition.appliesTo = this.LoadAppliesToSection(childNode, true);
          }
          else if (this.MatchNodeName(childNode, "CustomItem"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (ComplexControlEntryDefinition) null;
            }
            flag2 = true;
            controlEntryDefinition.itemDefinition.formatTokenList = this.LoadComplexControlTokenListDefinitions(childNode);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (controlEntryDefinition.itemDefinition.formatTokenList != null)
          return controlEntryDefinition;
        this.ReportError(XmlLoadingResourceManager.FormatString("MissingNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "CustomItem"));
        return (ComplexControlEntryDefinition) null;
      }
    }

    private List<FormatToken> LoadComplexControlTokenListDefinitions(XmlNode bodyNode)
    {
      using (this.StackFrame(bodyNode))
      {
        List<FormatToken> formatTokenList = new List<FormatToken>();
        int num1 = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        foreach (XmlNode childNode in bodyNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "ExpressionBinding"))
          {
            CompoundPropertyToken compoundPropertyToken = this.LoadCompoundProperty(childNode, num1++);
            if (compoundPropertyToken == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "ExpressionBinding"));
              return (List<FormatToken>) null;
            }
            formatTokenList.Add((FormatToken) compoundPropertyToken);
          }
          else if (this.MatchNodeName(childNode, "NewLine"))
          {
            NewLineToken newLineToken = this.LoadNewLine(childNode, num2++);
            if (newLineToken == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "NewLine"));
              return (List<FormatToken>) null;
            }
            formatTokenList.Add((FormatToken) newLineToken);
          }
          else if (this.MatchNodeNameWithAttributes(childNode, "Text"))
          {
            TextToken textToken = this.LoadText(childNode, num3++);
            if (textToken == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "Text"));
              return (List<FormatToken>) null;
            }
            formatTokenList.Add((FormatToken) textToken);
          }
          else if (this.MatchNodeName(childNode, "Frame"))
          {
            FrameToken frameToken = this.LoadFrameDefinition(childNode, num4++);
            if (frameToken == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "Frame"));
              return (List<FormatToken>) null;
            }
            formatTokenList.Add((FormatToken) frameToken);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (formatTokenList.Count != 0)
          return formatTokenList;
        this.ReportError(XmlLoadingResourceManager.FormatString("EmptyCustomControlList", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return (List<FormatToken>) null;
      }
    }

    private bool LoadPropertyBaseHelper(
      XmlNode propertyBaseNode,
      PropertyTokenBase ptb,
      List<XmlNode> unprocessedNodes)
    {
      TypeInfoDataBaseLoader.ExpressionNodeMatch expressionNodeMatch = new TypeInfoDataBaseLoader.ExpressionNodeMatch(this);
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      ExpressionToken expressionToken1 = (ExpressionToken) null;
      foreach (XmlNode childNode in propertyBaseNode.ChildNodes)
      {
        if (expressionNodeMatch.MatchNode(childNode))
        {
          if (flag1)
          {
            this.ProcessDuplicateNode(childNode);
            return false;
          }
          flag1 = true;
          if (!expressionNodeMatch.ProcessNode(childNode))
            return false;
        }
        else if (this.MatchNodeName(childNode, "EnumerateCollection"))
        {
          if (flag2)
          {
            this.ProcessDuplicateNode(childNode);
            return false;
          }
          flag2 = true;
          if (!this.ReadBooleanNode(childNode, out ptb.enumerateCollection))
            return false;
        }
        else if (this.MatchNodeName(childNode, "ItemSelectionCondition"))
        {
          if (flag3)
          {
            this.ProcessDuplicateNode(childNode);
            return false;
          }
          flag3 = true;
          expressionToken1 = this.LoadItemSelectionCondition(childNode);
          if (expressionToken1 == null)
            return false;
        }
        else if (!XmlLoaderBase.IsFilteredOutNode(childNode))
          unprocessedNodes.Add(childNode);
      }
      if (flag1)
      {
        ExpressionToken expressionToken2 = expressionNodeMatch.GenerateExpressionToken();
        if (expressionToken2 == null)
          return false;
        ptb.expression = expressionToken2;
        ptb.conditionToken = expressionToken1;
      }
      return true;
    }

    private CompoundPropertyToken LoadCompoundProperty(
      XmlNode compoundPropertyNode,
      int index)
    {
      using (this.StackFrame(compoundPropertyNode, index))
      {
        CompoundPropertyToken compoundPropertyToken = new CompoundPropertyToken();
        List<XmlNode> unprocessedNodes = new List<XmlNode>();
        if (!this.LoadPropertyBaseHelper(compoundPropertyNode, (PropertyTokenBase) compoundPropertyToken, unprocessedNodes))
          return (CompoundPropertyToken) null;
        compoundPropertyToken.control = (ControlBase) null;
        bool flag1 = false;
        bool flag2 = false;
        TypeInfoDataBaseLoader.ComplexControlMatch complexControlMatch = new TypeInfoDataBaseLoader.ComplexControlMatch(this);
        FieldControlBody fieldControlBody = (FieldControlBody) null;
        foreach (XmlNode n in unprocessedNodes)
        {
          if (complexControlMatch.MatchNode(n))
          {
            if (flag1)
            {
              this.ProcessDuplicateAlternateNode(n, "CustomControl", "CustomControlName");
              return (CompoundPropertyToken) null;
            }
            flag1 = true;
            if (!complexControlMatch.ProcessNode(n))
              return (CompoundPropertyToken) null;
          }
          else if (this.MatchNodeName(n, "FieldControl"))
          {
            if (flag2)
            {
              this.ProcessDuplicateAlternateNode(n, "CustomControl", "CustomControlName");
              return (CompoundPropertyToken) null;
            }
            flag2 = true;
            fieldControlBody = new FieldControlBody();
            fieldControlBody.fieldFormattingDirective.formatString = this.GetMandatoryInnerText(n);
            if (fieldControlBody.fieldFormattingDirective.formatString == null)
              return (CompoundPropertyToken) null;
          }
          else
            this.ProcessUnknownNode(n);
        }
        if (flag2 && flag1)
        {
          this.ProcessDuplicateAlternateNode("CustomControl", "CustomControlName");
          return (CompoundPropertyToken) null;
        }
        compoundPropertyToken.control = !flag2 ? complexControlMatch.Control : (ControlBase) fieldControlBody;
        return compoundPropertyToken;
      }
    }

    private NewLineToken LoadNewLine(XmlNode newLineNode, int index)
    {
      using (this.StackFrame(newLineNode, index))
        return !this.VerifyNodeHasNoChildren(newLineNode) ? (NewLineToken) null : new NewLineToken();
    }

    private TextToken LoadText(XmlNode textNode, int index)
    {
      using (this.StackFrame(textNode, index))
        return this.LoadTextToken(textNode);
    }

    internal TextToken LoadText(XmlNode textNode)
    {
      using (this.StackFrame(textNode))
        return this.LoadTextToken(textNode);
    }

    private int LoadIntegerValue(XmlNode node, out bool success)
    {
      using (this.StackFrame(node))
      {
        success = false;
        int result = 0;
        if (!this.VerifyNodeHasNoChildren(node))
          return result;
        string mandatoryInnerText = this.GetMandatoryInnerText(node);
        if (mandatoryInnerText == null)
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("MissingInnerText", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
          return result;
        }
        if (!int.TryParse(mandatoryInnerText, out result))
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("ExpectInteger", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
          return result;
        }
        success = true;
        return result;
      }
    }

    private int LoadPositiveOrZeroIntegerValue(XmlNode node, out bool success)
    {
      int num = this.LoadIntegerValue(node, out success);
      if (!success)
        return num;
      using (this.StackFrame(node))
      {
        if (num < 0)
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("ExpectNaturalNumber", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
          success = false;
        }
        return num;
      }
    }

    private FrameToken LoadFrameDefinition(XmlNode frameNode, int index)
    {
      using (this.StackFrame(frameNode, index))
      {
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = false;
        FrameToken frameToken = new FrameToken();
        bool success;
        foreach (XmlNode childNode in frameNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "LeftIndent"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (FrameToken) null;
            }
            flag2 = true;
            frameToken.frameInfoDefinition.leftIndentation = this.LoadPositiveOrZeroIntegerValue(childNode, out success);
            if (!success)
              return (FrameToken) null;
          }
          else if (this.MatchNodeName(childNode, "RightIndent"))
          {
            if (flag3)
            {
              this.ProcessDuplicateNode(childNode);
              return (FrameToken) null;
            }
            flag3 = true;
            frameToken.frameInfoDefinition.rightIndentation = this.LoadPositiveOrZeroIntegerValue(childNode, out success);
            if (!success)
              return (FrameToken) null;
          }
          else if (this.MatchNodeName(childNode, "FirstLineIndent"))
          {
            if (flag4)
            {
              this.ProcessDuplicateAlternateNode(childNode, "FirstLineIndent", "FirstLineHanging");
              return (FrameToken) null;
            }
            flag4 = true;
            frameToken.frameInfoDefinition.firstLine = this.LoadPositiveOrZeroIntegerValue(childNode, out success);
            if (!success)
              return (FrameToken) null;
          }
          else if (this.MatchNodeName(childNode, "FirstLineHanging"))
          {
            if (flag5)
            {
              this.ProcessDuplicateAlternateNode(childNode, "FirstLineIndent", "FirstLineHanging");
              return (FrameToken) null;
            }
            flag5 = true;
            frameToken.frameInfoDefinition.firstLine = this.LoadPositiveOrZeroIntegerValue(childNode, out success);
            if (!success)
              return (FrameToken) null;
            frameToken.frameInfoDefinition.firstLine = -frameToken.frameInfoDefinition.firstLine;
          }
          else if (this.MatchNodeName(childNode, "CustomItem"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (FrameToken) null;
            }
            flag1 = true;
            frameToken.itemDefinition.formatTokenList = this.LoadComplexControlTokenListDefinitions(childNode);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (flag5 && flag4)
        {
          this.ProcessDuplicateAlternateNode("FirstLineIndent", "FirstLineHanging");
          return (FrameToken) null;
        }
        if (frameToken.itemDefinition.formatTokenList != null)
          return frameToken;
        this.ReportError(XmlLoadingResourceManager.FormatString("MissingNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "CustomItem"));
        return (FrameToken) null;
      }
    }

    private bool ReadBooleanNode(XmlNode collectionElement, out bool val)
    {
      val = false;
      if (!this.VerifyNodeHasNoChildren(collectionElement))
        return false;
      string innerText = collectionElement.InnerText;
      if (string.IsNullOrEmpty(innerText))
      {
        val = true;
        return true;
      }
      if (string.Equals(innerText, "FALSE", StringComparison.OrdinalIgnoreCase))
      {
        val = false;
        return true;
      }
      if (string.Equals(innerText, "TRUE", StringComparison.OrdinalIgnoreCase))
      {
        val = true;
        return true;
      }
      this.ReportError(XmlLoadingResourceManager.FormatString("ExpectBoolean", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
      return false;
    }

    private ListControlBody LoadListControl(XmlNode controlNode)
    {
      using (this.StackFrame(controlNode))
      {
        ListControlBody listBody = new ListControlBody();
        bool flag = false;
        foreach (XmlNode childNode in controlNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "ListEntries"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(childNode);
            }
            else
            {
              flag = true;
              this.LoadListControlEntries(childNode, listBody);
              if (listBody.defaultEntryDefinition == null)
                return (ListControlBody) null;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (flag)
          return listBody;
        this.ReportMissingNode("ListEntries");
        return (ListControlBody) null;
      }
    }

    private void LoadListControlEntries(XmlNode listViewEntriesNode, ListControlBody listBody)
    {
      using (this.StackFrame(listViewEntriesNode))
      {
        int num = 0;
        foreach (XmlNode childNode in listViewEntriesNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "ListEntry"))
          {
            ListControlEntryDefinition controlEntryDefinition = this.LoadListControlEntryDefinition(childNode, num++);
            if (controlEntryDefinition == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "ListEntry"));
              listBody.defaultEntryDefinition = (ListControlEntryDefinition) null;
              return;
            }
            if (controlEntryDefinition.appliesTo == null)
            {
              if (listBody.defaultEntryDefinition == null)
              {
                listBody.defaultEntryDefinition = controlEntryDefinition;
              }
              else
              {
                this.ReportError(XmlLoadingResourceManager.FormatString("TooManyDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "ListEntry"));
                listBody.defaultEntryDefinition = (ListControlEntryDefinition) null;
                return;
              }
            }
            else
              listBody.optionalEntryList.Add(controlEntryDefinition);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (listBody.optionalEntryList != null)
          return;
        this.ReportError(XmlLoadingResourceManager.FormatString("NoDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "ListEntry"));
      }
    }

    private ListControlEntryDefinition LoadListControlEntryDefinition(
      XmlNode listViewEntryNode,
      int index)
    {
      using (this.StackFrame(listViewEntryNode, index))
      {
        bool flag1 = false;
        bool flag2 = false;
        ListControlEntryDefinition lved = new ListControlEntryDefinition();
        foreach (XmlNode childNode in listViewEntryNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "EntrySelectedBy"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (ListControlEntryDefinition) null;
            }
            flag1 = true;
            lved.appliesTo = this.LoadAppliesToSection(childNode, true);
          }
          else if (this.MatchNodeName(childNode, "ListItems"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (ListControlEntryDefinition) null;
            }
            flag2 = true;
            this.LoadListControlItemDefinitions(lved, childNode);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (lved.itemDefinitionList != null)
          return lved;
        this.ReportError(XmlLoadingResourceManager.FormatString("NoDefinitionList", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return (ListControlEntryDefinition) null;
      }
    }

    private void LoadListControlItemDefinitions(ListControlEntryDefinition lved, XmlNode bodyNode)
    {
      using (this.StackFrame(bodyNode))
      {
        int num = 0;
        foreach (XmlNode childNode in bodyNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "ListItem"))
          {
            ++num;
            ListControlItemDefinition controlItemDefinition = this.LoadListControlItemDefinition(childNode);
            if (controlItemDefinition == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidPropertyEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
              lved.itemDefinitionList = (List<ListControlItemDefinition>) null;
              return;
            }
            lved.itemDefinitionList.Add(controlItemDefinition);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (lved.itemDefinitionList.Count != 0)
          return;
        this.ReportError(XmlLoadingResourceManager.FormatString("NoListViewItem", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        lved.itemDefinitionList = (List<ListControlItemDefinition>) null;
      }
    }

    private ListControlItemDefinition LoadListControlItemDefinition(
      XmlNode propertyEntryNode)
    {
      using (this.StackFrame(propertyEntryNode))
      {
        TypeInfoDataBaseLoader.ViewEntryNodeMatch viewEntryNodeMatch = new TypeInfoDataBaseLoader.ViewEntryNodeMatch(this);
        List<XmlNode> unprocessedNodes = new List<XmlNode>();
        if (!viewEntryNodeMatch.ProcessExpressionDirectives(propertyEntryNode, unprocessedNodes))
          return (ListControlItemDefinition) null;
        TextToken textToken = (TextToken) null;
        ExpressionToken expressionToken = (ExpressionToken) null;
        bool flag1 = false;
        bool flag2 = false;
        foreach (XmlNode xmlNode in unprocessedNodes)
        {
          if (this.MatchNodeName(xmlNode, "ItemSelectionCondition"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (ListControlItemDefinition) null;
            }
            flag2 = true;
            expressionToken = this.LoadItemSelectionCondition(xmlNode);
            if (expressionToken == null)
              return (ListControlItemDefinition) null;
          }
          else if (this.MatchNodeNameWithAttributes(xmlNode, "Label"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (ListControlItemDefinition) null;
            }
            flag1 = true;
            textToken = this.LoadLabel(xmlNode);
            if (textToken == null)
              return (ListControlItemDefinition) null;
          }
          else
            this.ProcessUnknownNode(xmlNode);
        }
        ListControlItemDefinition controlItemDefinition = new ListControlItemDefinition();
        controlItemDefinition.label = textToken;
        controlItemDefinition.conditionToken = expressionToken;
        if (viewEntryNodeMatch.TextToken != null)
        {
          controlItemDefinition.formatTokenList.Add((FormatToken) viewEntryNodeMatch.TextToken);
        }
        else
        {
          FieldPropertyToken fieldPropertyToken = new FieldPropertyToken();
          fieldPropertyToken.expression = viewEntryNodeMatch.Expression;
          fieldPropertyToken.fieldFormattingDirective.formatString = viewEntryNodeMatch.FormatString;
          controlItemDefinition.formatTokenList.Add((FormatToken) fieldPropertyToken);
        }
        return controlItemDefinition;
      }
    }

    private ExpressionToken LoadItemSelectionCondition(XmlNode itemNode)
    {
      using (this.StackFrame(itemNode))
      {
        bool flag = false;
        TypeInfoDataBaseLoader.ExpressionNodeMatch expressionNodeMatch = new TypeInfoDataBaseLoader.ExpressionNodeMatch(this);
        foreach (XmlNode childNode in itemNode.ChildNodes)
        {
          if (expressionNodeMatch.MatchNode(childNode))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(childNode);
              return (ExpressionToken) null;
            }
            flag = true;
            if (!expressionNodeMatch.ProcessNode(childNode))
              return (ExpressionToken) null;
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        return expressionNodeMatch.GenerateExpressionToken();
      }
    }

    private ControlBase LoadTableControl(XmlNode controlNode)
    {
      using (this.StackFrame(controlNode))
      {
        TableControlBody tableBody = new TableControlBody();
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        foreach (XmlNode childNode in controlNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "HideTableHeaders"))
          {
            if (flag3)
            {
              this.ProcessDuplicateNode(childNode);
              return (ControlBase) null;
            }
            flag3 = true;
            if (!this.ReadBooleanNode(childNode, out tableBody.header.hideHeader))
              return (ControlBase) null;
          }
          else if (this.MatchNodeName(childNode, "AutoSize"))
          {
            if (flag4)
            {
              this.ProcessDuplicateNode(childNode);
              return (ControlBase) null;
            }
            flag4 = true;
            bool val;
            if (!this.ReadBooleanNode(childNode, out val))
              return (ControlBase) null;
            tableBody.autosize = new bool?(val);
          }
          else if (this.MatchNodeName(childNode, "TableHeaders"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (ControlBase) null;
            }
            flag1 = true;
            this.LoadHeadersSection(tableBody, childNode);
            if (tableBody.header.columnHeaderDefinitionList == null)
              return (ControlBase) null;
          }
          else if (this.MatchNodeName(childNode, "TableRowEntries"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (ControlBase) null;
            }
            flag2 = true;
            this.LoadRowEntriesSection(tableBody, childNode);
            if (tableBody.defaultDefinition == null)
              return (ControlBase) null;
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (!flag2)
        {
          this.ReportMissingNode("TableRowEntries");
          return (ControlBase) null;
        }
        if (tableBody.header.columnHeaderDefinitionList.Count != 0 && tableBody.header.columnHeaderDefinitionList.Count != tableBody.defaultDefinition.rowItemDefinitionList.Count)
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("IncorrectHeaderItemCount", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) tableBody.header.columnHeaderDefinitionList.Count, (object) tableBody.defaultDefinition.rowItemDefinitionList.Count));
          return (ControlBase) null;
        }
        if (tableBody.optionalDefinitionList.Count != 0)
        {
          int num = 0;
          foreach (TableRowDefinition optionalDefinition in tableBody.optionalDefinitionList)
          {
            if (optionalDefinition.rowItemDefinitionList.Count != tableBody.defaultDefinition.rowItemDefinitionList.Count)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("IncorrectRowItemCount", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) optionalDefinition.rowItemDefinitionList.Count, (object) tableBody.defaultDefinition.rowItemDefinitionList.Count, (object) (num + 1)));
              return (ControlBase) null;
            }
            ++num;
          }
        }
        return (ControlBase) tableBody;
      }
    }

    private void LoadHeadersSection(TableControlBody tableBody, XmlNode headersNode)
    {
      using (this.StackFrame(headersNode))
      {
        int num = 0;
        foreach (XmlNode childNode in headersNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "TableColumnHeader"))
          {
            TableColumnHeaderDefinition headerDefinition = this.LoadColumnHeaderDefinition(childNode, num++);
            if (headerDefinition != null)
            {
              tableBody.header.columnHeaderDefinitionList.Add(headerDefinition);
            }
            else
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidColumnHeader", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
              tableBody.header.columnHeaderDefinitionList = (List<TableColumnHeaderDefinition>) null;
              break;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private TableColumnHeaderDefinition LoadColumnHeaderDefinition(
      XmlNode columnHeaderNode,
      int index)
    {
      using (this.StackFrame(columnHeaderNode, index))
      {
        TableColumnHeaderDefinition headerDefinition = new TableColumnHeaderDefinition();
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        foreach (XmlNode childNode in columnHeaderNode.ChildNodes)
        {
          if (this.MatchNodeNameWithAttributes(childNode, "Label"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (TableColumnHeaderDefinition) null;
            }
            flag1 = true;
            headerDefinition.label = this.LoadLabel(childNode);
            if (headerDefinition.label == null)
              return (TableColumnHeaderDefinition) null;
          }
          else if (this.MatchNodeName(childNode, "Width"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (TableColumnHeaderDefinition) null;
            }
            flag2 = true;
            int val;
            if (this.ReadPositiveIntegerValue(childNode, out val))
            {
              headerDefinition.width = val;
            }
            else
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidNodeValue", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "Width"));
              return (TableColumnHeaderDefinition) null;
            }
          }
          else if (this.MatchNodeName(childNode, "Alignment"))
          {
            if (flag3)
            {
              this.ProcessDuplicateNode(childNode);
              return (TableColumnHeaderDefinition) null;
            }
            flag3 = true;
            if (!this.LoadAlignmentValue(childNode, out headerDefinition.alignment))
              return (TableColumnHeaderDefinition) null;
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        return headerDefinition;
      }
    }

    private bool ReadPositiveIntegerValue(XmlNode n, out int val)
    {
      val = -1;
      string mandatoryInnerText = this.GetMandatoryInnerText(n);
      if (mandatoryInnerText == null)
        return false;
      if (int.TryParse(mandatoryInnerText, out val) && val > 0)
        return true;
      this.ReportError(XmlLoadingResourceManager.FormatString("ExpectPositiveInteger", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
      return false;
    }

    private bool LoadAlignmentValue(XmlNode n, out int alignmentValue)
    {
      alignmentValue = 0;
      string mandatoryInnerText = this.GetMandatoryInnerText(n);
      if (mandatoryInnerText == null)
        return false;
      if (string.Equals(n.InnerText, "left", StringComparison.OrdinalIgnoreCase))
        alignmentValue = 1;
      else if (string.Equals(n.InnerText, "right", StringComparison.OrdinalIgnoreCase))
        alignmentValue = 3;
      else if (string.Equals(n.InnerText, "center", StringComparison.OrdinalIgnoreCase))
      {
        alignmentValue = 2;
      }
      else
      {
        this.ReportError(XmlLoadingResourceManager.FormatString("InvalidAlignmentValue", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) mandatoryInnerText));
        return false;
      }
      return true;
    }

    private void LoadRowEntriesSection(TableControlBody tableBody, XmlNode rowEntriesNode)
    {
      using (this.StackFrame(rowEntriesNode))
      {
        int num = 0;
        foreach (XmlNode childNode in rowEntriesNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "TableRowEntry"))
          {
            TableRowDefinition tableRowDefinition = this.LoadRowEntryDefinition(childNode, num++);
            if (tableRowDefinition == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("LoadTagFailed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "TableRowEntry"));
              tableBody.defaultDefinition = (TableRowDefinition) null;
              return;
            }
            if (tableRowDefinition.appliesTo == null)
            {
              if (tableBody.defaultDefinition == null)
              {
                tableBody.defaultDefinition = tableRowDefinition;
              }
              else
              {
                this.ReportError(XmlLoadingResourceManager.FormatString("TooManyDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "TableRowEntry"));
                tableBody.defaultDefinition = (TableRowDefinition) null;
                return;
              }
            }
            else
              tableBody.optionalDefinitionList.Add(tableRowDefinition);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (tableBody.defaultDefinition != null)
          return;
        this.ReportError(XmlLoadingResourceManager.FormatString("NoDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "TableRowEntry"));
      }
    }

    private TableRowDefinition LoadRowEntryDefinition(
      XmlNode rowEntryNode,
      int index)
    {
      using (this.StackFrame(rowEntryNode, index))
      {
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        TableRowDefinition trd = new TableRowDefinition();
        foreach (XmlNode childNode in rowEntryNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "EntrySelectedBy"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (TableRowDefinition) null;
            }
            flag1 = true;
            trd.appliesTo = this.LoadAppliesToSection(childNode, true);
          }
          else if (this.MatchNodeName(childNode, "TableColumnItems"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (TableRowDefinition) null;
            }
            this.LoadColumnEntries(childNode, trd);
            if (trd.rowItemDefinitionList == null)
              return (TableRowDefinition) null;
          }
          else if (this.MatchNodeName(childNode, "Wrap"))
          {
            if (flag3)
            {
              this.ProcessDuplicateNode(childNode);
              return (TableRowDefinition) null;
            }
            flag3 = true;
            if (!this.ReadBooleanNode(childNode, out trd.multiLine))
              return (TableRowDefinition) null;
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        return trd;
      }
    }

    private void LoadColumnEntries(XmlNode columnEntriesNode, TableRowDefinition trd)
    {
      using (this.StackFrame(columnEntriesNode))
      {
        int num = 0;
        foreach (XmlNode childNode in columnEntriesNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "TableColumnItem"))
          {
            TableRowItemDefinition rowItemDefinition = this.LoadColumnEntry(childNode, num++);
            if (rowItemDefinition != null)
            {
              trd.rowItemDefinitionList.Add(rowItemDefinition);
            }
            else
            {
              trd.rowItemDefinitionList = (List<TableRowItemDefinition>) null;
              break;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private TableRowItemDefinition LoadColumnEntry(
      XmlNode columnEntryNode,
      int index)
    {
      using (this.StackFrame(columnEntryNode, index))
      {
        TypeInfoDataBaseLoader.ViewEntryNodeMatch viewEntryNodeMatch = new TypeInfoDataBaseLoader.ViewEntryNodeMatch(this);
        List<XmlNode> unprocessedNodes = new List<XmlNode>();
        if (!viewEntryNodeMatch.ProcessExpressionDirectives(columnEntryNode, unprocessedNodes))
          return (TableRowItemDefinition) null;
        TableRowItemDefinition rowItemDefinition = new TableRowItemDefinition();
        bool flag = false;
        foreach (XmlNode n in unprocessedNodes)
        {
          if (this.MatchNodeName(n, "Alignment"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(n);
              return (TableRowItemDefinition) null;
            }
            flag = true;
            if (!this.LoadAlignmentValue(n, out rowItemDefinition.alignment))
              return (TableRowItemDefinition) null;
          }
          else
            this.ProcessUnknownNode(n);
        }
        if (viewEntryNodeMatch.TextToken != null)
          rowItemDefinition.formatTokenList.Add((FormatToken) viewEntryNodeMatch.TextToken);
        else if (viewEntryNodeMatch.Expression != null)
        {
          FieldPropertyToken fieldPropertyToken = new FieldPropertyToken();
          fieldPropertyToken.expression = viewEntryNodeMatch.Expression;
          fieldPropertyToken.fieldFormattingDirective.formatString = viewEntryNodeMatch.FormatString;
          rowItemDefinition.formatTokenList.Add((FormatToken) fieldPropertyToken);
        }
        return rowItemDefinition;
      }
    }

    private void LoadViewDefinitions(TypeInfoDataBase db, XmlNode viewDefinitionsNode)
    {
      using (this.StackFrame(viewDefinitionsNode))
      {
        int num = 0;
        foreach (XmlNode childNode in viewDefinitionsNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "View"))
          {
            ViewDefinition viewDefinition = this.LoadView(childNode, num++);
            if (viewDefinition != null)
            {
              this.ReportTrace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} view {1} is loaded from file {2}", (object) ControlBase.GetControlShapeName(viewDefinition.mainControl), (object) viewDefinition.name, (object) viewDefinition.loadingInfo.filePath));
              db.viewDefinitionsSection.viewDefinitionList.Add(viewDefinition);
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private ViewDefinition LoadView(XmlNode viewNode, int index)
    {
      using (this.StackFrame(viewNode, index))
      {
        ViewDefinition view = new ViewDefinition();
        List<XmlNode> unprocessedNodes1 = new List<XmlNode>();
        if (!this.LoadCommonViewData(viewNode, view, unprocessedNodes1))
        {
          this.ReportError(XmlLoadingResourceManager.FormatString("ViewNotLoaded", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
          return (ViewDefinition) null;
        }
        string[] names = new string[4]
        {
          "TableControl",
          "ListControl",
          "WideControl",
          "CustomControl"
        };
        List<XmlNode> unprocessedNodes2 = new List<XmlNode>();
        bool flag = false;
        foreach (XmlNode xmlNode in unprocessedNodes1)
        {
          if (this.MatchNodeName(xmlNode, "TableControl"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (ViewDefinition) null;
            }
            flag = true;
            view.mainControl = this.LoadTableControl(xmlNode);
          }
          else if (this.MatchNodeName(xmlNode, "ListControl"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (ViewDefinition) null;
            }
            flag = true;
            view.mainControl = (ControlBase) this.LoadListControl(xmlNode);
          }
          else if (this.MatchNodeName(xmlNode, "WideControl"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (ViewDefinition) null;
            }
            flag = true;
            view.mainControl = (ControlBase) this.LoadWideControl(xmlNode);
          }
          else if (this.MatchNodeName(xmlNode, "CustomControl"))
          {
            if (flag)
            {
              this.ProcessDuplicateNode(xmlNode);
              return (ViewDefinition) null;
            }
            flag = true;
            view.mainControl = (ControlBase) this.LoadComplexControl(xmlNode);
          }
          else
            unprocessedNodes2.Add(xmlNode);
        }
        if (view.mainControl == null)
        {
          this.ReportMissingNodes(names);
          return (ViewDefinition) null;
        }
        if (!this.LoadMainControlDependentData(unprocessedNodes2, view))
          return (ViewDefinition) null;
        if (!view.outOfBand || view.groupBy == null)
          return view;
        this.ReportError(XmlLoadingResourceManager.FormatString("OutOfBandGroupByConflict", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
        return (ViewDefinition) null;
      }
    }

    private bool LoadMainControlDependentData(List<XmlNode> unprocessedNodes, ViewDefinition view)
    {
      foreach (XmlNode unprocessedNode in unprocessedNodes)
      {
        bool flag1 = false;
        bool flag2 = false;
        if (this.MatchNodeName(unprocessedNode, "OutOfBand"))
        {
          if (flag1)
          {
            this.ProcessDuplicateNode(unprocessedNode);
            return false;
          }
          if (!this.ReadBooleanNode(unprocessedNode, out view.outOfBand))
            return false;
          if (!(view.mainControl is ComplexControlBody) && !(view.mainControl is ListControlBody))
          {
            this.ReportError(XmlLoadingResourceManager.FormatString("InvalidControlForOutOfBandView", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
            return false;
          }
        }
        else if (this.MatchNodeName(unprocessedNode, "Controls"))
        {
          if (flag2)
          {
            this.ProcessDuplicateNode(unprocessedNode);
            return false;
          }
          this.LoadControlDefinitions(unprocessedNode, view.formatControlDefinitionHolder.controlDefinitionList);
        }
        else
          this.ProcessUnknownNode(unprocessedNode);
      }
      return true;
    }

    private bool LoadCommonViewData(
      XmlNode viewNode,
      ViewDefinition view,
      List<XmlNode> unprocessedNodes)
    {
      if (viewNode == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (viewNode));
      if (view == null)
        throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (view));
      view.loadingInfo = this.LoadingInfo;
      view.loadingInfo.xPath = this.ComputeCurrentXPath();
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      foreach (XmlNode childNode in viewNode.ChildNodes)
      {
        if (this.MatchNodeName(childNode, "Name"))
        {
          if (flag1)
          {
            this.ProcessDuplicateNode(childNode);
            return false;
          }
          flag1 = true;
          view.name = this.GetMandatoryInnerText(childNode);
          if (view.name == null)
            return false;
        }
        else if (this.MatchNodeName(childNode, "ViewSelectedBy"))
        {
          if (flag2)
          {
            this.ProcessDuplicateNode(childNode);
            return false;
          }
          flag2 = true;
          view.appliesTo = this.LoadAppliesToSection(childNode, false);
          if (view.appliesTo == null)
            return false;
        }
        else if (this.MatchNodeName(childNode, "GroupBy"))
        {
          if (flag3)
          {
            this.ProcessDuplicateNode(childNode);
            return false;
          }
          flag3 = true;
          view.groupBy = this.LoadGroupBySection(childNode);
          if (view.groupBy == null)
            return false;
        }
        else
          unprocessedNodes.Add(childNode);
      }
      if (!flag1)
      {
        this.ReportMissingNode("Name");
        return false;
      }
      if (flag2)
        return true;
      this.ReportMissingNode("ViewSelectedBy");
      return false;
    }

    private void LoadControlDefinitions(
      XmlNode definitionsNode,
      List<ControlDefinition> controlDefinitionList)
    {
      using (this.StackFrame(definitionsNode))
      {
        int num = 0;
        foreach (XmlNode childNode in definitionsNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "Control"))
          {
            ControlDefinition controlDefinition = this.LoadControlDefinition(childNode, num++);
            if (controlDefinition != null)
              controlDefinitionList.Add(controlDefinition);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
      }
    }

    private ControlDefinition LoadControlDefinition(
      XmlNode controlDefinitionNode,
      int index)
    {
      using (this.StackFrame(controlDefinitionNode, index))
      {
        bool flag1 = false;
        bool flag2 = false;
        ControlDefinition controlDefinition = new ControlDefinition();
        foreach (XmlNode childNode in controlDefinitionNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "Name"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
            }
            else
            {
              flag1 = true;
              controlDefinition.name = this.GetMandatoryInnerText(childNode);
              if (controlDefinition.name == null)
              {
                this.ReportError(XmlLoadingResourceManager.FormatString("NullControlName", (object) this.ComputeCurrentXPath(), (object) this.FilePath));
                return (ControlDefinition) null;
              }
            }
          }
          else if (this.MatchNodeName(childNode, "CustomControl"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (ControlDefinition) null;
            }
            flag2 = true;
            controlDefinition.controlBody = (ControlBody) this.LoadComplexControl(childNode);
            if (controlDefinition.controlBody == null)
              return (ControlDefinition) null;
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (controlDefinition.name == null)
        {
          this.ReportMissingNode("Name");
          return (ControlDefinition) null;
        }
        if (controlDefinition.controlBody != null)
          return controlDefinition;
        this.ReportMissingNode("CustomControl");
        return (ControlDefinition) null;
      }
    }

    private WideControlBody LoadWideControl(XmlNode controlNode)
    {
      using (this.StackFrame(controlNode))
      {
        WideControlBody wideBody = new WideControlBody();
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        foreach (XmlNode childNode in controlNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "AutoSize"))
          {
            if (flag2)
            {
              this.ProcessDuplicateAlternateNode(childNode, "AutoSize", "ColumnNumber");
              return (WideControlBody) null;
            }
            flag2 = true;
            bool val;
            if (!this.ReadBooleanNode(childNode, out val))
              return (WideControlBody) null;
            wideBody.autosize = new bool?(val);
          }
          else if (this.MatchNodeName(childNode, "ColumnNumber"))
          {
            if (flag3)
            {
              this.ProcessDuplicateAlternateNode(childNode, "AutoSize", "ColumnNumber");
              return (WideControlBody) null;
            }
            flag3 = true;
            if (!this.ReadPositiveIntegerValue(childNode, out wideBody.columns))
              return (WideControlBody) null;
          }
          else if (this.MatchNodeName(childNode, "WideEntries"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
            }
            else
            {
              flag1 = true;
              this.LoadWideControlEntries(childNode, wideBody);
              if (wideBody.defaultEntryDefinition == null)
                return (WideControlBody) null;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (flag2 && flag3)
        {
          this.ProcessDuplicateAlternateNode("AutoSize", "ColumnNumber");
          return (WideControlBody) null;
        }
        if (flag1)
          return wideBody;
        this.ReportMissingNode("WideEntries");
        return (WideControlBody) null;
      }
    }

    private void LoadWideControlEntries(XmlNode wideControlEntriesNode, WideControlBody wideBody)
    {
      using (this.StackFrame(wideControlEntriesNode))
      {
        int num = 0;
        foreach (XmlNode childNode in wideControlEntriesNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "WideEntry"))
          {
            WideControlEntryDefinition controlEntryDefinition = this.LoadWideControlEntry(childNode, num++);
            if (controlEntryDefinition == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "WideEntry"));
              return;
            }
            if (controlEntryDefinition.appliesTo == null)
            {
              if (wideBody.defaultEntryDefinition == null)
              {
                wideBody.defaultEntryDefinition = controlEntryDefinition;
              }
              else
              {
                this.ReportError(XmlLoadingResourceManager.FormatString("TooManyDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "WideEntry"));
                wideBody.defaultEntryDefinition = (WideControlEntryDefinition) null;
                return;
              }
            }
            else
              wideBody.optionalEntryList.Add(controlEntryDefinition);
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (wideBody.defaultEntryDefinition != null)
          return;
        this.ReportError(XmlLoadingResourceManager.FormatString("NoDefaultShapeEntry", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "WideEntry"));
      }
    }

    private WideControlEntryDefinition LoadWideControlEntry(
      XmlNode wideControlEntryNode,
      int index)
    {
      using (this.StackFrame(wideControlEntryNode, index))
      {
        bool flag1 = false;
        bool flag2 = false;
        WideControlEntryDefinition controlEntryDefinition = new WideControlEntryDefinition();
        foreach (XmlNode childNode in wideControlEntryNode.ChildNodes)
        {
          if (this.MatchNodeName(childNode, "EntrySelectedBy"))
          {
            if (flag1)
            {
              this.ProcessDuplicateNode(childNode);
              return (WideControlEntryDefinition) null;
            }
            flag1 = true;
            controlEntryDefinition.appliesTo = this.LoadAppliesToSection(childNode, true);
          }
          else if (this.MatchNodeName(childNode, "WideItem"))
          {
            if (flag2)
            {
              this.ProcessDuplicateNode(childNode);
              return (WideControlEntryDefinition) null;
            }
            flag2 = true;
            controlEntryDefinition.formatTokenList = this.LoadPropertyEntry(childNode);
            if (controlEntryDefinition.formatTokenList == null)
            {
              this.ReportError(XmlLoadingResourceManager.FormatString("InvalidNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) "WideItem"));
              return (WideControlEntryDefinition) null;
            }
          }
          else
            this.ProcessUnknownNode(childNode);
        }
        if (controlEntryDefinition.formatTokenList.Count != 0)
          return controlEntryDefinition;
        this.ReportMissingNode("WideItem");
        return (WideControlEntryDefinition) null;
      }
    }

    private List<FormatToken> LoadPropertyEntry(XmlNode propertyEntryNode)
    {
      using (this.StackFrame(propertyEntryNode))
      {
        TypeInfoDataBaseLoader.ViewEntryNodeMatch viewEntryNodeMatch = new TypeInfoDataBaseLoader.ViewEntryNodeMatch(this);
        List<XmlNode> unprocessedNodes = new List<XmlNode>();
        if (!viewEntryNodeMatch.ProcessExpressionDirectives(propertyEntryNode, unprocessedNodes))
          return (List<FormatToken>) null;
        foreach (XmlNode n in unprocessedNodes)
          this.ProcessUnknownNode(n);
        List<FormatToken> formatTokenList = new List<FormatToken>();
        if (viewEntryNodeMatch.TextToken != null)
        {
          formatTokenList.Add((FormatToken) viewEntryNodeMatch.TextToken);
        }
        else
        {
          FieldPropertyToken fieldPropertyToken = new FieldPropertyToken();
          fieldPropertyToken.expression = viewEntryNodeMatch.Expression;
          fieldPropertyToken.fieldFormattingDirective.formatString = viewEntryNodeMatch.FormatString;
          formatTokenList.Add((FormatToken) fieldPropertyToken);
        }
        return formatTokenList;
      }
    }

    private static class XmlTags
    {
      internal const string DefaultSettingsNode = "DefaultSettings";
      internal const string ConfigurationNode = "Configuration";
      internal const string SelectionSetsNode = "SelectionSets";
      internal const string ViewDefinitionsNode = "ViewDefinitions";
      internal const string ControlsNode = "Controls";
      internal const string MultilineTablesNode = "WrapTables";
      internal const string PropertyCountForTableNode = "PropertyCountForTable";
      internal const string ShowErrorsAsMessagesNode = "ShowError";
      internal const string ShowErrorsInFormattedOutputNode = "DisplayError";
      internal const string EnumerableExpansionsNode = "EnumerableExpansions";
      internal const string EnumerableExpansionNode = "EnumerableExpansion";
      internal const string ExpandNode = "Expand";
      internal const string ControlNode = "Control";
      internal const string ComplexControlNameNode = "CustomControlName";
      internal const string SelectionSetNode = "SelectionSet";
      internal const string SelectionSetNameNode = "SelectionSetName";
      internal const string SelectionConditionNode = "SelectionCondition";
      internal const string NameNode = "Name";
      internal const string TypesNode = "Types";
      internal const string TypeNameNode = "TypeName";
      internal const string ViewNode = "View";
      internal const string TableControlNode = "TableControl";
      internal const string ListControlNode = "ListControl";
      internal const string WideControlNode = "WideControl";
      internal const string ComplexControlNode = "CustomControl";
      internal const string FieldControlNode = "FieldControl";
      internal const string ViewSelectedByNode = "ViewSelectedBy";
      internal const string GroupByNode = "GroupBy";
      internal const string OutOfBandNode = "OutOfBand";
      internal const string HideTableHeadersNode = "HideTableHeaders";
      internal const string TableHeadersNode = "TableHeaders";
      internal const string TableColumnHeaderNode = "TableColumnHeader";
      internal const string TableRowEntriesNode = "TableRowEntries";
      internal const string TableRowEntryNode = "TableRowEntry";
      internal const string MultiLineNode = "Wrap";
      internal const string TableColumnItemsNode = "TableColumnItems";
      internal const string TableColumnItemNode = "TableColumnItem";
      internal const string WidthNode = "Width";
      internal const string ListEntriesNode = "ListEntries";
      internal const string ListEntryNode = "ListEntry";
      internal const string ListItemsNode = "ListItems";
      internal const string ListItemNode = "ListItem";
      internal const string ColumnNumberNode = "ColumnNumber";
      internal const string WideEntriesNode = "WideEntries";
      internal const string WideEntryNode = "WideEntry";
      internal const string WideItemNode = "WideItem";
      internal const string ComplexEntriesNode = "CustomEntries";
      internal const string ComplexEntryNode = "CustomEntry";
      internal const string ComplexItemNode = "CustomItem";
      internal const string ExpressionBindingNode = "ExpressionBinding";
      internal const string NewLineNode = "NewLine";
      internal const string TextNode = "Text";
      internal const string FrameNode = "Frame";
      internal const string LeftIndentNode = "LeftIndent";
      internal const string RightIndentNode = "RightIndent";
      internal const string FirstLineIndentNode = "FirstLineIndent";
      internal const string FirstLineHangingNode = "FirstLineHanging";
      internal const string EnumerateCollectionNode = "EnumerateCollection";
      internal const string AutoSizeNode = "AutoSize";
      internal const string AlignmentNode = "Alignment";
      internal const string PropertyNameNode = "PropertyName";
      internal const string ScriptBlockNode = "ScriptBlock";
      internal const string FormatStringNode = "FormatString";
      internal const string LabelNode = "Label";
      internal const string EntrySelectedByNode = "EntrySelectedBy";
      internal const string ItemSelectionConditionNode = "ItemSelectionCondition";
      internal const string AssemblyNameAttribute = "AssemblyName";
      internal const string BaseNameAttribute = "BaseName";
      internal const string ResourceIdAttribute = "ResourceId";
    }

    private static class XMLStringValues
    {
      internal const string True = "TRUE";
      internal const string False = "FALSE";
      internal const string AligmentLeft = "left";
      internal const string AligmentCenter = "center";
      internal const string AligmentRight = "right";
    }

    private sealed class ExpressionNodeMatch
    {
      private TypeInfoDataBaseLoader _loader;
      private ExpressionToken _token;
      private bool _fatalError;

      internal ExpressionNodeMatch(TypeInfoDataBaseLoader loader) => this._loader = loader;

      internal bool MatchNode(XmlNode n) => this._loader.MatchNodeName(n, "PropertyName") || this._loader.MatchNodeName(n, "ScriptBlock");

      internal bool ProcessNode(XmlNode n)
      {
        if (this._loader.MatchNodeName(n, "PropertyName"))
        {
          if (this._token != null)
          {
            if (this._token.isScriptBlock)
              this._loader.ProcessDuplicateAlternateNode(n, "PropertyName", "ScriptBlock");
            else
              this._loader.ProcessDuplicateNode(n);
            return false;
          }
          this._token = new ExpressionToken();
          this._token.expressionValue = this._loader.GetMandatoryInnerText(n);
          if (this._token.expressionValue != null)
            return true;
          this._loader.ReportError(XmlLoadingResourceManager.FormatString("NoProperty", (object) this._loader.ComputeCurrentXPath(), (object) this._loader.FilePath));
          this._fatalError = true;
          return false;
        }
        if (this._loader.MatchNodeName(n, "ScriptBlock"))
        {
          if (this._token != null)
          {
            if (!this._token.isScriptBlock)
              this._loader.ProcessDuplicateAlternateNode(n, "PropertyName", "ScriptBlock");
            else
              this._loader.ProcessDuplicateNode(n);
            return false;
          }
          this._token = new ExpressionToken();
          this._token.isScriptBlock = true;
          this._token.expressionValue = this._loader.GetMandatoryInnerText(n);
          if (this._token.expressionValue == null)
          {
            this._loader.ReportError(XmlLoadingResourceManager.FormatString("NoScriptBlockText", (object) this._loader.ComputeCurrentXPath(), (object) this._loader.FilePath));
            this._fatalError = true;
            return false;
          }
          if (this._loader.VerifyScriptBlock(this._token.expressionValue))
            return true;
          this._fatalError = true;
          return false;
        }
        TypeInfoDataBaseLoader.tracer.NewInvalidOperationException();
        return false;
      }

      internal ExpressionToken GenerateExpressionToken()
      {
        if (this._fatalError)
          return (ExpressionToken) null;
        if (this._token != null)
          return this._token;
        this._loader.ReportMissingNodes(new string[2]
        {
          "PropertyName",
          "ScriptBlock"
        });
        return (ExpressionToken) null;
      }
    }

    private sealed class ViewEntryNodeMatch
    {
      private string _formatString;
      private TextToken _textToken;
      private ExpressionToken _expression;
      private TypeInfoDataBaseLoader _loader;

      internal ViewEntryNodeMatch(TypeInfoDataBaseLoader loader) => this._loader = loader;

      internal bool ProcessExpressionDirectives(
        XmlNode containerNode,
        List<XmlNode> unprocessedNodes)
      {
        if (containerNode == null)
          throw TypeInfoDataBaseLoader.tracer.NewArgumentNullException(nameof (containerNode));
        string str = (string) null;
        TextToken textToken = (TextToken) null;
        TypeInfoDataBaseLoader.ExpressionNodeMatch expressionNodeMatch = new TypeInfoDataBaseLoader.ExpressionNodeMatch(this._loader);
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        foreach (XmlNode childNode in containerNode.ChildNodes)
        {
          if (expressionNodeMatch.MatchNode(childNode))
          {
            if (flag2)
            {
              this._loader.ProcessDuplicateNode(childNode);
              return false;
            }
            flag2 = true;
            if (!expressionNodeMatch.ProcessNode(childNode))
              return false;
          }
          else if (this._loader.MatchNodeName(childNode, "FormatString"))
          {
            if (flag1)
            {
              this._loader.ProcessDuplicateNode(childNode);
              return false;
            }
            flag1 = true;
            str = this._loader.GetMandatoryInnerText(childNode);
            if (str == null)
            {
              this._loader.ReportError(XmlLoadingResourceManager.FormatString("NoFormatString", (object) this._loader.ComputeCurrentXPath(), (object) this._loader.FilePath));
              return false;
            }
          }
          else if (this._loader.MatchNodeNameWithAttributes(childNode, "Text"))
          {
            if (flag3)
            {
              this._loader.ProcessDuplicateNode(childNode);
              return false;
            }
            flag3 = true;
            textToken = this._loader.LoadText(childNode);
            if (textToken == null)
            {
              this._loader.ReportError(ResourceManagerCache.FormatResourceString("InvalidNode", this._loader.ComputeCurrentXPath(), (object) this._loader.FilePath, (object) "Text"));
              return false;
            }
          }
          else
            unprocessedNodes.Add(childNode);
        }
        if (flag2)
        {
          if (flag3)
          {
            this._loader.ReportError(XmlLoadingResourceManager.FormatString("NodeWithExpression", (object) this._loader.ComputeCurrentXPath(), (object) this._loader.FilePath, (object) "Text"));
            return false;
          }
          ExpressionToken expressionToken = expressionNodeMatch.GenerateExpressionToken();
          if (expressionToken == null)
            return false;
          if (!string.IsNullOrEmpty(str))
            this._formatString = str;
          this._expression = expressionToken;
        }
        else
        {
          if (flag1)
          {
            this._loader.ReportError(XmlLoadingResourceManager.FormatString("NodeWithoutExpression", (object) this._loader.ComputeCurrentXPath(), (object) this._loader.FilePath, (object) "FormatString"));
            return false;
          }
          if (flag3)
            this._textToken = textToken;
        }
        return true;
      }

      internal string FormatString => this._formatString;

      internal TextToken TextToken => this._textToken;

      internal ExpressionToken Expression => this._expression;
    }

    private sealed class ComplexControlMatch
    {
      private ControlBase _control;
      private TypeInfoDataBaseLoader _loader;

      internal ComplexControlMatch(TypeInfoDataBaseLoader loader) => this._loader = loader;

      internal bool MatchNode(XmlNode n) => this._loader.MatchNodeName(n, "CustomControl") || this._loader.MatchNodeName(n, "CustomControlName");

      internal bool ProcessNode(XmlNode n)
      {
        if (this._loader.MatchNodeName(n, "CustomControl"))
        {
          this._control = (ControlBase) this._loader.LoadComplexControl(n);
          return true;
        }
        if (this._loader.MatchNodeName(n, "CustomControlName"))
        {
          string mandatoryInnerText = this._loader.GetMandatoryInnerText(n);
          if (mandatoryInnerText == null)
            return false;
          this._control = (ControlBase) new ControlReference()
          {
            name = mandatoryInnerText,
            controlType = typeof (ComplexControlBody)
          };
          return true;
        }
        TypeInfoDataBaseLoader.tracer.NewInvalidOperationException();
        return false;
      }

      internal ControlBase Control => this._control;
    }
  }
}
