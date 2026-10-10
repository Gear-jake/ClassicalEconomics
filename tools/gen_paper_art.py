
# -*- coding: utf-8 -*-
"""生成「报纸风」UI 素材（9-slice, border 16）：
   paper_9slice.png        羊皮纸/报纸底（暖米色 + 纤维噪点 + 做旧边缘）
   paper_button_9slice.png 深色石板按钮（参考图底部那种）
供 UIArt.Get() 以 border=16 的 9-slice 方式加载。
"""
import random, os
from PIL import Image, ImageDraw, ImageFilter

SIZE = 128            # 源图尺寸（9-slice 用，远大于 2*border）
BORDER = 16
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'GameResources', 'ui', 'panels')

random.seed(20261007)

def noise(img, amount=10):
    px = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            n = random.randint(-amount, amount)
            px[x, y] = (max(0, min(255, r + n)), max(0, min(255, g + n)), max(0, min(255, b + n)), a)
    return img

# ---------- 羊皮纸底 ----------
paper = Image.new('RGBA', (SIZE, SIZE), (232, 220, 192, 255))
d = ImageDraw.Draw(paper)

# 纸纤维：随机浅/深细线
for _ in range(900):
    x = random.randint(0, SIZE - 1); y = random.randint(0, SIZE - 1)
    ln = random.randint(2, 6)
    shade = random.choice([(224, 210, 180), (240, 230, 205), (218, 204, 174)])
    d.line([(x, y), (x + ln, y + random.choice([0, 0, 1, -1]))], fill=shade + (255,))

# 污渍（淡淡几处）
stain = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0))
sd = ImageDraw.Draw(stain)
for _ in range(5):
    cx = random.randint(20, SIZE - 20); cy = random.randint(20, SIZE - 20)
    r = random.randint(10, 26)
    sd.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(206, 188, 152, 60))
stain = stain.filter(ImageFilter.GaussianBlur(6))
paper = Image.alpha_composite(paper, stain)

# 做旧边框：外圈深棕，内侧一道浅色高光（形成"纸边"）
d = ImageDraw.Draw(paper)
for i in range(3):
    a = 150 - i * 45
    d.rectangle([i, i, SIZE - 1 - i, SIZE - 1 - i], outline=(122, 100, 70, a))
d.rectangle([4, 4, SIZE - 5, SIZE - 5], outline=(250, 244, 226, 90))
# 内阴影（让纸面略有凹陷感）
inner = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0))
idr = ImageDraw.Draw(inner)
for i in range(10):
    idr.rectangle([6 + i, 6 + i, SIZE - 7 - i, SIZE - 7 - i], outline=(120, 100, 72, max(0, 40 - i * 4)))
inner = inner.filter(ImageFilter.GaussianBlur(2))
paper = Image.alpha_composite(paper, inner)
paper = noise(paper, 6)
paper.save(os.path.join(OUT, 'paper_9slice.png'))

# ---------- 深色石板按钮 ----------
btn = Image.new('RGBA', (SIZE, SIZE), (58, 58, 54, 255))
d = ImageDraw.Draw(btn)
# 顶部高光 / 底部阴影（立体感）
for i in range(BORDER):
    a = int(70 * (1 - i / BORDER))
    d.line([(i, i), (SIZE - 1 - i, i)], fill=(96, 96, 88, a))
    d.line([(i, SIZE - 1 - i), (SIZE - 1 - i, SIZE - 1 - i)], fill=(30, 30, 28, a))
# 外描边
d.rectangle([0, 0, SIZE - 1, SIZE - 1], outline=(24, 24, 22, 220))
d.rectangle([1, 1, SIZE - 2, SIZE - 2], outline=(130, 130, 118, 90))
# 轻微石纹
for _ in range(400):
    x = random.randint(0, SIZE - 1); y = random.randint(0, SIZE - 1)
    v = random.randint(-10, 10)
    r, g, b, a = btn.getpixel((x, y))
    btn.putpixel((x, y), (max(0, min(255, r + v)), max(0, min(255, g + v)), max(0, min(255, b + v)), a))
btn.save(os.path.join(OUT, 'paper_button_9slice.png'))

print('paper_9slice.png', os.path.getsize(os.path.join(OUT, 'paper_9slice.png')), 'bytes')
print('paper_button_9slice.png', os.path.getsize(os.path.join(OUT, 'paper_button_9slice.png')), 'bytes')
