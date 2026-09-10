# -*- coding: utf-8 -*-
r"""Test 包：每次从 bin\EconomyMod.dll 打一个完整可运行的模组包（含 DLL）。
输出两类产物（每次同步刷新，避免拿到旧包）：
  1) release/test/古典经济学/            —— 整个文件夹复制进 Mods\ 覆盖即完成更新
  2) release/test/古典经济学-test.zip     —— 同内容的 zip（顶层含"古典经济学/"文件夹）
包内附 _BUILD.txt（构建时间 + DLL 大小 + SHA256 前 12 位），安装后可与控制台输出核对。
用法：python tools/pack_test.py  （先跑 build_local.ps1 之外的编译）"""
import hashlib
import os
import shutil
import time
import zipfile

ROOT = r'E:\code\new\ClassicalEconomics'
STAGE = os.path.join(ROOT, 'release', 'test', '古典经济学')
ZIP_PATH = os.path.join(ROOT, 'release', 'test', '古典经济学-test.zip')

if os.path.exists(STAGE):
    shutil.rmtree(STAGE)
os.makedirs(STAGE)

# 运行所需的全部文件：DLL + 配置/事件/图标 + 语言/图标/游戏资源 + 六语 Steam 描述
shutil.copy2(os.path.join(ROOT, 'bin', 'EconomyMod.dll'), os.path.join(STAGE, 'EconomyMod.dll'))
for item in ('mod.json', 'default_config.json', 'events.json', 'icon.png'):
    shutil.copy2(os.path.join(ROOT, item), os.path.join(STAGE, item))
for d in ('Locales', 'Icons', 'GameResources'):
    shutil.copytree(os.path.join(ROOT, d), os.path.join(STAGE, d))
for f in os.listdir(ROOT):
    if f.startswith('STEAM_WORKSHOP_DESC_') and f.endswith('.txt'):
        shutil.copy2(os.path.join(ROOT, f), os.path.join(STAGE, f))

dll = os.path.join(STAGE, 'EconomyMod.dll')
dll_size = os.path.getsize(dll)
with open(dll, 'rb') as fh:
    dll_sha = hashlib.sha256(fh.read()).hexdigest()[:12]
build_stamp = time.strftime('%Y-%m-%d %H:%M:%S')

# 包内校验信息：安装后与此处输出、以及 zip 内 _BUILD.txt 三方核对，防止再拿到旧包
with open(os.path.join(STAGE, '_BUILD.txt'), 'w', encoding='utf-8') as fh:
    fh.write('build: {0}\nEconomyMod.dll: {1} bytes\nsha256[:12]: {2}\n'.format(
        build_stamp, dll_size, dll_sha))

# 同步刷新 zip（顶层含"古典经济学/"文件夹，解压后直接拖进 Mods）
if os.path.exists(ZIP_PATH):
    os.remove(ZIP_PATH)
parent = os.path.dirname(STAGE)
with zipfile.ZipFile(ZIP_PATH, 'w', zipfile.ZIP_DEFLATED) as zf:
    for base, _dirs, files in os.walk(STAGE):
        for name in files:
            full = os.path.join(base, name)
            zf.write(full, os.path.relpath(full, parent))

print('TEST PACK OK:', STAGE)
print('  EconomyMod.dll size:', dll_size, 'bytes')
print('  sha256[:12]:', dll_sha)
print('  build:', build_stamp)
print('  entries:', sum(len(fs) for _, _, fs in os.walk(STAGE)))
print('ZIP OK:', ZIP_PATH, '({0} bytes)'.format(os.path.getsize(ZIP_PATH)))
