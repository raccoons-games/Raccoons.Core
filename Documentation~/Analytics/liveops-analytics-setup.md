# Аналітика для live-ops: підключення гри через Raccoons.Core

Ця сторінка — для Unity-розробника, який підключає нову гру (або чистить стару) так, щоб вона
**одразу з'явилась у live-ops дашборді** і в щоденних звітах у TaskFlow. І щоб при цьому гра
слала **менше 1 млн подій на добу** — бо понад цей поріг Google ставить експорт у BigQuery на
паузу, і дашборд сліпне.

Зразок — Arrow Cube: це найповніше інструментована наша гра. Але й найгучніша. Тому нижче
поруч із «роби як в AC» є й «цього з AC не повторюй».

Усе потрібне — в одному місці, у пакеті Raccoons.Core (версія 0.1.0+): методи для всіх подій
дашборда, дохід від реклами, фільтр спаму й ця документація (`Documentation~/Analytics/`).

Скорочений шлях: дай своєму AI-асистенту файл `RACCOONS_ANALYTICS_AI_SETUP.md` (лежить поруч у
пакеті й у вкладеннях сторінки в TaskFlow) і попроси «засетапити аналітику за цим файлом і
перевірити».

> **Гра вже живе й на неї йде закупка?** Нічого не прибирай і не перейменовуй без live-ops. В Arrow
> Cube один виклик у коді шле подію одразу у Firebase, AppsFlyer і GameAnalytics, а події AppsFlyer —
> це постбеки, на яких стоять ROAS-кампанії. Прибереш виклик — зламаєш закупку. Чисть лише
> Firebase-гілку, а з власником Google Ads звір, які події GA4 імпортовані як конверсії.

Чого ця сторінка не покриває: AppsFlyer (дохід від реклами для ROAS, постбеки) — це окрема
інтеграція.

---

## Як дані доходять до дашборда

```
гра (Raccoons.Core) → Firebase / GA4 → BigQuery: events_YYYYMMDD → дашборд і звіти
```

Дашборд бачить лише те, що прийшло під **точним** ім'ям події й параметра. Перейменував подію
— сторінка, яка на ній стоїть, тихо стала порожньою. Тому імена нижче — контракт, не поради.

Хто що робить:

| Хто | Що | Розділ |
|---|---|---|
| Unity-розробник | SDK, ініціалізація, події, гігієна обсягу, перевірка | 1–6 |
| Власник Firebase/GA4 | зв'язок з BigQuery, доступ для дашборда | 7 |
| live-ops | реєстрація гри в дашборді й звітах | 7 |

---

## 1. Встановлення

**Raccoons.Core тягне за собою Zenject.** Без пакета Extenject/Zenject (UPM) не з'явиться define
`RACCOONS_DEPS_AVAILABLE`, і весь Runtime SDK, разом з аналітикою, просто не скомпілюється в гру.
Якщо Zenject стоїть як .unitypackage — додай define руками в Player Settings.

**Пакет пінимо на коміт, не на тег.** Жоден тег (v0.0.1, v0.0.2) ще не містить Analytics, а
незапінений main може змінитись під тобою. У `Packages/manifest.json`:

```json
"com.raccoons.core": "https://github.com/raccoons-games/Raccoons.Core.git#0d54dd7f9c04d3698d75968c78fcfac5597e9cfa"
```

Це перший коміт із live-ops методами (версія 0.1.0). ⚠️ Поки гілку `feat/liveops-analytics` не
змержено в main, оновити пін на тег або коміт main після мержу.

Не копіюй рядок із manifest Arrow Cube: там у `Packages/` лежить вбудована копія SDK, і git-URL
без тегу в новій грі дасть зовсім інший код.

**Firebase Unity SDK:** Analytics обов'язково, Crashlytics — дуже бажано (без нього в дашборді
немає крашів). AC сидить на Firebase Unity 13.10.0. Плюс AppLovin MAX для реклами: якщо MAX стоїть
UPM-пакетом `com.applovin.mediation.ads`, міст `MaxAnalyticsBridge` з'являється сам; якщо як
.unitypackage — додай define `RACCOONS_MAX_AVAILABLE`.

**Define `RACCOONS_FIREBASE_ANALYTICS` — для Android І для iOS.** Найпідступніша пастка SDK. Без
цього define сервіс Firebase компілюється в порожній метод: у лозі «SENT», а у Firebase — нуль.
Меню Raccoons → Integrations → Enable за замовчуванням ставить його **лише для поточної
платформи** — увімкни галку «Apply to all platforms» або перевір обидві групи руками. Arrow Cube
так прожив кілька версій, на iOS не відправивши жодної події SDK. З версії 0.1.0 SDK хоча б кричить
про це в лог помилкою, щойно створюється `FirebaseAnalyticsService` без define.

**`FirebaseAnalyticsInitializer` чи власна ініціалізація — рівно щось одне.** Компонент із SDK (з
0.1.0 він завжди відпускає чергу подій, навіть якщо Firebase не піднявся) годиться, якщо Firebase у
грі більше ніхто не ініціалізує. У новій грі на ньому треба **свідомо поставити галку** *Filter Noisy
Events* — за замовчуванням вона вимкнена, щоб уже покладені в сцени компоненти живих ігор не змінили
телеметрію самі. Якщо Remote Config чи Crashlytics теж чекають на Firebase — ініціалізуй сам, як
нижче, щоб `CheckAndFixDependenciesAsync` йшов один раз. Компонент у сцені й власний `Initialize`
разом — ніколи: другий `Initialize` шле зайвий `game_started` і підміняє сервіси.

**Оновлюєш пакет у живій грі з 0.0.x?** Лише разом із live-ops. Що 0.1.0 змінює в живій грі:
фільтр у компоненті лишається вимкненим; якщо гра не ставила `build_version`, з дня релізу його
поставить `Initialize` (`Application.version`) — і з цього дня до її воронки рівнів почне діяти
фільтр невалідного трафіку, тож запиши дату в історію гри; `Initialize` створює прихований об'єкт
`[RaccoonsAnalytics]` з `DontDestroyOnLoad`. Повний список — `CHANGELOG.md` у корені пакета.

---

## 2. Ініціалізація

Порядок важливий. Події, які гра шле до готовності Firebase, SDK тримає в черзі.

```csharp
using Raccoons.Analytics;
using Raccoons.Analytics.Firebase;
using Raccoons.Analytics.AppLovinMax;

async void Awake()
{
    RaccoonsAnalytics.HoldEvents();                       // 1. усе, що прилетить раніше, — в чергу
    try
    {
        // 2. CheckAndFixDependenciesAsync — ОДИН раз на процес, спільний для Analytics/RC/Crashlytics.
        var status = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != Firebase.DependencyStatus.Available) return;

        RaccoonsAnalytics.Initialize(
            new FilteredAnalyticsService(new FirebaseAnalyticsService())); // 3. сервіс + фільтр спаму
    }
    finally
    {
        RaccoonsAnalytics.ReleaseEvents();                // 4. відпустити чергу за будь-якого результату
    }
}

// Після MaxSdk.InitializeSdk():
MaxAnalyticsBridge.Attach();                              // ad_impression для всіх форматів + initial_ecpm
```

Кілька речей, які тут легко зіпсувати:

- `build_version` `Initialize` ставить сам — `Application.version`. Не перезаписуй його через
  `SetBuildVersion` versionCode-ом чи хардкодом: дашборд вважатиме всіх гравців «невалідним
  трафіком» і викине з воронки рівнів.
- `Initialize` викликай один раз. Кожен виклик шле ще один `game_started`.
- Версія застосунку — у форматі `1.2.3`. Інакше щоденний звіт не бачить жодної версії.
- **Вимкни автоматичний `screen_view`**, особливо на iOS. Там Firebase рахує екраном кожен
  рекламний контролер і App Store: в AC це дало 52 `screen_view` на гравця за добу, з яких сама
  гра — менше п'ятої частини. Дашборду ця подія не потрібна.
  - iOS: у `Info.plist` ключ `FirebaseAutomaticScreenReportingEnabled` = `NO` (зручно через
    PostProcessBuild, приклад — у файлі для AI).
  - Android: у `AndroidManifest.xml` всередині `<application>`:
    `<meta-data android:name="google_analytics_automatic_screen_reporting_enabled" android:value="false" />`

---

## 3. Події, які потрібні дашборду

Це повний список для загальних сторінок дашборда. Поза ним дашборд читає ще параметр `dd` на
`mission_*` (сегмент адаптивної складності — лише якщо вона є в грі) і власні події сторінок Farm та
Daily Quests (це специфіка Farm Doku, див. контракт rg-liveops). Перш ніж додавати щось понад
список — розділ 4.

### Рівні — ядро всього

Без `mission_*` порожніють воронка рівнів, Levels, ретеншн за рівнями, Version-health, D0-тренд,
eCPM-когорти, а у щоденному звіті платформа просто зникає.

```csharp
RaccoonsAnalytics.SetPlayerLevel(level);   // перед стартом рівня: дасть user_level усім подіям SDK
RaccoonsAnalytics.MissionStarted("level", $"level_{level}", level, attempt, isGamePlay: true);
RaccoonsAnalytics.MissionCompleted("level", $"level_{level}", level, attempt, isGamePlay: true);
RaccoonsAnalytics.MissionFailed("level", $"level_{level}", level, attempt, failReason: "no_moves", isGamePlay: true);
```

| Параметр | Тип | Правило |
|---|---|---|
| `mission_type` | string | `"level"` для основних рівнів. Інші родини (челенджі, ферма) — свій тип, бо `mission_id` між ними повторюється |
| `mission_id` | int | номер рівня, з 1. Тільки int — string ламає агрегацію |
| `mission_name` | string | `level_N` — номер іде в параметр, не в ім'я події |
| `mission_attempt` | int | з 1, +1 на кожну повторну спробу того самого рівня. Зберігай у PlayerPrefs: в AC лічильник живе в пам'яті й обнуляється після перезапуску |

`mission_started` шлеться на **кожну** спробу, включно з ретраями. `mission_completed` — лише при
реальній перемозі; вихід з рівня — це не перемога.

**Поразка і revive — саме в такому порядку.** `mission_failed` шли в момент поразки, ще до
пропозиції revive. Якщо гравець відродився, це `revive_used`, і спроба продовжується: без нового
`mission_started`, з тим самим `mission_attempt`. Кінець спроби — `mission_completed` або ще один
`mission_failed`. Якщо логувати поразку лише після відмови від revive, частка revive у дашборді
завищиться, аж до понад 100%, а частка поразок буде заниженою.

### Реклама — дохід

`ad_impression` — єдине джерело грошей для дашборда. З MAX його шле міст із SDK:

```csharp
MaxAnalyticsBridge.Attach();   // один раз, після MaxSdk.InitializeSdk()

MaxSdk.ShowInterstitial(interUnitId, "placement_level_complete_continue");
MaxSdk.ShowRewardedAd(rewardedUnitId, "placement_out_of_moves_revive");
```

Інша медіація — виклич сама з її колбека доходу, передавши формат одним із кодів, які читає дашборд:
`INTER`, `REWARDED`, `BANNER`, `LEADER` або `MREC`. Наприклад, для IronSource:

```csharp
var format = adUnit == "interstitial" ? "INTER" : adUnit == "rewarded_video" ? "REWARDED" : "BANNER";
RaccoonsAnalytics.AdImpression(format, revenueUsd, placement, network, adUnitId, "IronSource");
```

а з колбека завантаження інтерстішала — `RaccoonsAnalytics.AdLoaded("INTER", ...)`. Очевидні аліаси
(`INTERSTITIAL`, `REWARDED_VIDEO`) SDK сам перетворить на `INTER` / `REWARDED`, решту — ні.

SDK відправляє `ad_impression` з `value` (дохід у USD, double), `ad_format` (у MAX — код як є,
великими літерами: `BANNER` / `LEADER` / `INTER` / `REWARDED`), `placement`, `currency=USD`, а також
`initial_ecpm` — eCPM першого інтерстішала за запуск, по ньому дашборд ділить гравців на тири.
Колбеки MAX можуть прийти не з головного потоку — SDK це витримує.

- **Плейсмент** передавай у `Show*` з префіксом `placement_`: `placement_<екран>_<дія>`. Канонічний
  список AC — `docs/arrow-cube-placements.md` у репо rg-liveops. Без плейсмента розбивка доходу
  по місцях показу порожня.
- **Не викликай `RaccoonsAnalytics.*RevenuePaid`.** Поруч з `ad_impression` дашборд порахує гроші
  двічі. `FilteredAnalyticsService` їх і так відкидає.
- **Не вмикай міст, якщо гра вже сама логує `ad_impression`** — буде подвійний облік.
- MAX віддає `-1`, коли мережа не розкрила дохід. SDK пише такий показ з нулем, щоб не відняти
  гроші від суми.

### Rewarded-воронка, бустери, монети

| Подія | Як | Для чого |
|---|---|---|
| `reward_video_show` | `RaccoonsAnalytics.RewardVideoShow(placement, network, level)` | RV-воронка, частка гравців з RV |
| `reward_video_collect` | `RaccoonsAnalytics.RewardVideoCollect(placement, reward, level)` | гравець отримав нагороду |
| `power_up_used` | `RaccoonsAnalytics.PowerUpUsed(level.ToString(), "level", attempt, "hint", $"level_{level}")` | бустери на рівень; одна подія на одне використання |
| `revive_used` | `RaccoonsAnalytics.ReviveUsed(level, "rewarded")` | лише якщо в грі є revive. Шли на **кожен** revive: щойно подія з'являється, дашборд бере revive тільки з неї. Не дублюй revive ще й через `power_up_used` — дашборд рахує обидві як бустери |
| `coins_delta` | `RaccoonsAnalytics.CoinsDelta(+50, "level_complete")` / `CoinsDelta(-200, "booster_hint")` | економіка монет у D0-тренді. Одна подія на транзакцію, не на монетку |

Рівень для RV-подій дашборд бере з `user_level`, а його ставить лише `SetPlayerLevel`. Аргумент
`level` у `RewardVideoShow/Collect` SDK кладе в `level_num`, який дашборд не читає. Тож
`SetPlayerLevel` перед кожним рівнем — обов'язково.

### Туторіал (якщо є)

```csharp
RaccoonsAnalytics.TutorialStep("main", step: 1, stepCount: 5, finished: false); // open_tutorial_step_1_start
RaccoonsAnalytics.TutorialStep("main", step: 1, stepCount: 5, finished: true);  // open_tutorial_step_1_finish
```

Це **єдине** місце, де число дозволено в імені події — дашборд парсить крок саме звідти. Інший
префікс (`tutorial_step_…`, `farm_tutorial_…`) воронка туторіалу не побачить. Подію, якій немає
окремого методу, шли через `RaccoonsAnalytics.Custom(name, params)` — у dev-білді SDK попередить,
якщо ім'я з числом, зарезервоване Firebase або довше 40 символів. Кроки нумеруй з 1, а
`stepCount` — це номер останнього кроку: за ним дашборд вирішує, хто туторіал пройшов.

### Покупки і краші

- **IAP:** не логуй `in_app_purchase` руками — це зарезервоване ім'я Firebase. На Android його
  збирає сам Firebase. Суму дашборд бере з поля `event_value_in_usd`, яке Firebase ставить сам.
  На iOS зі StoreKit 2, за коментарем у коді Arrow Cube, автозбору немає і потрібен
  `LogAppleTransactionAsync` із Firebase Unity 13.12 — ми цього ще не перевіряли. Тому після релізу
  звір у BigQuery, що в `in_app_purchase` на обох платформах заповнений `event_value_in_usd`.
- **Краші:** Firebase Crashlytics сам дає `app_exception`. Окремо нічого слати не треба.

### Що Firebase шле сам — не чіпай

`user_engagement` (з нього рахується час гри), `session_start`, `first_open`, `app_update`,
`app_remove`. Не вимикай їх і не дублюй своїми подіями.

---

## 4. Бюджет: менше 1 млн подій на добу

**Чому це важливо.** Безкоштовний денний експорт GA4 → BigQuery має ліміт 1 млн подій на добу.
Google пише, що при стабільному перевищенні експорт ставлять на паузу, а пропущені дні назад не
дозаливають. В Arrow Cube денні таблиці двічі зникали саме після тижня понад 1 млн, і щоденні звіти
стояли. 24.10 гра відправила 2,74 млн подій при 5 354 гравцях — 512 на гравця. Дашборд із цього
читав лише 38%.

**Скільки можна.** Рахуй від запланованого DAU, з запасом на ріст закупки:

> подій на гравця за добу ≤ 800 000 / (DAU × 1,5)

| DAU | ліміт подій на гравця |
|---:|---:|
| 1 000 | 533 |
| 3 000 | 178 |
| 5 000 | 107 |
| 10 000 | 53 |
| 20 000 | 27 |

**Скільки коштує сам контракт** (розрахунок на ставках AC за 24.10, обидві платформи разом):

| Що | подій на гравця | влазить у бюджет із запасом до | жорстка межа 1 млн |
|---|---:|---:|---:|
| усе з розділу 3, без банера | ≈71 | — | — |
| + банер, подія на кожне оновлення раз на 15 с | ≈195 | ~2 700 DAU | ~5 100 DAU |
| + банер пакетами по 5 хв | ≈82 | ~6 500 DAU | ~12 000 DAU |

Із 71 близько 31 — це автоматичний `user_engagement`. На iOS його роздуває той самий автотрекінг
екранів, що й `screen_view`. Чи впаде він після вимкнення трекінгу, ще не міряли: на Android, де
екранів мало (1,1 `screen_view` на гравця), це 7,5. Перевір на першій добі. Перший замір дасть
найближчий iOS-реліз Arrow Cube: там трекінг уже вимкнено.

Висновок простий: для гри з банером місця на «а давай ще залогуємо» майже немає, а понад ~2 700
DAU без банерних пакетів не обійтись.

**Що з'їдало бюджет Arrow Cube і чого не робити:**

| Не роби | Чому (дані AC за 24.10) |
|---|---|
| Події завантаження реклами `*_load`, `*_load_fail` | 30% усього обсягу, жодна сторінка їх не читає. `FilteredAnalyticsService` їх відкидає. Треба для діагностики fill — семплуй 5% гравців |
| Подія на кожен тап, хід чи промах (`input_mistap`, `move_batch`, `level_first_action`) | 30 `input_mistap` на гравця за добу. Порахуй у грі й поклади число параметром у `mission_completed` |
| Подія «показалось» на елемент, що з'являється знову і знову (`hint_side_shown`) | 15 на гравця. Максимум — один раз за рівень |
| Номер рівня в імені (`level_12_start`) | 926 різних імен за добу. Ліміт GA4 — близько 500 імен на застосунок, понад нього нові імена можуть не потрапити у звіти; аудит AC бачив, як губились верхні рівні |
| Дві події на один факт (`hint_used` + `power_up_used`) | дашборд складає обидві й показує бустерів удвічі більше |
| Автоматичний `screen_view` на iOS | див. розділ 2 |

**Банер — головна стаття витрат серед потрібних подій.** В AC банер оновлюється раз на 15
секунд, і кожне оновлення — окремий `ad_impression`. Банери дали 95% усіх показів і 6% доходу.

У SDK є режим пакетів: `RaccoonsAnalytics.BannerBatchSeconds = 300` складає банерні покази за 5
хвилин в одну подію з сумою доходу й лічильником `impressions`. Для AC це з 660 тис. подій на добу
зробило б 56 тис.

Але **поки що він вимкнений**, і вмикати його без live-ops не можна. Дохід дашборд порахує
правильно, а от кількість показів, eCPM банера (завищиться в 10–12 разів) і fill зламаються — і в
дашборді, і в щоденному звіті, де в день перемикання ще й спрацюють алерти. Пакет втрачає мережу,
що показала банер (`ad_source`). А якщо `ad_impression` імпортується в Google Ads як конверсія,
кількість конверсій різко впаде, хоча сума лишиться та сама. AppsFlyer це не зачіпає.

Навчити дашборд читати `impressions` — задача live-ops. Її треба закрити до старту закупки будь-якої
гри з банером, яка планує понад ~2 700 DAU.

Будь-яку шумну подію можна відсікти без релізу логіки: `new FilteredAnalyticsService(firebase,
new[] { "input_mistap" })` або на льоту `filter.Block("…")` зі списку в Remote Config.

**Правило на кожну нову подію:** вона має відповідати на питання, яке хтось реально ставить. Якщо
подія спрацьовує частіше за десять разів на сесію, це або лічильник у параметрі, або пакет.

**Якщо вже над лімітом:** власник GA4 може виключити шумні події з експорту в BigQuery (GA4 Admin
→ BigQuery Links → виключити події). Вони лишаються у Firebase й AppsFlyer, але з дашборда зникають
назавжди, і ряд даних обривається на цій даті. Це страховка, а не заміна чистки в грі.

---

## 5. Перевірка

### На пристрої, до релізу

Editor не відправляє в Firebase нічого — десктопна версія SDK там заглушка. Перевіряй на телефоні
через DebugView:

- Android: `adb shell setprop debug.firebase.analytics.app <bundle_id>` (вимкнути — `.none.`)
- iOS: аргумент запуску в схемі Xcode `-FIRDebugEnabled`

Firebase Console → Analytics → DebugView. Пройди рівень, програй рівень, подивись рекламу кожного
формату, візьми бустер. Мають прийти `mission_*`, `ad_impression` з `value` і правильним
`ad_format`, `reward_video_show`/`collect`, `power_up_used`, `initial_ecpm` (один раз), і НЕ мають
прийти `*_load`, `*_load_fail`, `*_revenue_paid`, `screen_view`.

Лог «SENT» через `RaccoonsAnalytics.OnLogEvent` нічого не доводить: подія потрапляє туди **до**
відправки, навіть якщо фільтр її відкине чи define немає. Саме так AC не помічав, що на iOS не йде
нічого. Віримо тільки DebugView.

### Наступного дня, у BigQuery

Запити — у файлі для AI (крок «Verify»). Що вони мають показати на першій повній добі:

| Перевірка | Норма |
|---|---|
| подій за добу і на гравця | у бюджеті з розділу 4, з запасом |
| різних імен подій | менше 100 (ліміт GA4 — 500) |
| типи параметрів | `mission_id` int, `value` double, `delta` int |
| `ad_format` | лише великими літерами; у `INTER`/`REWARDED` плейсмент ≈ на 100% показів |
| `build_version` проти `app_info.version` | нуль розбіжностей |
| `ad_impression` зі `store=0` | ≈0. Інакше дохід таких інсталів випаде зі щоденного звіту — скажи live-ops |
| `in_app_purchase` (якщо є IAP) | на обох платформах, із заповненим `event_value_in_usd` |
| рядків у денних таблицях | кожна доба має таблицю `events_YYYYMMDD` і < 800 тис. рядків |

### У дашборді

Після реєстрації гри (розділ 7): Mission funnel показує рівні, Revenue — дохід за форматами й
плейсментами, D0-trend — монети й рівні. Порожня сторінка — дивись таблицю помилок нижче.

---

## 6. Типові помилки

| Симптом | Причина | Що робити |
|---|---|---|
| На iOS усе порожньо, а в лозі «SENT» | немає `RACCOONS_FIREBASE_ANALYTICS` в iOS-дефайнах | define для обох платформ |
| Воронка рівнів порожня, решта живе | `build_version` ≠ `app_info.version` або `mission_type` не той (`"Level"`, `"main"`), а live-ops про це не знає | прибрати власний `SetBuildVersion` або передавати лише `Application.version`; `mission_type="level"` |
| Частка revive понад 100% | `mission_failed` шлеться лише після відмови від revive | розділ 3, «Поразка і revive» |
| Дохід удвічі більший за MAX | `*_revenue_paid` поруч з `ad_impression`, або `MaxAnalyticsBridge` поруч із власним логуванням `ad_impression` | одне джерело доходу |
| Revenue за форматами порожній | `ad_format` у нижньому регістрі або перекладений | код MAX як є |
| Розбивка за плейсментами порожня | не передано плейсмент у `Show*` | `MaxSdk.Show*(unitId, "placement_…")` |
| Верхні рівні губляться | динамічні імена подій понад 500 | номер — у параметр |
| Денні таблиці в BigQuery перестали з'являтись | гра перевищила 1 млн подій на добу | розділ 4 |
| IAP = 0 | покупка не логується або логується руками під зарезервованим ім'ям | розділ 3, «Покупки» |

---

## 7. Що роблять власник Firebase і live-ops

**Власник Firebase/GA4:**

- Project Settings → Integrations → BigQuery: **Daily** — увімкнено (звіти читають лише денні
  таблиці). Streaming — за бажанням: платний, зате дає свіжі дані й страховку на час паузи.
- Експорт **User data** — вимкнено: його таблиці мають іншу схему і валять сторінки дашборда.
- Білінг GCP-проєкту увімкнено: без нього таблиці живуть лише 60 днів.
- Доступ для дашборда: акаунту `liveops-bq-reader@arrows-cube.iam.gserviceaccount.com` — роль
  BigQuery Job User на GCP-проєкт гри і BigQuery Data Viewer на датасет `analytics_<propertyId>`.

**live-ops** (передай їм GCP project id, датасет, bundle id обох платформ і часовий пояс GA4
property): запис гри в `lib/projects.ts` дашборда, доступи людям, рядок звіту в TaskFlow
(хто отримує щоденний звіт), AppsFlyer app id. Щоденний звіт вмикається, коли на версії набирається
від 25 гравців на добу.

---

## Arrow Cube як зразок

Код у репо гри (гілка develop): `Assets/ArrowCube/Scripts/Services/`.

- `Analytics/AnalyticsService.cs` — ініціалізація, рівні, бустери, монети;
- `Ads/VgAds/VgAdsMaxTelemetry.cs` — колбеки MAX, initial_ecpm, рекламна воронка;
- `Analytics/FirebaseAnalyticsService.cs` — `ad_impression` напряму у Firebase;
- `ARR-983_account_owner_steps.md` у корені — як AC різав обсяг.

Чого з AC **не** повторювати: `level_N_*` в іменах, дубль `hint_used` поруч із `power_up_used`,
банер на кожне оновлення, увімкнений `screen_view` на iOS, лічильник спроб у пам'яті, вбудовану
копію SDK. `RaccoonsAnalytics.Custom` у пакеті 0.1.0 має ту саму сигнатуру, що й у вбудованій копії
AC, тож AC може перейти на пакет без правок коду. Свій `ad_impression` AC шле сам — міст MAX
йому вмикати не можна.

---

Джерела: контракт подій — `docs/analytics-event-contract.md` у репо rg-liveops (якщо щось
розходиться, правий він). Обсяги — BigQuery Arrow Cube, доба 24.10.2026. SDK — Raccoons.Core 0.1.0
(гілка `feat/liveops-analytics`).
