# Tópicos ROS 2

La arquitectura del simulador utiliza una interfaz de entrada común basada en `/cmd_vel` y separa las referencias internas de ROS 2 de las órdenes que finalmente se envían a Unity.

## Tópicos principales

| Tópico | Tipo | Flujo | Función |
|---|---|---|---|
| `/cmd_vel` | `geometry_msgs/msg/Twist` | ROS 2 → ROS 2 | Consigna general de velocidad lineal y angular. |
| `/panter/steering_cmd` | `std_msgs/msg/Float32` | ROS 2 → Unity | Referencia normalizada de dirección en los modelos Ackermann. |
| `/panter/wheel_torque_cmd` | `std_msgs/msg/Float32MultiArray` | ROS 2 → Unity | Consignas de par para FL, FR, RL y RR. |
| `/panter/wheel_velocity_cmd` | `std_msgs/msg/Float32MultiArray` | ROS 2 interno | Velocidades angulares objetivo utilizadas por el controlador de velocidad por rueda. |
| `/panter/wheel_states` | `std_msgs/msg/Float32MultiArray` | Unity → ROS 2 | Velocidades angulares medidas de FL, FR, RL y RR. |
| `/panter/wheel_loads` | `std_msgs/msg/Float32MultiArray` | Unity → ROS 2 | Cargas verticales de FL, FR, RL y RR, expresadas en N. |
| `/panter/wheel_masses_equivalent` | `std_msgs/msg/Float32MultiArray` | Unity → ROS 2 | Masa equivalente asociada a cada carga vertical, calculada como `F/g`. |
| `/panter/wheel_load_distribution` | `std_msgs/msg/Float32MultiArray` | Unity → ROS 2 | Fracción de la carga total soportada por cada rueda. |
| `/fixposition/odometry` | `nav_msgs/msg/Odometry` | Unity → ROS 2 | Posición, orientación y movimiento del vehículo simulado. |
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

`/panter/wheel_velocity_cmd` permanece dentro de ROS 2. Unity recibe las consignas finales de par y la referencia de dirección.

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

## Publicación de cargas verticales

`WheelLoadPublisher.cs` obtiene la carga de cada `WheelController` y publica tres representaciones de la misma información:

```text
/panter/wheel_loads
/panter/wheel_masses_equivalent
/panter/wheel_load_distribution
```

Para una carga `F_i`:

```text
masa_equivalente_i = F_i / 9.81

distribucion_i = F_i / suma(F_FL, F_FR, F_RL, F_RR)
```

La distribución se publica como una fracción entre 0 y 1. La suma de los cuatro valores es aproximadamente 1 cuando existe carga total distinta de cero.

## Monitorización

Para consultar el grafo sin depender del daemon:

```bash
ros2 topic list --no-daemon
ros2 node list --no-daemon
```

Para observar una señal concreta:

```bash
ros2 topic echo /panter/wheel_states
ros2 topic echo /panter/wheel_loads
ros2 topic echo /fixposition/odometry
```

La guía completa de puesta en marcha se encuentra en [`GUIA_EJECUCION.md`](GUIA_EJECUCION.md).
