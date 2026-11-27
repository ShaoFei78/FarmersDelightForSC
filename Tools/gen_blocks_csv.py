"""Generate FD block table rows using the official SC template header."""
from __future__ import annotations

import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TEMPLATE = os.path.abspath(os.path.join(ROOT, "..", "SurvivalcraftTemplateModForAPI-API1.9", "SurvivalcraftTemplateModForAPI", "Assets", "TemplateBlocksData.csv"))
OUT = os.path.join(ROOT, "Assets", "FDBlocksData.csv")

header = open(TEMPLATE, encoding="utf-8-sig").read().splitlines()[0]
columns = header.split(";")

item = {
    "DefaultCategory": "Items",
    "DefaultIconBlockOffset": "0,0,0",
    "DefaultIconViewOffset": "0, 0.5, 1",
    "DefaultIconViewScale": "0.85",
    "FirstPersonScale": "0.4",
    "FirstPersonOffset": "0.5, -0.5, -0.6",
    "FirstPersonRotation": "0, -70, 0",
    "InHandScale": "0.4",
    "InHandOffset": "0, 0.12, 0",
    "InHandRotation": "0, 0, 0",
    "IsCollidable": "FALSE",
    "IsPlaceable": "FALSE",
    "IsDiggingTransparent": "FALSE",
    "IsPlacementTransparent": "TRUE",
    "DefaultIsInteractive": "FALSE",
    "IsEditable": "FALSE",
    "IsNonDuplicable": "FALSE",
    "IsGatherable": "FALSE",
    "HasCollisionBehavior": "FALSE",
    "KillsWhenStuck": "FALSE",
    "IsFluidBlocker": "FALSE",
    "IsTransparent": "TRUE",
    "GenerateFacesForSameNeighbors": "FALSE",
    "DefaultShadowStrength": "0",
    "LightAttenuation": "0",
    "DefaultEmittedLightAmount": "0",
    "ObjectShadowStrength": "0",
    "DefaultDropContent": "",
    "DefaultDropCount": "1",
    "RequiredToolLevel": "0",
    "MaxStacking": "40",
    "DefaultSoundMaterialName": "",
    "DestructionDebrisScale": "0",
    "DefaultTextureSlot": "0",
    "DefaultCreativeData": "0",
}

cube = {
    "DefaultCategory": "Construction",
    "DefaultIconBlockOffset": "0,0,0",
    "DefaultIconViewOffset": "1, 1, 1",
    "DefaultIconViewScale": "1",
    "FirstPersonScale": "0.4",
    "FirstPersonOffset": "0.5, -0.5, -0.6",
    "FirstPersonRotation": "0, 40, 0",
    "InHandScale": "0.3",
    "InHandOffset": "0, 0.12, 0",
    "InHandRotation": "0, 0, 45",
    "IsCollidable": "TRUE",
    "IsPlaceable": "TRUE",
    "IsDiggingTransparent": "FALSE",
    "IsPlacementTransparent": "FALSE",
    "DefaultIsInteractive": "FALSE",
    "IsEditable": "FALSE",
    "IsNonDuplicable": "FALSE",
    "IsGatherable": "FALSE",
    "HasCollisionBehavior": "FALSE",
    "KillsWhenStuck": "FALSE",
    "IsFluidBlocker": "TRUE",
    "IsTransparent": "FALSE",
    "GenerateFacesForSameNeighbors": "FALSE",
    "DefaultShadowStrength": "15",
    "LightAttenuation": "0",
    "DefaultEmittedLightAmount": "0",
    "ObjectShadowStrength": "1",
    "DefaultDropContent": "#",
    "DefaultDropCount": "1",
    "RequiredToolLevel": "0",
    "MaxStacking": "40",
    "DefaultSoundMaterialName": "Wood",
    "DestructionDebrisScale": "1",
    "DefaultTextureSlot": "0",
    "DefaultCreativeData": "0",
}

plant = {
    "DefaultCategory": "Plants",
    "DefaultIconBlockOffset": "0,0,0",
    "DefaultIconViewOffset": "0, 0, 1",
    "DefaultIconViewScale": "1",
    "FirstPersonScale": "0.4",
    "FirstPersonOffset": "0.5, -0.5, -0.6",
    "FirstPersonRotation": "0, -70, 0",
    "InHandScale": "0.4",
    "InHandOffset": "0, 0.12, 0",
    "InHandRotation": "0, 0, 0",
    "IsCollidable": "FALSE",
    "IsPlaceable": "TRUE",
    "IsDiggingTransparent": "FALSE",
    "IsPlacementTransparent": "TRUE",
    "DefaultIsInteractive": "FALSE",
    "IsEditable": "FALSE",
    "IsNonDuplicable": "FALSE",
    "IsGatherable": "FALSE",
    "HasCollisionBehavior": "FALSE",
    "KillsWhenStuck": "FALSE",
    "IsFluidBlocker": "FALSE",
    "IsTransparent": "TRUE",
    "GenerateFacesForSameNeighbors": "FALSE",
    "DefaultShadowStrength": "4",
    "LightAttenuation": "1",
    "DefaultEmittedLightAmount": "0",
    "ObjectShadowStrength": "0",
    "DefaultDropContent": "#",
    "DefaultDropCount": "1",
    "RequiredToolLevel": "0",
    "MaxStacking": "40",
    "DefaultSoundMaterialName": "Plant",
    "DestructionDebrisScale": "1.5",
    "DefaultTextureSlot": "0",
    "DefaultCreativeData": "0",
}

flat = dict(item)
flat.update({
    "DefaultCategory": "Construction",
    "IsCollidable": "FALSE",
    "IsPlaceable": "TRUE",
    "IsPlacementTransparent": "TRUE",
    "IsFluidBlocker": "FALSE",
    "IsTransparent": "TRUE",
    "DefaultDropContent": "#",
    "DefaultSoundMaterialName": "Cloth",
})

rows = []


def add(name, base, **overrides):
    row = dict(base)
    row["Class Name"] = name
    row["DefaultDisplayName"] = ""
    row["Behaviors"] = ""
    row["DisplayOrder"] = "0"
    row["CraftingId"] = ""
    row.update(overrides)
    rows.append(row)


add("FDItemBlock", item, CraftingId="fditem")
add("FDSeedItemBlock", flat, DefaultCategory="Plants", CraftingId="fdseed", IsPlaceable="TRUE")
add("FDKnifeBlock", item, DefaultCategory="Tools", MaxStacking="1", CraftingId="fdknife")

add("FDStoveBlock", cube, DefaultIsInteractive="TRUE", DefaultEmittedLightAmount="0", CraftingId="fdstove")
add("FDCookingPotBlock", cube, DefaultIsInteractive="TRUE", MaxStacking="1", CraftingId="fdcookingpot")
add("FDSkilletBlock", cube, DefaultIsInteractive="TRUE", MaxStacking="1", CraftingId="fdskillet", DefaultSoundMaterialName="Metal")
add("FDCuttingBoardBlock", cube, DefaultIsInteractive="TRUE", CraftingId="fdcuttingboard")
add("FDBasketBlock", cube, DefaultIsInteractive="TRUE", CraftingId="fdbasket")
add("FDCabinetBlock", cube, DefaultIsInteractive="TRUE", CraftingId="fdcabinet")
add("FDCrateBlock", cube, CraftingId="fdcrate")
add("FDBaleBlock", cube, CraftingId="fdbale")
add("FDRopeBlock", plant, CraftingId="fdrope", DefaultSoundMaterialName="Cloth")
add("FDSafetyNetBlock", flat, CraftingId="fdsafetynet")
add("FDRopeFenceBlock", cube, CraftingId="fdropefence")
add("FDRopeFenceGateBlock", cube, CraftingId="fdropefencegate")
add("FDTatamiBlock", flat, CraftingId="fdtatami")
add("FDCanvasRugBlock", flat, CraftingId="fdcarpet")
add("FDOrganicCompostBlock", cube, CraftingId="fdcompost")
add("FDRichSoilBlock", cube, CraftingId="fdrichsoil")
add("FDRichSoilFarmlandBlock", cube, CraftingId="fdrichfarmland")
add("FDMushroomColonyBlock", plant, CraftingId="fdmushroomcolony")
add("FDCropBlock", plant, CraftingId="fdcrop", DefaultDropContent="", MaxStacking="40")
add("FDWildCropBlock", plant, CraftingId="fdwildcrop")
add("FDSandyShrubBlock", plant, CraftingId="fdsandyshrub")
add("FDPieBlock", cube, DefaultIsInteractive="TRUE", CraftingId="fdfoodpie", DefaultCategory="Food")
add("FDFeastBlock", cube, DefaultIsInteractive="TRUE", CraftingId="fdfeast", DefaultCategory="Food")
add("FDCanvasSignBlock", plant, CraftingId="fdsign")
add("FDCraftingProxyBlock", item, CraftingId="")

lines = [header]
for row in rows:
    lines.append(";".join(row.get(c, "") for c in columns))
with open(OUT, "w", encoding="utf-8-sig", newline="\r\n") as f:
    f.write("\n".join(lines))
print("Wrote", OUT, "rows", len(rows))
