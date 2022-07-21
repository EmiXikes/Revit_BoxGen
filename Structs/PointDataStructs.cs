using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicWallBoxGen
{
    public class PointDataStructs
    {
        public enum ConduitDirection
        {
            UP,
            DOWN
        }

        public enum FixtureEnd
        {
            SOCKET,
            CONDUIT
        }

        public enum ConnectionEnd
        {
            BOX,
            CONDUIT
        }

        public enum SeperateConduitLine
        {
            NO,
            YES
        }
    }
}
