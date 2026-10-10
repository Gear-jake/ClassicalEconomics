from PIL import Image, ImageDraw
S = 128
img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d = ImageDraw.Draw(img)

def shield(inset, fill):
    top = 10 + inset
    left = 22 + inset
    right = S - 22 - inset
    bottom = S - 12 - inset
    mid = top + int((bottom - top) * 0.52)
    pts = [
        (left, top + 6), (left + 8, top), (right - 8, top), (right, top + 6),
        (right, mid), (S // 2, bottom), (left, mid),
    ]
    d.polygon(pts, fill=fill)

shield(-4, (28, 40, 62, 255))
shield(0, (46, 74, 116, 255))
shield(10, (74, 124, 186, 255))
shield(20, (104, 158, 220, 255))
d.polygon([(S//2-5, 26), (S//2+5, 26), (S//2+5, 74), (S//2, 92), (S//2-5, 74)], fill=(160, 205, 240, 255))
import os
out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Icons", "shield.png")
img.save(out)
print("SHIELD_OK", img.size, out)