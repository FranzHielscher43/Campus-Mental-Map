import os
import json
import csv
import numpy as np
from plyfile import PlyData, PlyElement

INPUT_PLY = "3DGS.ply"
OUT_DIR   = "chunks_final"

GRID_X = 8
GRID_Z = 8

THRESH_SPLIT = 20000   
TARGET_MAX   = 16000   
MAX_DEPTH    = 8
MIN_EXTENT   = 1e-4

def write_chunk(path, vertex_array, text_format):
    el = PlyElement.describe(vertex_array, "vertex")
    PlyData([el], text=text_format).write(path)


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
    ex = xmax - xmin
    ez = zmax - zmin

    if ex >= ez:
        m = float(np.median(x))
        mask = x < m
        key = "x"
    else:
        m = float(np.median(z))
        mask = z < m
        key = "z"

    a = v[mask]
    b = v[~mask]

    if len(a) == 0 or len(b) == 0:
        vs = np.sort(v, order=key)
        mid = len(vs) // 2
        a = vs[:mid]
        b = vs[mid:]

    return a, b

def refine_recursive(v, name, depth, text_format, meta):
    n = len(v)
    if n == 0:
        return

    xmin, xmax, ymin, ymax, zmin, zmax = bounds_of(v)
    ex = xmax - xmin
    ez = zmax - zmin

    if (
        n <= TARGET_MAX
        or depth >= MAX_DEPTH
        or (ex < MIN_EXTENT and ez < MIN_EXTENT)
    ):
        out_path = os.path.join(OUT_DIR, name + ".ply")
        write_chunk(out_path, v, text_format)

        meta.append({
            "name": name,
            "count": int(n),
            "bounds": {
                "min": [xmin, ymin, zmin],
                "max": [xmax, ymax, zmax]
            }
        })
        return

    a, b = split_once(v)
    refine_recursive(a, name + "_a", depth + 1, text_format, meta)
    refine_recursive(b, name + "_b", depth + 1, text_format, meta)

def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    ply = PlyData.read(INPUT_PLY)
    v_all = ply["vertex"].data
    text_format = ply.text

    x = v_all["x"].astype(np.float32)
    z = v_all["z"].astype(np.float32)

    xmin, xmax = float(x.min()), float(x.max())
    zmin, zmax = float(z.min()), float(z.max())

    sx = (xmax - xmin) / GRID_X
    sz = (zmax - zmin) / GRID_Z
    sx = max(sx, 1e-6)
    sz = max(sz, 1e-6)

    meta = []

    for ix in range(GRID_X):
        x0 = xmin + ix * sx
        x1 = xmin + (ix + 1) * sx
        mx = (x >= x0) & (x < x1) if ix < GRID_X - 1 else (x >= x0) & (x <= x1)

        for iz in range(GRID_Z):
            z0 = zmin + iz * sz
            z1 = zmin + (iz + 1) * sz
            mz = (z >= z0) & (z < z1) if iz < GRID_Z - 1 else (z >= z0) & (z <= z1)

            mask = mx & mz
            if not np.any(mask):
                continue

            v_tile = v_all[mask]
            name = f"chunk_{ix}_{iz}"

            if len(v_tile) <= THRESH_SPLIT:
                xmin2, xmax2, ymin2, ymax2, zmin2, zmax2 = bounds_of(v_tile)
                write_chunk(os.path.join(OUT_DIR, name + ".ply"), v_tile, text_format)
                meta.append({
                    "name": name,
                    "count": int(len(v_tile)),
                    "bounds": {
                        "min": [xmin2, ymin2, zmin2],
                        "max": [xmax2, ymax2, zmax2]
                    }
                })
            else:
                refine_recursive(v_tile, name, 0, text_format, meta)

            print(name, "->", len(v_tile))

    total_in = len(v_all)
    total_out = sum([c["count"] for c in meta])
    print("TOTAL IN :", total_in)
    print("TOTAL OUT:", total_out)
    print("DIFF     :", total_in - total_out)

    json_path = os.path.join(OUT_DIR, "chunks_meta.json")
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump({"chunks": meta}, f, indent=2)

    csv_path = os.path.join(OUT_DIR, "chunks_meta.csv")
    with open(csv_path, "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["name","minx","miny","minz","maxx","maxy","maxz","count"])
        for c in meta:
            bmin = c["bounds"]["min"]
            bmax = c["bounds"]["max"]
            w.writerow([
                c["name"],
                bmin[0], bmin[1], bmin[2],
                bmax[0], bmax[1], bmax[2],
                c["count"]
            ])

    counts = [c["count"] for c in meta]
    print("\nDONE")
    print("Chunks:", len(counts))
    print("min/avg/max:",
          min(counts),
          sum(counts)/len(counts),
          max(counts))
    print("→", OUT_DIR)


if __name__ == "__main__":
    main()