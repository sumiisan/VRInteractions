using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexColored : MonoBehaviour
{
    protected Mesh mesh;

    public void InitMesh(GameObject obj) {
        mesh = obj.GetComponent<MeshFilter>().mesh;
    }

    public void SetVertexColor(Color col) {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = col;

        mesh.colors = colors;
    }

}

