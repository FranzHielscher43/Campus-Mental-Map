import os
import numpy as np
from plyfile import PlyData, PlyElement

INPUT_PLY = "gs_HTWK_Test_Long.ply"
OUT_DIR = "room_chunks_4x4"

GRID_X = 8
GRID_Z = 8

# Overlap verhindert sichtbare Nähte an Chunk-Grenzen
OVERLAP = 0.25  # Meter

ply = PlyData.read(INPUT_PLY)
v = ply["vertex"].data

x = v["x"].astype(np.float32)
z = v["z"].astype(np.float32)

p_low, p_high = 1.0, 99.0
min_x, max_x = np.percentile(x, [p_low, p_high]).astype(float)
min_z, max_z = np.percentile(z, [p_low, p_high]).astype(float)

size_x = max_x - min_x
size_z = max_z - min_z

cell_x = size_x / GRID_X
cell_z = size_z / GRID_Z

os.makedirs(OUT_DIR, exist_ok=True)

print(f"Bounds X: {min_x:.2f}..{max_x:.2f} ({size_x:.2f}m)")
print(f"Bounds Z: {min_z:.2f}..{max_z:.2f} ({size_z:.2f}m)")
print(f"Grid: {GRID_X}x{GRID_Z} | Cell: {cell_x:.2f}m x {cell_z:.2f}m | Overlap: {OVERLAP:.2f}m")

def write_chunk(ix, iz, mask):
    chunk_v = v[mask]
    if len(chunk_v) == 0:
        return 0
    el = PlyElement.describe(chunk_v, "vertex")
    out_path = os.path.join(OUT_DIR, f"chunk_{ix}_{iz}.ply")
    PlyData([el], text=False).write(out_path)
    return len(chunk_v)

total_written = 0
for iz in range(GRID_Z):
    z0 = min_z + iz * cell_z - OVERLAP
    z1 = min_z + (iz + 1) * cell_z + OVERLAP
    for ix in range(GRID_X):
        x0 = min_x + ix * cell_x - OVERLAP
        x1 = min_x + (ix + 1) * cell_x + OVERLAP
        mask = (x >= x0) & (x < x1) & (z >= z0) & (z < z1)
        n = write_chunk(ix, iz, mask)
        total_written += n
        print(f"chunk_{ix}_{iz}: {n}")

print("Done. Total written (inkl. Overlap-Duplikate):", total_written)