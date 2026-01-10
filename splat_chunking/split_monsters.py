import os
import numpy as np
from plyfile import PlyData, PlyElement

IN_DIR = "room_chunks_final"
OUT_DIR = "room_chunks_final2"

SUB_X = 2
SUB_Z = 2
OVERLAP = 0.10

MONSTERS = [
    "chunk_3_3_sub_3_3.ply",
    "chunk_4_3_sub_0_3.ply",
    "chunk_4_4_sub_0_0.ply",
    "chunk_3_4_sub_3_0.ply",
]

os.makedirs(OUT_DIR, exist_ok=True)

def split_file(fname):
    in_path = os.path.join(IN_DIR, fname)
    if not os.path.exists(in_path):
        print("Missing:", in_path)
        return

    ply = PlyData.read(in_path)
    v = ply["vertex"].data

    x = v["x"].astype(np.float32)
    z = v["z"].astype(np.float32)

    min_x, max_x = float(x.min()), float(x.max())
    min_z, max_z = float(z.min()), float(z.max())

    size_x = max_x - min_x
    size_z = max_z - min_z

    cell_x = size_x / SUB_X
    cell_z = size_z / SUB_Z

    base = os.path.splitext(fname)[0]
    total = 0

    print(f"\n{fname}: {len(v)} pts")

    for iz in range(SUB_Z):
        z0 = min_z + iz * cell_z - OVERLAP
        z1 = min_z + (iz + 1) * cell_z + OVERLAP
        for ix in range(SUB_X):
            x0 = min_x + ix * cell_x - OVERLAP
            x1 = min_x + (ix + 1) * cell_x + OVERLAP

            mask = (x >= x0) & (x < x1) & (z >= z0) & (z < z1)
            chunk_v = v[mask]
            n = len(chunk_v)
            if n == 0:
                continue

            out_name = f"{base}_m_{ix}_{iz}.ply"
            out_path = os.path.join(OUT_DIR, out_name)
            el = PlyElement.describe(chunk_v, "vertex")
            PlyData([el], text=False).write(out_path)

            print(f"  {out_name}: {n}")
            total += n

    print(f"  written (incl overlap dupes): {total}")

for f in MONSTERS:
    split_file(f)

print("\nDone. Wrote:", OUT_DIR)