namespace Tracing.UnitTests.Correlation;

public sealed class CorrelationIdPolicyTests
{
    private readonly CorrelationIdPolicy policy = new();

    [Fact]
    public void Resolve_Should_Keep_Valid_Id_Of_Exactly_Max_Length()
    {
        var id = new string('a', 128);

        _ = policy.Resolve([id]).Should().Be(id);
    }

    [Fact]
    public void Resolve_Should_Reject_Id_One_Over_Max_Length()
    {
        _ = policy.Resolve([new string('a', 129)]).Should().BeNull();
    }

    [Theory]
    [InlineData("abc\r\ndef")]
    [InlineData("abc\ndef")]
    [InlineData("abc\u0000def")]
    [InlineData("abc def")]
    [InlineData("abc,def")]
    [InlineData("é")]
    public void Resolve_Should_Reject_Bad_Characters(string value)
    {
        _ = policy.Resolve([value]).Should().BeNull();
    }

    [Theory]
    [InlineData("Abc-1_2.3:4")]
    [InlineData("0f8fad5bd9cb469fa16570867728950e")]
    public void Resolve_Should_Accept_Allowed_Characters(string value)
    {
        _ = policy.Resolve([value]).Should().Be(value);
    }

    [Fact]
    public void Resolve_Should_Trim_Surrounding_Whitespace()
    {
        _ = policy.Resolve(["  abc  "]).Should().Be("abc");
    }

    [Fact]
    public void Resolve_Should_Return_Null_For_Empty_Input()
    {
        _ = policy.Resolve([]).Should().BeNull();
        _ = policy.Resolve([null, "", "  "]).Should().BeNull();
    }

    [Fact]
    public void Resolve_Should_Reject_Several_Distinct_Values()
    {
        _ = policy.Resolve(["a", "b"]).Should().BeNull();
    }

    [Fact]
    public void Resolve_Should_Accept_Repeated_Identical_Values()
    {
        _ = policy.Resolve(["a", " a ", null]).Should().Be("a");
    }

    [Fact]
    public void Resolve_Should_Honor_Custom_MaxLength_And_Charset()
    {
        var custom = new CorrelationIdPolicy { MaxLength = 3, IsAllowedChar = char.IsDigit };

        _ = custom.Resolve(["123"]).Should().Be("123");
        _ = custom.Resolve(["1234"]).Should().BeNull();
        _ = custom.Resolve(["12a"]).Should().BeNull();
    }

    [Fact]
    public void Generate_Should_Produce_Valid_Distinct_Ids()
    {
        var a = CorrelationIdPolicy.Generate();
        var b = CorrelationIdPolicy.Generate();

        _ = a.Should().HaveLength(32).And.NotBe(b);
        _ = policy.IsValid(a).Should().BeTrue();
    }

    [Fact]
    public void Setters_Should_Reject_Invalid_Values()
    {
        _ = FluentActions.Invoking(() => policy.MaxLength = 0).Should().Throw<ArgumentOutOfRangeException>();
        _ = FluentActions.Invoking(() => policy.IsAllowedChar = null!).Should().Throw<ArgumentNullException>();
        _ = FluentActions.Invoking(() => policy.Resolve(null!)).Should().Throw<ArgumentNullException>();
    }
}
