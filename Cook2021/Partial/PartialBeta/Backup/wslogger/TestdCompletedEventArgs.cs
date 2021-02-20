// Decompiled with JetBrains decompiler
// Type: Partial.wslogger.TestdCompletedEventArgs
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace Partial.wslogger
{
  [DebuggerStepThrough]
  [DesignerCategory("code")]
  [GeneratedCode("System.Web.Services", "2.0.50727.5420")]
  public class TestdCompletedEventArgs : AsyncCompletedEventArgs
  {
    private object[] results;

    internal TestdCompletedEventArgs(
      object[] results,
      Exception exception,
      bool cancelled,
      object userState)
      : base(exception, cancelled, userState)
    {
      this.results = results;
    }

    public MyTest Result
    {
      get
      {
        this.RaiseExceptionIfNecessary();
        return (MyTest) this.results[0];
      }
    }
  }
}
