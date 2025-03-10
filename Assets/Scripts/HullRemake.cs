using UnityEngine;
using System.Collections.Generic;

public class HullRemake : MonoBehaviour
{
    public static Mesh RemakeMesh(Mesh originalMesh)
    {
        // Step 1: Get original vertices and triangles
        Vector3[] vertices = originalMesh.vertices;
        int[] triangles = originalMesh.triangles;

        // Step 2: Identify boundary edges (edges that are part of only one triangle)
        HashSet<Edge> edges = new HashSet<Edge>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            edges.Add(new Edge(triangles[i], triangles[i + 1]));
            edges.Add(new Edge(triangles[i + 1], triangles[i + 2]));
            edges.Add(new Edge(triangles[i + 2], triangles[i]));
        }

        // Step 3: Detect boundary vertices (vertices that appear in only one triangle)
        HashSet<int> boundaryVertices = new HashSet<int>();
        foreach (var edge in edges)
        {
            if (!edges.Contains(edge.Reversed()))
            {
                boundaryVertices.Add(edge.v0);
                boundaryVertices.Add(edge.v1);
            }
        }

        // Step 4: Generate new faces by connecting boundary vertices (using simple triangulation)
        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<int> newTriangles = new List<int>();

        List<int> boundaryList = new List<int>(boundaryVertices);

        // Ensure the boundary vertices are ordered in a circular way
        List<int> orderedBoundary = OrderBoundaryVertices(boundaryList);

        if (orderedBoundary.Count >= 3)
        {
            // Triangulate the boundary vertices (connect the boundary in a simple fan)
            for (int i = 1; i < orderedBoundary.Count - 1; i++)
            {
                newTriangles.Add(orderedBoundary[0]);
                newTriangles.Add(orderedBoundary[i]);
                newTriangles.Add(orderedBoundary[i + 1]);
            }
        }

        // Step 5: Create a new mesh and return it
        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices.ToArray();
        newMesh.triangles = newTriangles.ToArray();
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        return newMesh;
    }

    // Helper class to define edges
    public class Edge
    {
        public int v0, v1;
        public Edge(int vertex0, int vertex1)
        {
            v0 = vertex0 < vertex1 ? vertex0 : vertex1;
            v1 = vertex0 < vertex1 ? vertex1 : vertex0;
        }

        // To ensure that the edge is unique, we'll use this method
        public override bool Equals(object obj)
        {
            Edge edge = obj as Edge;
            return edge != null && v0 == edge.v0 && v1 == edge.v1;
        }

        public override int GetHashCode()
        {
            return v0 ^ v1;
        }

        public Edge Reversed()
        {
            return new Edge(v1, v0);
        }
    }

    // Simple ordering of boundary vertices to form a loop (clockwise or counter-clockwise)
    private static List<int> OrderBoundaryVertices(List<int> boundaryVertices)
    {
        // For now, we'll just return the vertices as is.
        // Advanced ordering algorithms like convex hull or other sorting can be implemented later
        return boundaryVertices;
    }
}
