using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrammarGraph<T> : Graph<T> where T : class, IEquatable<T> {


    // Rewrites a portion of the graph, returns true when "match" is a subgraph of this graph
    public bool Rewrite(Graph<T> match, Graph<T> replace)
    {
        // Match pattern in graph
        Graph<T> subgraph = MatchPattern(match);

        // No match found
        if (subgraph == null)
            return false;

        return true;
    }

    //-----------------------------------------------------------------
    //  Helper Functions
    //-----------------------------------------------------------------

    // Based on Ullman's algorithm -
    // See: https://www.cs.bgu.ac.il/~dinitz/Course/SS-12/Ullman_Algorithm.pdf
    // https://stackoverflow.com/questions/17480142/is-there-any-simple-example-to-explain-ullmann-algorithm
    private Graph<T> MatchPattern(Graph<T> pattern)
    {

        Dictionary<int, List<int>> canditates = new Dictionary<int, List<int>>();
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
                        matches.Remove(match);
                        break;
                    }
                }
            }

            if (matches.Count == 0) // No data match found for a pattern node - can't match
                return null;

            canditates.Add(patternNode, matches);
        }


        // Start assigning nodes from candidates list recursively




        Graph<T> result = null;
        return result;
    }

    private void ApplyRule(Graph<T> match, Graph<T> replace)
    {
        //TODO replace subgraph in original with new one
    }


}
