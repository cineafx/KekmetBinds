namespace KekmetBinds.LeverHandling
{
    public interface ILeverHandler
    {
        bool IsInVehicle { get; set; }
        void Handle();
    }
}