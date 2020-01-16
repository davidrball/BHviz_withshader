using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class BHTex_xslice : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Texture3D texture3D;


    public Texture3D Texture3D
    {
        get{
            return texture3D;
        }
    }
    string inputfile = "3D_arr50_M50_nohead_nonan";

    public delegate void BHTexCalcComplete();
    public static event BHTexCalcComplete OnBHTexCalcComplete;

    private List<Dictionary<string, object>> pointList;
    // Indices for columns to be assigned
    int columnX = 0;
    int columnY = 2;
    int columnZ = 1;
    int columnT = 4;
 
    // Full column names
    public string xName;
    public string yName;
    public string zName;
    public string tName;

    int texsize = 255;


    [SerializeField] float width = 10.0f;


    // Start is called before the first frame update
    void Start()
    {
        pointList = CSVReader.Read(inputfile);
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


    //just checking t vals
    //for (int i=0; i<pointList.Count;i++)
    //{
    //    Debug.Log(pointList[i][tName]);
    //    Debug.Log("\n");
    //}


    //Debug.Log(pointList[])
    float array_size = System.Convert.ToSingle(IntCubeRoot(pointList.Count));
    //Debug.Log(IntCubeRoot(50*50*50)); // , so we can find length of our list
        
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
        Calculate3DUVW();
        
        texture3D = new Texture3D(texsize,texsize,texsize,TextureFormat.RGBA32,false);
        texture3D.wrapMode = TextureWrapMode.Clamp;
        texture3D.filterMode = FilterMode.Bilinear;
        CreateColor(array_size, tMax, tMin);
    }


    public void GenerateColor()
    {
        StartCoroutine("CreateColor");
    }
    IEnumerator CreateColor(float arraysize, float tMax, float tMin)
    {
        //array of colors for all the points in the cube
        Color32[] colors = new Color32[texture3D.width * texture3D.height * texture3D.depth];
     
        for(int z= 0; z< texture3D.depth;z++)
        {
            for(int y=0; y< texture3D.height;y++)
                {
                    for(int x=0; x< texture3D.width; x++)
                    {
                        Color32 temp = GetColor(x,y,z, arraysize, tMax, tMin);
                        
                        int index = x+y*texture3D.width + z*texture3D.width*texture3D.height;

                        //just for testing
                        //byte bytex = System.Convert.ToByte(x);
                        //byte bytex=255;
                        
                        
                        //Color32 test_tmp = new Color32(bytex,0,0,255);
                        
                        //if (x>127)
                        //{
                         //   test_tmp.b=255;
                         //   test_tmp.r=255;
                         //   test_tmp.g=255;
                        //}

                        colors[index] = temp;
                        //colors[index] = test_tmp;
                    }
                }
        }
    texture3D.SetPixels32(colors);
    texture3D.Apply();
    


    if(OnBHTexCalcComplete != null)
    {
        OnBHTexCalcComplete();
    }
    }



    Color32 GetColor(int x, int y, int z, float arraysize, float tMax,float tMin)
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
        float fracx = floatx/texsize; //these are only going from 0 to .9, not 0 to 1 (for texsize=10)
        float fracy = floaty/texsize;
        float fracz = floatz/texsize;

        //map fractional distance to simulation array distance
        float arr_distx = fracx*(arraysize-1f);
        float arr_disty = fracy*(arraysize-1f);
        float arr_distz = fracz*(arraysize-1f);

        int rounded_distx = (int)Math.Round(arr_distx);
        int rounded_disty = (int)Math.Round(arr_disty);
        int rounded_distz = (int)Math.Round(arr_distz);

        //Debug.Log(string.Format("xval: {0}, yval: {1}, zval: {2}", fracx,fracy,fracz));

        //xyz coords on the simulation grid, but still in UNITY coord frame
        

        //x stays the same
        //unity y corresponds to simulation z

        //unity z corresponds to simulation y
        
        
        //find index in pointlist that corresponds to this x,y,z val

        int intarraysize = System.Convert.ToInt32(arraysize);
        
        int index = rounded_disty + rounded_distz*intarraysize + rounded_distx*intarraysize*intarraysize;

        float mydens=System.Convert.ToSingle(pointList[index][tName]);
                //now map mydens to 0-255, and then into byte format for color
        float fracdens = (mydens-tMin)/(tMax-tMin);//map to 0-1
        float scaled_fracdens = fracdens*255;

        //applying fractional dens floor
        if (fracdens<.01f)
        {
        fracdens=.01f;
        }

        //taking log of density w/ floor applied
        float logn = Convert.ToSingle(Math.Log10(Convert.ToDouble(fracdens)));

     
        //lognMin has to match our limit sest in conditional density floor statement
        float lognMax=0;
        float lognMin=-2;

        //so divide by 6 and add 1 to get our range to where we want it?
        logn = (logn - lognMin)/(lognMax-lognMin);//rescale log to 0-1

        //rescale to 0-255
        logn = logn*255;

        byte bytedens = System.Convert.ToByte(logn);



        return new Color32(0, bytedens, 0, 255);
     
    }


    // Update is called once per frame
    void Update()
    {
         Calculate3DUVW();
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
