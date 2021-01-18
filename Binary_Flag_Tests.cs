using System;
using Xunit;
using Xunit.Abstractions;
using IIG.BinaryFlag;
using System.Reflection;

namespace lab3
{
    public class BinaryFlag_Test
    {
        private readonly ITestOutputHelper output;

        public BinaryFlag_Test(ITestOutputHelper outputHelper)
        {
                this.output = outputHelper;
        }

        private FieldInfo getPrivateAttribute(object _object, string _attributeName)
        {
            return _object.GetType().GetField(_attributeName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private uint getFlagAfterReset(uint _flag, ulong _position)
        {
            return _flag &= uint.MaxValue ^ (uint)(1L << (int)_position);
        }

        private uint getFlagAfterSet(uint _flag, ulong _position)
        {
            return _flag |= (uint)(1L << (int)_position);
        }

        private uint getFlagValue(MultipleBinaryFlag _multipleBinaryFlag)
        {
            var concreteFlag_FieldInfo = getPrivateAttribute(_multipleBinaryFlag, "_concreteFlag");
            var concreteFlag = concreteFlag_FieldInfo?.GetValue(_multipleBinaryFlag);
            var flag_FieldInfo = getPrivateAttribute(concreteFlag, "_flag");

            return (uint)flag_FieldInfo?.GetValue(concreteFlag);
        }

        private uint[] getFlagValue_UIntArray(MultipleBinaryFlag _multipleBinaryFlag)
        {
            var concreteFlag_FieldInfo = getPrivateAttribute(_multipleBinaryFlag, "_concreteFlag");
            var concreteFlag = concreteFlag_FieldInfo?.GetValue(_multipleBinaryFlag);
            var flag_FieldInfo = getPrivateAttribute(concreteFlag, "_flag");

            return (uint[])flag_FieldInfo?.GetValue(concreteFlag);
        }

        private int getFlagArrayLength(ulong _length)
        {
            return (int)(_length / 32) + (_length % 32 == 0 ? 0 : 1);
        }

        //****************************************************************************************************************/
        //      Branch Coverage
        //****************************************************************************************************************/
        private string positionExeptionErrorMessage = "Position must be lesser than length";

        [Theory]
        [InlineData(1L,             "Length of Flag must be bigger than one")]
        [InlineData(0x3fffffe21L,   "Length of Flag must be lesser than '17179868704'")]
        public void Test_LengthExeptions(ulong _length, string _errMessage)
        {
            try
            {
                MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(_length);
            } catch (ArgumentOutOfRangeException e)
            {
                output.WriteLine(e.Message);
                Assert.True(e.Message?.Contains(_errMessage));
            }
        }

        [Theory]
        [InlineData(32,             "UIntConcreteBinaryFlag")]
        [InlineData(33,             "ULongConcreteBinaryFlag")]
        [InlineData(64,             "ULongConcreteBinaryFlag")]
        [InlineData(65,             "UIntArrayConcreteBinaryFlag")]
        [InlineData(0x3fffffe20L,   "UIntArrayConcreteBinaryFlag")]
        public void Test_LengthBinaryFlagType(ulong _length, string _type)
        {
            var multipleBinaryFlag  = new MultipleBinaryFlag(_length);

            var concreteFlag        = getPrivateAttribute(multipleBinaryFlag, "_concreteFlag");

            Assert.True(concreteFlag?.GetValue(multipleBinaryFlag).GetType().ToString().Contains(_type));
        }

        [Fact]
        public void Test_SetFlag_PositionExeptions()
        {
            try
            {
                MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(10);

                multipleBinaryFlag.SetFlag(100);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.True(e.Message?.Contains(positionExeptionErrorMessage));
            }
        }

        [Fact]
        public void Test_ResetFlag_PositionExeptions()
        {
            try
            {
                MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(10);

                multipleBinaryFlag.ResetFlag(100);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.True(e.Message?.Contains(positionExeptionErrorMessage));
            }
        }
        //****************************************************************************************************************/
        //      UIntConcreteBinaryFlag Tests
        //****************************************************************************************************************/

        private const ulong   length_UIntConcreteBinaryFlag   = 10;
        private const uint    flagValue_UIntConcreteFlag      = 4294966272;

        [Fact]
        public void Test_UIntConcreteBinaryFlag_GetFlag_True()
        {
            MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(length_UIntConcreteBinaryFlag, true);

            Assert.True(multipleBinaryFlag.GetFlag());
        }

        [Fact]
        public void Test_UIntConcreteBinaryFlag_GetFlag_False()
        {
            MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(length_UIntConcreteBinaryFlag, false);

            Assert.False(multipleBinaryFlag.GetFlag());
        }

        [Theory]
        [InlineData(true,           uint.MaxValue)]
        [InlineData(false,          flagValue_UIntConcreteFlag)]
        public void Test_UIntConcreteBinaryFlag_FlagValue(bool _flag, uint _flagValue)
        {
            MultipleBinaryFlag multipleBinaryFlag   = new MultipleBinaryFlag(length_UIntConcreteBinaryFlag, _flag);

            uint flagValue = getFlagValue(multipleBinaryFlag);

            Assert.True(flagValue == _flagValue);
        }

        [Fact]
        public void Test_DataFlow_UIntConcreteBinaryFlag_ResetFlag()
        {
            MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(length_UIntConcreteBinaryFlag, true);

            for (ulong pos = 0; pos < length_UIntConcreteBinaryFlag; pos++)
            {
                uint flagValueBeforeReset           = getFlagValue(multipleBinaryFlag);

                multipleBinaryFlag.ResetFlag(pos);

                uint actualFlagValueAfterReset      = getFlagValue(multipleBinaryFlag);
                uint expectedFlagValueAfterReset    = getFlagAfterReset(flagValueBeforeReset, pos);

                Assert.True(actualFlagValueAfterReset == expectedFlagValueAfterReset);
            }
        }

        [Fact]
        public void Test_DataFlow_UintConcreteBinaryFlag_SetFlag_PositiveCase()
        {
            MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(length_UIntConcreteBinaryFlag, true);

            for (ulong pos = 0; pos < length_UIntConcreteBinaryFlag; pos++)
            {
                uint flagValueBeforeSet = getFlagValue(multipleBinaryFlag);

                multipleBinaryFlag.SetFlag(pos);

                uint flagValueAfterSet = getFlagValue(multipleBinaryFlag);

                Assert.True(flagValueAfterSet == flagValueBeforeSet);
            }
        }

        [Fact]
        public void Test_DataFlow_UintConcreteBinaryFlag_SetFlag_NegativeCase()
        {
            MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(length_UIntConcreteBinaryFlag, false);

            for (ulong pos = 0; pos < length_UIntConcreteBinaryFlag; pos++)
            {
                uint flagValueBeforeSet         = getFlagValue(multipleBinaryFlag);

                multipleBinaryFlag.SetFlag(pos);

                uint actualFlagValueAfterSet    = getFlagValue(multipleBinaryFlag);
                uint expectedFlagValueAfterSet  = getFlagAfterSet(flagValueBeforeSet, pos);

                Assert.True(actualFlagValueAfterSet == expectedFlagValueAfterSet);
            }
        }

        //****************************************************************************************************************/
        //      UIntArrayConcreteBinaryFlag Tests
        //****************************************************************************************************************/

        private const ulong length_UIntArrayConcreteBinaryFlag = 65;

        [Theory]
        [InlineData(length_UIntArrayConcreteBinaryFlag)]
        [InlineData(9999)]
        [InlineData(99999999)]
        [InlineData(17179868704)]
        public void Test_DataFlow_UIntArrayConcreteBinaryFlag_FlagValueArrayLength(ulong _length)
        {
            MultipleBinaryFlag multipleBinaryFlag = new MultipleBinaryFlag(_length, true);

            uint[] flagValueArray   = getFlagValue_UIntArray(multipleBinaryFlag);

            int expectedSize        = getFlagArrayLength(_length);

            Assert.True(flagValueArray.Length == expectedSize);
        }
    }
}