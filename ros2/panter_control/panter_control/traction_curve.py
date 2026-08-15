# Curva de fuerza de tracción del Panter.
# La curva relaciona velocidad del vehículo [km/h] con fuerza de tracción total disponible [N].
# Los puntos están aproximados a partir de la gráfica aportada.
# Si más adelante tienes la tabla exacta, solo hay que sustituir estos puntos.

TRACTION_CURVE = [
    (0.0, 14800.0),
    (15.0, 14800.0),
    (18.0, 14000.0),
    (22.0, 12500.0),
    (26.0, 10800.0),
    (30.0, 9500.0),
    (35.0, 8500.0),
    (40.0, 7600.0),
    (45.0, 6900.0),
    (50.0, 6400.0),
    (55.0, 6000.0),
    (60.0, 5600.0),
    (66.0, 5200.0),
    (72.0, 4800.0),
]


def clamp(value, min_value, max_value):
    return max(min(value, max_value), min_value)


def get_tractive_force(speed_kmh):
    """
    Devuelve la fuerza de tracción disponible [N] para una velocidad dada [km/h],
    usando interpolación lineal entre puntos de la curva.
    """
    speed_kmh = abs(speed_kmh)

    if speed_kmh <= TRACTION_CURVE[0][0]:
        return TRACTION_CURVE[0][1]

    if speed_kmh >= TRACTION_CURVE[-1][0]:
        return TRACTION_CURVE[-1][1]

    for i in range(len(TRACTION_CURVE) - 1):
        v0, f0 = TRACTION_CURVE[i]
        v1, f1 = TRACTION_CURVE[i + 1]

        if v0 <= speed_kmh <= v1:
            t = (speed_kmh - v0) / (v1 - v0)
            return f0 + t * (f1 - f0)

    return TRACTION_CURVE[-1][1]


def get_max_wheel_torque(speed_kmh, wheel_radius=0.3302, powered_wheel_count=4):
    """
    Convierte fuerza de tracción total [N] en par máximo por rueda [Nm].
    """
    tractive_force = get_tractive_force(speed_kmh)
    return tractive_force * wheel_radius / powered_wheel_count
