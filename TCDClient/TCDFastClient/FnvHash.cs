using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Doms.TCClient
{
    class FNV1a : HashAlgorithm
    {
        private const uint Prime = 16777619;
        private const uint Offset = 2166136261;

        private uint _hashValue;

        public FNV1a()
        {
            this.HashSizeValue = 32;
            this.Initialize();
        }

        public override void Initialize()
        {
            _hashValue = Offset;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int end = ibStart + cbSize;

            for (int i = ibStart; i < end; i++)
            {
                _hashValue = (_hashValue ^ array[i]) * FNV1a.Prime;
            }
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_hashValue);
        }
    }

}
