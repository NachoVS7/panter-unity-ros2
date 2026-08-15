import rclpy
from rclpy.node import Node
from geometry_msgs.msg import Twist
from std_msgs.msg import Float32MultiArray

from panter_control.traction_curve import get_max_wheel_torque


class PanterSkidMapper(Node):
    def __init__(self):
        super().__init__('panter_skid_mapper')

        self.declare_parameter('max_linear_speed', 2.0)
        self.declare_parameter('max_angular_speed', 1.0)
        self.declare_parameter('wheel_radius', 0.3302)
        self.declare_parameter('powered_wheel_count', 4)

        self.max_linear_speed = self.get_parameter('max_linear_speed').value
        self.max_angular_speed = self.get_parameter('max_angular_speed').value
        self.wheel_radius = self.get_parameter('wheel_radius').value
        self.powered_wheel_count = self.get_parameter('powered_wheel_count').value

        self.current_wheel_speeds = [0.0, 0.0, 0.0, 0.0]

        self.cmd_sub = self.create_subscription(
            Twist,
            '/cmd_vel',
            self.cmd_vel_callback,
            10
        )

        self.wheel_states_sub = self.create_subscription(
            Float32MultiArray,
            '/panter/wheel_states',
            self.wheel_states_callback,
            10
        )

        self.torque_pub = self.create_publisher(
            Float32MultiArray,
            '/panter/wheel_torque_cmd',
            10
        )

        self.get_logger().info('panter_skid_mapper iniciado con curva de tracción.')
        self.get_logger().info('Suscrito a /cmd_vel')
        self.get_logger().info('Suscrito a /panter/wheel_states')
        self.get_logger().info('Publicando en /panter/wheel_torque_cmd')

    def clamp(self, value, min_value, max_value):
        return max(min(value, max_value), min_value)

    def wheel_states_callback(self, msg):
        if len(msg.data) < 4:
            return

        self.current_wheel_speeds = [
            float(msg.data[0]),
            float(msg.data[1]),
            float(msg.data[2]),
            float(msg.data[3])
        ]

    def estimate_vehicle_speed_kmh(self):
        avg_wheel_speed_rad_s = sum(abs(w) for w in self.current_wheel_speeds) / 4.0
        speed_mps = avg_wheel_speed_rad_s * self.wheel_radius
        speed_kmh = speed_mps * 3.6
        return speed_kmh

    def cmd_vel_callback(self, msg):
        linear = self.clamp(
            msg.linear.x,
            -self.max_linear_speed,
            self.max_linear_speed
        )

        angular = self.clamp(
            msg.angular.z,
            -self.max_angular_speed,
            self.max_angular_speed
        )

        throttle = linear / self.max_linear_speed
        turn = angular / self.max_angular_speed

        left_command = throttle - turn
        right_command = throttle + turn

        left_command = self.clamp(left_command, -1.0, 1.0)
        right_command = self.clamp(right_command, -1.0, 1.0)

        vehicle_speed_kmh = self.estimate_vehicle_speed_kmh()

        max_wheel_torque = get_max_wheel_torque(
            vehicle_speed_kmh,
            wheel_radius=self.wheel_radius,
            powered_wheel_count=self.powered_wheel_count
        )

        left_torque = left_command * max_wheel_torque
        right_torque = right_command * max_wheel_torque

        torque_msg = Float32MultiArray()
        torque_msg.data = [
            float(left_torque),   # FL
            float(right_torque),  # FR
            float(left_torque),   # RL
            float(right_torque),  # RR
        ]

        self.torque_pub.publish(torque_msg)


def main(args=None):
    rclpy.init(args=args)
    node = PanterSkidMapper()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()


if __name__ == '__main__':
    main()
