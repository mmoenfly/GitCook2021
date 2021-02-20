// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.Pipe
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Internal
{
  internal class Pipe
  {
    [TraceSource("Pipe", "Pipe")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (Pipe), nameof (Pipe));
    private ExecutionContext _context;
    private PipelineProcessor _outputPipeline;
    private CommandProcessorBase _downstreamCmdlet;
    private PipelineReader<object> _objectReader;
    private PipelineWriter _externalWriter;
    private int _outBufferCount;
    private bool _nullPipe;
    private Queue _objectQueue;
    private bool _isRedirected;
    private ArrayList _resultList;
    private IEnumerator _enumeratorToProcess;
    private bool _enumeratorToProcessIsEmpty;

    internal PipelineProcessor PipelineProcessor => this._outputPipeline;

    internal CommandProcessorBase DownstreamCmdlet
    {
      get => this._downstreamCmdlet;
      set => this._downstreamCmdlet = value;
    }

    internal PipelineReader<object> ExternalReader
    {
      get => this._objectReader;
      set => this._objectReader = value;
    }

    internal PipelineWriter ExternalWriter
    {
      get => this._externalWriter;
      set => this._externalWriter = value;
    }

    public override string ToString() => this._downstreamCmdlet != null ? this._downstreamCmdlet.ToString() : base.ToString();

    internal int OutBufferCount
    {
      get => this._outBufferCount;
      set => this._outBufferCount = value;
    }

    internal bool NullPipe
    {
      get => this._nullPipe;
      set
      {
        this._isRedirected = true;
        this._nullPipe = value;
      }
    }

    internal Queue ObjectQueue => this._objectQueue;

    internal bool Empty
    {
      get
      {
        if (this._enumeratorToProcess != null)
          return this._enumeratorToProcessIsEmpty;
        return this._objectQueue == null || this._objectQueue.Count == 0;
      }
    }

    internal bool IsRedirected => this._downstreamCmdlet != null || this._isRedirected;

    internal Pipe() => this._objectQueue = new Queue();

    internal Pipe(ArrayList resultList)
    {
      this._isRedirected = true;
      this._resultList = resultList;
    }

    internal Pipe(ExecutionContext context, PipelineProcessor outputPipeline)
    {
      this._isRedirected = true;
      this._context = context;
      this._outputPipeline = outputPipeline;
    }

    internal Pipe(IEnumerator enumeratorToProcess)
    {
      this._enumeratorToProcess = enumeratorToProcess;
      this._enumeratorToProcessIsEmpty = false;
    }

    internal event EventHandler<ScriptCmdletVariableUpdateEventArgs> ScriptCmdletVariableUpdate;

    internal void UpdateScriptCmdletVariable(ScriptCmdletVariable variable, object value)
    {
      EventHandler<ScriptCmdletVariableUpdateEventArgs> cmdletVariableUpdate = this.ScriptCmdletVariableUpdate;
      if (cmdletVariableUpdate == null)
        return;
      cmdletVariableUpdate((object) this, new ScriptCmdletVariableUpdateEventArgs(variable, value));
    }

    internal void Add(object obj)
    {
      if (AutomationNull.Value == obj || this._nullPipe)
        return;
      if (this._outputPipeline != null)
      {
        this._context.PushPipelineProcessor(this._outputPipeline);
        this._outputPipeline.Step(obj);
        this._context.PopPipelineProcessor();
      }
      else if (this._resultList != null)
        this._resultList.Add(obj);
      else if (this._externalWriter != null)
      {
        this._externalWriter.Write(obj);
      }
      else
      {
        if (this._objectQueue == null)
          return;
        this._objectQueue.Enqueue(obj);
        if (this._downstreamCmdlet == null || this._objectQueue.Count <= this._outBufferCount)
          return;
        this._downstreamCmdlet.DoExecute();
      }
    }

    internal void AddItems(object objects) => this.AddItemsWithRedirect(objects, false);

    internal void AddItemsWithRedirect(object objects, bool makeRedirectedException)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(objects);
      if (enumerator == null)
      {
        this.Add(objects);
      }
      else
      {
        while (ParserOps.MoveNext(this._context, (Token) null, enumerator))
        {
          object o = ParserOps.Current((Token) null, enumerator);
          if (o != AutomationNull.Value)
            this.Add(makeRedirectedException ? (object) ErrorRecord.MakeRedirectedException(o) : o);
        }
      }
      if (this._externalWriter != null || this._downstreamCmdlet == null || (this._objectQueue == null || this._objectQueue.Count <= this._outBufferCount))
        return;
      this._downstreamCmdlet.DoExecute();
    }

    internal object Retrieve()
    {
      if (this._objectQueue != null && this._objectQueue.Count != 0)
        return this._objectQueue.Dequeue();
      if (this._enumeratorToProcess != null)
      {
        if (this._enumeratorToProcessIsEmpty)
          return (object) AutomationNull.Value;
        if (ParserOps.MoveNext(this._context, (Token) null, this._enumeratorToProcess))
          return ParserOps.Current((Token) null, this._enumeratorToProcess);
        this._enumeratorToProcessIsEmpty = true;
        return (object) AutomationNull.Value;
      }
      if (this.ExternalReader == null)
        return (object) AutomationNull.Value;
      try
      {
        object obj = this.ExternalReader.Read();
        if (AutomationNull.Value == obj)
          this.ExternalReader = (PipelineReader<object>) null;
        return obj;
      }
      catch (PipelineClosedException ex)
      {
        return (object) AutomationNull.Value;
      }
      catch (ObjectDisposedException ex)
      {
        return (object) AutomationNull.Value;
      }
    }

    internal void Clear()
    {
      if (this._objectQueue == null)
        return;
      this._objectQueue.Clear();
    }

    internal object[] ToArray() => this._objectQueue == null || this._objectQueue.Count == 0 ? MshCommandRuntime.StaticEmptyArray : this._objectQueue.ToArray();
  }
}
