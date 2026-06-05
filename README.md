# Hangman.Client

Aplicación de escritorio WPF que funciona como cliente del juego del Ahorcado, desarrollada como proyecto final de la materia *Tecnologías para la Construcción de Software* — Ingeniería de Software, UV FEI, FEB–JUL 2026.

Consume los servicios WCF expuestos por `Hangman.Server`.

## Tecnologías

- C# / .NET Framework 4.7.2
- WPF (Windows Presentation Foundation)
- WCF (Windows Communication Foundation) — consumo de servicios
- Visual Studio 2022
- Git / GitHub

## Estructura de la solución

```text
Hangman.Client
└── Hangman.Client.WPF\
    ├── Views\                <- Ventanas y UserControls (.xaml / .xaml.cs)
    ├── ViewModels\           <- Lógica de presentación (patrón MVVM)
    ├── ServiceProxies\       <- Proxies generados para consumir los servicios WCF del servidor
    ├── Models\               <- Modelos y DTOs locales del cliente
    ├── Resources\            <- Estilos, colores, fuentes, íconos
    └── Localization\         <- Archivos de recursos para ES / EN (.resx)
```

## Vistas principales

| Vista | Caso de uso |
|---|---|
| `LoginWindow` | CU-02 Iniciar sesión |
| `RegisterWindow` | CU-01 Registrar usuario |
| `MainMenuWindow` | Menú principal |
| `MatchListWindow` | CU-05 Ver partidas disponibles |
| `CreateMatchWindow` | CU-06 Crear nueva partida |
| `CategorySelectorWindow` | CU-15 Seleccionar categoría y palabra |
| `GameBoardWindow` | CU-08 Jugar partida |
| `ProfileWindow` | CU-03 Ver perfil / CU-04 Editar información |
| `ScoreWindow` | CU-10 Ver puntaje global / CU-11 Ver penalizaciones |

## Configuración del entorno

### Prerrequisitos

- Visual Studio 2022 con carga de trabajo **Desarrollo de escritorio .NET**
- .NET Framework 4.7.2
- El servidor `Hangman.Server` corriendo y accesible en red

### Clonar y abrir

```bash
git clone https://github.com/MZSM98/Hangman.Client.git
cd Hangman.Client
```

Abrir `Hangman.Client.sln` en Visual Studio 2022.

### Agregar referencia de servicio

Los proxies WCF se generan apuntando al servidor. Con el servidor corriendo:

1. Clic derecho sobre `Hangman.Client.WPF` → **Agregar** → **Referencia de servicio**
2. Ingresar la URL del servidor: `http://TU_SERVIDOR:8000/HangmanService/`
3. Visual Studio genera automáticamente las clases proxy en `ServiceProxies\`

### Configurar endpoint

La dirección del servidor se configura en `App.config`:

```xml
<endpoint address="http://TU_SERVIDOR:8000/HangmanService/"
          binding="basicHttpBinding"
          contract="ServiceProxies.IAuthService" />
```

Reemplazar `TU_SERVIDOR` con la IP o nombre del equipo donde corre `Hangman.Server`.

## Convención de ramas

| Rama | Uso |
|---|---|
| `main` | Versiones estables entregadas |
| `develop` | Integración continua del equipo |
| `feature/nombre-vista` | Desarrollo de una vista específica |
| `fix/descripcion` | Corrección de errores |

Ejemplo: `feature/game-board-window`, `feature/login-window`

## Relación con el servidor

Este repositorio es exclusivamente el cliente. El servidor WCF vive en:

> [https://github.com/tuzc0/Hangman.Server](https://github.com/tuzc0/Hangman.Server)

Ambos proyectos deben estar corriendo simultáneamente para que el juego funcione.

## Equipo

- Jorge Manuel Cobos Castro
- Marcos Zenón Sánchez Mendizábal
- Guillermo Velázquez Rosiles
- Claudio Trujillo Zepeda

**Docente:** Mtro. Ramón Gómez Romero  
**Materia:** Tecnologías para la Construcción de Software  
**Periodo:** FEB – JUL 2026
