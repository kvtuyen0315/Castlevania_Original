# Bien doi tim / thanh ENEMY tu CSV HUD 2 video boss (cvm2-04): tim -1 = luc bam nem, ENEMY -1 = trung. Chay tu goc repo: python -P -E Tools/cvm2/holy4_hud.py
import csv,sys
for name,p in [('boss1','ClaudeNotes/cvgp/_cache/video/work/boss_hud.csv'),('boss2','ClaudeNotes/cvm2/ref/data/boss2_hud.csv')]:
    rows=list(csv.DictReader(open(p)))
    ph=pe=None
    out=[]
    for r in rows:
        if r['hud_ok']!='1': continue
        h=r['hearts']; e=r['ebar']
        if ph is not None and h!=ph: out.append('%s f%s t%s hearts %s->%s'%(name,r['frame'],r['t'],ph,h))
        if pe is not None and e!=pe: out.append('%s f%s t%s ebar %s->%s'%(name,r['frame'],r['t'],pe,e))
        ph,pe=h,e
    print('\n'.join(out))
