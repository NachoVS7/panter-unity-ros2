using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using NWH.WheelController3D;
using System;
using System.Reflection;

public class WheelLoadPublisher : MonoBehaviour
{
    [Header("ROS")]
    public string loadTopicName = "/panter/wheel_loads";
    public string massTopicName = "/panter/wheel_masses_equivalent";
    public string distributionTopicName = "/panter/wheel_load_distribution";
    public float publishFrequency = 20f;

    [Header("Publish Options")]
    public bool publishLoadsN = true;
    public bool publishMassEquivalentKg = true;
    public bool publishDistribution = true;

    [Header("Wheel Controllers")]
    public WheelController frontLeftWheel;
    public WheelController frontRightWheel;
    public WheelController rearLeftWheel;
    public WheelController rearRightWheel;

    [Header("Debug Loads [N]")]
    public float frontLeftLoad;
    public float frontRightLoad;
    public float rearLeftLoad;
    public float rearRightLoad;
    public float totalLoad;

    [Header("Debug Equivalent Mass [kg]")]
    public float frontLeftMass;
    public float frontRightMass;
    public float rearLeftMass;
    public float rearRightMass;
    public float totalMassEquivalent;

    private ROSConnection ros;
    private float timer = 0f;
    private float publishPeriod;
    private const float gravity = 9.81f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        publishPeriod = 1f / publishFrequency;

        if (publishLoadsN)
        {
            ros.RegisterPublisher<Float32MultiArrayMsg>(loadTopicName);
        }

        if (publishMassEquivalentKg)
        {
            ros.RegisterPublisher<Float32MultiArrayMsg>(massTopicName);
        }

        if (publishDistribution)
        {
            ros.RegisterPublisher<Float32MultiArrayMsg>(distributionTopicName);
        }

        Debug.Log("WheelLoadPublisher iniciado.");
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= publishPeriod)
        {
            timer = 0f;
            PublishWheelLoads();
        }
    }

    void PublishWheelLoads()
    {
        frontLeftLoad = ReadWheelLoad(frontLeftWheel);
        frontRightLoad = ReadWheelLoad(frontRightWheel);
        rearLeftLoad = ReadWheelLoad(rearLeftWheel);
        rearRightLoad = ReadWheelLoad(rearRightWheel);

        totalLoad = frontLeftLoad + frontRightLoad + rearLeftLoad + rearRightLoad;

        frontLeftMass = frontLeftLoad / gravity;
        frontRightMass = frontRightLoad / gravity;
        rearLeftMass = rearLeftLoad / gravity;
        rearRightMass = rearRightLoad / gravity;

        totalMassEquivalent = totalLoad / gravity;

        if (publishLoadsN)
        {
            Float32MultiArrayMsg loadMsg = new Float32MultiArrayMsg
            {
                data = new float[]
                {
                    frontLeftLoad,
                    frontRightLoad,
                    rearLeftLoad,
                    rearRightLoad
                }
            };

            ros.Publish(loadTopicName, loadMsg);
        }

        if (publishMassEquivalentKg)
        {
            Float32MultiArrayMsg massMsg = new Float32MultiArrayMsg
            {
                data = new float[]
                {
                    frontLeftMass,
                    frontRightMass,
                    rearLeftMass,
                    rearRightMass
                }
            };

            ros.Publish(massTopicName, massMsg);
        }

        if (publishDistribution)
        {
            float fl = 0f;
            float fr = 0f;
            float rl = 0f;
            float rr = 0f;

            if (totalLoad > 0.001f)
            {
                fl = frontLeftLoad / totalLoad;
                fr = frontRightLoad / totalLoad;
                rl = rearLeftLoad / totalLoad;
                rr = rearRightLoad / totalLoad;
            }

            Float32MultiArrayMsg distributionMsg = new Float32MultiArrayMsg
            {
                data = new float[]
                {
                    fl,
                    fr,
                    rl,
                    rr
                }
            };

            ros.Publish(distributionTopicName, distributionMsg);
        }
    }

    float ReadWheelLoad(WheelController wheelController)
    {
        if (wheelController == null)
        {
            return 0f;
        }

        Type type = wheelController.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        PropertyInfo loadProperty = type.GetProperty("Load", flags);
        if (loadProperty != null && loadProperty.GetIndexParameters().Length == 0)
        {
            object value = loadProperty.GetValue(wheelController, null);
            return ConvertToFloat(value);
        }

        FieldInfo loadField = type.GetField("load", flags);
        if (loadField != null)
        {
            object value = loadField.GetValue(wheelController);
            return ConvertToFloat(value);
        }

        Debug.LogWarning("No se ha podido leer Load/load en " + wheelController.name);
        return 0f;
    }

    float ConvertToFloat(object value)
    {
        if (value == null)
        {
            return 0f;
        }

        if (value is float)
        {
            return (float)value;
        }

        if (value is double)
        {
            return (float)(double)value;
        }

        if (value is int)
        {
            return (int)value;
        }

        try
        {
            return Convert.ToSingle(value);
        }
        catch
        {
            return 0f;
        }
    }
}
