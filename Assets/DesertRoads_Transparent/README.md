# Desert Roads Transparent - Unity 6

Folder gata de pus in Assets. Drumuri cu pietris, fara fundal de nisip (PNG transparent).

## Instalare (fara Import Package)
1. Dezarhiveaza arhiva ZIP.
2. Copiaza **folderul DesertRoads_Transparent** in folderul **Assets** al proiectului Unity.
3. Deschide proiectul in Unity si asteapta importul/compilarea.
4. Se vor genera automat folderele **Materials**, **Meshes** si **Prefabs**.
5. Gasesti piesele gata de plasat in **Assets/DesertRoads_Transparent/Prefabs**.
   Trage-le direct in scena.

Daca nu apar prefabs, foloseste in meniul Unity:
**Tools > Desert Roads Transparent > Create/Refresh Materials & Prefabs**.

## Continut
- 6 drumuri: drept, curba 45 grade, curba 90 grade, curba S, intersectie T, intersectie cruce
- 6 texturi PNG cu fundal transparent
- Script de editor care creeaza 6 materiale si 24 prefab-uri orientate la 0/90/180/270 grade
- Tile-uri de 10 x 10 unitati Unity, pivot in coltul (0,0), cu Y=0 la transform

Nota: drumurile sunt suprafete plate ce stau usor deasupra terenului, iar curba de 45 grade nu se imbina neaparat perfect cu toate piesele. Suprafetele nu urmaresc relief montan.
