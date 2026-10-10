"""Export the pool props from pool.blend as one FBX each, for Unity.

Blender is the prop workshop (pipeline decision 2026-10-10); Unity owns the layout.
Each prop is exported with its pivot at the bottom centre of its bounding box, so
Unity can drop it on a floor position. Layout-only objects (shell, lanes, stairs,
bunting, lights, camera) are not exported.

Run (from the repo root):
  blender -b blender/pool/pool.blend --python blender/pool/export_props.py
"""
import os

import bpy
from mathutils import Vector

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.normpath(os.path.join(HERE, "..", "..", "unity", "Assets", "_Project", "Sets", "Pool", "Props"))

# FBX file name -> object-name prefixes that make up the prop
PROPS = {
    "PoolSeats": ("seat_",),
    "PoolKiosk": ("kiosk_",),
    "PoolLounger": ("lounger_",),
    "PoolSideTable": ("table_",),
    "PoolParasol": ("parasol_",),
}
# one FBX per balloon, named after the object (balloon_lavender -> Balloon_Lavender)
BALLOON_PREFIX = "balloon_"


def world_bounds(objs):
    pts = [o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    return lo, hi


def export(name, objs):
    lo, hi = world_bounds(objs)
    pivot = Vector(((lo.x + hi.x) / 2, (lo.y + hi.y) / 2, lo.z))
    for o in objs:
        o.location -= pivot
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    path = os.path.join(OUT_DIR, name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        object_types={"MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        bake_anim=False,
    )
    for o in objs:
        o.location += pivot
    size = hi - lo
    print(f"[export_props] {name}: {len(objs)} objects, size {size.x:.2f} x {size.y:.2f} x {size.z:.2f} m, "
          f"Blender pivot ({pivot.x:.2f}, {pivot.y:.2f}, {pivot.z:.2f}) -> {path}")


os.makedirs(OUT_DIR, exist_ok=True)
meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
for name, prefixes in PROPS.items():
    objs = [o for o in meshes if o.name.startswith(prefixes)]
    if not objs:
        raise SystemExit(f"[export_props] no objects found for {name}")
    export(name, objs)
for o in [o for o in meshes if o.name.startswith(BALLOON_PREFIX)]:
    colour = o.name[len(BALLOON_PREFIX):].split(".")[0]
    export("Balloon_" + "".join(part.capitalize() for part in colour.split("_")), [o])
