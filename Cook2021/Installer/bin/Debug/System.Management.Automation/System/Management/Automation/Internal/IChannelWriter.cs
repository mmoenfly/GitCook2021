// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.IChannelWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal abstract class IChannelWriter
  {
    internal abstract void WriteEvent(
      PSEventId id,
      PSLevel level,
      PSOpcode opcode,
      PSTask task,
      params object[] args);

    internal abstract void WriteCritialError(
      PSEventId id,
      PSOpcode opcode,
      PSTask task,
      params object[] args);

    internal abstract void WriteError(
      PSEventId id,
      PSOpcode opcode,
      PSTask task,
      params object[] args);

    internal abstract void WriteWarning(
      PSEventId id,
      PSOpcode opcode,
      PSTask task,
      params object[] args);

    internal abstract void WriteInformation(
      PSEventId id,
      PSOpcode opcode,
      PSTask task,
      params object[] args);

    internal abstract void WriteVerbose(
      PSEventId id,
      PSOpcode opcode,
      PSTask task,
      params object[] args);

    internal abstract void WriteDebug(
      PSEventId id,
      PSOpcode opcode,
      PSTask task,
      params object[] args);
  }
}
