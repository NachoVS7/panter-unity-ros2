# Dependencias

Este repositorio contiene únicamente los archivos desarrollados o adaptados para el TFM que pueden distribuirse. Las dependencias externas deben instalarse por separado.

## ROS 2

El desarrollo se ha realizado sobre **ROS 2 Humble** en Ubuntu 22.04 ejecutado mediante WSL.

Dependencias principales:

- ROS 2 Humble.
- `geometry_msgs`.
- `std_msgs`.
- `sensor_msgs`.
- `nav_msgs`.
- ROS-TCP-Endpoint.

## Unity

El proyecto utiliza Unity como motor de simulación física y visual.

Dependencias principales:

- ROS-TCP-Connector.
- Wheel Controller 3D, de NWH Coding.

### Wheel Controller 3D

Wheel Controller 3D es un recurso comercial distribuido mediante Unity Asset Store. No forma parte del código desarrollado en este TFM y **no se redistribuye en este repositorio**.

Para reproducir el proyecto es necesario disponer legalmente de esta dependencia e importarla en el proyecto Unity antes de configurar el vehículo.

## Modelo 3D del Panter

La distribución pública de los archivos CAD/FBX originales del vehículo depende de los permisos asociados al modelo mecánico. Por este motivo, dichos archivos solo se incorporarán al repositorio si se confirma que pueden publicarse.

El código y la documentación pueden consultarse independientemente de estos recursos.
