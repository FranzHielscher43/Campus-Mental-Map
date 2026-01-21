import os, json, csv
import numpy as np
from plyfile import PlyData, PlyElement

# =======================
# SETTINGS (HQ for Quest)
# =======================
INPUT_PLY = "gs_VR_Room_v2.ply"
OUT_DIR   = "hq_tiles_quest"

# Tile-Grid: moderat halten (sonst explodiert die Anzahl Tiles)
TILE_SIZE = 100.0       # 5x5m (wenn zu viele Tiles -> 8.0 nehmen)

# Für HQ: KEIN Overlap, KEIN Wegwerfen (sonst Qualitätsverlust / Sort-Artefakte)
OVERLAP  = 0.0
MIN_KEEP = 0

# Chunk-Limit: Quest-friendly (nicht zu groß)
TARGET_MAX = 20000    # 15k-20k ist ein guter Bereich
MAX_DEPTH  = 12
MIN_EXTENT = 1e-4
# =======================


def write_chunk(path, verts, text):
    el = PlyElement.describe(verts, "vertex")
    PlyData([el], text=text).write(path)


def bounds_of(v):
    x = v["x"].astype(np.float32)
    y = v["y"].astype(np.float32)
    z = v["z"].astype(np.float32)
    return (
        float(x.min()), float(x.max()),
        float(y.min()), float(y.max()),
        float(z.min()), float(z.max())
    )


def split_once(v):
    x = v["x"].astype(np.float32)
    z = v["z"].astype(np.float32)

    xmin, xmax = float(x.min()), float(x.max())
    zmin, zmax = float(z.min()), float(z.max())

    if (xmax - xmin) >= (zmax - zmin):
        m = float(np.median(x))
        mask = x < m
        key = "x"
    else:
        m = float(np.median(z))
        mask = z < m
        key = "z"

    a = v[mask]
    b = v[~mask]

    # Fallback gegen leere Seiten
    if len(a) == 0 or len(b) == 0:
        vs = np.sort(v, order=key)
        mid = len(vs) // 2
        a = vs[:mid]
        b = vs[mid:]

    return a, b


def refine_recursive(v, name, depth, text, meta, tile_info):
    n = len(v)
    if n == 0:
        return

    xmin, xmax, ymin, ymax, zmin, zmax = bounds_of(v)
    ex = xmax - xmin
    ez = zmax - zmin

    if n <= TARGET_MAX or depth >= MAX_DEPTH or (ex < MIN_EXTENT and ez < MIN_EXTENT):
        path = os.path.join(OUT_DIR, name + ".ply")
        write_chunk(path, v, text)

        meta.append({
            "name": name,
            "count": int(n),
            "bounds": {"min":[xmin,ymin,zmin], "max":[xmax,ymax,zmax]},
            **tile_info
        })
        return

    a, b = split_once(v)
    refine_recursive(a, name + "_a", depth + 1, text, meta, tile_info)
    refine_recursive(b, name + "_b", depth + 1, text, meta, tile_info)


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    ply = PlyData.read(INPUT_PLY)
    v_all = ply["vertex"].data
    text  = ply.text

    x = v_all["x"].astype(np.float32)
    z = v_all["z"].astype(np.float32)

    xmin, xmax = float(x.min()), float(x.max())
    zmin, zmax = float(z.min()), float(z.max())

    ix0 = int(np.floor(xmin / TILE_SIZE))
    ix1 = int(np.floor(xmax / TILE_SIZE))
    iz0 = int(np.floor(zmin / TILE_SIZE))
    iz1 = int(np.floor(zmax / TILE_SIZE))

    meta = []

    for ix in range(ix0, ix1 + 1):
        x0 = ix * TILE_SIZE - OVERLAP
        x1 = (ix + 1) * TILE_SIZE + OVERLAP
        mx = (x >= x0) & (x <= x1)

        for iz in range(iz0, iz1 + 1):
            z0 = iz * TILE_SIZE - OVERLAP
            z1 = (iz + 1) * TILE_SIZE + OVERLAP
            mz = (z >= z0) & (z <= z1)

            mask = mx & mz
            if not np.any(mask):
                continue

            v_tile = v_all[mask]
            if len(v_tile) < MIN_KEEP:
                continue

            base = f"tile_{ix}_{iz}"

            tile_info = {
                "tile_ix": ix,
                "tile_iz": iz,
                "tile_size": float(TILE_SIZE),
                "overlap": float(OVERLAP)
            }

            if len(v_tile) <= TARGET_MAX:
                write_chunk(os.path.join(OUT_DIR, base + ".ply"), v_tile, text)

                xmin2, xmax2, ymin2, ymax2, zmin2, zmax2 = bounds_of(v_tile)
                meta.append({
                    "name": base,
                    "count": int(len(v_tile)),
                    "bounds": {"min":[xmin2,ymin2,zmin2], "max":[xmax2,ymax2,zmax2]},
                    **tile_info
                })
            else:
                refine_recursive(v_tile, base, 0, text, meta, tile_info)

            print(base, "->", len(v_tile))

    counts = [c["count"] for c in meta]
    print("\nDONE")
    print("Chunks:", len(counts))
    print("min/avg/max:",
          min(counts),
          sum(counts)/len(counts),
          max(counts))

    # CSV für Unity
    with open(os.path.join(OUT_DIR, "tiles_meta.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["name","tile_ix","tile_iz","count"])
        for c in meta:
            w.writerow([c["name"], c["tile_ix"], c["tile_iz"], c["count"]])

    # JSON optional
    with open(os.path.join(OUT_DIR, "tiles_meta.json"), "w", encoding="utf-8") as f:
        json.dump({"tiles": meta}, f, indent=2)


if __name__ == "__main__":
    main()