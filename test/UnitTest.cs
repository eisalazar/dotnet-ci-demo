using Xunit;

namespace ConsoleDemo.Test
{
  public class UnitTest
  {
    [Fact]
    public void Test_AddMethod()
    {
      BasicMaths bm = new BasicMaths();
      double res = bm.Add(10, 10);
      Assert.Equal(20, res);
    }
    [Fact]
    public void Test_SubstractMethod()
    {
      BasicMaths bm = new BasicMaths();
      double res = bm.Substract(10, 10);
      Assert.Equal(0, res);
    }
    [Fact]
    public void Test_DivideMethod()
    {
      BasicMaths bm = new BasicMaths();
      double res = bm.Divide(10, 5);
      Assert.Equal(2, res);
    }
    [Fact]
    public void Test_MultiplyMethod()
    {
      BasicMaths bm = new BasicMaths();
      double res = bm.Multiply(10, 10);
      Assert.Equal(100, res);
    }
  }
}
