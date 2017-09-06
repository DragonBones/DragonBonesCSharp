using System;
using System.Collections.Generic;
using System.Text;

namespace DragonBones
{
    public class Armature : BaseObject, IAnimateble
    {
        public WorldClock clock { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private static int _OnSortSlots(Slot a, Slot b)
        {
            return a._zOrder > b._zOrder ? 1 : -1;
        }

        public void AdvanceTime(float passedTime)
        {
            throw new NotImplementedException();
        }

        protected override void _OnClear()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
