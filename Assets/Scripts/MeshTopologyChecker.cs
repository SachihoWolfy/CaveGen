using UnityEngine;
using System.Collections.Generic;

public class MeshTopologyChecker : MonoBehaviour
{
    public static void CheckMeshTopology(Mesh mesh)
    {
        HashSet<Edge> edges = new HashSet<Edge>();
        Dictionary<Edge, int> edgeCount = new Dictionary<Edge, int>();
        HashSet<int> verticesInUse = new HashSet<int>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int v0 = mesh.triangles[i];
            int v1 = mesh.triangles[i + 1];
            int v2 = mesh.triangles[i + 2];

            verticesInUse.Add(v0);
            verticesInUse.Add(v1);
            verticesInUse.Add(v2);

            Edge edge1 = new Edge(v0, v1);
            Edge edge2 = new Edge(v1, v2);
            Edge edge3 = new Edge(v2, v0);

            AddEdgeCount(edge1, edgeCount);
            AddEdgeCount(edge2, edgeCount);
            AddEdgeCount(edge3, edgeCount);
        }


        bool hasNonManifoldEdges = false;
        foreach (var edge in edgeCount)
        {
            if (edge.Value != 2) 
            {
                Debug.LogWarning("Non-manifold edge detected: " + edge.Key);
                hasNonManifoldEdges = true;
            }
        }


        bool hasHoles = false;
        foreach (var edge in edgeCount)
        {
            if (edge.Value == 1) 
            {
                Debug.LogWarning("Hole detected in mesh at edge: " + edge.Key);
                hasHoles = true;
            }
        }

        bool hasUnusedVertices = false;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (!verticesInUse.Contains(i))
            {
                Debug.LogWarning("Unused vertex found at index: " + i);
                hasUnusedVertices = true;
            }
        }

        if (hasNonManifoldEdges || hasHoles || hasUnusedVertices)
        {
            Debug.LogError("Mesh has topology issues!");
        }
        else
        {
            Debug.Log("Mesh topology is valid.");
        }
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
