using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph<T> where T : class
{
    protected Dictionary<int, Node> Nodes = new Dictionary<int, Node>();
    protected Dictionary<Pair, Edge> Edges = new Dictionary<Pair, Edge>();
    protected int NodeIDCount;

    public Graph() { }
    public Graph(Graph<T> original)
    {
        Nodes = (from x in original.Nodes select x).ToDictionary(x => x.Key, x => x.Value.Clone());
        Edges = (from x in original.Edges select x).ToDictionary(x => x.Key, x => x.Value);
        NodeIDCount = original.NodeIDCount;
    }

    public int AddNode(T data)
    {
        Node node = new Node(NodeIDCount, data);
        Nodes.Add(node.NodeID, node);
        NodeIDCount++;
        return node.NodeID;
    }


    public bool RemoveNode(int nodeID)
    {
        if (Nodes.ContainsKey(nodeID))
        {
            foreach (Node neighbor in Nodes.Where(a => a.Value.Neighbors.Contains(nodeID)).Select(a => a.Value))
            {
                neighbor.Neighbors.Remove(nodeID);
            }

            List<Pair> edgesToRemove = new List<Pair>();
            foreach (Edge edge in Edges.Where(a => a.Value.Nodes.A == nodeID || a.Value.Nodes.B == nodeID).Select(a => a.Value))
            {
                edgesToRemove.Add(edge.Nodes);
            }
            foreach (Pair p in edgesToRemove)
            {
                Edges.Remove(p);
            }
            Nodes.Remove(nodeID);

            return true;
        }

        Debug.Log("Node not found in graph");
        return false;
    }

    public int[] FindNodesWithData(T data)
    {
        var result = Nodes.Where(a => ((T)a.Value.Data).Equals(data)).Select(a => a.Key).ToArray();
        return result;
    }

    public int[] GetNeighbours(int nodeID)
    {
        if (Nodes.ContainsKey(nodeID))
            return Nodes[nodeID].Neighbors.ToArray();

        Debug.Log("Node not found in graph");
        return null;
    }

    public int[] GetAllNodeIDs()
    {
        return Nodes.Keys.ToArray();
    }

    public Vector2Int[] GetAllEdges()
    {
        var result = new Vector2Int[Edges.Count];
        var edgeArray = Edges.Values.ToArray();
        for (int i = 0; i < Edges.Count; i++)
        {
            result[i] = new Vector2Int(edgeArray[i].Nodes.A, edgeArray[i].Nodes.B);
        }
        return result;
    }

    public T[] GetAllNodeData()
    {
        return Nodes.Values.Select(node => node.Data).ToArray();
    }

    public int NodeCount()
    {
        return Nodes.Count();
    }

    public int EdgeCount()
    {
        return Edges.Count;
    }

    public T GetNodeData(int nodeID)
    {
        if (Nodes.ContainsKey(nodeID))
        {
            return Nodes[nodeID].Data;
        }

        Debug.Log("Node not found in graph");
        return default(T);
    }

    public void ReplaceNodeData(int nodeID, T data)
    {
        if (Nodes.ContainsKey(nodeID))
            Nodes[nodeID].Data = data;
        else
            Debug.Log("Node not found in graph");
    }

    public bool AddEdge(int node1, int node2, int value)
    {
        bool nodesExist = Nodes.ContainsKey(node1) && Nodes.ContainsKey(node2);
        if (nodesExist && !Edges.ContainsKey(new Pair(node1, node2)) && node1 != node2)
        {
            Edge edge = new Edge(node1, node2, value);
            Nodes[node1].AddNeighbor(node2);
            Nodes[node2].AddNeighbor(node1);
            Edges.Add(edge.Nodes, edge);
            return true;
        }

        if (!nodesExist)
            Debug.Log("One or both of nodes not found in graph");
        
        return false;
    }

    public bool RemoveEdge(int node1, int node2)
    {
        if (Edges.Remove(new Pair(node1, node2)))
        {
            Nodes[node1].Neighbors.RemoveWhere(node => node == node2);
            Nodes[node2].Neighbors.RemoveWhere(node => node == node1);
            return true;
        }

        Debug.Log("Edge not found in graph");
        return false;
    }

    public int GetEdgeValue(int node1, int node2)
    {
        Edge edge;
        if (Edges.TryGetValue(new Pair(node1, node2), out edge))
            return edge.Value;

        Debug.Log("Edge not found in graph");
        return -1;
    }

    public int GetEdgeValue(Vector2Int edge)
    {
        Edge element;
        if (Edges.TryGetValue(new Pair(edge.x, edge.y), out element))
            return element.Value;

        Debug.Log("Edge not found in graph");
        return -1;
    }

    public bool SetEdgeValue(int node1, int node2, int weight)
    {
        Pair pair = new Pair(node1, node2);
        if (Edges.ContainsKey(pair))
        {
            Edges[pair].Value = weight;
            return true;
        }

        Debug.Log("Edge not found in graph");
        return false;
    }

    protected class Edge
    {

        public readonly Pair Nodes;
        public int Value;

        public Edge(int node1, int node2, int value)
        {
            Nodes = new Pair(node1, node2);
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals(Nodes, ((Edge)obj).Nodes);
        }

        public override string ToString()
        {
            return Nodes.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = -1475187997;
            hashCode = hashCode * -1521134295 + EqualityComparer<Pair>.Default.GetHashCode(Nodes);
            return hashCode;
        }

        public static bool operator ==(Edge edge1, Edge edge2)
        {
            return EqualityComparer<Edge>.Default.Equals(edge1, edge2);
        }

        public static bool operator !=(Edge edge1, Edge edge2)
        {
            return !(edge1 == edge2);
        }
    }

    protected class Node : IEqualityComparer<Node>
    {
        public readonly int NodeID;
        public readonly HashSet<int> Neighbors = new HashSet<int>();
        public T Data;

        public Node(int nodeID, T data)
        {
            NodeID = nodeID;
            Data = data;
        }

        public bool Equals(Node x, Node y)
        {
            return y != null && x != null && x.NodeID == y.NodeID;
        }

        public int GetHashCode(Node obj)
        {
            return NodeID.GetHashCode();
        }

        public bool AddNeighbor(int node)
        {
            return node != this.NodeID && Neighbors.Add(node);
        }

        public Node Clone()
        {
            var result = new Node(NodeID, Data);
            result.Neighbors.UnionWith(Neighbors);
            return result;
        }
    }

    protected class Pair
    {
        public readonly int A, B; // with A <= B

        public Pair(int a, int b)
        {
            A = a < b ? a : b;
            B = a > b ? a : b;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Pair)obj;
            return other.A == A && other.B == B;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (A * 397) ^ B;
            }
        }

        public override string ToString()
        {
            return A + " " + B;
        }

    }
}