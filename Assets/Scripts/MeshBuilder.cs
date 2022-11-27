using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshBuilder
{
    public static Mesh CreateRectangle(Vector3 Offset, float Width, float Height)
    {
        Mesh Data = new Mesh();

        Data.vertices = new Vector3[]
        {
                new Vector3(Offset.x - Width * 0.5f, Offset.y + Height * 0.5f, Offset.z),
                new Vector3(Offset.x - Width * 0.5f, Offset.y - Height * 0.5f, Offset.z),
                new Vector3(Offset.x + Width * 0.5f, Offset.y - Height * 0.5f, Offset.z),
                new Vector3(Offset.x + Width * 0.5f, Offset.y + Height * 0.5f, Offset.z)
        };

        Data.SetIndices(new int[] {2, 1, 0, 3, 2, 0}, MeshTopology.Triangles, 0);

        Data.normals = new Vector3[]
        {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
        };

        return Data;
    }
}
