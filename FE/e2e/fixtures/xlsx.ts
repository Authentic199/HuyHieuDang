import { inflateRawSync } from 'node:zlib';

/**
 * Trình đọc `.xlsx` tối giản, viết bằng thư viện chuẩn của Node.
 *
 * Ca E5-02 → E5-07 đòi MỞ RA ĐỌC tệp tải về chứ không chỉ kiểm tên, nên bộ
 * kiểm thử phải tự đọc được Excel. Dự án Frontend không có gói đọc Excel nào,
 * và T28 không được phép thêm phụ thuộc vào `FE/package.json` (ngoài phạm vi
 * tệp của task), nên phần này tự giải nén ZIP rồi đọc XML của SpreadsheetML.
 *
 * Chỉ đọc những gì các ca kiểm thử cần: danh sách sheet, giá trị từng ô dưới
 * dạng chữ, theo đúng lưới dòng/cột. Không xử lý công thức, định dạng số hay
 * biểu đồ — tệp xuất của hệ thống không có những thứ đó.
 */

/** Giải nén một tệp ZIP trong bộ nhớ thành bảng tên tệp → nội dung. */
function unzip(buffer: Buffer): Map<string, Buffer> {
  const entries = new Map<string, Buffer>();

  // Bản ghi cuối thư mục trung tâm nằm ở cuối tệp, tối đa 64 KB phần chú thích.
  let eocd = -1;
  for (
    let offset = buffer.length - 22;
    offset >= Math.max(0, buffer.length - 65_557);
    offset -= 1
  ) {
    if (buffer.readUInt32LE(offset) === 0x06054b50) {
      eocd = offset;
      break;
    }
  }
  if (eocd < 0) throw new Error('Tệp không phải ZIP hợp lệ: không tìm thấy bản ghi cuối thư mục');

  const count = buffer.readUInt16LE(eocd + 10);
  let pointer = buffer.readUInt32LE(eocd + 16);

  for (let index = 0; index < count; index += 1) {
    if (buffer.readUInt32LE(pointer) !== 0x02014b50) {
      throw new Error(`Mục thứ ${index} của thư mục trung tâm sai chữ ký`);
    }
    const method = buffer.readUInt16LE(pointer + 10);
    const compressedSize = buffer.readUInt32LE(pointer + 20);
    const nameLength = buffer.readUInt16LE(pointer + 28);
    const extraLength = buffer.readUInt16LE(pointer + 30);
    const commentLength = buffer.readUInt16LE(pointer + 32);
    const localOffset = buffer.readUInt32LE(pointer + 42);
    const name = buffer.toString('utf8', pointer + 46, pointer + 46 + nameLength);

    // Phần đầu cục bộ có độ dài tên và phần phụ RIÊNG, không dùng lại của trung tâm.
    const localNameLength = buffer.readUInt16LE(localOffset + 26);
    const localExtraLength = buffer.readUInt16LE(localOffset + 28);
    const dataStart = localOffset + 30 + localNameLength + localExtraLength;
    const raw = buffer.subarray(dataStart, dataStart + compressedSize);

    entries.set(name, method === 0 ? Buffer.from(raw) : inflateRawSync(raw));
    pointer += 46 + nameLength + extraLength + commentLength;
  }

  return entries;
}

const ENTITIES: Record<string, string> = {
  amp: '&',
  lt: '<',
  gt: '>',
  quot: '"',
  apos: "'",
};

function decodeXml(text: string): string {
  return text.replace(/&(#x?[0-9a-fA-F]+|[a-z]+);/g, (whole, token: string) => {
    if (token.startsWith('#x') || token.startsWith('#X')) {
      return String.fromCodePoint(Number.parseInt(token.slice(2), 16));
    }
    if (token.startsWith('#')) return String.fromCodePoint(Number.parseInt(token.slice(1), 10));
    return ENTITIES[token] ?? whole;
  });
}

/** `B7` → 1 (chỉ số cột bắt đầu từ 0). */
function columnIndex(reference: string): number {
  const letters = reference.replace(/[0-9]/g, '');
  let value = 0;
  for (const letter of letters) value = value * 26 + (letter.charCodeAt(0) - 64);
  return value - 1;
}

/**
 * Hai trình ghi Excel gặp trong dự án viết thẻ khác nhau: `openpyxl` (sinh bộ
 * dữ liệu biên) viết `<row>`, còn MiniExcel (Backend xuất) viết `<x:row>` và
 * đôi chỗ còn chèn khoảng trắng kiểu `t ="str"`. Mọi mẫu dưới đây vì vậy đều
 * cho phép tiền tố không gian tên và khoảng trắng quanh dấu bằng.
 */
const TEXT_TAG = /<(?:\w+:)?t\b[^>]*>([\s\S]*?)<\/(?:\w+:)?t>/g;
const VALUE_TAG = /<(?:\w+:)?v\b[^>]*>([\s\S]*?)<\/(?:\w+:)?v>/;
const ROW_TAG = /<(?:\w+:)?row\b[^>]*?\br\s*=\s*"(\d+)"[^>]*>([\s\S]*?)<\/(?:\w+:)?row>/g;
const CELL_TAG = /<(?:\w+:)?c\b([^>]*?)(?:\/>|>([\s\S]*?)<\/(?:\w+:)?c>)/g;
const SHEET_NAME_TAG = /<(?:\w+:)?sheet\b[^>]*?\bname\s*=\s*"([^"]*)"/g;
const SI_TAG = /<(?:\w+:)?si\b[^>]*>([\s\S]*?)<\/(?:\w+:)?si>/g;

function joinText(fragment: string): string {
  return Array.from(fragment.matchAll(TEXT_TAG))
    .map((part) => decodeXml(part[1]))
    .join('');
}

/** Chuỗi dùng chung của workbook. */
function readSharedStrings(files: Map<string, Buffer>): string[] {
  const xml = files.get('xl/sharedStrings.xml')?.toString('utf8');
  if (!xml) return [];
  // Chuỗi có định dạng bị chẻ thành nhiều <t>, phải nối lại theo đúng thứ tự.
  return Array.from(xml.matchAll(SI_TAG)).map((item) => joinText(item[1]));
}

export interface Sheet {
  name: string;
  /** Lưới ô đã quy về chữ; ô trống là chuỗi rỗng. */
  rows: string[][];
}

export interface Workbook {
  sheets: Sheet[];
  /** Sheet đầu tiên — mọi tệp xuất của hệ thống chỉ có đúng một sheet. */
  first: Sheet;
}

/** Đọc một tệp `.xlsx` trong bộ nhớ. */
export function readWorkbook(buffer: Buffer): Workbook {
  const files = unzip(buffer);
  const shared = readSharedStrings(files);

  const workbookXml = files.get('xl/workbook.xml')?.toString('utf8') ?? '';
  const sheetNames = Array.from(workbookXml.matchAll(SHEET_NAME_TAG)).map((item) =>
    decodeXml(item[1]),
  );

  const sheetFiles = Array.from(files.keys())
    .filter((name) => /^xl\/worksheets\/sheet\d+\.xml$/.test(name))
    .sort((left, right) => left.localeCompare(right, 'en'));

  const sheets = sheetFiles.map((file, index) => ({
    name: sheetNames[index] ?? `Sheet${index + 1}`,
    rows: readSheet(files.get(file)!.toString('utf8'), shared),
  }));

  if (sheets.length === 0) throw new Error('Tệp Excel không có sheet nào');
  return { sheets, first: sheets[0] };
}

function readSheet(xml: string, shared: string[]): string[][] {
  const rows: string[][] = [];

  for (const rowMatch of xml.matchAll(ROW_TAG)) {
    const rowNumber = Number(rowMatch[1]);
    const cells: string[] = [];

    for (const cellMatch of rowMatch[2].matchAll(CELL_TAG)) {
      const attributes = cellMatch[1];
      const inner = cellMatch[2] ?? '';
      const reference = /\br\s*=\s*"([A-Z]+\d+)"/.exec(attributes)?.[1];
      const type = /\bt\s*=\s*"([^"]+)"/.exec(attributes)?.[1] ?? 'n';

      let value = '';
      if (type === 's') {
        const index = VALUE_TAG.exec(inner)?.[1];
        value = index === undefined ? '' : (shared[Number(index)] ?? '');
      } else if (type === 'inlineStr') {
        value = joinText(inner);
      } else {
        value = decodeXml(VALUE_TAG.exec(inner)?.[1] ?? '');
      }

      const target = reference ? columnIndex(reference) : cells.length;
      while (cells.length < target) cells.push('');
      cells[target] = value;
    }

    while (rows.length < rowNumber - 1) rows.push([]);
    rows[rowNumber - 1] = cells;
  }

  return rows;
}

/** Bỏ các dòng trống ở cuối bảng để đếm số dòng dữ liệu cho chính xác. */
export function trimTrailingEmptyRows(rows: string[][]): string[][] {
  const copy = [...rows];
  while (copy.length > 0 && copy[copy.length - 1].every((cell) => cell === '')) copy.pop();
  return copy;
}
