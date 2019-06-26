using Identity.STS.Identity.Configuration.Intefaces;

namespace Identity.STS.Identity.Configuration
{
    public class RegisterConfiguration : IRegisterConfiguration
    {
        public bool Enabled { get; set; } = true;
    }
}
