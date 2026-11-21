using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
var builder = WebApplication.CreateBuilder(args);

// 1. ĐỌC CHUỖI KẾT NỐI: Lấy chuổi kết nối từ appsettings.json 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2.ĐĂNG KÍ DATABSE: Cấu hình cho Entity Framework Core dùng MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


var app = builder.Build();

// 3. KHỞI TẠO DATABSE VÀ SEED DATA (Chạy 1 lần khi app khởi động)
using (var scope = app.Services.CreateScope())
{
    //Lấy service DbContext ra để sử dụng
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //Tự động tạo Database và các bảng nếu chưa tồn tại (Dựa trên các class Entity)
    db.Database.EnsureCreated();

    //Nếu bảng Assignments đang trống (chưa có dữ liệu) thì thêm dữ liệu mẫu
    if (!db.Assignments.Any())
    {
        db.Assignments.AddRange(
            new Assignment { Title = "Bài tập 1: Cài đặt mội trường", Description = "Cài đặt .NET 10 và VS Code", IsOpen = true},
            new Assignment { Title = "Bài tập 2: REST API cơ bản", Description = "Viết API CRUD", IsOpen = true },
            new Assignment { Title = "Bài tập 3: SignalR", Description = "Làm quen với Real-time", IsOpen = false }
        );
        db.SaveChanges(); //Lưu thay đổi vào Database
    }
}

// 4. API ENDPOINT: Định nghĩa route GET  /api/assignments
app.MapGet("/", () => Results.Ok(new { 
    message = "Chào mừng đến với EduSubmit API",
    version = "1.0",
    endpoints = new[] { "/api/assignments" } 
}));

app.MapGet("/api/assignments", async (AppDbContext db) =>
{
    //Truy vấn Database lấy danh sách bài tập, chiếu (select) sang dang DTC để trả về
    var assignments = await db.Assignments
        .Select(a => new AssignmentDto(a.Id, a.Title, a.Description, a.IsOpen))
        .ToListAsync(); // Thực thi truy vấn bất đồng bộ
    
    return Results.Ok(assignments); //Trả về HTTP 200 OK kèm danh sách JSON
});

app.Run();

// ====================================
// Các định nghĩa class
// ====================================

//Class Entity: Đại diện cho bảng 'Assignments' trong Database


//Class Entity: Đại diện cho bảng 'Asignments' trong Database
public class Assignment
{
    public int Id { get; set; } //Khóa chính, tự động tăng
    public string Title {get; set;} = string.Empty;
    public string Description {get;set;} = string.Empty;
    public bool IsOpen { get; set;}
}

//Class DbContext: Cầu nối giữa code C# và Databse
public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }

    //Đại diện cho bảng Asignments trong cơ sở dữ liệu
    public DbSet<Assignment> Assignments => Set<Assignment>();

    public record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
}

//Record DTO: Định dang dữ liệu trả về cho Client (ẩn các trường không cần thiết)
public record AssignmentDto(int Id, string Title, string Description, bool Isopen);