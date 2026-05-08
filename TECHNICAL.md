# Documentación Técnica del Juego

## Información general

- Motor: Unity 6.0.4.0f1
- Lenguaje: C#
- Plataforma objetivo: PC / Mac
- Tipo de juego: Combate local 2.5D para hasta 2 jugadores
- Sistema de entrada: Unity Input System
- Formato de proyecto: Unity estándar con assets en `Assets/`

## Paquetes principales

- `Input System` — gestión de dispositivos, bindings y acciones de jugador
- `TextMeshPro` — textos de interfaz, HUD y elementos de UI

## Escenas principales

- `Assets/Scenes/MainMenuScene.unity`
- `Assets/Scenes/CharacterSelectionScene.unity`
- `Assets/Scenes/StageSelectionScene.unity`
- `Assets/Scenes/ControlsScene.unity`
- `Assets/Scenes/FightScene.unity`
- `Assets/Scenes/VictoryScene.unity`
- `Assets/Scenes/TestScene.unity`

## Inputs y bindings

Archivos de input:

- `Assets/Inputs/GeneralInput.inputactions`
- `Assets/Inputs/Player1Input.inputactions`
- `Assets/Inputs/Player2Input.inputactions`

Acciones usadas por `Player.cs`:

- `Move` / `Movement` — movimiento horizontal
- `Left` / `Right` — control digital alternativo para mover
- `Jump` / `Up` — salto
- `Action1` — ataque principal
- `Action2` — ataque secundario / especial
- `Dodge` — esquiva
- `Return` / `Back` / `Select` — navegación de menú

## Arquitectura de juego

### `GameManager.cs`

- Singleton global con `DontDestroyOnLoad`
- Controla el flujo de juego general y las transiciones de escena
- Registra jugadores (`Player`) y resuelve instancias en `FightScene`
- Almacena selecciones de personajes, escenario y dificultad
- Administra condición de victoria y carga de escenas de combate / victoria
- Inicializa la escena de pelea con `SpawnCharacter(...)` y secuencia de intro

### `Player.cs`

- Componente que representa al jugador lógico
- Gestiona inputs mediante `InputActionAsset` y `InputActionMap`
- Expuesta lógica de vidas, HUD de daño y puntos de spawn
- Encapsula la referencia al `Character` actual y su posicionamiento
- Contiene `PlayerInputDeviceRouter` para asignar dispositivos según el slot del jugador:
  - 2 mandos: cada jugador usa su propio gamepad
  - 1 mando: Player1 usa mando + teclado/ratón, Player2 usa teclado/ratón
  - 0 mandos: ambos jugadores usan teclado + ratón
- Lee movimiento, salto, ataque y esquiva desde `Update()` si el personaje puede actuar

### `Character.cs`

- Clase base de personajes con física y animación
- Gestiona:
  - movimiento horizontal con `speed`
  - salto con `jumpForce` y doble salto
  - ataques en combo (`Attack1`, `Attack2`)
  - estados de invulnerabilidad, daño y knockback
  - animaciones de hurt, death y dodge
  - colisiones con hitboxes y desactivación de ataque
- Implementa cola de ataques y control de combo con tiempos de reset
- Calcula knockback en función de daño acumulado
- Soporta `LongDistanceAttack()` virtual para ataques a distancia en subclases

### Subclases de personajes

- `Batman.cs`
- `Joker.cs`
- `RedHood.cs`

Estas extensiones heredan `Character` y sirven como puntos de extensión para habilidades específicas.

### `HitBox.cs`

- Componente de hitbox que detecta colisiones de ataque
- Tipos:
  - `Hand` (daño base 7)
  - `Foot` (daño base 10)
- Usa `OnTriggerEnter` / `OnTriggerStay` para detectar objetivos
- Verifica línea de fuego con `Physics.RaycastAll(...)` para evitar interacciones a través de paredes
- Evita hits repetidos con `HashSet<Character>` por ataque activo
- Llama a `target.TakeDamage(...)` cuando el hit es válido

### `CameraController.cs`

- Sigue a los personajes usando un punto medio dinámico
- Ajusta `fieldOfView` en tiempo real según la distancia entre personajes
- Aplica límites de cámara en X/Y y mantiene Z fijo
- Refresca la lista de personajes cada cierto `refreshRate`

### `UIManager.cs`

- Administra HUD de vida, imágenes de personajes y fondo de escenario
- Actualiza corazones según vidas actuales de los jugadores
- Mapea selección de escenario a texturas y colores de plataforma
- Soporta retorno al menú mediante acción de input configurada

### `MusicManager.cs`

- Singleton de música y SFX con `DontDestroyOnLoad`
- Reproduce música por escena:
  - `mainMenuMusic` en menú principal
  - `fightMusic` en combate
  - `victoryMusic` / temas de personaje en victoria
- Controla volumen de música y efectos en `Update()` para ajustar cambios en tiempo real
- Reproduce sonidos de menú y efectos de personaje

### `HeartSpawner.cs`

- Instancia `HeartPickup` cuando ambos jugadores tienen 2 vidas o menos
- Solo genera un pickup por instancia (`hasSpawned`)
- Usa `GameManager.Instance` para leer el estado de vidas

### Manejo de escenas y UI de menú

- `MainMenu.cs`
  - Secuencia de intro y fade-in
  - Navegación entre opciones de menú
  - Selección de modo singleplayer/multiplayer
  - Carga de `CharacterSelectionScene`

- `CharacterSelection.cs`
  - Cuadrícula de selección de personajes para ambos jugadores
  - Soporte de dificultad en modo singleplayer
  - Input routing para ambos jugadores y selección/desselección
  - Actualiza previsualización de personaje y textos de UI

- `StageSelection.cs`
  - Selección de escenario en cuadrícula 2x2
  - Previsualización de escenario seleccionado
  - Carga de `FightScene` con control de build index basado en primera carga

- `ControlsSceneManager.cs`
  - Muestra controles antes de la pelea
  - Fade y blink de texto
  - Transición simple hacia `FightScene`

## Prefabs y recursos técnicos importantes

- Personajes:
  - `Assets/Prefabs/Batman.prefab`
  - `Assets/Prefabs/Joker.prefab`
  - `Assets/Prefabs/RedHood.prefab`
- Proyectiles / armas:
  - `Assets/Prefabs/Batarang.prefab`
  - `Assets/Prefabs/Gun.prefab`
- Pickups y objetos de escenario:
  - `Assets/Prefabs/HeartPickup.prefab`
  - `Assets/Prefabs/Ground.prefab`
  - `Assets/Prefabs/Barrier.prefab`
- Recursos multimedia:
  - `Assets/Audios/`
  - `Assets/Images/`

## Flujo de juego técnico

1. `MainMenuScene` → selección de modo
2. `CharacterSelectionScene` → elección de personajes
3. `StageSelectionScene` → elección de escenario
4. `ControlsScene` → pantalla de controles
5. `FightScene` → combate activo
6. `VictoryScene` → resultado final

## Consideraciones técnicas

- El juego está diseñado para combate local con soporte explícito para 2 jugadores.
- El sistema de input prioriza gamepads cuando están conectados.
- `GameManager` y `MusicManager` perduran entre escenas para conservar estado global.
- El combate depende de físicas de Unity (`Rigidbody`, `Collider`, `Physics`) y animaciones controladas por parámetros hash.
- La lógica de daño y knockback está ligada a la acumulación de daño y la orientación entre atacante/objetivo.

## Archivos relevantes

- `ProjectSettings/ProjectVersion.txt` — versión exacta de Unity
- `Assets/Scripts/Character.cs`
- `Assets/Scripts/Player.cs`
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/HitBox.cs`
- `Assets/Scripts/CameraController.cs`
- `Assets/Scripts/UIManager.cs`
- `Assets/Scripts/MusicManager.cs`
- `Assets/Scripts/MainMenu.cs`
- `Assets/Scripts/CharacterSelection.cs`
- `Assets/Scripts/StageSelection.cs`
- `Assets/Scripts/ControlsSceneManager.cs`
- `Assets/Scripts/HeartSpawner.cs`

## Notas de extensión

- `Throwable.cs` existe como archivo base para proyectiles y puede usarse para generalizar ataques a distancia.
- Las clases de personajes actuales están listas para añadir habilidades únicas y estadísticas diferenciadas.
- El diseño actual asume hasta 2 jugadores y puede extenderse con más slots y lógica de input en `PlayerInputDeviceRouter`.
