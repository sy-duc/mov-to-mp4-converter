# MOV to MP4 Converter (.NET 9, C#)

Ứng dụng Windows GUI nhỏ gọn cho phép kéo thả nhiều file `.mov` và chuyển đổi sang `.mp4` sử dụng **FFmpeg**.

## 🚀 Tính năng

- Kéo & thả nhiều file `.mov`.
- Nút **Convert** để bắt đầu chuyển đổi.
- Thanh tiến độ hiển thị quá trình convert.
- Log hiển thị file nào đã chuyển đổi xong.
- Tự lưu file `.mp4` trong cùng thư mục với file gốc.

---

## 🔗 Download

- [Download for Windows](https://github.com/sy-duc/mov-to-mp4-converter/raw/refs/heads/main/ReleasePackage/Output/MovToMp4ConverterSetup.exe)

---

## 📦 Yêu cầu hệ thống

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) (hoặc Visual Studio nếu thích)
- [FFmpeg](https://ffmpeg.org/download.html)

---

## ⚙️ Cài đặt công cụ

### 1. Cài .NET 9 SDK

- Tải từ link ở trên và cài đặt.
- Kiểm tra bằng:
  ```powershell
  dotnet --version
  ```
  → hiển thị `9.x.x`.

### 2. Cài đặt FFmpeg

- Tải FFmpeg bản "release full" từ [gyan.dev](https://www.gyan.dev/ffmpeg/builds#release-builds).
- Giải nén, copy thư mục `bin/` vào một nơicố định (VD: `C:\ffmpeg\bin`).
- Add vào **PATH**:
  - Windows Search → “Edit systemenvironment variables” → EnvironmentVariables.
  - Ở phần System variables, tìm biến Path → chọn Edit.
  - Bấm New, thêm đường dẫn:`C:\ffmpeg\bin`
  - Nhấn OK để lưu.
- Kiểm tra bằng:
  ```powershell
  ffmpeg -version
  ```

### 3. Cài Visual Studio Code

- Cài VSCode và extension **C# Dev Kit**.
- Có thể dùng Terminal trong VSCode để chạy `dotnet`.

---

## 🛠️ Tạo Project GUI

### 1. Tạo project WinForms App

```powershell
dotnet new winforms -n MovToMp4Converter
cd MovToMp4Converter
```

### 2. Mở bằng VSCode

```powershell
code .
```

### 3. Code gọi ffmpeg

- Mở `Form1.cs [Design]` (VS Code không có designer mạnh như Visual Studio, nhưng bạn có thể code tay GUI).

- Bạn sẽ thêm các control sau:

  - Panel (khu vực kéo thả file)

  - Button (Convert)

  - ProgressBar (hiển thị tiến độ)

  - TextBox multiline (log kết quả, read-only)

- Các logic xử lý event được code trong `Form1.cs`.

---

## ▶️ Chạy ứng dụng

```powershell
dotnet run
```

---

## 📂 Build ra EXE

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

File EXE sẽ nằm trong:

```bash
bin/Release/net9.0/win-x64/publish/
```

---

## 📝 Ghi chú

### 1. Thư viện FFmpeg
- FFmpeg là một thư viện và bộ công cụ mã nguồn mở chuyên xử lý đa phương tiện (multimedia): video, audio, hình ảnh.

#### 📂 Cấu trúc gồm:
  - ffmpeg (command-line tool) → Dùng dòng lệnh để chuyển đổi, cắt ghép, nén video/audio.

  - ffplay → Player đơn giản để test.

  - ffprobe → Lấy thông tin chi tiết của file multimedia.

  - Libavcodec, libavformat, libavfilter, libavdevice… → các thư viện C/C++ cho lập trình viên.

#### 🎯 Các tính năng chính:

- Chuyển đổi định dạng: MOV → MP4, AVI → MKV, WAV → MP3, v.v.

- Nén video/audio: giảm dung lượng bằng thay đổi codec, bitrate, resolution.

- Cắt ghép: trích xuất 1 đoạn video, nối nhiều video/audio lại.

- Streaming: hỗ trợ RTMP, HLS, DASH để phát trực tuyến.

- Filter: thêm hiệu ứng, phụ đề, watermark, xoay/lật video, thay đổi tốc độhát.

- Trích xuất frame: lấy ảnh từ video.

- Metadata: đọc/ghi thông tin file (codec, bitrate, tags…).

#### ✅ Ưu điểm:

- Mạnh mẽ: gần như hỗ trợ tất cả codec và container.

- Đa nền tảng: Windows, Linux, macOS.

- Tích hợp dễ: có thể gọi qua command line hoặc nhúng trực tiếp thư viện.

- Hiệu suất cao: hỗ trợ GPU (CUDA, NVENC, VAAPI).

### 2. Tạo phần mềm Windows với Inno Setup
- Để người khác dùng tool của bạn như một phần mềm Windows bình thường (có installer, copy vào `C:\Program Files\YourApp`, tạo shortcut Desktop, có thể Uninstall trong Control Panel) → cần thêm bước đóng gói cài đặt.

#### 🔹 Các cách phổ biến để tạo file cài đặt:

- Sử dụng Visual Studio Installer Projects (MSI)

- Dùng WiX Toolset (tạo MSI/MSIX)

- Dùng Inno Setup (EXE installer)

- Dùng MSIX Packaging Tool (của Microsoft Store style)

#### ⚙️ Tạo file cài đặt (installer) bằng Inno Setup

1️⃣ Bước 1: Chuẩn bị các file cần thiết
- Build app WinForms bằng lệnh:
  ```powershell
  dotnet publish -c Release -r win-x64 --self-contained true
  ```

- Folder output sẽ nằm trong:
  ```
  bin\Release\net9.0-windows\win-x64\publish\
  ```

- Copy thêm FFmpeg (bản essentials build, file `ffmpeg.exe` trong thư mục `bin`) vào cùng folder `publish`.

- Tạo 1 folder riêng, ví dụ:
```
ReleasePackage\
   AppFiles\   (chứa tất cả file trong publish)
   setup.iss   (script Inno Setup)
```
2️⃣ Bước 2: Cài đặt Inno Setup

- Tải tại: https://jrsoftware.org/isinfo.php

- Cài vào máy, mở Inno Setup Compiler để viết và biên dịch script `.iss`.

3️⃣ Bước 3: Viết file `setup.iss`
- File `.iss` là một script (kiểu cấu hình text) định nghĩa toàn bộ thông tin về cài đặt app của bạn.

- Nó có nhiều section ([Setup], [Files], [Icons], v.v.). Mỗi section có nhiệm vụ riêng:

- Ví dụ:

  ```ini
  ; Thông tin chung của app
  [Setup]
  ; Tên app
  AppName=Mov to MP4 Converter
  ; Version hiển thị trong Control Panel
  AppVersion=1.0
  ; Tác giả / công ty
  AppPublisher=Duc Ho Sy
  ; Thư mục cài mặc định
  DefaultDirName={pf}\MovToMp4Converter
  ; Tên hiển thị trong Control Panel (Programs and Features)
  DefaultGroupName=Mov to MP4 Converter
  ; Thư mục output sau khi compile
  OutputDir=Output
  ; Tên file cài đặt sinh ra
  OutputBaseFilename=MovToMp4ConverterSetup
  ; Dùng thuật toán nén LZMA giúp file cài đặt nhỏ hơn nhưng cài đặt chậm hơn
  Compression=lzma
  ; Kích hoạt chế độ Solid Compression - các file trong installer được nén như một khối lớn thay vì nén từng file riêng lẻ
  SolidCompression=yes

  ; Những file của app cần copy (exe, dll, config, ffmpeg.exe, v.v.)
  [Files]
  ; Copy toàn bộ file trong AppFiles vào Program Files
  Source: "AppFiles\*"; DestDir: "{app}"; Flags: recursesubdirs

  ; Tạo shortcut ngoài Desktop và trong Start Menu
  [Icons]
  ; Shortcut trên Desktop
  Name: "{commondesktop}\MovToMp4 Converter"; Filename: "{app}\MovToMp4Converter. exe"; IconFilename: "{app}\favicon.ico"

  ; Shortcut trong Start Menu
  Name: "{group}\MovToMp4 Converter"; Filename: "{app}\MovToMp4Converter.exe"; IconFilename: "{app}\favicon.ico"

  ; Chạy app ngay sau khi cài đặt xong (tùy chọn)
  [Run]
  Filename: "{app}\MovToMp4Converter.exe"; Description: "Run MovToMp4 Converter";   Flags: nowait postinstall skipifsilent

  ; Ngôn ngữ installer (tùy chọn)
  [Languages]
  Name: "english"; MessagesFile: "compiler:Default.isl"
  Name: "vietnamese"; MessagesFile: "compiler:Languages\Vietnamese.isl"
  ```

  → Lưu file này thành setup.iss trong thư mục ReleasePackage.

- Mặc định Inno Setup chỉ có vài file ngôn ngữ phổ biến (English, German, French…). Nếu muốn dùng tiếng Việt bạn cần download file ngôn ngữ [Vietnamese.isl](https://jrsoftware.org/files/istrans/) và copy vào thư mục Languages của Inno Setup.

4️⃣ Bước 4: Build file cài đặt

- Mở Inno Setup Compiler → Open → chọn file `setup.iss`.

- Bấm Build (hoặc F9).

- Sau khi build thành công, bạn sẽ thấy file `MovToMp4ConverterSetup.exe` trong folder đã thiết định theo OutputDir (ví dụ `Output`).

5️⃣ Bước 5: Cài đặt thử trên máy khác

- Copy file `MovToMp4ConverterSetup.exe` sang máy khác.

- Cài đặt → App sẽ được cài vào `C:\Program Files\MovToMp4Converter`.

- Shortcut sẽ có trên Desktop và Start Menu.
