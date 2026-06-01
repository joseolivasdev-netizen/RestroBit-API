# 🍽 RestroBit API (En Desarrollo Activo)

RestroBit es el backend robusto de un sistema ERP diseñado para la gestión integral de restaurantes. Esta API RESTful maneja toda la lógica de negocio central, seguridad y procesamiento de datos, lista para ser consumida por clientes móviles (Android) y paneles administrativos.

##  Stack Tecnológico

* **Framework:** C# / .NET
* **Base de Datos:** PostgreSQL
* **Infraestructura:** Docker y Docker Compose
* **Patrones:** Uso de DTOs (Data Transfer Objects) para la separación de datos y seguridad.
* **JWT** BYCRIPT.

##  Módulos y Funcionalidades (WIP)

El sistema está estructurado en múltiples módulos operativos:

- **🔐 Autenticación y Seguridad:** Gestión de usuarios, roles, login y recuperación de contraseñas.
- **📦 Inventario y Menú:** Administración completa de categorías y productos.
- **🍽️ Operación de Sala:** Gestión de mesas, destinos y procesamiento de comandas.
- **💰 Finanzas:** Control de cuentas y módulo para cortes de caja.

##  Despliegue Local con Docker

Para levantar la base de datos PostgreSQL en un entorno local, asegúrate de tener Docker instalado y ejecuta el siguiente comando en la raíz del proyecto:

```bash
docker-compose up -d