"""Generate a small .cr file for staples that have no Survivalcraft equivalent."""
import json
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
items = json.load(open(os.path.join(ROOT, "Assets", "FD", "items.json"), encoding="utf-8"))
by_name = {item["name"]: item["index"] for item in items}

recipes = []
if "bowl" in by_name:
    recipes.append(("bowl", 4, "planks", '"aaa"', "a"))
if "glass_bottle" in by_name:
    recipes.append(("glass_bottle", 3, "glass", '"aaa"', "a"))
if "kelp" in by_name:
    recipes.append(("kelp", 1, "kelp", '"a"', "a"))

lines = [
    '<Mod xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="https://gitee.com/SC-SPM/SurvivalcraftApi/raw/SCAPI1.9/Survivalcraft/Content/Assets/CraftingRecipes.xsd">',
]
for name, count, ingredient, pattern, key in recipes:
    lines.append(f'  <Recipe Result="FDItemBlock:{by_name[name]}" ResultCount="{count}" RequiredHeatLevel="0" {key}="{ingredient}" Description="[0]">')
    lines.append(f"    {pattern}")
    lines.append("  </Recipe>")
lines.append("</Mod>")
path = os.path.join(ROOT, "Assets", "FDStaples.cr")
with open(path, "w", encoding="utf-8") as f:
    f.write("\n".join(lines))
print("Wrote", path, "recipes", len(recipes))
