using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    Camera Cam;
    [SerializeField]
    BoidManager Manager;
    [SerializeField]
    bool Follow;
    [SerializeField]
    int FollowBoidIndex;
    [SerializeField]
    Text FramerateText;

    List<float> Framerates;
    [SerializeField]
    int FramesCounted = 10;

    // Start is called before the first frame update
    void Start()
    {
        Framerates = new List<float>();
        if (Cam == null) Cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Follow)
        {
            if (FollowBoidIndex < Manager.Boids.Length)
            {
                Cam.transform.position = new Vector3(Manager.Boids[FollowBoidIndex].Position.x, Manager.Boids[FollowBoidIndex].Position.y, Cam.transform.position.z);
            }
        }
        Framerates.Add(1 / Time.deltaTime);

        if (Framerates.Count > FramesCounted) Framerates.RemoveAt(0);

        float FinalFramerate = 0f;
        for (int i = 0; i < Framerates.Count; i++)
        {
            FinalFramerate += Framerates[i];
        }
        FinalFramerate /= Framerates.Count;

        FramerateText.text = Mathf.Round(FinalFramerate).ToString()+" FPS";
    }
}
