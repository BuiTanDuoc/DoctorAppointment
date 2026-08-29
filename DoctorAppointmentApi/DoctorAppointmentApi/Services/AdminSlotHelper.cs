using DoctorAppointmentApi.Entities;

namespace DoctorAppointmentApi.Services;

/// <summary>Keeps a doctor's slots_booked map in sync when appointments are booked or cancelled.</summary>
public static class AdminSlotHelper
{
    public static bool IsBooked(Doctor doctor, string slotDate, string slotTime)
    {
        var slots = doctor.GetSlotsBooked();
        return slots.TryGetValue(slotDate, out var times) && times.Contains(slotTime);
    }

    public static void Book(Doctor doctor, string slotDate, string slotTime)
    {
        var slots = doctor.GetSlotsBooked();
        if (!slots.TryGetValue(slotDate, out var times))
        {
            times = new List<string>();
            slots[slotDate] = times;
        }
        times.Add(slotTime);
        doctor.SetSlotsBooked(slots);
    }

    public static void Release(Appointment appointment)
    {
        if (appointment.Doctor is null) return;

        var slots = appointment.Doctor.GetSlotsBooked();
        if (slots.TryGetValue(appointment.SlotDate, out var times))
        {
            times.Remove(appointment.SlotTime);
            if (times.Count == 0)
            {
                slots.Remove(appointment.SlotDate);
            }
        }
        appointment.Doctor.SetSlotsBooked(slots);
    }
}
