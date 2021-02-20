// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public class PSEventArgs : EventArgs
  {
    private string computerName;
    private Guid runspaceId;
    private int eventIdentifier;
    private object sender;
    private EventArgs sourceEventArgs;
    private object[] sourceArgs;
    private string sourceIdentifier;
    private DateTime timeGenerated;
    private PSObject data;
    private bool forwardEvent;

    internal PSEventArgs(
      string computerName,
      Guid runspaceId,
      int eventIdentifier,
      string sourceIdentifier,
      object sender,
      object[] originalArgs,
      PSObject additionalData)
    {
      if (originalArgs != null)
      {
        foreach (object originalArg in originalArgs)
        {
          if (originalArg is EventArgs eventArgs)
          {
            this.sourceEventArgs = eventArgs;
            break;
          }
          if (ForwardedEventArgs.IsRemoteSourceEventArgs(originalArg))
          {
            this.sourceEventArgs = (EventArgs) new ForwardedEventArgs((PSObject) originalArg);
            break;
          }
        }
      }
      this.computerName = computerName;
      this.runspaceId = runspaceId;
      this.eventIdentifier = eventIdentifier;
      this.sender = sender;
      this.sourceArgs = originalArgs;
      this.sourceIdentifier = sourceIdentifier;
      this.timeGenerated = DateTime.Now;
      this.data = additionalData;
      this.forwardEvent = false;
    }

    public string ComputerName
    {
      get => this.computerName;
      internal set => this.computerName = value;
    }

    public Guid RunspaceId
    {
      get => this.runspaceId;
      internal set => this.runspaceId = value;
    }

    public int EventIdentifier
    {
      get => this.eventIdentifier;
      internal set => this.eventIdentifier = value;
    }

    public object Sender => this.sender;

    public EventArgs SourceEventArgs => this.sourceEventArgs;

    public object[] SourceArgs => this.sourceArgs;

    public string SourceIdentifier => this.sourceIdentifier;

    public DateTime TimeGenerated
    {
      get => this.timeGenerated;
      internal set => this.timeGenerated = value;
    }

    public PSObject MessageData => this.data;

    internal bool ForwardEvent
    {
      get => this.forwardEvent;
      set => this.forwardEvent = value;
    }
  }
}
