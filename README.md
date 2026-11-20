# Stop farming, it's enough!
A Rimworld mod for farming and stock control, details can be seen on steam workshop page.

## UFPC compatibility (1.4.3)

UFPC is optional; load this mod after `Kindseal.UFPC` when both are enabled. Its standard `Building_PlantGrower` hydroponics use the existing sowing/harvesting controls. Additional products declared through `UFPC.PlantWithProductsExtension` are now discovered at startup, including before the first harvest. These automatic links cannot be deleted; each new link defaults to ignoring both limits. Disable the appropriate ignore option and set its thresholds to monitor it. Existing manual links to the same products retain their thresholds and ignore settings and become automatic links.

Discovery runs only when the crop catalog is built. Inventory checks still read vanilla cached resource counts every 600 ticks; no growth polling or additional inventory scanning is added. Products excluded from vanilla resource statistics remain unsupported. Verification used isolated game objects, not an in-game save; a failure involving ordinary crops still needs the affected player's configuration to reproduce.

## UFPC 兼容（1.4.3）

UFPC 为可选模组；同时启用时，将本模组放在 `Kindseal.UFPC` 下方。它使用原版 `Building_PlantGrower` 的水培箱沿用现有播种／收获控制。现在启动时即可识别 `UFPC.PlantWithProductsExtension` 声明的副产物，无需等待第一次收获。这些自动关联不可删除；每个新增关联默认忽略两种限制，需要取消对应的忽略勾选并设置阈值才会监测。此前手动关联的同一产物会保留阈值及忽略设置，并转为自动关联。

识别仅在建立作物目录时执行。库存仍固定每 600 ticks 读取原版资源统计缓存，不增加生长状态轮询或库存扫描。原版不统计的产物仍不支持监测。验证使用隔离的游戏对象，未进行游戏存档实测；若普通作物仍失效，需要玩家的具体配置才能复现。

## 1.5.0：存档独立规则与种植区按钮

- 选中种植区或玩家水培箱，点击“作物库存限额”（收获图标＋问号），打开全部作物／产物的设置窗口。保留搜索、图标、双忽略、上下限，以及手动关联的添加／删除。
- Mod 设置中仅保留功能总开关、播种／收获控制总开关和地图暂停图标显示选项。逐作物规则不再在此编辑。
- 逐作物规则、阈值和手动关联现在由 GameComponent 保存。当前存档的所有地图共享规则，每张地图仍使用自己的资源统计和滞回暂停状态。保存游戏才会持久保存本次修改。
- 新游戏使用默认规则（两种忽略均开启）。旧存档首次升级时深复制旧版全局规则，以后随该存档独立保存；全局文件中的旧规则仅保留为其他尚未升级旧存档的迁移模板，不接收当前游戏编辑。
- 在同一进程切换存档时重建目录，不复用上一个存档的可变规则。保持固定 600 ticks 库存检查；没有添加生长轮询或库存扫描。问号在游戏内叠绘，不需要额外贴图文件。

## 1.5.0: Save-specific rules and growing-area button

- Select a growing zone or player hydroponics building and click Crop stock limits (harvest icon + question mark) to edit all crop/product rules. Search, icons, both ignore options, thresholds, and manual-link addition/removal remain available.
- Mod settings retain only the master switch, global sowing/harvesting controls, and map pause-icon visibility. Individual rules are edited in the separate window.
- A GameComponent now stores rules, thresholds, and manual links. All maps in one save share these rules; resource counts and hysteresis pause states remain separate per map. Save the game to persist changes.
- New games use defaults (both ignore options checked). On first upgrade, older saves receive independent copies of the former global rules. The old global list remains only as a migration template for other older saves and is never updated by current-game edits.
- Switching games rebuilds the catalog without sharing mutable rules. Inventory checks remain at 600 ticks, with no growth polling or inventory scans. The question mark is drawn in-game; no additional texture is required.

## RimWorld 1.7（2026-10-18）

已增加独立的 `1.7/` 目录：1.7 加载该目录，1.6 继续加载 `1.6/`。1.6 的已发布 DLL 保持 1.5.0.0；1.7 DLL 为 1.5.1.0。两版使用相同 packageId 和存档字段，无需改名或重新配置阈值。

1.7 使用游戏根目录的 Assembly-CSharp.dll、UnityStubs.dll 及游戏自带 .NET 10 引用；不要沿用 1.6 的 UnityEngine 模块引用。使用 Visual Studio 的开发者终端或已安装 .NET 9 SDK 的命令行，从模组根目录运行：

```powershell
dotnet msbuild Source/StopFarmingItsEnough.csproj -t:BuildRimWorld17
```

默认从当前 RimWorld 安装目录取引用，并从工坊 Harmony 的 `1.7/Assemblies/0Harmony.dll` 取依赖。安装位置不同可以传入 `-p:RimWorld17Dir=...` 和 `-p:Harmony17Path=...`。此目标直接调用 SDK 的 C# 编译器，使用游戏自带的 .NET 10 程序集，不要求额外安装 .NET 10 SDK；构建输出只写入 `1.7/Assemblies/`。普通 Build / Release 仍是 1.6 构建，需要独立的 1.6 游戏引用；已升级为 1.7 的安装目录不能作为 1.6 引用来源。

修正了逐存档配置改造后额外产物组件可能漏装补丁的问题：安装补丁时读取已加载的植物定义，而非尚未创建的存档作物目录。该修正在本次 1.7 DLL 中生效。

验证环境：RimWorld 1.7.4981 rev136，游戏自带 .NET 10.0.13-waylithclr.dev，工坊 Harmony 1.7。30 项隔离运行检查通过；中英文文件、版本分流和三个原版图标资源路径均核对通过。**未启动完整游戏场景，尚未进行设置窗口、按钮及地图图标的目视验收，也未完成所有农业模组组合的完整游戏实测。** 详细证据见 VALIDATION.md。

## RimWorld 1.7 (2026-10-18)

The separate `1.7/` directory contains the 1.5.1.0 build. RimWorld 1.6 keeps loading its existing 1.5.0.0 DLL from `1.6/`. Package identity and save keys are unchanged. Build 1.7 with `dotnet msbuild Source/StopFarmingItsEnough.csproj -t:BuildRimWorld17` using a .NET 9 SDK and the game-provided .NET 10 references; override `RimWorld17Dir` and `Harmony17Path` if needed. Ordinary Build/Release still targets 1.6 and requires a separate 1.6 installation.

The 1.7 build also fixes discovery of additional-yield component overrides before the save-specific catalog exists. Thirty isolated checks passed on the actual 1.7 runtime. Language files, version routing and vanilla icon resources were checked. Full in-game visual validation and complete third-party mod combinations remain untested; see VALIDATION.md.

## 1.6.0：新存档默认规则（2026-10-29）

模组设置中恢复逐作物／产物上下限和独立忽略设置，顶部提示“此处设置只会改变新存档的初始阈值设置，不会改变已有存档中的阈值设置”。此处使用独立的 newGameDefaults 全局容器，保存模组设置即可保留默认配置。新游戏在 StartedNewGame 时深复制模板；各游戏之间、模板与游戏之间均不共享可变规则。加载已有存档继续使用其保存规则，不应用最新模板。旧版全局 rules 迁移模板独立保留，新默认值不会影响旧档升级逻辑。默认编辑器不读库存，也不切换当前存档作物目录。

已重建当前 RimWorld 1.7 DLL，程序集版本 1.6.0.0。1.6 游戏源码及语言已同步，但本机没有 1.6 游戏引用，未重编译 1.6 DLL；1.6 用户需针对 1.6 游戏执行普通 Release 构建后才能使用本轮功能。

Mod settings now contain editable defaults for new saves, including crop/product thresholds and both ignore options. New games receive independent copies; changing the template never changes an existing save, and editing a save never modifies the template. Existing-save loading and legacy migration remain separate. The 1.7 DLL has been rebuilt; the 1.6 DLL has not been rebuilt because matching game references are unavailable locally.

### 1.6 DLL 补充构建

现已使用 `G:\Games\Rimworld reference\1.6\Managed` 和工坊 Harmony 的 `1.6/Assemblies/0Harmony.dll` 完成 Release 构建，0 警告、0 错误，输出至 `1.6/Assemblies/StopFarmingItsEnough.dll`，程序集版本 1.6.0.0。此前“1.6 DLL 未重编译”的限制已解除；包含最新新存档默认阈值设置功能。未进行完整 1.6 游戏内目视验收。

重新构建命令（在模组根目录执行）：

```powershell
dotnet build Source/StopFarmingItsEnough.csproj -c Release '-p:GameManaged=G:\Games\Rimworld reference\1.6\Managed' '-p:HarmonyPath=G:\SteamLibrary\steamapps\workshop\content\294100\2009463077\1.6\Assemblies\0Harmony.dll'
```

## 种植区手动开关与贴图（程序集 1.7.0.0）

选中种植区后增加“禁止种植”“禁止收获”“仅割除成熟作物”三个独立切换按钮，默认均关闭。它们随地图／存档保存，独立于库存限额总开关，库存下降不会解除手动限制。按钮关闭只是撤销这项手动限制，不会覆盖库存或其他模组的限制。仅作用于种植区，不向水培箱添加这三个按钮。

成熟判定为 Growth >= 1（100%）。保护所有可播种植物的幼苗，包括改种前遗留的旧作物；枯萎病植物始终可割除。不可播种的野生植物按杂草放行；模组添加的可播种草类仍按作物保护。“仅割除成熟作物”只限制割除，不限制正常收获。禁止收获不会禁止 CutPlant 病株清理；本模组不负责自动标记病株。

状态采用地图组件按种植区引用查询，无每 tick 轮询、无定时区域地块扫描。区域派工入口优先过滤禁止播种／收获的区域，单株与执行阶段继续兜底。删除区域会删除其状态。新开关的自定义地图图标在未提供图片时不显示，按钮暂用原版绿勾／红叉。

### 手动添加贴图

已为 1.6 与 1.7 分别预留以下目录（内置 .gitkeep 只是目录占位，不是图片）。按正在使用的游戏版本放置；若希望两版都有图标，请复制到两版目录。使用 PNG、透明背景，文件名大小写与下表一致。添加后重启游戏，无需修改代码或重新编译 DLL。

| 状态 | 按钮 PNG（建议 64×64，可用 128×128） | 地图状态 PNG（建议 64×64，实际显示 28×28） |
|---|---|---|
| 禁止种植 | `Textures/UI/ZonePolicies/Buttons/NoSow.png` | `Textures/UI/ZonePolicies/Overlays/NoSow.png` |
| 禁止收获 | `Textures/UI/ZonePolicies/Buttons/NoHarvest.png` | `Textures/UI/ZonePolicies/Overlays/NoHarvest.png` |
| 仅割除成熟作物 | `Textures/UI/ZonePolicies/Buttons/MatureOnly.png` | `Textures/UI/ZonePolicies/Overlays/MatureOnly.png` |

上表路径相对于模组的 `1.6/` 或 `1.7/`。例如 1.7 禁止收获按钮完整路径为 `1.7/Textures/UI/ZonePolicies/Buttons/NoHarvest.png`。不需要在图片中画绿色对勾或红叉，原版 Command_Toggle 会显示开关状态。地图状态图只在对应开关开启时显示；三张图横向排列，与原库存暂停标记分行，位置采用区域首格作为锚点，避免遍历区域计算中心。建议主体留 4–8 像素透明边距，使用清晰轮廓。

English: Three manual zone toggles are off by default and saved per map. They operate independently of inventory controls. Mature-only cutting protects sowable plants below 100% growth, except blighted plants; non-sowable wild plants remain cuttable. Normal harvesting is unaffected by mature-only cutting. Add optional transparent PNGs at the version-specific paths above and restart the game; no rebuild is needed. Buttons use vanilla check/cross icons until custom art is supplied; map overlays appear only when custom overlay art is supplied and the associated toggle is on.
