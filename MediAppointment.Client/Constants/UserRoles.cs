namespace MediAppointment.Client.Constants
{
    public static class UserRoles
    {
        public const string Patient = "Patient";
        public const string Doctor = "Doctor";
        public const string Admin = "Admin";
        public const string Manager = "Manager";

        public static readonly string[] AllRoles = { Patient, Doctor, Admin, Manager };
    }
}
