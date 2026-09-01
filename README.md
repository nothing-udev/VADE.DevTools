# VADE.DevTools

Библиотека **VADE** для ускорения разработки на Unity: реактивность, автосохранение,
DI, единая точка входа, кастомные атрибуты инспектора, утилиты, система UI-окон,
онбординг, локализация, звук, IAP и реклама (LevelPlay).

Код в этой версии — без комментариев; весь функционал, все нюансы и ограничения
описаны здесь.

## Установка

Git-репозиторий с `package.json` в корне — подключается через Unity Package Manager:

1. Window → Package Manager → `+` → **Add package from git URL**
2. `https://github.com/<ваш-аккаунт>/vade-devtools.git` (или `...#2.0.0` для тега)

**Newtonsoft.Json** — обязательная зависимость, объявлена в `package.json`, Unity
ставит её сама. **UGUI**/**TextMeshPro** — обязательны, уже прописаны в asmdef.
Всё остальное (DOTween, IAP, LevelPlay) — опционально, ставится
через `Tools/VADE/Setup Window` (см. ниже).

## Tools/VADE/Setup Window

Единая панель: `Tools/VADE/Setup Window`.

- **Сцена** — кнопка добавляет в текущую сцену `Bootstrap` (пустой объект, на
  который вешаете свой наследник), `UI Root` (Canvas + CanvasScaler +
  GraphicRaycaster + ScreensRoot/PopupsRoot + `WindowService`, уже связанные) и
  `EventSystem` (сама решает `InputSystemUIInputModule` или `StandaloneInputModule`).
  Идемпотентно — можно жать сколько угодно раз. То же самое доступно отдельным
  пунктом `Tools/VADE/Setup Scene` (вызывается ещё и автоматически при File →
  New Scene).
- **Опциональные зависимости** — по строке на DOTween/IAP/LevelPlay,
  у каждой индикатор (включено/нет) и кнопки Install/Remove. Под капотом —
  `Tools/VADE/Dependencies/...` (те же команды доступны и напрямую из меню).

Install для DOTween: ищет уже установленный DOTween (нет единого официального
UPM-пакета — Asset Store/сайт разработчика), прописывает ссылку на его сборку в
`Runtime/UGUI/VADE.DevTools.UGUI.asmdef` и включает define `VADE_DOTWEEN`. Если у
DOTween ещё нет своей asmdef — подскажет открыть его Utility Panel → "Create
ASMDEF...".

Install для IAP/LevelPlay: официальные UPM-пакеты, ставятся
через Package Manager Client автоматически, дальше — та же схема (asmdef-ссылка
+ define).

## Структура

```
VADE.DevTools/
├── package.json, README.md, CHANGELOG.md, LICENSE.md
├── Runtime/
│   ├── Core/VADE.DevTools.Core.asmdef          (без UGUI-зависимостей)
│   │   ├── Reactive/       Reactive<T>, ReactiveList, ReactiveDictionary,
│   │   │                   Connectable, ReactiveBehaviour/Object, UnityEventExtensions
│   │   ├── Persistence/    AutoSave<T>, хранилища, сериализация
│   │   ├── DI/             Dependency
│   │   ├── Bootstrap/      Bootstrap
│   │   ├── Attributes/     EditorButton, ShowIf/HideIf, ReadOnly, GeneratedId,
│   │   │                   SerializeReferenceList (сами атрибуты)
│   │   ├── Extensions/     ExtensionMethods, Dictionary/Math/Vector/Component
│   │   ├── Utilities/      Pool<T>, CoroutineRunner
│   │   ├── StateMachine/   StateMachine<TState>
│   │   ├── Audio/          AudioService/IAudioService, AudioLibrary, PooledAudioSource
│   │   ├── IAP/            ProductData, IAPService            (опц. VADE_IAP)
│   │   └── Ads/            IAdsService, AdsServiceLevelPlay, AdsConfig (опц. VADE_LEVELPLAY)
│   └── UGUI/VADE.DevTools.UGUI.asmdef           (ссылается на Core)
│       ├── UI/             Window, BaseWindow, PopupWindow, WindowService,
│       │                   IWindowFactory, ResourcesWindowFactory, ToggleInput
│       ├── Reactive/       ReactiveExtensions (BindTo для Text/TMP/Slider/Image/Button)
│       ├── Extensions/     UIListExtensions (Present/BindTo), ScrollRectExtensions,
│       │                   EventTriggerExtensions
│       ├── Localization/   LocalizationService, LocalizedText, LocalizationKeyAttribute
│       └── Onboarding/     OnboardingService + Core/Definitions/Components/
│                           Actions/Conditions/Pointers (см. раздел ниже)
└── Editor/VADE.DevTools.Editor.asmdef
    ├── Setup/          VADESetupWindow
    ├── Bootstrap/      Tools/VADE/Setup Scene
    ├── Dependencies/   DOTween/IAP/LevelPlay setup, AsmdefPatcher
    ├── Attributes/     дровер'ы ShowIf/HideIf/ReadOnly/GeneratedId/EditorButton/SerializeReferenceList
    ├── Localization/   таблица переводов, [LocalizationKey] дровер
    ├── Onboarding/     OnboardingServiceEditor, OnboardingMenuEditor, TaskIdDrawer
    ├── Utilities/      FindMissingScripts/FindNullReferences/FindStaticIssues/
    │                   MeshBaker/ReadWriteMeshFixer Editor
    └── Utils/          OptionalTypeUtility, AsmdefPatcher
```

---

## Reactive (Core)

```csharp
var health = new Reactive<int>(100);
health.OnChanged += hp => Debug.Log(hp);
health.value -= 20;

var inventory = new ReactiveList<Item>();
inventory.OnAdd += (item, index) => Debug.Log(item.Name);
```

`Connectable` — бак подписок с оператором `+=`:

```csharp
public class ShopPanel : ReactiveBehaviour
{
    [SerializeField] private Button buyButton;
    private void Start() => Connections += buyButton.Subscribe(() => Debug.Log("buy"));
}
```

`ReactiveBehaviour`/`ReactiveObject` сами вызывают `Connections.Dispose()` в
`OnDestroy`/`Dispose()`. Биндинги к UGUI/TMP (`BindTo`, двусторонний
`BindTwoWay`, `Present`/`BindTo` для списков) — в `VADE.DevTools.UGUI`, доступны
только там, где подключён UGUI.

## AutoSave\<T\> (Core/Persistence)

```csharp
var isGameStarted = new AutoSave<bool>("is_game_started", AutoSaveType.PlayerPrefs, false);
isGameStarted.value = true;
bool started = isGameStarted; // implicit operator

var stats = new AutoSave<Dictionary<string,int>>("stats", AutoSaveType.File, new());
stats.value["gold"] = 10;
stats.Flush(); // мутация коллекции на месте не триггерит автосейв сама — Flush() форсит запись
```

Сериализация по умолчанию — Newtonsoft.Json (Dictionary работает из коробки).
`JsonUtilitySerializer` — облегчённая альтернатива без Dictionary (`AutoSaveSerializer.Current = new JsonUtilitySerializer();`).

Версионирование (опционально):

```csharp
new AutoSave<PlayerData>("player", version: 2, migrate: (rawJson, savedVersion) => { /* ... */ });
```

Хранилище подменяемое: `AutoSaveStorage.FileStorage = new MyEncryptedFileStorage();`
или напрямую в конструктор конкретного `AutoSave<T>`.

## DI (Core/DI) — `Dependency`

```csharp
Dependency.Register<IAudioService>(new AudioService());
var audio = Dependency.Resolve<IAudioService>();
bool ok = Dependency.TryResolve<IAudioService>(out var s);
```

Простой статический service locator, живёт весь процесс. `Dependency.Clear()` —
если нужно сбрасывать между сценами.

## Bootstrap (Core/Bootstrap)

```csharp
public class GameBootstrap : Bootstrap
{
    protected override void RegisterDependencies()
    {
        Dependency.Register<IAudioService>(new AudioService());
    }

    protected override void Initialize() { /* синхронно */ }

    // либо вместо Initialize() — асинхронный вариант:
    protected override void Initialize(Action onComplete)
    {
        StartCoroutine(LoadConfigThenComplete(onComplete));
    }
}
```

Один `Bootstrap` на процесс (`DontDestroyOnLoad`, дубликаты уничтожаются с
предупреждением). `IsInitialized`/`event Initialized` — если другим системам
нужно дождаться готовности.

## Атрибуты (Core/Attributes + Editor/Attributes)

- **`[EditorButton]`** — кнопка в инспекторе у метода (`MonoBehaviour`/`ScriptableObject`,
  fallback-инспектор, не перебивает существующие `[CustomEditor]`). Поддерживает
  параметры `int`/`float`/`string`/`bool` — рисуются инлайн-полями перед кнопкой.
  ```csharp
  [EditorButton] private void Regenerate() { }
  [EditorButton("Дать золото", ButtonMode.PlayModeOnly)] private void DebugAddGold() { }
  [EditorButton] private void MoveTo(int task, int step) { }
  ```
  Мульти-редактирование для типов под этим fallback-инспектором не поддерживается
  (Unity покажет "Multi-object editing not supported") — навесьте свой
  `[CustomEditor(typeof(X))]` без `isFallback`, если это критично.

- **`[ShowIf]`/`[HideIf]`** — условная видимость поля по имени bool-члена
  (`nameof(...)`) в том же классе:
  ```csharp
  [ShowIf(nameof(useCustomColor))] public Color color;
  [ShowIf(nameof(mode), Mode.Advanced)] public float advancedParam;
  ```
  Не найден член — поле показывается (fail-open) + предупреждение в консоль
  (один раз на тип+имя).

- **`[ReadOnly]`** — поле видно, но не редактируется.

- **`[GeneratedId]`** — read-only строковое поле + кнопки Generate/Copy, авто-
  генерирует GUID при первом пустом значении. Используется для `TaskComponentBase.id`,
  `StepDefinition.key`, `TaskDefinition.key`.

- **`[SerializeReference, SerializeReferenceList(typeof(IMyInterface))]`** на
  `List<IMyInterface>` — полиморфный список без Odin: кнопка добавления с
  выпадающим меню всех неабстрактных реализаций интерфейса (через рефлексию по
  всем загруженным сборкам), смена типа элемента, up/down/remove. Пример — см.
  `StepDefinition.onAction`/`conditions` в Onboarding.

## Extensions (Core + UGUI)

`ExtensionMethods.cs` — списки/строки/трансформы (`GetRandom`, `Shuffle`,
`GetClamp`, `ToShortAmount`, `WrapText`, `TimeAgo`, `DestroyAllChild(Immediate)`,
`GetWorldRect`, `IsNullOrEmpty` и т.д.), `DictionaryExtensions.GetOrAdd/GetValueOrDefault`,
`MathExtensions.Remap/Approximately/RoundToNearest/PercentOf`,
`VectorExtensions.With/ToVector2XZ/ToVector3XZ`, `ComponentExtensions.GetOrAdd<T>()`.

В UGUI: `UIListExtensions.Present`/`BindTo` (пул вместо Destroy+Instantiate на
каждый элемент списка), `ScrollRectExtensions.ScrollToItemHorizontal`,
`EventTriggerExtensions.AddEvent`.

```csharp
items.Present(container, itemViewPrefab, (index, item, view) => view.SetData(item));
Connections += reactiveList.BindTo(container, itemViewPrefab, (i, item, view) => view.SetData(item));
```

## Utilities / StateMachine (Core)

```csharp
var bulletPool = new Pool<Bullet>(bulletPrefab, transform, prewarm: 20);
var bullet = bulletPool.Get();
bulletPool.Release(bullet);

var runner = someGameObject.AddComponent<CoroutineRunner>();
runner.Run(MyRoutine());

var fsm = new StateMachine<GameStage>(GameStage.MainMenu)
    .OnEnter(GameStage.Gameplay, () => Debug.Log("start"))
    .OnExit(GameStage.MainMenu, () => menuUI.Hide());
fsm.ChangeState(GameStage.Gameplay);
```

---

## UI: Window / WindowService (UGUI)

```csharp
public class ShopWindow : BaseWindow
{
    protected override void OnShow(object data) { base.OnShow(data); }
    protected override void OnHide() { }
}

WindowService.Instance.Open<ShopWindow>();
WindowService.Instance.Open<ConfirmPopup>(data); // popup определяется автоматически по типу (T : PopupWindow)
WindowService.Instance.Close<ShopWindow>();
WindowService.Instance.CloseTop();

var current = WindowService.Instance.CurrentWindow;
WindowService.Instance.WindowOpened += w => Debug.Log(w.name);
WindowService.Instance.WindowClosed += w => Debug.Log(w.name);

// показ строго по одному, не стеком — для наград/уведомлений:
WindowService.Instance.EnqueuePopup<RewardPopup>(reward);
```

Анимация показа/скрытия — встроенный coroutine-lerp по умолчанию, либо DOTween
(`Ease`) после `Tools/VADE/Dependencies/Enable DOTween Support`. Окна создаются
через `Resources.Load<T>($"UI/{typeof(T).Name}")` (`ResourcesWindowFactory`).
Если понадобится свой способ загрузки — реализуйте `IWindowFactory` и подставьте
в `WindowService.Factory`.

`ToggleInput` — обёртка над UGUI `Toggle` с `toggleOutputEvent`
(`Subscribe(this ToggleInput, Action<bool>)`); в присланных файлах самого класса
не было, реконструирован по сигнатуре — если у вас уже есть свой, замените
`Runtime/UGUI/UI/ToggleInput.cs` на него.

---

## Localization (UGUI)

```csharp
LocalizationService.Instance.SetLanguage("ru");
string text = LocalizationService.Instance.GetText("hello_key");
LocalizationService.Instance.CurrentLanguage.Subscribe(lang => Debug.Log(lang));
```

`LocalizedText` (компонент на объекте с `TMP_Text`) — поле `key` с атрибутом
`[LocalizationKey]` (текстовое поле + выпадающий список известных ключей).
Формат файлов — `Resources/Localization/{код}.json`, плоский `Dictionary<string,string>`
через Newtonsoft.Json. Выбранный язык хранится через `AutoSave<string>`.

**`Tools/VADE/Localization/Editor`** — таблица всех языковых файлов бок о бок:
правка значений, добавление ключей/языков, подсветка отсутствующих переводов,
кнопка создания нового языкового файла.

---

## Audio (Core)

```csharp
var audio = new AudioService();
audio.Init(Resources.Load<AudioLibrary>("AudioLibrary"), poolSize: 10);
Dependency.Register<IAudioService>(audio);

Dependency.Resolve<IAudioService>().Play("Click", transform.position);
Dependency.Resolve<IAudioService>().Play(clip, position, new AudioConfigOverride(volume: 0.5f));
Dependency.Resolve<IAudioService>().IsMuted.value = true; // AutoSave<bool>, сохраняется само
```

`AudioLibrary` — `[CreateAssetMenu]` ScriptableObject со списком `AudioData`
(клипы, громкость, `loop`/`loopDuration` под `[ShowIf]`) и фоновой музыкой.
Пул `AudioSource` через `PooledAudioSource` + `CoroutineRunner`.

---

## IAP (Core, опционально `VADE_IAP`)

Включить: `Tools/VADE/Dependencies/Enable IAP Support` (ставит
`com.unity.purchasing` + `com.unity.services.core`).

```csharp
[CreateAssetMenu(menuName = "Configs/VADE/IAP/Product")]
// ProductData: id, type (ProductType), icon, onProductPurchased

var iap = new IAPService(Resources.LoadAll<ProductData>("Configs/Shop"));
await iap.Initialize();
Dependency.Register(iap);

productData.Purchase();
productData.Subscribe(() => Debug.Log("purchased"));       // на каждую покупку (consumable)
productData.SubscribeOnce(() => Debug.Log("owned"));        // разово, срабатывает сразу если уже куплено
```

Валидация чеков — `Window > Unity IAP > IAP Receipt Validation Obfuscator`
(генерирует `GooglePlayTangle`/`AppleTangle` с вашими ключами) — без этого
`IAPService` пропускает валидацию с предупреждением в консоль, но продолжает работать.

## Ads / LevelPlay (Core, опционально `VADE_LEVELPLAY`)

Включить: `Tools/VADE/Dependencies/Enable LevelPlay Ads Support` (ставит
`com.unity.services.levelplay`).

```csharp
var ads = new AdsServiceLevelPlay(myAdsConfig); // AdsConfig — ScriptableObject с ключами/ad unit id по платформам
ads.IsBlocked = () => GameData.PremiumUnlocked; // свой геймплейный гейт вместо хардкода
Dependency.Register<IAdsService>(ads);

ads.Init();
ads.ShowInterstitial();
ads.ShowRewarded(() => Debug.Log("rewarded"));
```

`AdsConfig` — свой ассет на проект (`Configs/VADE/Ads/AdsConfig`), ключи/ad unit
id по платформам как поля, а не хардкод в коде библиотеки. После установки сети
через LevelPlay — `Ads Mediation > LevelPlay Network Manager` (Resolve
зависимостей mediation-сетей автоматизировать нельзя, это отдельный шаг Unity).

---

## Onboarding (UGUI)

Целиком новый модуль: пошаговый онбординг, работающий и с UI-элементами
(`UiClickComponent`, `UiHandPointer`), и с мировыми объектами (`WorldArrowPointer`,
`ITargetObject`).

```csharp
// 1. Создать ассет: Tools/VADE/Onboarding/Create Asset
// 2. Настроить задачи/шаги/действия/условия прямо в инспекторе (полиморфные
//    списки — через [SerializeReferenceList], см. раздел про атрибуты)
// 3. Повесить OnboardingService на сцену, назначить ассет
// 4. На объекты, которые участвуют в онбординге — TaskComponentBase-наследник
//    (DefaultComponent/UiClickComponent/UIEventComponent), id генерируется сам

OnboardingService.Instance.StartOnboarding();
OnboardingService.Instance.StepCompleted += (stepIndex, step) => MyAnalytics.LogStep(step.key);
OnboardingService.Instance.OnOnboardingComplete.AddListener(() => Debug.Log("done"));
```

Встроенные условия: `WaitForComponentCompleted`, `WaitForCollect`, `WaitForUiEvent`,
`WaitForUiClick`, `WaitForInteractClick`/`WaitForInteract` (через `ITargetObject`
— `Highlighted`/`OnInteractedEvent`/`GetInstanceOfObject`), `WaitForBuildByPart`
(слушает `TaskEvents.ObjectBuilt` — событие должен поднять ваш компонент
объекта постройки, сам генератор такого компонента не входит в библиотеку).
Встроенные действия: `ShowUiHand`, `ShowWorldArrow`, `PlayCutscene`.

Сохранение прогресса — `OnboardingSave` на `AutoSave<T>` (вместо ручных
PlayerPrefs-ключей), включая список собранных объектов.

### Что не вошло (и почему)

Присланная версия была плотно завязана на конкретную игру — исключено, но
интерфейсы (`IAction`/`ICondition`) открыты для расширения в вашем собственном
коде тем же способом:
- `ObjectBuildedComponent`/`WaitForBuildByPlacer`/`WaitForUiClickOnValidBuildPlace`
  (EasyBuildSystem), `ObjectCollectComponent` (свой `CollectableObject`),
  `ResourcePickupComponent`/`UIPatchComponent` (своя система инвентаря/окон),
  `UnlockCraftItem`/`PatchItemByConfig`/`CheckAndAddItem`/`WaitForResourceAmount`
  (`ServiceLocator`/`UserInventoryService`/`CraftingService`), `WaitForTriggerZone`
  (свой `TriggerZone`), аналитика (`GameAnalyticsSDK`) — замените на
  `StepCompleted`/`OnOnboardingComplete` события.
- **`WorldArrowPointer` обобщён**: вместо хардкода `Outline`/`InteractableObject`/
  `ArrowInteractable`/`InteractionsController`/тега `"Train"` — событие
  `TargetChanged(Transform)`, на которое вешаете свою подсветку/интерактивность:
  ```csharp
  worldArrowPointer.TargetChanged += t => { if (t != null) t.GetOrAdd<MyOutline>().enabled = true; };
  ```

---

## Utilities (Editor)

`Tools/VADE/Utilities/`: **Find Missing Scripts** (сцена/префабы/Resources),
**Find Null References** (публичные и `[SerializeField]` поля), **Find Static
Issues** (объекты со Static-флагами без нужных компонентов/UV). `GameObject/VADE/Bake
Selected Meshes` — объединение выделенных мешей в один (с диалогами разрешения
конфликтов по normals/colors/uv/tangents). `Tools/VADE/Utilities/Read-Write Mesh
Fixer` — находит модели с включённым Read/Write и отключает по выбору.

---

## Расширение библиотеки

- Новый модуль — новая папка в `Runtime/Core/` или `Runtime/UGUI/` (в
  зависимости от того, нужен ли UGUI) + `Editor/`, если нужны свои drawer'ы.
- Новое хранилище/сериализатор AutoSave — `IAutoSaveStorage`/`IAutoSaveSerializer`.
- Новый способ создания окон — `IWindowFactory` → `WindowService.Factory`.
- Новые Actions/Conditions для Onboarding — просто реализуйте `IAction`/`ICondition`
  в своём коде, `[SerializeReferenceList]` подхватит их автоматически (сканирует
  все загруженные сборки).
