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

    // Start is called before the first frame update
    void Start()
    {
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
        FramerateText.text = Mathf.Round(1f / Time.deltaTime).ToString()+" FPS";
    }
}
