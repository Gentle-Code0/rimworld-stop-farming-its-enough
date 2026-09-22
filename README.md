# Stop farming, it's enough!
A Rimworld mod for farming and stock control, details can be seen on steam workshop page.

## UFPC compatibility (1.4.3)

UFPC is optional; load this mod after `Kindseal.UFPC` when both are enabled. Its standard `Building_PlantGrower` hydroponics use the existing sowing/harvesting controls. Additional products declared through `UFPC.PlantWithProductsExtension` are now discovered at startup, including before the first harvest. These automatic links cannot be deleted; each new link defaults to ignoring both limits. Disable the appropriate ignore option and set its thresholds to monitor it. Existing manual links to the same products retain their thresholds and ignore settings and become automatic links.

Discovery runs only when the crop catalog is built. Inventory checks still read vanilla cached resource counts every 600 ticks; no growth polling or additional inventory scanning is added. Products excluded from vanilla resource statistics remain unsupported. Verification used isolated game objects, not an in-game save; a failure involving ordinary crops still needs the affected player's configuration to reproduce.

## UFPC 兼容（1.4.3）

UFPC 为可选模组；同时启用时，将本模组放在 `Kindseal.UFPC` 下方。它使用原版 `Building_PlantGrower` 的水培箱沿用现有播种／收获控制。现在启动时即可识别 `UFPC.PlantWithProductsExtension` 声明的副产物，无需等待第一次收获。这些自动关联不可删除；每个新增关联默认忽略两种限制，需要取消对应的忽略勾选并设置阈值才会监测。此前手动关联的同一产物会保留阈值及忽略设置，并转为自动关联。

识别仅在建立作物目录时执行。库存仍固定每 600 ticks 读取原版资源统计缓存，不增加生长状态轮询或库存扫描。原版不统计的产物仍不支持监测。验证使用隔离的游戏对象，未进行游戏存档实测；若普通作物仍失效，需要玩家的具体配置才能复现。
