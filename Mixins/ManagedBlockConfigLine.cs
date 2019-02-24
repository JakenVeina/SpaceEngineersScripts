using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public partial struct ManagedBlockConfigLine
        {
            public ManagedBlockConfigLine(IReadOnlyList<string> linePieces)
            {
                _linePieces = linePieces;
            }

            public string BlockTag
                => _linePieces[0];

            public string Option
                => _linePieces[1];

            public int ParamCount
                => _linePieces.Count - 2;

            public string GetParam(int index)
                => _linePieces[index + 2];

            private readonly IReadOnlyList<string> _linePieces;
        }
    }
}
