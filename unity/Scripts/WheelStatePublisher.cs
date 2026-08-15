using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class WheelStatePublisher : MonoBehaviour
{
    private ROSConnection ros;

    [Header("ROS")]
    public string topicName = "/panter/wheel_states";
    public float publishFrequency = 20f;

    [Header("Wheel References")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    private Quaternion lastFLRotation;
    private Quaternion lastFRRotation;
    private Quaternion lastRLRotation;
    private Quaternion lastRRRotation;

    private float timeElapsed = 0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32MultiArrayMsg>(topicName);

        if (frontLeftWheel != null) lastFLRotation = frontLeftWheel.rotation;
        if (frontRightWheel != null) lastFRRotation = frontRightWheel.rotation;
        if (rearLeftWheel != null) lastRLRotation = rearLeftWheel.rotation;
        if (rearRightWheel != null) lastRRRotation = rearRightWheel.rotation;

        Debug.Log("WheelStatePublisher publicando en " + topicName);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= 1.0f / publishFrequency)
        {
            PublishWheelStates(timeElapsed);
            timeElapsed = 0f;
        }
    }

    float CalculateAngularVelocity(Transform wheel, Quaternion lastRotation)
    {
        if (wheel == null)
        {
            return 0f;
        }

        Quaternion deltaRotation = wheel.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleDeg, out Vector3 axis);

        if (angleDeg > 180f)
        {
            angleDeg -= 360f;
        }

        float sign = Mathf.Sign(Vector3.Dot(axis, wheel.right));
        float angularVelocityRadS = sign * angleDeg * Mathf.Deg2Rad / timeElapsed;

        return angularVelocityRadS;
    }

    void PublishWheelStates(float dt)
    {
        float flSpeed = CalculateAngularVelocity(frontLeftWheel, lastFLRotation);
        float frSpeed = CalculateAngularVelocity(frontRightWheel, lastFRRotation);
        float rlSpeed = CalculateAngularVelocity(rearLeftWheel, lastRLRotation);
        float rrSpeed = CalculateAngularVelocity(rearRightWheel, lastRRRotation);

        Float32MultiArrayMsg msg = new Float32MultiArrayMsg
        {
            data = new float[]
            {
                flSpeed,
                frSpeed,
                rlSpeed,
                rrSpeed
            }
        };

        ros.Publish(topicName, msg);

        if (frontLeftWheel != null) lastFLRotation = frontLeftWheel.rotation;
        if (frontRightWheel != null) lastFRRotation = frontRightWheel.rotation;
        if (rearLeftWheel != null) lastRLRotation = rearLeftWheel.rotation;
        if (rearRightWheel != null) lastRRRotation = rearRightWheel.rotation;
    }
}
