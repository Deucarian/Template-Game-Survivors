namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Ordered live surge values added after the tuning base and upgrade modifier.</summary>
    internal readonly struct SurvivorsPlayerSurgeValues
    {
        private readonly float _streak, _roamingCache, _arenaShrine, _waystoneFocus, _waystoneChain, _hordeRushClear;
        private readonly float _weaponLoadout, _passiveLoadout, _bossRelic, _gemRush, _evolutionChain, _endless;
        internal SurvivorsPlayerSurgeValues(float streak = 0f, float roamingCache = 0f, float arenaShrine = 0f,
            float waystoneFocus = 0f, float waystoneChain = 0f, float hordeRushClear = 0f, float weaponLoadout = 0f,
            float passiveLoadout = 0f, float bossRelic = 0f, float gemRush = 0f, float evolutionChain = 0f, float endless = 0f)
        {
            _streak = streak; _roamingCache = roamingCache; _arenaShrine = arenaShrine; _waystoneFocus = waystoneFocus;
            _waystoneChain = waystoneChain; _hordeRushClear = hordeRushClear; _weaponLoadout = weaponLoadout;
            _passiveLoadout = passiveLoadout; _bossRelic = bossRelic; _gemRush = gemRush;
            _evolutionChain = evolutionChain; _endless = endless;
        }
        internal float AddTo(float value) => value + _streak + _roamingCache + _arenaShrine + _waystoneFocus +
            _waystoneChain + _hordeRushClear + _weaponLoadout + _passiveLoadout + _bossRelic + _gemRush + _evolutionChain + _endless;
    }
}
