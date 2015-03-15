using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageMetadata
{
    public enum IfdTagType
    {
        Byte = 1,
        AscII = 2,
        Short = 3,
        Long = 4,
        Rational = 5,
        Undefined = 7,
        SignedLong = 9,
        SignedRational = 10,
        PrivateIFD = 0,
    }
}
