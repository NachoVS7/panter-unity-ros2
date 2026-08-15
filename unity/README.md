# Unity — simulación del Panter

Esta carpeta contendrá los scripts propios y los archivos distribuibles necesarios para integrar el Panter con ROS 2.

## Función de Unity

Unity se encarga de:

- ejecutar la simulación física del vehículo;
- aplicar las consignas de dirección y par calculadas en ROS 2;
- obtener las velocidades angulares de las ruedas;
- obtener las cargas verticales de cada rueda;
- publicar odometría e información inercial simulada.

## Scripts principales

Los scripts empleados en la versión final incluyen, entre otros:

- `WheelTorqueCommandSubscriber.cs`;
- `SteeringCommandSubscriber.cs`;
- `WheelStatePublisher.cs`;
- `WheelLoadPublisher.cs`;
- scripts de publicación de odometría e IMU;
- scripts auxiliares de configuración del vehículo.

En los modos de velocidad por rueda, `/panter/wheel_velocity_cmd` permanece dentro de ROS 2. Unity recibe el resultado final del controlador mediante `/panter/wheel_torque_cmd`.

## Orden de las ruedas

Los vectores de cuatro valores siguen siempre el orden:

```text
[FL, FR, RL, RR]
```

## Dependencias externas

El proyecto necesita:

- ROS-TCP-Connector;
- Wheel Controller 3D.

Wheel Controller 3D no se redistribuye en este repositorio. Consulta [`../docs/dependencies.md`](../docs/dependencies.md).

## Configuración física de referencia

Parámetros principales utilizados en el modelo final:

| Parámetro | Delantero | Trasero |
|---|---:|---:|
| Masa de rueda | 22.8 kg | 26.5 kg |
| Radio | 0.3302 m | 0.3302 m |
| Recorrido máximo de suspensión | 0.0547 m | 0.0340 m |
| Fuerza máxima del muelle | 3545 N | 4395 N |
| Amortiguación en compresión | 2500 N·s/m | 3000 N·s/m |
| Amortiguación en extensión | 3500 N·s/m | 4000 N·s/m |

Masa total configurada del vehículo: `866 kg`.

## Archivos del modelo

Antes de publicar la escena, los prefabs o los modelos FBX/CAD se comprobará que puedan distribuirse y que no incluyan contenido perteneciente a Wheel Controller 3D u otras dependencias comerciales.
