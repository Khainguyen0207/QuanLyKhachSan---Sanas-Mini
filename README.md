

# Project Setup 

Tài liệu này hướng dẫn quá trình cài đặt, restore NuGet packages, xử lý lỗi sai đường dẫn `packages`, và cấu hình file môi trường sau khi clone project.

---

## 1. Restore NuGet Packages

### Cách 1: Restore bằng Visual Studio (khuyến nghị)

1. Mở solution trong Visual Studio  
2. Chuột phải vào **Solution** tại *Solution Explorer*  
3. Chọn **Restore NuGet Packages**

Visual Studio sẽ tự động tải về các package còn thiếu.

### Cách 2: Restore bằng Package Manager Console

Mở:

**Tools → NuGet Package Manager → Package Manager Console**

Chạy:

```powershell
Update-Package -reinstall
````

Lệnh này sẽ đọc lại `packages.config` và `.csproj` để khôi phục toàn bộ dependency.

---

## 2. Sửa lỗi sai đường dẫn thư mục `packages`

Một số project tham chiếu NuGet packages qua relative path:

```
..\packages\<package-name>\build\...
```

Khi clone project, nếu cấu trúc thư mục không khớp như máy người tạo ban đầu, đường dẫn này sẽ không hợp lệ và gây lỗi:

```
This project references NuGet package(s) that are missing on this computer.
The missing file is ..\packages\<package>\build\...\*.props
```

### Nguyên nhân phổ biến

* Thư mục `packages` được tạo sai cấp
* Máy khác commit cấu trúc thư mục không đồng nhất
* NuGet restore về sai vị trí
* `.csproj` tham chiếu đường dẫn tuyệt đối đến phiên bản package cụ thể

---

## 3. Cách khắc phục lỗi sai path `packages`

### Bước 1: Mở terminal tại thư mục chứa `.csproj`

Trong File Explorer:

1. Mở thư mục chứa project
2. Click vào thanh đường dẫn
3. Gõ `cmd` → Enter

Terminal sẽ mở đúng tại thư mục dự án.

### Bước 2: Di chuyển thư mục `packages` lên đúng cấp

Nếu `.csproj` tham chiếu tới:

```
..\packages\
```

nhưng `packages` lại đang nằm **cùng cấp** với `.csproj`, hãy di chuyển:

```cmd
move packages ..\
```

### Cấu trúc đúng mong muốn

```
SolutionFolder/
    packages/
    YourProject/
        YourProject.csproj
```


