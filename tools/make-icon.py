"""
Thunderstore icon for SmartRepair.

Thunderstore wants the icon at exactly 256x256 PNG, so this takes the 1024x1024 source
art down to that. Kept as a script rather than a one-off resize so the icon can be
regenerated if the master art ever changes.

    python tools/make-icon.py
"""
import os

from PIL import Image

SIZE = 256

HERE = os.path.dirname(os.path.abspath(__file__))
SOURCE = os.path.join(HERE, "icon-source.jpg")
OUT = os.path.join(os.path.dirname(HERE), "icon.png")


def main():
    art = Image.open(SOURCE).convert("RGBA")

    if art.width != art.height:
        # Centre-crop to square first; Thunderstore would otherwise squash it.
        side = min(art.width, art.height)
        left = (art.width - side) // 2
        top = (art.height - side) // 2
        art = art.crop((left, top, left + side, top + side))

    art.resize((SIZE, SIZE), Image.LANCZOS).save(OUT)
    print(f"wrote {OUT} ({SIZE}x{SIZE})")


if __name__ == "__main__":
    main()
