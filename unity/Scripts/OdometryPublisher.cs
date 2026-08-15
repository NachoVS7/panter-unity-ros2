using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using System;

public class OdometryPublisher : MonoBehaviour
{
    ROSConnection ros;
    public GameObject fixpositionObject;
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public float publishFrequency = 10.0f;
    private float timeElapsed;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OdometryMsg>("/fixposition/odometry");

        lastPosition = fixpositionObject.transform.position;
        lastRotation = fixpositionObject.transform.rotation;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 1.0f / publishFrequency)
        {
            PublishOdometryData();
            timeElapsed = 0;
        }
    }

    void PublishOdometryData()
    {
        Vector3 currentPosition = fixpositionObject.transform.position;
        Quaternion currentRotation = fixpositionObject.transform.rotation;

        OdometryMsg odometryMessage = new OdometryMsg
        {
            header = new HeaderMsg
            {
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    nanosec = (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000 * 1000000)
                },
                frame_id = "fix_position"
            },
            child_frame_id = "ODOM",
            pose = new PoseWithCovarianceMsg
            {
                pose = new PoseMsg
                {
                    position = new PointMsg
                    {
                        x = currentPosition.x,
                        y = currentPosition.y,
                        z = currentPosition.z
                    },
                    orientation = new QuaternionMsg
                    {
                        x = currentRotation.x,
                        y = currentRotation.y,
                        z = currentRotation.z,
                        w = currentRotation.w
                    }
                },
                covariance = new double[36]
            }
        };

        ros.Publish("/fixposition/odometry", odometryMessage);

        lastPosition = currentPosition;
        lastRotation = currentRotation;

        Debug.Log("Publicado odometría.");
    }
}
