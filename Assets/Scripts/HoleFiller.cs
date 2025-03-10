using UnityEngine;
using System.Collections.Generic;

public class HoleFiller : MonoBehaviour
{
    // Useless now, not that it worked
    public static Mesh FillHoles(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        List<int> newTriangles = new List<int>(triangles);

        HashSet<Edge> boundaryEdges = new HashSet<Edge>();
        Dictionary<Edge, int> edgeCount = new Dictionary<Edge, int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];

            AddEdgeCount(new Edge(v0, v1), edgeCount);
            AddEdgeCount(new Edge(v1, v2), edgeCount);
            AddEdgeCount(new Edge(v2, v0), edgeCount);
        }

        foreach (var edge in edgeCount)
        {
            if (edge.Value == 1) 
            {
                boundaryEdges.Add(edge.Key);
            }
        }

        foreach (var edge in boundaryEdges)
        {
            int v0 = edge.v0;
            int v1 = edge.v1;

            Vector3 center = (vertices[v0] + vertices[v1]) / 2f;

            int centerIndex = vertices.Length;  

            List<Vector3> newVertices = new List<Vector3>(vertices) { center };
            vertices = newVertices.ToArray();

            newTriangles.Add(v0);
            newTriangles.Add(v1);
            newTriangles.Add(centerIndex);

            mesh.vertices = vertices;
            mesh.triangles = newTriangles.ToArray();
            mesh.RecalculateNormals();
        }

        return mesh;
    }

    private static void AddEdgeCount(Edge edge, Dictionary<Edge, int> edgeCount)
    {
        if (!edgeCount.ContainsKey(edge))
        {
            edgeCount[edge] = 0;
        }
        edgeCount[edge]++;
    }

    public class Edge
    {
        public int v0, v1;
        public Edge(int vertex0, int vertex1)
        {
            v0 = vertex0 < vertex1 ? vertex0 : vertex1;
            v1 = vertex0 < vertex1 ? vertex1 : vertex0;
        }

        public override bool Equals(object obj)
        {
            Edge edge = obj as Edge;
            return edge != null && v0 == edge.v0 && v1 == edge.v1;
        }

        public override int GetHashCode()
        {
            return v0 ^ v1;
        }

        public override string ToString()
        {
            return $"({v0}, {v1})";
        }
    }
}
