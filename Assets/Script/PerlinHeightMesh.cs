//procedural grid mesh based on perlin noise.
using UnityEngine;
using System.Collections;

public class PerlinHeightMesh : MonoBehaviour
{
    public Vector3 Unitsize = new Vector3(200, 30, 200);
    public Vector2 MeshSize = new Vector2(50, 50); //texturesize
    public float lacunarity = 2.404f;
    public float h = 0.354f;
    public float gain = 1.0f;
    public float octaves = 2.315f;
    public float offset = 0.245f;
    public float scale = 0.053f;
    public float offsetPos = 0.0f;
    public Vector2 TimeMove = new Vector2(0.005f, 0);

    private Vector2 FracTime = Vector2.zero;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private Vector4[] tangents;
    private Vector2 uvScale;
    private Vector3 sizeScale;
    private int[] triangles;

    void Awake()
    {
        mesh = gameObject.AddComponent<MeshFilter>().mesh;
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material.color = Color.blue;
        
        // Build vertices and UVs
        uvScale = new Vector2(1.0f / (MeshSize.x - 1f), 1.0f / (MeshSize.y - 1f));
        sizeScale = new Vector3(Unitsize.x / (MeshSize.x - 1f), Unitsize.y, Unitsize.z / (MeshSize.y - 1f));

        //Set up triangle array, will need to be reset if you dynamically change size
        triangles = new int[((int)MeshSize.y - 1) * ((int)MeshSize.x - 1) * 6];
        int x, y, index = 0;
        for (y = 0; y < (int)MeshSize.y - 1; y++)
        {
            for (x = 0; x < (int)MeshSize.x - 1; x++)
            {
                // For each grid cell output two triangles
                triangles[index++] = (y * (int)MeshSize.x) + x;
                triangles[index++] = ((y + 1) * (int)MeshSize.x) + x;
                triangles[index++] = (y * (int)MeshSize.x) + x + 1;
                triangles[index++] = ((y + 1) * (int)MeshSize.x) + x;
                triangles[index++] = ((y + 1) * (int)MeshSize.x) + x + 1;
                triangles[index++] = (y * (int)MeshSize.x) + x + 1;
            }
        }

        GenerateFractalPlaneMesh();
    }

    void Update()
    {
        GenerateFractalPlaneMesh();
    }

    private void GenerateFractalPlaneMesh()
    {
        mesh.Clear();
        vertices = new Vector3[(int)MeshSize.y * (int)MeshSize.x];
        uv = new Vector2[(int)MeshSize.y * (int)MeshSize.x];
        tangents = new Vector4[(int)MeshSize.y * (int)MeshSize.x];

        int x, y;
        FracTime += TimeMove;
        for (y = 0; y < (int)MeshSize.y; ++y)
        {
            for (x = 0; x < (int)MeshSize.x; ++x)
            {
                float value = FractalNoise(x * scale + FracTime.x, y * scale + FracTime.y);
                float pixelHeight = new Color(value, value, value).grayscale;
                Vector3 vertex = new Vector3(x, pixelHeight, y);
                vertices[y * (int)MeshSize.x + x] = Vector3.Scale(sizeScale, vertex);
                uv[y * (int)MeshSize.x + x] = Vector2.Scale(new Vector2(x, y), uvScale);

                Vector3 vertexL = new Vector3(x - 1, new Color(value, value, value).grayscale, y);
                Vector3 vertexR = new Vector3(x + 1, new Color(value, value, value).grayscale, y);
                Vector3 tan = Vector3.Scale(sizeScale, vertexR - vertexL).normalized;
                tangents[y * (int)MeshSize.x + x] = new Vector4(tan.x, tan.y, tan.z, -1.0f);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        // Auto-calculate vertex normals from the mesh
        mesh.RecalculateNormals();
        // Assign tangents after recalculating normals
        mesh.tangents = tangents;
    }

    private float FractalNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }
}
