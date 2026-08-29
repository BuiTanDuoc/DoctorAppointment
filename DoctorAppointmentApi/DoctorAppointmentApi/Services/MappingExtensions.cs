using DoctorAppointmentApi.Dtos.Appointment;
using DoctorAppointmentApi.Dtos.Common;
using DoctorAppointmentApi.Dtos.Doctor;
using DoctorAppointmentApi.Dtos.User;
using DoctorAppointmentApi.Entities;

namespace DoctorAppointmentApi.Services;

public static class MappingExtensions
{
    public static DoctorDto ToDto(this Doctor doctor) => new()
    {
        Id = doctor.Id.ToString(),
        Name = doctor.Name,
        Email = doctor.Email,
        Image = doctor.Image,
        Speciality = doctor.Speciality,
        Degree = doctor.Degree,
        Experience = doctor.Experience,
        About = doctor.About,
        Available = doctor.Available,
        Fees = doctor.Fees,
        Address = new AddressDto { Line1 = doctor.AddressLine1, Line2 = doctor.AddressLine2 },
        SlotsBooked = doctor.GetSlotsBooked()
    };

    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id.ToString(),
        Name = user.Name,
        Email = user.Email,
        Image = user.Image,
        Address = new AddressDto { Line1 = user.AddressLine1, Line2 = user.AddressLine2 },
        Gender = user.Gender,
        Dob = user.Dob,
        Phone = user.Phone
    };

    public static AppointmentDto ToDto(this Appointment appointment) => new()
    {
        Id = appointment.Id.ToString(),
        SlotDate = appointment.SlotDate,
        SlotTime = appointment.SlotTime,
        Amount = appointment.Amount,
        Cancelled = appointment.Cancelled,
        Payment = appointment.Payment,
        IsCompleted = appointment.IsCompleted,
        UserData = appointment.User!.ToDto(),
        DocData = appointment.Doctor!.ToDto()
    };
}
