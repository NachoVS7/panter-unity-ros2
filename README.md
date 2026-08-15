# panter-unity-ros2

Simulación y control del vehículo todoterreno **Panter** en Unity con integración en **ROS 2**.

Este repositorio acompaña al Trabajo Fin de Máster dedicado al desarrollo de un simulador del Panter. Unity se utiliza para la simulación física del vehículo y ROS 2 para la generación de consignas, la ejecución de los nodos de control y el registro de variables de estado.

## Objetivo

El proyecto permite:

- recibir consignas generales de movimiento mediante `/cmd_vel`;
- aplicar dirección y par sobre las ruedas del modelo físico;
- utilizar estrategias Ackermann y tipo *skid-steering*;
- cerrar un lazo de control de velocidad de forma individual para cada rueda;
- publicar velocidades de rueda y cargas verticales;
- publicar información de odometría e IMU para el análisis de las maniobras.

## Arquitectura

La arquitectura separa el control de la simulación física:

```text
                       ROS 2

 /cmd_vel
     |
     v
 +-----------------------------+
 | mappers / controladores     |
 | panter_control              |
 +-----------------------------+
       |              ^
       |              |
       |              | /panter/wheel_states
       |              | /panter/wheel_loads
       v              |
 /panter/steering_cmd |
 /panter/wheel_torque_cmd
       |              |
       +-------> ROS-TCP <-------+
                    |
                    v
                  Unity
                    |
             modelo físico
                 Panter
```

Las referencias de velocidad `/panter/wheel_velocity_cmd` se utilizan internamente en ROS 2 por el controlador de velocidad por rueda. La actuación final enviada a Unity se realiza mediante consignas de dirección y/o par.

## Modos de control implementados

### Ackermann directo por par

La consigna `/cmd_vel` se transforma en:

- `/panter/steering_cmd`: referencia de dirección para las ruedas delanteras;
- `/panter/wheel_torque_cmd`: consignas de par para las cuatro ruedas motrices.

### *Skid-steering* directo por par

Las ruedas delanteras permanecen alineadas y el giro se genera mediante diferencias de par entre los lados izquierdo y derecho.

### Ackermann con control de velocidad por rueda

El nodo de mapeo genera referencias individuales en `/panter/wheel_velocity_cmd` y una referencia de dirección. El controlador compara las referencias con `/panter/wheel_states` y calcula el par de cada rueda.

### *Skid-steering* con control de velocidad por rueda

Las referencias de velocidad se generan por lados. El controlador de rueda utiliza la misma estructura de realimentación y genera las consignas finales de par.

## Estructura prevista del repositorio

```text
panter-unity-ros2/
├── README.md
├── .gitignore
├── docs/
│   ├── dependencies.md
│   └── topics.md
├── ros2/
│   └── README.md
└── unity/
    └── README.md
```

Los directorios `ros2/` y `unity/` contendrán únicamente el código y los archivos que pueden distribuirse como parte del trabajo. Las dependencias de terceros no se incluyen en el repositorio.

## Dependencias principales

- ROS 2 Humble.
- ROS-TCP-Endpoint.
- Unity.
- ROS-TCP-Connector.
- Wheel Controller 3D.

Wheel Controller 3D es una dependencia externa distribuida mediante Unity Asset Store y **no se incluye en este repositorio**.

Consulta [`docs/dependencies.md`](docs/dependencies.md) para obtener más información.

## Tópicos principales

| Tópico | Flujo | Función |
|---|---|---|
| `/cmd_vel` | ROS 2 → ROS 2 | Consigna general de velocidad lineal y angular. |
| `/panter/steering_cmd` | ROS 2 → Unity | Referencia de dirección en los modos Ackermann. |
| `/panter/wheel_torque_cmd` | ROS 2 → Unity | Consignas de par para las cuatro ruedas. |
| `/panter/wheel_velocity_cmd` | ROS 2 interno | Referencias de velocidad utilizadas por el controlador cerrado. |
| `/panter/wheel_states` | Unity → ROS 2 | Velocidades angulares medidas de las ruedas. |
| `/panter/wheel_loads` | Unity → ROS 2 | Cargas verticales de las cuatro ruedas. |
| `/fixposition/odometry` | Unity → ROS 2 | Posición y orientación simuladas. |
| `/fixposition/imu` | Unity → ROS 2 | Información inercial simulada. |

La descripción ampliada se encuentra en [`docs/topics.md`](docs/topics.md).

## Parámetros principales del modelo

Los valores utilizados en la versión del TFM incluyen:

- masa total del vehículo: `866 kg`;
- radio de rueda: `0.3302 m`;
- batalla: `2.3054 m`;
- ancho de vía empleado en los modelos de control: `1.336 m`;
- masa de rueda delantera: `22.8 kg`;
- masa de rueda trasera: `26.5 kg`;
- recorrido máximo de suspensión delantera: `0.0547 m`;
- recorrido máximo de suspensión trasera: `0.0340 m`.

Los parámetros completos y su justificación se describen en la memoria del TFM.

## Código ROS 2

El paquete principal es `panter_control`. Incluye los mappers Ackermann y *skid-steering*, el controlador de velocidad por rueda y el módulo utilizado para limitar el par disponible mediante la curva de tracción--velocidad.

Las instrucciones de compilación y ejecución se recogerán en [`ros2/README.md`](ros2/README.md) junto con el código correspondiente a la versión final empleada en los ensayos.

## Código Unity

Los scripts propios necesarios para la comunicación, actuación y publicación de variables se documentan en [`unity/README.md`](unity/README.md).

El repositorio no redistribuye Wheel Controller 3D ni otros recursos de terceros.

## Reproducibilidad

La versión final asociada a la entrega del TFM se fijará mediante una etiqueta del repositorio una vez incorporados y comprobados todos los archivos utilizados en los ensayos.

## Autor

**Ignacio Viera**  
Trabajo Fin de Máster — Ingeniería Mecatrónica
