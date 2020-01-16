using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneGenerator : MonoBehaviour
{
    // Start is called before the first frame update

[SerializeField] Material material;
[SerializeField] Vector3 dimensions;
float spacing = 0.01f;
GameObject xPlanes;
BHTex_xslice bhtex_xslice;





    void Start()
    {
        bhtex_xslice = GetComponent<BHTex_xslice>();
        xPlanes = new GameObject("xPlanes");
        bhtex_xslice.CreateColor();
        BHTex_xslice.OnBHTexCalcComplete += OnBHTexCalcComplete;

    }

    void OnBHTexCalcComplete()
    {
        material.SetTexture("_MainTex",bhtex_xslice.Texture3D);
        CreateXplanes();
    }

    void CreateXPlanes()
    {
        Vector3 start = new Vector3(-dimensions.x,0.0f, 0.0f)*0.5f;
        Vector3 end = new Vector3(dimensions.x, 0.0f, 0.0f)*0.5f;
        int numberOfplanes = (int) (dimensions.x / spacing);

        for(int i=0; i<numberOfplanes;i++)
        {
            Vector3 centerPoint = start + Vector3.right * (i*spacing);
            float uvOffset = (1.0f/numberOfplanes)*i;

            GameObject plane = CreatePlane(centerPoint, uvOffset);
            //Create a plane
            plane.transform.parent = xPlanes.transform;

        }
    }

    GameObject CreatePlane(Vector3 centerPoint, float uvOffset)
    {
        GameObject tempPlane = new GameObject();
        MeshFilter meshFilter = tempPlane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = tempPlane.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh(); //every single plane is its own game object so they're sorted properly back to frton

        Vector3[] vertices = new Vector3[4];
        List<Vector3> uvs = new List<Vector3>();
        int[] indices = new int[4];

        Vector3 halfDimensions = dimensions/2;

        Vector3 offsetLowerLeft = new Vector3(0.0f, -halfDimensions.y, halfDimensions.z);
        Vector3 offsetUpperLeft = new Vector3(0.0f, halfDimensions.y, halfDimensions.z);
        Vector3 offsetLowerRight = new Vector3(0.0f, halfDimensions.y, -halfDimensions.z);
        Vector3 offsetUpperRight = new Vector3(0.0f, -halfDimensions.y, -halfDimensions.z);

        Vector3 uvoffsetLowerLeft = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 uvoffsetUpperLeft = new Vector3(0.0f, 1.0f, 1.0f);
        Vector3 uvoffsetLowerRight = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 uvoffsetUpperRight = new Vector3(0.0f, 0.0f, 0.0f);

        //take points, assign them to uvs, vertices, indeces

        vertices[0] = centerPoint + offsetLowerLeft;
        uvs.Add(uvoffsetLowerLeft);
        indices[0]=0;

        vertices[1] = centerPoint + offsetUpperLeft;
        uvs.Add(uvoffsetLowerLeft);
        indices[1]=0

        vertices[2] = centerPoint + offsetUpperRight;
        uvs.Add(uvoffsetLowerLeft);
        indices[2]=0

        vertices[3] = centerPoint + offsetLowerRight;
        uvs.Add(uvoffsetLowerLeft);
        indices[3]=0;

        mesh.vertices = vertices;
        mesh.SetUVs(0,uvs);
        mesh.SetIndices(indices, MeshTopology.Quads,0);
        meshFilter.mesh = mesh;
        meshRenderer.material = material;

        return tempPlane;


    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
