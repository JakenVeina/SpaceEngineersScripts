using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class ConfigLineTests
    {
        #region Test Cases

        public static readonly string[][][] LinePiecesTestCases
            = new string[][][]
            {
                new[] { new[] { "option1" } },
                new[] { new[] { "option2", "param1" } },
                new[] { new[] { "option3", "param2", "param3" } }
            };

        public static readonly object[][] IndexAndLinePiecesTestCases
            = LinePiecesTestCases
                .Select(x => x[0])
                .SelectMany(linePieces => Enumerable.Range(0, (linePieces.Length - 1)),
                    (linePieces, index) => new object[] { index, linePieces })
                .ToArray();

        #endregion Test Cases

        #region Option Tests

        [TestCaseSource(nameof(LinePiecesTestCases))]
        public void Option_Always_ReturnsPiece0(IReadOnlyList<string> linePieces)
        {
            var uut = new ConfigLine(linePieces);

            uut.Option.ShouldBe(linePieces[0]);
        }

        #endregion Option Tests

        #region ParamCount Tests

        [TestCaseSource(nameof(LinePiecesTestCases))]
        public void ParamCount_Always_ReturnsPieceCountMinus1(IReadOnlyList<string> linePieces)
        {
            var uut = new ConfigLine(linePieces);

            uut.ParamCount.ShouldBe(linePieces.Count - 1);
        }

        #endregion ParamCount Tests

        #region GetParam() Tests

        [TestCaseSource(nameof(IndexAndLinePiecesTestCases))]
        public void GetParam_Always_ReturnsPieceAtIndexPlus(int index, IReadOnlyList<string> linePieces)
        {
            var uut = new ConfigLine(linePieces);

            uut.GetParam(index).ShouldBe(linePieces[index + 1]);
        }

        #endregion GetParam() Tests
    }
}
