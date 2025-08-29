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