// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.TypeTable
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management.Automation.Host;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml;

namespace System.Management.Automation.Runspaces
{
  public sealed class TypeTable
  {
    internal const string TypesXml = "TypesXml";
    internal const string FileLineError = "FileLineError";
    internal const string NotMoreThanOnceOne = "NotMoreThanOnceOne";
    internal const string NotMoreThanOnceZeroOrOne = "NotMoreThanOnceZeroOrOne";
    internal const string UnknownNode = "UnknownNode";
    internal const string AllowedNodesSeparator = "AllowedNodesSeparator";
    internal const string NodeShouldNotHaveInnerText = "NodeShouldNotHaveInnerText";
    internal const string NodeShouldHaveInnerText = "NodeShouldHaveInnerText";
    internal const string NodeNotFoundOnce = "NodeNotFoundOnce";
    internal const string NodeNotFoundAtLeastOnce = "NodeNotFoundAtLeastOnce";
    internal const string ExpectedNodeTypeInstead = "ExpectedNodeTypeInstead";
    internal const string UnexpectedNodeType = "UnexpectedNodeType";
    internal const string ExpectedNodeNameInstead = "ExpectedNodeNameInstead";
    internal const string TypeNodeShouldHaveMembersOrTypeConverters = "TypeNodeShouldHaveMembersOrTypeConverters";
    internal const string UnableToInstantiateTypeConverter = "UnableToInstantiateTypeConverter";
    internal const string UnableToInstantiateTypeAdapter = "UnableToInstantiateTypeAdapter";
    internal const string InvalidAdaptedType = "InvalidAdaptedType";
    internal const string TypeIsNotTypeConverter = "TypeIsNotTypeConverter";
    internal const string TypeIsNotTypeAdapter = "TypeIsNotTypeAdapter";
    internal const string CouldNotLoadAssembly = "CouldNotLoadAssembly";
    internal const string DuplicateMember = "DuplicateMember";
    internal const string ReservedNameMember = "ReservedNameMember";
    internal const string Exception = "Exception";
    internal const string ScriptPropertyShouldHaveGetterOrSetter = "ScriptPropertyShouldHaveGetterOrSetter";
    internal const string CodePropertyShouldHaveGetterOrSetter = "CodePropertyShouldHaveGetterOrSetter";
    internal const string ValueShouldBeTrueOrFalse = "ValueShouldBeTrueOrFalse";
    internal const string TypeConverterAlreadyPresent = "TypeConverterAlreadyPresent";
    internal const string TypeAdapterAlreadyPresent = "TypeAdapterAlreadyPresent";
    internal const string IsHiddenNotSupported = "IsHiddenNotSupported";
    internal const string IsHiddenValueShouldBeTrueOrFalse = "IsHiddenValueShouldBeTrueOrFalse";
    internal const string MemberShouldBeNote = "MemberShouldBeNote";
    internal const string ErrorConvertingNote = "ErrorConvertingNote";
    internal const string MemberShouldNotBePresent = "MemberShouldNotBePresent";
    internal const string MemberShouldHaveType = "MemberShouldHaveType";
    internal const string MemberMustBePresent = "MemberMustBePresent";
    internal const string SerializationSettingsIgnored = "SerializationSettingsIgnored";
    internal const string NotAStandardMember = "NotAStandardMember";
    internal const string PSStandardMembers = "PSStandardMembers";
    internal const string SerializationDepth = "SerializationDepth";
    internal const string StringSerializationSource = "StringSerializationSource";
    internal const string SerializationMethodNode = "SerializationMethod";
    internal const string TargetTypeForDeserialization = "TargetTypeForDeserialization";
    internal const string PropertySerializationSet = "PropertySerializationSet";
    internal const string InheritPropertySerializationSet = "InheritPropertySerializationSet";
    internal const string Types = "Types";
    internal const string Type = "Type";
    internal const string DefaultDisplayPropertySet = "DefaultDisplayPropertySet";
    internal const string DefaultKeyPropertySet = "DefaultKeyPropertySet";
    internal const string DefaultDisplayProperty = "DefaultDisplayProperty";
    internal const string IsHiddenAttribute = "IsHidden";
    internal const SerializationMethod defaultSerializationMethod = SerializationMethod.AllPublicProperties;
    internal const bool defaultInheritPropertySerializationSet = true;
    [TraceSource("TypeTable", "TypeTable")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (TypeTable), nameof (TypeTable));
    private static Node[] codeReferenceNodes = new Node[2]
    {
      new Node("TypeName", true, NodeCardinality.One, new Node[0]),
      new Node("MethodName", true, NodeCardinality.One, new Node[0])
    };
    private static Node nameNode = new Node("Name", true, NodeCardinality.One, new Node[0]);
    private static Node noteNode = new Node("NoteProperty", false, NodeCardinality.ZeroToMany, new Node[3]
    {
      TypeTable.nameNode.Clone(),
      new Node("Value", true, NodeCardinality.One, new Node[0]),
      new Node("TypeName", true, NodeCardinality.ZeroOrOne, new Node[0])
    }, new bool?(false));
    private static Node aliasNode = new Node("AliasProperty", false, NodeCardinality.ZeroToMany, new Node[3]
    {
      TypeTable.nameNode.Clone(),
      new Node("ReferencedMemberName", true, NodeCardinality.One, new Node[0]),
      new Node("TypeName", true, NodeCardinality.ZeroOrOne, new Node[0])
    }, new bool?(false));
    private static Node scriptPropertyNode = new Node("ScriptProperty", false, NodeCardinality.ZeroToMany, new Node[3]
    {
      TypeTable.nameNode.Clone(),
      new Node("GetScriptBlock", true, NodeCardinality.ZeroOrOne, new Node[0]),
      new Node("SetScriptBlock", true, NodeCardinality.ZeroOrOne, new Node[0])
    }, new bool?(false));
    private static Node scriptMethodNode = new Node("ScriptMethod", false, NodeCardinality.ZeroToMany, new Node[2]
    {
      TypeTable.nameNode.Clone(),
      new Node("Script", true, NodeCardinality.One, new Node[0])
    });
    private static Node codePropertyNode = new Node("CodeProperty", false, NodeCardinality.ZeroToMany, new Node[3]
    {
      TypeTable.nameNode.Clone(),
      new Node("GetCodeReference", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(TypeTable.codeReferenceNodes)),
      new Node("SetCodeReference", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(TypeTable.codeReferenceNodes))
    }, new bool?(false));
    private static Node codeMethodNode = new Node("CodeMethod", false, NodeCardinality.ZeroToMany, new Node[2]
    {
      TypeTable.nameNode.Clone(),
      new Node("CodeReference", false, NodeCardinality.One, Node.CloneNodeArray(TypeTable.codeReferenceNodes))
    });
    private static Node propertySetNode = new Node("PropertySet", false, NodeCardinality.ZeroToMany, new Node[2]
    {
      TypeTable.nameNode.Clone(),
      new Node("ReferencedProperties", false, NodeCardinality.One, new Node[1]
      {
        new Node("Name", true, NodeCardinality.OneToMany, new Node[0])
      })
    }, new bool?(false));
    private static Node memberSetNode = new Node("MemberSet", false, NodeCardinality.ZeroToMany, new Node[0], new bool?(false));
    private static Node[] membersNodeArray = new Node[8]
    {
      TypeTable.noteNode.Clone(),
      TypeTable.aliasNode.Clone(),
      TypeTable.scriptPropertyNode.Clone(),
      TypeTable.codePropertyNode.Clone(),
      TypeTable.scriptMethodNode.Clone(),
      TypeTable.codeMethodNode.Clone(),
      TypeTable.propertySetNode.Clone(),
      TypeTable.memberSetNode.Clone()
    };
    private static Node typeConverterNode = new Node("TypeConverter", false, NodeCardinality.ZeroOrOne, new Node[1]
    {
      new Node("TypeName", true, NodeCardinality.One, new Node[0])
    });
    private static Node typeAdapterNode = new Node("TypeAdapter", false, NodeCardinality.ZeroOrOne, new Node[1]
    {
      new Node("TypeName", true, NodeCardinality.One, new Node[0])
    });
    private static Node typeNode = new Node(nameof (Type), false, NodeCardinality.ZeroToMany, new Node[4]
    {
      TypeTable.nameNode.Clone(),
      new Node("Members", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(TypeTable.membersNodeArray)),
      TypeTable.typeConverterNode.Clone(),
      TypeTable.typeAdapterNode.Clone()
    });
    private static Node typesNode = new Node(nameof (Types), false, NodeCardinality.One, new Node[1]
    {
      TypeTable.typeNode.Clone()
    });
    private static Node fileNode = new Node("File", true, NodeCardinality.ZeroToMany, new Node[0]);
    private static Node filesNode = new Node("Files", false, NodeCardinality.One, new Node[1]
    {
      TypeTable.fileNode.Clone()
    });
    private HybridDictionary consolidatedMembers = new HybridDictionary();
    private HybridDictionary consolidatedSpecificProperties = new HybridDictionary();
    private Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>> members = new Dictionary<string, PSMemberInfoInternalCollection<PSMemberInfo>>();
    private Dictionary<string, object> typeConverters = new Dictionary<string, object>();
    private Dictionary<string, PSObject.AdapterSet> typeAdapters = new Dictionary<string, PSObject.AdapterSet>();
    private bool isShared;
    private List<string> typeFileList;

    private static bool GetTypeAndMethodName(
      LoadContext context,
      string typeName,
      Node codeReferenceNode,
      out System.Type type,
      out string methodName)
    {
      type = (System.Type) null;
      methodName = (string) null;
      Node actualNode1 = codeReferenceNode.possibleChildren[0].actualNodes[0];
      if (actualNode1.nodeError)
        return false;
      Node actualNode2 = codeReferenceNode.possibleChildren[1].actualNodes[0];
      if (actualNode2.nodeError)
        return false;
      methodName = actualNode2.innerText;
      type = TypeTable.GetTypeFromString(context, typeName, actualNode1);
      return type != null;
    }

    private static System.Type GetTypeFromString(
      LoadContext context,
      string typeName,
      Node typeNode)
    {
      System.Type type = (System.Type) null;
      try
      {
        type = (System.Type) LanguagePrimitives.ConvertTo((object) typeNode.innerText, typeof (System.Type), (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (PSInvalidCastException ex)
      {
        context.AddError(typeName, typeNode.lineNumber, "Exception", (object) ex.Message);
      }
      return type;
    }

    private static void AddMember(
      LoadContext context,
      string typeName,
      int memberLineNumber,
      PSMemberInfo member,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (PSMemberInfoCollection<PSMemberInfo>.IsReservedName(member.name))
        context.AddError(typeName, memberLineNumber, "ReservedNameMember", (object) member.name);
      else if (membersCollection[member.name] != null)
      {
        context.AddError(typeName, memberLineNumber, "DuplicateMember", (object) member.name);
      }
      else
      {
        member.isInstance = false;
        membersCollection.Add(member);
        nodeLineNumbers.Add(memberLineNumber);
      }
    }

    private static void ProcessNote(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode1 = node.possibleChildren[0].actualNodes[0];
      if (actualNode1.nodeError)
        return;
      Node actualNode2 = node.possibleChildren[1].actualNodes[0];
      if (actualNode2.nodeError)
        return;
      object innerText = (object) actualNode2.innerText;
      Collection<Node> actualNodes = node.possibleChildren[2].actualNodes;
      if (actualNodes.Count == 1)
      {
        if (actualNodes[0].nodeError)
          return;
        System.Type typeFromString = TypeTable.GetTypeFromString(context, typeName, actualNodes[0]);
        if (typeFromString == null)
          return;
        try
        {
          innerText = LanguagePrimitives.ConvertTo(innerText, typeFromString, (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (PSInvalidCastException ex)
        {
          context.AddError(typeName, actualNodes[0].lineNumber, "Exception", (object) ex.Message);
          return;
        }
      }
      PSNoteProperty psNoteProperty = new PSNoteProperty(actualNode1.innerText, innerText);
      psNoteProperty.isHidden = node.isHidden.HasValue && node.isHidden.Value;
      TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psNoteProperty, membersCollection, nodeLineNumbers);
    }

    private static void ProcessAlias(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode1 = node.possibleChildren[0].actualNodes[0];
      if (actualNode1.nodeError)
        return;
      Node actualNode2 = node.possibleChildren[1].actualNodes[0];
      if (actualNode2.nodeError)
        return;
      System.Type conversionType = (System.Type) null;
      Collection<Node> actualNodes = node.possibleChildren[2].actualNodes;
      if (actualNodes.Count == 1)
      {
        if (actualNodes[0].nodeError)
          return;
        conversionType = TypeTable.GetTypeFromString(context, typeName, actualNodes[0]);
        if (conversionType == null)
          return;
      }
      PSAliasProperty psAliasProperty = new PSAliasProperty(actualNode1.innerText, actualNode2.innerText, conversionType);
      psAliasProperty.isHidden = node.isHidden.HasValue && node.isHidden.Value;
      TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psAliasProperty, membersCollection, nodeLineNumbers);
    }

    private static void ProcessScriptProperty(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode = node.possibleChildren[0].actualNodes[0];
      if (actualNode.nodeError)
        return;
      ScriptBlock getterScript = (ScriptBlock) null;
      Collection<Node> actualNodes1 = node.possibleChildren[1].actualNodes;
      if (actualNodes1.Count == 1)
      {
        if (actualNodes1[0].nodeError)
          return;
        getterScript = ScriptBlock.Create(actualNodes1[0].innerText);
      }
      ScriptBlock setterScript = (ScriptBlock) null;
      Collection<Node> actualNodes2 = node.possibleChildren[2].actualNodes;
      if (actualNodes2.Count == 1)
      {
        if (actualNodes2[0].nodeError)
          return;
        setterScript = ScriptBlock.Create(actualNodes2[0].innerText);
      }
      if (setterScript == null && getterScript == null)
      {
        context.AddError(typeName, node.lineNumber, "ScriptPropertyShouldHaveGetterOrSetter");
      }
      else
      {
        PSScriptProperty psScriptProperty = new PSScriptProperty(actualNode.innerText, getterScript, setterScript, true);
        psScriptProperty.isHidden = node.isHidden.HasValue && node.isHidden.Value;
        TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psScriptProperty, membersCollection, nodeLineNumbers);
      }
    }

    private static void ProcessScriptMethod(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode1 = node.possibleChildren[0].actualNodes[0];
      if (actualNode1.nodeError)
        return;
      Node actualNode2 = node.possibleChildren[1].actualNodes[0];
      if (actualNode2.nodeError)
        return;
      PSScriptMethod psScriptMethod = new PSScriptMethod(actualNode1.innerText, ScriptBlock.Create(actualNode2.innerText), true);
      TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psScriptMethod, membersCollection, nodeLineNumbers);
    }

    private static void ProcessCodeProperty(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode = node.possibleChildren[0].actualNodes[0];
      if (actualNode.nodeError)
        return;
      PSCodeProperty psCodeProperty = new PSCodeProperty(actualNode.innerText);
      Collection<Node> actualNodes1 = node.possibleChildren[1].actualNodes;
      Collection<Node> actualNodes2 = node.possibleChildren[2].actualNodes;
      if (actualNodes1.Count == 0 && actualNodes2.Count == 0)
      {
        context.AddError(typeName, node.lineNumber, "CodePropertyShouldHaveGetterOrSetter");
      }
      else
      {
        if (actualNodes1.Count == 1)
        {
          Node node1 = actualNodes1[0];
          System.Type type;
          string methodName;
          if (!TypeTable.GetTypeAndMethodName(context, typeName, actualNodes1[0], out type, out methodName))
            return;
          try
          {
            psCodeProperty.SetGetterFromTypeTable(type, methodName);
          }
          catch (ExtendedTypeSystemException ex)
          {
            context.AddError(typeName, node1.lineNumber, "Exception", (object) ex.Message);
            return;
          }
        }
        if (actualNodes2.Count == 1)
        {
          Node codeReferenceNode = actualNodes2[0];
          System.Type type;
          string methodName;
          if (!TypeTable.GetTypeAndMethodName(context, typeName, codeReferenceNode, out type, out methodName))
            return;
          try
          {
            psCodeProperty.SetSetterFromTypeTable(type, methodName);
          }
          catch (ExtendedTypeSystemException ex)
          {
            context.AddError(typeName, codeReferenceNode.lineNumber, "Exception", (object) ex.Message);
            return;
          }
        }
        psCodeProperty.isHidden = node.isHidden.HasValue && node.isHidden.Value;
        TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psCodeProperty, membersCollection, nodeLineNumbers);
      }
    }

    private static void ProcessCodeMethod(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode1 = node.possibleChildren[0].actualNodes[0];
      if (actualNode1.nodeError)
        return;
      PSCodeMethod psCodeMethod = new PSCodeMethod(actualNode1.innerText);
      Node actualNode2 = node.possibleChildren[1].actualNodes[0];
      if (actualNode2.nodeError)
        return;
      System.Type type;
      string methodName;
      if (!TypeTable.GetTypeAndMethodName(context, typeName, actualNode2, out type, out methodName))
        return;
      try
      {
        psCodeMethod.SetCodeReference(type, methodName);
      }
      catch (ExtendedTypeSystemException ex)
      {
        context.AddError(typeName, actualNode2.lineNumber, "Exception", (object) ex.Message);
        return;
      }
      TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psCodeMethod, membersCollection, nodeLineNumbers);
    }

    private static void ProcessPropertySet(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode1 = node.possibleChildren[0].actualNodes[0];
      if (actualNode1.nodeError)
        return;
      Node actualNode2 = node.possibleChildren[1].actualNodes[0];
      if (actualNode2.nodeError)
        return;
      Collection<string> collection = new Collection<string>();
      foreach (Node actualNode3 in actualNode2.possibleChildren[0].actualNodes)
      {
        if (actualNode3.nodeError)
          return;
        collection.Add(actualNode3.innerText);
      }
      PSPropertySet psPropertySet = new PSPropertySet(actualNode1.innerText, (IEnumerable<string>) collection);
      psPropertySet.isHidden = node.isHidden.HasValue && node.isHidden.Value;
      TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psPropertySet, membersCollection, nodeLineNumbers);
    }

    private static bool GetCheckNote(
      LoadContext context,
      string typeName,
      Collection<int> lineNumbers,
      PSMemberInfoInternalCollection<PSMemberInfo> members,
      string noteName,
      System.Type noteType,
      out PSNoteProperty note)
    {
      note = (PSNoteProperty) null;
      PSMemberInfo psMemberInfo = (PSMemberInfo) null;
      int errorLineNumber = 0;
      for (int index = 0; index < members.Count; ++index)
      {
        if (string.Compare(members[index].Name, noteName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          psMemberInfo = members[index];
          errorLineNumber = lineNumbers[index];
        }
      }
      if (psMemberInfo == null)
        return true;
      note = psMemberInfo as PSNoteProperty;
      if (note == null)
      {
        context.AddError(typeName, errorLineNumber, "MemberShouldBeNote", (object) psMemberInfo.Name);
        return false;
      }
      object valueToConvert = note.Value;
      if (System.Type.GetTypeCode(noteType).Equals((object) TypeCode.Boolean))
      {
        if (valueToConvert is string strA)
        {
          note.noteValue = strA.Length != 0 ? (object) (string.Compare(strA, "false", StringComparison.OrdinalIgnoreCase) != 0) : (object) true;
          return true;
        }
      }
      try
      {
        note.noteValue = LanguagePrimitives.ConvertTo(valueToConvert, noteType, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (PSInvalidCastException ex)
      {
        context.AddError(typeName, errorLineNumber, "ErrorConvertingNote", (object) note.Name, (object) ex.Message);
        return false;
      }
      return true;
    }

    private static bool EnsureNotPresent(
      LoadContext context,
      string typeName,
      Collection<int> lineNumbers,
      PSMemberInfoInternalCollection<PSMemberInfo> members,
      string memberName)
    {
      for (int index = 0; index < members.Count; ++index)
      {
        if (string.Compare(members[index].Name, memberName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          context.AddError(typeName, lineNumbers[index], "MemberShouldNotBePresent", (object) members[index].Name);
          return false;
        }
      }
      return true;
    }

    private static bool GetCheckMemberType(
      LoadContext context,
      string typeName,
      Collection<int> lineNumbers,
      PSMemberInfoInternalCollection<PSMemberInfo> members,
      string noteName,
      System.Type memberType,
      out PSMemberInfo member)
    {
      member = (PSMemberInfo) null;
      int errorLineNumber = 0;
      for (int index = 0; index < members.Count; ++index)
      {
        if (string.Compare(members[index].Name, noteName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          member = members[index];
          errorLineNumber = lineNumbers[index];
        }
      }
      if (member == null || memberType.IsAssignableFrom(member.GetType()))
        return true;
      context.AddError(typeName, errorLineNumber, "MemberShouldHaveType", (object) member.Name, (object) memberType.Name);
      member = (PSMemberInfo) null;
      return false;
    }

    private static void CheckStandardMembers(
      LoadContext context,
      string typeName,
      int standardMembersLine,
      Collection<int> lineNumbers,
      PSMemberInfoInternalCollection<PSMemberInfo> members)
    {
      string[] strArray = new string[9]
      {
        "DefaultDisplayProperty",
        "DefaultDisplayPropertySet",
        "DefaultKeyPropertySet",
        "SerializationMethod",
        "SerializationDepth",
        "StringSerializationSource",
        "PropertySerializationSet",
        "InheritPropertySerializationSet",
        "TargetTypeForDeserialization"
      };
      ArrayList arrayList = new ArrayList();
      for (int index = 0; index < members.Count; ++index)
      {
        bool flag = false;
        foreach (string strB in strArray)
        {
          if (string.Compare(members[index].Name, strB, StringComparison.OrdinalIgnoreCase) == 0)
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          arrayList.Add((object) members[index].Name);
          context.AddError(typeName, lineNumbers[index], "NotAStandardMember", (object) members[index].Name);
        }
      }
      foreach (string name in arrayList)
        members.Remove(name);
      PSNoteProperty note1;
      bool flag1 = TypeTable.GetCheckNote(context, typeName, lineNumbers, members, "SerializationMethod", typeof (SerializationMethod), out note1);
      if (flag1)
      {
        SerializationMethod serializationMethod = SerializationMethod.AllPublicProperties;
        if (note1 != null)
          serializationMethod = (SerializationMethod) note1.Value;
        switch (serializationMethod)
        {
          case SerializationMethod.AllPublicProperties:
            flag1 = TypeTable.EnsureNotPresent(context, typeName, lineNumbers, members, "InheritPropertySerializationSet");
            if (flag1)
            {
              flag1 = TypeTable.EnsureNotPresent(context, typeName, lineNumbers, members, "PropertySerializationSet");
              if (flag1)
              {
                flag1 = TypeTable.GetCheckNote(context, typeName, lineNumbers, members, "SerializationDepth", typeof (int), out PSNoteProperty _);
                if (!flag1)
                  goto label_34;
                else
                  break;
              }
              else
                goto label_34;
            }
            else
              goto label_34;
          case SerializationMethod.String:
            flag1 = TypeTable.EnsureNotPresent(context, typeName, lineNumbers, members, "InheritPropertySerializationSet");
            if (flag1)
            {
              flag1 = TypeTable.EnsureNotPresent(context, typeName, lineNumbers, members, "PropertySerializationSet");
              if (flag1)
              {
                flag1 = TypeTable.EnsureNotPresent(context, typeName, lineNumbers, members, "SerializationDepth");
                if (flag1)
                  break;
                goto label_34;
              }
              else
                goto label_34;
            }
            else
              goto label_34;
          case SerializationMethod.SpecificProperties:
            PSNoteProperty note2;
            flag1 = TypeTable.GetCheckNote(context, typeName, lineNumbers, members, "InheritPropertySerializationSet", typeof (bool), out note2);
            if (flag1)
            {
              PSMemberInfo member;
              flag1 = TypeTable.GetCheckMemberType(context, typeName, lineNumbers, members, "PropertySerializationSet", typeof (PSPropertySet), out member);
              if (flag1)
              {
                if (note2 != null && note2.Value.Equals((object) false) && member == null)
                {
                  context.AddError(typeName, standardMembersLine, "MemberMustBePresent", (object) "PropertySerializationSet", (object) "SerializationMethod", (object) SerializationMethod.SpecificProperties.ToString(), (object) "InheritPropertySerializationSet", (object) "false");
                  flag1 = false;
                  goto label_34;
                }
                else
                {
                  flag1 = TypeTable.GetCheckNote(context, typeName, lineNumbers, members, "SerializationDepth", typeof (int), out PSNoteProperty _);
                  if (flag1)
                    break;
                  goto label_34;
                }
              }
              else
                goto label_34;
            }
            else
              goto label_34;
        }
        flag1 = TypeTable.GetCheckMemberType(context, typeName, lineNumbers, members, "StringSerializationSource", typeof (PSPropertyInfo), out PSMemberInfo _);
        int num = flag1 ? 1 : 0;
      }
label_34:
      if (!flag1)
      {
        context.AddError(typeName, standardMembersLine, "SerializationSettingsIgnored");
        members.Remove("InheritPropertySerializationSet");
        members.Remove("SerializationMethod");
        members.Remove("StringSerializationSource");
        members.Remove("PropertySerializationSet");
      }
      PSMemberInfo member1;
      if (!TypeTable.GetCheckMemberType(context, typeName, lineNumbers, members, "DefaultDisplayPropertySet", typeof (PSPropertySet), out member1))
        members.Remove("DefaultDisplayPropertySet");
      if (!TypeTable.GetCheckMemberType(context, typeName, lineNumbers, members, "DefaultKeyPropertySet", typeof (PSPropertySet), out member1))
        members.Remove("DefaultKeyPropertySet");
      if (!TypeTable.GetCheckNote(context, typeName, lineNumbers, members, "DefaultDisplayProperty", typeof (string), out PSNoteProperty _))
        members.Remove("DefaultDisplayProperty");
      PSNoteProperty note3;
      if (!TypeTable.GetCheckNote(context, typeName, lineNumbers, members, "TargetTypeForDeserialization", typeof (System.Type), out note3))
      {
        members.Remove("TargetTypeForDeserialization");
      }
      else
      {
        if (note3 == null)
          return;
        members.Remove("TargetTypeForDeserialization");
        members.Add((PSMemberInfo) note3, true);
      }
    }

    private void ProcessMemberSet(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      Node actualNode = node.possibleChildren[0].actualNodes[0];
      if (actualNode.nodeError)
        return;
      bool flag = true;
      Collection<Node> actualNodes1 = node.possibleChildren[1].actualNodes;
      if (actualNodes1.Count == 1)
      {
        if (actualNodes1[0].nodeError)
          return;
        if (actualNodes1[0].innerText.Equals("true", StringComparison.OrdinalIgnoreCase))
          flag = true;
        else if (actualNodes1[0].innerText.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
          flag = false;
        }
        else
        {
          context.AddError(typeName, actualNodes1[0].lineNumber, "ValueShouldBeTrueOrFalse", (object) actualNodes1[0].innerText);
          return;
        }
      }
      PSMemberInfoInternalCollection<PSMemberInfo> internalCollection = new PSMemberInfoInternalCollection<PSMemberInfo>();
      Collection<Node> actualNodes2 = node.possibleChildren[2].actualNodes;
      if (actualNodes2.Count == 1)
      {
        if (actualNodes2[0].nodeError)
          return;
        this.ProcessMembers(context, typeName, actualNodes2[0], internalCollection, nodeLineNumbers);
        if (string.Compare(actualNode.innerText, "PSStandardMembers", StringComparison.OrdinalIgnoreCase) == 0)
        {
          TypeTable.CheckStandardMembers(context, typeName, node.lineNumber, nodeLineNumbers, internalCollection);
          PSMemberSet psMemberSet = new PSMemberSet(actualNode.innerText, (IEnumerable<PSMemberInfo>) internalCollection);
          psMemberSet.inheritMembers = flag;
          psMemberSet.isHidden = true;
          psMemberSet.shouldSerialize = false;
          TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psMemberSet, membersCollection, nodeLineNumbers);
          return;
        }
      }
      PSMemberSet psMemberSet1 = new PSMemberSet(actualNode.innerText, (IEnumerable<PSMemberInfo>) internalCollection);
      psMemberSet1.inheritMembers = flag;
      psMemberSet1.isHidden = node.isHidden.HasValue && node.isHidden.Value;
      TypeTable.AddMember(context, typeName, node.lineNumber, (PSMemberInfo) psMemberSet1, membersCollection, nodeLineNumbers);
    }

    private void ProcessMembers(
      LoadContext context,
      string typeName,
      Node node,
      PSMemberInfoInternalCollection<PSMemberInfo> membersCollection,
      Collection<int> nodeLineNumbers)
    {
      if (node.nodeError)
        return;
      foreach (Node actualNode in node.possibleChildren[0].actualNodes)
        TypeTable.ProcessNote(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[1].actualNodes)
        TypeTable.ProcessAlias(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[2].actualNodes)
        TypeTable.ProcessScriptProperty(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[3].actualNodes)
        TypeTable.ProcessCodeProperty(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[4].actualNodes)
        TypeTable.ProcessScriptMethod(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[5].actualNodes)
        TypeTable.ProcessCodeMethod(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[6].actualNodes)
        TypeTable.ProcessPropertySet(context, typeName, actualNode, membersCollection, nodeLineNumbers);
      foreach (Node actualNode in node.possibleChildren[7].actualNodes)
        this.ProcessMemberSet(context, typeName, actualNode, membersCollection, nodeLineNumbers);
    }

    private void ProcessTypeConverter(LoadContext context, string typeName, Node node)
    {
      if (node.nodeError)
        return;
      if (this.typeConverters.ContainsKey(typeName))
        context.AddError(typeName, node.lineNumber, "TypeConverterAlreadyPresent");
      Node actualNode = node.possibleChildren[0].actualNodes[0];
      if (actualNode.nodeError)
        return;
      object instance = (object) null;
      if (!this.CreateInstance(context, typeName, actualNode, "UnableToInstantiateTypeConverter", out instance))
        return;
      switch (instance)
      {
        case TypeConverter _:
        case PSTypeConverter _:
          this.typeConverters[typeName] = instance;
          break;
        default:
          context.AddError(typeName, node.lineNumber, "TypeIsNotTypeConverter", (object) actualNode.innerText);
          break;
      }
    }

    private void ProcessTypeAdapter(LoadContext context, string typeName, Node node)
    {
      if (node.nodeError)
        return;
      if (this.typeAdapters.ContainsKey(typeName))
        context.AddError(typeName, node.lineNumber, "TypeAdapterAlreadyPresent");
      Node actualNode = node.possibleChildren[0].actualNodes[0];
      if (actualNode.nodeError)
        return;
      object instance = (object) null;
      if (!this.CreateInstance(context, typeName, actualNode, "UnableToInstantiateTypeAdapter", out instance))
        return;
      if (!(instance is PSPropertyAdapter adapter))
      {
        context.AddError(typeName, node.lineNumber, "TypeIsNotTypeAdapter", (object) actualNode.innerText);
      }
      else
      {
        System.Type result = (System.Type) null;
        if (!LanguagePrimitives.TryConvertTo<System.Type>((object) typeName, out result))
          context.AddError(typeName, node.lineNumber, "InvalidAdaptedType", (object) typeName);
        else
          this.typeAdapters[typeName] = PSObject.CreateThirdPartyAdapterSet(result, adapter);
      }
    }

    private bool CreateInstance(
      LoadContext context,
      string typeName,
      Node typeNameNode,
      string errorFormatString,
      out object instance)
    {
      instance = (object) null;
      System.Type typeFromString = TypeTable.GetTypeFromString(context, typeName, typeNameNode);
      if (typeFromString == null)
        return false;
      System.Exception exception = (System.Exception) null;
      try
      {
        instance = Activator.CreateInstance(typeFromString);
      }
      catch (TargetInvocationException ex)
      {
        exception = ex.InnerException == null ? (System.Exception) ex : ex.InnerException;
      }
      catch (System.Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        exception = ex;
      }
      if (exception == null)
        return true;
      context.AddError(typeName, typeNameNode.lineNumber, errorFormatString, (object) typeNameNode.innerText, (object) exception.Message);
      return false;
    }

    private void ProcessTypeNode(LoadContext context, Node node)
    {
      if (node.nodeError)
        return;
      Node actualNode = node.possibleChildren[0].actualNodes[0];
      if (actualNode.nodeError)
        return;
      string innerText = actualNode.innerText;
      Collection<Node> actualNodes1 = node.possibleChildren[1].actualNodes;
      Collection<Node> actualNodes2 = node.possibleChildren[2].actualNodes;
      Collection<Node> actualNodes3 = node.possibleChildren[3].actualNodes;
      if (actualNodes1.Count == 0 && actualNodes2.Count == 0 && actualNodes3.Count == 0)
      {
        context.AddError(innerText, node.lineNumber, "TypeNodeShouldHaveMembersOrTypeConverters");
      }
      else
      {
        if (actualNodes1.Count == 1)
        {
          PSMemberInfoInternalCollection<PSMemberInfo> membersCollection;
          if (!this.members.TryGetValue(innerText, out membersCollection))
          {
            membersCollection = new PSMemberInfoInternalCollection<PSMemberInfo>();
            this.members[innerText] = membersCollection;
          }
          Collection<int> nodeLineNumbers = new Collection<int>();
          this.ProcessMembers(context, innerText, actualNodes1[0], membersCollection, nodeLineNumbers);
        }
        if (actualNodes2.Count == 1)
          this.ProcessTypeConverter(context, innerText, actualNodes2[0]);
        if (actualNodes3.Count != 1)
          return;
        this.ProcessTypeAdapter(context, innerText, actualNodes3[0]);
      }
    }

    private static string PossibleNodes(Node[] nodes)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string resourceString = ResourceManagerCache.GetResourceString("TypesXml", "AllowedNodesSeparator");
      if (nodes.Length != 0)
      {
        foreach (Node node in nodes)
        {
          stringBuilder.Append(node.name);
          stringBuilder.Append(resourceString);
        }
        stringBuilder.Remove(stringBuilder.Length - resourceString.Length, resourceString.Length);
      }
      return stringBuilder.ToString();
    }

    private static void SkipUntillNodeEnd(LoadContext context, string nodeName)
    {
      while (context.Read())
      {
        if (context.reader.IsStartElement() && context.reader.LocalName.Equals(nodeName))
          TypeTable.SkipUntillNodeEnd(context, nodeName);
        else if (context.reader.NodeType == XmlNodeType.EndElement && context.reader.LocalName.Equals(nodeName))
          break;
      }
    }

    private static Node ProcessStartElement(LoadContext context, Node node)
    {
      foreach (Node possibleChild in node.possibleChildren)
      {
        if (possibleChild.name.Equals(context.reader.LocalName))
        {
          if (possibleChild.nodeCount == 1)
          {
            if (possibleChild.cardinality == NodeCardinality.ZeroOrOne)
            {
              context.AddError(context.lineNumber, "NotMoreThanOnceZeroOrOne", (object) possibleChild.name, (object) node.name);
              node.nodeError = true;
            }
            if (possibleChild.cardinality == NodeCardinality.One)
            {
              context.AddError(context.lineNumber, "NotMoreThanOnceOne", (object) possibleChild.name, (object) node.name);
              node.nodeError = true;
            }
          }
          ++possibleChild.nodeCount;
          Node node1 = possibleChild.Clone();
          node1.lineNumber = context.lineNumber;
          possibleChild.actualNodes.Add(node1);
          if (context.reader.HasAttributes)
          {
            string attribute = context.reader.GetAttribute("IsHidden");
            if (!node1.isHidden.HasValue && attribute != null)
            {
              context.AddError(context.lineNumber, "IsHiddenNotSupported", (object) node1.name, (object) "IsHidden");
              node.nodeError = true;
            }
            else if (attribute != null)
            {
              if (attribute.Equals("true", StringComparison.OrdinalIgnoreCase))
                node1.isHidden = new bool?(true);
              else if (attribute.Equals("false", StringComparison.OrdinalIgnoreCase))
              {
                node1.isHidden = new bool?(false);
              }
              else
              {
                context.AddError(context.lineNumber, "IsHiddenValueShouldBeTrueOrFalse", (object) attribute, (object) "IsHidden");
                node.nodeError = true;
              }
            }
          }
          return node1;
        }
      }
      context.AddError(context.lineNumber, "UnknownNode", (object) context.reader.LocalName, (object) TypeTable.PossibleNodes(node.possibleChildren), (object) node.name);
      return (Node) null;
    }

    private static void ProcessEndElement(LoadContext context, Node node)
    {
      foreach (Node possibleChild in node.possibleChildren)
      {
        if (possibleChild.nodeCount == 0)
        {
          if (possibleChild.cardinality == NodeCardinality.One)
          {
            context.AddError(context.lineNumber, "NodeNotFoundOnce", (object) possibleChild.name, (object) node.name);
            node.nodeError = true;
          }
          else if (possibleChild.cardinality == NodeCardinality.OneToMany)
          {
            context.AddError(context.lineNumber, "NodeNotFoundAtLeastOnce", (object) possibleChild.name, (object) node.name);
            node.nodeError = true;
          }
        }
      }
      if (node.innerText != null || !node.hasInnerText)
        return;
      context.AddError(context.lineNumber, "NodeShouldHaveInnerText", (object) node.name);
      node.nodeError = true;
    }

    private void ReadNode(LoadContext context, Node node)
    {
      if (node.name.Equals("MemberSet"))
        node.possibleChildren = new Node[3]
        {
          TypeTable.nameNode.Clone(),
          new Node("InheritMembers", true, NodeCardinality.ZeroOrOne, new Node[0]),
          new Node("Members", false, NodeCardinality.ZeroOrOne, Node.CloneNodeArray(TypeTable.membersNodeArray))
        };
      while (context.Read())
      {
        if (context.reader.NodeType != XmlNodeType.Comment)
        {
          if (context.reader.IsEmptyElement)
          {
            Node node1 = TypeTable.ProcessStartElement(context, node);
            if (node1 != null)
              TypeTable.ProcessEndElement(context, node1);
          }
          else if (context.reader.IsStartElement())
          {
            Node node1 = TypeTable.ProcessStartElement(context, node);
            if (node1 == null)
            {
              string localName = context.reader.LocalName;
              TypeTable.SkipUntillNodeEnd(context, localName);
            }
            else
              this.ReadNode(context, node1);
          }
          else
          {
            if (context.reader.NodeType == XmlNodeType.EndElement)
            {
              TypeTable.ProcessEndElement(context, node);
              break;
            }
            if (context.reader.NodeType == XmlNodeType.Text)
            {
              if (node.hasInnerText)
              {
                node.innerText = context.reader.Value.Trim();
              }
              else
              {
                context.AddError(context.lineNumber, "NodeShouldNotHaveInnerText", (object) node.name);
                node.nodeError = true;
              }
            }
            else
              context.AddError(context.lineNumber, "UnexpectedNodeType", (object) context.reader.NodeType.ToString());
          }
        }
      }
    }

    private bool ReadDocument(LoadContext context, ref Node rootNode)
    {
      Node node = new Node("Document", false, NodeCardinality.One, new Node[1]
      {
        rootNode.Clone()
      });
      rootNode.cardinality = NodeCardinality.One;
      try
      {
        this.ReadNode(context, node);
      }
      catch (XmlException ex)
      {
        context.AddError("Exception", (object) ex.Message);
        return false;
      }
      TypeTable.ProcessEndElement(context, node);
      if (node.nodeError || rootNode.nodeError)
        return false;
      rootNode = node.possibleChildren[0].actualNodes[0];
      return true;
    }

    private void Update(LoadContext context)
    {
      Node rootNode = TypeTable.typesNode.Clone();
      if (!this.ReadDocument(context, ref rootNode))
        return;
      foreach (Node actualNode in rootNode.possibleChildren[0].actualNodes)
        this.ProcessTypeNode(context, actualNode);
      LanguagePrimitives.ResetCaches(this);
      Adapter.ResetCaches();
    }

    internal TypeTable() => this.typeFileList = new List<string>();

    public TypeTable(IEnumerable<string> typeFiles)
      : this(typeFiles, (AuthorizationManager) null, (PSHost) null)
    {
    }

    internal TypeTable(
      IEnumerable<string> typeFiles,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      if (typeFiles == null)
        throw TypeTable.tracer.NewArgumentNullException(nameof (typeFiles));
      this.isShared = true;
      this.typeFileList = new List<string>();
      Collection<string> collection = new Collection<string>();
      foreach (string typeFile in typeFiles)
      {
        if (string.IsNullOrEmpty(typeFile) || !Path.IsPathRooted(typeFile))
          throw TypeTable.tracer.NewArgumentException("typeFile", nameof (TypesXml), "TypeFileNotRooted", (object) typeFile);
        this.Initialize(string.Empty, typeFile, collection, authorizationManager, host);
        this.typeFileList.Add(typeFile);
      }
      if (collection.Count > 0)
        throw new TypeTableLoadException(collection);
    }

    internal Collection<string> GetSpecificProperties(ConsolidatedString types)
    {
      lock (this.members)
      {
        if (types == null || types.Key == null || types.Key.Length == 0)
          return new Collection<string>();
        Collection<string> collection = (Collection<string>) this.consolidatedSpecificProperties[(object) types.Key];
        if (collection == null)
        {
          CacheTable cacheTable = new CacheTable();
          foreach (string type in (Collection<string>) types)
          {
            PSMemberInfoInternalCollection<PSMemberInfo> internalCollection;
            if (this.members.TryGetValue(type, out internalCollection) && internalCollection["PSStandardMembers"] is PSMemberSet settings && settings.Members["PropertySerializationSet"] is PSPropertySet member)
            {
              foreach (string referencedPropertyName in member.ReferencedPropertyNames)
              {
                if (cacheTable[referencedPropertyName] == null)
                  cacheTable.Add(referencedPropertyName, (object) referencedPropertyName);
              }
              if (!(bool) PSObject.GetNoteSettingValue(settings, "InheritPropertySerializationSet", (object) true, typeof (bool), false, (PSObject) null))
                break;
            }
          }
          collection = new Collection<string>();
          foreach (string member in cacheTable.memberCollection)
            collection.Add(member);
          this.consolidatedSpecificProperties[(object) types.Key] = (object) collection;
        }
        return collection;
      }
    }

    internal PSMemberInfoInternalCollection<T> GetMembers<T>(
      ConsolidatedString types)
      where T : PSMemberInfo
    {
      return PSObject.TransformMemberInfoCollection<PSMemberInfo, T>((PSMemberInfoCollection<PSMemberInfo>) this.GetMembers(types));
    }

    private PSMemberInfoInternalCollection<PSMemberInfo> GetMembers(
      ConsolidatedString types)
    {
      lock (this.members)
      {
        if (types == null || types.Key == null || types.Key.Length == 0)
          return new PSMemberInfoInternalCollection<PSMemberInfo>();
        PSMemberInfoInternalCollection<PSMemberInfo> internalCollection1 = (PSMemberInfoInternalCollection<PSMemberInfo>) this.consolidatedMembers[(object) types.Key];
        if (internalCollection1 == null)
        {
          internalCollection1 = new PSMemberInfoInternalCollection<PSMemberInfo>();
          for (int index = types.Count - 1; index >= 0; --index)
          {
            PSMemberInfoInternalCollection<PSMemberInfo> internalCollection2;
            if (this.members.TryGetValue(types[index], out internalCollection2))
            {
              foreach (PSMemberInfo psMemberInfo1 in (PSMemberInfoCollection<PSMemberInfo>) internalCollection2)
              {
                PSMemberInfo psMemberInfo2 = internalCollection1[psMemberInfo1.Name];
                if (psMemberInfo2 == null)
                {
                  internalCollection1.Add(psMemberInfo1.Copy());
                }
                else
                {
                  PSMemberSet psMemberSet1 = psMemberInfo2 as PSMemberSet;
                  PSMemberSet psMemberSet2 = psMemberInfo1 as PSMemberSet;
                  if (psMemberSet1 == null || psMemberSet2 == null || !psMemberSet2.InheritMembers)
                  {
                    internalCollection1.Remove(psMemberInfo1.Name);
                    internalCollection1.Add(psMemberInfo1.Copy());
                  }
                  else
                  {
                    foreach (PSMemberInfo member in psMemberSet2.Members)
                    {
                      if (psMemberSet1.Members[member.Name] == null)
                        ((PSMemberInfoIntegratingCollection<PSMemberInfo>) psMemberSet1.Members).AddToTypesXmlCache(member, false);
                      else
                        psMemberSet1.InternalMembers.Replace(member);
                    }
                  }
                }
              }
            }
          }
          this.consolidatedMembers[(object) types.Key] = (object) internalCollection1;
        }
        return internalCollection1;
      }
    }

    internal object GetTypeConverter(string typeName)
    {
      switch (typeName)
      {
        case "":
        case null:
          return (object) null;
        default:
          lock (this.members)
          {
            object obj;
            this.typeConverters.TryGetValue(typeName, out obj);
            return obj;
          }
      }
    }

    internal void ForEachTypeConverter(Action<string> action)
    {
      lock (this.members)
      {
        foreach (string key in this.typeConverters.Keys)
          action(key);
      }
    }

    internal PSObject.AdapterSet GetTypeAdapter(System.Type type)
    {
      if (type == null)
        return (PSObject.AdapterSet) null;
      lock (this.members)
      {
        PSObject.AdapterSet adapterSet = (PSObject.AdapterSet) null;
        this.typeAdapters.TryGetValue(type.FullName, out adapterSet);
        return adapterSet;
      }
    }

    internal Collection<string> ReadFiles(
      string PSSnapinName,
      string xmlFileListFileName,
      Collection<string> errors,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      Collection<string> collection = new Collection<string>();
      LoadContext context = new LoadContext(PSSnapinName, xmlFileListFileName, errors);
      ExternalScriptInfo externalScriptInfo;
      string scriptContents;
      try
      {
        externalScriptInfo = new ExternalScriptInfo(xmlFileListFileName, xmlFileListFileName);
        scriptContents = externalScriptInfo.ScriptContents;
      }
      catch (SecurityException ex)
      {
        context.AddError("Exception", (object) ex.Message);
        return collection;
      }
      if (authorizationManager != null)
      {
        try
        {
          authorizationManager.ShouldRunInternal((CommandInfo) externalScriptInfo, CommandOrigin.Internal, host);
        }
        catch (PSSecurityException ex)
        {
          string str = ResourceManagerCache.FormatResourceString("TypesXml", "ValidationException", (object) PSSnapinName, (object) xmlFileListFileName, (object) ex.Message);
          errors.Add(str);
          return collection;
        }
      }
      Node rootNode = TypeTable.filesNode.Clone();
      using (StringReader stringReader = new StringReader(scriptContents))
      {
        XmlTextReader xmlTextReader = new XmlTextReader((TextReader) stringReader);
        context.reader = xmlTextReader;
        xmlTextReader.WhitespaceHandling = WhitespaceHandling.Significant;
        if (this.ReadDocument(context, ref rootNode))
          xmlTextReader.Close();
        else
          goto label_20;
      }
      foreach (Node actualNode in rootNode.possibleChildren[0].actualNodes)
      {
        if (!actualNode.nodeError)
          collection.Add(actualNode.innerText);
      }
label_20:
      return collection;
    }

    internal TypeTable Clone()
    {
      TypeTable typeTable = new TypeTable();
      typeTable.Update(string.Empty, this, false);
      return typeTable;
    }

    internal void Initialize(
      string snapinName,
      string fileToLoad,
      Collection<string> errors,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      LoadContext context = new LoadContext(snapinName, fileToLoad, errors);
      ExternalScriptInfo externalScriptInfo;
      string scriptContents;
      try
      {
        externalScriptInfo = new ExternalScriptInfo(fileToLoad, fileToLoad);
        scriptContents = externalScriptInfo.ScriptContents;
      }
      catch (SecurityException ex)
      {
        context.AddError("Exception", (object) ex.Message);
        return;
      }
      if (authorizationManager != null)
      {
        try
        {
          authorizationManager.ShouldRunInternal((CommandInfo) externalScriptInfo, CommandOrigin.Internal, host);
        }
        catch (PSSecurityException ex)
        {
          string str = ResourceManagerCache.FormatResourceString("TypesXml", "ValidationException", (object) snapinName, (object) fileToLoad, (object) ex.Message);
          errors.Add(str);
          return;
        }
      }
      using (StringReader stringReader = new StringReader(scriptContents))
      {
        XmlTextReader xmlTextReader = new XmlTextReader((TextReader) stringReader);
        context.reader = xmlTextReader;
        xmlTextReader.WhitespaceHandling = WhitespaceHandling.Significant;
        this.Update(context);
        xmlTextReader.Close();
      }
    }

    internal void Add(string typeFile, bool shouldPrepend)
    {
      if (string.IsNullOrEmpty(typeFile) || !Path.IsPathRooted(typeFile))
        throw TypeTable.tracer.NewArgumentException(nameof (typeFile), "TypesXml", "TypeFileNotRooted", (object) typeFile);
      lock (this.typeFileList)
      {
        if (shouldPrepend)
          this.typeFileList.Insert(0, typeFile);
        else
          this.typeFileList.Add(typeFile);
      }
    }

    internal void Remove(string typeFile)
    {
      lock (this.typeFileList)
        this.typeFileList.Remove(typeFile);
    }

    internal void Update(
      Collection<PSSnapInTypeAndFormatErrors> PSSnapinFiles,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      if (this.isShared)
        throw TypeTable.tracer.NewInvalidOperationException("TypesXml", "SharedTypeTableCannotBeUpdated");
      lock (this.members)
      {
        this.members.Clear();
        this.typeConverters.Clear();
        this.typeAdapters.Clear();
        this.consolidatedMembers.Clear();
        this.consolidatedSpecificProperties.Clear();
        foreach (PSSnapInTypeAndFormatErrors psSnapinFile in PSSnapinFiles)
          this.Initialize(psSnapinFile.PSSnapinName, psSnapinFile.FullPath, psSnapinFile.Errors, authorizationManager, host);
      }
    }

    internal void Update(string filePath, Collection<string> errors, bool clearTable) => this.Update(filePath, filePath, errors, clearTable, (AuthorizationManager) null, (PSHost) null);

    internal void Update(
      string moduleName,
      string filePath,
      Collection<string> errors,
      bool clearTable,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      if (filePath == null)
        throw new ArgumentNullException(nameof (filePath));
      if (errors == null)
        throw new ArgumentNullException(nameof (errors));
      if (this.isShared)
        throw TypeTable.tracer.NewInvalidOperationException("TypesXml", "SharedTypeTableCannotBeUpdated");
      lock (this.members)
      {
        if (clearTable)
        {
          this.members.Clear();
          this.typeConverters.Clear();
          this.typeAdapters.Clear();
        }
        this.consolidatedMembers.Clear();
        this.consolidatedSpecificProperties.Clear();
        this.Initialize(moduleName, filePath, errors, authorizationManager, host);
      }
    }

    internal void Update(string moduleName, TypeTable typeTable, bool clearTable)
    {
      if (typeTable == null)
        throw TypeTable.tracer.NewArgumentNullException(nameof (typeTable));
      if (this.isShared)
        throw TypeTable.tracer.NewInvalidOperationException("TypesXml", "SharedTypeTableCannotBeUpdated");
      lock (this.members)
      {
        if (clearTable)
        {
          this.members.Clear();
          this.typeConverters.Clear();
          this.typeAdapters.Clear();
          this.consolidatedMembers.Clear();
          this.consolidatedSpecificProperties.Clear();
        }
        foreach (string key in typeTable.members.Keys)
        {
          PSMemberInfoInternalCollection<PSMemberInfo> member = typeTable.members[key];
          PSMemberInfoInternalCollection<PSMemberInfo> internalCollection = new PSMemberInfoInternalCollection<PSMemberInfo>();
          foreach (PSMemberInfo psMemberInfo in (PSMemberInfoCollection<PSMemberInfo>) member)
            internalCollection.Add(psMemberInfo.Copy());
          this.members.Add(key, internalCollection);
        }
        foreach (string key in typeTable.typeAdapters.Keys)
          this.typeAdapters.Add(key, typeTable.typeAdapters[key]);
        foreach (string key in typeTable.typeConverters.Keys)
          this.typeConverters.Add(key, typeTable.typeConverters[key]);
      }
    }
  }
}
