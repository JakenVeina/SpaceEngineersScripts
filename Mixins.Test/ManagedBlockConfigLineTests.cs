using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class ManagedBlockConfigLineTests
    {
        #region Test Cases

        public static readonly string[][] LinePiecesTestCases
            = new string[][]
            {
                new[] { "blockTag1", "option1" },
                new[] { "blockTag2", "option2", "param1" },
                new[] { "blockTag3", "option3", "param2", "param3" }
            };

        public static readonly object[][] IndexAndLinePiecesTestCases
            = LinePiecesTestCases
                .SelectMany(linePieces => Enumerable.Range(0, (linePieces.Length - 2)),
                    (linePieces, index) => new object[] { index, linePieces })
                .ToArray();

        #endregion Test Cases

        #region BlockTag Tests

        [TestCaseSource(nameof(LinePiecesTestCases))]
        public void BlockTag_Always_ReturnsPiece0(IReadOnlyList<string> linePieces)
        {
            var uut = new ManagedBlockConfigLine(linePieces);

            uut.BlockTag.ShouldBe(linePieces[0]);
        }

        #endregion BlockTag Tests

        #region Option Tests

        [TestCaseSource(nameof(LinePiecesTestCases))]
        public void Option_Always_ReturnsPiece1(IReadOnlyList<string> linePieces)
        {
            var uut = new ManagedBlockConfigLine(linePieces);

            uut.Option.ShouldBe(linePieces[1]);
        }

        #endregion Option Tests

        #region ParamCount Tests

        [TestCaseSource(nameof(LinePiecesTestCases))]
        public void ParamCount_Always_ReturnsPieceCountMinus2(IReadOnlyList<string> linePieces)
        {
            var uut = new ManagedBlockConfigLine(linePieces);

            uut.ParamCount.ShouldBe(linePieces.Count - 2);
        }

        #endregion ParamCount Tests

        #region GetParam() Tests

        [TestCaseSource(nameof(IndexAndLinePiecesTestCases))]
        public void GetParam_Always_ReturnsPieceAtIndexPlus2(int index, IReadOnlyList<string> linePieces)
        {
            var uut = new ManagedBlockConfigLine(linePieces);

            uut.GetParam(index).ShouldBe(linePieces[index + 2]);
        }

        #endregion GetParam() Tests
    }
}
