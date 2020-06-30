using System.Collections.Generic;
using JetBrains.Annotations;
using Test.Map;
using Test.Netowrker;
using UnityEngine;
using Random = System.Random;

public class ChunkMesh
{
    private const float g_n = 1.0f;
    private const float inBlockShift = 0.5f;

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

        con.vertices.Add(new Vector3(x + inBlockShift, y - inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y + inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y + inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y - inBlockShift, z - inBlockShift));

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

        con.vertices.Add(new Vector3(x - inBlockShift, y - inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y + inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y + inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y - inBlockShift, z - inBlockShift));

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

        con.vertices.Add(new Vector3(x - inBlockShift, y - inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y + inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y + inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y - inBlockShift, z + inBlockShift));

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

        con.vertices.Add(new Vector3(x - inBlockShift, y - inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y + inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y + inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y - inBlockShift, z - inBlockShift));

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

        con.vertices.Add(new Vector3(x - inBlockShift, y + inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y + inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y + inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y + inBlockShift, z + inBlockShift));

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

        con.vertices.Add(new Vector3(x - inBlockShift, y - inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y - inBlockShift, z - inBlockShift));
        con.vertices.Add(new Vector3(x + inBlockShift, y - inBlockShift, z + inBlockShift));
        con.vertices.Add(new Vector3(x - inBlockShift, y - inBlockShift, z + inBlockShift));

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