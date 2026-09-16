"""
Thunderstore icon for SmartRepair.

Anvil and hammer under a sweep arrow, in the same cream/ink sticker treatment as the
LJIndustries ROUNDS icons, with a forge-amber accent instead of the ROUNDS green.

Writes tools/icon-preview.png. Copy it to icon.png when it looks right:

    python tools/make-icon.py && cp tools/icon-preview.png icon.png
"""
import math
import os

from PIL import Image, ImageDraw, ImageFilter

S = 256          # final size Thunderstore wants
SS = 4           # supersample factor
W = S * SS

CREAM = (245, 239, 225, 255)
INK = (13, 15, 20, 255)
AMBER = (247, 164, 51, 255)
EMBER = (214, 92, 28, 255)
GROUND = (26, 29, 36, 255)
GROUND_EDGE = (14, 16, 21, 255)


def px(v):
    """1000-unit design space -> supersampled pixels."""
    return v * W / 1000.0


def pts(seq):
    return [(px(x), px(y)) for x, y in seq]


def layer():
    return Image.new("RGBA", (W, W), (0, 0, 0, 0))


def grow(src, radius, colour):
    """Dilate the alpha of src into a solid halo, the sticker outline trick."""
    a = src.split()[3].point(lambda v: 255 if v > 110 else 0)
    step = 9
    for _ in range(max(1, round(radius / 4))):
        a = a.filter(ImageFilter.MaxFilter(step))
    a = a.filter(ImageFilter.GaussianBlur(1.2))
    out = Image.new("RGBA", src.size, colour)
    out.putalpha(a)
    return out


def sticker(base, art, cream_px=22, ink_px=10):
    base.alpha_composite(grow(art, cream_px + ink_px, CREAM))
    base.alpha_composite(grow(art, ink_px, INK))
    base.alpha_composite(art)


def background():
    bg = Image.new("RGBA", (W, W), GROUND)
    draw = ImageDraw.Draw(bg)

    # Soft vignette so the cream art has something to sit on.
    steps = 26
    for i in range(steps):
        t = i / steps
        inset = px(500) * t
        shade = tuple(
            round(GROUND_EDGE[c] + (GROUND[c] - GROUND_EDGE[c]) * t) for c in range(3)
        ) + (255,)
        draw.ellipse([inset, inset, W - inset, W - inset], fill=shade)

    return bg


def rounded_poly(draw, points, radius, fill):
    """Polygon with round joins, faked by stroking the outline then filling."""
    draw.polygon(points, fill=fill)
    draw.line(points + [points[0]], fill=fill, width=int(radius * 2), joint="curve")


def anvil():
    art = layer()
    draw = ImageDraw.Draw(art)

    body = [
        (300, 415), (860, 415), (905, 452), (865, 508),
        (655, 508), (612, 566), (617, 646),
        (772, 662), (812, 738), (238, 738), (278, 662),
        (433, 646), (438, 566), (395, 508), (300, 508),
        (96, 487),
    ]
    rounded_poly(draw, pts(body), px(9), CREAM)

    # Face highlight, the struck top of the anvil.
    draw.polygon(pts([(312, 428), (852, 428), (852, 462), (312, 462)]), fill=AMBER)
    return art


def hammer():
    art = layer()
    draw = ImageDraw.Draw(art)

    # Handle, head end at lower left.
    draw.line(pts([(330, 300), (835, 92)]), fill=CREAM, width=int(px(62)), joint="curve")

    # Head: a chunky block squared onto the handle angle.
    head = [(150, 268), (352, 185), (420, 330), (218, 413)]
    rounded_poly(draw, pts(head), px(10), CREAM)
    draw.polygon(pts([(150, 268), (222, 238), (290, 383), (218, 413)]), fill=EMBER)
    return art


def sweep():
    """Circular arrow: the automatic half of the mod."""
    art = layer()
    draw = ImageDraw.Draw(art)

    cx, cy, r = 500.0, 520.0, 388.0
    start, end = 152.0, 358.0     # clockwise from lower left, gap at the bottom

    draw.arc(
        [px(cx - r), px(cy - r), px(cx + r), px(cy + r)],
        start=start, end=end, fill=AMBER, width=int(px(52)),
    )

    # Arrowhead on the arc's own end, out to the right where nothing overlaps it.
    # Anything tucked into the upper left just disappears under the hammer head.
    def on_arc(angle, radius):
        rad = math.radians(angle)
        return cx + radius * math.cos(rad), cy + radius * math.sin(rad)

    draw.polygon(pts([
        on_arc(end - 6, r - 78),
        on_arc(end - 6, r + 78),
        on_arc(end + 24, r),
    ]), fill=AMBER)
    return art


def main():
    canvas = background()

    sticker(canvas, sweep(), cream_px=20, ink_px=9)
    sticker(canvas, anvil(), cream_px=22, ink_px=10)
    sticker(canvas, hammer(), cream_px=22, ink_px=10)

    out = canvas.resize((S, S), Image.LANCZOS).convert("RGBA")
    path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "icon-preview.png")
    out.save(path)
    print(f"wrote {path} ({out.size[0]}x{out.size[1]})")


if __name__ == "__main__":
    main()
