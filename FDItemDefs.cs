using System;
using System.Collections.Generic;
using System.Text.Json;
using Engine;

namespace Game {
    /// <summary>One Farmer's Delight item definition loaded from Assets/FD/items.json.</summary>
    public class FDItemDef {
        public int Index;
        public string Name;
        public string Kind;
        public int Slot;
        public int Nutrition;
        public float Saturation;
        public int Stack;
        public string FoodType;
        public float Sickness;
        public int Rot;
    }

    /// <summary>
    /// Runtime item table. All ordinary FD items share one block class and use the
    /// block data as an index into this table, keeping ~100 items to one block ID.
    /// </summary>
    public static class FDItemDefs {
        static List<FDItemDef> s_defs = [];
        static Dictionary<string, FDItemDef> s_byName = [];
        static bool s_loaded;

        public static IReadOnlyList<FDItemDef> All {
            get {
                EnsureLoaded();
                return s_defs;
            }
        }

        public static void EnsureLoaded() {
            if (s_loaded) {
                return;
            }
            s_loaded = true;
            try {
                JsonDocument document = ContentManager.Get<JsonDocument>("FD/items");
                if (document == null) {
                    Log.Error("[FD] Missing FD/items.json");
                    return;
                }
                foreach (JsonElement element in document.RootElement.EnumerateArray()) {
                    FDItemDef def = new() {
                        Index = element.GetProperty("index").GetInt32(),
                        Name = element.GetProperty("name").GetString(),
                        Kind = element.GetProperty("kind").GetString(),
                        Slot = element.GetProperty("slot").GetInt32(),
                        Nutrition = element.GetProperty("nutrition").GetInt32(),
                        Saturation = element.GetProperty("saturation").GetSingle(),
                        Stack = element.GetProperty("stack").GetInt32(),
                        FoodType = element.GetProperty("foodType").GetString(),
                        Sickness = element.GetProperty("sickness").GetSingle(),
                        Rot = element.GetProperty("rot").GetInt32()
                    };
                    s_defs.Add(def);
                    s_byName[def.Name] = def;
                }
                s_defs.Sort((a, b) => a.Index.CompareTo(b.Index));
            }
            catch (Exception e) {
                Log.Error("[FD] Failed to load item definitions: " + e);
            }
        }

        public static FDItemDef Get(int index) {
            EnsureLoaded();
            if (index >= 0 && index < s_defs.Count && s_defs[index].Index == index) {
                return s_defs[index];
            }
            foreach (FDItemDef def in s_defs) {
                if (def.Index == index) {
                    return def;
                }
            }
            return null;
        }

        public static FDItemDef Find(string name) {
            EnsureLoaded();
            return name != null && s_byName.TryGetValue(name, out FDItemDef def) ? def : null;
        }
    }

    /// <summary>Block texture atlas slot lookup (Assets/FD/block_slots.json).</summary>
    public static class FDAtlas {
        static Dictionary<string, int> s_slots = [];
        static bool s_loaded;

        public static void EnsureLoaded() {
            if (s_loaded) {
                return;
            }
            s_loaded = true;
            try {
                JsonDocument document = ContentManager.Get<JsonDocument>("FD/block_slots");
                if (document == null) {
                    return;
                }
                foreach (JsonProperty property in document.RootElement.EnumerateObject()) {
                    s_slots[property.Name] = property.Value.GetInt32();
                }
            }
            catch (Exception e) {
                Log.Error("[FD] Failed to load block slots: " + e);
            }
        }

        public static int Slot(string name) {
            EnsureLoaded();
            return s_slots.TryGetValue(name, out int slot) ? slot : 0;
        }
    }
}
