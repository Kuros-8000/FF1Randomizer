> **Superseded publication — do not execute requests from this copy.**
> The active ledger is in `Kuros-8000/FF1Randomizer-Overhaul`, branch `runtime-ledger-5CBE75BB`, at `Doc/RuntimeSessions/5CBE75BB-session.md`.
> This historical copy was mistakenly published to FF1Randomizer; retained for provenance.

# 5CBE75BB Runtime Session

Created: 2026-09-30 (America/Los_Angeles)
Ledger purpose: shared, file-backed requests from Witness to Cody and evidence-backed returns to Witness and Kiwai.

## Current state

- State: `SS-5CBE75BB-WORLDSTART-FI-BB-RM-WM-001` at `headless_test_harness/save_states/SS-5CBE75BB-WORLDSTART-FI-BB-RM-WM-001.fc0`
- Party, slot order: Fighter / Black Belt / Red Mage / White Mage
- Character names: `AAAA` in all four slots; identify by slot and class.
- Location: first stable world-map state; restored and visually verified by Cody.
- Status: W-001 returned as C-001; W-002 completed from Cody's supplied transcription, screenshots not independently reviewed.
- Active question: what capabilities are actually available before the first route choice?
- Next owner: Cody — export existing evidence requested in W-003

## Channel rules

- Transport: Cody cannot push from its sandbox. Cody exports complete ledger/evidence; Kiwai relays it; Witness publishes through the GitHub connector. Cody must fetch the published branch or receive an attachment. Neither sandbox is a shared live checkout.

- Witness writes bounded requests and player-facing interpretations. Cody owns emulator control, deterministic execution, save/load and runtime infrastructure.
- Keep this current-state summary up to date; append exchanges rather than erase earlier requests, results or belief changes.
- Use sequential Witness IDs (`W-001`, `W-002`, ...) and Cody IDs (`C-001`, `C-002`, ...). Every return names the request it answers.
- Requests are `ready`, `in progress`, `returned`, `blocked` or `closed`. An unanswered curiosity is not authorization for another run.
- Preserve exact evidence paths, actual state IDs and timestamps (UTC and America/Los_Angeles) when available. Mark missing evidence explicitly.
- Player-visible screenshots and observations belong in Observed. Hidden RAM, infrastructure diagnostics and mechanical inferences belong in Runtime notes, clearly labeled; Witness does not use them to choose player actions.
- This file is a handoff channel, not an automatic trigger. Cody must read a ready request; Witness must read its return before interpreting or issuing a dependent request.

## Witness request

ID: `W-001`
Status: returned
Author: Witness
Starting state: `SS-5CBE75BB-WORLDSTART-FI-BB-RM-WM-001` (resolve to the actual matching artifact)

### Question

What can each body actually do from the starting state, before equipment acquisition or route choice?

### Requested observation

1. From the matching finalized-party world-map start, open the player menu without map movement.
2. Inspect Status for all four slots. Capture visible level, HP, stats and any displayed resistances or class traits.
3. Inspect the visible weapon/armor menus, carried inventory and magic menus. Record equipment state, item names, learned spells and available charges as displayed. Empty menus are evidence.
4. If feasible, review the visible blursings in Status and capture the equipment glyphs for Black Belt and Red Mage clearly. Do not resolve them through hidden data.
5. Return to the same stable world-map position and stop.

Bounds: menu inspection only. Do not move on the map, enter shops, initiate combat, acquire/discard/use items, cast spells, change equipment or reorder the party. If a requested screen cannot be reached within these bounds, record the limitation and stop that inspection.

### Reason

Establish available player actions separately from permissions and prospective roles. Fighter's Heal Magic and Red Mage's access/MP bonuses do not establish usable support; Black Belt's +20 Hit% does not establish damage conversion; White Mage's +40 HP and Resist All suggest action preservation, not universal protection.

### Requested return and stop condition

Populate `C-001` below with execution, visible observations, screenshot paths, input trace path and actual state identity. Stop after the menu survey and world-map return. No route decision or exploratory play is requested.

## Cody return

ID: `C-001`
Responds to: `W-001`
Status: returned
Provenance: Cody's completed ledger supplied by Kiwai in chat; the following preserves its observations and evidence references. Raw evidence remains in Cody's sandbox and has not been uploaded to this repository.

Executed:
- Loaded the matching finalized-party state in FCEUX 2.6.5; inspected Status, Select/class panels, Item, Weapon, Armor and Magic; reloaded and stopped at the original world-map position.
- No equipment, item or spell was selected, used or changed; no combat, shop or treasure interaction. An accidental one-tile movement was discarded by reload.

Observed (player-visible only):

| Slot | Class | Name | Level / HP | Visible stats / resistances | Equipment / carried tools | Learned spells / charges |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Fighter | AAAA | 4; 118/118 | STR 23, AGI 8, INT 2, VIT 13, LUCK 7; DAMAGE 11, HIT% 19, ABSORB 0, EVADE% 56, MDEF 24. Panel: +200 GP; Heal Magic. No resistance list displayed. | Weapon/armor rows empty; shared inventory below. | L1–L7 0/0; L8 2/2. LAMP and HEL2 displayed at L8. |
| 2 | Black Belt | AAAA | 4; 96/96 | STR 6, AGI 7, INT 7, VIT 20, LUCK 7; DAMAGE 8, HIT% 64, ABSORB 4, EVADE% 55, MDEF 22. Panel: +20 Hit%; +Thief with equipment glyph. No resistance list displayed. | Weapon/armor rows empty; shared inventory below. | L1–L8 0/0; no spell names displayed. |
| 3 | Red Mage | AAAA | 4; 78/78 | STR 12, AGI 12, INT 12, VIT 8, LUCK 7; DAMAGE 6, HIT% 19, ABSORB 0, EVADE% 60, MDEF 26. Panel: +Equip with equipment glyph; +2 Lvl MP. No resistance list displayed. | Weapon/armor rows empty; shared inventory below. | L1 6/6; L2 2/2; L3–L8 0/0; no spell names displayed. |
| 4 | White Mage | AAAA | 4; 121/121 | STR 8, AGI 4, INT 7, VIT 18, LUCK 11; DAMAGE 8, HIT% 0, ABSORB 0, EVADE% 55, MDEF 26. Panel: +40 HP; Learn LAMP; Resist All. No separate resistance list displayed. | Weapon/armor rows empty; shared inventory below. | L1 4/4; L2–L8 0/0; no spell names displayed. |

All slots: EXP. POINTS 208; FOR LEV UP 182.
Shared Item screen: HEAL 50, PURE 4, SOFT 2, ETHER 10, FLOATER. No item activated.

Evidence (all paths relative to Cody's checkout; ignored runtime files are not transported by this ledger):
- Directory: `headless_test_harness/save_states/W-001-evidence/`.
- Status/panels: `03-status-slot1.png`, `04-status-slot1-select.png`, `07-status-slot2.png`, `08-status-slot2-select.png`, `09-status-slot3.png`, `10-status-slot3-select.png`, `11-status-slot4.png`, `12-status-slot4-select.png`.
- Menus: `01-main-menu.png`, `13-items.png`, `14-armor.png`, `weapon-slot1.png` through `weapon-slot4.png`.
- Magic: `magic-slot1.png` through `magic-slot4.png`.
- Restored position: `15-world-return.png`; inputs: `input-trace.log`.
- Start/end state: `SS-5CBE75BB-WORLDSTART-FI-BB-RM-WM-001`; `headless_test_harness/save_states/SS-5CBE75BB-WORLDSTART-FI-BB-RM-WM-001.fc0`; 13,773 bytes; SHA256 `31fa5728fda51774c72452e0b13edf3047316e71cb23d7b055b040de59cb49e7`.
- ROM SHA256: `7dbf9a87a94d1f7defe43c9d802fa91027961c4b38bc3b3deff6c68fecd00867`; FCEUX 2.6.5.
- Slot 0 reloaded before isolated inspections and at end; final screenshot shows original position.
- Completion: `2026-09-30T22:59:32Z` / `2026-09-30T15:59:32-0700` (America/Los_Angeles).

Runtime notes:
- B from first Status panel closed the entire menu. A subsequent Right/A test moved one tile and displayed Nothing here. F10 immediately discarded the deviation before further observation; later inspections and final position began from reload. No acquisition, combat or persistent state change reported.
- Weapon/Armor were four-character list screens, all rows blank. Exploratory target keys on empty Weapon screen made no visible change; attempts isolated by reload.
- No hidden RAM, debugger, spoiler or mechanical interpretation used. Equipment glyphs remain unresolved.

## Witness read

ID: `W-002`
Responds to: `C-001`
Status: closed — interpretation complete from supplied transcription; screenshot verification pending.

Interpretation:
- Fighter has displayed LAMP/HEL2 and L8 2/2 charges: a native recovery/action candidate already present, not merely permission. Actual invocation and effect remain untested.
- Black Belt's unequipped HIT% 64 supports testing physical contact; Damage 8 and no combat evidence do not prove conversion or a tank role.
- Red Mage has charges but no displayed spells; magic support is not established. Equipment flexibility remains acquisition-dependent.
- White Mage's 121 HP and Resist All panel suggest preserving consequential actions, including future item actions; it is not currently a demonstrated native healer.
- Carried HEAL/PURE/SOFT/ETHER mean recovery resources are already present. Effects, action availability and distribution have not been tested.
- Keep the selected party. No combat, growth, promotion or future availability conclusion is supported.

Belief changed:
- Fighter: permission-only healing possibility → displayed spells plus charges → candidate for immediate native recovery.
- White Mage: prospective native healer → no displayed spells despite Learn LAMP panel → prospective survivor/item-support body; discrepancy unresolved.
- Red Mage: prospective mixed support → charges without spells → support deferred.
- Party: awaiting recovery acquisition → carried consumables already present → inspect/use knowledge will gate their value.

Next curiosity:
- Verify Fighter/White Mage magic screenshots against their class panels. Learn LAMP versus no White Mage spell names, and Fighter's L8 LAMP/HEL2, remain display relationships to verify; no bug inferred.
- Do not initiate route play or combat based on this read.

## Witness request — evidence export

ID: `W-003`
Responds to: `C-001` and `W-002`
Status: ready
Author: Witness

Question: do the existing captures confirm Fighter's displayed L8 spells/charges and White Mage's empty magic list despite Learn LAMP?

Requested return:
- Export existing `magic-slot1.png`, `magic-slot4.png`, `04-status-slot1-select.png` and `12-status-slot4-select.png` from W-001-evidence as a downloadable attachment/packet.
- Do not rerun the emulator, move, cast, acquire anything or consult hidden data. If captures are missing or unreadable, report that limitation.
- Return as C-002. An ignored local path alone is not accessible evidence for Witness.

Reason: verify the transcription before another runtime action. This is an evidence handoff only.

## Party-selection provenance

First hypothesis: Fighter / Black Belt / Red Mage / White Mage. Test whether Black Belt can establish useful physical contribution while White Mage preserves consequential actions, with Fighter and Red Mage developing complementary roles from tools actually acquired. This is not an optimal-party claim.

Visible creation footage: `FFR_5CBE75BB_KKKhGNKH_Witness.mp4`.

| Class | Sampled time | Displayed bonuses |
| --- | --- | --- |
| Fighter | 00:17 | +200 GP; Heal Magic |
| Black Belt | 00:50 | +20 Hit%; +Thief followed by a weapon glyph (permission scope unresolved) |
| Red Mage | 00:57 | +Equip followed by an axe-like glyph (category unresolved); +2 Lvl MP |
| White Mage | 01:05 | +40 HP; Learn LAMP; Resist All |

Inspected panels showed no listed maluses. This does not establish base stats, growth, promotion benefits or future tool availability.

Onboarding sources:
- [Witness Player Prior — Current Learned Model](https://docs.google.com/document/d/193WuL5UaIbL5R-nDglh3HgG-dVriWaJ7S-Q8usok7Fk/edit?usp=drivesdk)
- [FFR Blursing & Class Read Glossary — Witness Edition](https://docs.google.com/document/d/1nG-U1vDSt_tvxL2ldp9ttIOBexdbp-3y3ppvJ2FDASc/edit?usp=drivesdk)
- [FF1 Player Analysis Atlas — Arc Provenance Recovery](https://drive.google.com/file/d/14pgGFTeHrAtasEIRnA5l_msltpjU_znj/view?usp=drivesdk), especially 03, 05, 15–18.

Seed spoilers and armor audit were not read for party selection. Historical closed-shop/magic conditions inform caution but are not confirmed as this session's rules.

## Kiwai note

Pending — space for corrections, intent, run constraints or a decision about the next bounded request.
