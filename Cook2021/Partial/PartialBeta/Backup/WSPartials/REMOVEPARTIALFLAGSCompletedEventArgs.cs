// Decompiled with JetBrains decompiler
// Type: Partial.WSPartials.REMOVEPARTIALFLAGSCompletedEventArgs
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace Partial.WSPartials
{
  [DebuggerStepThrough]
  [GeneratedCode("System.Web.Services", "2.0.50727.5420")]
  [DesignerCategory("code")]
  public class REMOVEPARTIALFLAGSCompletedEventArgs : AsyncCompletedEventArgs
  {
    private object[] results;

    internal REMOVEPARTIALFLAGSCompletedEventArgs(
      object[] results,
      Exception exception,
      bool cancelled,
      object userState)
      : base(exception, cancelled, userState)
    {
      this.results = results;
    }

    public string Result
    {
      get
      {
        this.RaiseExceptionIfNecessary();
        return (string) this.results[0];
      }
    }
  }
}
