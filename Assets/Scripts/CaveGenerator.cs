using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CaveGenerator : MonoBehaviour
{
    public int width = 32;
    public int height = 32;
    public int depth = 32;
    public float noiseScale = 0.1f;
    public float threshold = 0.5f;
    private float[,,] densityGrid;
    private float minDensity, maxDensity;
    public Material caveMaterial;
    public GameObject spherePrefab;
    private GameObject cave;
    private MeshProcessor meshProcessor;
    public Slider noiseSlider;
    public Slider thresholdSlider;

    void Start()
    {
        cave = new GameObject("Cave");
        meshProcessor = FindObjectOfType<MeshProcessor>();
        GenerateCave();
    }

    public void RegenerateCave()
    {
        Destroy(cave);
        cave = new GameObject("Cave");
        GenerateCave();
    }
    void GenerateCave()
    {
        GenerateDensityGrid();
        GenerateMesh();
        //VisualizeDensityGrid();
    }
    public void OnNoiseScaleChanged()
    {
        noiseScale = noiseSlider.value;
    }

    public void OnThresholdChanged()
    {
        threshold = thresholdSlider.value;
    }

    void GenerateDensityGrid()
    {
        densityGrid = new float[width + 1, height + 1, depth + 1];
        minDensity = float.MaxValue;
        maxDensity = float.MinValue;

        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    if (x == 0 || x == width || y == 0 || y == height || z == 0 || z == depth)
                    {
                        densityGrid[x, y, z] = 0f;
                    }
                    else
                    {
                        float sampleX = x * noiseScale;
                        float sampleY = y * noiseScale;
                        float sampleZ = z * noiseScale;

                        float noiseXY = Mathf.PerlinNoise(sampleX, sampleY);
                        float noiseXZ = Mathf.PerlinNoise(sampleX, sampleZ);
                        float noiseYZ = Mathf.PerlinNoise(sampleY, sampleZ);

                        float noiseValue = (noiseXY + noiseXZ + noiseYZ) / 3f;
                        densityGrid[x, y, z] = noiseValue;

                        if (noiseValue < minDensity) minDensity = noiseValue;
                        if (noiseValue > maxDensity) maxDensity = noiseValue;
                    }
                }
            }
        }
    }

    void GenerateMesh()
    {
        Mesh mesh = MarchingCubes.GenerateMesh(densityGrid, threshold);
        mesh = meshProcessor.ProcessMesh(mesh);
        MeshFilter mf = cave.AddComponent<MeshFilter>();
        MeshRenderer mr = cave.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = caveMaterial;
    }

    void VisualizeDensityGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    float density = densityGrid[x, y, z];
                    if (density < threshold) continue;

                    Color color = Color.Lerp(Color.blue, Color.red, Mathf.InverseLerp(minDensity, maxDensity, density));
                    GameObject sphere = Instantiate(spherePrefab, new Vector3(x, y, z), Quaternion.identity);
                    sphere.GetComponent<Renderer>().material.color = color;
                    sphere.transform.localScale = Vector3.one * 0.2f;
                }
            }
        }
    }
}
