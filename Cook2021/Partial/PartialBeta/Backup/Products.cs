// Decompiled with JetBrains decompiler
// Type: Partial.Products
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using System.Collections.Specialized;

namespace Partial
{
  internal class Products
  {
    public string sProduct;
    public string sFileName;
    public string sUrl;
    public string sReadOnly;
    public string sMethodCall;
    public string sQueryEnd;
    public string sPassed;
    public string sVisible;
    public NameValueCollection Nvp;

    public Products()
    {
    }

    public Products(string sProd, string sFile, string sRead)
    {
      this.sProduct = sProd;
      this.sFileName = sFile;
      this.sReadOnly = sRead;
      this.Nvp = new NameValueCollection();
    }

    private struct sAttr
    {
      public string skeyname;
      public string sValue;
      public string sPassed;
    }
  }
}
