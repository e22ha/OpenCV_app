using OpenCV2_photoapp;

namespace Tests;

public class UnitTest1
{
    [Theory]
    [InlineData(127, 127)]
    [InlineData(300, 255)]
    [InlineData(50, 50)]
    [InlineData(0, 0)]
    [InlineData(255, 255)]
    [InlineData(-20, 0)]
    public void TestBetween0255(double input, byte expected)
    {
        var result = Filter.between0255(input);
        Assert.Equal(expected, result);
    }

}