using UnityEngine;
using System.Collections.Generic;

public class MeshProcessor : MonoBehaviour
{
    //Uwwaaaa save me from my own nightmare of perfection!!
    public float mergeThreshold = 0.01f;  
    public int subdivisions = 1;
    public Mesh ProcessMesh(Mesh originalMesh)
    {
        Mesh mergedMesh = MergeVertices(originalMesh);

        Mesh subdividedMesh = SubdivideMesh(mergedMesh);

        Mesh smoothedMesh = LaplacianSmooth(subdividedMesh, 3, 0.5f);

        Mesh noisyMesh = ApplyNoise(smoothedMesh, 0.01f, 0.1f);
   
        return noisyMesh;
    }

    private Mesh MergeVertices(Mesh mesh, float mergeThreshold = 0.001f)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = mesh.uv;
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        Dictionary<Vector3, int> vertexCache = new Dictionary<Vector3, int>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 currentVertex = vertices[i];

            bool found = false;
            foreach (var key in vertexCache.Keys)
            {
                if (Vector3.Distance(currentVertex, key) < mergeThreshold)
                {
                    newTriangles.Add(vertexCache[key]);
                    newNormals[vertexCache[key]] += normals[i];  
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                newVertices.Add(currentVertex);
                newNormals.Add(normals[i]);  
                vertexCache[currentVertex] = newVertices.Count - 1;
                newTriangles.Add(newVertices.Count - 1);  
            }
        }
        // Ha, make the normals normalized. Funny.
        for (int i = 0; i < newNormals.Count; i++)
        {
            newNormals[i].Normalize();
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices.ToArray();
        newMesh.triangles = newTriangles.ToArray();
        newMesh.normals = newNormals.ToArray();  
        newMesh.uv = uv;  

        return newMesh;
    }
    // I'm losing it, avoiding duplicates is not as easy as it was supposed to be.
    private Mesh SubdivideMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        Dictionary<string, int> edgeCache = new Dictionary<string, int>();
        // You can put a function inside a funcation, and it's silly.
        int GetEdgeMidpoint(int v1, int v2)
        {
            int vertex1 = Mathf.Min(v1, v2);
            int vertex2 = Mathf.Max(v1, v2);
            string edgeKey = vertex1 + "_" + vertex2;

            if (edgeCache.ContainsKey(edgeKey))
                return edgeCache[edgeKey];

            Vector3 midpoint = (vertices[v1] + vertices[v2]) / 2f;

            newVertices.Add(midpoint);
            int newIndex = newVertices.Count - 1;

            edgeCache[edgeKey] = newIndex;
            return newIndex;
        }

        foreach (Vector3 vertex in vertices)
        {
            newVertices.Add(vertex);
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            int mid01 = GetEdgeMidpoint(i0, i1);
            int mid12 = GetEdgeMidpoint(i1, i2);
            int mid20 = GetEdgeMidpoint(i2, i0);

            newTriangles.Add(i0);    
            newTriangles.Add(mid01); 
            newTriangles.Add(mid20); 

            newTriangles.Add(i1);    
            newTriangles.Add(mid12); 
            newTriangles.Add(mid01);

            newTriangles.Add(i2);    
            newTriangles.Add(mid20);
            newTriangles.Add(mid12);

            newTriangles.Add(mid01);
            newTriangles.Add(mid12);
            newTriangles.Add(mid20);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices.ToArray();
        newMesh.triangles = newTriangles.ToArray();
        newMesh.RecalculateNormals();

        return newMesh;
    }

    // HOLES. HOLES HOLES HOLES. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    // No holes no more, it wasn't this that was the problem, but the merger.
    private Mesh LaplacianSmooth(Mesh mesh, int smoothingIterations = 1, float smoothingFactor = 0.3f)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Dictionary<int, List<int>> vertexNeighbors = new Dictionary<int, List<int>>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            if (!vertexNeighbors.ContainsKey(i0)) vertexNeighbors[i0] = new List<int>();
            if (!vertexNeighbors.ContainsKey(i1)) vertexNeighbors[i1] = new List<int>();
            if (!vertexNeighbors.ContainsKey(i2)) vertexNeighbors[i2] = new List<int>();

            vertexNeighbors[i0].Add(i1);
            vertexNeighbors[i0].Add(i2);
            vertexNeighbors[i1].Add(i0);
            vertexNeighbors[i1].Add(i2);
            vertexNeighbors[i2].Add(i0);
            vertexNeighbors[i2].Add(i1);
        }

        for (int iter = 0; iter < smoothingIterations; iter++)
        {
            Vector3[] smoothedVertices = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                if (!vertexNeighbors.ContainsKey(i) || vertexNeighbors[i].Count == 0)
                {
                    smoothedVertices[i] = vertices[i];
                    continue;
                }

                List<int> neighbors = vertexNeighbors[i];

                Vector3 averagePosition = Vector3.zero;
                foreach (int neighbor in neighbors)
                {
                    averagePosition += vertices[neighbor];
                }
                averagePosition /= neighbors.Count;

                smoothedVertices[i] = vertices[i] + (averagePosition - vertices[i]) * smoothingFactor;
            }

            vertices = smoothedVertices;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh FillHoles(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        HashSet<Edge> edges = new HashSet<Edge>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            edges.Add(new Edge(triangles[i], triangles[i + 1]));
            edges.Add(new Edge(triangles[i + 1], triangles[i + 2]));
            edges.Add(new Edge(triangles[i + 2], triangles[i]));
        }

        List<Edge> missingEdges = new List<Edge>();
        foreach (var edge in edges)
        {
            if (!edges.Contains(edge.Reversed()))
            {
                missingEdges.Add(edge);
            }
        }

        List<int> newTriangles = new List<int>(triangles);
        foreach (var edge in missingEdges)
        {
            int v0 = edge.v0;
            int v1 = edge.v1;

            int v2 = FindAdjacentVertex(v0, v1, vertices);

            if (v2 != -1)
            {
                newTriangles.Add(v0);
                newTriangles.Add(v1);
                newTriangles.Add(v2);
            }
        }

        mesh.triangles = newTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
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

        public Edge Reversed()
        {
            return new Edge(v1, v0);
        }
    }

    private int FindAdjacentVertex(int v0, int v1, Vector3[] vertices)
    {

        for (int i = 0; i < vertices.Length; i++)
        {
            if (i != v0 && i != v1)
            {
                return i;
            }
        }
        return -1;
    }
    Vector3 ApplyWallNoise(Vector3 vertex, float noiseScale, float noiseStrength)
    {
        float noiseX = Mathf.PerlinNoise(vertex.x * noiseScale, vertex.y * noiseScale);
        float noiseY = Mathf.PerlinNoise(vertex.y * noiseScale, vertex.z * noiseScale);
        float noiseZ = Mathf.PerlinNoise(vertex.z * noiseScale, vertex.x * noiseScale);

        Vector3 noise = new Vector3(noiseX, noiseY, noiseZ) * noiseStrength;

        return vertex + noise;
    }
    Mesh ApplyNoise(Mesh mesh, float noiseScale = 0.1f, float noiseStrength = 0.1f)
    {
        Mesh newNoise = mesh;
        Vector3[] vertices = newNoise.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = ApplyWallNoise(vertices[i], noiseScale, noiseStrength);
        }
        newNoise.vertices = vertices; 
        newNoise.RecalculateNormals();
        return newNoise;
    }


}
