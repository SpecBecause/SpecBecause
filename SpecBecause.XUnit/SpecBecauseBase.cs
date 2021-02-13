namespace SpecBecause.XUnit
{
    public class SpecBecauseBase
    {
        private IEngine Engine { get; }

        public SpecBecauseBase(IEngine? engine = null)
        {
            Engine = engine ?? new Engine();
        }
    }
}
