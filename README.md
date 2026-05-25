# ⏱ TimeTracker Pro

> Aplicación de escritorio para Windows que permite controlar el tiempo y los gastos de tus proyectos de forma sencilla y portable.

---

## 📋 Tabla de contenidos

- [Descripción](#descripción)
- [Características](#características)
- [Tecnologías](#tecnologías)
- [Requisitos](#requisitos)
- [Instalación](#instalación)
- [Uso](#uso)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Portabilidad](#portabilidad)
- [Roadmap](#roadmap)
- [Licencia](#licencia)

---

## Descripción

**TimeTracker Pro** es una aplicación WPF para Windows desarrollada con .NET 8 que permite gestionar proyectos, registrar el tiempo trabajado mediante cronómetro o entrada manual, controlar gastos por categorías y generar informes PDF detallados.

Los datos se almacenan localmente en un único archivo SQLite portable, lo que facilita la migración entre equipos sin necesidad de servidores ni configuraciones adicionales.

---

## Características

- ✅ Gestión completa de proyectos (Activo, Pausado, Completado, Archivado)
- ✅ Estructura jerárquica: proyectos → secciones → subsecciones
- ✅ Cronómetro en tiempo real con recuperación automática de sesión al reiniciar
- ✅ Entrada manual de sesiones con fecha y duración
- ✅ Horas estimadas por sección con cálculo de desviación automático
- ✅ Fechas límite con indicador visual de desviación por colores
- ✅ Módulo de gastos con categorías personalizables
- ✅ Generación de informes PDF con QuestPDF
- ✅ Edición inline en todas las vistas
- ✅ Base de datos local SQLite — sin dependencias externas

---

## Tecnologías

| Tecnología | Uso |
|---|---|
| WPF + .NET 8 | Framework de interfaz de usuario |
| MVVM | Patrón de arquitectura |
| SQLite | Base de datos local |
| Dapper | ORM ligero para acceso a datos |
| QuestPDF | Generación de informes PDF |
| Visual Studio 2022 | Entorno de desarrollo |

---

## Requisitos

- Windows 10 / 11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Instalación

```bash
git clone https://github.com/tu-usuario/TimeTrackerPro.git
cd TimeTrackerPro
```

Abre `TimeTrackerPro.sln` en Visual Studio 2022 y ejecuta el proyecto. La base de datos se crea automáticamente en el primer arranque.

---

## Uso

### Crear un proyecto
1. Haz clic en **＋ Nuevo proyecto**.
2. Introduce el nombre, descripción, fecha límite y horas estimadas.
3. Guarda — el proyecto aparecerá en la lista.

### Añadir secciones
1. Selecciona un proyecto de la lista.
2. Haz clic en **＋ Sección** en el panel derecho.
3. Introduce un nombre y las horas estimadas.
4. Usa **＋ Sub** en cualquier sección para añadir subsecciones.

### Registrar tiempo
- **Cronómetro**: haz clic en **▶ Iniciar** en una sección y **⏹ Parar** al terminar.
- **Manual**: haz clic en **＋ Manual**, introduce la fecha y el rango horario.

### Gastos
1. Abre un proyecto y accede al panel de **Gastos**.
2. Haz clic en **＋ Gasto**, introduce concepto, importe y categoría.

### Informe PDF
- Haz clic en **📄 Informe PDF** en la cabecera del proyecto.
- Elige la ruta de destino y el informe se genera y abre automáticamente.

---

## Estructura del proyecto

```
TimeTrackerPro/
├── Models/           # Clases de datos: Project, Section, WorkSession, Expense
├── ViewModels/       # Lógica de UI siguiendo el patrón MVVM
├── Views/            # Ventanas y controles de usuario XAML
├── Services/         # TimerService y lógica de negocio
├── Repositories/     # Acceso a SQLite mediante Dapper
├── Infrastructure/   # DatabaseContext e inicialización
├── Helpers/          # RelayCommand, Converters y utilidades
└── Assets/           # Estilos y recursos visuales
```

---

## Portabilidad

Para mover todos tus datos a otro equipo:

1. Localiza el archivo de base de datos:
   ```
   C:\Users\[usuario]\AppData\Roaming\TimeTrackerPro\timetracker.db
   ```
2. Cópialo al mismo directorio en el nuevo equipo.
3. Abre la aplicación — todos tus proyectos estarán disponibles.

> Las versiones futuras incluirán exportación e importación JSON directamente desde la interfaz.

---

## Roadmap

- [x] Gestión de proyectos, secciones y subsecciones
- [x] Cronómetro y registro manual de sesiones
- [x] Recuperación automática de sesión al reiniciar
- [x] Estimación automática de fechas de finalización
- [x] Edición inline de proyectos y secciones
- [x] Módulo de gastos con categorías
- [x] Generación de informes PDF
- [ ] Exportación / importación JSON desde la UI
- [x] Soporte de temas claro y oscuro

---

## Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.

---
---

# ⏱ TimeTracker Pro

> A Windows desktop application for tracking project time and expenses — simple, local, and portable.

---

## 📋 Table of contents

- [Description](#description)
- [Features](#features)
- [Tech stack](#tech-stack)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Project structure](#project-structure)
- [Portability](#portability)
- [Roadmap](#roadmap)
- [License](#license)

---

## Description

**TimeTracker Pro** is a WPF desktop application for Windows built with .NET 8. It lets you manage projects, log worked hours via stopwatch or manual entry, track expenses by category, and generate detailed PDF reports.

All data is stored locally in a single portable SQLite file — no server, no cloud, no extra configuration needed.

---

## Features

- ✅ Full project lifecycle management (Active, Paused, Completed, Archived)
- ✅ Hierarchical structure: projects → sections → subsections
- ✅ Real-time stopwatch with automatic session recovery on restart
- ✅ Manual session entry with date and duration
- ✅ Estimated hours per section with automatic deviation tracking
- ✅ Deadline dates with color-coded deviation indicators
- ✅ Expense module with customizable categories
- ✅ PDF report generation powered by QuestPDF
- ✅ Inline editing across all views
- ✅ Local SQLite database — no external dependencies

---

## Tech stack

| Technology | Purpose |
|---|---|
| WPF + .NET 8 | UI framework |
| MVVM | Architecture pattern |
| SQLite | Local database |
| Dapper | Lightweight ORM |
| QuestPDF | PDF report generation |
| Visual Studio 2022 | Development environment |

---

## Requirements

- Windows 10 / 11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Installation

```bash
git clone https://github.com/tu-usuario/TimeTrackerPro.git
cd TimeTrackerPro
```

Open `TimeTrackerPro.sln` in Visual Studio 2022 and run the project. The database is created automatically on first launch.

---

## Usage

### Create a project
1. Click **＋ New project**.
2. Enter the name, description, deadline and estimated hours.
3. Save — the project will appear in the list.

### Add sections
1. Select a project from the list.
2. Click **＋ Section** in the right panel.
3. Enter a name and estimated hours.
4. Use **＋ Sub** on any section to add subsections.

### Track time
- **Stopwatch**: click **▶ Start** on a section and **⏹ Stop** when done.
- **Manual**: click **＋ Manual**, enter the date and time range.

### Expenses
1. Open a project and go to the **Expenses** panel.
2. Click **＋ Expense**, enter the concept, amount and category.

### PDF Report
- Click **📄 PDF Report** in the project header.
- Choose a destination path — the report is generated and opened automatically.

---

## Project structure

```
TimeTrackerPro/
├── Models/           # Data classes: Project, Section, WorkSession, Expense
├── ViewModels/       # UI logic following the MVVM pattern
├── Views/            # XAML windows and user controls
├── Services/         # TimerService and business logic
├── Repositories/     # SQLite access via Dapper
├── Infrastructure/   # DatabaseContext and initialization
├── Helpers/          # RelayCommand, Converters, and utilities
└── Assets/           # Styles and visual resources
```

---

## Portability

To move all your data to another machine:

1. Locate the database file:
   ```
   C:\Users\[username]\AppData\Roaming\TimeTrackerPro\timetracker.db
   ```
2. Copy it to the same directory on the new machine.
3. Open the app — all your projects will be there.

> Future versions will include direct JSON export and import from the UI.

---

## Roadmap

- [x] Project, section, and subsection management
- [x] Stopwatch and manual session tracking
- [x] Automatic session recovery on restart
- [x] Automatic completion date estimation
- [x] Inline editing of projects and sections
- [x] Expense tracking module with categories
- [x] PDF report generation
- [ ] JSON export / import from the UI
- [x] Light and dark theme support

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
