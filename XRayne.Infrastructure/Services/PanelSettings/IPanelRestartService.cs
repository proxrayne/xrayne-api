namespace XRayne.Infrastructure.Services.PanelSettings;

public interface IPanelRestartService
{
    // Возвращает true если шатдаун запланирован этим вызовом; false если уже был запланирован
    // ранее. Полагается на supervisor (systemd / docker restart policy) для повторного запуска.
    bool ScheduleRestart();
}
