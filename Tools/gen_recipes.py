"""Convert Farmer's Delight recipe JSON into runtime JSON used by the SC port."""
from __future__ import annotations

import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FD = os.path.abspath(os.path.join(ROOT, "..", "FarmersDelight-1.21"))
GEN = os.path.join(FD, "src", "generated", "resources", "data")
OUT = os.path.join(ROOT, "Assets", "FD")

# Vanilla items that do not exist in Survivalcraft map to these substitutes.
VANILLA = {
    "bowl": "bowl",
    "glass_bottle": "glass_bottle",
    "egg": "egg",
    "milk_bucket": "milk_bucket",
    "wheat": "wheat",
    "sugar": "sugar",
    "cocoa_beans": "cocoa_beans",
    "carrot": "carrot",
    "potato": "potato",
    "beetroot": "beetroot",
    "sweet_berries": "sweet_berries",
    "glow_berries": "glow_berries",
    "honey_bottle": "honey_bottle",
    "gold_ingot": "gold_ingot",
    "gold_nugget": "gold_nugget",
    "netherite_ingot": "netherite_ingot",
    "kelp": "kelp",
    "dried_kelp": "dried_kelp",
    "bone": "bone",
    "beef": "beef",
    "porkchop": "beef",
    "mutton": "beef",
    "cooked_beef": "cooked_beef",
    "cooked_porkchop": "cooked_beef",
    "cooked_mutton": "cooked_beef",
    "chicken": "chicken",
    "cooked_chicken": "cooked_chicken",
    "cod": "cod",
    "salmon": "cod",
    "cooked_cod": "cooked_cod",
    "cooked_salmon": "cooked_cod",
    "bread": "bread",
    "apple": "apple",
    "water_bucket": "water_bucket",
    "iron_ingot": "ironingot",
    "gold_ingot": "gold_ingot",
    "diamond": "diamond",
    "cobblestone": "cobblestone",
    "stone": "cobblestone",
    "oak_planks": "planks",
    "stick": "stick",
    "glass": "glass",
    "brick": "brick",
    "clay_ball": "clay",
    "paper": "canvas",
    "leather": "leather",
    "string": "string",
    "feather": "feather",
    "flint": "stonechunk",
    "wooden_shovel": "woodenshovel",
}


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def all_tags():
    tags = {}
    for data_dir in os.listdir(GEN):
        tag_dir = os.path.join(GEN, data_dir, "tags", "item")
        if not os.path.isdir(tag_dir):
            continue
        for root, _, files in os.walk(tag_dir):
            for file in files:
                if not file.endswith(".json"):
                    continue
                path = os.path.join(root, file)
                rel = os.path.relpath(path, tag_dir).replace("\\", "/")[:-5]
                key = f"{data_dir}:{rel}"
                values = load(path).get("values", [])
                tags[key] = values
    # Resolve nested tags recursively.
    resolved = {}
    def resolve(key, stack=None):
        if key in resolved:
            return resolved[key]
        if key not in tags:
            return []
        stack = set(stack or [])
        if key in stack:
            return []
        stack.add(key)
        result = []
        for value in tags[key]:
            if isinstance(value, dict):
                value = value.get("id", "")
            if value.startswith("#"):
                result += resolve(value[1:], stack)
            else:
                result.append(value.replace("minecraft:", "").replace("farmersdelight:", ""))
        resolved[key] = result
        return result
    for key in tags:
        resolve(key)
    return resolved


def canonical(name: str):
    if not name:
        return None
    name = name.replace("minecraft:", "").replace("farmersdelight:", "")
    if name in VANILLA:
        return VANILLA[name]
    if name.endswith("_log") or name.endswith("_wood") or name in ("crimson_stem", "warped_stem", "crimson_hyphae", "warped_hyphae"):
        return "oak_wood"
    if name in ("poppy", "dandelion", "allium", "azure_bluet", "blue_orchid", "cornflower", "lily_of_the_valley",
                "oxeye_daisy", "torchflower", "wither_rose", "orange_tulip", "pink_tulip", "red_tulip", "white_tulip"):
        return "white_flower"
    if name in ("melon", "pumpkin"):
        return "pumpkin"
    if name == "gravel":
        return "gravel"
    if name == "ink_sac":
        return "squid_ink"
    if name == "rotten_flesh":
        return "rotten_meat"
    if name == "bone_meal":
        return "bone"
    if name == "rabbit":
        return "beef"
    if name in KNOWN:
        return name
    # Common tag path fallback is handled by resolve_ref.
    return None


def resolve_ref(ref):
    if "item" in ref:
        value = canonical(ref["item"])
        return [value] if value else []
    if "type" in ref and ref["type"] == "neoforge:compound":
        result = []
        for child in ref.get("children", []):
            for name in resolve_ref(child):
                if name not in result:
                    result.append(name)
        return result
    if "type" in ref and ref["type"] == "neoforge:difference":
        return resolve_ref(ref.get("base", {}))
    if "tag" in ref:
        tag = ref["tag"]
        names = TAGS.get(tag, [])
        if not names:
            # Sensible fallback for common tags not present in FD's data.
            last = tag.split(":")[-1].split("/")[-1]
            aliases = {
                "carrot": ["carrot"], "potato": ["potato"], "beetroot": ["beetroot"],
                "onion": ["onion"], "tomato": ["tomato"], "cabbage": ["cabbage"],
                "rice": ["rice"], "carrot": ["carrot"], "potato": ["potato"], "beetroot": ["beetroot"],
                "raw_beef": ["beef"], "raw_pork": ["beef"], "raw_mutton": ["beef"],
                "raw_chicken": ["chicken"], "raw_fish": ["cod"], "raw_cod": ["cod"],
                "raw_salmon": ["cod"], "eggs": ["egg"], "egg": ["egg"],
                "leafy_green": ["cabbage"], "leafy_greens": ["cabbage"],
                "mushrooms": ["brown_mushroom", "red_mushroom"],
                "raw_meat": ["beef", "chicken"],
                "safe_raw_fish": ["cod"],
                "vegetable": ["carrot", "potato", "beetroot", "tomato", "onion", "cabbage"],
                "bones": ["bone"],
                "bread": ["bread"],
                "berry": ["sweet_berries", "glow_berries"],
                "cooked_beef": ["cooked_beef"], "cooked_pork": ["cooked_beef"],
                "cooked_mutton": ["cooked_beef"], "cooked_chicken": ["cooked_chicken"],
                "cooked_fish": ["cooked_cod"], "cooked_cod": ["cooked_cod"],
                "cooked_salmon": ["cooked_cod"], "dough": ["wheat_dough"],
                "pasta": ["raw_pasta"], "knife": ["fdknife"],
                "water": ["water_bucket"], "milk": ["milk_bucket"],
                "iron": ["ironingot"], "gold": ["gold_ingot"], "diamond": ["diamond"],
                "planks": ["planks"], "wood": ["planks"], "stone": ["cobblestone"],
                "glass": ["glass"], "brick": ["brick"], "clay": ["clay"],
                "leather": ["leather"], "string": ["string"], "feather": ["feather"],
                "flint": ["stonechunk"], "paper": ["canvas"],
            }
            names = aliases.get(last, [])
        result = []
        for name in names:
            value = canonical(name)
            if value and value not in result:
                result.append(value)
        return result
    return []


def main():
    global TAGS, KNOWN
    TAGS = all_tags()

    # Known content names: FD blocks/items + staples + vanilla substitutes.
    items_text = open(os.path.join(FD, "src/main/java/vectorwing/farmersdelight/common/registry/ModItems.java"), encoding="utf-8").read()
    item_names = re.findall(r'registerWithTab\("([^"]+)"', items_text)
    blocks_text = open(os.path.join(FD, "src/main/java/vectorwing/farmersdelight/common/registry/ModBlocks.java"), encoding="utf-8").read()
    block_names = re.findall(r'BLOCKS\.register\("([^"]+)"', blocks_text)
    KNOWN = set(item_names) | set(block_names) | set(VANILLA.values()) | {
        "bowl", "glass_bottle", "sugar", "cocoa_beans", "carrot", "potato", "beetroot",
        "wheat", "sweet_berries", "glow_berries", "honey_bottle", "gold_ingot", "gold_nugget",
        "netherite_ingot", "kelp", "dried_kelp", "bone", "beef", "cooked_beef", "chicken",
        "cooked_chicken", "cod", "cooked_cod", "bread", "apple", "water_bucket", "ironingot",
        "diamond", "cobblestone", "planks", "stick", "glass", "brick", "clay", "leather",
        "string", "feather", "stonechunk", "canvas", "woodenshovel",
        "brown_mushroom", "red_mushroom",
        "oak_wood", "white_flower", "pumpkin", "gravel", "rotten_meat", "squid_ink",
    }

    # Crafting and smelting.
    crafting = []
    smelting = []
    recipe_root = os.path.join(GEN, "farmersdelight", "recipe")
    for root, _, files in os.walk(recipe_root):
        for file in files:
            if not file.endswith(".json"):
                continue
            path = os.path.join(root, file)
            data = load(path)
            rtype = data.get("type", "")
            if rtype in ("minecraft:crafting_shaped", "minecraft:crafting_shapeless"):
                result = data.get("result", {})
                result_name = canonical(result.get("id", ""))
                if not result_name:
                    continue
                count = result.get("count", 1)
                ingredients = []
                if rtype == "minecraft:crafting_shaped":
                    key = data.get("key", {})
                    pattern = data.get("pattern", [])
                    chars = sorted({c for row in pattern for c in row if c != " "})
                    for char in chars:
                        if char not in key:
                            continue
                        options = resolve_ref(key[char])
                        if not options:
                            ingredients = []
                            break
                        ingredients.append(options)
                else:
                    for ref in data.get("ingredients", []):
                        options = resolve_ref(ref)
                        if not options:
                            ingredients = []
                            break
                        ingredients.append(options)
                if ingredients:
                    crafting.append({"ingredients": ingredients, "result": result_name, "count": count})
            elif rtype in ("minecraft:smelting", "minecraft:campfire_cooking", "minecraft:smoking", "minecraft:blasting"):
                result = data.get("result", {})
                result_name = canonical(result.get("id", ""))
                ing = data.get("ingredient", {})
                options = resolve_ref(ing)
                if result_name and options:
                    smelting.append({"ingredients": [options], "result": result_name, "count": result.get("count", 1), "time": data.get("cookingtime", 200)})

    # Cooking.
    cooking = []
    cooking_root = os.path.join(recipe_root, "cooking")
    for root, _, files in os.walk(cooking_root):
        for file in files:
            if not file.endswith(".json"):
                continue
            data = load(os.path.join(root, file))
            result = data.get("result", {})
            result_name = canonical(result.get("id", ""))
            if not result_name:
                continue
            ingredients = []
            for ref in data.get("ingredients", []):
                options = resolve_ref(ref)
                if not options:
                    ingredients = []
                    break
                ingredients.append(options)
            if not ingredients:
                continue
            container = data.get("container")
            container_name = canonical(container.get("id", "")) if isinstance(container, dict) else None
            cooking.append({
                "ingredients": ingredients,
                "result": result_name,
                "count": result.get("count", 1),
                "container": container_name,
                "time": data.get("cookingtime", 200),
            })

    # Cutting.
    cutting = []
    cutting_root = os.path.join(recipe_root, "cutting")
    for root, _, files in os.walk(cutting_root):
        for file in files:
            if not file.endswith(".json"):
                continue
            data = load(os.path.join(root, file))
            input_options = []
            for ref in data.get("ingredients", []):
                input_options += resolve_ref(ref)
            input_options = list(dict.fromkeys(input_options))
            if not input_options:
                continue
            results = []
            for entry in data.get("result", []):
                item = entry.get("item", {})
                name = canonical(item.get("id", ""))
                if name:
                    results.append({"name": name, "count": item.get("count", 1)})
            if not results:
                continue
            for input_name in input_options:
                cutting.append({"input": input_name, "tool": 0, "results": results})

    os.makedirs(OUT, exist_ok=True)
    with open(os.path.join(OUT, "crafting.json"), "w", encoding="utf-8") as f:
        json.dump(crafting, f, ensure_ascii=False, indent=2)
    with open(os.path.join(OUT, "cooking.json"), "w", encoding="utf-8") as f:
        json.dump(cooking, f, ensure_ascii=False, indent=2)
    with open(os.path.join(OUT, "cutting.json"), "w", encoding="utf-8") as f:
        json.dump(cutting, f, ensure_ascii=False, indent=2)
    with open(os.path.join(OUT, "smelting.json"), "w", encoding="utf-8") as f:
        json.dump(smelting, f, ensure_ascii=False, indent=2)
    print("crafting", len(crafting), "cooking", len(cooking), "cutting", len(cutting), "smelting", len(smelting))


if __name__ == "__main__":
    main()
