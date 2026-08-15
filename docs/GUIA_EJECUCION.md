# Guía de instalación y ejecución del simulador Panter

Esta guía recoge la configuración utilizada para ejecutar la versión final del simulador del Panter con Unity y ROS 2.

## 1. Entorno utilizado

- Windows con WSL.
- Ubuntu 22.04.
- ROS 2 Humble.
- Unity 2022.3 LTS.
- Espacio de trabajo ROS 2: `~/ros2_unity_ws`.
- Paquete ROS 2: `panter_control`.
- Comunicación Unity--ROS 2 mediante ROS-TCP-Connector y ROS-TCP-Endpoint.
- Simulación de las ruedas mediante Wheel Controller 3D.

Orden de las ruedas en todos los vectores de cuatro elementos:

```text
[FL, FR, RL, RR]
```

- `FL`: delantera izquierda.
- `FR`: delantera derecha.
- `RL`: trasera izquierda.
- `RR`: trasera derecha.

## 2. Instalación del paquete ROS 2

Clonar el repositorio en una ubicación temporal o descargarlo desde GitHub. El paquete que debe copiarse al espacio de trabajo es:

```text
ros2/panter_control
```

Ejemplo de estructura final:

```text
~/ros2_unity_ws/
└── src/
    └── panter_control/
        ├── package.xml
        ├── setup.py
        ├── setup.cfg
        ├── resource/
        └── panter_control/
```

Compilar el paquete:

```bash
source /opt/ros/humble/setup.bash
cd ~/ros2_unity_ws
colcon build --symlink-install --packages-select panter_control
source install/setup.bash
```

Para comprobar que los ejecutables finales están instalados:

```bash
ros2 pkg executables panter_control
```

Los ejecutables utilizados en la arquitectura final son:

```text
panter_ackermann_mapper
panter_skid_mapper
panter_ackermann_velocity_mapper
panter_skid_velocity_mapper
panter_wheel_velocity_controller
```

## 3. ROS-TCP-Endpoint

Mantener una terminal dedicada al endpoint durante toda la simulación:

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint
```

Para consultar la dirección IPv4 de WSL:

```bash
hostname -I
```

En Unity, configurar `ROSConnection` con la dirección IPv4 de WSL y el mismo puerto utilizado por ROS-TCP-Endpoint.

## 4. Uso de ROS 2 sin depender del daemon

Durante el desarrollo se produjeron problemas de descubrimiento asociados al daemon de ROS 2. Por este motivo, para las consultas del grafo se utilizó el modo directo siempre que el comando lo permite.

No es necesario ejecutar `ros2 daemon start` para utilizar el simulador.

Si existe un daemon anterior y se quiere detener:

```bash
ros2 daemon stop
```

Para listar tópicos y nodos sin utilizar el daemon:

```bash
ros2 topic list --no-daemon
ros2 node list --no-daemon
```

Los comandos que crean directamente sus propios nodos, suscripciones o publicadores, como `ros2 run`, `ros2 topic echo`, `ros2 topic pub` y `ros2 topic hz`, pueden utilizarse normalmente sin iniciar manualmente el daemon.

No debe añadirse `--no-daemon` a comandos que no admitan dicha opción.

## 5. Configuración común en Unity

En los cuatro modos finales:

- `ROSConnection`: activado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.
- `WheelLoadPublisher`: activado cuando se registran cargas.
- `WheelVelocityCommandSubscriber`: desactivado.
- `CmdVelCarController`: desactivado.

`/panter/wheel_velocity_cmd` se utiliza como referencia interna en ROS 2. La velocidad objetivo no se impone directamente en Unity. La actuación final llega a Unity mediante `/panter/wheel_torque_cmd` y, en las variantes Ackermann, también mediante `/panter/steering_cmd`.

Los scripts propios necesarios se encuentran en `unity/Scripts`. El código de Wheel Controller 3D no se redistribuye en este repositorio.

## 6. Ackermann directo por par

### Configuración en Unity

- Dirección delantera: activada.
- `SteeringCommandSubscriber`: activado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.
- Configuración normal de fricción de las ruedas.

### Nodo ROS 2

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

Además, recibe `/panter/wheel_states` para estimar la velocidad del vehículo utilizada por la limitación de tracción.

Parámetros por defecto:

```text
max_linear_speed      = 2.0 m/s
max_angular_speed     = 1.0 rad/s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

## 7. Skid-steering directo por par

### Configuración en Unity

- Dirección delantera: desactivada.
- Ruedas delanteras alineadas con el chasis.
- `SteeringCommandSubscriber`: desactivado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.

Para las pruebas finales de este modo se utilizaron los siguientes parámetros de fricción lateral:

```text
Grip        = 0.4
Load Rating = 1.0
```

Esta configuración reduce la resistencia al deslizamiento lateral necesario para producir el giro mediante diferencias de actuación entre los lados izquierdo y derecho.

### Nodo ROS 2

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_skid_mapper
```

El nodo recibe `/cmd_vel` y `/panter/wheel_states`, y publica `/panter/wheel_torque_cmd`.

Parámetros por defecto:

```text
max_linear_speed      = 2.0 m/s
max_angular_speed     = 1.0 rad/s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

## 8. Ackermann con controlador de velocidad por rueda

Este modo utiliza dos nodos ROS 2.

### Terminal 1: generador de referencias Ackermann

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_ackermann_velocity_mapper
```

Parámetros por defecto del nodo:

```text
wheel_radius             = 0.3302 m
track_width              = 1.40 m
wheel_base               = 2.20 m
max_linear_speed         = 30.0 m/s
max_angular_speed        = 1.5 rad/s
max_steering_angle_deg   = 35 deg
min_turning_radius       = 2.0 m
```

Estos son los valores conservados en el nodo utilizado durante las pruebas. No deben confundirse con las dimensiones geométricas de referencia del modelo físico de Unity.

### Terminal 2: controlador de velocidad por rueda

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_wheel_velocity_controller
```

Parámetros por defecto:

```text
kp                    = 80.0
max_torque_safety     = 1500 Nm
command_timeout       = 0.5 s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
control_period        = 0.02 s (50 Hz)
```

### Configuración en Unity

- Dirección delantera: activada.
- `SteeringCommandSubscriber`: activado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.
- `WheelVelocityCommandSubscriber`: desactivado.
- Configuración normal de fricción.

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

### Configuración en Unity

- Dirección delantera: desactivada.
- Ruedas delanteras alineadas con el chasis.
- `SteeringCommandSubscriber`: desactivado.
- `WheelTorqueCommandSubscriber`: activado.
- `WheelStatePublisher`: activado.
- `WheelVelocityCommandSubscriber`: desactivado.

En los ensayos finales de esta variante se utilizaron:

```text
Grip        = 0.6
Load Rating = 1.1
```

### Terminal 1: generador de referencias skid-steering

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

### Terminal 2: controlador de velocidad

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run panter_control panter_wheel_velocity_controller
```

El controlador utilizado es el mismo que en la variante Ackermann por velocidad.

## 10. Teleoperación

Para generar consignas manualmente:

```bash
source /opt/ros/humble/setup.bash
source ~/ros2_unity_ws/install/setup.bash
ros2 run teleop_twist_keyboard teleop_twist_keyboard
```

El nodo publica en `/cmd_vel`, por lo que puede utilizarse con cualquiera de los cuatro modos siempre que esté activo el nodo de conversión correspondiente.

Antes de teleoperar debe comprobarse que solo existe un nodo publicando las órdenes correspondientes al modo seleccionado.

## 11. Publicación manual de consignas

Avance recto:

```bash
ros2 topic pub -r 20 /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 1.0}, angular: {z: 0.0}}"
```

Giro con avance:

```bash
ros2 topic pub -r 20 /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 1.0}, angular: {z: 0.25}}"
```

Detención:

```bash
ros2 topic pub --once /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 0.0}, angular: {z: 0.0}}"
```

Estos comandos proporcionan pruebas reproducibles. Las consignas concretas utilizadas en cada figura o ensayo deben consultarse en la memoria y en los datos asociados al ensayo correspondiente.

## 12. Monitorización de tópicos

Listar tópicos sin daemon:

```bash
ros2 topic list --no-daemon
```

Listar nodos sin daemon:

```bash
ros2 node list --no-daemon
```

Mostrar las principales señales de control:

```bash
ros2 topic echo /cmd_vel
ros2 topic echo /panter/wheel_states
ros2 topic echo /panter/wheel_torque_cmd
ros2 topic echo /panter/wheel_velocity_cmd
ros2 topic echo /panter/steering_cmd
```

Mostrar las cargas publicadas desde Unity:

```bash
ros2 topic echo /panter/wheel_loads
ros2 topic echo /panter/wheel_masses_equivalent
ros2 topic echo /panter/wheel_load_distribution
```

Los tres tópicos anteriores contienen, respectivamente:

- carga vertical de cada rueda en N;
- masa equivalente de cada carga, calculada como `F/g`, en kg;
- fracción de la carga total soportada por cada rueda.

Mostrar información de movimiento:

```bash
ros2 topic echo /fixposition/odometry
ros2 topic echo /fixposition/imu
```

Comprobar frecuencias:

```bash
ros2 topic hz /panter/wheel_states
ros2 topic hz /panter/wheel_loads
ros2 topic hz /fixposition/odometry
ros2 topic hz /fixposition/imu
```

## 13. Registro con rosbag

Ejemplo de registro general:

```bash
ros2 bag record -o panter_test \
/cmd_vel \
/panter/wheel_states \
/panter/wheel_torque_cmd \
/panter/wheel_velocity_cmd \
/panter/steering_cmd \
/panter/wheel_loads \
/panter/wheel_masses_equivalent \
/panter/wheel_load_distribution \
/fixposition/odometry \
/fixposition/imu
```

Los tópicos que no estén activos en el modo seleccionado pueden omitirse.

Para comparar ensayos es recomendable utilizar nombres de carpeta diferentes y mantener las mismas condiciones iniciales del vehículo.

## 14. Curva de tracción

Los nodos que actúan mediante par utilizan `traction_curve.py` para limitar el par disponible en función de la velocidad estimada del vehículo.

La fuerza total se interpola linealmente entre los siguientes puntos:

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

El par máximo por rueda se calcula mediante:

```text
Tmax = Ftraccion * wheel_radius / powered_wheel_count
```

Con la configuración actual se utilizan cuatro ruedas motrices y un radio de `0.3302 m`.

## 15. Cambio entre modos

Antes de cambiar de estrategia de control:

1. enviar una consigna nula;
2. detener con `Ctrl+C` los nodos del modo anterior;
3. detener la ejecución de Unity;
4. configurar la dirección delantera;
5. ajustar la fricción lateral si se cambia a una variante skid-steering;
6. iniciar los nodos del nuevo modo;
7. volver a ejecutar la simulación.

Nunca deben ejecutarse simultáneamente dos nodos de conversión que publiquen sobre los mismos tópicos de actuación.

## 16. Solución de problemas

### No aparecen los tópicos

```bash
ros2 topic list --no-daemon
ros2 node list --no-daemon
```

Comprobar que Unity está ejecutándose, que `ROSConnection` está conectado y que ROS-TCP-Endpoint continúa activo.

### El vehículo no se mueve

Comprobar primero la entrada:

```bash
ros2 topic echo /cmd_vel
```

Después comprobar la orden final enviada a Unity:

```bash
ros2 topic echo /panter/wheel_torque_cmd
```

Y la realimentación:

```bash
ros2 topic echo /panter/wheel_states
```

En los modos de velocidad comprobar además:

```bash
ros2 topic echo /panter/wheel_velocity_cmd
```

### El vehículo gira en sentido contrario

Revisar:

- signo de `angular.z`;
- orden `[FL, FR, RL, RR]`;
- signo de las velocidades publicadas por `WheelStatePublisher`;
- configuración de las ruedas directrices en Unity.

### Las velocidades reales tienen signo incorrecto

El signo debe corregirse en la publicación de `WheelStatePublisher` o en la configuración de los ejes de las ruedas. No debe corregirse únicamente durante el tratamiento posterior de los datos.

### El Panter vibra en reposo

Durante el desarrollo se observó una mejora importante al eliminar scripts auxiliares que aplicaban fuerzas adicionales de estabilización y al aumentar la frecuencia de actualización física de Unity. También deben comprobarse los recorridos de suspensión, la amortiguación, la posición de las ruedas y los colliders.

### Problemas de descubrimiento asociados al daemon

Si una consulta del grafo devuelve información incoherente:

```bash
ros2 daemon stop
ros2 topic list --no-daemon
ros2 node list --no-daemon
```

No es necesario volver a iniciar manualmente el daemon para ejecutar los nodos del simulador.

## 17. Cierre de una prueba

Al terminar un ensayo:

1. enviar una consigna nula;
2. detener la teleoperación o publicación manual;
3. detener `rosbag` si estaba activo;
4. detener los nodos del modo de control con `Ctrl+C`;
5. detener Unity;
6. detener ROS-TCP-Endpoint cuando ya no se vayan a realizar más pruebas.

Consigna nula:

```bash
ros2 topic pub --once /cmd_vel geometry_msgs/msg/Twist \
"{linear: {x: 0.0}, angular: {z: 0.0}}"
```

## 18. Archivos no incluidos

El repositorio no incluye por el momento el modelo CAD/FBX del Panter ni otros recursos tridimensionales cuya distribución todavía no se ha decidido.

Tampoco se redistribuye Wheel Controller 3D. La carpeta `unity/Scripts` contiene únicamente los scripts propios que pueden publicarse de forma independiente de dicha dependencia.
