namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>The one sequence shared by enemy, pickup and projectile spawn requests, including failed attempts.</summary>
    internal sealed class SurvivorsSpawnSequence
    {
        public long Current { get; private set; }
        public long Next() => ++Current;
        public void Reset() => Current = 0;
    }
}
