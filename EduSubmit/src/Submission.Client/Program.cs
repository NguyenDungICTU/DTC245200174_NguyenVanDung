using System.Net.Http.Json;

// 1. LẤY ĐỊA CHỈ API: Lấy từ tham số dòng lệnh (args), nếu không có thì mặc định là localhost:5000
var apiBaseUrl = args.Length > 0 ? args[0].TrimEnd('/') : "http://localhost:5000";


// 2. TẠO HTTP CLIENT: Dùng để gửi request đến API
using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

Console.WriteLine($"Đang kết nối tới API tại: {apiBaseUrl}...");

try
{
    // 3. GỌI API GET: Gửi request đến endpoint /api/assignments và đọc dữ liệu JSON trả về
    // Dữ liệu JSON sẽ được tự động map vào List<AsignmentDto>
    var assignments = await http.GetFromJsonAsync<List<AssignmentDto>>("/api/assignments");

    // Kiểm tra xem API có trả về dữ liệu không
    if (assignments == null || assignments.Count == 0)
    {
        Console.WriteLine("Không có bài tập nào.");
        return;
    }

    // 4. HIỂN THỊ DỨ LIỆU: duyệt danh sách và in ra màn hình console
    Console.WriteLine("\n-- DANH SÁCH BÀI TẬP --");
    foreach (var item in assignments)
    {
        //Chuyển đổi trang thái IsOpen cho dễ đọc
        string status = item.IsOpen ? "Đang mở" : "Đã đóng";

        Console.WriteLine($"[{item.Id} {item.Title} ({status})]");
        Console.WriteLine($"    Mô tả: {item.Description}");
        Console.WriteLine();
    }
}

catch (HttpRequestException ex)
{
    // Bắt lỗi nếu không kết nối được tới API
    Console.WriteLine($"Lỗi kết nối tới API: {ex.Message}");
    Console.WriteLine("Hãy chắc chắn rằng API đã được chạy (dotnet run --project src/Submission.Api)");
}
catch (Exception ex)
{
    // Bắt các lỗi khác
    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
}

//Định nghĩa DTO (Phải giống hệ bên API để map dữ liệu JSON chính xác)
public record AssignmentDto(int Id, string Title, string Description, bool IsOpen);