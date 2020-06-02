using System.Collections.Generic;
using JetBrains.Annotations;
using Test.Map;
using Test.Netowrker;
using UnityEngine;
using Random = System.Random;

public class ChunkMesh
{
    public const int g_size_shift = 4;
    public const int g_size_mask = 0x0F;
    public const int g_size = 16;
    public const int g_height = 256;
    public const int g_center = 128;
    public const int g_hmin = -128;
    public const int g_hmax = 127;
    private const float g_n = 1.0f;

    private static readonly Vector3[] s_normal_xp =
    {
        new Vector3(g_n, 0.0f, 0.0f), new Vector3(g_n, 0.0f, 0.0f), new Vector3(g_n, 0.0f, 0.0f),
        new Vector3(g_n, 0.0f, 0.0f)
    };

    private static readonly Vector3[] s_normal_xn =
    {
        new Vector3(-g_n, 0.0f, 0.0f), new Vector3(-g_n, 0.0f, 0.0f), new Vector3(-g_n, 0.0f, 0.0f),
        new Vector3(-g_n, 0.0f, 0.0f)
    };

    private static readonly Vector3[] s_normal_zp =
    {
        new Vector3(0.0f, g_n, 0.0f), new Vector3(0.0f, g_n, 0.0f), new Vector3(0.0f, g_n, 0.0f),
        new Vector3(0.0f, g_n, 0.0f)
    };

    private static readonly Vector3[] s_normal_zn =
    {
        new Vector3(0.0f, -g_n, 0.0f), new Vector3(0.0f, -g_n, 0.0f), new Vector3(0.0f, -g_n, 0.0f),
        new Vector3(0.0f, -g_n, 0.0f)
    };

    private static readonly Vector3[] s_normal_yp =
    {
        new Vector3(0.0f, 0.0f, g_n), new Vector3(0.0f, 0.0f, g_n), new Vector3(0.0f, 0.0f, g_n),
        new Vector3(0.0f, 0.0f, g_n)
    };

    private static readonly Vector3[] s_normal_yn =
    {
        new Vector3(0.0f, 0.0f, -g_n), new Vector3(0.0f, 0.0f, -g_n), new Vector3(0.0f, 0.0f, -g_n),
        new Vector3(0.0f, 0.0f, -g_n)
    };

    private static readonly Vector2[] s_uv_xp =
        {new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f)};

    private static readonly Vector2[] s_uv_yp =
        {new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f)};

    private static readonly Vector2[] s_uv_zp =
        {new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f)};

    public static readonly Vector2Int[] g_neigbor =
    {
        new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1)
    };

    private static readonly float[] g_pos =
    {
        -0.5f, 0.5f, 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 6.5f, 7.5f, 8.5f, 9.5f, 10.5f, 11.5f, 12.5f, 13.5f, 14.5f, 15.5f,
        16.5f
    };

    private static readonly float[] g_zpos =
    {
        -128.5f, -127.5f, -126.5f, -125.5f, -124.5f, -123.5f, -122.5f, -121.5f, -120.5f, -119.5f, -118.5f, -117.5f,
        -116.5f, -115.5f, -114.5f,
        -113.5f, -112.5f, -111.5f, -110.5f, -109.5f, -108.5f, -107.5f, -106.5f, -105.5f, -104.5f, -103.5f, -102.5f,
        -101.5f, -100.5f, -99.5f, -98.5f,
        -97.5f, -96.5f, -95.5f, -94.5f, -93.5f, -92.5f, -91.5f, -90.5f, -89.5f, -88.5f, -87.5f, -86.5f, -85.5f, -84.5f,
        -83.5f, -82.5f,
        -81.5f, -80.5f, -79.5f, -78.5f, -77.5f, -76.5f, -75.5f, -74.5f, -73.5f, -72.5f, -71.5f, -70.5f, -69.5f, -68.5f,
        -67.5f, -66.5f,
        -65.5f, -64.5f, -63.5f, -62.5f, -61.5f, -60.5f, -59.5f, -58.5f, -57.5f, -56.5f, -55.5f, -54.5f, -53.5f, -52.5f,
        -51.5f, -50.5f,
        -49.5f, -48.5f, -47.5f, -46.5f, -45.5f, -44.5f, -43.5f, -42.5f, -41.5f, -40.5f, -39.5f, -38.5f, -37.5f, -36.5f,
        -35.5f, -34.5f,
        -33.5f, -32.5f, -31.5f, -30.5f, -29.5f, -28.5f, -27.5f, -26.5f, -25.5f, -24.5f, -23.5f, -22.5f, -21.5f, -20.5f,
        -19.5f, -18.5f,
        -17.5f, -16.5f, -15.5f, -14.5f, -13.5f, -12.5f, -11.5f, -10.5f, -9.5f, -8.5f, -7.5f, -6.5f, -5.5f, -4.5f, -3.5f,
        -2.5f,
        -1.5f, -0.5f, 0.5f, 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 6.5f, 7.5f, 8.5f, 9.5f, 10.5f, 11.5f, 12.5f, 13.5f,
        14.5f, 15.5f, 16.5f, 17.5f, 18.5f, 19.5f, 20.5f, 21.5f, 22.5f, 23.5f, 24.5f, 25.5f, 26.5f, 27.5f, 28.5f, 29.5f,
        30.5f, 31.5f, 32.5f, 33.5f, 34.5f, 35.5f, 36.5f, 37.5f, 38.5f, 39.5f, 40.5f, 41.5f, 42.5f, 43.5f, 44.5f, 45.5f,
        46.5f, 47.5f, 48.5f, 49.5f, 50.5f, 51.5f, 52.5f, 53.5f, 54.5f, 55.5f, 56.5f, 57.5f, 58.5f, 59.5f, 60.5f, 61.5f,
        62.5f, 63.5f, 64.5f, 65.5f, 66.5f, 67.5f, 68.5f, 69.5f, 70.5f, 71.5f, 72.5f, 73.5f, 74.5f, 75.5f, 76.5f, 77.5f,
        78.5f, 79.5f, 80.5f, 81.5f, 82.5f, 83.5f, 84.5f, 85.5f, 86.5f, 87.5f, 88.5f, 89.5f, 90.5f, 91.5f, 92.5f, 93.5f,
        94.5f, 95.5f, 96.5f, 97.5f, 98.5f, 99.5f, 100.5f, 101.5f, 102.5f, 103.5f, 104.5f, 105.5f, 106.5f, 107.5f,
        108.5f, 109.5f,
        110.5f, 111.5f, 112.5f, 113.5f, 114.5f, 115.5f, 116.5f, 117.5f, 118.5f, 119.5f, 120.5f, 121.5f, 122.5f, 123.5f,
        124.5f, 125.5f,
        126.5f, 127.5f
    };

    public class Constructor
    {
        public BlockId blockId;
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<Vector2> uv;
        public List<int> triangles;

        public Constructor()
        {
            vertices = new List<Vector3>(1024);
            normals = new List<Vector3>(1024);
            uv = new List<Vector2>(1024);
            triangles = new List<int>(1024);
        }

        public void Reset()
        {
            vertices.Clear();
            normals.Clear();
            uv.Clear();
            triangles.Clear();
        }
    }

    #region Sides

    public static void AddSide_XP(Constructor con, int x, int y, int z)
    {
        int n = con.vertices.Count;

        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y + 1], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y + 1], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y], g_pos[z]));

        con.normals.AddRange(s_normal_xp);
        con.uv.AddRange(s_uv_xp);

        con.triangles.Add(n + 3);
        con.triangles.Add(n + 2);
        con.triangles.Add(n + 1);

        con.triangles.Add(n + 3);
        con.triangles.Add(n + 1);
        con.triangles.Add(n + 0);
    }

    public static void AddSide_XN(Constructor con, int x, int y, int z)
    {
        int n = con.vertices.Count;

        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y + 1], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y + 1], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y], g_pos[z]));

        con.normals.AddRange(s_normal_xn);
        con.uv.AddRange(s_uv_xp);

        con.triangles.Add(n + 0);
        con.triangles.Add(n + 1);
        con.triangles.Add(n + 2);

        con.triangles.Add(n + 0);
        con.triangles.Add(n + 2);
        con.triangles.Add(n + 3);
    }

    public static void AddSide_YP(Constructor con, int x, int y, int z)
    {
        int n = con.vertices.Count;

        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y + 1], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y + 1], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y], g_pos[z + 1]));

        con.normals.AddRange(s_normal_yp);
        con.uv.AddRange(s_uv_yp);

        con.triangles.Add(n + 3);
        con.triangles.Add(n + 2);
        con.triangles.Add(n + 1);

        con.triangles.Add(n + 3);
        con.triangles.Add(n + 1);
        con.triangles.Add(n + 0);
    }

    public static void AddSide_YN(Constructor con, int x, int y, int z)
    {
        int n = con.vertices.Count;

        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y + 1], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y + 1], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y], g_pos[z]));

        con.normals.AddRange(s_normal_yn);
        con.uv.AddRange(s_uv_xp);

        con.triangles.Add(n + 0);
        con.triangles.Add(n + 1);
        con.triangles.Add(n + 2);

        con.triangles.Add(n + 0);
        con.triangles.Add(n + 2);
        con.triangles.Add(n + 3);
    }

    public static void AddSide_ZP(Constructor con, int x, int y, int z)
    {
        int n = con.vertices.Count;

        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y + 1], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y + 1], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y + 1], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y + 1], g_pos[z + 1]));

        con.normals.AddRange(s_normal_zp);
        con.uv.AddRange(s_uv_zp);

        con.triangles.Add(n + 3);
        con.triangles.Add(n + 2);
        con.triangles.Add(n + 1);

        con.triangles.Add(n + 3);
        con.triangles.Add(n + 1);
        con.triangles.Add(n + 0);
    }

    public static void AddSide_ZN(Constructor con, int x, int y, int z)
    {
        int n = con.vertices.Count;

        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y], g_pos[z]));
        con.vertices.Add(new Vector3(g_pos[x + 1], g_zpos[y], g_pos[z + 1]));
        con.vertices.Add(new Vector3(g_pos[x], g_zpos[y], g_pos[z + 1]));

        con.normals.AddRange(s_normal_zn);
        con.uv.AddRange(s_uv_xp);

        con.triangles.Add(n + 0);
        con.triangles.Add(n + 1);
        con.triangles.Add(n + 2);

        con.triangles.Add(n + 0);
        con.triangles.Add(n + 2);
        con.triangles.Add(n + 3);
    }

    #endregion
}