#!/usr/bin/env python3
"""Generates the adaptive launcher icon from the source artwork.

    python3 android/tools/make_launcher_icon.py

Run it after replacing app/src/main/ic_launcher-source.png. The PNGs it writes are
derived files - regenerate them rather than editing them, the same way 004_seed_people.sql
is generated from the AD export rather than hand-written.

What it does, and why each step is needed:

  * Removes the artwork's own navy background. An adaptive icon is two layers, and the
    launcher masks them into a circle, a squircle or a rounded square depending on the
    device. Artwork with its own background baked in gets that background masked into a
    shape inside the real one - a navy tile floating on a navy field with a visible seam.
    So the background becomes the background LAYER (@color/di_navy, the brand navy the
    rest of the app uses) and only the glyphs go in the foreground.

    The removal is two ordered floods from the border, and the order is the whole trick.
    White first: it clears the margin and the area outside the rounded corners, and it
    cannot reach the white person or the white card because navy separates them. Navy
    second: the border is transparent by then, so it clears the navy field and the
    anti-aliased ring, stopping at anything that is a glyph colour. Doing it the other way
    round - or in one pass allowing both - leaks through the navy into the white glyphs
    and erases most of the artwork. That is not hypothetical; it is what the first
    attempt did.

  * Scales the glyphs to 46% of the 108dp canvas. Adaptive icons only guarantee the
    central 66dp circle is visible. At the artwork's natural size the gold monogram, the
    card's right edge and the green tick all fall outside it and get clipped on any
    launcher using a circular mask. 46% is the largest that keeps them in.

minSdk is 26 and adaptive icons landed in 26, so every device uses this. No legacy
bitmaps are produced because nothing would ever load them.
"""

from collections import deque
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "app/src/main/ic_launcher-source.png"
RES = ROOT / "app/src/main/res"

# 108dp foreground, at each density bucket.
DENSITIES = {"mdpi": 108, "hdpi": 162, "xhdpi": 216, "xxhdpi": 324, "xxxhdpi": 432}

# Fraction of the 108dp canvas the glyphs occupy. See the docstring.
SCALE = 0.46


def is_white(p):
    return p[0] > 205 and p[1] > 205 and p[2] > 205


def is_gold(p):
    return p[0] > 150 and p[1] > 110 and p[2] < 120 and p[0] - p[2] > 60


def is_green(p):
    return p[1] > 110 and p[0] < 140 and p[2] < 140 and p[1] - p[0] > 40


def is_glyph(p):
    return is_white(p) or is_gold(p) or is_green(p)


def flood(image, allow):
    """Every pixel reachable from the border through pixels `allow` accepts."""
    width, height = image.size
    pixels = image.load()
    seen = bytearray(width * height)

    queue = deque((x, y) for x in range(width) for y in (0, height - 1))
    queue.extend((x, y) for y in range(height) for x in (0, width - 1))

    found = []
    while queue:
        x, y = queue.popleft()
        if not (0 <= x < width and 0 <= y < height):
            continue
        index = y * width + x
        if seen[index] or not allow(pixels[x, y]):
            continue
        seen[index] = 1
        found.append((x, y))
        queue.extend(((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)))
    return found


def main():
    if not SOURCE.exists():
        raise SystemExit(f"No artwork at {SOURCE}")

    image = Image.open(SOURCE).convert("RGBA")
    pixels = image.load()

    white = flood(image, lambda p: p[3] != 0 and is_white(p))
    for x, y in white:
        pixels[x, y] = (0, 0, 0, 0)

    navy = flood(image, lambda p: p[3] == 0 or not is_glyph(p))
    for x, y in navy:
        pixels[x, y] = (0, 0, 0, 0)

    glyphs = image.crop(image.getbbox())
    print(f"glyphs: {glyphs.size[0]}x{glyphs.size[1]} from {image.size[0]}x{image.size[1]}")

    for density, canvas in DENSITIES.items():
        side = int(canvas * SCALE)
        fitted = glyphs.copy()
        fitted.thumbnail((side, side), Image.LANCZOS)

        layer = Image.new("RGBA", (canvas, canvas), (0, 0, 0, 0))
        layer.alpha_composite(
            fitted,
            ((canvas - fitted.size[0]) // 2, (canvas - fitted.size[1]) // 2),
        )

        target = RES / f"mipmap-{density}"
        target.mkdir(parents=True, exist_ok=True)
        out = target / "ic_launcher_foreground.png"
        layer.save(out, optimize=True)
        print(f"  {out.relative_to(ROOT)}  {canvas}x{canvas}")


if __name__ == "__main__":
    main()
