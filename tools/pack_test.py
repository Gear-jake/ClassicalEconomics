# -*- coding: utf-8 -*-
r"""Test 包：每次从 bin\EconomyMod.dll 打一个完整可运行的模组包（含 DLL）。
输出 release/test/古典经济学/ —— 与你的游戏模组目录同名，整个文件夹复制进
Mods\ 覆盖即完成更新。
用法：python tools/pack_test.py  （先跑 build_local.ps1 + deploy_local.ps1 之外的编译）"""
import os, shutil

ROOT = r'E:\code\new\ClassicalEconomics'
STAGE = os.path.join(ROOT, 'release', 'test', '古典经济学')

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
print('TEST PACK OK:', STAGE)
print('  EconomyMod.dll size:', os.path.getsize(dll), 'bytes')
print('  entries:', sum(len(fs) for _, _, fs in os.walk(STAGE)))
