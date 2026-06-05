# Hangman.Client.WPF — Estructura y guía de vistas

## Estructura de carpetas

```text
Hangman.Client.WPF\
├── App.xaml                          <- Recursos globales, arranque de la app
├── App.xaml.cs
├── Hangman.Client.sln
│
├── Views\
│   ├── Windows\
│   │   ├── LoginWindow.xaml          <- Punto de entrada de la app
│   │   ├── LoginWindow.xaml.cs
│   │   ├── RegisterWindow.xaml       <- Registro de nuevo usuario
│   │   ├── RegisterWindow.xaml.cs
│   │   ├── MainMenuWindow.xaml       <- Shell principal post-login
│   │   ├── MainMenuWindow.xaml.cs
│   │   └── GameBoardWindow.xaml      <- Tablero de juego (pantalla completa)
│   │       GameBoardWindow.xaml.cs
│   │
│   └── UserControls\
│       ├── MatchListView.xaml        <- CU-05 Lista de partidas disponibles
│       ├── MatchListView.xaml.cs
│       ├── CreateMatchView.xaml      <- CU-06 Crear nueva partida
│       ├── CreateMatchView.xaml.cs
│       ├── CategorySelectorView.xaml <- CU-15 Seleccionar categoría y palabra
│       ├── CategorySelectorView.xaml.cs
│       ├── ProfileView.xaml          <- CU-03 Ver perfil / CU-04 Editar
│       ├── ProfileView.xaml.cs
│       ├── ScoreView.xaml            <- CU-10 Puntaje global / CU-11 Penalizaciones
│       └── ScoreView.xaml.cs
│
├── ViewModels\
│   ├── Base\
│   │   ├── BaseViewModel.cs          <- INotifyPropertyChanged base
│   │   └── RelayCommand.cs           <- Implementación de ICommand
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── MainMenuViewModel.cs          <- Controla la navegación del ContentControl
│   ├── GameBoardViewModel.cs
│   ├── MatchListViewModel.cs
│   ├── CreateMatchViewModel.cs
│   ├── CategorySelectorViewModel.cs
│   ├── ProfileViewModel.cs
│   └── ScoreViewModel.cs
│
├── ServiceProxies\                   <- Generado por Visual Studio (Add Service Reference)
│   └── (archivos auto-generados al conectar con Hangman.Server)
│
├── Models\
│   └── (DTOs locales si se necesitan, distinto a los del servidor)
│
├── Resources\
│   ├── Styles\
│   │   ├── ButtonStyles.xaml
│   │   ├── TextStyles.xaml
│   │   └── WindowStyles.xaml
│   ├── Colors.xaml                   <- Paleta de colores de la app
│   └── Icons\
│
└── Localization\
    ├── Resources.es.resx             <- Textos en español
    └── Resources.en.resx             <- Textos en inglés
```

## Navegación dentro de MainMenuWindow

`MainMenuWindow` es la shell que permanece visible después del login. Contiene:

- Barra superior: nombre del usuario, puntaje acumulado, selector de idioma, botón cerrar sesión
- Área central: `ContentControl` que intercambia el `UserControl` activo

```xml
<!-- Esquema de MainMenuWindow.xaml -->
<Window>
    <Grid>
        <!-- Barra superior fija -->
        <StackPanel>
            <TextBlock Text="{Binding CurrentUser.FullName}"/>
            <TextBlock Text="{Binding CurrentUser.Score}"/>
            <ComboBox ItemsSource="{Binding Languages}" />
            <Button Command="{Binding LogoutCommand}"/>
        </StackPanel>

        <!-- Área de navegación -->
        <ContentControl Content="{Binding CurrentView}" />
    </Grid>
</Window>
```

`MainMenuViewModel` expone la propiedad `CurrentView` (de tipo `BaseViewModel`). Cambiar su valor intercambia la vista visible.

```csharp
// Ejemplo de navegación en MainMenuViewModel.cs
public void NavigateTo(BaseViewModel view)
{
    CurrentView = view;
}

// Uso:
NavigateTo(new MatchListViewModel());
NavigateTo(new ProfileViewModel());
```

En `App.xaml` se registran los `DataTemplate` para que WPF sepa qué `UserControl` renderizar por cada ViewModel:

```xml
<!-- App.xaml -->
<Application.Resources>
    <DataTemplate DataType="{x:Type vm:MatchListViewModel}">
        <views:MatchListView/>
    </DataTemplate>
    <DataTemplate DataType="{x:Type vm:CreateMatchViewModel}">
        <views:CreateMatchView/>
    </DataTemplate>
    <DataTemplate DataType="{x:Type vm:ProfileViewModel}">
        <views:ProfileView/>
    </DataTemplate>
    <DataTemplate DataType="{x:Type vm:ScoreViewModel}">
        <views:ScoreView/>
    </DataTemplate>
    <DataTemplate DataType="{x:Type vm:CategorySelectorViewModel}">
        <views:CategorySelectorView/>
    </DataTemplate>
</Application.Resources>
```

## Flujo de navegación entre ventanas

```
App arranca
    └── LoginWindow
            ├── [Registrarse] → RegisterWindow
            │       └── [Éxito / Cancelar] → LoginWindow
            └── [Login exitoso] → MainMenuWindow (shell)
                    ├── ContentControl muestra: MatchListView  (vista por defecto)
                    ├── [Crear partida] → ContentControl muestra: CreateMatchView
                    │       └── [Siguiente] → ContentControl muestra: CategorySelectorView
                    │               └── [Confirmar] → GameBoardWindow (nueva ventana)
                    ├── [Unirse a partida] → ContentControl muestra: MatchListView
                    │       └── [Unirse] → GameBoardWindow (nueva ventana)
                    ├── [Mi perfil] → ContentControl muestra: ProfileView
                    ├── [Ver puntaje] → ContentControl muestra: ScoreView
                    └── [Cerrar sesión] → LoginWindow
```

## Responsabilidades por vista

| Vista / Ventana | ViewModel | Responsabilidad |
|---|---|---|
| `LoginWindow` | `LoginViewModel` | Captura credenciales, valida campos, llama `AuthService` |
| `RegisterWindow` | `RegisterViewModel` | Captura datos del usuario, valida, llama `AuthService` |
| `MainMenuWindow` | `MainMenuViewModel` | Shell, navegación, datos del usuario en sesión |
| `MatchListView` | `MatchListViewModel` | Lista partidas disponibles, acción unirse |
| `CreateMatchView` | `CreateMatchViewModel` | Configuración inicial de partida nueva |
| `CategorySelectorView` | `CategorySelectorViewModel` | Selección de categoría, idioma y palabra (solo descripción visible) |
| `GameBoardWindow` | `GameBoardViewModel` | Tablero, teclado en pantalla, dibujo del ahorcado, intentos |
| `ProfileView` | `ProfileViewModel` | Ver y editar información personal |
| `ScoreView` | `ScoreViewModel` | Puntaje desglosado: victorias, defensas, penalizaciones |

## Convención de nombres

- Ventanas: sufijo `Window` → `LoginWindow`, `GameBoardWindow`
- UserControls: sufijo `View` → `MatchListView`, `ProfileView`
- ViewModels: sufijo `ViewModel` → `LoginViewModel`, `ProfileViewModel`
- Comandos en ViewModel: sufijo `Command` → `LoginCommand`, `SaveChangesCommand`
- Propiedades enlazadas: PascalCase → `FullName`, `SelectedCategory`

## Notas importantes

- En esta entrega las vistas van **sin conexión a servicios WCF**. Los ViewModels pueden usar datos de prueba hardcodeados.
- El `GameBoardWindow` debe manejar dos estados visuales: vista del Jugador 1 (observa letras seleccionadas) y vista del Jugador 2 (teclado activo para adivinar).
- La internacionalización ES/EN se maneja cambiando el `ResourceDictionary` activo desde `MainMenuViewModel` al cambiar idioma (CU-14).
