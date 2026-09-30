# Kiểm thử T63 — bỏ hẳn cách xưng hô khỏi giao diện

Ngày chạy: 30/09/2026 · Nhánh `fix/T63-bo-xung-ho` · Tách từ đầu nhánh PR #64 `test/T61-nghiem-thu-dot-30-09` (`e94f153`)

Cho tới khi #60 – #64 vào `main`, diff của PR còn kèm thay đổi của ba việc đó. Bình thường.

## Việc đã làm

Chủ dự án chốt ngày 30/09: người dùng là người trẻ, giao diện **bỏ luôn đại từ xưng hô**,
không thay bằng "bạn", "anh chị" hay "quý vị", và không thêm "vui lòng", "xin mời" cho lễ phép.
Câu giữ nguyên ý, chỉ ngắn lại.

- **15 câu hiển thị** trong 9 tệp `FE/src`: 14 câu còn chữ "bác" và câu lỗi máy chủ còn "vui lòng".
- **10 chú thích trong mã** đổi "bác" / "bác cán bộ" thành "người dùng", để sau này không ai
  dựa vào giả định cũ mà viết lại câu có xưng hô. Chín chú thích nêu trong mô tả việc, cộng
  `pages/dashboard/EligibleTableCard.tsx:57` — chú thích vào từ #60, QC ghi nhận ở HUYH-69.
- **2 chỗ trong bộ kiểm thử cũ**: khẳng định `'Bác thử bớt chữ'` ở `e2-import-co-loi.spec.ts:149`
  và chú thích ở dòng 179. Thêm một chú thích ở `ui-tests/specs/t58-quyet-dinh-giao-dien.spec.ts:116`
  mà lệnh `git grep` của định nghĩa hoàn thành bắt được.
- **Ca chặn mới** `FE/ui-tests/specs/t63-khong-xung-ho.spec.ts`.

Không chạm `CHANGELOG.md`, không chạm `BE/`. Không sửa `docs/huong-dan-su-dung.md`: HUYH-71
(PR #63) đã rà và kết luận hướng dẫn không trích nguyên văn câu giao diện nào đổi ở đây.

### Hai câu trong `api/messages.ts`

| Hằng | Câu mới |
|---|---|
| `SESSION_EXPIRED_MESSAGE` | Phiên làm việc đã hết hạn. Đăng nhập lại để dùng tiếp. |
| `SERVER_MESSAGE` | Hệ thống gặp sự cố. Thử lại sau. |

`docs/api-contract.md` dòng 111 trích nguyên văn câu `SERVER_MESSAGE` cũ. Tệp đó do Technical
Writer sở hữu nên việc này **không** sửa; CEO giao lại khi gác cổng. Cụm mở đầu "Hệ thống gặp
sự cố" giữ nguyên vì `e2-import-co-loi.spec.ts:285` bám vào nó.

## Ca chặn mới `t63-khong-xung-ho`

Quét tĩnh mọi tệp `.ts` và `.tsx` dưới `FE/src`, theo đúng cách ca `E-905` quét mã tìm
`waitForTimeout`. Chặn sáu cụm: "bác", "bạn", "anh chị", "quý vị", "vui lòng", "xin mời" —
kể cả trong chú thích. Không cần trình duyệt.

Ranh giới từ dựng bằng lớp chữ cái tiếng Việt chứ không dùng `\b`: `\b` của JavaScript chỉ
biết chữ cái ASCII nên "bác" sẽ khớp cả bên trong một từ dài hơn.

Chính tệp ca nằm ngoài vùng quét vì nó chỉ quét `FE/src` — như E-905 không quét tệp của mình.

**Đã kiểm ca chặn thật sự chặn**: thả một tệp `src/__tmp-xung-ho.ts` chứa chú thích
`// bác thử xem` rồi chạy lại, ca đỏ đúng chỗ:

```
    +   "__tmp-xung-ho.ts:1 — \"bác\"",
  1 failed
```

Tệp thử đã xóa ngay sau đó.

## Định nghĩa hoàn thành — nguyên văn đầu ra

### Hai lệnh quét

```
$ git grep -n -iE "\b(bác|bạn|anh chị|quý vị)\b" -- FE/src FE/e2e FE/ui-tests ':!FE/ui-tests/specs/t63-khong-xung-ho.spec.ts'
[mã thoát: 1 — không có dòng nào khớp]

$ git grep -n -iE "vui lòng|xin mời" -- FE/src
[mã thoát: 1 — không có dòng nào khớp]
```

### Bốn lệnh bắt buộc

```
$ npm run build

> huyhieudang-fe@1.0.0 build
> tsc -b && vite build

dist/assets/modal-Cu8Uyg4o.js     105.08 kB │ gzip:  33.20 kB
dist/assets/tokens-BRiahIRy.js    124.55 kB │ gzip:  42.46 kB
dist/assets/jsx-runtime-Dt1Evkee.js  127.29 kB │ gzip:  43.37 kB
dist/assets/table-fWnS3zRW.js     329.43 kB │ gzip: 101.01 kB
dist/assets/index-BRPcG8Z-.js     422.10 kB │ gzip: 138.11 kB
✓ built in 5.67s

[mã thoát: 0]

$ npm run lint

> huyhieudang-fe@1.0.0 lint
> eslint .

[mã thoát: 0]

$ npm run typecheck:e2e

> huyhieudang-fe@1.0.0 typecheck:e2e
> tsc -p tsconfig.e2e.json --noEmit

[mã thoát: 0]

$ npm run format:check

> huyhieudang-fe@1.0.0 format:check
> prettier --check .

Checking formatting...
All matched files use Prettier code style!

[mã thoát: 0]
```

### Bộ ca giao diện — cổng riêng 4181

```
$ T50_PORT=4181 npm run test:ui

  ok 39 [chromium] › ui-tests\specs\t63-khong-xung-ho.spec.ts:44:1 › T63 · Mã giao diện không còn chữ xưng hô nào (417ms)

  99 passed (31.2s)

[mã thoát: 0]
```

98 ca cũ giữ nguyên kết quả, cộng ca mới là 99.

### Luồng E2E-2 trên stack docker thật

```
$ docker compose -f e2e/docker-compose.e2e.yml up -d --build
$ npx playwright test -c e2e/playwright.e2e.config.ts specs/e2-import-co-loi.spec.ts

───── Môi trường kiểm thử end-to-end (T28) ─────
  Giao diện      : http://localhost:4174
  API            : http://localhost:4174/api
  Mốc đang ép    : 2026-09-19
  Máy chủ báo    : 2026-09-19
  Đồng hồ        : ĐÃ ĐÓNG BĂNG đúng mốc (T-FIX-4 hoạt động)
────────────────────────────────────────────────

Running 1 test using 1 worker

  ok 1 [chromium] › e2e\specs\e2-import-co-loi.spec.ts:30:1 › E2E-2 · Import có lỗi: xem trước đúng, chỉ dòng hợp lệ được nạp (20.5s)

  1 passed (22.1s)

[mã thoát: 0]
```

Lần chạy đầu tiên ngay sau `up -d --build` đỏ ở bước `E2E-2 · E2-04` vì container `be` còn đang
khởi động; hai lần chạy sau khi stack đã sẵn sàng đều xanh. Không có thay đổi mã nào giữa ba
lần chạy.

## Ảnh chụp

Ba màn chụp ở khung 1440x900 bằng một ca tạm dùng fixture của `ui-tests`, đính kèm comment
nghiệm thu. Ca tạm đã xóa, không nằm trong PR.

| Ảnh | Câu đã đổi |
|---|---|
| Hộp xác nhận xóa ở màn Đảng viên | "…không lấy lại được. **Kiểm tra kỹ trước khi xóa.**" |
| Trạng thái rỗng do lọc ở màn Đảng viên | "**Thử** xóa bớt chữ trong ô tìm, hoặc chọn lại Giới tính và Mốc kế tiếp: Tất cả." |
| Import bước 2 khi cả file lỗi | "Cả file đều vướng lỗi. **Sửa trong file gốc rồi nạp lại.**" |

## Ba tệp dùng chung với việc khác

Ngoại lệ có kiểm soát với quy tắc mỗi tệp một việc, theo đúng mô tả việc:

| Tệp | Việc song song | Việc này chỉ sửa |
|---|---|---|
| `pages/periods/PeriodsPage.tsx` | HUYH-73 (T64) | một câu "Thử xóa bớt chữ trong ô tìm." |
| `pages/periods/detail/EligibilityTab.tsx` | HUYH-76 (T66) | hai câu "Thử …" trong trạng thái rỗng |
| `pages/uncovered/UncoveredPage.tsx` | HUYH-76 (T66) | một câu "Thử xóa bớt chữ trong ô tìm, hoặc chọn lại Mốc: Tất cả." |

Các chỗ sửa cách nhau hàng chục dòng nên git tự gộp được; PR nào vào trước cũng không sao.
QC kiểm lại bản gộp ở HUYH-77.
