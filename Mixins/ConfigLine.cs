using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public partial struct ConfigLine
        {
            public ConfigLine(IReadOnlyList<string> linePieces)
            {
                _linePieces = linePieces;
            }

            public string Option
                => _linePieces[0];

            public int ParamCount
                => _linePieces.Count - 1;

            public string GetParam(int index)
                => _linePieces[index + 1];

            private readonly IReadOnlyList<string> _linePieces;
        }
    }
}
