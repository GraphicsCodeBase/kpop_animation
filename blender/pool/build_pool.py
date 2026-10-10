"""Build the S2 pool set (REF-01, Zone A) and render a look-test frame.

Run headless:
  blender -b -P blender/pool/build_pool.py -- --out captures/blender/pool/pool_v001.png

Everything is generated from this script, so the scene can be rebuilt at any time.
Units are metres. Layout (top view):
  - Wall A (stairs wall) runs along +X at y = 0; the pool lies on the -Y side.
  - Wall B (lounger wall) runs along -Y at x = 0; the pool lies on the -X side.
  - The kiosk sits near the corner where A and B meet. Lane lines run parallel to wall A.
"""

import math
import os
import random
import sys

import bmesh
import bpy
from mathutils import Vector

# ---------------------------------------------------------------- settings
POOL_X = 14.0      # interior size along X (x from -POOL_X to 0)
POOL_Y = 16.0      # interior size along Y (y from -POOL_Y to 0)
WALL_H = 2.1       # pool wall height
WALL_T = 0.3
TILE = 0.022       # mosaic tile size (m)
LANES_Y = [-1.8, -4.3, -6.8, -9.3, -11.8]
LANE_W = 0.24

CAM_LOC = (-6.0, -6.4, 5.2)
CAM_TARGET = (-2.3, -2.3, -0.7)
CAM_LENS = 24.0

RES = (1440, 1080)     # 4:3, matches the source video
RES_PCT = 75
SAMPLES = 96

random.seed(7)


def arg_out():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    if "--out" in argv:
        return argv[argv.index("--out") + 1]
    return "captures/blender/pool/pool_latest.png"


def srgb(hexstr, a=1.0):
    """Hex sRGB -> linear RGBA (what Blender colour sockets expect)."""
    hexstr = hexstr.lstrip("#")
    out = []
    for i in (0, 2, 4):
        c = int(hexstr[i:i + 2], 16) / 255.0
        out.append(c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4)
    return (*out, a)


# ---------------------------------------------------------------- scene reset
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
col = scene.collection


def link(obj):
    col.objects.link(obj)
    return obj


# ---------------------------------------------------------------- materials
def principled(name, color, rough=0.5, **kw):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = srgb(color) if isinstance(color, str) else color
    b.inputs["Roughness"].default_value = rough
    for k, v in kw.items():
        b.inputs[k].default_value = v
    return m


def mosaic(name, palette, axes="xy", grout="#E9ECE8", tile=TILE, rough=0.18):
    """World-space mosaic: random colour per tile from a weighted palette, plus grout lines.

    palette: list of (hex, weight). axes: which two world axes form the tile grid.
    """
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    N, L = nt.nodes, nt.links
    bsdf = N["Principled BSDF"]
    bsdf.inputs["Roughness"].default_value = rough
    bsdf.inputs["Specular IOR Level"].default_value = 0.6

    geo = N.new("ShaderNodeNewGeometry")
    sep = N.new("ShaderNodeSeparateXYZ")
    L.new(geo.outputs["Position"], sep.inputs[0])
    comb = N.new("ShaderNodeCombineXYZ")
    idx = {"x": 0, "y": 1, "z": 2}
    L.new(sep.outputs[idx[axes[0]]], comb.inputs[0])
    L.new(sep.outputs[idx[axes[1]]], comb.inputs[1])

    scale = N.new("ShaderNodeVectorMath"); scale.operation = "SCALE"
    scale.inputs[3].default_value = 1.0 / tile
    L.new(comb.outputs[0], scale.inputs[0])

    cell = N.new("ShaderNodeVectorMath"); cell.operation = "FLOOR"
    L.new(scale.outputs[0], cell.inputs[0])
    frac = N.new("ShaderNodeVectorMath"); frac.operation = "FRACTION"
    L.new(scale.outputs[0], frac.inputs[0])

    noise = N.new("ShaderNodeTexWhiteNoise"); noise.noise_dimensions = "3D"
    L.new(cell.outputs[0], noise.inputs["Vector"])

    ramp = N.new("ShaderNodeValToRGB")
    ramp.color_ramp.interpolation = "CONSTANT"
    total = sum(w for _, w in palette)
    els = ramp.color_ramp.elements
    els[0].position = 0.0
    els[0].color = srgb(palette[0][0])
    els[1].position = palette[0][1] / total
    els[1].color = srgb(palette[1][0])
    acc = (palette[0][1] + palette[1][1]) / total
    for hx, w in palette[2:]:
        e = els.new(min(acc, 0.999))
        e.color = srgb(hx)
        acc += w / total
    L.new(noise.outputs["Value"], ramp.inputs["Fac"])

    # grout mask: 1 where either fractional coordinate is near the tile edge
    fsep = N.new("ShaderNodeSeparateXYZ")
    L.new(frac.outputs[0], fsep.inputs[0])
    g = 0.09
    lx = N.new("ShaderNodeMath"); lx.operation = "LESS_THAN"; lx.inputs[1].default_value = g
    ly = N.new("ShaderNodeMath"); ly.operation = "LESS_THAN"; ly.inputs[1].default_value = g
    L.new(fsep.outputs[0], lx.inputs[0])
    L.new(fsep.outputs[1], ly.inputs[0])
    gm = N.new("ShaderNodeMath"); gm.operation = "MAXIMUM"
    L.new(lx.outputs[0], gm.inputs[0])
    L.new(ly.outputs[0], gm.inputs[1])

    mix = N.new("ShaderNodeMix"); mix.data_type = "RGBA"
    L.new(gm.outputs[0], mix.inputs[0])
    L.new(ramp.outputs["Color"], mix.inputs[6])
    mix.inputs[7].default_value = srgb(grout)
    L.new(mix.outputs[2], bsdf.inputs["Base Color"])

    # slight recess at the grout so tiles catch light
    bump = N.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.25
    bump.inputs["Distance"].default_value = 0.002
    inv = N.new("ShaderNodeMath"); inv.operation = "SUBTRACT"; inv.inputs[0].default_value = 1.0
    L.new(gm.outputs[0], inv.inputs[1])
    L.new(inv.outputs[0], bump.inputs["Height"])
    L.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    return m


POOL_PALETTE = [("#E9F4F0", 14), ("#B4E2DC", 30), ("#8ED3CF", 28), ("#6CBFD6", 18), ("#3E8DC8", 10)]
LANE_PALETTE = [("#1B357E", 65), ("#25499A", 22), ("#132A62", 10), ("#4F79BE", 3)]

M_FLOOR = mosaic("mosaic_floor", POOL_PALETTE, "xy", grout="#DCEBE8")
M_WALL_A = mosaic("mosaic_wall_xz", POOL_PALETTE, "xz", grout="#DCEBE8")   # walls facing ±Y
M_WALL_B = mosaic("mosaic_wall_yz", POOL_PALETTE, "yz", grout="#DCEBE8")   # walls facing ±X
M_LANE = mosaic("mosaic_lane", LANE_PALETTE, "xy", grout="#AFC0DA")

M_CREAM = principled("cream", "#EEE6D5", 0.55)
M_DECK = principled("deck", "#E7E1D3", 0.7)
M_ORANGE_STRIPE = principled("stair_stripe", "#E8833A", 0.5)
M_CHROME = principled("chrome", "#D9DCE0", 0.15, Metallic=1.0)
M_SEAT = principled("seat_orange", "#F28A1C", 0.3)
M_METAL = principled("metal_grey", "#8A8F96", 0.35, Metallic=0.8)
M_WHITE = principled("white_paint", "#F4F2EE", 0.4)
M_RED = principled("kiosk_red", "#D5322B", 0.35)
M_DARK = principled("panel_dark", "#3A3F4A", 0.3)
M_CORAL = principled("cushion_coral", "#E0606A", 0.75)
M_MINT = principled("mint", "#8FD3C3", 0.4)
M_FIG = principled("scale_figure", "#B7B9BE", 0.6)


def balloon_mat(name, hx):
    return principled(name, hx, 0.12, **{"Transmission Weight": 0.55, "Coat Weight": 0.4})


def striped_canopy_mat():
    m = bpy.data.materials.new("parasol_stripes")
    m.use_nodes = True
    nt = m.node_tree
    N, L = nt.nodes, nt.links
    bsdf = N["Principled BSDF"]
    bsdf.inputs["Roughness"].default_value = 0.8
    tc = N.new("ShaderNodeTexCoord")
    grad = N.new("ShaderNodeTexGradient"); grad.gradient_type = "RADIAL"
    L.new(tc.outputs["Object"], grad.inputs["Vector"])
    mul = N.new("ShaderNodeMath"); mul.operation = "MULTIPLY"; mul.inputs[1].default_value = 16
    L.new(grad.outputs["Fac"], mul.inputs[0])
    fr = N.new("ShaderNodeMath"); fr.operation = "FRACT"
    L.new(mul.outputs[0], fr.inputs[0])
    st = N.new("ShaderNodeMath"); st.operation = "GREATER_THAN"; st.inputs[1].default_value = 0.5
    L.new(fr.outputs[0], st.inputs[0])
    mix = N.new("ShaderNodeMix"); mix.data_type = "RGBA"
    L.new(st.outputs[0], mix.inputs[0])
    mix.inputs[6].default_value = srgb("#F1EFE4")
    mix.inputs[7].default_value = srgb("#9CB79B")
    L.new(mix.outputs[2], bsdf.inputs["Base Color"])
    return m


# ---------------------------------------------------------------- geometry helpers
def box(name, center, size, mat):
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    bm.to_mesh(me)
    bm.free()
    ob = link(bpy.data.objects.new(name, me))
    ob.location = center
    ob.data.materials.append(mat)
    return ob


def cyl(name, center, radius, depth, mat, rot=(0, 0, 0), verts=32):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth,
                                        location=center, rotation=rot)
    ob = bpy.context.active_object
    ob.name = name
    ob.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return ob


def sphere(name, center, radius, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=48, ring_count=24, radius=radius, location=center)
    ob = bpy.context.active_object
    ob.name = name
    ob.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return ob


def tube_between(name, a, b, radius, mat):
    a, b = Vector(a), Vector(b)
    d = b - a
    ob = cyl(name, (a + b) / 2, radius, d.length, mat, verts=12)
    ob.rotation_mode = "QUATERNION"
    ob.rotation_quaternion = d.to_track_quat("Z", "Y")
    return ob


# ---------------------------------------------------------------- pool shell
x0, x1 = -POOL_X, 0.0
y0, y1 = -POOL_Y, 0.0
cx, cy = (x0 + x1) / 2, (y0 + y1) / 2

box("pool_floor", (cx, cy, -0.05), (POOL_X, POOL_Y, 0.1), M_FLOOR)
box("wall_A", (cx, y1 + WALL_T / 2, WALL_H / 2), (POOL_X + 2 * WALL_T, WALL_T, WALL_H), M_WALL_A)
box("wall_far_y", (cx, y0 - WALL_T / 2, WALL_H / 2), (POOL_X + 2 * WALL_T, WALL_T, WALL_H), M_WALL_A)
box("wall_B", (x1 + WALL_T / 2, cy, WALL_H / 2), (WALL_T, POOL_Y, WALL_H), M_WALL_B)
box("wall_far_x", (x0 - WALL_T / 2, cy, WALL_H / 2), (WALL_T, POOL_Y, WALL_H), M_WALL_B)

# rounded cream coping along the wall tops
for name, a, b in [("coping_A", (x0, y1 + 0.12, WALL_H), (x1, y1 + 0.12, WALL_H)),
                   ("coping_far_y", (x0, y0 - 0.12, WALL_H), (x1, y0 - 0.12, WALL_H)),
                   ("coping_B", (x1 + 0.12, y0, WALL_H), (x1 + 0.12, y1, WALL_H)),
                   ("coping_far_x", (x0 - 0.12, y0, WALL_H), (x0 - 0.12, y1, WALL_H))]:
    tube_between(name, a, b, 0.16, M_CREAM)

# deck around the pool
D = 12.0
box("deck_A", (cx, y1 + D / 2, WALL_H - 0.05), (POOL_X + 2 * D, D, 0.1), M_DECK)
box("deck_far_y", (cx, y0 - D / 2, WALL_H - 0.05), (POOL_X + 2 * D, D, 0.1), M_DECK)
box("deck_B", (x1 + D / 2, cy, WALL_H - 0.05), (D, POOL_Y, 0.1), M_DECK)
box("deck_far_x", (x0 - D / 2, cy, WALL_H - 0.05), (D, POOL_Y, 0.1), M_DECK)

# ---------------------------------------------------------------- lane lines (with T ends)
for i, ly in enumerate(LANES_Y):
    xa, xb = x0 + 2.0, x1 - 1.4
    box(f"lane_{i}", ((xa + xb) / 2, ly, 0.003), (xb - xa, LANE_W, 0.006), M_LANE)
    for j, xe in enumerate((xa, xb)):
        box(f"lane_{i}_T{j}", (xe, ly, 0.003), (LANE_W, 1.0, 0.006), M_LANE)

# ---------------------------------------------------------------- stairs into the pool (along wall A)
STEPS = 8
st_x_top, st_x_bot = -2.0, -5.0        # top step near the kiosk, descending toward -X
st_depth = 1.0                          # how far the stair block sticks out from wall A
rise = WALL_H / STEPS
run = (st_x_top - st_x_bot) / STEPS
for s in range(STEPS):
    h = WALL_H - s * rise               # top of this step
    xs = st_x_top - s * run
    # tread: cream slab with an orange nosing stripe on the front edge
    box(f"step_{s}", (xs - run / 2, -st_depth / 2, h / 2), (run, st_depth, h), M_WALL_A)
    box(f"tread_{s}", (xs - run / 2, -st_depth / 2, h + 0.015), (run + 0.02, st_depth + 0.02, 0.03), M_CREAM)
    box(f"nosing_{s}", (xs - run + 0.03, -st_depth / 2, h + 0.032), (0.06, st_depth + 0.02, 0.008), M_ORANGE_STRIPE)
# chrome handrail on the open side
rail_y = -st_depth - 0.05
tube_between("handrail", (st_x_bot - 0.3, rail_y, 0.95), (st_x_top + 0.4, rail_y, WALL_H + 0.9), 0.025, M_CHROME)
for px, pz in [(st_x_bot - 0.2, 0.0), (st_x_top + 0.3, WALL_H)]:
    tube_between("rail_post", (px, rail_y, pz), (px, rail_y, pz + 0.95), 0.025, M_CHROME)

# ---------------------------------------------------------------- orange stadium seats (left, against wall A)
for k in range(2):
    sx = -6.6 + k * 0.55
    box(f"seat_{k}", (sx, -0.42, 0.45), (0.46, 0.42, 0.05), M_SEAT)
    box(f"seat_back_{k}", (sx, -0.2, 0.75), (0.46, 0.05, 0.5), M_SEAT)
box("seat_beam", (-6.32, -0.35, 0.38), (1.2, 0.06, 0.06), M_METAL)
for lx in (-6.8, -5.85):
    box("seat_leg", (lx, -0.35, 0.19), (0.05, 0.05, 0.38), M_METAL)

# ---------------------------------------------------------------- kiosk / booth (near the corner)
# KS scales the whole kiosk; 0.72 brings it to ~1.3 m tall, matching ref01_pool_01 (Unity overlay check).
KS = 0.72
kx, ky = -0.95, -0.45
box("kiosk_base", (kx, ky, 0.35 * KS), (0.62 * KS, 0.52 * KS, 0.7 * KS), M_RED)
box("kiosk_body", (kx, ky, 1.15 * KS), (0.6 * KS, 0.5 * KS, 0.9 * KS), M_WHITE)
box("kiosk_panel", (kx, ky - 0.26 * KS, 1.15 * KS), (0.36 * KS, 0.02, 0.42 * KS), M_DARK)
box("kiosk_sign", (kx, ky - 0.26 * KS, 1.5 * KS), (0.4 * KS, 0.02, 0.12 * KS), M_RED)
cap = sphere("kiosk_cap", (kx, ky, 1.6 * KS), 0.3 * KS, M_WHITE)
cap.scale = (1.0, 0.85, 0.55)

# ---------------------------------------------------------------- sun lounger (against wall B)
lx0, lx1, lyc = -2.7, -0.5, -2.6
box("lounger_cushion", ((lx0 + lx1 - 0.6) / 2, lyc, 0.42), (lx1 - lx0 - 0.6, 0.7, 0.1), M_CORAL)
back = box("lounger_back", (lx1 - 0.35, lyc, 0.75), (0.75, 0.7, 0.1), M_CORAL)
back.rotation_euler = (0, math.radians(55), 0)
for fx in (lx0 + 0.05, lx1 - 0.15):
    for fy in (lyc - 0.33, lyc + 0.33):
        tube_between("lounger_leg", (fx, fy, 0.0), (fx, fy, 0.38), 0.02, M_WHITE)
for fy in (lyc - 0.36, lyc + 0.36):
    tube_between("lounger_rail", (lx0, fy, 0.38), (lx1 - 0.1, fy, 0.38), 0.02, M_WHITE)

# ---------------------------------------------------------------- side table + parasol
cyl("table_top", (-0.55, -3.65, 0.55), 0.26, 0.04, M_MINT)
cyl("table_stem", (-0.55, -3.65, 0.27), 0.06, 0.54, M_MINT)
cyl("table_foot", (-0.55, -3.65, 0.02), 0.2, 0.04, M_MINT)

px, py = -0.45, -4.7
cyl("parasol_base", (px, py, 0.04), 0.3, 0.08, M_WHITE)
tube_between("parasol_pole", (px, py, 0.0), (px - 0.25, py + 0.15, 2.35), 0.025, M_ORANGE_STRIPE)
bpy.ops.mesh.primitive_cone_add(vertices=64, radius1=1.25, radius2=0.03, depth=0.45,
                                location=(px - 0.25, py + 0.15, 2.3))
canopy = bpy.context.active_object
canopy.name = "parasol_canopy"
canopy.rotation_euler = (math.radians(-6), math.radians(8), 0)
canopy.data.materials.append(striped_canopy_mat())
bpy.ops.object.shade_smooth()

# ---------------------------------------------------------------- balloons on the floor
for name, pos, r, hx in [
    ("balloon_lavender", (-2.1, -1.2), 0.28, "#C7B8E8"),
    ("balloon_mint_a", (-2.6, -2.1), 0.32, "#BFE7B8"),
    ("balloon_mint_b", (-0.55, -6.2), 0.32, "#BCE6C8"),
    ("balloon_pink", (-0.6, -7.7), 0.34, "#F2C4D6"),
    ("balloon_lilac", (-0.8, -9.0), 0.3, "#CFC4EE"),
]:
    sphere(name, (pos[0], pos[1], r * 0.95), r, balloon_mat(name, hx))

# ---------------------------------------------------------------- bunting (two sagging strings with flags)
FLAG_COLS = ["#F28DB2", "#2BB3C0", "#F5C842", "#B79CE0", "#3D7FD1", "#E04848"]
M_FLAGS = [principled(f"flag_{i}", c, 0.7) for i, c in enumerate(FLAG_COLS)]
M_STRING = principled("string", "#F4F2EE", 0.6)


def bunting(name, a, b, sag, n_flags):
    a, b = Vector(a), Vector(b)
    pts = []
    for i in range(41):
        t = i / 40
        p = a.lerp(b, t)
        p.z -= sag * 4 * t * (1 - t)
        pts.append(p)
    for i in range(40):
        tube_between(f"{name}_str", pts[i], pts[i + 1], 0.006, M_STRING)
    side = (b - a).normalized()
    for f in range(n_flags):
        t = (f + 0.5) / n_flags
        p = a.lerp(b, t)
        p.z -= sag * 4 * t * (1 - t)
        w, h = 0.16, 0.34
        me = bpy.data.meshes.new(f"{name}_flag")
        bm = bmesh.new()
        v1 = bm.verts.new(p - side * w)
        v2 = bm.verts.new(p + side * w)
        v3 = bm.verts.new(p + Vector((0, 0, -h)))
        bm.faces.new((v1, v2, v3))
        bm.to_mesh(me)
        bm.free()
        ob = link(bpy.data.objects.new(f"{name}_flag_{f}", me))
        ob.data.materials.append(M_FLAGS[f % len(M_FLAGS)])


bunting("bunting_A", (-8.0, -1.1, 3.1), (0.3, -1.6, 3.1), 0.45, 18)
bunting("bunting_near", (-7.4, -2.6, 2.9), (-6.4, -7.6, 2.9), 0.35, 9)

# ---------------------------------------------------------------- scale figures (stand-ins for the dancers)
for name, (fx, fy) in [("figure_dahyun", (-3.7, -2.9)), ("figure_chaeyoung", (-2.1, -4.3))]:
    cyl(f"{name}_body", (fx, fy, 0.72), 0.2, 1.44, M_FIG)
    sphere(f"{name}_head", (fx, fy, 1.56), 0.12, M_FIG)

# ---------------------------------------------------------------- lighting
world = bpy.data.worlds.new("world")
scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes["Background"]
bg.inputs["Color"].default_value = srgb("#DCE7EC")
bg.inputs["Strength"].default_value = 0.9

bpy.ops.object.light_add(type="AREA", location=(cx, cy, 9.0))
sky = bpy.context.active_object
sky.name = "skylight"
sky.data.shape = "RECTANGLE"
sky.data.size, sky.data.size_y = POOL_X + 6, POOL_Y + 6
sky.data.energy = 3400
sky.data.color = srgb("#F4F8FF")[:3]

bpy.ops.object.light_add(type="SUN", location=(0, 0, 10))
sun = bpy.context.active_object
sun.name = "soft_sun"
sun.data.energy = 1.4
sun.data.angle = math.radians(35)
sun.rotation_euler = (math.radians(30), math.radians(-15), math.radians(40))

# ---------------------------------------------------------------- camera
cam_data = bpy.data.cameras.new("cam_wide_high")
cam_data.lens = CAM_LENS
cam = link(bpy.data.objects.new("cam_wide_high", cam_data))
cam.location = CAM_LOC
direction = Vector(CAM_TARGET) - Vector(CAM_LOC)
cam.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
scene.camera = cam

# ---------------------------------------------------------------- render settings
scene.render.engine = "CYCLES"
scene.cycles.samples = SAMPLES
scene.cycles.use_denoising = True
scene.render.resolution_x, scene.render.resolution_y = RES
scene.render.resolution_percentage = RES_PCT
scene.view_settings.view_transform = "AgX"
scene.view_settings.look = "AgX - Base Contrast"
scene.view_settings.exposure = 0.1

prefs = bpy.context.preferences.addons["cycles"].preferences
for backend in ("OPTIX", "CUDA", "HIP", "ONEAPI"):
    try:
        prefs.compute_device_type = backend
        prefs.get_devices()
        gpus = [d for d in prefs.devices if d.type == backend]
        if gpus:
            for d in prefs.devices:
                d.use = d.type == backend
            scene.cycles.device = "GPU"
            print(f"[build_pool] rendering on {backend}: {[d.name for d in gpus]}")
            break
    except TypeError:
        continue
else:
    scene.cycles.device = "CPU"
    print("[build_pool] no GPU backend found, rendering on CPU")

# ---------------------------------------------------------------- save + render
here = os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(here, "pool.blend"))

out = os.path.abspath(arg_out())
os.makedirs(os.path.dirname(out), exist_ok=True)
scene.render.filepath = out
bpy.ops.render.render(write_still=True)
print(f"[build_pool] wrote {out}")
