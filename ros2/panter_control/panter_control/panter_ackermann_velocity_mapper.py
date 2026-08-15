import math

import rclpy
from rclpy.node import Node
from geometry_msgs.msg import Twist
from std_msgs.msg import Float32MultiArray, Float32


class PanterAckermannVelocityMapper(Node):
    def __init__(self):
        super().__init__('panter_ackermann_velocity_mapper')

        self.declare_parameter('wheel_radius', 0.3302)
        self.declare_parameter('track_width', 1.40)
        self.declare_parameter('wheel_base', 2.20)
        self.declare_parameter('max_linear_speed', 30.0)
        self.declare_parameter('max_angular_speed', 1.5)
        self.declare_parameter('max_steering_angle_deg', 35.0)
        self.declare_parameter('min_turning_radius', 2.0)

        self.wheel_radius = self.get_parameter('wheel_radius').value
        self.track_width = self.get_parameter('track_width').value
        self.wheel_base = self.get_parameter('wheel_base').value
        self.max_linear_speed = self.get_parameter('max_linear_speed').value
        self.max_angular_speed = self.get_parameter('max_angular_speed').value
        self.max_steering_angle_deg = self.get_parameter('max_steering_angle_deg').value
        self.min_turning_radius = self.get_parameter('min_turning_radius').value

        self.max_steering_angle_rad = math.radians(self.max_steering_angle_deg)

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

        self.steering_pub = self.create_publisher(
            Float32,
            '/panter/steering_cmd',
            10
        )

        self.get_logger().info('panter_ackermann_velocity_mapper iniciado.')
        self.get_logger().info('Suscrito a /cmd_vel')
        self.get_logger().info('Publicando en /panter/wheel_velocity_cmd')
        self.get_logger().info('Publicando en /panter/steering_cmd')

    def clamp(self, value, min_value, max_value):
        return max(min(value, max_value), min_value)

    def sign(self, value):
        if value >= 0.0:
            return 1.0
        return -1.0

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

        # Caso recto o casi recto.
        if abs(wz) < 1e-4:
            wheel_omega = v / self.wheel_radius

            wheel_msg = Float32MultiArray()
            wheel_msg.data = [
                float(wheel_omega),  # FL
                float(wheel_omega),  # FR
                float(wheel_omega),  # RL
                float(wheel_omega),  # RR
            ]

            steering_msg = Float32()
            steering_msg.data = 0.0

            self.velocity_pub.publish(wheel_msg)
            self.steering_pub.publish(steering_msg)
            return

        # Ackermann no puede girar sobre sí mismo sin avance.
        # Si v = 0 y solo hay angular.z, giramos la dirección, pero las ruedas no avanzan.
        if abs(v) < 1e-4:
            steering_msg = Float32()
            steering_msg.data = float(self.sign(wz))

            wheel_msg = Float32MultiArray()
            wheel_msg.data = [0.0, 0.0, 0.0, 0.0]

            self.velocity_pub.publish(wheel_msg)
            self.steering_pub.publish(steering_msg)
            return

        # Radio de giro respecto al centro del vehículo.
        turning_radius = abs(v / wz)

        # Limitamos radios demasiado pequeños, porque un Ackermann real no gira como un tanque.
        if turning_radius < self.min_turning_radius:
            turning_radius = self.min_turning_radius
            wz = self.sign(wz) * abs(v) / turning_radius

        # Si wz > 0, giro a la izquierda: lado izquierdo interior.
        if wz > 0.0:
            rear_left_radius = turning_radius - half_track
            rear_right_radius = turning_radius + half_track
        else:
            rear_left_radius = turning_radius + half_track
            rear_right_radius = turning_radius - half_track

        rear_left_radius = max(rear_left_radius, 0.01)
        rear_right_radius = max(rear_right_radius, 0.01)

        front_left_radius = math.sqrt(rear_left_radius ** 2 + self.wheel_base ** 2)
        front_right_radius = math.sqrt(rear_right_radius ** 2 + self.wheel_base ** 2)

        direction = self.sign(v)
        yaw_rate_abs = abs(wz)

        v_fl = direction * yaw_rate_abs * front_left_radius
        v_fr = direction * yaw_rate_abs * front_right_radius
        v_rl = direction * yaw_rate_abs * rear_left_radius
        v_rr = direction * yaw_rate_abs * rear_right_radius

        omega_fl = v_fl / self.wheel_radius
        omega_fr = v_fr / self.wheel_radius
        omega_rl = v_rl / self.wheel_radius
        omega_rr = v_rr / self.wheel_radius

        steering_angle = math.atan(self.wheel_base / turning_radius)
        steering_normalized = steering_angle / self.max_steering_angle_rad
        steering_normalized = self.clamp(steering_normalized, 0.0, 1.0)

        if wz < 0.0:
            steering_normalized *= -1.0

        wheel_msg = Float32MultiArray()
        wheel_msg.data = [
            float(omega_fl),
            float(omega_fr),
            float(omega_rl),
            float(omega_rr),
        ]

        steering_msg = Float32()
        steering_msg.data = float(steering_normalized)

        self.velocity_pub.publish(wheel_msg)
        self.steering_pub.publish(steering_msg)


def main(args=None):
    rclpy.init(args=args)
    node = PanterAckermannVelocityMapper()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()


if __name__ == '__main__':
    main()
