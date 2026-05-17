-- TOF Battle Snapshot Logger (Mesen Lua)
--
-- Purpose:
--   Non-invasive, emulator-side battle telemetry for TOF lab sessions.
--   Collects battle-entry and per-action exchange snapshots without ROM edits.
--
-- Usage:
--   1) Load ROM in Mesen 2.
--   2) Open Scripting panel and run this file.
--   3) Edit CONFIG below for your ROM label map / output path.
--
-- Notes:
--   - Address map values are intentionally centralized and overrideable.
--   - If a field address is unknown for your build, leave it nil; logger will skip gracefully.

local CONFIG = {
  output_file = "tof_battle_snapshot.ndjson",
  flush_every = 1,
  include_raw_bytes = true,

  -- Optional hard stop to keep logs small in long soak sessions
  max_records = 100000,
}

-- Address map (NES CPU addresses) for FF1 battle state.
-- Fill in / override as needed per ROM build.
local ADDR = {
  in_battle = 0x006B,          -- non-zero while battle mode is active
  battle_round = 0x6857,       -- battle round counter
  acting_index = 0x6880,       -- current actor index when action resolves
  defender_index = 0x6885,     -- target index during physical attacks in this codebase
  formation_id = 0x6A03,       -- loaded formation id (candidate)

  -- party structs (candidate defaults)
  party_base = 0x6100,
  party_stride = 0x40,
  party_count = 4,

  -- enemy slots (candidate defaults)
  enemy_ids_base = 0x6B24,
  enemy_count = 9,

  -- action scratch (candidate defaults)
  action_type = 0x6870,
  action_id = 0x6871,
  hit_count = 0x6874,
  damage_lo = 0x6875,
  damage_hi = 0x6876,
}

local state = {
  was_in_battle = false,
  record_count = 0,
  battle_seq = 0,
  action_seq = 0,
  handle = nil,
}

local function safe_read_u8(addr)
  if not addr then return nil end
  return emu.read(addr, emu.memType.cpu)
end

local function safe_read_u16(lo_addr)
  if not lo_addr then return nil end
  local lo = emu.read(lo_addr, emu.memType.cpu)
  local hi = emu.read(lo_addr + 1, emu.memType.cpu)
  return (hi << 8) | lo
end

local function now_frame()
  return emu.framecount()
end

local function to_json_value(v)
  if v == nil then return "null" end
  if type(v) == "number" then return tostring(v) end
  if type(v) == "boolean" then return v and "true" or "false" end
  local s = tostring(v):gsub("\\", "\\\\"):gsub('"', '\\"')
  return '"' .. s .. '"'
end

local function encode_json(tbl)
  local parts = {}
  for k, v in pairs(tbl) do
    table.insert(parts, '"' .. k .. '":' .. to_json_value(v))
  end
  return "{" .. table.concat(parts, ",") .. "}"
end

local function write_record(kind, fields)
  if state.record_count >= CONFIG.max_records then return end
  local rec = {
    kind = kind,
    frame = now_frame(),
    battle_seq = state.battle_seq,
    action_seq = state.action_seq,
  }
  for k, v in pairs(fields or {}) do rec[k] = v end

  state.handle:write(encode_json(rec) .. "\n")
  state.record_count = state.record_count + 1
  if CONFIG.flush_every > 0 and (state.record_count % CONFIG.flush_every == 0) then
    state.handle:flush()
  end
end

local function capture_party_snapshot()
  local out = {}
  for i = 0, ADDR.party_count - 1 do
    local b = ADDR.party_base + (i * ADDR.party_stride)
    out["p" .. i .. "_class"] = safe_read_u8(b + 0x00)
    out["p" .. i .. "_level"] = safe_read_u8(b + 0x01)
    out["p" .. i .. "_hp"] = safe_read_u16(b + 0x0A)
    out["p" .. i .. "_maxhp"] = safe_read_u16(b + 0x0C)
    out["p" .. i .. "_weapon"] = safe_read_u8(b + 0x20)
    out["p" .. i .. "_armor1"] = safe_read_u8(b + 0x24)
    out["p" .. i .. "_armor2"] = safe_read_u8(b + 0x25)
    out["p" .. i .. "_armor3"] = safe_read_u8(b + 0x26)
    out["p" .. i .. "_armor4"] = safe_read_u8(b + 0x27)
    out["p" .. i .. "_absorb"] = safe_read_u8(b + 0x2A)
    out["p" .. i .. "_evade"] = safe_read_u8(b + 0x2B)
    out["p" .. i .. "_mdef"] = safe_read_u8(b + 0x2C)
  end
  return out
end

local function capture_enemy_snapshot()
  local out = {}
  for i = 0, ADDR.enemy_count - 1 do
    out["e" .. i .. "_id"] = safe_read_u8(ADDR.enemy_ids_base + i)
  end
  return out
end

local function log_battle_entry()
  state.battle_seq = state.battle_seq + 1
  state.action_seq = 0

  local fields = {
    formation_id = safe_read_u8(ADDR.formation_id),
    round = safe_read_u8(ADDR.battle_round),
    encounter_source = "unknown",
    map_area = "unknown",
  }

  for k, v in pairs(capture_party_snapshot()) do fields[k] = v end
  for k, v in pairs(capture_enemy_snapshot()) do fields[k] = v end

  if CONFIG.include_raw_bytes then
    fields.raw_actor = safe_read_u8(ADDR.acting_index)
    fields.raw_defender = safe_read_u8(ADDR.defender_index)
  end

  write_record("battle_entry", fields)
end

local function log_action_exchange()
  state.action_seq = state.action_seq + 1

  local fields = {
    round = safe_read_u8(ADDR.battle_round),
    actor_index = safe_read_u8(ADDR.acting_index),
    target_index = safe_read_u8(ADDR.defender_index),
    action_type = safe_read_u8(ADDR.action_type),
    action_id = safe_read_u8(ADDR.action_id),
    hit_count = safe_read_u8(ADDR.hit_count),
    damage = safe_read_u16(ADDR.damage_lo),
  }

  write_record("action_exchange", fields)
end

local function ensure_open()
  if state.handle then return end
  state.handle = io.open(CONFIG.output_file, "w")
  write_record("logger_start", { message = "TOF snapshot logger online" })
end

local function tick()
  ensure_open()
  local in_battle = (safe_read_u8(ADDR.in_battle) or 0) ~= 0

  if in_battle and not state.was_in_battle then
    log_battle_entry()
  end

  if in_battle then
    log_action_exchange()
  elseif state.was_in_battle and not in_battle then
    write_record("battle_exit", { message = "battle ended" })
  end

  state.was_in_battle = in_battle
end

emu.addEventCallback(tick, emu.eventType.endFrame)
