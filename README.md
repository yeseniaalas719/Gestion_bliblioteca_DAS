📚 Sistema de Gestión de Biblioteca (BibliotecaApp)
Este proyecto es una aplicación de escritorio desarrollada en C# con Windows Forms para la administración eficiente de libros, usuarios y préstamos. El sistema cuenta con persistencia de datos automática mediante archivos JSON.

🛠️ Instalación

**Clonar el repositorio:**

Bash
git clone https://github.com/tu-usuario/BibliotecaApp.git

**Abrir en Visual Studio:**

Abre Visual Studio.
Selecciona "Abrir un proyecto o solución" y elige el archivo DASYE1.sln.

Restaurar Dependencias:

El proyecto utiliza System.Text.Json para la persistencia. Visual Studio restaurará los paquetes NuGet automáticamente al compilar.

🚀 Guía de Uso

La aplicación está dividida en tres módulos principales:

1-Gestión de Libros: Permite registrar títulos, autores, ISBN y cantidad de copias. El sistema calcula automáticamente la disponibilidad.

2-Gestión de Usuarios: Registro de lectores mediante nombre, documento y correo electrónico.

3-Préstamos y Devoluciones: El sistema valida que existan copias disponibles antes de prestar.

**No permite que un usuario preste el mismo libro dos veces simultáneamente**

Indicadores Visuales (Colores):

🟨 Amarillo: Préstamos activos o libros con stock bajo.
🟩 Verde: Libros devueltos correctamente.
🟥 Rojo: Libros sin copias disponibles (Agotados).

📦 Guía de Despliegue

Generar .exe final

1-Cambia la configuración de compilación a Release en Visual Studio.
2-Haz clic derecho sobre el proyecto DASYE1 en el Explorador de Soluciones y selecciona Publicar.
3-Configura el perfil para "Producir un solo archivo" en las opciones de publicación.
4-Ejecuta el archivo generado; el sistema creará automáticamente la carpeta /data para almacenar los archivos JSON de persistencia.
