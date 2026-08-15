import rclpy
from rclpy.node import Node
from geometry_msgs.msg import Twist
from std_msgs.msg import Float32MultiArray


class PanterSkidVelocityMapper(Node):
    def __init__(self):
        super().__init__('panter_skid_velocity_mapper')

        self.declare_parameter('wheel_radius', 0.3302)
        self.declare_parameter('track_width', 1.40)
        self.declare_parameter('max_linear_speed', 20.0)
        self.declare_parameter('max_angular_speed', 10.0)

        self.wheel_radius = self.get_parameter('wheel_radius').value
        self.track_width = self.get_parameter('track_width').value
        self.max_linear_speed = self.get_parameter('max_linear_speed').value
        self.max_angular_speed = self.get_parameter('max_angular_speed').value

        self.cmd_sub = self.create_subscription(
            Twist,
            '/cmd_vel',
            self.cmd_vel_callback,
            10
        )

        self.velocity_pub = self.create_publisher(
            Float32MultiArray,
            '/panter/wheel_velocity_cmd',
            10
        )

        self.get_logger().info('panter_skid_velocity_mapper iniciado.')
        self.get_logger().info('Suscrito a /cmd_vel')
        self.get_logger().info('Publicando en /panter/wheel_velocity_cmd')

    def clamp(self, value, min_value, max_value):
        return max(min(value, max_value), min_value)

    def cmd_vel_callback(self, msg):
        v = self.clamp(
            msg.linear.x,
            -self.max_linear_speed,
            self.max_linear_speed
        )

        wz = self.clamp(
            msg.angular.z,
            -self.max_angular_speed,
            self.max_angular_speed
        )

        half_track = self.track_width / 2.0

        v_left = v - wz * half_track
        v_right = v + wz * half_track

        omega_left = v_left / self.wheel_radius
        omega_right = v_right / self.wheel_radius

        wheel_msg = Float32MultiArray()
        wheel_msg.data = [
            float(omega_left),   # FL
            float(omega_right),  # FR
            float(omega_left),   # RL
            float(omega_right),  # RR
        ]

        self.velocity_pub.publish(wheel_msg)


def main(args=None):
    rclpy.init(args=args)
    node = PanterSkidVelocityMapper()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()


if __name__ == '__main__':
    main()
