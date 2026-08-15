# Guía de ejecución del simulador Panter

Esta guía resume la configuración utilizada para ejecutar la versión final del simulador del Panter con Unity y ROS 2 Humble.

## 1. Entorno

- Ubuntu 22.04 sobre WSL.
- ROS 2 Humble.
- Espacio de trabajo: `~/ros2_unity_ws`.
- Paquete ROS 2: `panter_control`.
- Comunicación Unity--ROS 2 mediante ROS-TCP-Connector y ROS-TCP-Endpoint.

Orden de las ruedas en todos los vectores:

```text
[FL, FR, RL, RR]
```

- `FL`: delantera izquierda.
- `FR`: delantera derecha.
- `RL`: trasera izquierda.
- `RR`: trasera derecha.

## 2. Preparación del espacio de trabajo

En una terminal de WSL:

```bash
source /opt/ros/humble/setup.bash
cd ~/ros2_unity_ws
colcon build --symlink-install
source install/setup.bash
```

Para comprobar que los ejecutables finales están instalados:

```bash
ros2 pkg executables panter_control
```

Los ejecutables utilizados en la versión final son:

```text
panter_ackermann_mapper
panter_skid_mapper
panter_ackermann_velocity_mapper
panter_skid_velocity_mapper
panter_wheel_velocity_controller
```

## 3. ROS-TCP-Endpoint

Mantener una terminal dedicada al endpoint:

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint
```

Para consultar la dirección IPv4 de WSL:

```bash
hostname -I
```

En Unity, configurar `ROSConnection` con la dirección de WSL y el mismo puerto utilizado por ROS-TCP-Endpoint.

## 4. Consultas ROS 2 sin daemon

En el entorno utilizado durante el desarrollo se evitó depender del daemon de ROS 2 para las consultas del grafo. Cuando sea necesario listar nodos o tópicos, se recomienda utilizar:

```bash
ros2 topic list --no-daemon
ros2 node list --no-daemon
```

Para consultar información detallada de un tópico puede utilizarse igualmente la opción `--no-daemon` cuando esté disponible en el verbo empleado.

Los comandos que crean sus propias suscripciones o publicadores, como `ros2 topic echo`, `ros2 topic pub`, `ros2 topic hz` o `ros2 run`, no necesitan el daemon para realizar su función principal.

## 5. Configuración común en Unity

En los cuatro modos finales:

- `ROSConnection`: activado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.
- `WheelLoadPublisher`: activado cuando se registran cargas.
- `WheelVelocityCommandSubscriber`: desactivado.
- `CmdVelCarController`: desactivado.

`/panter/wheel_velocity_cmd` se utiliza como referencia interna en ROS 2. La velocidad objetivo no se impone directamente en Unity. La actuación final se realiza mediante `/panter/wheel_torque_cmd` y, en Ackermann, también mediante `/panter/steering_cmd`.

## 6. Ackermann directo por par

### Unity

- Dirección delantera: activada.
- `SteeringCommandSubscriber`: activado.
- `WheelTorqueCommandSubscriber`: activado.
- Configuración normal de fricción de las ruedas.

### ROS 2

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_ackermann_mapper
```

El nodo recibe `/cmd_vel` y publica:

```text
/panter/steering_cmd
/panter/wheel_torque_cmd
```

Parámetros por defecto del nodo:

```text
max_linear_speed      = 2.0 m/s
max_angular_speed     = 1.0 rad/s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

## 7. Skid-steering directo por par

### Unity

- Dirección delantera: desactivada.
- Ruedas delanteras alineadas con el chasis.
- `SteeringCommandSubscriber`: desactivado.
- `WheelTorqueCommandSubscriber`: activado.

Para las pruebas finales de este modo se modificaron los parámetros de fricción lateral de las ruedas a:

```text
Grip        = 0.4
Load Rating = 1.0
```

Esta modificación facilita el deslizamiento lateral necesario para producir el giro mediante diferencias de par entre los lados.

### ROS 2

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_skid_mapper
```

El nodo recibe `/cmd_vel` y publica `/panter/wheel_torque_cmd`.

Parámetros por defecto:

```text
max_linear_speed      = 2.0 m/s
max_angular_speed     = 1.0 rad/s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

## 8. Ackermann con controlador de velocidad por rueda

Este modo utiliza dos nodos ROS 2.

### Terminal del mapper

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_ackermann_velocity_mapper
```

Parámetros por defecto del mapper:

```text
wheel_radius             = 0.3302 m
track_width              = 1.40 m
wheel_base               = 2.20 m
max_linear_speed         = 30.0 m/s
max_angular_speed        = 1.5 rad/s
max_steering_angle_deg   = 35 deg
min_turning_radius       = 2.0 m
```

### Terminal del controlador

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_wheel_velocity_controller
```

Parámetros por defecto del controlador:

```text
kp                    = 80.0
max_torque_safety     = 1500 Nm
command_timeout       = 0.5 s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
control period        = 0.02 s (50 Hz)
```

### Unity

- Dirección delantera: activada.
- `SteeringCommandSubscriber`: activado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.
- `WheelVelocityCommandSubscriber`: desactivado.

Flujo principal:

```text
/cmd_vel
   -> panter_ackermann_velocity_mapper
   -> /panter/wheel_velocity_cmd
   -> panter_wheel_velocity_controller
   -> /panter/wheel_torque_cmd
   -> Unity
```

La dirección se publica en paralelo mediante `/panter/steering_cmd`.

## 9. Skid-steering con controlador de velocidad por rueda

### Unity

- Dirección delantera: desactivada.
- Ruedas delanteras alineadas.
- `SteeringCommandSubscriber`: desactivado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.

En los ensayos finales de esta variante se utilizaron los siguientes valores de fricción lateral:

```text
Grip        = 0.6
Load Rating = 1.1
```

### Terminal del mapper

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_skid_velocity_mapper
```

Parámetros por defecto:

```text
wheel_radius        = 0.3302 m
track_width         = 1.40 m
max_linear_speed    = 20.0 m/s
max_angular_speed   = 10.0 rad/s
```

### Terminal del controlador

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_wheel_velocity_controller
```

## 10. Teleoperación

Para generar consignas manualmente:

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run teleop_twist_keyboard teleop_twist_keyboard
```

El nodo publica en `/cmd_vel`, por lo que puede utilizarse con cualquiera de los cuatro modos anteriores siempre que el mapper correspondiente esté activo.

## 11. Publicación manual de consignas

Ejemplo de avance recto:

```bash
ros2 topic pub -r 20 /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 1.0}, angular: {z: 0.0}}"
```

Ejemplo de giro:

```bash
ros2 topic pub -r 20 /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 1.0}, angular: {z: 0.25}}"
```

Detención:

```bash
ros2 topic pub --once /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 0.0}, angular: {z: 0.0}}"
```

Estos comandos son ejemplos reproducibles de comprobación y no deben interpretarse necesariamente como las consignas exactas de todos los registros incluidos en la memoria.

## 12. Monitorización

Listar tópicos sin daemon:

```bash
ros2 topic list --no-daemon
```

Mostrar variables principales:

```bash
ros2 topic echo /cmd_vel
ros2 topic echo /panter/wheel_states
ros2 topic echo /panter/wheel_torque_cmd
ros2 topic echo /panter/wheel_velocity_cmd
ros2 topic echo /panter/steering_cmd
ros2 topic echo /panter/wheel_loads
ros2 topic echo /fixposition/odometry
```

Comprobar frecuencias:

```bash
ros2 topic hz /panter/wheel_states
ros2 topic hz /panter/wheel_loads
ros2 topic hz /fixposition/odometry
ros2 topic hz /fixposition/imu
```

## 13. Registro con rosbag

Ejemplo de registro de las variables principales:

```bash
ros2 bag record -o panter_test \
/cmd_vel \
/panter/wheel_states \
/panter/wheel_torque_cmd \
/panter/wheel_velocity_cmd \
/panter/steering_cmd \
/panter/wheel_loads \
/fixposition/odometry \
/fixposition/imu
```

Los tópicos que no estén activos en el modo seleccionado pueden omitirse.

## 14. Curva de tracción

Los modelos que actúan mediante par utilizan `traction_curve.py` para calcular el límite de par en función de la velocidad estimada del vehículo.

La fuerza total se interpola entre los siguientes puntos:

| Velocidad [km/h] | Fuerza [N] |
|---:|---:|
| 0 | 14800 |
| 15 | 14800 |
| 18 | 14000 |
| 22 | 12500 |
| 26 | 10800 |
| 30 | 9500 |
| 35 | 8500 |
| 40 | 7600 |
| 45 | 6900 |
| 50 | 6400 |
| 55 | 6000 |
| 60 | 5600 |
| 66 | 5200 |
| 72 | 4800 |

El par máximo por rueda se calcula como:

```text
Tmax = Ftraccion * wheel_radius / powered_wheel_count
```

## 15. Solución de problemas

### No aparecen los tópicos

```bash
ros2 topic list --no-daemon
```

Comprobar que Unity está en Play, que `ROSConnection` está conectado y que ROS-TCP-Endpoint continúa ejecutándose.

### El vehículo no se mueve

Comprobar:

```bash
ros2 topic echo /cmd_vel
ros2 topic echo /panter/wheel_torque_cmd
ros2 topic echo /panter/wheel_states
```

En los modos de velocidad comprobar además:

```bash
ros2 topic echo /panter/wheel_velocity_cmd
```

### Giro en sentido incorrecto

Revisar:

- signo de `angular.z`;
- orden `[FL, FR, RL, RR]`;
- signo de las velocidades publicadas por `WheelStatePublisher`;
- configuración de las ruedas directrices en Unity.

### Vibraciones en reposo

Durante el desarrollo se observó que la estabilidad mejoró al eliminar scripts auxiliares de estabilización que introducían fuerzas adicionales y al aumentar la frecuencia de actualización física de Unity. También deben comprobarse los recorridos de suspensión, la amortiguación y los colliders.

### Cambio entre modos

Antes de cambiar de estrategia:

1. enviar una consigna nula;
2. detener los nodos anteriores con `Ctrl+C`;
3. detener Play en Unity;
4. cambiar la configuración de dirección y, cuando corresponda, la fricción lateral;
5. iniciar los nuevos nodos;
6. volver a ejecutar la simulación.

Nunca deben ejecutarse simultáneamente dos mappers que publiquen sobre los mismos tópicos de actuación.
