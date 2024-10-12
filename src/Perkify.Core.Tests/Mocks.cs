namespace Perkify.Core.Tests
{
    public interface IMockExpiry : INowUtc, IExpiry<IMockExpiry> { }

    public interface IMockBalance : IBalance<IMockBalance> { }
}