// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InvocationInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class InvocationInfo
  {
    private CommandInfo commandInfo;
    private Token scriptToken;
    private string invocationName;
    private bool isDeserializedObject;
    private Token parameterToken;
    private Dictionary<string, object> boundParameters;
    private List<object> unboundArguments;
    private int deserializedScriptLineNumber;
    private int deserializedOffsetInLine;
    private long historyId = -1;
    private string deserializedScriptName;
    private string deserializedLine;
    private string deserializedPositionMessage;
    private int pipelineLength;
    private int pipelinePosition;
    private int[] pipelineIterationInfo = new int[0];
    private bool expectingInput;
    private CommandOrigin commandOrigin;

    internal InvocationInfo(InternalCommand command)
      : this(command.CommandInfo, command.CallingToken)
    {
      if (command == null)
        return;
      this.commandOrigin = command.CommandOrigin;
    }

    internal InvocationInfo(CommandInfo commandInfo, Token token)
      : this(commandInfo, token, (Token) null, (ExecutionContext) null)
    {
    }

    internal InvocationInfo(CommandInfo commandInfo, Token scriptToken, Token parameterToken)
      : this(commandInfo, scriptToken, parameterToken, (ExecutionContext) null)
    {
    }

    internal InvocationInfo(CommandInfo commandInfo, Token token, ExecutionContext context)
      : this(commandInfo, token, (Token) null, context)
    {
    }

    internal InvocationInfo(
      CommandInfo commandInfo,
      Token scriptToken,
      Token parameterToken,
      ExecutionContext context)
    {
      this.commandInfo = commandInfo;
      this.scriptToken = scriptToken;
      this.parameterToken = parameterToken;
      this.commandOrigin = CommandOrigin.Internal;
      this.isDeserializedObject = false;
      ExecutionContext executionContext = (ExecutionContext) null;
      if (commandInfo != null && commandInfo.Context != null)
        executionContext = commandInfo.Context;
      else if (context != null)
        executionContext = context;
      if (executionContext == null || !(executionContext.CurrentRunspace is LocalRunspace currentRunspace))
        return;
      HistoryInfo[] entries = currentRunspace.History.GetEntries(-1L, 1L, (SwitchParameter) true);
      if (entries.Length <= 0)
        return;
      this.historyId = entries[0].Id + 1L;
    }

    internal Token ScriptToken
    {
      get => this.scriptToken;
      set => this.scriptToken = value;
    }

    internal Token ParameterToken
    {
      get => this.parameterToken;
      set => this.parameterToken = value;
    }

    public CommandInfo MyCommand => this.commandInfo;

    public Dictionary<string, object> BoundParameters
    {
      get
      {
        if (this.boundParameters == null)
          this.boundParameters = new Dictionary<string, object>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        return this.boundParameters;
      }
      internal set => this.boundParameters = value;
    }

    public List<object> UnboundArguments
    {
      get
      {
        if (this.unboundArguments == null)
          this.unboundArguments = new List<object>();
        return this.unboundArguments;
      }
      internal set => this.unboundArguments = value;
    }

    public int ScriptLineNumber
    {
      get
      {
        if (this.scriptToken != null)
          return this.scriptToken.LineNumber;
        return this.isDeserializedObject ? this.deserializedScriptLineNumber : 0;
      }
    }

    public int OffsetInLine
    {
      get
      {
        int num = 0;
        if (this.parameterToken != null)
          num = this.parameterToken.OffsetInLine;
        else if (this.scriptToken != null)
          num = this.scriptToken.OffsetInLine;
        else if (this.isDeserializedObject)
          num = this.deserializedOffsetInLine;
        return num;
      }
    }

    public long HistoryId
    {
      get => this.historyId;
      internal set => this.historyId = value;
    }

    public string ScriptName
    {
      get
      {
        if (this.scriptToken != null)
          return this.scriptToken.File ?? "";
        return this.isDeserializedObject ? this.deserializedScriptName : "";
      }
    }

    public string Line
    {
      get
      {
        if (this.scriptToken != null)
          return this.scriptToken.Line;
        return this.isDeserializedObject ? this.deserializedLine : "";
      }
    }

    public string PositionMessage
    {
      get
      {
        if (this.parameterToken != null)
          return "\n" + this.parameterToken.Position();
        if (this.scriptToken != null)
          return "\n" + this.scriptToken.Position();
        return this.isDeserializedObject ? this.deserializedPositionMessage : "";
      }
    }

    public string InvocationName
    {
      get
      {
        if (this.scriptToken != null)
          return this.scriptToken.TokenText;
        return this.invocationName != null ? this.invocationName : "";
      }
      internal set => this.invocationName = value;
    }

    public int PipelineLength
    {
      get => this.pipelineLength;
      internal set => this.pipelineLength = value;
    }

    public int PipelinePosition
    {
      get => this.pipelinePosition;
      internal set => this.pipelinePosition = value;
    }

    internal int[] PipelineIterationInfo
    {
      get => this.pipelineIterationInfo;
      set => this.pipelineIterationInfo = value;
    }

    public bool ExpectingInput
    {
      get => this.expectingInput;
      internal set => this.expectingInput = value;
    }

    public CommandOrigin CommandOrigin
    {
      get => this.commandOrigin;
      internal set => this.commandOrigin = value;
    }

    internal InvocationInfo(PSObject psObject)
    {
      this.isDeserializedObject = true;
      this.scriptToken = (Token) null;
      this.parameterToken = (Token) null;
      this.commandOrigin = (CommandOrigin) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_CommandOrigin");
      this.expectingInput = (bool) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_ExpectingInput");
      this.invocationName = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_InvocationName");
      this.deserializedLine = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_Line");
      this.deserializedOffsetInLine = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_OffsetInLine");
      this.historyId = (long) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_HistoryId");
      this.pipelineLength = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_PipelineLength");
      this.pipelinePosition = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_PipelinePosition");
      this.deserializedPositionMessage = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_PositionMessage");
      this.deserializedScriptLineNumber = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_ScriptLineNumber");
      this.deserializedScriptName = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_ScriptName");
      this.commandInfo = (CommandInfo) RemoteCommandInfo.FromPSObjectForRemoting(psObject);
      ArrayList propertyBaseObject1 = (ArrayList) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_PipelineIterationInfo");
      this.pipelineIterationInfo = propertyBaseObject1 == null ? new int[0] : (int[]) propertyBaseObject1.ToArray(Type.GetType("System.Int32"));
      Hashtable propertyBaseObject2 = (Hashtable) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_BoundParameters");
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      if (propertyBaseObject2 != null)
      {
        foreach (DictionaryEntry dictionaryEntry in propertyBaseObject2)
          dictionary.Add((string) dictionaryEntry.Key, dictionaryEntry.Value);
      }
      this.boundParameters = dictionary;
      ArrayList propertyBaseObject3 = (ArrayList) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_UnboundArguments");
      this.unboundArguments = new List<object>();
      if (propertyBaseObject3 == null)
        return;
      foreach (object obj in propertyBaseObject3)
        this.unboundArguments.Add(obj);
    }

    internal void ToPSObjectForRemoting(PSObject psObject)
    {
      RemotingEncoder.AddNoteProperty<object>(psObject, "InvocationInfo_BoundParameters", (RemotingEncoder.ValueGetterDelegate<object>) (() => (object) this.BoundParameters));
      RemotingEncoder.AddNoteProperty<CommandOrigin>(psObject, "InvocationInfo_CommandOrigin", (RemotingEncoder.ValueGetterDelegate<CommandOrigin>) (() => this.CommandOrigin));
      RemotingEncoder.AddNoteProperty<bool>(psObject, "InvocationInfo_ExpectingInput", (RemotingEncoder.ValueGetterDelegate<bool>) (() => this.ExpectingInput));
      RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_InvocationName", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.InvocationName));
      RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_Line", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.Line));
      RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_OffsetInLine", (RemotingEncoder.ValueGetterDelegate<int>) (() => this.OffsetInLine));
      RemotingEncoder.AddNoteProperty<long>(psObject, "InvocationInfo_HistoryId", (RemotingEncoder.ValueGetterDelegate<long>) (() => this.HistoryId));
      RemotingEncoder.AddNoteProperty<int[]>(psObject, "InvocationInfo_PipelineIterationInfo", (RemotingEncoder.ValueGetterDelegate<int[]>) (() => this.PipelineIterationInfo));
      RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_PipelineLength", (RemotingEncoder.ValueGetterDelegate<int>) (() => this.PipelineLength));
      RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_PipelinePosition", (RemotingEncoder.ValueGetterDelegate<int>) (() => this.PipelinePosition));
      RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_PositionMessage", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.PositionMessage));
      RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_ScriptLineNumber", (RemotingEncoder.ValueGetterDelegate<int>) (() => this.ScriptLineNumber));
      RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_ScriptName", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.ScriptName));
      RemotingEncoder.AddNoteProperty<object>(psObject, "InvocationInfo_UnboundArguments", (RemotingEncoder.ValueGetterDelegate<object>) (() => (object) this.UnboundArguments));
      RemoteCommandInfo.ToPSObjectForRemoting(this.MyCommand, psObject);
    }
  }
}
