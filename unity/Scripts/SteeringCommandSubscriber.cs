using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using NWH.WheelController3D;

public class SteeringCommandSubscriber : MonoBehaviour
{
    [Header("ROS")]
    public string topicName = "/panter/steering_cmd";

    [Header("Vehicle")]
    public CarController carController;

    [Header("Safety")]
    public float commandTimeout = 0.5f;

    [Header("Received Steering Command")]
    [Range(-1f, 1f)]
    public float steeringCommand = 0f;

    private ROSConnection ros;
    private float lastCommandTime = -999f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Float32Msg>(topicName, ReceiveSteeringCommand);

        if (carController == null)
        {
            Debug.LogError("SteeringCommandSubscriber: falta asignar CarController.");
            enabled = false;
            return;
        }

        Debug.Log("SteeringCommandSubscriber suscrito a " + topicName);
    }

    void ReceiveSteeringCommand(Float32Msg msg)
    {
        steeringCommand = Mathf.Clamp(msg.data, -1f, 1f);

        carController.useExternalInput = true;
        carController.externalSteerInput = -steeringCommand;

        lastCommandTime = Time.time;

        Debug.Log("Steering cmd recibido: " + steeringCommand.ToString("F2"));
    }

    void Update()
    {
        if (carController == null)
        {
            return;
        }

        if (Time.time - lastCommandTime > commandTimeout)
        {
            steeringCommand = 0f;
            carController.externalSteerInput = 0f;
            carController.useExternalInput = false;
        }
    }
}
