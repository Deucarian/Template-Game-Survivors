namespace Deucarian.TemplateGameSurvivors
{
    internal readonly struct SurvivorsPlayerHudValues
    {
        public SurvivorsPlayerHudValues(
            string milestoneLabel = null,
            string buildSlotLabel = null,
            string activeWeaponLabel = null,
            float currentPickupAttractRange = 0,
            float currentPickupAttractionSpeed = 0,
            string pickupPulseLabel = null,
            string evolutionLabel = null)
        {
            MilestoneLabel = milestoneLabel;
            BuildSlotLabel = buildSlotLabel;
            ActiveWeaponLabel = activeWeaponLabel;
            CurrentPickupAttractRange = currentPickupAttractRange;
            CurrentPickupAttractionSpeed = currentPickupAttractionSpeed;
            PickupPulseLabel = pickupPulseLabel;
            EvolutionLabel = evolutionLabel;
        }
        public string MilestoneLabel { get; }
        public string BuildSlotLabel { get; }
        public string ActiveWeaponLabel { get; }
        public float CurrentPickupAttractRange { get; }
        public float CurrentPickupAttractionSpeed { get; }
        public string PickupPulseLabel { get; }
        public string EvolutionLabel { get; }
    }
}
