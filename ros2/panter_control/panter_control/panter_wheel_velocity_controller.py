import rclpy
from rclpy.node import Node
from std_msgs.msg import Float32MultiArray

from panter_control.traction_curve import get_max_wheel_torque


class PanterWheelVelocityController(Node):
    def __init__(self):
        super().__init__('panter_wheel_velocity_controller')

        self.declare_parameter('kp', 80.0)
        self.declare_parameter('max_torque_safety', 1500.0)
        self.declare_parameter('command_timeout', 0.5)
        self.declare_parameter('wheel_radius', 0.3302)
        self.declare_parameter('powered_wheel_count', 4)

        self.kp = self.get_parameter('kp').value
        self.max_torque_safety = self.get_parameter('max_torque_safety').value
        self.command_timeout = self.get_parameter('command_timeout').value
        self.wheel_radius = self.get_parameter('wheel_radius').value
        self.powered_wheel_count = self.get_parameter('powered_wheel_count').value

        self.target_velocity = [0.0, 0.0, 0.0, 0.0]
        self.current_velocity = [0.0, 0.0, 0.0, 0.0]

        self.last_cmd_time = self.get_clock().now()

        self.velocity_cmd_sub = self.create_subscription(
            Float32MultiArray,
            '/panter/wheel_velocity_cmd',
            self.velocity_cmd_callback,
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

        self.timer = self.create_timer(0.02, self.control_loop)

        self.get_logger().info('panter_wheel_velocity_controller iniciado con curva de tracción.')
        self.get_logger().info('Suscrito a /panter/wheel_velocity_cmd')
        self.get_logger().info('Suscrito a /panter/wheel_states')
        self.get_logger().info('Publicando en /panter/wheel_torque_cmd')

    def clamp(self, value, min_value, max_value):
        return max(min(value, max_value), min_value)

    def velocity_cmd_callback(self, msg):
        if len(msg.data) < 4:
            self.get_logger().warn('wheel_velocity_cmd recibido con menos de 4 valores.')
            return

        self.target_velocity = [
            float(msg.data[0]),
            float(msg.data[1]),
            float(msg.data[2]),
            float(msg.data[3]),
        ]

        self.last_cmd_time = self.get_clock().now()

    def wheel_states_callback(self, msg):
        if len(msg.data) < 4:
            self.get_logger().warn('wheel_states recibido con menos de 4 valores.')
            return

        self.current_velocity = [
            float(msg.data[0]),
            float(msg.data[1]),
            float(msg.data[2]),
            float(msg.data[3]),
        ]

    def estimate_vehicle_speed_kmh(self):
        avg_wheel_speed_rad_s = sum(abs(w) for w in self.current_velocity) / 4.0
        speed_mps = avg_wheel_speed_rad_s * self.wheel_radius
        return speed_mps * 3.6

    def control_loop(self):
        now = self.get_clock().now()
        elapsed = (now - self.last_cmd_time).nanoseconds / 1e9

        if elapsed > self.command_timeout:
            torque_cmd = [0.0, 0.0, 0.0, 0.0]
        else:
            vehicle_speed_kmh = self.estimate_vehicle_speed_kmh()

            curve_torque_limit = get_max_wheel_torque(
                vehicle_speed_kmh,
                wheel_radius=self.wheel_radius,
                powered_wheel_count=self.powered_wheel_count
            )

            torque_limit = min(curve_torque_limit, self.max_torque_safety)

            torque_cmd = []

            for target, current in zip(self.target_velocity, self.current_velocity):
                error = target - current
                torque = self.kp * error
                torque = self.clamp(torque, -torque_limit, torque_limit)
                torque_cmd.append(float(torque))

        msg = Float32MultiArray()
        msg.data = torque_cmd
        self.torque_pub.publish(msg)


def main(args=None):
    rclpy.init(args=args)
    node = PanterWheelVelocityController()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()


if __name__ == '__main__':
    main()
