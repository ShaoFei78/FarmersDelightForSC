"""Generate Farmer's Delight item definitions, atlases and language files."""
from __future__ import annotations

import json
import os
import re
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FD = os.path.abspath(os.path.join(ROOT, "..", "FarmersDelight-1.21"))
JAVA = os.path.join(FD, "src", "main", "java", "vectorwing", "farmersdelight")
ASSETS = os.path.join(FD, "src", "main", "resources", "assets", "farmersdelight")
OUT = os.path.join(ROOT, "Assets")

# Items deliberately skipped here (handled by FDSeedItemBlock / FDKnifeBlock).
SEED_ITEMS = {"cabbage_seeds", "tomato_seeds", "onion", "rice"}
KNIFE_ITEMS = {"flint_knife", "iron_knife", "diamond_knife", "netherite_knife", "golden_knife"}

# Extra staples needed because Survivalcraft lacks the corresponding vanilla items.
STAPLES = {
    "bowl": ("Wooden Bowl", "木碗", "bowl"),
    "glass_bottle": ("Glass Bottle", "玻璃瓶", None),
    "sugar": ("Sugar", "糖", None),
    "cocoa_beans": ("Cocoa Beans", "可可豆", None),
    "carrot": ("Carrot", "胡萝卜", None),
    "potato": ("Potato", "马铃薯", None),
    "beetroot": ("Beetroot", "甜菜根", None),
    "wheat": ("Wheat", "小麦", None),
    "sweet_berries": ("Sweet Berries", "甜浆果", None),
    "glow_berries": ("Glow Berries", "发光浆果", None),
    "honey_bottle": ("Honey Bottle", "蜂蜜瓶", None),
    "gold_ingot": ("Gold Ingot", "金锭", None),
    "gold_nugget": ("Gold Nugget", "金粒", None),
    "netherite_ingot": ("Netherite Ingot", "下界合金锭", None),
    "kelp": ("Kelp", "海带", None),
    "dried_kelp": ("Dried Kelp", "干海带", None),
    "bone": ("Bone", "骨头", None),
    "brown_mushroom": ("Brown Mushroom", "棕色蘑菇", None),
    "red_mushroom": ("Red Mushroom", "红色蘑菇", None),
    "apple": ("Apple", "苹果", None),
}


def load_json(path: str):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def read(path: str) -> str:
    with open(path, encoding="utf-8") as f:
        return f.read()


def parse_food_values() -> dict[str, tuple[int, float]]:
    text = read(os.path.join(JAVA, "common", "FoodValues.java"))
    result = {}
    for m in re.finditer(r"FoodProperties\s+([A-Z0-9_]+)\s*=\s*(.*?);", text, re.S):
        name, body = m.group(1), m.group(2)
        nut = re.search(r"\.nutrition\((\d+)\)", body)
        sat = re.search(r"\.saturationModifier\(([\d.]+)f?\)", body)
        if nut:
            result[name] = (int(nut.group(1)), float(sat.group(1)) if sat else 0.0)
            continue
        m2 = re.search(r"new\s+FoodProperties\(\s*(\d+)\s*,\s*([\d.]+)", body)
        if m2:
            result[name] = (int(m2.group(1)), float(m2.group(2)))
    return result


def food_type(name: str) -> str:
    if any(k in name for k in ("cooked", "bacon", "ham", "patty", "barbecue", "sausage")):
        return "Meat"
    if any(k in name for k in ("cod", "salmon", "fish", "kelp_roll")):
        return "Fish"
    if any(k in name for k in ("bread", "dough", "pasta", "pie", "cookie", "sandwich", "hamburger", "wrap", "dumpling", "cake")):
        return "Bread"
    if any(k in name for k in ("cabbage", "tomato", "onion", "pumpkin", "fruit", "berry", "melon", "carrot", "potato", "beetroot", "salad")):
        return "Fruit"
    return "None"


def draw_staple(name: str) -> Image.Image:
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    if name == "glass_bottle":
        d.rectangle((5, 5, 10, 14), fill=(190, 230, 240, 220), outline=(80, 130, 150, 255))
        d.rectangle((7, 2, 8, 5), fill=(190, 230, 240, 220), outline=(80, 130, 150, 255))
    elif name == "sugar":
        d.rectangle((3, 5, 12, 13), fill=(245, 245, 235, 255), outline=(190, 190, 180, 255))
        d.rectangle((5, 3, 10, 5), fill=(235, 235, 225, 255), outline=(190, 190, 180, 255))
        for x, y in ((6, 8), (9, 9), (7, 11), (10, 7)):
            d.point((x, y), fill=(255, 255, 255, 255))
    elif name == "cocoa_beans":
        for x, y in ((4, 5), (8, 4), (6, 8), (10, 8), (5, 11), (9, 11)):
            d.ellipse((x, y, x + 3, y + 4), fill=(105, 55, 25, 255), outline=(65, 30, 15, 255))
    elif name == "carrot":
        d.polygon([(7, 2), (10, 2), (9, 14), (7, 14)], fill=(235, 130, 35, 255), outline=(150, 70, 15, 255))
        d.line((7, 2, 5, 0), fill=(60, 140, 55, 255))
        d.line((9, 2, 11, 0), fill=(60, 140, 55, 255))
    elif name == "potato":
        d.ellipse((3, 4, 13, 13), fill=(185, 145, 85, 255), outline=(120, 85, 45, 255))
        for x, y in ((6, 7), (10, 9), (8, 11)):
            d.point((x, y), fill=(110, 75, 40, 255))
    elif name == "beetroot":
        d.ellipse((5, 5, 11, 14), fill=(170, 35, 70, 255), outline=(110, 20, 45, 255))
        d.line((7, 5, 6, 1), fill=(60, 140, 55, 255))
        d.line((9, 5, 10, 1), fill=(60, 140, 55, 255))
    elif name == "wheat":
        d.line((8, 15, 8, 2), fill=(190, 150, 55, 255))
        for y in (3, 6, 9, 12):
            d.line((8, y, 5, y - 3), fill=(225, 190, 80, 255))
            d.line((8, y, 11, y - 3), fill=(225, 190, 80, 255))
    elif name in ("sweet_berries", "glow_berries"):
        col = (200, 40, 50, 255) if name == "sweet_berries" else (90, 220, 220, 255)
        for x, y in ((4, 6), (8, 4), (9, 9), (5, 11)):
            d.ellipse((x, y, x + 4, y + 4), fill=col, outline=(80, 30, 30, 255))
    elif name == "honey_bottle":
        d.rectangle((5, 6, 10, 14), fill=(225, 160, 45, 255), outline=(150, 95, 20, 255))
        d.rectangle((7, 2, 8, 6), fill=(200, 200, 200, 255))
    elif name in ("gold_ingot", "netherite_ingot"):
        col = (235, 195, 65, 255) if name == "gold_ingot" else (85, 75, 80, 255)
        d.polygon([(2, 10), (5, 5), (13, 5), (14, 9), (10, 13), (3, 13)], fill=col, outline=(80, 60, 20, 255))
    elif name == "gold_nugget":
        d.ellipse((5, 6, 11, 12), fill=(235, 195, 65, 255), outline=(130, 90, 20, 255))
    elif name in ("kelp", "dried_kelp"):
        col = (55, 130, 65, 255) if name == "kelp" else (70, 90, 45, 255)
        for x in (4, 8, 11):
            d.line((x, 14, x + (2 if x % 2 else -2), 2), fill=col)
    elif name == "apple":
        d.ellipse((3, 5, 13, 14), fill=(205, 40, 45, 255), outline=(120, 20, 25, 255))
        d.line((8, 5, 8, 1), fill=(90, 60, 25, 255))
        d.ellipse((8, 1, 12, 4), fill=(60, 150, 55, 255))
    elif name in ("brown_mushroom", "red_mushroom"):
        cap = (150, 100, 55, 255) if name == "brown_mushroom" else (190, 55, 55, 255)
        d.ellipse((2, 4, 14, 10), fill=cap, outline=(90, 50, 30, 255))
        d.rectangle((6, 8, 10, 14), fill=(230, 220, 200, 255), outline=(150, 130, 110, 255))
    elif name == "bone":
        d.rectangle((6, 2, 9, 14), fill=(235, 235, 220, 255), outline=(170, 170, 150, 255))
        d.ellipse((4, 1, 8, 5), fill=(235, 235, 220, 255))
        d.ellipse((8, 1, 12, 5), fill=(235, 235, 220, 255))
        d.ellipse((4, 11, 8, 15), fill=(235, 235, 220, 255))
        d.ellipse((8, 11, 12, 15), fill=(235, 235, 220, 255))
    return im


def main() -> None:
    os.makedirs(os.path.join(OUT, "Textures", "FD"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "FD"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "Lang"), exist_ok=True)

    # Names from the Java registry.
    blocks_text = read(os.path.join(JAVA, "common", "registry", "ModBlocks.java"))
    block_names = re.findall(r'BLOCKS\.register\("([^"]+)"', blocks_text)
    items_text = read(os.path.join(JAVA, "common", "registry", "ModItems.java"))
    item_names = re.findall(r'registerWithTab\("([^"]+)"', items_text)
    block_set = set(block_names)
    nonblock = [n for n in item_names if n not in block_set and n not in KNIFE_ITEMS and n not in SEED_ITEMS]
    # Add staples, but do not duplicate one already present in FD.
    for n in STAPLES:
        if n not in nonblock:
            nonblock.append(n)

    food_values = parse_food_values()
    item_dir = os.path.join(ASSETS, "textures", "item")
    block_tex_dir = os.path.join(ASSETS, "textures", "block")

    def texture_for(name: str) -> Image.Image:
        for path in (
            os.path.join(item_dir, f"{name}.png"),
            os.path.join(block_tex_dir, f"{name}.png"),
            os.path.join(block_tex_dir, f"{name}_side.png"),
        ):
            if os.path.exists(path):
                im = Image.open(path).convert("RGBA")
                if im.width != 16 or im.height != 16:
                    im = im.crop((0, 0, min(im.width, 16), min(im.height, 16))).resize((16, 16), Image.NEAREST)
                return im
        if name in STAPLES:
            return draw_staple(name)
        # Last resort: a coloured question tile.
        im = Image.new("RGBA", (16, 16), (180, 60, 180, 255))
        d = ImageDraw.Draw(im)
        d.rectangle((2, 2, 13, 13), outline=(255, 255, 255, 255))
        d.text((5, 4), "?", fill=(255, 255, 255, 255))
        return im

    atlas = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    defs = []
    all_defs = [(n, "item") for n in nonblock]
    all_defs += [(n, "seed") for n in ("cabbage_seeds", "tomato_seeds", "onion", "rice")]
    all_defs += [(n, "knife") for n in ("flint_knife", "iron_knife", "golden_knife", "diamond_knife", "netherite_knife")]
    for index, (name, kind) in enumerate(all_defs):
        img = texture_for(name)
        atlas.alpha_composite(img, ((index % 16) * 16, (index // 16) * 16))
        key = name.upper().replace("-", "_")
        nut, sat = food_values.get(key, (0, 0.0))
        stack = 40
        if name in ("bowl", "glass_bottle"):
            stack = 40
        elif name in ("honey_bottle",):
            stack = 16
        elif key in ("COOKED_RICE", "BONE_BROTH", "BEEF_STEW", "VEGETABLE_SOUP", "FISH_STEW", "ONION_SOUP", "CHICKEN_SOUP", "FRIED_RICE", "PUMPKIN_SOUP", "BAKED_COD_STEW", "NOODLE_SOUP", "BACON_AND_EGGS", "RATATOUILLE", "STEAK_AND_POTATOES", "PASTA_WITH_MEATBALLS", "PASTA_WITH_MUTTON_CHOP", "MUSHROOM_RICE", "ROASTED_MUTTON_CHOPS", "VEGETABLE_NOODLES", "SQUID_INK_PASTA", "GRILLED_SALMON", "FRUIT_SALAD", "MIXED_SALAD", "NETHER_SALAD", "DOG_FOOD"):
            stack = 16
        elif key in ("MILK_BOTTLE", "HOT_COCOA", "APPLE_CIDER", "MELON_JUICE", "GLOW_BERRY_CUSTARD"):
            stack = 16
        elif key in ("ROTTEN_TOMATO",):
            stack = 16
        elif kind == "knife":
            stack = 1
        defs.append({
            "index": index,
            "name": name,
            "kind": kind,
            "slot": index,
            "nutrition": nut,
            "saturation": sat,
            "stack": stack,
            "foodType": food_type(name),
            "sickness": 0.2 if name in ("chicken_cuts", "bacon", "minced_beef", "mutton_chops", "cod_slice", "salmon_slice") else 0.0,
            "rot": 120,
        })
    atlas.save(os.path.join(OUT, "Textures", "FD", "Items.png"))
    with open(os.path.join(OUT, "FD", "items.json"), "w", encoding="utf-8") as f:
        json.dump(defs, f, ensure_ascii=False, indent=2)

    # Block texture atlas: every block PNG gets a slot.
    block_tex = sorted(
        f[:-4] for f in os.listdir(block_tex_dir)
        if f.endswith(".png")
    )
    # Canvas signs live in textures/entity/signs in the Java edition.
    sign_tex_dir = os.path.join(ASSETS, "textures", "entity", "signs")
    sign_colors = ["", "white", "orange", "magenta", "light_blue", "yellow", "lime", "pink", "gray", "light_gray", "cyan", "purple", "blue", "brown", "green", "red", "black"]
    sign_textures = {}
    for color in sign_colors:
        key = "canvas_sign" if color == "" else f"{color}_canvas_sign"
        filename = "canvas.png" if color == "" else f"canvas_{color}.png"
        path = os.path.join(sign_tex_dir, filename)
        if os.path.exists(path):
            sign_textures[key] = path
            if key not in block_tex:
                block_tex.append(key)
    block_atlas = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    slots = {}
    for i, name in enumerate(block_tex[:256]):
        path = sign_textures.get(name, os.path.join(block_tex_dir, f"{name}.png"))
        im = Image.open(path).convert("RGBA")
        if name in sign_textures:
            im = im.resize((16, 16), Image.NEAREST)
        elif im.width != 16 or im.height != 16:
            im = im.crop((0, 0, min(im.width, 16), min(im.height, 16))).resize((16, 16), Image.NEAREST)
        block_atlas.alpha_composite(im, ((i % 16) * 16, (i // 16) * 16))
        slots[name] = i
    block_atlas.save(os.path.join(OUT, "Textures", "FD", "Blocks.png"))
    with open(os.path.join(OUT, "FD", "block_slots.json"), "w", encoding="utf-8") as f:
        json.dump(slots, f, ensure_ascii=False, indent=2)

    # Language entries from FD, adapted to SC keys.
    en = load_json(os.path.join(ASSETS, "lang", "en_us.json"))
    zh = load_json(os.path.join(ASSETS, "lang", "zh_cn.json"))
    en_blocks = {}
    zh_blocks = {}

    def read_recipe(name: str):
        path = os.path.join(OUT, "FD", name)
        if not os.path.exists(path):
            return []
        return json.load(open(path, encoding="utf-8"))

    cooking_ingredients = {}
    for recipe in read_recipe("cooking.json"):
        result = recipe.get("result")
        if not result:
            continue
        names = []
        for ingredient in recipe.get("ingredients", []):
            if ingredient:
                names.append(ingredient[0])
        cooking_ingredients.setdefault(result, names)
    smelting_sources = {}
    for recipe in read_recipe("smelting.json"):
        result = recipe.get("result")
        if result and recipe.get("ingredients"):
            smelting_sources.setdefault(result, recipe["ingredients"][0][0] if recipe["ingredients"][0] else "")
    cutting_sources = {}
    cutting_inputs = {}
    cutting_results = set()
    for recipe in read_recipe("cutting.json"):
        input_name = recipe.get("input")
        outputs = [entry.get("name") for entry in recipe.get("results", []) if entry.get("name")]
        if input_name:
            cutting_inputs.setdefault(input_name, [])
            for output in outputs:
                if output not in cutting_inputs[input_name]:
                    cutting_inputs[input_name].append(output)
                cutting_sources.setdefault(output, input_name)
        cutting_results.update(outputs)
    cooking_results = {recipe.get("result") for recipe in read_recipe("cooking.json") if recipe.get("result")}
    smelting_results = {recipe.get("result") for recipe in read_recipe("smelting.json") if recipe.get("result")}
    crafting_results = {recipe.get("result") for recipe in read_recipe("crafting.json") if recipe.get("result")}

    def operation_hint(name: str, chinese: bool) -> str:
        if name in cutting_inputs:
            outputs = "、".join(cutting_inputs[name][:3]) if chinese else ", ".join(cutting_inputs[name][:3])
            if chinese:
                return f" 切菜板操作：右击切菜板放上，再持刀右击切割，产出 {outputs}。"
            return f" Cutting board: right-click to place, then right-click with a knife to cut into {outputs}."
        if name in cooking_ingredients:
            ingredients = "、".join(cooking_ingredients[name][:6]) if chinese else ", ".join(cooking_ingredients[name][:6])
            if chinese:
                return f" 烹饪锅配方：把 {ingredients} 放入点燃炉灶上方的锅中。"
            return f" Cooking pot recipe: put {ingredients} in a pot above a lit stove."
        if name in cutting_sources:
            source = cutting_sources[name]
            return f" 切菜板产物：用刀切割 {source} 获得。" if chinese else f" Cutting board result: cut {source} with a knife."
        if name in smelting_sources:
            source = smelting_sources[name]
            return f" 熔炉配方：熔炼 {source} 获得。" if chinese else f" Furnace recipe: smelt {source}."
        if name in cutting_results:
            return " 切菜板产物：在切菜板上用刀切割获得。" if chinese else " Cutting board result: obtained by cutting on a board with a knife."
        if name in smelting_results:
            return " 熔炉/烟熏产物。" if chinese else " Furnace or smoker result."
        if name in crafting_results:
            return " 工作台合成。" if chinese else " Crafted at a workbench."
        return ""

    def item_description(d, chinese: bool) -> str:
        name = d["name"]
        nut = d["nutrition"]
        kind = d["kind"]
        special_en = {
            "bowl": "A wooden bowl for soups, stews and salads.",
            "glass_bottle": "A glass bottle for juices and drinks.",
            "sugar": "Sugar used in desserts and drinks.",
            "cocoa_beans": "Cocoa beans used in chocolate desserts.",
            "wheat": "Wheat used for dough, pies and cookies.",
            "sweet_berries": "Sweet berries. Eat them or use them in desserts.",
            "glow_berries": "Glow berries. Eat them or use them in desserts.",
            "honey_bottle": "A bottle of honey for sweet recipes.",
            "gold_ingot": "A gold ingot used for advanced recipes.",
            "gold_nugget": "A small gold nugget used in recipes.",
            "netherite_ingot": "A netherite ingot used for advanced recipes.",
            "kelp": "Kelp. Dry it or use it in cooking.",
            "dried_kelp": "Dried kelp. A cooking ingredient.",
            "bone": "A bone used in broth and other recipes.",
            "apple": "An apple. Eat it or use it in desserts.",
            "brown_mushroom": "A brown mushroom. Eat it or use it in cooking.",
            "red_mushroom": "A red mushroom. Eat it or use it in cooking.",
            "rotten_tomato": "A rotten tomato. Not very appetizing.",
        }
        special_zh = {
            "bowl": "木碗，用于盛放汤、炖菜和沙拉。",
            "glass_bottle": "玻璃瓶，用于装果汁和饮料。",
            "sugar": "糖，用于甜点和饮料。",
            "cocoa_beans": "可可豆，用于巧克力甜点。",
            "wheat": "小麦，用于制作面团、派和饼干。",
            "sweet_berries": "甜浆果，可直接食用或制作甜点。",
            "glow_berries": "发光浆果，可直接食用或制作甜点。",
            "honey_bottle": "蜂蜜瓶，用于甜味配方。",
            "gold_ingot": "金锭，用于高级配方。",
            "gold_nugget": "小金粒，用于合成。",
            "netherite_ingot": "下界合金锭，用于高级配方。",
            "kelp": "海带，可烘干或用于烹饪。",
            "dried_kelp": "干海带，烹饪食材。",
            "bone": "骨头，用于高汤等配方。",
            "apple": "苹果，可直接食用或制作甜点。",
            "brown_mushroom": "棕色蘑菇，可食用或烹饪。",
            "red_mushroom": "红色蘑菇，可食用或烹饪。",
            "rotten_tomato": "腐烂的番茄，不太好吃。",
        }
        if name in special_en:
            text = special_zh[name] if chinese else special_en[name]
            return text + operation_hint(name, chinese)
        if kind == "knife":
            return "刀，用于切菜板切割食材，也可作为武器。" if chinese else "A knife for the cutting board; also a light weapon."
        if kind == "seed":
            return "种子。右击土壤或肥沃土壤种植，成熟后收获。" if chinese else "A seed. Right-click soil or rich soil to plant; harvest when mature."
        feast = {"roast_chicken", "stuffed_pumpkin", "honey_glazed_ham", "shepherds_pie", "gleaming_salad"}
        if name in feast:
            text = "盛宴的一份。右击盛宴方块取用。" if chinese else "A portion from a feast block. Right-click the feast to take one."
            return text + operation_hint(name, chinese)
        if name.endswith("_slice"):
            text = "派或蛋糕的一片。可直接食用。" if chinese else "A slice of pie or cake. Can be eaten directly."
            return text + operation_hint(name, chinese)
        if any(k in name for k in ("juice", "cider", "cocoa", "milk_bottle")):
            text = "饮料，可直接饮用。" if chinese else "A drink. Can be consumed directly."
            return text + operation_hint(name, chinese)
        if nut > 0:
            text = f"食物：恢复约 {nut} 点饥饿值，可直接食用。" if chinese else f"Food: restores about {nut} hunger points. Can be eaten directly."
            return text + operation_hint(name, chinese)
        if any(k in name for k in ("raw_", "minced", "bacon", "chops", "slice", "fillet")):
            text = "生食材，可用于烹饪锅、煎锅或熔炉。" if chinese else "A raw ingredient for the cooking pot, skillet or furnace."
            return text + operation_hint(name, chinese)
        text = "农夫乐事食材，可用于烹饪或合成。" if chinese else "A Farmer's Delight ingredient used in cooking or crafting."
        return text + operation_hint(name, chinese)

    def block_description(cls: str, data: int, chinese: bool) -> str:
        if cls == "FDStoveBlock":
            return "炉灶。手持燃料右击点燃，可为上方的烹饪锅供热。" if chinese else "Stove. Light it with fuel to heat a cooking pot above."
        if cls == "FDCookingPotBlock":
            return "烹饪锅。放在点燃的炉灶上，右击打开烹饪界面。" if chinese else "Cooking pot. Place on a lit stove, then right-click to open the cooking GUI."
        if cls == "FDSkilletBlock":
            return "煎锅。放在热源上方，手持可煎食材右击。" if chinese else "Skillet. Place above heat, then right-click with cookable food."
        if cls == "FDCuttingBoardBlock":
            return "切菜板。先右击放食材，再手持刀右击切割。" if chinese else "Cutting board. Place food, then right-click with a knife."
        if cls == "FDBasketBlock":
            return "篮子。右击打开 27 格储物空间。" if chinese else "Basket. Right-click to open 27 storage slots."
        if cls == "FDCabinetBlock":
            return "橱柜。右击打开 27 格储物空间。" if chinese else "Cabinet. Right-click to open 27 storage slots."
        if cls == "FDCrateBlock":
            return "作物箱，装饰性储物方块。" if chinese else "A decorative crop crate."
        if cls == "FDBaleBlock":
            return "草捆/米袋，装饰性方块。" if chinese else "A decorative bale or rice bag."
        if cls == "FDRopeBlock":
            return "绳索装饰方块。" if chinese else "A decorative rope block."
        if cls == "FDSafetyNetBlock":
            return "安全网装饰方块。" if chinese else "A decorative safety net."
        if cls == "FDRopeFenceBlock":
            return "绳栅栏装饰方块。" if chinese else "A decorative rope fence."
        if cls == "FDRopeFenceGateBlock":
            return "绳栅栏门装饰方块。" if chinese else "A decorative rope fence gate."
        if cls == "FDTatamiBlock":
            return "榻榻米垫，装饰方块。" if chinese else "A tatami mat decoration."
        if cls == "FDCanvasRugBlock":
            return "帆布地毯，装饰方块。" if chinese else "A canvas rug decoration."
        if cls == "FDOrganicCompostBlock":
            return "有机堆肥，土壤装饰方块。" if chinese else "Organic compost, a soil decoration."
        if cls == "FDRichSoilBlock":
            return "肥沃土壤，作物生长更快。" if chinese else "Rich soil; crops grow faster."
        if cls == "FDRichSoilFarmlandBlock":
            return "肥沃耕地，作物生长更快。" if chinese else "Rich farmland; crops grow faster."
        if cls == "FDMushroomColonyBlock":
            return "蘑菇菌落，会逐渐生长。" if chinese else "A mushroom colony that grows over time."
        if cls == "FDCropBlock":
            names = ["卷心菜", "洋葱", "番茄", "水稻"]
            en_names = ["cabbage", "onion", "tomato", "rice"]
            t = (data >> 4) & 3
            return f"{names[t]}作物。成熟后收获产物和种子。" if chinese else f"{en_names[t].title()} crop. Harvest when mature for produce and seeds."
        if cls == "FDWildCropBlock":
            return "野生作物。破坏后获得产物和种子。" if chinese else "A wild crop. Break it for produce and seeds."
        if cls == "FDSandyShrubBlock":
            return "沙生灌木，装饰植物。" if chinese else "A sandy shrub decoration."
        if cls == "FDPieBlock":
            return "派。右击取用一片。" if chinese else "Pie. Right-click to take a slice."
        if cls == "FDFeastBlock":
            return "盛宴。右击取用一份。" if chinese else "Feast. Right-click to take a portion."
        if cls == "FDCanvasSignBlock":
            return "帆布招牌，17 种颜色，可切换站立/墙面/悬挂。" if chinese else "A canvas sign in 17 colors with standing, wall and hanging mounts."
        if cls == "FDSeedItemBlock":
            return "种子。右击土壤或肥沃土壤种植。" if chinese else "A seed. Right-click soil or rich soil to plant."
        if cls == "FDKnifeBlock":
            return "刀。用于切菜板切割食材，也可作为武器。" if chinese else "A knife for the cutting board; also a light weapon."
        return "农夫乐事方块。" if chinese else "A Farmer's Delight block."

    for d in defs:
        name = d["name"]
        en_name = en.get(f"item.farmersdelight.{name}") or en.get(f"block.farmersdelight.{name}") or name.replace("_", " ").title()
        zh_name = zh.get(f"item.farmersdelight.{name}") or zh.get(f"block.farmersdelight.{name}") or en_name
        en_blocks[f"FDItemBlock:{d['index']}"] = {"DisplayName": en_name, "Description": item_description(d, False)}
        zh_blocks[f"FDItemBlock:{d['index']}"] = {"DisplayName": zh_name, "Description": item_description(d, True)}
    # A few overrides for generated staples.
    for name, (en_name, zh_name, _) in STAPLES.items():
        for d in defs:
            if d["name"] == name:
                en_blocks[f"FDItemBlock:{d['index']}"] = {"DisplayName": en_name, "Description": item_description(d, False)}
                zh_blocks[f"FDItemBlock:{d['index']}"] = {"DisplayName": zh_name, "Description": item_description(d, True)}

    seed_order = ["cabbage_seeds", "tomato_seeds", "onion", "rice"]
    knife_order = ["flint_knife", "iron_knife", "golden_knife", "diamond_knife", "netherite_knife"]
    for i, name in enumerate(seed_order):
        en_blocks[f"FDSeedItemBlock:{i}"] = {"DisplayName": en.get(f"item.farmersdelight.{name}", name.replace("_", " ").title()), "Description": "Seed. Right-click soil or rich soil to plant."}
        zh_blocks[f"FDSeedItemBlock:{i}"] = {"DisplayName": zh.get(f"item.farmersdelight.{name}", en_blocks[f"FDSeedItemBlock:{i}"]["DisplayName"]), "Description": "种子。右击土壤或肥沃土壤种植。"}
    for i, name in enumerate(knife_order):
        en_blocks[f"FDKnifeBlock:{i}"] = {"DisplayName": en.get(f"item.farmersdelight.{name}", name.replace("_", " ").title()), "Description": "A knife for the cutting board; also a light weapon."}
        zh_blocks[f"FDKnifeBlock:{i}"] = {"DisplayName": zh.get(f"item.farmersdelight.{name}", en_blocks[f"FDKnifeBlock:{i}"]["DisplayName"]), "Description": "刀。用于切菜板切割食材，也可作为武器。"}

    def block_name(cls: str, data: int) -> str | None:
        if cls == "FDStoveBlock": return "stove"
        if cls == "FDCookingPotBlock": return "cooking_pot"
        if cls == "FDSkilletBlock": return "skillet"
        if cls == "FDCuttingBoardBlock": return "cutting_board"
        if cls == "FDBasketBlock": return "wooden_basket" if data == 0 else "bamboo_basket"
        if cls == "FDCabinetBlock":
            woods = ["oak", "spruce", "birch", "jungle", "acacia", "dark_oak", "mangrove", "cherry", "bamboo", "crimson", "warped"]
            return f"{woods[data]}_cabinet" if 0 <= data < len(woods) else None
        if cls == "FDCrateBlock":
            crops = ["carrot", "potato", "beetroot", "cabbage", "tomato", "onion"]
            return f"{crops[data]}_crate" if 0 <= data < len(crops) else None
        if cls == "FDBaleBlock":
            return ["rice_bale", "rice_bag", "straw_bale"][data] if 0 <= data < 3 else None
        if cls == "FDRopeBlock": return "rope"
        if cls == "FDSafetyNetBlock": return "safety_net"
        if cls == "FDRopeFenceBlock": return "rope_fence"
        if cls == "FDRopeFenceGateBlock": return "rope_fence_gate"
        if cls == "FDTatamiBlock":
            return ["tatami", "full_tatami_mat", "half_tatami_mat"][data] if 0 <= data < 3 else "tatami"
        if cls == "FDCanvasRugBlock": return "canvas_rug"
        if cls == "FDOrganicCompostBlock": return "organic_compost"
        if cls == "FDRichSoilBlock": return "rich_soil"
        if cls == "FDRichSoilFarmlandBlock": return "rich_soil_farmland"
        if cls == "FDMushroomColonyBlock": return "brown_mushroom_colony" if (data & 1) == 0 else "red_mushroom_colony"
        if cls == "FDCropBlock": return ["cabbages", "onions", "tomatoes", "rice"][(data >> 4) & 3]
        if cls == "FDWildCropBlock":
            names = ["wild_cabbages", "wild_onions", "wild_tomatoes", "wild_carrots", "wild_potatoes", "wild_beetroots", "wild_rice", "sandy_shrub"]
            return names[data] if 0 <= data < len(names) else None
        if cls == "FDSandyShrubBlock": return "sandy_shrub"
        if cls == "FDPieBlock":
            return ["apple_pie", "sweet_berry_cheesecake", "chocolate_pie", "pumpkin_pie"][data] if 0 <= data < 4 else None
        if cls == "FDFeastBlock":
            names = ["roast_chicken_block", "stuffed_pumpkin_block", "honey_glazed_ham_block", "shepherds_pie_block", "gleaming_salad_block", "rice_roll_medley_block"]
            return names[data] if 0 <= data < len(names) else None
        if cls == "FDCanvasSignBlock":
            colors = ["", "white", "orange", "magenta", "light_blue", "yellow", "lime", "pink", "gray", "light_gray", "cyan", "purple", "blue", "brown", "green", "red", "black"]
            return ("canvas_sign" if data == 0 else f"{colors[data]}_canvas_sign") if 0 <= data < len(colors) else None
        return None

    block_classes = {
        "FDStoveBlock": 2, "FDCookingPotBlock": 1, "FDSkilletBlock": 1, "FDCuttingBoardBlock": 1,
        "FDBasketBlock": 2, "FDCabinetBlock": 11, "FDCrateBlock": 6, "FDBaleBlock": 3,
        "FDRopeBlock": 1, "FDSafetyNetBlock": 1, "FDRopeFenceBlock": 1, "FDRopeFenceGateBlock": 1,
        "FDTatamiBlock": 3, "FDCanvasRugBlock": 1, "FDOrganicCompostBlock": 1,
        "FDRichSoilBlock": 1, "FDRichSoilFarmlandBlock": 1, "FDMushroomColonyBlock": 2,
        "FDCropBlock": 32, "FDWildCropBlock": 8, "FDSandyShrubBlock": 1,
        "FDPieBlock": 4, "FDFeastBlock": 6, "FDCanvasSignBlock": 17,
    }
    for cls, count in block_classes.items():
        for data in range(count):
            name = block_name(cls, data)
            if not name:
                continue
            en_name = en.get(f"block.farmersdelight.{name}") or name.replace("_", " ").title()
            zh_name = zh.get(f"block.farmersdelight.{name}") or en_name
            en_blocks[f"{cls}:{data}"] = {"DisplayName": en_name, "Description": block_description(cls, data, False)}
            zh_blocks[f"{cls}:{data}"] = {"DisplayName": zh_name, "Description": block_description(cls, data, True)}

    en_data = {"Blocks": en_blocks, "ContentWidgets": {
        "FDCookingPotWidget": {"1": "Cooking Pot", "2": "Inventory", "3": "Container"},
        "FDStorageWidget": {"1": "Storage", "2": "Inventory"},
    }}
    zh_data = {"Blocks": zh_blocks, "ContentWidgets": {
        "FDCookingPotWidget": {"1": "烹饪锅", "2": "背包", "3": "容器"},
        "FDStorageWidget": {"1": "储物", "2": "背包"},
    }}

    with open(os.path.join(OUT, "Lang", "en-US.json"), "w", encoding="utf-8") as f:
        json.dump(en_data, f, ensure_ascii=False, indent=2)
    with open(os.path.join(OUT, "Lang", "zh-CN.json"), "w", encoding="utf-8") as f:
        json.dump(zh_data, f, ensure_ascii=False, indent=2)

    print("items:", len(defs), "block textures:", len(slots))


if __name__ == "__main__":
    main()
