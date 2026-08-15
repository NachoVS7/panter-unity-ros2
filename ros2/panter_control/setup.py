from setuptools import find_packages, setup

package_name = 'panter_control'

setup(
    name=package_name,
    version='1.0.0',
    packages=find_packages(exclude=['test']),
    data_files=[
        ('share/ament_index/resource_index/packages', ['resource/' + package_name]),
        ('share/' + package_name, ['package.xml']),
    ],
    install_requires=['setuptools'],
    zip_safe=True,
    maintainer='Ignacio Viera',
    maintainer_email='ivs7viera@gmail.com',
    description='ROS 2 control nodes for the Panter Unity simulation.',
    license='Proprietary',
    entry_points={
        'console_scripts': [
            'panter_ackermann_mapper = panter_control.panter_ackermann_mapper:main',
            'panter_skid_mapper = panter_control.panter_skid_mapper:main',
            'panter_wheel_velocity_controller = panter_control.panter_wheel_velocity_controller:main',
            'panter_ackermann_velocity_mapper = panter_control.panter_ackermann_velocity_mapper:main',
            'panter_skid_velocity_mapper = panter_control.panter_skid_velocity_mapper:main',
        ],
    },
)
