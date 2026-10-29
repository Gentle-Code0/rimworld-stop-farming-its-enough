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
