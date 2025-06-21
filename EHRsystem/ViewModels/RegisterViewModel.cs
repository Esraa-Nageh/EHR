namespace EHRsystem.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; } = string.Empty;   // لو بيستخدمه في الفورم
        public string Name { get; set; } = string.Empty;       // الحل للمشكلة الحالية
        public string FullName { get; set; } = string.Empty;   // مستخدم في الـ View
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
