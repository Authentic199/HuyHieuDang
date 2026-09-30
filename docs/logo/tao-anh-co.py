"""Sinh các bản dẫn xuất của ảnh lá cờ từ đúng một tệp nguồn.

Nguồn: docs/logo/co-dang-co-to-quoc-goc.png — ảnh chủ dự án gửi, nền trắng,
512x324, không có kênh trong suốt. Không vẽ lại, không phóng to quá 512 px ngang.

Chạy lại khi có bản nguồn nét hơn:

    python docs/logo/tao-anh-co.py

Sinh ra:
  FE/src/assets/co-dang-co-to-quoc.png  — nền trong suốt, đã cắt sát mép cờ
  FE/public/co-dang-co-to-quoc.png      — biểu tượng tab, đệm thành hình vuông

Cách tách nền: ảnh chỉ có ba màu phẳng (trắng nền, đỏ cờ, vàng sao và búa
liềm). Với mỗi điểm ảnh, chiếu màu quan sát được lên đoạn thẳng nối màu trắng
với từng màu cờ để suy ra độ đục. Điểm nằm sát đoạn thẳng đó là điểm mép đã khử
răng cưa: trả về đúng màu cờ kèm độ đục vừa tính, nên không còn chút trắng nào
lẫn trong màu — đó là thứ gây viền trắng lem khi đặt lên nền đỏ. Điểm lệch xa cả
hai đoạn thẳng là điểm pha đỏ với vàng nằm trong lòng cờ: giữ nguyên màu, đục
hoàn toàn.
"""

from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'docs' / 'logo' / 'co-dang-co-to-quoc-goc.png'
ASSET = ROOT / 'FE' / 'src' / 'assets' / 'co-dang-co-to-quoc.png'
FAVICON = ROOT / 'FE' / 'public' / 'co-dang-co-to-quoc.png'

WHITE = (255, 255, 255)
# Hai màu phẳng của ảnh nguồn, đếm trực tiếp từ tệp.
FLAG_COLOURS = ((218, 37, 28), (251, 236, 29))
# Lệch quá ngần này so với mọi đoạn trắng->màu cờ thì coi là điểm pha trong lòng cờ.
BLEND_TOLERANCE = 30.0
# Nền trắng của ảnh nguồn lấm tấm nhiễu gần trắng trải khắp khung. Để nguyên thì
# nhiễu thành một lớp đục rất mỏng, phủ mờ cả khung khi đặt lên nền đỏ của header.
# Kéo giãn độ đục: dưới ngưỡng dưới coi như nền, trên ngưỡng trên coi như đặc.
ALPHA_FLOOR = 0.12
ALPHA_CEILING = 0.88


def cut_white_background(image: Image.Image) -> Image.Image:
    """Trả về bản RGBA đã bỏ nền trắng, giữ mép khử răng cưa."""
    source = image.convert('RGB')
    pixels = list(source.getdata())
    out: list[tuple[int, int, int, int]] = []

    for observed in pixels:
        best = None
        for colour in FLAG_COLOURS:
            direction = tuple(c - w for c, w in zip(colour, WHITE))
            length_squared = sum(d * d for d in direction)
            offset = tuple(o - w for o, w in zip(observed, WHITE))
            alpha = sum(o * d for o, d in zip(offset, direction)) / length_squared
            alpha = min(1.0, max(0.0, alpha))
            rebuilt = tuple(w + alpha * d for w, d in zip(WHITE, direction))
            residual = sum((o - r) ** 2 for o, r in zip(observed, rebuilt)) ** 0.5
            if best is None or residual < best[0]:
                best = (residual, colour, alpha)

        residual, colour, alpha = best
        alpha = (alpha - ALPHA_FLOOR) / (ALPHA_CEILING - ALPHA_FLOOR)
        alpha = min(1.0, max(0.0, alpha))
        if residual > BLEND_TOLERANCE:
            # Điểm pha đỏ với vàng trong lòng cờ — giữ nguyên màu, đục hoàn toàn.
            out.append((*observed, 255))
        else:
            out.append((*colour, round(alpha * 255)))

    cut = Image.new('RGBA', source.size)
    cut.putdata(out)
    return cut


def main() -> None:
    source = Image.open(SOURCE)
    if source.size[0] > 512:
        raise SystemExit(f'Ảnh nguồn rộng {source.size[0]} px, vượt trần 512 px.')

    cut = cut_white_background(source)

    # Cắt sát mép cờ: ảnh nguồn có lề trắng, để lại sẽ thành khoảng trống chết
    # quanh logo ở mọi nơi dùng nó.
    trimmed = cut.crop(cut.getbbox())
    ASSET.parent.mkdir(parents=True, exist_ok=True)
    trimmed.save(ASSET)

    # Favicon phải vuông. Cạnh lấy đúng bề ngang cờ nên không hề phóng to.
    side = trimmed.width
    icon = Image.new('RGBA', (side, side), (0, 0, 0, 0))
    icon.paste(trimmed, (0, (side - trimmed.height) // 2))
    FAVICON.parent.mkdir(parents=True, exist_ok=True)
    icon.save(FAVICON)

    print(f'{SOURCE.name}: {source.size[0]}x{source.size[1]}')
    print(f'{ASSET.relative_to(ROOT)}: {trimmed.width}x{trimmed.height}')
    print(f'{FAVICON.relative_to(ROOT)}: {side}x{side}')


if __name__ == '__main__':
    main()
