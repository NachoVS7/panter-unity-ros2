using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using NWH.WheelController3D;

public class WheelTorqueCommandSubscriber : MonoBehaviour
{
    [Header("ROS")]
    public string topicName = "/panter/wheel_torque_cmd";

    [Header("Vehicle")]
    public CarController carController;

    [Header("Scaling")]
    public float maxTorque = 600f;

    [Header("Safety")]
    public float commandTimeout = 0.5f;

    [Header("Received Torque Commands")]
    public float frontLeftTorque;
    public float frontRightTorque;
    public float rearLeftTorque;
    public float rearRightTorque;

    private ROSConnection ros;
    private float lastCommandTime = -999f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Float32MultiArrayMsg>(topicName, ReceiveWheelTorqueCommand);

        if (carController == null)
        {
            Debug.LogError("WheelTorqueCommandSubscriber: falta asignar CarController.");
            enabled = false;
            return;
        }

        Debug.Log("WheelTorqueCommandSubscriber suscrito a " + topicName);
    }

    void ReceiveWheelTorqueCommand(Float32MultiArrayMsg msg)
    {
        if (msg.data.Length < 4)
        {
            Debug.LogWarning("Mensaje recibido en " + topicName + " con menos de 4 valores.");
            return;
        }

        frontLeftTorque = Mathf.Clamp(msg.data[0], -maxTorque, maxTorque);
        frontRightTorque = Mathf.Clamp(msg.data[1], -maxTorque, maxTorque);
        rearLeftTorque = Mathf.Clamp(msg.data[2], -maxTorque, maxTorque);
        rearRightTorque = Mathf.Clamp(msg.data[3], -maxTorque, maxTorque);

        carController.useExternalWheelTorqueInput = true;

        carController.externalFrontLeftTorque = frontLeftTorque;
        carController.externalFrontRightTorque = frontRightTorque;
        carController.externalRearLeftTorque = rearLeftTorque;
        carController.externalRearRightTorque = rearRightTorque;

        lastCommandTime = Time.time;

        Debug.Log(
            "Wheel torque cmd recibido: " +
            "FL=" + frontLeftTorque.ToString("F1") + " | " +
            "FR=" + frontRightTorque.ToString("F1") + " | " +
            "RL=" + rearLeftTorque.ToString("F1") + " | " +
            "RR=" + rearRightTorque.ToString("F1")
        );
    }

    void Update()
    {
        if (carController == null)
        {
            return;
        }

        if (Time.time - lastCommandTime > commandTimeout)
        {
            frontLeftTorque = 0f;
            frontRightTorque = 0f;
            rearLeftTorque = 0f;
            rearRightTorque = 0f;

            carController.externalFrontLeftTorque = 0f;
            carController.externalFrontRightTorque = 0f;
            carController.externalRearLeftTorque = 0f;
            carController.externalRearRightTorque = 0f;

            carController.useExternalWheelTorqueInput = false;
        }
    }
}
