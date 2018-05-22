﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class GrammarGraph<T> : Graph<T> where T : class, IEquatable<T> {


    // Rewrites a portion of the graph, returns true when "match" is a subgraph of this graph
    public bool Rewrite(Graph<T> match, Graph<T> replace, Dictionary<int, int> correnpondencies)
    {
        // Match pattern in graph
        Dictionary<int, int> assignments = MatchPattern(match);

        // No match found
        if (assignments.Count == 0)
            return false;

        ApplyRule(replace, correnpondencies, assignments);

        return true;
    }

    //-----------------------------------------------------------------
    //  Helper Functions
    //-----------------------------------------------------------------

    // Based on Ullman's algorithm -
    // See: https://www.cs.bgu.ac.il/~dinitz/Course/SS-12/Ullman_Algorithm.pdf
    // https://stackoverflow.com/questions/17480142/is-there-any-simple-example-to-explain-ullmann-algorithm
    private Dictionary<int, int> MatchPattern(Graph<T> pattern)
    {
        Dictionary<int, List<int>> possibilities = new Dictionary<int, List<int>>();
        int[] patternNodes = pattern.GetAllNodeIDs();

        if (patternNodes.Length == 0) // Empty pattern can't match
            return null;

        // Match each pattern vertex with canditates from this graph - Candidate list creation
        foreach (var patternNode in patternNodes)
        {
            // Get pattern node neighbours
            int[] patternNeighbours = pattern.GetNeighbours(patternNode);

            // Get all nodes with the same data from this graph
            T patternNodeData = pattern.GetNodeData(patternNode);
            List<int> matches = FindNodesWithData(patternNodeData).ToList();


            // Check if node degree is sufficiently large and neighbours' data coincide
            foreach (var match in matches.ToList())
            {
                // Degree check
                int[] neighbours = GetNeighbours(match);
                if (patternNeighbours.Length > GetNeighbours(match).Length)
                {
                    matches.Remove(match);
                    continue;
                }

                // Check if there are enough neighbours with the corresponding data
                List<T> patternNeighboursData = patternNeighbours.Select(pattern.GetNodeData).ToList();
                foreach (var neighbour in neighbours)
                {
                    T data = GetNodeData(neighbour);
                    if (patternNeighboursData.Contains(data))
                    {
                        patternNeighboursData.Remove(data);
                    }
                    else
                    {
                        if(patternNeighboursData.Count != 0)
                            matches.Remove(match);
                        break;
                    }
                }
            }

            if (matches.Count == 0) // No data match found for a pattern node - can't match
                return null;

            possibilities.Add(patternNode, matches);
        }


        // Start assigning nodes from candidates list recursively
        Dictionary<int, int> assignments = new Dictionary<int, int>();
        UpdateAssignments(pattern, assignments, possibilities);
        return assignments;
    }

    // Recursively assign nodes from pattern to graph
    private bool UpdateAssignments(Graph<T> pattern, Dictionary<int, int> assignments, Dictionary<int, List<int>> possibilities)
    {
        if (possibilities.Count == 0)
            return true; // Break condition on success -> all candidates have a match

        // Helper variables
        Dictionary<int, List<int>> currentPossibilities = new Dictionary<int, List<int>>(possibilities);
        int[] keys = possibilities.Keys.ToArray();

        // Deep copy current mapping of candidates for this level
        foreach (var key in keys)
        {
            currentPossibilities[key] = new List<int>(possibilities[key]);
        }

        // Select a random node from pattern that has not yet been assigned
        int patternNode = keys[Random.Range(0, keys.Length)];
        List<int> candidates = new List<int>(currentPossibilities[patternNode]); // List for iteration

        assignments.Add(patternNode, 0);
        currentPossibilities.Remove(patternNode);

        // Try each candidate until a solution is found or no matching is possible
        while (candidates.Count > 0)
        {
            // Choose a candidate
            int assignedNode = candidates[Random.Range(0, candidates.Count)];
            var assignedNeighborhood = GetNeighbours(assignedNode);
            candidates.Remove(assignedNode);
            assignments[patternNode] = assignedNode;

            // Remove candidate from all other sets
            foreach (var e in currentPossibilities)
            {
                currentPossibilities[e.Key].Remove(assignedNode);
            }

            // Check if neighborhood is valid
            // i.e. every neighbor of patternNode has at least one possible assignment among neighbors of candidateNode
            bool failed = false;
            foreach (var patternNeighbour in pattern.GetNeighbours(patternNode))
            {
                if (currentPossibilities.ContainsKey(patternNeighbour))
                    failed = !currentPossibilities[patternNeighbour].Any(a => assignedNeighborhood.Contains(a));
                else
                    failed = !assignedNeighborhood.Contains(assignments[patternNeighbour]);

                if (failed)
                    break;
            }
            
            // Previous condition was not met for this assignment, try another
            if(failed)
                continue;

            // Recursion
            if(UpdateAssignments(pattern, assignments, currentPossibilities)) // <--- RECURSION CALL
                return true; // Break condition on success
        }
        assignments.Remove(patternNode);

        // Break condition on fail
        return false;
    }

    // Replace match in graph with new subgraph
    private void ApplyRule(Graph<T> replace, Dictionary<int, int> correspondences, Dictionary<int, int> assignments)
    {
        List<Vector2Int> edgeList = new List<Vector2Int>();

        // Remove old nodes
        foreach (var assignment in assignments)
        {
            // Save edge information
            foreach (var neighbour in GetNeighbours(assignment.Value))
            {
                edgeList.Add(new Vector2Int(neighbour, assignment.Key));
            }

            // Replace node
            RemoveNode(assignment.Value);
        }

        // Add new nodes
        Dictionary<int, int> temp = new Dictionary<int, int>();
        foreach (var newNode in replace.GetAllNodeIDs())
        {
            temp.Add(newNode, AddNode(replace.GetNodeData(newNode)));
        }

        // Add edges back
        foreach (var edge in edgeList)
        {
            AddEdge(edge.x, temp[correspondences[edge.y]], 1);
        }
    }
}