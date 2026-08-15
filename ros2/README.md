# ROS 2 — paquete `panter_control`

Esta carpeta contiene la versión final del paquete ROS 2 utilizado para los cuatro modos de control analizados en el TFM.

## Archivos principales

- `panter_ackermann_mapper.py`: Ackermann directo por par.
- `panter_skid_mapper.py`: *skid-steering* directo por par.
- `panter_ackermann_velocity_mapper.py`: generación de referencias individuales de velocidad y dirección Ackermann.
- `panter_skid_velocity_mapper.py`: generación de referencias de velocidad por lado.
- `panter_wheel_velocity_controller.py`: controlador proporcional de velocidad por rueda.
- `traction_curve.py`: curva fuerza de tracción--velocidad y cálculo del límite de par por rueda.

Los mappers básicos utilizados durante etapas anteriores del desarrollo no forman parte de la arquitectura final publicada en este repositorio.

## Instalación en el workspace

Copiar la carpeta `panter_control` dentro de `src`:

```bash
~/ros2_unity_ws/src/panter_control
```

Compilar:

```bash
source /opt/ros/humble/setup.bash
cd ~/ros2_unity_ws
colcon build --symlink-install --packages-select panter_control
source install/setup.bash
```

Comprobar los ejecutables:

```bash
ros2 pkg executables panter_control
```

## Ejecutables finales

```text
panter_ackermann_mapper
panter_skid_mapper
panter_ackermann_velocity_mapper
panter_skid_velocity_mapper
panter_wheel_velocity_controller
```

## Modos

### Ackermann directo por par

```bash
ros2 run panter_control panter_ackermann_mapper
```

Parámetros por defecto:

```text
max_linear_speed      = 2.0 m/s
max_angular_speed     = 1.0 rad/s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

### Skid-steering directo por par

```bash
ros2 run panter_control panter_skid_mapper
```

Parámetros por defecto:

```text
max_linear_speed      = 2.0 m/s
max_angular_speed     = 1.0 rad/s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

### Ackermann por velocidad

Terminal del mapper:

```bash
ros2 run panter_control panter_ackermann_velocity_mapper
```

Terminal del controlador:

```bash
ros2 run panter_control panter_wheel_velocity_controller
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

### Skid-steering por velocidad

Terminal del mapper:

```bash
ros2 run panter_control panter_skid_velocity_mapper
```

Terminal del controlador:

```bash
ros2 run panter_control panter_wheel_velocity_controller
```

Parámetros por defecto del mapper:

```text
wheel_radius        = 0.3302 m
track_width         = 1.40 m
max_linear_speed    = 20.0 m/s
max_angular_speed   = 10.0 rad/s
```

## Controlador de velocidad por rueda

Parámetros por defecto:

```text
kp                    = 80.0
max_torque_safety     = 1500 Nm
command_timeout       = 0.5 s
wheel_radius          = 0.3302 m
powered_wheel_count   = 4
```

El bucle se ejecuta cada `0.02 s`, equivalente a aproximadamente `50 Hz`.

## Curva de tracción

`traction_curve.py` interpola la fuerza disponible en función de la velocidad estimada del vehículo y calcula:

```text
Tmax = Ftraccion * wheel_radius / powered_wheel_count
```

El controlador de velocidad utiliza el menor valor entre el límite obtenido de esta curva y `max_torque_safety`.

## Tópicos

Entrada común:

```text
/cmd_vel
```

Tópicos de actuación:

```text
/panter/steering_cmd
/panter/wheel_torque_cmd
```

Tópicos internos del lazo de velocidad:

```text
/panter/wheel_velocity_cmd
/panter/wheel_states
```

La descripción completa y la configuración de Unity se encuentran en [`../docs/GUIA_EJECUCION.md`](../docs/GUIA_EJECUCION.md).
