# Unity — simulación del Panter

Esta carpeta contiene los scripts propios distribuibles utilizados para integrar el modelo del Panter con ROS 2.

## Entorno

La versión final del proyecto se desarrolló con Unity 2022.3 LTS.

Unity se encarga de:

- ejecutar la simulación física del vehículo;
- aplicar las consignas de dirección y par calculadas en ROS 2;
- obtener las velocidades angulares de las ruedas;
- obtener las cargas verticales de cada rueda;
- publicar odometría e información inercial simulada.

## Scripts incluidos

En `Scripts/` se incluyen actualmente:

- `WheelTorqueCommandSubscriber.cs`;
- `SteeringCommandSubscriber.cs`;
- `WheelStatePublisher.cs`;
- `WheelLoadPublisher.cs`;
- `OdometryPublisher.cs`;
- `IMUPublisher1.cs`;
- `AutoSetVehicleCenterOfMass.cs`;
- `AutoFitBodyCollider.cs`.

## Orden de las ruedas

Todos los mensajes de cuatro valores siguen:

```text
[FL, FR, RL, RR]
```

- `FL`: delantera izquierda.
- `FR`: delantera derecha.
- `RL`: trasera izquierda.
- `RR`: trasera derecha.

## Modos basados en velocidad

`/panter/wheel_velocity_cmd` no se aplica directamente en Unity. Este tópico permanece dentro de ROS 2 y se utiliza como referencia del controlador cerrado. El resultado del controlador llega a Unity mediante `/panter/wheel_torque_cmd`.

## Configuración de dirección

### Ackermann

- ruedas delanteras con dirección activada;
- `SteeringCommandSubscriber` activado;
- `WheelTorqueCommandSubscriber` activado;
- `WheelStatePublisher` activado.

### Skid-steering

- ruedas delanteras alineadas con el chasis;
- dirección desactivada;
- `SteeringCommandSubscriber` desactivado;
- `WheelTorqueCommandSubscriber` activado;
- `WheelStatePublisher` activado.

## Fricción utilizada en skid-steering

La configuración de fricción lateral se modificó para permitir el deslizamiento necesario durante el giro.

### Directo por par

```text
Grip        = 0.4
Load Rating = 1.0
```

### Control de velocidad por rueda

```text
Grip        = 0.6
Load Rating = 1.1
```

## Publicación de cargas verticales

`WheelLoadPublisher.cs` publica:

```text
/panter/wheel_loads
/panter/wheel_masses_equivalent
/panter/wheel_load_distribution
```

El primer tópico contiene las cargas verticales en N. El segundo transforma cada carga en una masa equivalente mediante `F/9.81`, y el tercero publica la fracción de carga soportada por cada rueda.

## Dependencias externas

El proyecto necesita:

- ROS-TCP-Connector;
- Wheel Controller 3D.

Wheel Controller 3D no se redistribuye en este repositorio. Consulta [`../docs/dependencies.md`](../docs/dependencies.md).

## Adaptación del CarController

El proyecto utiliza una versión adaptada del `CarController` suministrado con Wheel Controller 3D para aceptar entradas externas de dirección y par individual por rueda. Al tratarse de código perteneciente a una dependencia comercial, ese archivo no se redistribuye en este repositorio.

La integración requiere que el controlador permita, como mínimo:

- activar/desactivar entrada externa;
- recibir una entrada externa de dirección;
- habilitar actuación externa por par;
- disponer de un valor de par para cada rueda: FL, FR, RL y RR.

## Configuración física de referencia

| Parámetro | Delantero | Trasero |
|---|---:|---:|
| Masa de rueda | 22.8 kg | 26.5 kg |
| Radio | 0.3302 m | 0.3302 m |
| Recorrido máximo de suspensión | 0.0547 m | 0.0340 m |
| Fuerza máxima del muelle | 3545 N | 4395 N |
| Amortiguación en compresión | 2500 N·s/m | 3000 N·s/m |
| Amortiguación en extensión | 3500 N·s/m | 4000 N·s/m |

Masa total configurada del vehículo: `866 kg`.

## Modelo y escena

El `.unitypackage` utilizado durante el desarrollo incluye también Wheel Controller 3D y otros recursos externos, por lo que no se publica directamente tal como fue exportado.

Por el momento tampoco se incluyen el CAD, los modelos FBX, prefabs ni la escena completa del Panter. Su publicación queda pendiente de comprobar que pueden distribuirse de forma independiente de los recursos de terceros utilizados durante el desarrollo.

## Guía de ejecución

La activación de scripts, los comandos de ROS 2, el funcionamiento sin daemon y la configuración de cada modo se describen en [`../docs/GUIA_EJECUCION.md`](../docs/GUIA_EJECUCION.md).
