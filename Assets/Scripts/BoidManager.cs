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
    float MaxSpeed = 6f;
    [SerializeField]
    float MinSpeed = 2f;
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

    public BoidData[] Boids; 
    ComputeBuffer BoidInputBuffer;
    ComputeBuffer BoidOutputBuffer;

    uint[] Args;
    ComputeBuffer ArgsBuffer;
    Mesh Mesh;

    int PrevBoidCount;

    void Start()
    {
        Mesh = MeshBuilder.CreateRectangle(Vector3.zero, 1f, 1f);
        KernelIndex = MainBoidShader.FindKernel("CSMain");

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
            BoidInputBuffer.SetData(Boids);
            MainBoidShader.SetFloat("DeltaTime", Time.deltaTime);
            MainBoidShader.SetFloat("MaxSpeed", MaxSpeed);
            MainBoidShader.SetFloat("MinSpeed", MinSpeed);
            MainBoidShader.SetFloat("MaxSteerForce", MaxSteerForce);
            MainBoidShader.SetFloat("SqrCollisionAvoidanceDistance", CollisionAvoidanceDistance * CollisionAvoidanceDistance);
            MainBoidShader.SetFloat("SqrFlockViewDistance", FlockViewDistance * FlockViewDistance);
            MainBoidShader.SetFloat("CollisionAvoidanceWeight", CollisionAvoidanceWeight);
            MainBoidShader.SetFloat("FlockCenteringWeight", FlockCenteringWeight);
            MainBoidShader.SetFloat("AlignmentWeight", AlignmentWeight);

            int Groups = Mathf.CeilToInt(Boids.Length / (float)GroupSize);
            MainBoidShader.Dispatch(KernelIndex, Groups, 1, 1);

            //we should be doign somethign while gpu prcosses compute, move rendering here?

            BoidOutputBuffer.GetData(Boids);

            Graphics.DrawMeshInstancedIndirect(Mesh, 0, BoidMaterial, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), ArgsBuffer);
        }
    }
    
    void GenerateBoids()
    {
        Boids = new BoidData[BoidSimCount];

        for (int i = 0; i < BoidSimCount; i++)
        {
            Boids[i] = new BoidData(new Vector3(Random.Range(-SpawnWidth / 2f, SpawnWidth / 2f), Random.Range(-SpawnHeight / 2f, SpawnHeight / 2f), 0f), new Vector3(Random.Range(-SpawnMaxSpeed, SpawnMaxSpeed), Random.Range(-SpawnMaxSpeed, SpawnMaxSpeed), 0f));
        }

        if (BoidInputBuffer != null) BoidInputBuffer.Release();
        if (BoidOutputBuffer != null) BoidOutputBuffer.Release();
        BoidInputBuffer = new ComputeBuffer(Boids.Length, BoidData.Size());
        BoidOutputBuffer = new ComputeBuffer(Boids.Length, BoidData.Size());
        MainBoidShader.SetInt("BoidCount", Boids.Length);
        
        MainBoidShader.SetBuffer(0, "InputBoids", BoidInputBuffer);
        MainBoidShader.SetBuffer(0, "OutputBoids", BoidOutputBuffer);
        BoidMaterial.SetBuffer("BoidBuffer", BoidOutputBuffer);      

        //https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
        if (ArgsBuffer != null) ArgsBuffer.Release();
        ArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        Args[1] = (uint)BoidSimCount;
        ArgsBuffer.SetData(Args);

        PrevBoidCount = BoidSimCount;
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