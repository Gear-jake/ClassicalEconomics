# -*- coding: utf-8 -*-
r"""发布包：从源码 + 资源生成 GitHub Release 用的 zip（源码包，不含 DLL）。
结构（与 2.1.8 发布包同构，229 条目级别）：
  根        mod.json / default_config.json / events.json / icon.png / STEAM_WORKSHOP_DESC_*.txt ×6
  资源      GameResources/ Icons/ Locales/
  Source/   EconomyModMain.cs + Core/ Services/ UI/ Models/ tools/ docs/（全部源码与文档）
输出 release/ClassicalEconomics-<version>.zip；版本号默认读 mod.json。
用法：python tools/pack_release.py [version]"""
import json
import os
import sys
import zipfile

ROOT = r'E:\code\new\ClassicalEconomics'
RELEASE_DIR = os.path.join(ROOT, 'release')

version = sys.argv[1] if len(sys.argv) > 1 else None
if not version:
    with open(os.path.join(ROOT, 'mod.json'), encoding='utf-8') as fh:
        version = json.load(fh)['version']
ZIP_PATH = os.path.join(RELEASE_DIR, 'ClassicalEconomics-{0}.zip'.format(version))

SKIP_DIRS = {'__pycache__', 'obj', 'bin', '.git'}


def iter_files(base):
    for cur, dirs, files in os.walk(base):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for name in files:
            if name.endswith('.pyc'):
                continue
            full = os.path.join(cur, name)
            yield full, os.path.relpath(full, base).replace('\\', '/')


entries = []

# 根文件
for name in ('mod.json', 'default_config.json', 'events.json', 'icon.png'):
    entries.append((os.path.join(ROOT, name), name))
for name in sorted(os.listdir(ROOT)):
    if name.startswith('STEAM_WORKSHOP_DESC_') and name.endswith('.txt'):
        entries.append((os.path.join(ROOT, name), name))

# 运行资源（arcname 必须带顶层目录名：GameResources/ Icons/ Locales/，游戏按此路径加载）
for folder in ('GameResources', 'Icons', 'Locales'):
    for full, rel in iter_files(os.path.join(ROOT, folder)):
        entries.append((full, '{0}/{1}'.format(folder, rel)))

# 源码与文档（Source/ 前缀，保持 2.1.8 发布包结构）
entries.append((os.path.join(ROOT, 'EconomyModMain.cs'), 'Source/EconomyModMain.cs'))
for folder in ('Core', 'Services', 'UI', 'Models', 'tools', 'docs'):
    for full, rel in iter_files(os.path.join(ROOT, folder)):
        entries.append((full, 'Source/{0}/{1}'.format(folder, rel)))

if os.path.exists(ZIP_PATH):
    os.remove(ZIP_PATH)
with zipfile.ZipFile(ZIP_PATH, 'w', zipfile.ZIP_DEFLATED) as zf:
    for full, arc in entries:
        zf.write(full, arc)

names = zipfile.ZipFile(ZIP_PATH).namelist()
bad = [n for n in names if n.lower().endswith(('.dll', '.exe', '.pdb'))]
print('RELEASE PACK OK:', ZIP_PATH)
print('  version:', version)
print('  entries:', len(names))
print('  size:', os.path.getsize(ZIP_PATH), 'bytes')
print('  dll/exe present:', bad if bad else 'none (source pack)')
