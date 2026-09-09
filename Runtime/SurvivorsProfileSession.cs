using System;
using Deucarian.Persistence;

namespace Deucarian.TemplateGameSurvivors
{
    /// <summary>Owns profile creation/loading and releases only persistence created by this session.</summary>
    internal sealed class SurvivorsProfileSession : IDisposable
    {
        private readonly Func<IPersistenceService> _createPersistence;
        private IPersistenceService _borrowedPersistence;
        private SaveSlotId _slotId = new SaveSlotId("survivors-template");
        private bool _ownsPersistence;
        private bool _loaded;

        public SurvivorsProfileSession(Func<IPersistenceService> createPersistence)
        {
            _createPersistence = createPersistence ?? throw new ArgumentNullException(nameof(createPersistence));
        }

        public SurvivorsMetaProgressionService Current { get; private set; }

        public void ConfigureBorrowedPersistence(IPersistenceService persistence, SaveSlotId slotId)
        {
            if (persistence == null) throw new ArgumentNullException(nameof(persistence));
            Release();
            _borrowedPersistence = persistence;
            _slotId = slotId;
        }

        public SurvivorsMetaProgressionService EnsureLoaded(SurvivorsMetaProgressionDefinition definition)
        {
            if (Current == null)
            {
                IPersistenceService persistence = _borrowedPersistence ?? _createPersistence();
                bool owns = _borrowedPersistence == null;
                try
                {
                    Current = new SurvivorsMetaProgressionService(persistence, _slotId, definition);
                    _ownsPersistence = owns;
                }
                catch
                {
                    if (owns) persistence?.Dispose();
                    throw;
                }
            }

            if (!_loaded)
            {
                Current.Load();
                _loaded = true;
            }

            return Current;
        }

        public void Release()
        {
            SurvivorsMetaProgressionService previous = Current;
            bool owned = _ownsPersistence;
            Current = null;
            _ownsPersistence = false;
            _loaded = false;
            if (owned) previous?.Dispose();
        }

        public void Dispose() => Release();
    }
}
