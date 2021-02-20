// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Node
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  internal class Node
  {
    internal string name;
    internal bool hasInnerText;
    internal NodeCardinality cardinality;
    internal Node[] possibleChildren;
    internal Collection<Node> actualNodes;
    internal bool? isHidden = new bool?();
    internal int nodeCount;
    internal bool nodeError;
    internal string innerText;
    internal int lineNumber;

    internal Node(
      string name,
      bool hasInnerText,
      NodeCardinality cardinality,
      Node[] possibleChildren)
    {
      this.name = name;
      this.hasInnerText = hasInnerText;
      this.cardinality = cardinality;
      this.possibleChildren = possibleChildren;
      this.actualNodes = new Collection<Node>();
    }

    internal Node(
      string name,
      bool hasInnerText,
      NodeCardinality cardinality,
      Node[] possibleChildren,
      bool? supportsIsHidden)
      : this(name, hasInnerText, cardinality, possibleChildren)
    {
      this.isHidden = supportsIsHidden;
    }

    internal Node Clone() => new Node(this.name, this.hasInnerText, this.cardinality, Node.CloneNodeArray(this.possibleChildren), this.isHidden);

    internal static Node[] CloneNodeArray(Node[] source)
    {
      Node[] nodeArray = new Node[source.Length];
      for (int index = 0; index < source.Length; ++index)
        nodeArray[index] = source[index].Clone();
      return nodeArray;
    }

    internal delegate void NodeMethod(LoadContext context, Node node);
  }
}
