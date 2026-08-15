# Tópicos ROS 2

La arquitectura del simulador utiliza una interfaz de entrada común basada en `/cmd_vel` y separa las referencias internas de ROS 2 de las órdenes que finalmente se envían a Unity.

## Tópicos principales

| Tópico | Tipo | Flujo | Función |
|---|---|---|---|
| `/cmd_vel` | `geometry_msgs/msg/Twist` | ROS 2 → ROS 2 | Consigna general de movimiento: velocidad lineal y velocidad angular. |
| `/panter/steering_cmd` | `std_msgs/msg/Float32` | ROS 2 → Unity | Referencia normalizada de dirección en los modelos Ackermann. |
| `/panter/wheel_torque_cmd` | `std_msgs/msg/Float32MultiArray` | ROS 2 → Unity | Consignas de par para FL, FR, RL y RR. |
| `/panter/wheel_velocity_cmd` | `std_msgs/msg/Float32MultiArray` | ROS 2 interno | Velocidades angulares objetivo utilizadas por el controlador de velocidad por rueda. |
| `/panter/wheel_states` | `std_msgs/msg/Float32MultiArray` | Unity → ROS 2 | Velocidades angulares medidas de FL, FR, RL y RR. |
| `/panter/wheel_loads` | `std_msgs/msg/Float32MultiArray` | Unity → ROS 2 | Cargas verticales de FL, FR, RL y RR. |
| `/fixposition/odometry` | `nav_msgs/msg/Odometry` | Unity → ROS 2 | Posición y orientación simuladas del vehículo. |
| `/fixposition/imu` | `sensor_msgs/msg/Imu` | Unity → ROS 2 | Información inercial simulada. |

## Orden de las ruedas

Los vectores de cuatro elementos mantienen siempre el siguiente orden:

```text
[FL, FR, RL, RR]
```

- `FL`: delantera izquierda.
- `FR`: delantera derecha.
- `RL`: trasera izquierda.
- `RR`: trasera derecha.

## Flujo Ackermann directo por par

```text
/cmd_vel
   |
   v
panter_ackermann_mapper
   |-----------------------------> /panter/steering_cmd ------> Unity
   |
   +-----------------------------> /panter/wheel_torque_cmd --> Unity

Unity ---------------------------> /panter/wheel_states
```

`/panter/wheel_states` se utiliza para estimar la velocidad del vehículo y limitar el par máximo mediante la curva de tracción--velocidad.

## Flujo skid-steering directo por par

```text
/cmd_vel
   |
   v
panter_skid_mapper
   |
   +-----------------------------> /panter/wheel_torque_cmd --> Unity

Unity ---------------------------> /panter/wheel_states
```

El nodo genera una demanda común para las ruedas de cada lado y mantiene las ruedas delanteras sin dirección.

## Flujo Ackermann con control de velocidad por rueda

```text
/cmd_vel
   |
   v
panter_ackermann_velocity_mapper
   |-----------------------------> /panter/steering_cmd ------> Unity
   |
   +----> /panter/wheel_velocity_cmd
                         |
                         v
              panter_wheel_velocity_controller
                         ^
                         |
              /panter/wheel_states <------------------------- Unity
                         |
                         v
              /panter/wheel_torque_cmd ---------------------> Unity
```

## Flujo skid-steering con control de velocidad por rueda

```text
/cmd_vel
   |
   v
panter_skid_velocity_mapper
   |
   +----> /panter/wheel_velocity_cmd
                         |
                         v
              panter_wheel_velocity_controller
                         ^
                         |
              /panter/wheel_states <------------------------- Unity
                         |
                         v
              /panter/wheel_torque_cmd ---------------------> Unity
```

En esta variante no se utiliza `/panter/steering_cmd`.
