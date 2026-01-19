import json, csv, os, sys

IN_JSON = "chunks_max_splat/chunks_meta.json"
OUT_CSV = "chunks_max_splat/chunks_meta.csv"

print("[meta_to_csv] cwd:", os.getcwd())
print("[meta_to_csv] IN_JSON:", IN_JSON, "exists:", os.path.exists(IN_JSON))

if not os.path.exists(IN_JSON):
    print("[meta_to_csv] ERROR: cannot find input json:", IN_JSON)
    sys.exit(1)

with open(IN_JSON, "r", encoding="utf-8") as f:
    meta = json.load(f)

chunks = meta.get("chunks", [])
print("[meta_to_csv] chunks:", len(chunks))

os.makedirs(os.path.dirname(OUT_CSV) or ".", exist_ok=True)

with open(OUT_CSV, "w", newline="", encoding="utf-8") as f:
    w = csv.writer(f)
    w.writerow([
        "name",
        "cx","cy","cz",
        "sx","sy","sz",
        "count",
        "tx","tz",
        "gx","gz","sx_idx","sz_idx"
    ])

    for info in chunks:
        name = info.get("name", "")
        cx, cy, cz = info["center"]
        sx, sy, sz = info["size"]

        w.writerow([
            name,
            cx, cy, cz,
            sx, sy, sz,
            info.get("count", 0),
            info.get("tx", 0),
            info.get("tz", 0),
            info.get("gx", 0),
            info.get("gz", 0),
            info.get("sx", 0),
            info.get("sz", 0),
        ])

print("[meta_to_csv] Wrote:", OUT_CSV)