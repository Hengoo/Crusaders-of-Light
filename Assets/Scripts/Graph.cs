using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph<T> where T : class
{
    private readonly Dictionary<int, Node> _nodes = new Dictionary<int, Node>();
    private readonly Dictionary<Pair, Edge> _edges = new Dictionary<Pair, Edge>();
    private int _nodeIDCount;

    public void AddNode(T data)
    {
        Node node = new Node(_nodeIDCount, data);
        _nodes.Add(node.NodeID, node);
        _nodeIDCount++;
    }

    public bool RemoveNode(int nodeID)
    {
        if (_nodes.ContainsKey(nodeID))
        {
            Node node = _nodes[nodeID];
            _nodes.Remove(nodeID);

            foreach (Node neighbor in _nodes.Where(a => a.Value.Neighbors.Contains(node)).Select(a => a.Value))
            {
                neighbor.Neighbors.Remove(node);
            }

            foreach (Edge edge in _edges.Where(a => a.Value.Nodes.A == nodeID || a.Value.Nodes.B == nodeID).Select(a => a.Value))
            {
                _edges.Remove(edge.Nodes);
            }

            return true;
        }

        Debug.Log("Node not found in graph");
        return false;
    }

    public int[] FindNodesWithData(T data)
    {
        return _nodes.Where(a => a.Value.Data == data).Select(a => a.Key).ToArray();
    }


    private int[] GetNeighbours(int nodeID)
    {
        if (_nodes.ContainsKey(nodeID))
            return _nodes[nodeID].Neighbors.Select(a => a.NodeID).ToArray();

        Debug.Log("Node not found in graph");
        return null;
    }

    public T GetNodeData(int nodeID)
    {
        if (_nodes.ContainsKey(nodeID))
        {
            return _nodes[nodeID].Data;
        }

        Debug.Log("Node not found in graph");
        return default(T);
    }

    public bool AddEdge(int node1, int node2, float weight)
    {
        if (_nodes.ContainsKey(node1) && _nodes.ContainsKey(node2))
        {
            Edge edge = new Edge(node1, node2, weight);
            _nodes[node1].Neighbors.Add(_nodes[node2]);
            _nodes[node2].Neighbors.Add(_nodes[node1]);
            _edges.Add(edge.Nodes, edge);
            return true;
        }

        Debug.Log("One of nodes not found in graph");
        return false;
    }

    public bool RemoveEdge(int node1, int node2)
    {
        if (_edges.Remove(new Pair(node1, node2)))
        {
            _nodes[node1].Neighbors.RemoveWhere(a => a.NodeID == node2);
            _nodes[node2].Neighbors.RemoveWhere(a => a.NodeID == node1);
            return true;
        }

        Debug.Log("Edge not found in graph");
        return false;
    }

    public float GetEdgeWeight(int node1, int node2)
    {
        Edge edge;
        if (_edges.TryGetValue(new Pair(node1, node2), out edge))
            return edge.Weight;

        Debug.Log("Edge not found in graph");
        return -1;
    }

    public bool SetEdgeWeight(int node1, int node2, float weight)
    {
        Pair pair = new Pair(node1, node2);
        if (_edges.ContainsKey(pair))
        {
            _edges[pair].Weight = weight;
            return true;
        }

        Debug.Log("Edge not found in graph");
        return false;
    }

    private class Edge
    {
        public readonly Pair Nodes;
        public float Weight; // always >= 0

        public Edge(int node1, int node2, float weight)
        {
            Nodes = new Pair(node1, node2);
            Weight = weight >= 0 ? weight : 0;
        }
    }

    private class Node : IEqualityComparer<Node>
    {
        public readonly int NodeID;
        public readonly HashSet<Node> Neighbors = new HashSet<Node>();
        public readonly T Data;

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
    }

    private class Pair
    {
        public readonly int A, B; // with A <= B

        public Pair(int a, int b)
        {
            A = a < b ? a : b;
            B = a > b ? a : b;
        }
    }
}