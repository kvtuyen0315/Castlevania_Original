# accept12_objects.py (CVM2 12, mat 2) - doi chieu 3 nguon vat the Block 2:
#   ClaudeNotes/cvm2/MAP2_PLACEMENT.md (bang nen 1 + khoi 2)  <->  Assets/Data/Config/Level/Block2/Map2Objects.json  <->  ConfigDrop.csv (stage 4-6)
#   + (tuy chon) vet luong that accept12 (dong A: nen/khoi hien trong StageScreen that).
# Chay (tu goc repo): python -P -E Tools/cvm2/accept12_objects.py [telemetry.tsv] -> in + ghi ClaudeNotes/cvm2/_cache/12/objects_xcheck.txt
import csv, json, os, re, sys
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.stdout.reconfigure(encoding='utf-8')
out = []
def P(s):
    out.append(s)
    print(s)

md = open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'MAP2_PLACEMENT.md'), encoding='utf-8').read().splitlines()
cand_md = {}
for l in md:
    m = re.match(r'\|\s*((?:TEA|TEB|HA|HB|HC|MD)_C\d+)\s*\|\s*(\d+)\s*\|\s*(\d+)\s*\|\s*([A-Za-z]+)\s*\|', l)
    if m:
        cand_md[m.group(1)] = (int(m.group(2)), int(m.group(3)), m.group(4))
blk_md = {}
for l in md:
    m = re.match(r'\|\s*`((?:TowerEntranceA|TowerEntranceB|TowerHallA|TowerHallB|TowerHallC|Medusa)_X\d+_\d+)`\s*\|[^|]*\|\s*\**([A-Za-z]+)\**\s*\|', l)
    if m:
        blk_md[m.group(1)] = m.group(2)

js = json.load(open(os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Level', 'Block2', 'Map2Objects.json'), encoding='utf-8'))
cand_js = {}
n_sp = n_edge = n_sec = n_mech = n_boss = 0
for s in js['sections']:
    for c in s.get('candles', []):
        cand_js[c['id']] = (c['x'], c['y'], c['content'])
    n_sp += len(s.get('spawnPoints', []))
    n_edge += len(s.get('edgeSpawners', []))
    n_sec += len(s.get('secrets', []))
    n_mech += len(s.get('mechSpawners', []))
    n_boss += len(s.get('bossArenas', []))

drop = {}
for r in csv.DictReader(open(os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigDrop.csv'), encoding='utf-8')):
    if r['stage'] in ('4', '5', '6'):
        drop[r['objectId']] = r['item']

P('nen: MAP2_PLACEMENT %d dong | Map2Objects.json %d | ConfigDrop (stage 4-6, id nen) %d' % (len(cand_md), len(cand_js), sum(1 for k in drop if '_C' in k)))
bad = 0
for cid in sorted(set(cand_md) | set(cand_js)):
    a, b, c = cand_md.get(cid), cand_js.get(cid), drop.get(cid)
    if a is None or b is None or c is None or a != b or a[2] != c:
        bad += 1
        P('  LECH %s: md %s | json %s | ConfigDrop %s' % (cid, a, b, c))
P('  => nen trung ca 3 nguon (id, x, Y, noi dung): %d/%d' % (len(set(cand_md) | set(cand_js)) - bad, len(set(cand_md) | set(cand_js))))
bb = 0
for bid in sorted(set(blk_md) | set(k for k in drop if '_X' in k)):
    if blk_md.get(bid) != drop.get(bid):
        bb += 1
        P('  LECH khoi %s: md %s | ConfigDrop %s' % (bid, blk_md.get(bid), drop.get(bid)))
P('khoi pha: %d/%d trung (md <-> ConfigDrop)' % (len(blk_md) - bb, len(blk_md)))
P('json: SpawnPoint enemy %d, EdgeSpawner %d, bi mat %d, MechSpawner %d, BossArena %d' % (n_sp, n_edge, n_sec, n_mech, n_boss))
P('ConfigDrop stage 4-6: %d dong (nen %d + khoi %d)' % (len(drop), sum(1 for k in drop if '_C' in k), sum(1 for k in drop if '_X' in k)))
with open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '12', 'objects_xcheck.txt'), 'w', encoding='utf-8') as f:
    f.write('\n'.join(out) + '\n')
