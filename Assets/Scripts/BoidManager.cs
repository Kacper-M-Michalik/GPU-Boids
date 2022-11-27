using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField]
    public ComputeShader MainBoidShader;    
   
    [SerializeField]
    public Material BoidMaterial;

    [Space]
    [SerializeField]
    int BoidSimCount = 300;
    [SerializeField]
    float SpawnWidth = 60f;
    [SerializeField]
    float SpawnHeight = 60f;
    [SerializeField]
    float SpawnMaxSpeed = 3.5f;
    [Space]
    [SerializeField]
    float FlockViewDistance = 5f;
    [SerializeField]
    float CollisionAvoidanceDistance = 2f;
    [SerializeField]
    float MinSpeed = 2f;
    [SerializeField]
    float MaxSpeed = 6f;    
    [SerializeField]
    float MaxSteerForce = 3f;
    [Space]
    [SerializeField]
    float CollisionAvoidanceWeight = 1f;
    [SerializeField]
    float FlockCenteringWeight = 1f;
    [SerializeField]
    float AlignmentWeight = 1f;
    //add angle variable

    const int GroupSize = 64;
    int KernelIndex;
    ShaderParam Param;

    [SerializeField]
    bool ExtractData;
    public BoidData[] Boids; 
    ComputeBuffer BoidBuffer1;
    ComputeBuffer BoidBuffer2;
    bool IsBuffer1Input;

    uint[] Args;
    ComputeBuffer ArgsBuffer;
    Mesh Mesh;

    int PrevBoidCount;

    void Start()
    {
        Mesh = MeshBuilder.CreateRectangle(Vector3.zero, 1f, 1f);
        KernelIndex = MainBoidShader.FindKernel("CSMain");
        Param = new ShaderParam();

        Args = new uint[5]; 
        Args[0] = (uint)Mesh.GetIndexCount(0);
        Args[2] = (uint)Mesh.GetIndexStart(0);
        Args[3] = (uint)Mesh.GetBaseVertex(0);
        Args[4] = 0;
        GenerateBoids();
    }

    void Update()
    {
        if (BoidSimCount != PrevBoidCount)
        {
            GenerateBoids();
        }

        if (Boids != null)
        {
            MainBoidShader.SetFloat(Param.DeltaTimeID, Time.deltaTime);
            MainBoidShader.SetFloat(Param.MinSpeedID, MinSpeed);
            MainBoidShader.SetFloat(Param.MaxSpeedID, MaxSpeed);
            MainBoidShader.SetFloat(Param.MaxSteerForceID, MaxSteerForce);
            MainBoidShader.SetFloat(Param.SqrCollisionAvoidanceDistanceID, CollisionAvoidanceDistance * CollisionAvoidanceDistance);
            MainBoidShader.SetFloat(Param.SqrFlockViewDistanceID, FlockViewDistance * FlockViewDistance);
            MainBoidShader.SetFloat(Param.CollisionAvoidanceWeightID, CollisionAvoidanceWeight);
            MainBoidShader.SetFloat(Param.FlockCenteringWeightID, FlockCenteringWeight);
            MainBoidShader.SetFloat(Param.AlignmentWeightID, AlignmentWeight);

            int Groups = Mathf.CeilToInt(Boids.Length / (float)GroupSize);
            MainBoidShader.Dispatch(KernelIndex, Groups, 1, 1);

            if (IsBuffer1Input)
            {
                MainBoidShader.SetBuffer(0, "InputBoids", BoidBuffer2);
                MainBoidShader.SetBuffer(0, "OutputBoids", BoidBuffer1);
                BoidMaterial.SetBuffer("BoidBuffer", BoidBuffer2);
                IsBuffer1Input = false;
            }
            else
            {
                MainBoidShader.SetBuffer(0, "InputBoids", BoidBuffer1);
                MainBoidShader.SetBuffer(0, "OutputBoids", BoidBuffer2);
                BoidMaterial.SetBuffer("BoidBuffer", BoidBuffer1);
                IsBuffer1Input = true;
            }
            
            Graphics.DrawMeshInstancedIndirect(Mesh, 0, BoidMaterial, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), ArgsBuffer);     
            
            if (ExtractData)
            {
                if (!IsBuffer1Input) BoidBuffer2.GetData(Boids);
                else BoidBuffer1.GetData(Boids);
            }
        }       
    }
    
    void GenerateBoids()
    {
        Boids = new BoidData[BoidSimCount];

        for (int i = 0; i < BoidSimCount; i++)
        {
            Boids[i] = new BoidData(new Vector3(Random.Range(-SpawnWidth / 2f, SpawnWidth / 2f), Random.Range(-SpawnHeight / 2f, SpawnHeight / 2f), 0f), new Vector3(Random.Range(-SpawnMaxSpeed, SpawnMaxSpeed), Random.Range(-SpawnMaxSpeed, SpawnMaxSpeed), 0f));
        }

        if (BoidBuffer1 != null) BoidBuffer1.Release();
        if (BoidBuffer2 != null) BoidBuffer2.Release();
        BoidBuffer1 = new ComputeBuffer(Boids.Length, BoidData.Size());
        BoidBuffer2 = new ComputeBuffer(Boids.Length, BoidData.Size());

        BoidBuffer1.SetData(Boids);
        IsBuffer1Input = true;

        MainBoidShader.SetInt("BoidCount", Boids.Length);        
        MainBoidShader.SetBuffer(0, "InputBoids", BoidBuffer1);
        MainBoidShader.SetBuffer(0, "OutputBoids", BoidBuffer2);

        //https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
        if (ArgsBuffer != null) ArgsBuffer.Release();
        ArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        Args[1] = (uint)BoidSimCount;
        ArgsBuffer.SetData(Args);

        PrevBoidCount = BoidSimCount;
    }

}

public class ShaderParam
{
    public int DeltaTimeID;
    public int MinSpeedID;
    public int MaxSpeedID;
    public int MaxSteerForceID;
    public int SqrCollisionAvoidanceDistanceID;
    public int SqrFlockViewDistanceID;
    public int CollisionAvoidanceWeightID;
    public int FlockCenteringWeightID;
    public int AlignmentWeightID;

    public ShaderParam()
    {
        DeltaTimeID = Shader.PropertyToID("DeltaTime");
        MinSpeedID = Shader.PropertyToID("MinSpeed");
        MaxSpeedID = Shader.PropertyToID("MaxSpeed");
        MaxSteerForceID = Shader.PropertyToID("MaxSteerForce");
        SqrCollisionAvoidanceDistanceID = Shader.PropertyToID("SqrCollisionAvoidanceDistance");
        SqrFlockViewDistanceID = Shader.PropertyToID("SqrFlockViewDistance");
        CollisionAvoidanceWeightID = Shader.PropertyToID("CollisionAvoidanceWeight");
        FlockCenteringWeightID = Shader.PropertyToID("FlockCenteringWeight");
        AlignmentWeightID = Shader.PropertyToID("AlignmentWeight");
    }
}

public struct BoidData
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector2 Padding;

    public BoidData(Vector3 SetPosition, Vector3 SetVelocity)
    {
        Position = SetPosition;
        Velocity = SetVelocity;
        Padding = Vector2.zero;
    }

    public static int Size()
    {
        return sizeof(float) * 8;
    }
}

public struct ObstacleData
{
    public Vector2 TopLeftBound;
    public Vector2 BottomRightBound;

    public ObstacleData(Vector2 SeTopLeftBound, Vector2 SetBottomRightBound)
    {
        TopLeftBound = SeTopLeftBound;
        BottomRightBound = SetBottomRightBound;
    }

    public static int Size()
    {
        return sizeof(float) * 4;
    }
}