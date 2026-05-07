# Super Smash Blocks

Juego de combate local 2.5D estilo plataforma inspirado en Super Smash Bros, desarrollado en Unity. Enfrenta a Batman, Joker y Red Hood en escenarios dinámicos con mecánicas de combate, combos, pickups y más.

---

## Tabla de contenidos

- [Vista general](#vista-general)
- [Requisitos](#requisitos)
- [Instalación y apertura del proyecto](#instalación-y-apertura-del-proyecto)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Escenas](#escenas)
- [Personajes](#personajes-jugables)
- [Controles e Inputs](#controles-e-inputs)
- [Scripts principales](#scripts-principales)
- [Prefabs y assets](#prefabs-y-assets)
- [Ejecución y build](#ejecución-y-build)
- [Desarrollo y contribución](#desarrollo-y-contribución)
- [Notas y TODOs](#notas-y-todos)

---

## Vista general

**Super Smash Blocks** es un juego de combate local para 2 jugadores. Los jugadores eligen entre tres personajes del universo DC, seleccionan un escenario y se enfrentan en peleas hasta agotar vidas. Incluye pickups de vida, plataformas unidireccionales, mecánicas de knockback, esquiva e invulnerabilidad.

| Campo | Detalle |
| ------- | --------- |
| Motor | Unity 2021.3 LTS o superior |
| Género | Plataformas / Combate 2.5D |
| Jugadores | 2 (local) |
| Plataforma objetivo | PC / Mac |
| Lenguaje | C# |

---

## Requisitos

- **Unity** 6.0.4.0f1 (ver versión exacta en `ProjectSettings/ProjectVersion.txt`)
- **Paquetes Unity** requeridos:
  - `Input System` — para gestión de controles y bindings
  - `TextMeshPro` — para textos de UI y HUD

> ⚠️ Si al abrir el proyecto faltan paquetes, ve a **Window → Package Manager** e instálalos manualmente.

---

## Instalación y apertura del proyecto

1. Clonar el repositorio:

   ```bash
   git clone https://github.com/joackagui/Super-Smash-Blocks.git
   ```

2. Abrir **Unity Hub**.

3. Hacer clic en **Add** y seleccionar la carpeta raíz del repositorio clonado.

4. Asegurarse de que la versión de Unity coincide con la indicada en `ProjectSettings/ProjectVersion.txt`. Si no está instalada, Unity Hub la detectará y ofrecerá descargarla.

5. Abrir el proyecto y luego cargar la escena principal:

   ```text
   Assets/Scenes/MainMenuScene.unity
   ```

6. Verificar en el inspector que los objetos `GameManager` y `MusicManager` estén presentes en la escena o se instancien correctamente por código.

---

## Estructura del proyecto

```text
Assets/
├── Audios/          # Música y efectos de sonido
├── Images/          # Sprites, íconos y texturas
├── Inputs/          # Archivos .inputactions con bindings por jugador
├── Prefabs/         # Personajes, armas, pickups y plataformas
├── Scenes/          # Todas las escenas del juego
└── Scripts/         # Lógica del juego (personajes, managers, UI, utilidades)
```

---

## Escenas

| Escena | Ruta | Descripción |
| ------- | ------ | ------------- |
| Menú principal | `Assets/Scenes/MainMenuScene.unity` | Introducción y transición a selección. Usa `MainMenu.cs`. |
| Selección de personaje | `Assets/Scenes/CharacterSelectionScene.unity` | Cada jugador elige su personaje. |
| Selección de escenario | `Assets/Scenes/StageSelectionScene.unity` | Grid de escenarios. Controlado por `StageSelection.cs`. |
| Controles | `Assets/Scenes/ControlsScene.unity` | Pantalla informativa de controles antes de la pelea. Usa `ControlsSceneManager.cs`. |
| Combate | `Assets/Scenes/FightScene.unity` | Escena principal de pelea. `GameManager` instancia personajes y administra la partida. |
| Victoria | `Assets/Scenes/VictoryScene.unity` | Muestra al ganador. `WinnerScreenUI` gestiona el retorno al menú. |
| Test | `Assets/Scenes/Test.unity` | Escena de pruebas internas. |

---

## Personajes jugables

Los tres personajes del juego heredan de la clase base `Character`. Actualmente no tienen lógica diferenciada, pero están preparados para recibir habilidades y animaciones únicas.

| Personaje | Prefab | Clase |
| ----------- | -------- | ------- |
| Batman | `Assets/Prefabs/Batman.prefab` | `Batman.cs` |
| Joker | `Assets/Prefabs/Joker.prefab` | `Joker.cs` |
| Red Hood | `Assets/Prefabs/RedHood.prefab` | `RedHood.cs` |

---

## Controles e Inputs

Los assets de input se encuentran en `Assets/Inputs/`. Se configuran mediante el **Input System** de Unity y soportan teclado y gamepad.

| Acción | Descripción |
| ------- | ------------- |
| `Move` / `Movement` | Movimiento horizontal (Vector2) |
| `Left` / `Right` | Alternativas digitales para moverse |
| `Jump` / `Up` | Salto |
| `Action1` / `Action2` | Ataques |
| `Dodge` | Esquiva |
| `Return` / `Back` / `Select` | Entradas de menú |

Para reasignar controles, editar los archivos `.inputactions` en `Assets/Inputs/` desde el editor de Unity o directamente con el Input System.

---

## Scripts principales

### `Character.cs`

Clase base de todos los personajes. Gestiona:

- Movimiento horizontal y saltos (incluyendo multisalto)
- Sistema de ataques y combos
- Recepción de daño, knockback e invulnerabilidad temporal
- Muerte y respawn

### `Player.cs`

Conecta el `InputActionAsset` con el componente `Character`. Gestiona las vidas del jugador y actualiza el HUD de daño.

### `GameManager.cs`

Singleton que controla el flujo completo de la partida:

- Registra los jugadores activos
- Instancia los personajes a partir de prefabs configurados
- Detecta condición de victoria y gestiona transiciones de escena

### `CameraController.cs`

Mantiene a ambos personajes en pantalla. Centra la cámara entre ellos y ajusta el FOV dinámicamente según la distancia entre jugadores.

### `UIManager.cs`

Administra el HUD en tiempo real:

- Indicadores de vidas (corazones)
- Imágenes de personajes
- Fondo de escenario

### `MusicManager.cs`

Controla la reproducción de música por escena y los efectos de sonido (SFX).

### `Hitbox.cs`

Detecta colisiones activas durante ataques y aplica daño. Diferencia entre hitboxes de mano (`hand`) y pie (`foot`).

### `HeartSpawner.cs`

Genera pickups de vida en el escenario según el estado de vidas de los jugadores.

---

## Prefabs y assets

### Personajes

- `Batman.prefab`
- `Joker.prefab`
- `RedHood.prefab`

### Objetos y escenario

- `Batarang.prefab` — Proyectil de Batman
- `Gun.prefab` — Arma de Red Hood
- `HeartPickup.prefab` — Pickup de vida
- `Ground.prefab` — Suelo estándar
- `Barrier.prefab` — Barrera de límite del escenario

### Recursos multimedia

- **Imágenes:** `Assets/Images/`
- **Audio:** `Assets/Audios/`

---

## Ejecución y build

### Ejecución en el editor

1. Abrir la escena `MainMenuScene` desde `Assets/Scenes/`.
2. Presionar el botón **Play**.
3. Verificar que `GameManager` y `MusicManager` están presentes en la escena.
4. Asegurarse de que cada objeto `Player` tiene asignado un `InputActionAsset` válido en el inspector.

### Build para PC / Mac

1. **File → Build Settings**
2. Hacer clic en **Add Open Scenes** (o agregar manualmente todas las escenas en el orden correcto).
3. Seleccionar plataforma: **PC, Mac & Linux Standalone**.
4. Configurar la arquitectura objetivo (x86_64 recomendado).
5. Hacer clic en **Build and Run**.

> 💡 Para Mac con chip Apple Silicon (M1/M2/M3/M5), seleccionar arquitectura **Apple Silicon** o **Universal** para mejor rendimiento.

---

## Desarrollo y contribución

### Estilo de código

- Nombres de tipos públicos y métodos: `PascalCase`
- Campos privados: `camelCase` (con prefijo `_` opcional: `_myField`)
- Seguir las convenciones estándar de C# en proyectos Unity

### Flujo de trabajo con Git

1. Crear una rama por feature o fix:

   ```bash
   git checkout -b feature/nombre-del-feature
   ```

2. Realizar commits con mensajes descriptivos en español o inglés (consistente con el equipo).
3. Abrir un Pull Request hacia `main` con descripción del cambio.
4. Esperar revisión antes de hacer merge.

### Reporte de bugs

Abrir un **Issue** en el repositorio con:

- Descripción del problema
- Pasos para reproducirlo
- Versión de Unity utilizada
- Capturas o videos si aplica

---

## Notas y TODOs

- [ ] `Assets/Scripts/Throwable.cs` está vacío — implementar si se requieren proyectiles genéricos lanzables.
- [ ] Las clases `Batman`, `Joker` y `RedHood` extienden `Character` sin lógica propia — agregar habilidades especiales, animaciones únicas y estadísticas diferenciadas.
- [ ] Crear `CONTRIBUTING.md` con pasos de setup, normas de commit y guía de estilo.
- [ ] Considerar añadir tests con **Unity Test Framework** para lógica crítica (daño, knockback, condición de victoria).
- [ ] Agregar soporte para más de 2 jugadores si se desea expandir.
- [ ] Implementar pantalla de pausa durante el combate.
