using System.Collections.Generic;

namespace HardcoreSystems.Configuration
{
    public sealed class ValidationResult
    {
        private readonly List<string> errors = new List<string>();

        public bool IsValid
        {
            get { return errors.Count == 0; }
        }

        public IList<string> Errors
        {
            get { return errors; }
        }

        public void Add(string error)
        {
            errors.Add(error);
        }

        public string[] ToLogFields()
        {
            return new[] { "errors", string.Join("; ", errors.ToArray()) };
        }
    }
}
