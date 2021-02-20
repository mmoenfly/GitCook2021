// Decompiled with JetBrains decompiler
// Type: Partial.wslogger.MyTest
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Partial.wslogger
{
  [XmlType(Namespace = "http://installs.ccisupportsite.com/wslogger/")]
  [DesignerCategory("code")]
  [DebuggerStepThrough]
  [GeneratedCode("System.Xml", "2.0.50727.5473")]
  [Serializable]
  public class MyTest
  {
    private string nameField;
    private string[] testField;
    private string errmsgField;

    public string name
    {
      get => this.nameField;
      set => this.nameField = value;
    }

    public string[] test
    {
      get => this.testField;
      set => this.testField = value;
    }

    public string errmsg
    {
      get => this.errmsgField;
      set => this.errmsgField = value;
    }
  }
}
