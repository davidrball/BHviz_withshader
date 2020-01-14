using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class BHTex : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Material material;
    Texture3D texture3D;

    public string inputfile = "test_csv";


    private List<Dictionary<string, object>> pointList;
    // Indices for columns to be assigned
    public int columnX = 0;
    public int columnY = 1;
    public int columnZ = 2;
    public int columnT = 3;
 
    // Full column names
    public string xName;
    public string yName;
    public string zName;
    public string tName;

    public int texsize = 255;


    [SerializeField] float width = 10.0f;


    // Start is called before the first frame update
    void Start()
    {
        pointList = CSVReader.Read("test_csv");
         List<string> columnList = new List<string>(pointList[1].Keys);
    Debug.Log("There are " + columnList.Count + " columns in CSV");
    foreach (string key in columnList)
    Debug.Log("Column name is " + key);
    // Assign column name from columnList to Name variables
    xName = columnList[columnX];
    yName = columnList[columnY];
    zName = columnList[columnZ];
    tName = columnList[columnT];
    // Get maxes of each axis
    float xMax = FindMaxValue(xName);
    float yMax = FindMaxValue(yName);
    float zMax = FindMaxValue(zName);
    float tMax = FindMaxValue(tName);
    // Get minimums of each axis
    float xMin = FindMinValue(xName);
    float yMin = FindMinValue(yName);
    float zMin = FindMinValue(zName);
    float tMin = FindMinValue(tName);
    float array_size = System.Convert.ToSingle(IntCubeRoot(pointList.Count));
    //Debug.Log(IntCubeRoot(50*50*50)); // , so we can find length of our list
        
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
        material = meshRenderer.sharedMaterial;
        
        Calculate3DUVW();
        
        texture3D = new Texture3D(texsize,texsize,texsize,TextureFormat.RGBA32,false);
        texture3D.wrapMode = TextureWrapMode.Clamp;
        texture3D.filterMode = FilterMode.Bilinear;
        CreateColor(array_size, tMax);
    }

    void CreateColor(float arraysize, float tMax)
    {
        //array of colors for all the points in the cube
        Color32[] colors = new Color32[texture3D.width * texture3D.height * texture3D.depth];
     
        for(int z= 0; z< texture3D.depth;z++)
        {
            for(int y=0; y< texture3D.height;y++)
                {
                    for(int x=0; x< texture3D.width; x++)
                    {
                        //Color32 temp = GetColor(x,y,z, arraysize, tMax);
                        
                        int index = x+y*texture3D.width + z*texture3D.width*texture3D.height;

                        //just for testing
                        byte bytex = System.Convert.ToByte(x);
                        //byte bytex=255;
                        
                        
                        Color32 test_tmp = new Color32(bytex,0,0,255);
                        
                        //if (x>127)
                        //{
                         //   test_tmp.b=255;
                         //   test_tmp.r=255;
                         //   test_tmp.g=255;
                        //}

                        //colors[index] = temp;
                        colors[index] = test_tmp;
                    }
                }
        }
    texture3D.SetPixels32(colors);
    texture3D.Apply();
    material.SetTexture("_MainTex",texture3D);
    }

    void CreateColor3D(float arraysize, float tMax)
    {
        //create 3D array of colors w/ the desired dims in array
        Color32[,,] colors3D = new Color32[texture3D.width, texture3D.height, texture3D.depth];
        Color32[] colors = new Color32[texture3D.width * texture3D.height * texture3D.depth];
        for(int z= 0; z< texture3D.depth;z++)
        {
            for(int y=0; y< texture3D.height;y++)
                {
                    for(int x=0; x< texture3D.width; x++)
                    {
                       colors3D[x,y,z] = GetColor(x,y,z,arraysize,tMax);
                    }
                }
        }



    texture3D.SetPixels32(colors);
    texture3D.Apply();
    material.SetTexture("_MainTex",texture3D);

    }


    Color32 GetColor(int x, int y, int z, float arraysize, float tMax)
    {   
        /*For new stuff, first we should map x,y,z given in the texture
        coords (0-255) to the indices given from data
         remember, texsize is how many elements in texture we have,

         pointlist.count is total number of elements in pointlist,
         cuberoot of this will give length per side

         */
        float floatx = (float)x;
        float floaty = (float)y;
        float floatz = (float)z;

        //x,y,z go from 0-texsize
        //defining fractional distance along each axis
        float fracx = floatx/texsize;
        float fracy = floaty/texsize;
        float fracz = floatz/texsize;

        //map fractional distance to simulation array distance
        float arr_distx = fracx*(arraysize-1f);
        float arr_disty = fracy*(arraysize-1f);
        float arr_distz = fracz*(arraysize-1f);

        int rounded_distx = (int)Math.Round(arr_distx);
        int rounded_disty = (int)Math.Round(arr_disty);
        int rounded_distz = (int)Math.Round(arr_distz);

        //loop through simulation grid, if grid location matches our rounded_distx,y,z numbers, then grab the density at this point
        //should be a faster way to do this, can prob convert rounded vals to a flattened index
        //map this to values in pointlist, assuming we're saving the data in some specified ordered way
        //what we have here is slower but more sure to be correct since we haven't carefully formatted data
        
        for (var i=0; i<pointList.Count;i++)
        {
            int grid_xval = Convert.ToInt32(pointList[i][xName]);
            int grid_yval = Convert.ToInt32(pointList[i][yName]);
            int grid_zval = Convert.ToInt32(pointList[i][zName]);

            if (grid_xval==rounded_distx && grid_yval==rounded_disty && grid_yval==rounded_disty)
            {
                float mydens=System.Convert.ToSingle(pointList[i][tName]);
                //now map mydens to 0-255, and then into byte format for color
                float fracdens = mydens/tMax;
                float scaled_fracdens = fracdens*255;
                byte bytedens = System.Convert.ToByte(scaled_fracdens);

                return new Color32(bytedens, bytedens, bytedens, 255);
            }

        }

        //now we should have the grid indices to grab


        //ok, now turn these into grid indices


        //float dist = (floatx*floatx + floaty*floaty + floatz*floatz)/(texture3D.width*texture3D.width + texture3D.height*texture3D.height+texture3D.depth*texture3D.depth); // just square of distance from 0,0
        //dist = dist * 255f;
        //float[] mydist = {dist};
        //Debug.Log(mydist);
        //byte[] mynum = System.BitConverter.GetBytes(mydist);
        //return new Color32(mynum[0], mynum[0], mynum[0], 255);
        //Debug.Log(dist);


        //byte bytedist = System.Convert.ToByte(dist);
        //return new Color32(bytedist,bytedist,bytedist,255);
        return new Color32(0,0,0,255);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private float FindMaxValue(string columnName)
    {
        //set initial value to first value
        float maxValue = Convert.ToSingle(pointList[0][columnName]);
 
        //Loop through Dictionary, overwrite existing maxValue if new value is larger
        for (var i = 0; i < pointList.Count; i++)
        {
            if (maxValue < Convert.ToSingle(pointList[i][columnName]))
                maxValue = Convert.ToSingle(pointList[i][columnName]);
        }
 
        //Spit out the max value
        return maxValue;
    }
 
    private float FindMinValue(string columnName)
    {
 
        float minValue = Convert.ToSingle(pointList[0][columnName]);
 
        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < pointList.Count; i++)
        {
            if (Convert.ToSingle(pointList[i][columnName]) < minValue)
                minValue = Convert.ToSingle(pointList[i][columnName]);
        }
 
        return minValue;
    }

     private double IntCubeRoot(int inputnum)
    {
        double number, result;
    
        number = System.Convert.ToDouble(inputnum);
        result = Math.Ceiling(Math.Pow(number, (double)1 / 3));
        return result;
    }

    void Calculate3DUVW()
    {
        List<Vector3> newVertices = new List<Vector3>(meshFilter.sharedMesh.vertices);
        //create copy of vertex data we have, store it in newvertices

        for(int i=0; i< newVertices.Count;i++)
        {
            newVertices[i]=transform.TransformPoint(newVertices[i]);
        }
    meshFilter.sharedMesh.SetUVs(0,newVertices);
    }
    

}
