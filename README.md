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
- publicar odometría e información inercial para analizar las maniobras.

## Arquitectura

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

Las referencias `/panter/wheel_velocity_cmd` permanecen dentro de ROS 2 en los modos de velocidad por rueda. Unity recibe la actuación final mediante par y, en Ackermann, mediante una referencia adicional de dirección.

## Modos de control

### Ackermann directo por par

`/cmd_vel` se transforma en:

- `/panter/steering_cmd`;
- `/panter/wheel_torque_cmd`.

### *Skid-steering* directo por par

Las ruedas delanteras permanecen alineadas y el giro se genera mediante una diferencia de par entre los lados izquierdo y derecho.

### Ackermann con control de velocidad por rueda

El mapper genera referencias individuales en `/panter/wheel_velocity_cmd` y una referencia de dirección. El controlador compara las referencias con `/panter/wheel_states` y calcula el par de cada rueda.

### *Skid-steering* con control de velocidad por rueda

El mapper genera una referencia común para las ruedas de cada lado. El mismo controlador de velocidad transforma posteriormente el error de seguimiento en par.

## Estructura

```text
panter-unity-ros2/
├── README.md
├── .gitignore
├── docs/
│   ├── GUIA_EJECUCION.md
│   ├── dependencies.md
│   └── topics.md
├── ros2/
│   ├── README.md
│   └── panter_control/
└── unity/
    ├── README.md
    └── Scripts/
```

## Guía de instalación y ejecución

La configuración de los cuatro modos, los comandos de terminal, los parámetros de los nodos, la configuración de fricción y los procedimientos de diagnóstico se recogen en:

**[`docs/GUIA_EJECUCION.md`](docs/GUIA_EJECUCION.md)**

## Dependencias principales

- ROS 2 Humble.
- ROS-TCP-Endpoint.
- Unity.
- ROS-TCP-Connector.
- Wheel Controller 3D.

Wheel Controller 3D es una dependencia externa distribuida mediante Unity Asset Store y **no se incluye en este repositorio**.

Consulta [`docs/dependencies.md`](docs/dependencies.md).

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

La descripción ampliada está en [`docs/topics.md`](docs/topics.md).

## Parámetros físicos principales del modelo Unity

- masa total: `866 kg`;
- radio de las ruedas: `0.3302 m`;
- batalla geométrica del modelo: `2.3054 m`;
- ancho de vía geométrico de referencia: `1.336 m`;
- masa de rueda delantera: `22.8 kg`;
- masa de rueda trasera: `26.5 kg`;
- recorrido máximo delantero: `0.0547 m`;
- recorrido máximo trasero: `0.0340 m`.

Los mappers ROS 2 conservan los parámetros con los que se realizaron las pruebas. En particular, `panter_ackermann_velocity_mapper` utiliza por defecto `track_width=1.40 m` y `wheel_base=2.20 m`, mientras que `panter_skid_velocity_mapper` utiliza `track_width=1.40 m`. La guía de ejecución recoge todos los valores usados por los nodos.

## Configuración de fricción para *skid-steering*

En las pruebas finales se emplearon configuraciones específicas de fricción lateral:

- directo por par: `Grip = 0.4`, `Load Rating = 1.0`;
- control de velocidad: `Grip = 0.6`, `Load Rating = 1.1`.

Estas configuraciones facilitan el deslizamiento lateral necesario para el giro tipo *skid-steering*.

## Código ROS 2

El paquete [`ros2/panter_control`](ros2/panter_control) contiene los cinco ejecutables utilizados en la arquitectura final y el módulo de curva de tracción.

## Código Unity

Los scripts propios distribuibles están en [`unity/Scripts`](unity/Scripts). El repositorio no redistribuye el código fuente de Wheel Controller 3D ni otros recursos comerciales.

Algunas modificaciones necesarias se aplicaron sobre el `CarController` incluido con Wheel Controller 3D. Debido a que ese archivo pertenece a una dependencia de terceros, no se publica aquí; las adaptaciones necesarias se documentarán como instrucciones de integración.

## Reproducibilidad

Una vez completada la revisión de los archivos de Unity y de la documentación se fijará la versión correspondiente a la entrega mediante una etiqueta del repositorio.

## Autor

**Ignacio Viera**  
Trabajo Fin de Máster — Ingeniería Mecatrónica
