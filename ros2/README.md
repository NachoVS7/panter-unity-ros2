# ROS 2 — paquete `panter_control`

Esta carpeta contendrá la versión final del paquete ROS 2 utilizado en los ensayos del TFM.

## Nodos principales

- `panter_ackermann_mapper.py`: control Ackermann directo por par.
- `panter_skid_mapper.py`: control tipo *skid-steering* directo por par.
- `panter_ackermann_velocity_mapper.py`: generación de referencias de velocidad por rueda para Ackermann.
- `panter_skid_velocity_mapper.py`: generación de referencias de velocidad por lado para *skid-steering*.
- `panter_wheel_velocity_controller.py`: controlador proporcional de velocidad por rueda.
- `traction_curve.py`: curva fuerza de tracción--velocidad y cálculo del par máximo disponible por rueda.

## Compilación

Una vez situado `panter_control` dentro de un workspace ROS 2:

```bash
cd ~/ros2_unity_ws
colcon build --packages-select panter_control
source install/setup.bash
```

## Puesta en marcha básica

En una terminal debe ejecutarse ROS-TCP-Endpoint para establecer la comunicación con Unity. En terminales independientes pueden lanzarse el nodo de control correspondiente y `teleop_twist_keyboard`.

### Ackermann directo por par

```bash
ros2 run panter_control panter_ackermann_mapper
```

### Skid-steering directo por par

```bash
ros2 run panter_control panter_skid_mapper
```

### Ackermann con control de velocidad por rueda

Terminal 1:

```bash
ros2 run panter_control panter_ackermann_velocity_mapper
```

Terminal 2:

```bash
ros2 run panter_control panter_wheel_velocity_controller
```

### Skid-steering con control de velocidad por rueda

Terminal 1:

```bash
ros2 run panter_control panter_skid_velocity_mapper
```

Terminal 2:

```bash
ros2 run panter_control panter_wheel_velocity_controller
```

### Teleoperación

```bash
ros2 run teleop_twist_keyboard teleop_twist_keyboard
```

## Parámetros empleados en la versión del TFM

Entre los parámetros comunes utilizados en los modelos se encuentran:

- radio de rueda: `0.3302 m`;
- ancho de vía: `1.336 m`;
- batalla: `2.3054 m`;
- ganancia proporcional del controlador de velocidad: `Kp = 80`;
- velocidad lineal máxima del controlador por velocidad: `20 m/s`.

El código fuente final se incorporará en esta carpeta antes de fijar la versión de entrega del repositorio.
